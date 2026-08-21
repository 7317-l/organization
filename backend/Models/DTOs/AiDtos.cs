using System.ComponentModel.DataAnnotations;

namespace PartySchoolApi.Models.DTOs;

/// <summary>AI推荐响应</summary>
public class AiRecommendationResponse
{
    public List<ContentListItemDto> Contents { get; set; } = new();
    public string Reason { get; set; } = string.Empty;
}

/// <summary>AI自然语言查询请求</summary>
public class AiQueryRequest
{
    [Required(ErrorMessage = "问题不能为空")]
    public string Question { get; set; } = string.Empty;

    public string? Context { get; set; }
}

/// <summary>AI自然语言查询响应</summary>
public class AiQueryResponse
{
    public string AnswerText { get; set; } = string.Empty;
    public object? ChartData { get; set; }
    public string Intent { get; set; } = string.Empty;
}

/// <summary>AI评价报告请求</summary>
public class AiAssessmentRequest
{
    public int? MemberId { get; set; }
}

/// <summary>AI评价报告响应</summary>
public class AiAssessmentResponse
{
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public double OverallScore { get; set; }
    public string Level { get; set; } = string.Empty;
    public List<AiDimensionDto> Dimensions { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
    public List<string> Suggestions { get; set; } = new();
}

/// <summary>AI评价维度</summary>
public class AiDimensionDto
{
    public string Name { get; set; } = string.Empty;
    public double Score { get; set; }
    public string Comment { get; set; } = string.Empty;
}
