using System.ComponentModel.DataAnnotations;
using PartySchoolApi.Models.Common;

namespace PartySchoolApi.Models.Entities;

/// <summary>党员发展全流程记录</summary>
public class PartyDevelopmentProcess
{
    [Key]
    public int Id { get; set; }

    public int PartyMemberId { get; set; }
    public PartyMember? PartyMember { get; set; }

    public PartyDevelopmentStage Stage { get; set; }

    public ProcessStatus Status { get; set; }

    /// <summary>提交材料清单JSON</summary>
    [MaxLength(2000)]
    public string? MaterialsJson { get; set; }

    /// <summary>思想汇报内容</summary>
    [MaxLength(4000)]
    public string? ReportContent { get; set; }

    public DateTime? SubmittedAt { get; set; }

    [MaxLength(1000)]
    public string? ReviewComment { get; set; }

    public int? ReviewerId { get; set; }

    public DateTime? ReviewedAt { get; set; }

    /// <summary>是否已发送转正提醒</summary>
    public bool IsReminderSent { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
