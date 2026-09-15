using System.ComponentModel.DataAnnotations;

namespace PartySchoolApi.Models.Entities;

/// <summary>测验发布记录</summary>
public class ExamTest
{
    [Key]
    public int Id { get; set; }

    public int PaperId { get; set; }
    public ExamPaper? Paper { get; set; }

    public int PublisherId { get; set; }
    public PartyMember? Publisher { get; set; }

    public int TargetOrgId { get; set; }
    public Organization? TargetOrg { get; set; }

    /// <summary>限时（分钟）</summary>
    public int TimeLimitMinutes { get; set; } = 60;

    public DateTime Deadline { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ===== 新增字段 =====
    /// <summary>是否AI生成</summary>
    public bool IsAiGenerated { get; set; } = false;

    /// <summary>目标薄弱知识点标签JSON，如 ["党史","党章"]</summary>
    [MaxLength(1000)]
    public string? TargetWeaknessTags { get; set; }

    // 导航属性
    public List<MemberTestRecord> TestRecords { get; set; } = new();
}
