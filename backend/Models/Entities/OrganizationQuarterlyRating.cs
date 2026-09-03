using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PartySchoolApi.Models.Entities;

/// <summary>支部季度量化评级</summary>
[Table("organization_quarterly_ratings")]
public class OrganizationQuarterlyRating
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
    [Column("rating")]
    public char Rating { get; set; }

    [Column("rating_score")]
    public decimal RatingScore { get; set; }

    [MaxLength(4000)]
    [Column("detail_json")]
    public string? DetailJson { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
