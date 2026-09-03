using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PartySchoolApi.Models.Entities;

/// <summary>NL2SQL 多轮会话记录</summary>
[Table("nl2sql_sessions")]
public class Nl2SqlSession
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(64)]
    [Column("session_id")]
    public string SessionId { get; set; } = string.Empty;

    [Column("member_id")]
    public int MemberId { get; set; }

    [ForeignKey(nameof(MemberId))]
    public PartyMember? Member { get; set; }

    [Required]
    [Column("question")]
    public string Question { get; set; } = string.Empty;

    [Column("rewritten")]
    public string? Rewritten { get; set; }

    [Column("sql_text")]
    public string? SqlText { get; set; }

    [MaxLength(2000)]
    [Column("explanation")]
    public string? Explanation { get; set; }

    [MaxLength(4000)]
    [Column("result_summary")]
    public string? ResultSummary { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
