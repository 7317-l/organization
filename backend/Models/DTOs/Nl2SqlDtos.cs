namespace PartySchoolApi.Models.DTOs;

public class Nl2SqlRequest
{
    public string NaturalLanguage { get; set; } = string.Empty;
    public string? SessionId { get; set; }
    public int HistoryCount { get; set; } = 5;
    public int? UserId { get; set; }
}

public class Nl2SqlResponse
{
    public string GeneratedSql { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public List<Dictionary<string, object>> ResultData { get; set; } = new();
    public ChartDataDto? ChartData { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public List<string> CorrectionsApplied { get; set; } = new();
    public string Intent { get; set; } = string.Empty;
    public string RewrittenQuery { get; set; } = string.Empty;
    public bool IsResolvedFromHistory { get; set; }
    public List<Nl2SqlConversationItem> Conversation { get; set; } = new();
}

public class ChartDataDto
{
    public string ChartType { get; set; } = "bar";
    public List<string> Labels { get; set; } = new();
    public List<double> Values { get; set; } = new();
}
