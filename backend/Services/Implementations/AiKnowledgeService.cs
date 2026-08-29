using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Services.Implementations;

/// <summary>
/// 党建知识库问答服务（真千问 RAG 实现）。
/// 流程：本地知识库关键词召回 → 组装参考资料 → 交给千问生成回答 → 返回引用来源。
/// 千问不可用时自动回退到内置关键词答案，保证系统始终可用。
/// </summary>
public class AiKnowledgeService : IAiKnowledgeService
{
    private readonly IQwenService _qwen;
    private readonly IKnowledgeSearchService _knowledge;

    // 内置兜底知识库（千问不可用时使用）
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

    private const string SystemPrompt =
        "你是一名专业的党建知识解答助手，服务对象是党校/党支部的党员。请严格遵循以下要求：\n" +
        "1. 优先依据「参考资料」中提供的权威党建资料作答，答案要准确、简明、条理清晰。\n" +
        "2. 如果参考资料足以回答，请在回答末尾另起一行注明「（资料来源：<文件名>）」。\n" +
        "3. 如果参考资料不足以回答，则结合你的党建常识客观作答，并如实说明该内容不在本地知识库中。\n" +
        "4. 禁止编造事实；涉及领导人、历史事件等内容一律以官方权威表述为准。\n" +
        "5. 用简体中文回答。";

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

        if (string.IsNullOrEmpty(question))
        {
            return new AiKnowledgeQueryResponse
            {
                Answer = "请先输入您想咨询的党建知识问题。",
                SourceReferences = new List<string>(),
                Confidence = 0,
                SessionId = sessionId
            };
        }

        // 1. 本地知识库召回
        var results = _knowledge.Search(question, limit: 5);
        var context = _knowledge.BuildContext(results);
        var references = results
            .Select(r => r.File)
            .Distinct()
            .ToList();

        // 2. 千问生成回答
        if (_qwen.IsConfigured)
        {
            try
            {
                var userPrompt = context.Length > 0
                    ? $"【参考资料】\n{context}\n\n【用户问题】\n{question}"
                    : $"（本次未检索到本地知识库资料，请结合党建常识作答）\n\n【用户问题】\n{question}";

                var answer = await _qwen.ChatAsync(SystemPrompt, userPrompt, temperature: 0.4);
                if (!string.IsNullOrWhiteSpace(answer))
                {
                    return new AiKnowledgeQueryResponse
                    {
                        Answer = answer,
                        SourceReferences = references,
                        Confidence = references.Count > 0 ? 0.92 : 0.65,
                        SessionId = sessionId
                    };
                }
            }
            catch
            {
                // 千问异常 → 走兜底
            }
        }

        // 3. 兜底：内置关键词答案
        return FallbackQuery(question, sessionId);
    }

    private static AiKnowledgeQueryResponse FallbackQuery(string question, string sessionId)
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
            SessionId = sessionId
        };
    }
}
