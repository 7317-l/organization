using PartySchoolApi.Models.DTOs;

namespace PartySchoolApi.Services.Interfaces;

public interface IOrganizationService
{
    Task<List<OrganizationTreeDto>> GetTreeAsync();
    Task<OrganizationTreeDto> CreateAsync(CreateOrganizationRequest request);
    Task UpdateAsync(int id, UpdateOrganizationRequest request);
    Task DeleteAsync(int id);
    Task<OrganizationStatsDto> GetStatsAsync(int orgId);
}
