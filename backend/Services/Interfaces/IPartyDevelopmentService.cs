using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;

namespace PartySchoolApi.Services.Interfaces;

public interface IPartyDevelopmentService
{
    Task<(List<PartyDevelopmentListItemDto> items, long total)> GetListAsync(PartyDevelopmentQueryParams query);
    Task<PartyDevelopmentDetailDto?> GetByIdAsync(int id);
    Task<PartyDevelopmentDetailDto> CreateAsync(CreatePartyDevelopmentRequest request);
    Task<PartyDevelopmentDetailDto> SubmitAsync(int id, SubmitPartyDevelopmentRequest request);
    Task<PartyDevelopmentDetailDto> ReviewAsync(int id, ReviewPartyDevelopmentRequest request);
    Task<PartyDevelopmentDetailDto> AdvanceStageAsync(int id);
    Task<List<PartyDevelopmentListItemDto>> GetRemindersAsync();
    Task<AiMaterialCheckResultDto> AiCheckMaterialsAsync(int id);
    Task<ReportSuggestionResponse> GetReportSuggestionAsync(int id, ReportSuggestionRequest request, int currentMemberId, int currentRole);
    Task<MaterialCheckResponse> CheckMaterialsV2Async(int id, MaterialCheckRequest request, int currentMemberId, int currentRole);
    Task<ReminderTriggerResponse> TriggerRemindersAsync(ReminderTriggerRequest request, int currentRole, int currentOrgId);
    Task<(List<ReminderItemDto> items, long total)> GetRemindersListAsync(ReminderQueryParams query, int currentRole, int currentOrgId);
}
