using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Services.Implementations;

/// <summary>
/// 党建知识库问答服务（两级检索+重排 RAG 实现）。
/// 一级：关键词召回20条；二级：BM25/千问重排取TopK；逐条置信度。
/// </summary>
public class AiKnowledgeService : IAiKnowledgeService
{
    private readonly IQwenService _qwen;
    private readonly IKnowledgeSearchService _knowledge;

    private static readonly Dictionary<string, string> FallbackKnowledgeBase = new()
    {
        ["入党誓词"] = "我志愿加入中国共产党，拥护党的纲领，遵守党的章程，履行党员义务，执行党的决定，严守党的纪律，保守党的秘密，对党忠诚，积极工作，为共产主义奋斗终身，随时准备为党和人民牺牲一切，永不叛党。",
        ["党的宗旨"] = "中国共产党的根本宗旨是全心全意为人民服务。",
        ["党的性质"] = "中国共产党是中国工人阶级的先锋队，同时是中国人民和中华民族的先锋队，是中国特色社会主义事业的领导核心。",
        ["四个意识"] = "政治意识、大局意识、核心意识、看齐意识。",
        ["四个自信"] = "道路自信、理论自信、制度自信、文化自信。",
        ["两个维护"] = "坚决维护习近平总书记党中央的核心、全党的核心地位，坚决维护党中央权威和集中统一领导。",
        ["三会一课"] = "支部党员大会、支部委员会、党小组会和党课。",
        ["两学一做"] = "学党章党规、学系列讲话，做合格党员。",
        ["不忘初心"] = "不忘初心，方得始终。中国共产党人的初心和使命，就是为中国人民谋幸福，为中华民族谋复兴。"
    };

    /// <summary>党员端（学习者视角）知识问答提示词</summary>
    private const string MemberSystemPrompt =
        "你是一名专业的党建知识解答助手，正在为一位党员学习者解答问题。请严格遵循以下要求：\n" +
        "1. 优先依据「参考资料」中提供的权威党建资料作答，答案要准确、通俗易懂。\n" +
        "2. 回答必须采用「总分总」结构：先一句话直接给出结论，再分点逐条解释（每条用『第一/第二/第三』或『1. 2. 3.』引导），最后用一两句话总结要点。让党员读完能立刻抓住核心并愿意继续读下去。\n" +
        "3. 严禁使用任何 Markdown 符号：不要出现 *、#、-、>、` 等标记字符，不要使用加粗、斜体、列表符号，用纯文字自然表述。\n" +
        "4. 如果参考资料足以回答，请在回答末尾注明「（资料来源：<文件名>）」。\n" +
        "5. 如果参考资料不足以回答，则结合党建常识客观作答，并如实说明该内容不在本地知识库中。\n" +
        "6. 禁止编造事实；涉及领导人、历史事件等内容一律以官方权威表述为准。\n" +
        "7. 用简体中文回答，条理清晰、逻辑缜密、语言亲切自然。";

    /// <summary>管理端（党务管理者视角）知识问答提示词</summary>
    private const string AdminSystemPrompt =
        "你是一名专业的党建知识解答助手，正在为党务管理者（系统管理员、支部书记）解答问题。请严格遵循以下要求：\n" +
        "1. 优先依据「参考资料」中提供的权威党建资料作答，答案要准确、规范，并体现党务管理视角（可补充政策背景、组织建设或工作落实要点）。\n" +
        "2. 回答必须采用「总分总」结构：先一句话直接给出结论，再分点逐条展开（每条用『第一/第二/第三』或『1. 2. 3.』引导），最后给出管理层面的一句话总结或工作提示。\n" +
        "3. 严禁使用任何 Markdown 符号：不要出现 *、#、-、>、` 等标记字符，不要使用加粗、斜体、列表符号，用纯文字自然表述。\n" +
        "4. 如果参考资料足以回答，请在回答末尾注明「（资料来源：<文件名>）」。\n" +
        "5. 如果参考资料不足以回答，则结合党建常识客观作答，并如实说明该内容不在本地知识库中。\n" +
        "6. 禁止编造事实；涉及领导人、历史事件等内容一律以官方权威表述为准。\n" +
        "7. 用简体中文回答，条理清晰、逻辑缜密、表述严谨。";

    /// <summary>错题分析（单题深度解析）提示词</summary>
    private const string AnalysisSystemPrompt =
        "你是一名资深的党建知识辅导老师，正在为一位党员做单题错题分析。请严格遵循以下要求：\n" +
        "1. 采用「总分总」结构分析：先一句话点明该题考查的核心知识点和正确答案，再分步详细讲解为什么选这个答案、其他选项为什么错（或党员答案错在哪里），最后总结本题的答题思路与易错点提醒。\n" +
        "2. 分析要让党员真正弄懂这道题，而不是只给答案；讲解结合党建知识原理，通俗易懂。\n" +
        "3. 严禁使用任何 Markdown 符号：不要出现 *、#、-、>、` 等标记字符，不要使用加粗、斜体、列表符号，用纯文字自然表述；选项用『A选项/B选项』这样的文字表述。\n" +
        "4. 不要编造题目中没有的选项内容；选项内容以题目给出的为准。\n" +
        "5. 用简体中文回答，条理清晰、逻辑缜密，语言亲切。";

    /// <summary>寒暄/闲聊场景的助手提示词（不要求引用来源、不检索）</summary>
    private const string ChatSystemPrompt =
        "你是一名友善的党建学习平台智能助手。用户可能只是在问候或闲聊（例如「你好」「你是谁」「谢谢」），" +
        "请自然、友好、简短地回应，并顺带说明你可以帮助解答党建知识、推荐学习内容、生成学习报告等。" +
        "不要编造党建事实，不涉及敏感话题，用简体中文回答。" +
        "注意：每次回答都要变换表达方式，使用不同的开场白和句式，不要每次都说一样的话；" +
        "但内容保持简洁（50字以内），亲切自然即可。";

    public AiKnowledgeService(IQwenService qwen, IKnowledgeSearchService knowledge)
    {
        _qwen = qwen;
        _knowledge = knowledge;
    }

    public async Task<AiKnowledgeQueryResponse> QueryAsync(AiKnowledgeQueryRequest request)
    {
        var question = request.Question.Trim();
        var sessionId = string.IsNullOrEmpty(request.SessionId)
            ? Guid.NewGuid().ToString("N")
            : request.SessionId;
        var topK = Math.Clamp(request.TopK, 1, 10);

        if (string.IsNullOrEmpty(question))
        {
            return new AiKnowledgeQueryResponse
            {
                Answer = "请先输入您想咨询的党建知识问题。",
                SourceReferences = new List<string>(),
                Confidence = 0,
                SessionId = sessionId,
                Results = new List<RagResultItem>()
            };
        }

        // 一级召回：20条
        var candidates = _knowledge.Search(question, limit: 20);
        if (!string.IsNullOrEmpty(request.FilterFile))
        {
            candidates = candidates.Where(c => c.File.Contains(request.FilterFile, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // 二级重排
        var reranked = RerankCandidates(candidates, question, request.Rerank);
        var topResults = reranked.Take(topK).ToList();

        var context = _knowledge.BuildContext(candidates.Take(topK).ToList());
        var references = topResults.Select(r => $"{r.File}-{r.Id}").Distinct().ToList();

        // 千问生成回答
        if (_qwen.IsConfigured)
        {
            try
            {
                var userPrompt = context.Length > 0
                    ? $"【参考资料】\n{context}\n\n【用户问题】\n{question}"
                    : $"（本次未检索到本地知识库资料，请结合党建常识作答）\n\n【用户问题】\n{question}";

                var answer = await _qwen.ChatAsync(ResolveKnowledgePrompt(request.Role), userPrompt, temperature: 0.4);
                if (!string.IsNullOrWhiteSpace(answer))
                {
                    return new AiKnowledgeQueryResponse
                    {
                        Answer = answer,
                        SourceReferences = references,
                        Confidence = topResults.Count > 0 ? topResults[0].Confidence : 0.3,
                        SessionId = sessionId,
                        Results = topResults
                    };
                }
            }
            catch { }
        }

        // 兜底
        return FallbackQuery(question, sessionId, topResults);
    }

    /// <summary>流式问答预处理：识别问题类型（寒暄/知识），完成检索/重排/提示词，供 SSE 端点先发元数据再流式生成回答</summary>
    public Task<AiKnowledgeStreamContext> PrepareStreamAsync(AiKnowledgeQueryRequest request)
    {
        var question = request.Question.Trim();
        var sessionId = string.IsNullOrEmpty(request.SessionId)
            ? Guid.NewGuid().ToString("N")
            : request.SessionId;
        var topK = Math.Clamp(request.TopK, 1, 10);

        if (string.IsNullOrEmpty(question))
        {
            return Task.FromResult(new AiKnowledgeStreamContext
            {
                Meta = new AiKnowledgeStreamMeta
                {
                    SessionId = sessionId,
                    SourceReferences = new List<string>(),
                    Confidence = 0,
                    Results = new List<RagResultItem>()
                },
                FallbackAnswer = "请先输入您想咨询的党建知识问题。"
            });
        }

        // 寒暄/闲聊类问题：不检索知识库、不展示置信度与引用来源
        if (IsChatQuestion(question))
        {
            var chatMeta = new AiKnowledgeStreamMeta
            {
                SessionId = sessionId,
                SourceReferences = new List<string>(),
                Confidence = 0,
                Results = new List<RagResultItem>(),
                QuestionType = "chat"
            };
            if (!_qwen.IsConfigured)
            {
                return Task.FromResult(new AiKnowledgeStreamContext
                {
                    Meta = chatMeta,
                    FallbackAnswer = "您好！我是党建学习平台的 AI 助手，可以为您解答党史、党章、党的理论、政策精神等知识问题，也可以根据您的学习情况提供个性化建议。有什么可以帮您的吗？"
                });
            }
            return Task.FromResult(new AiKnowledgeStreamContext
            {
                Meta = chatMeta,
                SystemPrompt = ChatSystemPrompt,
                UserPrompt = question
            });
        }

        // 一级召回：20条
        var candidates = _knowledge.Search(question, limit: 20);
        if (!string.IsNullOrEmpty(request.FilterFile))
        {
            candidates = candidates.Where(c => c.File.Contains(request.FilterFile, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // 二级重排
        var reranked = RerankCandidates(candidates, question, request.Rerank);
        var topResults = reranked.Take(topK).ToList();

        var context = _knowledge.BuildContext(candidates.Take(topK).ToList());
        var references = topResults.Select(r => $"{r.File}-{r.Id}").Distinct().ToList();

        var meta = new AiKnowledgeStreamMeta
        {
            SessionId = sessionId,
            SourceReferences = references,
            Confidence = topResults.Count > 0 ? topResults[0].Confidence : 0.3,
            Results = topResults,
            QuestionType = "knowledge"
        };

        // 未配置 API Key 时直接走兜底
        if (!_qwen.IsConfigured)
        {
            var fallback = FallbackQuery(question, sessionId, topResults);
            return Task.FromResult(new AiKnowledgeStreamContext
            {
                Meta = meta,
                FallbackAnswer = fallback.Answer
            });
        }

        var userPrompt = context.Length > 0
            ? $"【参考资料】\n{context}\n\n【用户问题】\n{question}"
            : $"（本次未检索到本地知识库资料，请结合党建常识作答）\n\n【用户问题】\n{question}";

        return Task.FromResult(new AiKnowledgeStreamContext
        {
            Meta = meta,
            SystemPrompt = ResolveKnowledgePrompt(request.Role),
            UserPrompt = userPrompt
        });
    }

    /// <summary>按端侧角色选择知识问答提示词：member=党员端，admin=管理端，其他默认党员端</summary>
    private static string ResolveKnowledgePrompt(string? role)
    {
        return string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase) ? AdminSystemPrompt : MemberSystemPrompt;
    }

    /// <summary>错题分析预处理：构造单题深度解析的提示词，供 SSE 端点流式输出分析</summary>
    public Task<AiAnalyzeStreamContext> PrepareAnalyzeStreamAsync(AiQuestionAnalyzeRequest request)
    {
        var sessionId = string.IsNullOrEmpty(request.SessionId)
            ? Guid.NewGuid().ToString("N")
            : request.SessionId;

        var meta = new AiKnowledgeStreamMeta
        {
            SessionId = sessionId,
            SourceReferences = new List<string>(),
            Confidence = 0,
            Results = new List<RagResultItem>(),
            QuestionType = "analysis"
        };

        var question = request.Question?.Trim();
        if (string.IsNullOrEmpty(question))
        {
            return Task.FromResult(new AiAnalyzeStreamContext
            {
                Meta = meta,
                FallbackAnswer = "暂未检测到题目内容，请先进入练习/考试页面再使用错题分析。"
            });
        }

        if (!_qwen.IsConfigured)
        {
            return Task.FromResult(new AiAnalyzeStreamContext
            {
                Meta = meta,
                FallbackAnswer = "未配置千问 API 密钥，暂无法进行 AI 错题分析。请在项目根目录 .env 中配置 DASHSCOPE_API_KEY 后重试。"
            });
        }

        // 结构化题目信息：题干 + 选项 + 用户答案 + 正确答案 + 知识点
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("【题目】").AppendLine(question);
        if (request.Options != null && request.Options.Count > 0)
        {
            sb.AppendLine().AppendLine("【选项】");
            for (var i = 0; i < request.Options.Count; i++)
            {
                var letter = ((char)('A' + i)).ToString();
                var opt = request.Options[i];
                // 选项可能是 {text} 或 {label,text} 或纯文本
                var text = opt;
                try
                {
                    var obj = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(opt);
                    if (obj != null)
                    {
                        text = obj.TryGetValue("text", out var t) ? t?.ToString() ?? "" : obj.TryGetValue("content", out var c) ? c?.ToString() ?? "" : opt;
                    }
                }
                catch { /* 不是JSON，按纯文本处理 */ }
                sb.AppendLine($"{letter}选项：{text}");
            }
        }
        sb.AppendLine().AppendLine($"【党员的作答】{request.UserAnswer ?? "未作答"}");
        if (!string.IsNullOrEmpty(request.CorrectAnswer))
        {
            sb.AppendLine($"【正确答案】{request.CorrectAnswer}");
        }
        if (!string.IsNullOrEmpty(request.KnowledgePoint))
        {
            sb.AppendLine($"【涉及知识点】{request.KnowledgePoint}");
        }

        return Task.FromResult(new AiAnalyzeStreamContext
        {
            Meta = meta,
            SystemPrompt = AnalysisSystemPrompt,
            UserPrompt = sb.ToString()
        });
    }

    /// <summary>判断是否为寒暄/闲聊类问题（问候、身份、致谢、道别等短句）</summary>
    private static bool IsChatQuestion(string question)
    {
        var t = question.Trim().ToLowerInvariant().TrimEnd('？', '?', '！', '!', '。', '，', ',', '.', ' ', '～', '~');
        if (string.IsNullOrEmpty(t)) return false;

        // 纯寒暄词
        string[] greetings = {
            "你好", "您好", "哈喽", "嗨", "hello", "hi", "嗨喽", "在吗", "在不在",
            "你是谁", "你叫什么", "你是干什么", "你是啥", "介绍一下你", "介绍下你",
            "谢谢", "多谢", "感谢", "辛苦啦", "辛苦你了",
            "再见", "拜拜", "晚安", "早上好", "中午好", "晚上好",
            "嗯", "好的", "ok", "没问题", "可以", "知道啦"
        };
        if (greetings.Contains(t)) return true;

        // 短句（≤8字符）命中问候词即视为闲聊，避免"你好，什么是三会一课"被误判
        if (t.Length <= 8)
        {
            foreach (var g in greetings)
            {
                if (t.Contains(g)) return true;
            }
        }
        return false;
    }

    private static List<RagResultItem> RerankCandidates(IReadOnlyList<KnowledgeDocument> candidates, string question, bool rerank)
    {
        var results = new List<RagResultItem>();
        var keywords = ExtractKeywords(question);

        foreach (var c in candidates)
        {
            var score = CalculateBm25Score(question, c.Content, keywords);
            var rerankScore = rerank ? score : score;
            var confidence = Sigmoid(score * 2);
            var matched = keywords.Where(k => c.Content.Contains(k, StringComparison.OrdinalIgnoreCase)).ToList();

            results.Add(new RagResultItem
            {
                Id = c.Id,
                File = c.File,
                Snippet = c.Content.Length > 200 ? c.Content[..200] : c.Content,
                Score = Math.Round(score, 3),
                RerankScore = Math.Round(rerankScore, 3),
                Confidence = Math.Round(confidence, 3),
                MatchedKeywords = matched
            });
        }

        return results.OrderByDescending(r => r.RerankScore).ToList();
    }

    private static double CalculateBm25Score(string query, string content, List<string> keywords)
    {
        if (string.IsNullOrEmpty(content)) return 0;
        double score = 0;
        foreach (var kw in keywords)
        {
            var count = 0;
            var index = 0;
            while ((index = content.IndexOf(kw, index, StringComparison.OrdinalIgnoreCase)) != -1)
            {
                count++;
                index += kw.Length;
            }
            score += count * (1.0 / (1 + content.Length / 1000.0));
        }
        // 连续4字子串匹配加分
        for (int i = 0; i < query.Length - 3; i++)
        {
            var sub = query.Substring(i, 4);
            if (content.Contains(sub, StringComparison.OrdinalIgnoreCase))
                score += 0.5;
        }
        return score;
    }

    private static List<string> ExtractKeywords(string question)
    {
        var keywords = new List<string>();
        var stopWords = new[] { "的", "了", "是", "什么", "怎么", "如何", "请问", "吗", "呢", "啊" };
        var words = question.Split(new[] { ' ', '，', '。', '？', '?', '、' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var w in words)
        {
            if (!stopWords.Contains(w) && w.Length >= 2)
                keywords.Add(w);
        }
        if (keywords.Count == 0 && question.Length >= 2)
            keywords.Add(question);
        return keywords;
    }

    private static double Sigmoid(double x)
    {
        return 1.0 / (1.0 + Math.Exp(-x));
    }

    private static AiKnowledgeQueryResponse FallbackQuery(string question, string sessionId, List<RagResultItem> topResults)
    {
        string answer = "抱歉，暂未找到相关知识。建议您查阅党章或相关学习资料。";
        double confidence = 0.3;
        var references = new List<string>();

        foreach (var kv in FallbackKnowledgeBase)
        {
            if (question.Contains(kv.Key) || kv.Key.Contains(question))
            {
                answer = kv.Value;
                confidence = 0.9;
                references.Add($"《党建知识库》- {kv.Key}");
                break;
            }
        }

        return new AiKnowledgeQueryResponse
        {
            Answer = answer,
            SourceReferences = references,
            Confidence = confidence,
            SessionId = sessionId,
            Results = topResults
        };
    }
}
