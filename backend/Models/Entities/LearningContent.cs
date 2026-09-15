using System.ComponentModel.DataAnnotations;
using PartySchoolApi.Models.Common;

namespace PartySchoolApi.Models.Entities;

/// <summary>学习内容</summary>
public class LearningContent
{
    [Key]
    public int Id { get; set; }

    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(8000)]
    public string? Body { get; set; }

    [MaxLength(500)]
    public string? VideoUrl { get; set; }

    public ContentType ContentType { get; set; } = ContentType.Article;

    public int? CategoryId { get; set; }
    public ContentCategory? Category { get; set; }

    public bool IsPublic { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ===== 新增字段 =====
    /// <summary>内容来源类型</summary>
    public ContentSourceType SourceType { get; set; } = ContentSourceType.Manual;

    /// <summary>关联文档URL</summary>
    [MaxLength(500)]
    public string? RelatedDocumentUrl { get; set; }

    // 导航属性
    public List<ContentTag> ContentTags { get; set; } = new();
    public List<TaskContent> TaskContents { get; set; } = new();
    public List<MemberLearningProgress> LearningProgresses { get; set; } = new();
}
