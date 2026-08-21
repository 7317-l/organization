using PartySchoolApi.Models.DTOs;

namespace PartySchoolApi.Services.Interfaces;

public interface INotificationService
{
    Task SendAsync(SendNotificationRequest request);
    Task BatchSendAsync(BatchSendNotificationRequest request);
    Task<List<NotificationDto>> GetUnreadAsync(int memberId);
    Task<List<NotificationDto>> GetAllAsync(int memberId);
    Task MarkAsReadAsync(int memberId, int notificationId);
    Task MarkAllAsReadAsync(int memberId);
}
