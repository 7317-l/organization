using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PartySchoolApi.Models.Entities;

/// <summary>
/// 内容分类表（树形）
/// </summary>
[Table("content_categories")]
public class ContentCategory
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("parent_id")]
    public int? ParentId { get; set; }

    [ForeignKey(nameof(ParentId))]
    public ContentCategory? Parent { get; set; }

    public ICollection<ContentCategory> Children { get; set; } = new List<ContentCategory>();

    public ICollection<LearningContent> Contents { get; set; } = new List<LearningContent>();

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
