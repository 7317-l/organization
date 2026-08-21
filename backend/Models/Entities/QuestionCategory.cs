using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PartySchoolApi.Models.Entities;

/// <summary>
/// 题库分类表
/// </summary>
[Table("question_categories")]
public class QuestionCategory
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    public ICollection<Question> Questions { get; set; } = new List<Question>();

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
