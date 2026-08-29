using System.ComponentModel.DataAnnotations;
using PartySchoolApi.Models.Common;

namespace PartySchoolApi.Models.Entities;

/// <summary>党员用户</summary>
public class PartyMember
{
    [Key]
    public int Id { get; set; }

    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(200)]
    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.PartyMember;

    public int OrganizationId { get; set; }
    public Organization? Organization { get; set; }

    public bool IsEnabled { get; set; } = true;

    [MaxLength(500)]
    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiry { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ===== 新增字段 =====
    /// <summary>总积分</summary>
    public int PointTotal { get; set; } = 0;

    /// <summary>党员身份：正式党员 / 预备党员</summary>
    [MaxLength(20)]
    public string MemberType { get; set; } = "正式党员";

    // 导航属性
    public List<MemberLearningProgress> LearningProgresses { get; set; } = new();
    public List<MemberTestRecord> TestRecords { get; set; } = new();
}
