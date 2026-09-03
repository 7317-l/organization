using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PartySchoolApi.Models.Entities;

/// <summary>支部整改项</summary>
[Table("org_rectifications")]
public class OrgRectification
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    public Organization? Organization { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("quarter")]
    public string Quarter { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    [Column("issue")]
    public string Issue { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    [Column("suggestion")]
    public string Suggestion { get; set; } = string.Empty;

    [Column("status")]
    public int Status { get; set; } = 0;

    [MaxLength(500)]
    [Column("remark")]
    public string? Remark { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }
}
