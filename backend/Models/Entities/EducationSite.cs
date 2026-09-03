using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PartySchoolApi.Models.Entities;

/// <summary>红色教育基地</summary>
[Table("education_sites")]
public class EducationSite
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    [Column("address")]
    public string? Address { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [MaxLength(2000)]
    [Column("historical_facts")]
    public string? HistoricalFacts { get; set; }

    [MaxLength(2000)]
    [Column("ai_interpretation")]
    public string? AiInterpretation { get; set; }

    [MaxLength(500)]
    [Column("cover_url")]
    public string? CoverUrl { get; set; }

    [Column("latitude")]
    public decimal? Latitude { get; set; }

    [Column("longitude")]
    public decimal? Longitude { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
