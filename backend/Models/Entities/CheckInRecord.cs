using System.ComponentModel.DataAnnotations;

namespace PartySchoolApi.Models.Entities;

/// <summary>红色教育基地打卡记录</summary>
public class CheckInRecord
{
    [Key]
    public int Id { get; set; }

    public int PartyMemberId { get; set; }
    public PartyMember? PartyMember { get; set; }

    [MaxLength(200)]
    public string LocationName { get; set; } = string.Empty;

    public DateTime CheckInTime { get; set; } = DateTime.UtcNow;

    [MaxLength(2000)]
    public string? Note { get; set; }

    /// <summary>AI背景解读</summary>
    [MaxLength(2000)]
    public string? AiBackgroundInterpretation { get; set; }

    /// <summary>教育基地ID</summary>
    public int? SiteId { get; set; }

    /// <summary>获得的积分</summary>
    public int PointsEarned { get; set; } = 5;
}
