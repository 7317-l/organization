using PartySchoolApi.Models.Common;

namespace PartySchoolApi.Models.DTOs;

public class PartyDevelopmentQueryParams : PagedQueryParams
{
    public int? PartyMemberId { get; set; }
    public PartyDevelopmentStage? Stage { get; set; }
    public ProcessStatus? Status { get; set; }
}

public class PartyDevelopmentListItemDto
{
    public int Id { get; set; }
    public int PartyMemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string StageName { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public bool IsReminderSent { get; set; }
}

public class PartyDevelopmentDetailDto
{
    public int Id { get; set; }
    public int PartyMemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public PartyDevelopmentStage Stage { get; set; }
    public string StageName { get; set; } = string.Empty;
    public ProcessStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public List<string>? Materials { get; set; }
    public string? ReportContent { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string? ReviewComment { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public bool IsReminderSent { get; set; }
}

public class CreatePartyDevelopmentRequest
{
    public int PartyMemberId { get; set; }
    public PartyDevelopmentStage Stage { get; set; }
    public List<string>? Materials { get; set; }
    public string? ReportContent { get; set; }
}

public class SubmitPartyDevelopmentRequest
{
    public List<string>? Materials { get; set; }
    public string? ReportContent { get; set; }
}

/// <summary>更新发展流程记录基本信息（材料、思想汇报等）</summary>
public class UpdatePartyDevelopmentRequest
{
    /// <summary>提交材料清单</summary>
    public List<string>? Materials { get; set; }

    /// <summary>思想汇报内容</summary>
    public string? ReportContent { get; set; }
}

public class ReviewPartyDevelopmentRequest
{
    public bool IsApproved { get; set; }
    public string ReviewComment { get; set; } = string.Empty;
}

public class AiMaterialCheckResultDto
{
    public bool IsComplete { get; set; }
    public List<string> MissingMaterials { get; set; } = new();
    public string Suggestion { get; set; } = string.Empty;
}
