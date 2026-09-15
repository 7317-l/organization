using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PartySchoolApi.Models.Entities;

/// <summary>
/// 党员学习进度表
/// </summary>
[Table("member_learning_progress")]
public class MemberLearningProgress
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("member_id")]
    public int MemberId { get; set; }

    [ForeignKey(nameof(MemberId))]
    public PartyMember? Member { get; set; }

    [Column("content_id")]
    public int ContentId { get; set; }

    [ForeignKey(nameof(ContentId))]
    public LearningContent? Content { get; set; }

    /// <summary>任务Id，为空表示公共内容自主学习</summary>
    [Column("task_id")]
    public int? TaskId { get; set; }

    [ForeignKey(nameof(TaskId))]
    public LearningTask? Task { get; set; }

    /// <summary>学习时长（秒）</summary>
    [Column("duration_seconds")]
    public int DurationSeconds { get; set; }

    [Column("is_completed")]
    public bool IsCompleted { get; set; }

    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
