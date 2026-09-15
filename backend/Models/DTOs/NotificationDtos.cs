using PartySchoolApi.Models.Common;

namespace PartySchoolApi.Models.DTOs;

public class NotificationDto
{
    public int Id { get; set; }
    public int PartyMemberId { get; set; }
    public NotificationType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SendNotificationRequest
{
    public int PartyMemberId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class BatchSendNotificationRequest
{
    public List<int> PartyMemberIds { get; set; } = new();
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
