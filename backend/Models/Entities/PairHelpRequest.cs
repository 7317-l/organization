using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PartySchoolApi.Models.Entities;

/// <summary>结对帮扶申请</summary>
[Table("pair_help_requests")]
public class PairHelpRequest
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("helper_id")]
    public int HelperId { get; set; }

    [ForeignKey(nameof(HelperId))]
    public PartyMember? Helper { get; set; }

    [Column("help_receiver_id")]
    public int HelpReceiverId { get; set; }

    [ForeignKey(nameof(HelpReceiverId))]
    public PartyMember? HelpReceiver { get; set; }

    [Column("status")]
    public int Status { get; set; } = 0;

    [MaxLength(1000)]
    [Column("match_reason")]
    public string? MatchReason { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
