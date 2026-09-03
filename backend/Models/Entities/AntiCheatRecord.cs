using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PartySchoolApi.Models.Entities;

/// <summary>防挂机验证记录</summary>
[Table("anticheat_records")]
public class AntiCheatRecord
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("party_member_id")]
    public int PartyMemberId { get; set; }

    [ForeignKey(nameof(PartyMemberId))]
    public PartyMember? PartyMember { get; set; }

    [Column("content_id")]
    public int? ContentId { get; set; }

    [Column("question_id")]
    public int? QuestionId { get; set; }

    [Required]
    [MaxLength(64)]
    [Column("challenge_id")]
    public string ChallengeId { get; set; } = string.Empty;

    [Column("is_pass")]
    public bool IsPass { get; set; }

    [Column("verified_at")]
    public DateTime VerifiedAt { get; set; } = DateTime.Now;
}
