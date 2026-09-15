using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PartySchoolApi.Models.Entities;

/// <summary>
/// 标签表
/// </summary>
[Table("tags")]
public class Tag
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    public ICollection<ContentTag> ContentTags { get; set; } = new List<ContentTag>();

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
