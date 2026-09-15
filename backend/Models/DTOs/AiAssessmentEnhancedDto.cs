namespace PartySchoolApi.Models.DTOs;

/// <summary>增强版AI评价报告</summary>
public class AiAssessmentEnhancedResponse
{
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public double OverallScore { get; set; }
    public string Level { get; set; } = string.Empty;
    public RadarChartDataDto RadarData { get; set; } = new();
    public List<AiDimensionDetailDto> Dimensions { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
    public List<string> Suggestions { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class RadarChartDataDto
{
    public List<string> Labels { get; set; } = new();
    public List<double> Values { get; set; } = new();
}

public class AiDimensionDetailDto
{
    public string Name { get; set; } = string.Empty;
    public double Score { get; set; }
    public double Weight { get; set; }
    public double WeightedScore { get; set; }
    public string Comment { get; set; } = string.Empty;
}

/// <summary>加权推荐内容</summary>
public class WeightedRecommendationDto
{
    public ContentListItemDto Content { get; set; } = new();
    public double TotalScore { get; set; }
    public double ErrorMatchScore { get; set; }
    public double SimilarityScore { get; set; }
    public double UrgencyScore { get; set; }
    public string Reason { get; set; } = string.Empty;
}
