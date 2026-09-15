using System.ComponentModel.DataAnnotations;

namespace PartySchoolApi.Models.Entities;

/// <summary>党员互助结对帮扶记录</summary>
public class PairHelpRecord
{
    [Key]
    public int Id { get; set; }

    public int HelperId { get; set; }
    public PartyMember? Helper { get; set; }

    public int HelpReceiverId { get; set; }
    public PartyMember? HelpReceiver { get; set; }

    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    public DateTime? EndTime { get; set; }

    /// <summary>帮扶内容记录JSON</summary>
    [MaxLength(4000)]
    public string? HelpContentJson { get; set; }

    /// <summary>学习成果简述</summary>
    [MaxLength(2000)]
    public string? OutcomeSummary { get; set; }
}
