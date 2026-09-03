using PartySchoolApi.Models.DTOs;

namespace PartySchoolApi.Services.Interfaces;

public interface INotificationService
{
    Task SendAsync(SendNotificationRequest request);
    Task BatchSendAsync(BatchSendNotificationRequest request);
    Task<List<NotificationDto>> GetUnreadAsync(int memberId);
    Task<List<NotificationDto>> GetAllAsync(int memberId);
    Task MarkReadAsync(int id, int memberId);
    Task MarkAllReadAsync(int memberId);
    Task<TargetedSendResponse> TargetedSendAsync(TargetedSendRequest request, int currentRole, int currentOrgId);
}
