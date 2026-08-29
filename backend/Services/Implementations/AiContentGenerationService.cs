using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Services.Implementations;

/// <summary>
/// AI素材生成服务（真千问实现）：
/// 基于源材料/文档内容，由千问生成指定数量的单选题、多选题、判断题与学习卡片（严格 JSON 输出）。
/// 千问不可用时自动回退到内置示例。
/// </summary>
public class AiContentGenerationService : IAiContentGenerationService
{
    private readonly IQwenService _qwen;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AiContentGenerationService> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public AiContentGenerationService(
        IQwenService qwen,
        IHttpClientFactory httpClientFactory,
        ILogger<AiContentGenerationService> logger)
    {
        _qwen = qwen;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<AiGenerateContentResponse> GenerateAsync(AiGenerateContentRequest request)
    {
        // 1. 准备源材料文本
        var source = await ResolveSourceTextAsync(request);

        // 2. 千问生成（严格 JSON）
        if (_qwen.IsConfigured && !string.IsNullOrWhiteSpace(source))
        {
            try
            {
                var result = await GenerateWithQwenAsync(source, request);
                if (result != null && result.Questions.Count > 0)
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "千问素材生成失败，回退到内置示例。");
            }
        }

        // 3. 回退：内置示例
        return GenerateFallback(request);
    }

    private async Task<string> ResolveSourceTextAsync(AiGenerateContentRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.SourceText))
            return request.SourceText.Trim();

        if (!string.IsNullOrWhiteSpace(request.PdfUrl))
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(20);
                var content = await client.GetStringAsync(request.PdfUrl);
                if (!string.IsNullOrWhiteSpace(content) && content.Length > 50)
                {
                    // 简单去 HTML 标签
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
            "你是党建学习平台的出题专家。请基于给定的源材料，严格按照要求生成题目，只输出一个 JSON 对象，禁止输出任何多余文字。\n" +
            "JSON 结构如下：\n" +
            "{\n" +
            "  \"questions\": [\n" +
            "    {\"type\":\"single|multi|truefalse\",\"stem\":\"题干\",\"options\":[\"选项A\",\"选项B\",\"选项C\",\"选项D\"],\"correct\":\"正确项标识\",\"score\":分值},\n" +
            "  ],\n" +
            "  \"flashcards\": [{\"front\":\"卡片正面问题\",\"back\":\"卡片背面答案\"}],\n" +
            "  \"summary\": \"一句话总结生成了多少题\"\n" +
            "}\n" +
            "规则：\n" +
            "- 单选题 correct 填选项字母（如 B）；多选题 correct 填正确项下标数组（如 [0,2]）；判断题选项固定为 [\"正确\",\"错误\"]，correct 填 A（正确）或 B（错误）。\n" +
            "- 题目必须来自源材料中的真实知识点，不得凭空编造；每道题 4 个选项（判断题 2 个）。";

        var user = new System.Text.StringBuilder();
        user.AppendLine($"【源材料】\n{(source.Length > 6000 ? source.Substring(0, 6000) : source)}");
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
        catch
        {
            return null;
        }
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
                // 多选题 correct 需为下标数组字符串
                if (!correct.StartsWith("["))
                {
                    var opts = q.Options ?? new List<string>();
                    var indices = correct.Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(c => c.Trim().ToUpperInvariant())
                        .Select(c => c.Length == 1 && c[0] >= 'A' && c[0] <= 'Z' ? c[0] - 'A' : -1)
                        .Where(i => i >= 0 && i < opts.Count)
                        .ToList();
                    if (indices.Count == 0 && int.TryParse(correct, out _)) indices = new List<int>();
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

    // ===== 解析用内部模型 =====

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

    /// <summary>将千问返回的 correct 字段统一解析为字符串形式（兼容 "B" / [0,2] / 数字）</summary>
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

