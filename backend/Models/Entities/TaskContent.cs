using System.ComponentModel.DataAnnotations.Schema;

namespace PartySchoolApi.Models.Entities;

/// <summary>
/// 任务-内容关联表（多对多）
/// </summary>
[Table("task_contents")]
public class TaskContent
{
    [Column("task_id")]
    public int TaskId { get; set; }

    [ForeignKey(nameof(TaskId))]
    public LearningTask? Task { get; set; }

    [Column("content_id")]
    public int ContentId { get; set; }

    [ForeignKey(nameof(ContentId))]
    public LearningContent? Content { get; set; }
}
