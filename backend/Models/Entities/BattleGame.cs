using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PartySchoolApi.Models.Entities;

/// <summary>党史PK对局</summary>
[Table("battle_games")]
public class BattleGame
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("challenger_id")]
    public int ChallengerId { get; set; }

    [ForeignKey(nameof(ChallengerId))]
    public PartyMember? Challenger { get; set; }

    [Column("opponent_id")]
    public int OpponentId { get; set; }

    [ForeignKey(nameof(OpponentId))]
    public PartyMember? Opponent { get; set; }

    [Column("status")]
    public int Status { get; set; } = 0;

    [Required]
    [Column("question_ids")]
    public string QuestionIds { get; set; } = "[]";

    [Column("challenger_score")]
    public int ChallengerScore { get; set; } = 0;

    [Column("opponent_score")]
    public int OpponentScore { get; set; } = 0;

    [Column("current_question_index")]
    public int CurrentQuestionIndex { get; set; } = 0;

    [Column("timeout_minutes")]
    public int TimeoutMinutes { get; set; } = 10;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("started_at")]
    public DateTime? StartedAt { get; set; }

    [Column("finished_at")]
    public DateTime? FinishedAt { get; set; }
}
