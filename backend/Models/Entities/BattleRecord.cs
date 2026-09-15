using System.ComponentModel.DataAnnotations;

namespace PartySchoolApi.Models.Entities;

/// <summary>党史PK对战记录</summary>
public class BattleRecord
{
    [Key]
    public int Id { get; set; }

    public int ChallengerId { get; set; }
    public PartyMember? Challenger { get; set; }

    public int OpponentId { get; set; }
    public PartyMember? Opponent { get; set; }

    /// <summary>对战结果JSON（各自得分、用时）</summary>
    [MaxLength(1000)]
    public string ResultJson { get; set; } = string.Empty;

    public DateTime BattleTime { get; set; } = DateTime.UtcNow;
}
