using System.ComponentModel.DataAnnotations;

namespace PartySchoolApi.Models.Entities;

/// <summary>支部季度AI考核报告</summary>
public class OrganizationQuarterlyReport
{
    [Key]
    public int Id { get; set; }

    public int OrganizationId { get; set; }
    public Organization? Organization { get; set; }

    /// <summary>季度标识，如 2026Q1</summary>
    [MaxLength(20)]
    public string Quarter { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string ReportJson { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
