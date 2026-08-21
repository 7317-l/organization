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

    public ICollection<TaskContent> TaskContents { get; set; } = new List<TaskContent>();

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
