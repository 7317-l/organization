using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;

namespace PartySchoolApi.Services.Interfaces;

public interface ICheckInService
{
    Task<PagedResponse> GetPagedAsync(CheckInQueryParams query);
    Task<CheckInRecordDto> CreateAsync(int memberId, CreateCheckInRequest request);
    Task<AiBackgroundDto> GetAiBackgroundAsync(string locationName);
}
