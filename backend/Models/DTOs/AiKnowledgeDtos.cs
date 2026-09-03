namespace PartySchoolApi.Models.DTOs;

public class AiKnowledgeQueryRequest
{
    public string Question { get; set; } = string.Empty;
    public string? SessionId { get; set; }
    public int TopK { get; set; } = 5;
    public bool Rerank { get; set; } = true;
    public string? FilterFile { get; set; }
}

public class AiKnowledgeQueryResponse
{
    public string Answer { get; set; } = string.Empty;
    public List<string> SourceReferences { get; set; } = new();
    public double Confidence { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public List<RagResultItem> Results { get; set; } = new();
}
