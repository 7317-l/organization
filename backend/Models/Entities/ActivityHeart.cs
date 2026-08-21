using System.ComponentModel.DataAnnotations;

namespace PartySchoolApi.Models.Entities;

/// <summary>活动心得提交</summary>
public class ActivityHeart
{
    [Key]
    public int Id { get; set; }

    public int MeetingActivityId { get; set; }
    public MeetingActivity? MeetingActivity { get; set; }

    public int PartyMemberId { get; set; }
    public PartyMember? PartyMember { get; set; }

    [MaxLength(4000)]
    public string Content { get; set; } = string.Empty;

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    /// <summary>AI润色建议</summary>
    [MaxLength(2000)]
    public string? AiPolishSuggestion { get; set; }
}
