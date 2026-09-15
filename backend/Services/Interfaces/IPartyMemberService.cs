using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;

namespace PartySchoolApi.Services.Interfaces;

public interface IPartyMemberService
{
    Task<PagedResponse> GetPagedAsync(MemberQueryParams query);
    Task<MemberListItemDto> GetByIdAsync(int id);
    Task<MemberListItemDto> CreateAsync(CreateMemberRequest request);
    Task UpdateAsync(int id, UpdateMemberRequest request);
    Task DeleteAsync(int id);
    Task AssignRoleAsync(int id, Models.Common.UserRole role);
    Task<ImportResultDto> ImportAsync(Stream fileStream, string fileName);
    Task<byte[]> ExportAsync();
}
