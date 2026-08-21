using PartySchoolApi.Models.Common;

namespace PartySchoolApi.Models.DTOs;

public class PointRecordQueryParams : PagedQueryParams
{
    public int? PartyMemberId { get; set; }
    public PointSourceType? SourceType { get; set; }
}

public class PointRecordDto
{
    public int Id { get; set; }
    public int PartyMemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public PointSourceType SourceType { get; set; }
    public string SourceTypeName { get; set; } = string.Empty;
    public int? SourceId { get; set; }
    public int Points { get; set; }
    public DateTime EarnedAt { get; set; }
}

public class PointRankingDto
{
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public int TotalPoints { get; set; }
    public int Rank { get; set; }
}
