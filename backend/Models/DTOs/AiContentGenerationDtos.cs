using PartySchoolApi.Models.Common;

namespace PartySchoolApi.Models.DTOs;

public class AiGenerateContentRequest
{
    /// <summary>源文本内容</summary>
    public string? SourceText { get; set; }
    /// <summary>PDF文件URL（二选一）</summary>
    public string? PdfUrl { get; set; }
    /// <summary>生成单选题数量</summary>
    public int SingleChoiceCount { get; set; } = 5;
    /// <summary>生成多选题数量</summary>
    public int MultiChoiceCount { get; set; } = 3;
    /// <summary>生成判断题数量</summary>
    public int TrueFalseCount { get; set; } = 2;
    /// <summary>是否生成学习卡片</summary>
    public bool GenerateFlashCards { get; set; } = true;
    public int? CategoryId { get; set; }
}

public class AiGeneratedQuestionDto
{
    public QuestionType QuestionType { get; set; }
    public string QuestionTypeName { get; set; } = string.Empty;
    public string Stem { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public string CorrectAnswer { get; set; } = string.Empty;
    public int Score { get; set; } = 10;
}

public class AiFlashCardDto
{
    public string Front { get; set; } = string.Empty;
    public string Back { get; set; } = string.Empty;
}

public class AiGenerateContentResponse
{
    public List<AiGeneratedQuestionDto> Questions { get; set; } = new();
    public List<AiFlashCardDto> FlashCards { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
}
