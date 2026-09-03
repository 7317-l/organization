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
