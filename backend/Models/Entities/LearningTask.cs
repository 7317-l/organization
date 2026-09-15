using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PartySchoolApi.Models.Entities;

/// <summary>
/// 学习任务表
/// </summary>
[Table("learning_tasks")]
public class LearningTask
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("task_name")]
    public string TaskName { get; set; } = string.Empty;

    [Column("target_org_id")]
    public int TargetOrgId { get; set; }

    [ForeignKey(nameof(TargetOrgId))]
    public Organization? TargetOrg { get; set; }

    [Column("deadline")]
    public DateTime Deadline { get; set; }

    /// <summary>关联的测验ID（可选，完成测验后自动关闭任务）</summary>
    [Column("test_id")]
    public int? TestId { get; set; }

    [ForeignKey(nameof(TestId))]
    public ExamTest? Test { get; set; }

    /// <summary>任务状态：0-进行中，1-已完成，2-已关闭</summary>
    [Column("status")]
    public int Status { get; set; } = 0;

    public ICollection<TaskContent> TaskContents { get; set; } = new List<TaskContent>();

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
