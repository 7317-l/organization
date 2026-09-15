using System.ComponentModel.DataAnnotations;
using PartySchoolApi.Models.Common;

namespace PartySchoolApi.Models.Entities;

/// <summary>积分获取记录</summary>
public class LearningPoint
{
    [Key]
    public int Id { get; set; }

    public int PartyMemberId { get; set; }
    public PartyMember? PartyMember { get; set; }

    public PointSourceType SourceType { get; set; }

    /// <summary>关联内容Id（可为空）</summary>
    public int? SourceId { get; set; }

    public int Points { get; set; }

    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;
}
