using PartySchoolApi.Models.Common;

namespace PartySchoolApi.Models.DTOs;

public class AiGenerateContentRequest
{
    public string ContentType { get; set; } = "questions";
    public string? SourceText { get; set; }
    public string? PdfUrl { get; set; }
    public string? Topic { get; set; }
    public string? Audience { get; set; }
    public int? DurationMinutes { get; set; }
    public string? Tone { get; set; }
    public int? MaxWords { get; set; }
    public List<string>? Keywords { get; set; }
    public int SingleChoiceCount { get; set; } = 5;
    public int MultiChoiceCount { get; set; } = 3;
    public int TrueFalseCount { get; set; } = 2;
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
    public string? Tag { get; set; }
}

public class AiGenerateContentResponse
{
    public string ContentType { get; set; } = "questions";
    public List<AiGeneratedQuestionDto> Questions { get; set; } = new();
    public List<AiFlashCardDto> FlashCards { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
    public AiGeneratedContentDto? Content { get; set; }
}

/// <summary>结合党员数据生成宣讲稿的请求</summary>
public class SpeechGenerateRequest
{
    /// <summary>目标组织（支部）Id</summary>
    public int OrganizationId { get; set; }

    /// <summary>宣讲主题，不传则使用默认党建主题</summary>
    public string? Topic { get; set; }

    /// <summary>目标时长（分钟），默认 15</summary>
    public int? DurationMinutes { get; set; }

    /// <summary>风格，默认 正式</summary>
    public string? Tone { get; set; }

    /// <summary>字数上限，默认 2500</summary>
    public int? MaxWords { get; set; }
}

/// <summary>结合党员数据生成宣讲稿的响应</summary>
public class SpeechGenerateResponse
{
    public string Summary { get; set; } = string.Empty;
    public AiGeneratedContentDto? Content { get; set; }

    /// <summary>宣讲稿引用的党员数据摘要</summary>
    public SpeechDataSummaryDto? DataSummary { get; set; }
}

/// <summary>宣讲稿引用的党员数据摘要</summary>
public class SpeechDataSummaryDto
{
    public string OrganizationName { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public int FormalCount { get; set; }
    public int ProbationaryCount { get; set; }
    public double TotalLearningHours { get; set; }
    public double? TaskCompletionRate { get; set; }
    public double? AvgExamScore { get; set; }
    public int IdleCount { get; set; }

    /// <summary>学习时长 Top 党员（如 "张三(12.5小时)"）</summary>
    public List<string> TopLearners { get; set; } = new();

    /// <summary>需重点关注党员（挂机/低分，如 "李四(挂机2次,平均分62)"）</summary>
    public List<string> WarningMembers { get; set; } = new();
}
