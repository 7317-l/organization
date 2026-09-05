using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PartySchoolApi.Models.Common;

namespace PartySchoolApi.Models.Entities;

/// <summary>三会一课/主题党日活动</summary>
public class MeetingActivity
{
    [Key]
    public int Id { get; set; }

    public int OrganizationId { get; set; }
    public Organization? Organization { get; set; }

    public MeetingType Type { get; set; }

    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Description { get; set; }

    /// <summary>活动时间</summary>
    public DateTime ActivityTime { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>是否已生成AI总结</summary>
    public bool IsAiSummaryGenerated { get; set; } = false;

    /// <summary>AI总结内容</summary>
    [MaxLength(4000)]
    public string? AiSummaryContent { get; set; }

    /// <summary>活动状态：0=草稿，1=待审核，2=已归档，3=已上报</summary>
    [Column("status")]
    public int Status { get; set; } = 0;

    /// <summary>审核人ID</summary>
    [Column("reviewer_id")]
    public int? ReviewerId { get; set; }

    /// <summary>审核时间</summary>
    [Column("reviewed_at")]
    public DateTime? ReviewedAt { get; set; }

    /// <summary>审核意见</summary>
    [MaxLength(1000)]
    [Column("review_comment")]
    public string? ReviewComment { get; set; }

    /// <summary>归档时间</summary>
    [Column("archived_at")]
    public DateTime? ArchivedAt { get; set; }

    /// <summary>上报时间</summary>
    [Column("reported_at")]
    public DateTime? ReportedAt { get; set; }

    public List<ActivityHeart> ActivityHearts { get; set; } = new();
}
