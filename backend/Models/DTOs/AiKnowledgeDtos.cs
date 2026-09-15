namespace PartySchoolApi.Models.DTOs;

public class AiKnowledgeQueryRequest
{
    public string Question { get; set; } = string.Empty;
    /// <summary>会话Id，用于多轮上下文</summary>
    public string? SessionId { get; set; }
}

public class AiKnowledgeQueryResponse
{
    public string Answer { get; set; } = string.Empty;
    public List<string> SourceReferences { get; set; } = new();
    public double Confidence { get; set; }
    public string SessionId { get; set; } = string.Empty;
}
