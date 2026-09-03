using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PartySchoolApi.Models.Entities;

/// <summary>党员发展到期提醒记录</summary>
[Table("party_development_reminders")]
public class PartyDevelopmentReminder
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("process_id")]
    public int ProcessId { get; set; }

    [ForeignKey(nameof(ProcessId))]
    public PartyDevelopmentProcess? Process { get; set; }

    [Column("party_member_id")]
    public int PartyMemberId { get; set; }

    [ForeignKey(nameof(PartyMemberId))]
    public PartyMember? PartyMember { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("reminder_type")]
    public string ReminderType { get; set; } = string.Empty;

    [Column("due_date")]
    public DateTime? DueDate { get; set; }

    [Required]
    [MaxLength(500)]
    [Column("message")]
    public string Message { get; set; } = string.Empty;

    [Column("status")]
    public int Status { get; set; } = 0;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("sent_at")]
    public DateTime? SentAt { get; set; }
}
