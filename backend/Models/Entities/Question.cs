using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PartySchoolApi.Models.Common;

namespace PartySchoolApi.Models.Entities;

/// <summary>
/// 题目表
/// </summary>
[Table("questions")]
public class Question
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("question_type")]
    public QuestionType QuestionType { get; set; } = QuestionType.SingleChoice;

    [Required]
    [Column("stem", TypeName = "text")]
    public string Stem { get; set; } = string.Empty;

    /// <summary>选项，JSON格式存储，如["A选项","B选项"]</summary>
    [Column("options", TypeName = "json")]
    public string Options { get; set; } = "[]";

    /// <summary>正确答案，JSON格式，单选如"0"，多选如"[0,2]"，判断如"true"</summary>
    [Required]
    [MaxLength(200)]
    [Column("correct_answer")]
    public string CorrectAnswer { get; set; } = string.Empty;

    /// <summary>分值</summary>
    [Column("score")]
    public int Score { get; set; } = 5;

    [Column("category_id")]
    public int? CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public QuestionCategory? Category { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
