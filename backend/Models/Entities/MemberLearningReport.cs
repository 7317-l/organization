using System.ComponentModel.DataAnnotations;

namespace PartySchoolApi.Models.Entities;

/// <summary>党员个人AI学习报告历史记录</summary>
public class MemberLearningReport
{
    [Key]
    public int Id { get; set; }

    public int PartyMemberId { get; set; }
    public PartyMember? PartyMember { get; set; }

    /// <summary>报告内容JSON（评语/得分/维度等）</summary>
    [MaxLength(4000)]
    public string ReportJson { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
