using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;

namespace PartySchoolApi.Services.Interfaces;

public interface IPartyDevelopmentService
{
    Task<PagedResponse> GetPagedAsync(PartyDevelopmentQueryParams query);
    Task<PartyDevelopmentDetailDto> GetByIdAsync(int id);
    Task<PartyDevelopmentDetailDto> CreateAsync(CreatePartyDevelopmentRequest request);
    Task SubmitAsync(int id, SubmitPartyDevelopmentRequest request);
    Task ReviewAsync(int id, ReviewPartyDevelopmentRequest request);
    Task AdvanceStageAsync(int id);
    Task<AiMaterialCheckResultDto> AiCheckMaterialsAsync(int id);
    Task<List<PartyDevelopmentListItemDto>> GetRemindersAsync();
    Task DeleteAsync(int id);
}
