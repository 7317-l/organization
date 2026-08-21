using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PartySchoolApi.Models.Entities;

/// <summary>
/// 组织架构表（党委/总支 → 支部，多级树形）
/// </summary>
[Table("organizations")]
public class Organization
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>父级组织Id，顶级为null</summary>
    [Column("parent_id")]
    public int? ParentId { get; set; }

    [ForeignKey(nameof(ParentId))]
    public Organization? Parent { get; set; }

    public ICollection<Organization> Children { get; set; } = new List<Organization>();

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // 导航：该组织下的党员
    public ICollection<PartyMember> Members { get; set; } = new List<PartyMember>();
}
