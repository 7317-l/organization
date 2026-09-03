using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Services.Implementations;

/// <summary>
/// AI素材生成服务：支持题目、文章、宣讲稿、知识卡片四种 contentType。
/// </summary>
public class AiContentGenerationService : IAiContentGenerationService
{
    private readonly IQwenService _qwen;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AiContentGenerationService> _logger;
    private readonly IKnowledgeSearchService _knowledge;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public AiContentGenerationService(
        IQwenService qwen,
        IHttpClientFactory httpClientFactory,
        ILogger<AiContentGenerationService> logger,
        IKnowledgeSearchService knowledge)
    {
        _qwen = qwen;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _knowledge = knowledge;
    }

    public async Task<AiGenerateContentResponse> GenerateAsync(AiGenerateContentRequest request)
    {
        var contentType = string.IsNullOrEmpty(request.ContentType) ? "questions" : request.ContentType.ToLowerInvariant();

        if (contentType == "questions")
        {
            return await GenerateQuestionsAsync(request);
        }

        return await GenerateContentAsync(request, contentType);
    }

    private async Task<AiGenerateContentResponse> GenerateQuestionsAsync(AiGenerateContentRequest request)
    {
        var source = await ResolveSourceTextAsync(request);

        if (_qwen.IsConfigured && !string.IsNullOrWhiteSpace(source))
        {
            try
            {
                var result = await GenerateWithQwenAsync(source, request);
                if (result != null && result.Questions.Count > 0)
                {
                    result.ContentType = "questions";
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "千问素材生成失败，回退到内置示例。");
            }
        }

        var fallback = GenerateFallback(request);
        fallback.ContentType = "questions";
        return fallback;
    }

    private async Task<AiGenerateContentResponse> GenerateContentAsync(AiGenerateContentRequest request, string contentType)
    {
        var topic = request.Topic ?? "";
        if (string.IsNullOrEmpty(topic) && !string.IsNullOrEmpty(request.SourceText))
        {
            topic = request.SourceText.Length > 50 ? request.SourceText[..50] : request.SourceText;
        }

        var audience = request.Audience ?? "党员";
        var tone = request.Tone ?? "正式";
        var maxWords = request.MaxWords ?? (contentType == "speech" ? 2500 : 1500);
        var durationMinutes = request.DurationMinutes ?? 15;

        // 知识库检索补充素材
        var kbContext = "";
        if (!string.IsNullOrEmpty(topic))
        {
            var kbResults = _knowledge.Search(topic, limit: 3);
            if (kbResults.Count > 0)
            {
                kbContext = "\n\n【参考资料】\n" + string.Join("\n", kbResults.Select(r => r.Content));
            }
        }

        AiGeneratedContentDto content;
        if (_qwen.IsConfigured)
        {
            try
            {
                content = await GenerateContentWithQwenAsync(contentType, topic, audience, tone, maxWords, durationMinutes, request.Keywords, kbContext);
            }
            catch
            {
                content = GenerateContentFallback(contentType, topic, audience, maxWords, durationMinutes);
            }
        }
        else
        {
            content = GenerateContentFallback(contentType, topic, audience, maxWords, durationMinutes);
        }

        var response = new AiGenerateContentResponse
        {
            ContentType = contentType,
            Summary = $"已生成关于「{topic}」的{(contentType == "speech" ? "宣讲稿" : contentType == "article" ? "文章" : "知识卡片")}",
            Content = content
        };

        if (contentType == "quizcard" || request.GenerateFlashCards)
        {
            response.FlashCards = GenerateFlashCardsFallback(topic, request.Keywords);
        }

        return response;
    }

    private async Task<AiGeneratedContentDto> GenerateContentWithQwenAsync(
        string contentType, string topic, string audience, string tone, int maxWords,
        int durationMinutes, List<string>? keywords, string kbContext)
    {
        var typeLabel = contentType switch
        {
            "speech" => "宣讲稿",
            "article" => "文章",
            "quizcard" => "知识卡片",
            _ => "文章"
        };

        var system = $"你是党建内容创作专家。请生成一篇{typeLabel}，只输出 JSON 对象。\n" +
                     "JSON结构：{\"title\":\"标题\",\"text\":\"正文（分段用\\n\\n）\",\"outline\":[\"小标题1\",\"小标题2\"],\"keyPoints\":[\"要点1\",\"要点2\"],\"sections\":[{\"heading\":\"段落标题\",\"minutes\":5,\"content\":\"段落内容\"}]}";

        var user = $"主题：{topic}\n受众：{audience}\n风格：{tone}\n字数上限：{maxWords}\n" +
                   (contentType == "speech" ? $"目标时长：{durationMinutes}分钟（按180-220字/分钟折算）\n" : "") +
                   (keywords != null && keywords.Count > 0 ? $"关键词：{string.Join("、", keywords)}\n" : "") +
                   kbContext +
                   $"\n\n请生成{typeLabel}，要求结构清晰、内容准确、符合党建语境。";

        var raw = await _qwen.ChatAsync(system, user, temperature: 0.6, jsonMode: true, maxTokens: 8192);
        var content = ParseContentRaw(raw);
        if (content != null)
        {
            content.TargetAudience = audience;
            content.WordCount = content.Text.Length;
            if (contentType == "speech")
                content.EstimatedMinutes = durationMinutes;
            return content;
        }

        return GenerateContentFallback(contentType, topic, audience, maxWords, durationMinutes);
    }

    private static AiGeneratedContentDto GenerateContentFallback(
        string contentType, string topic, string audience, int maxWords, int durationMinutes)
    {
        var typeLabel = contentType == "speech" ? "宣讲稿" : contentType == "article" ? "文章" : "知识卡片";
        var text = $"同志们：\n\n今天，我们围绕「{topic}」这一主题进行学习交流。\n\n" +
                   $"一、深刻认识{topic}的重要意义\n{topic}是党的建设的重要组成部分，对于推动事业发展具有重要意义。\n\n" +
                   $"二、准确把握{topic}的核心要求\n我们要坚持以习近平新时代中国特色社会主义思想为指导，全面贯彻落实相关要求。\n\n" +
                   $"三、扎实推进{topic}落地见效\n要结合实际工作，把学习成果转化为推动发展的实际行动。\n\n" +
                   $"让我们共同努力，不断推进{topic}取得新成效！";

        if (text.Length > maxWords) text = text[..maxWords];

        return new AiGeneratedContentDto
        {
            Title = $"关于{topic}的{typeLabel}",
            Text = text,
            Outline = new List<string> { "重要意义", "核心要求", "落地见效" },
            KeyPoints = new List<string> { $"深刻认识{topic}的重要性", $"准确把握{topic}的要求", $"扎实推进{topic}落地" },
            TargetAudience = audience,
            WordCount = text.Length,
            EstimatedMinutes = contentType == "speech" ? durationMinutes : null,
            Sections = new List<AiContentSectionDto>
            {
                new() { Heading = "开场", Minutes = contentType == "speech" ? 2 : 0, Content = $"同志们：今天我们围绕「{topic}」进行学习。" },
                new() { Heading = "主体", Minutes = contentType == "speech" ? durationMinutes - 4 : 0, Content = text },
                new() { Heading = "总结", Minutes = 2, Content = $"让我们共同推进{topic}取得新成效！" }
            }
        };
    }

    private static List<AiFlashCardDto> GenerateFlashCardsFallback(string topic, List<string>? keywords)
    {
        return new List<AiFlashCardDto>
        {
            new() { Front = $"{topic}的核心要义是什么？", Back = $"坚持以习近平新时代中国特色社会主义思想为指导，全面推进{topic}。", Tag = topic },
            new() { Front = $"如何推进{topic}落地？", Back = "结合实际工作，制定具体措施，强化督促检查，确保取得实效。", Tag = topic }
        };
    }

    private async Task<string> ResolveSourceTextAsync(AiGenerateContentRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.SourceText))
        {
            var text = request.SourceText.Trim();
            if (text.Length > 6000)
                return text[..3000] + "\n...（内容过长已截断）...\n" + text[^3000..];
            return text;
        }

        if (!string.IsNullOrWhiteSpace(request.PdfUrl))
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(20);
                var content = await client.GetStringAsync(request.PdfUrl);
                if (!string.IsNullOrWhiteSpace(content) && content.Length > 50)
                {
                    var text = System.Text.RegularExpressions.Regex.Replace(content, "<[^>]+>", " ");
                    if (text.Length > 50) return text;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "读取 PDF/URL 内容失败：{Url}", request.PdfUrl);
            }
        }

        return string.Empty;
    }

    private async Task<AiGenerateContentResponse?> GenerateWithQwenAsync(
        string source, AiGenerateContentRequest request)
    {
        var system =
            "你是党建学习平台的出题专家。请基于给定的源材料，严格按照要求生成题目，只输出一个 JSON 对象。\n" +
            "JSON结构：{\"questions\":[{\"type\":\"single|multi|truefalse\",\"stem\":\"题干\",\"options\":[\"A\",\"B\",\"C\",\"D\"],\"correct\":\"正确项\",\"score\":分值}],\"flashcards\":[{\"front\":\"正面\",\"back\":\"背面\"}],\"summary\":\"总结\"}\n" +
            "规则：单选题correct填字母；多选题correct填下标数组如[0,2]；判断题选项固定为[\"正确\",\"错误\"]，correct填A或B。";

        var user = new System.Text.StringBuilder();
        user.AppendLine($"【源材料】\n{(source.Length > 6000 ? source[..6000] : source)}");
        user.AppendLine();
        user.AppendLine($"【生成要求】单选 {request.SingleChoiceCount} 道；多选 {request.MultiChoiceCount} 道；判断 {request.TrueFalseCount} 道；学习卡片：{(request.GenerateFlashCards ? "是" : "否")}。");

        var raw = await _qwen.ChatAsync(system, user.ToString(), temperature: 0.5, jsonMode: true, maxTokens: 8192);
        var parsed = ParseRaw(raw);
        if (parsed == null) return null;

        var questions = parsed.Questions
            .Where(q => !string.IsNullOrWhiteSpace(q.Stem))
            .Select(q => ToDto(q))
            .Where(q => q != null)
            .Take(request.SingleChoiceCount + request.MultiChoiceCount + request.TrueFalseCount)
            .Cast<AiGeneratedQuestionDto>()
            .ToList();

        if (questions.Count == 0) return null;

        return new AiGenerateContentResponse
        {
            Questions = questions,
            FlashCards = request.GenerateFlashCards
                ? parsed.FlashCards.Where(f => !string.IsNullOrWhiteSpace(f.Front)).ToList()
                : new List<AiFlashCardDto>(),
            Summary = $"已基于源材料生成{questions.Count}道题目" +
                      (request.GenerateFlashCards ? $"和{parsed.FlashCards.Count}张学习卡片" : "") +
                      "，建议人工审核后再发布使用。"
        };
    }

    private static GeneratedQuestionRaw? ParseRaw(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var start = raw.IndexOf('{');
        var end = raw.LastIndexOf('}');
        if (start < 0 || end <= start) return null;
        try
        {
            return JsonSerializer.Deserialize<GeneratedQuestionRaw>(
                raw.Substring(start, end - start + 1), JsonOpts);
        }
        catch { return null; }
    }

    private static AiGeneratedContentDto? ParseContentRaw(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var start = raw.IndexOf('{');
        var end = raw.LastIndexOf('}');
        if (start < 0 || end <= start) return null;
        try
        {
            return JsonSerializer.Deserialize<AiGeneratedContentDto>(
                raw.Substring(start, end - start + 1), JsonOpts);
        }
        catch { return null; }
    }

    private static AiGeneratedQuestionDto? ToDto(GeneratedQuestionRawRaw q)
    {
        var type = q.Type?.Trim().ToLowerInvariant();
        QuestionType questionType;
        string typeName;
        var correct = ResolveCorrect(q.Correct);

        switch (type)
        {
            case "multi":
            case "多选":
                questionType = QuestionType.MultiChoice;
                typeName = "多选题";
                if (!correct.StartsWith("["))
                {
                    var opts = q.Options ?? new List<string>();
                    var indices = correct.Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(c => c.Trim().ToUpperInvariant())
                        .Select(c => c.Length == 1 && c[0] >= 'A' && c[0] <= 'Z' ? c[0] - 'A' : -1)
                        .Where(i => i >= 0 && i < opts.Count)
                        .ToList();
                    correct = "[" + string.Join(",", indices) + "]";
                }
                break;
            case "truefalse":
            case "判断":
                questionType = QuestionType.TrueFalse;
                typeName = "判断题";
                if (q.Options == null || q.Options.Count == 0)
                    q.Options = new List<string> { "正确", "错误" };
                break;
            default:
                questionType = QuestionType.SingleChoice;
                typeName = "单选题";
                break;
        }

        var options = q.Options ?? new List<string>();
        if (options.Count < 2) return null;
        if (options.Count < 4 && questionType == QuestionType.SingleChoice) return null;

        return new AiGeneratedQuestionDto
        {
            QuestionType = questionType,
            QuestionTypeName = typeName,
            Stem = q.Stem ?? string.Empty,
            Options = options,
            CorrectAnswer = string.IsNullOrWhiteSpace(correct) ? "A" : correct,
            Score = q.Score > 0 ? q.Score : (questionType == QuestionType.MultiChoice ? 15 : questionType == QuestionType.TrueFalse ? 5 : 10)
        };
    }

    private static AiGenerateContentResponse GenerateFallback(AiGenerateContentRequest request)
    {
        var questions = new List<AiGeneratedQuestionDto>();

        for (int i = 0; i < request.SingleChoiceCount; i++)
        {
            questions.Add(new AiGeneratedQuestionDto
            {
                QuestionType = QuestionType.SingleChoice,
                QuestionTypeName = "单选题",
                Stem = $"（AI生成示例）中国共产党成立于哪一年？第{i + 1}题",
                Options = new List<string> { "1919年", "1921年", "1927年", "1949年" },
                CorrectAnswer = "B",
                Score = 10
            });
        }

        for (int i = 0; i < request.MultiChoiceCount; i++)
        {
            questions.Add(new AiGeneratedQuestionDto
            {
                QuestionType = QuestionType.MultiChoice,
                QuestionTypeName = "多选题",
                Stem = $"（AI生成示例）以下哪些属于\"四个意识\"？第{i + 1}题",
                Options = new List<string> { "政治意识", "大局意识", "核心意识", "看齐意识" },
                CorrectAnswer = "[0,1,2,3]",
                Score = 15
            });
        }

        for (int i = 0; i < request.TrueFalseCount; i++)
        {
            questions.Add(new AiGeneratedQuestionDto
            {
                QuestionType = QuestionType.TrueFalse,
                QuestionTypeName = "判断题",
                Stem = $"（AI生成示例）中国共产党的根本宗旨是全心全意为人民服务。第{i + 1}题",
                Options = new List<string> { "正确", "错误" },
                CorrectAnswer = "A",
                Score = 5
            });
        }

        var flashCards = new List<AiFlashCardDto>();
        if (request.GenerateFlashCards)
        {
            flashCards.Add(new AiFlashCardDto { Front = "中国共产党的初心和使命是什么？", Back = "为中国人民谋幸福，为中华民族谋复兴。" });
            flashCards.Add(new AiFlashCardDto { Front = "什么是\"三会一课\"？", Back = "支部党员大会、支部委员会、党小组会和党课。" });
        }

        return new AiGenerateContentResponse
        {
            Questions = questions,
            FlashCards = flashCards,
            Summary = $"已基于源材料生成{questions.Count}道题目和{flashCards.Count}张学习卡片。建议人工审核后再发布使用。"
        };
    }

    private class GeneratedQuestionRaw
    {
        public List<GeneratedQuestionRawRaw> Questions { get; set; } = new();
        public List<AiFlashCardDto> FlashCards { get; set; } = new();
        public string? Summary { get; set; }
    }

    private class GeneratedQuestionRawRaw
    {
        public string? Type { get; set; }
        public string? Stem { get; set; }
        public List<string>? Options { get; set; }
        public object? Correct { get; set; }
        public int Score { get; set; }
    }

    private static string ResolveCorrect(object? value)
    {
        if (value == null) return string.Empty;
        if (value is string s) return s.Trim();
        if (value is JsonElement je)
        {
            if (je.ValueKind == JsonValueKind.Array)
            {
                var indices = je.EnumerateArray()
                    .Select(x => x.ValueKind == JsonValueKind.Number ? x.GetInt32() : -1)
                    .Where(i => i >= 0)
                    .ToList();
                return "[" + string.Join(",", indices) + "]";
            }
            if (je.ValueKind == JsonValueKind.String) return je.GetString() ?? string.Empty;
            if (je.ValueKind == JsonValueKind.Number) return je.GetInt32().ToString();
        }
        return value.ToString() ?? string.Empty;
    }
}
