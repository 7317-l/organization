using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PartySchoolApi.Models.Entities;

/// <summary>
/// 试卷表
/// </summary>
[Table("exam_papers")]
public class ExamPaper
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    /// <summary>题目Id列表，JSON格式存储试卷题目顺序，如[1,2,3]</summary>
    [Column("question_ids", TypeName = "json")]
    public string QuestionIds { get; set; } = "[]";

    [Column("total_score")]
    public int TotalScore { get; set; }

    public ICollection<ExamTest> ExamTests { get; set; } = new List<ExamTest>();

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
