using System.ComponentModel.DataAnnotations;
using PartySchoolApi.Models.Common;

namespace PartySchoolApi.Models.Entities;

/// <summary>消息推送记录</summary>
public class MessageNotification
{
    [Key]
    public int Id { get; set; }

    public int PartyMemberId { get; set; }
    public PartyMember? PartyMember { get; set; }

    public NotificationType Type { get; set; }

    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
