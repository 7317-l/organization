using PartySchoolApi.Models.Common;

namespace PartySchoolApi.Models.DTOs;

public class MeetingActivityQueryParams : PagedQueryParams
{
    public int? OrganizationId { get; set; }
    public MeetingType? Type { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class MeetingActivityListItemDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public MeetingType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime ActivityTime { get; set; }
    public bool IsAiSummaryGenerated { get; set; }
    public int HeartCount { get; set; }
    /// <summary>状态：0=草稿，1=待审核，2=已归档，3=已上报</summary>
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
}

public class MeetingActivityDetailDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public MeetingType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ActivityTime { get; set; }
    public bool IsAiSummaryGenerated { get; set; }
    public string? AiSummaryContent { get; set; }
    public List<ActivityHeartDto> Hearts { get; set; } = new();
    /// <summary>状态：0=草稿，1=待审核，2=已归档，3=已上报</summary>
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewComment { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public DateTime? ReportedAt { get; set; }
}

public class CreateMeetingActivityRequest
{
    public int OrganizationId { get; set; }
    public MeetingType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ActivityTime { get; set; }
}

public class UpdateMeetingActivityRequest
{
    public MeetingType? Type { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? ActivityTime { get; set; }
    public string? AiSummaryContent { get; set; }
}

public class ReviewMeetingActivityRequest
{
    public bool Approved { get; set; }
    public string? Comment { get; set; }
}

public class ActivityHeartDto
{
    public int Id { get; set; }
    public int MeetingActivityId { get; set; }
    public int PartyMemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public string? AiPolishSuggestion { get; set; }
}

public class SubmitActivityHeartRequest
{
    public int MeetingActivityId { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class AiMeetingSummaryDto
{
    public int ActivityId { get; set; }
    public string Summary { get; set; } = string.Empty;
    public List<string> KeyPoints { get; set; } = new();
}
