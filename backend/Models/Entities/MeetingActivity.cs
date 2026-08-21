using System.ComponentModel.DataAnnotations;
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

    public List<ActivityHeart> ActivityHearts { get; set; } = new();
}
