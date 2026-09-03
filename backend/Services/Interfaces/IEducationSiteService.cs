using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Models.Entities;

namespace PartySchoolApi.Services.Interfaces;

public interface IEducationSiteService
{
    Task<(List<EducationSiteDto> items, long total)> GetSitesAsync(EducationSiteQueryParams query);
    Task<EducationSiteDto?> GetSiteAsync(int id);
    Task<EducationSiteDto> CreateSiteAsync(EducationSite site);
    Task<EducationSiteDto> UpdateSiteAsync(int id, EducationSite site);
    Task DeleteSiteAsync(int id);
    Task<(List<EducationSiteCheckinDto> items, long total)> GetSiteCheckinsAsync(int siteId, int page, int size);
}
