using System.ComponentModel.DataAnnotations.Schema;

namespace PartySchoolApi.Models.Entities;

/// <summary>
/// 内容-标签关联表（多对多）
/// </summary>
[Table("content_tags")]
public class ContentTag
{
    [Column("content_id")]
    public int ContentId { get; set; }

    [ForeignKey(nameof(ContentId))]
    public LearningContent? Content { get; set; }

    [Column("tag_id")]
    public int TagId { get; set; }

    [ForeignKey(nameof(TagId))]
    public Tag? Tag { get; set; }
}
