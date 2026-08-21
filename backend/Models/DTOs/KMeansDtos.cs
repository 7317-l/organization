namespace PartySchoolApi.Models.DTOs;

public class KMeansClusteringRequest
{
    public int PartyMemberId { get; set; }
    /// <summary>聚类数量，默认3</summary>
    public int ClusterCount { get; set; } = 3;
}

public class KMeansClusterDto
{
    public string ClusterName { get; set; } = string.Empty;
    public List<string> KnowledgeTags { get; set; } = new();
    public int ErrorCount { get; set; }
    public double Severity { get; set; }
}

public class KMeansClusteringResponse
{
    public int PartyMemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public List<KMeansClusterDto> Clusters { get; set; } = new();
    public List<string> TopWeaknessTags { get; set; } = new();
    public string Suggestion { get; set; } = string.Empty;
}
