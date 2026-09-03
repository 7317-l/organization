using PartySchoolApi.Models.Common;

namespace PartySchoolApi.Models.DTOs;

public class CheckInQueryParams : PagedQueryParams
{
    public int? PartyMemberId { get; set; }
    public string? LocationName { get; set; }
}

public class CheckInRecordDto
{
    public int Id { get; set; }
    public int PartyMemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public DateTime CheckInTime { get; set; }
    public string? Note { get; set; }
    public string? AiBackgroundInterpretation { get; set; }
    public int? SiteId { get; set; }
    public string? SiteName { get; set; }
    public List<string> HistoricalFacts { get; set; } = new();
    public int PointsEarned { get; set; }
}

public class CreateCheckInRequest
{
    public string LocationName { get; set; } = string.Empty;
    public string? Note { get; set; }
    public int? SiteId { get; set; }
}

public class AiBackgroundDto
{
    public string LocationName { get; set; } = string.Empty;
    public string Interpretation { get; set; } = string.Empty;
    public List<string> HistoricalFacts { get; set; } = new();
}
