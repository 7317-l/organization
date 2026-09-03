using PartySchoolApi.Models.DTOs;

namespace PartySchoolApi.Services.Interfaces;

public interface IRectificationService
{
    Task<(List<RectificationDto> items, long total)> GetRectificationsAsync(int organizationId, string? quarter, int? status, int page, int size);
    Task<RectificationDto> CreateRectificationAsync(int organizationId, CreateRectificationRequest request);
    Task<RectificationDto> CompleteRectificationAsync(int id, CompleteRectificationRequest request);
    Task<RectificationDto> UpdateStatusAsync(int id, UpdateRectificationStatusRequest request);
}
