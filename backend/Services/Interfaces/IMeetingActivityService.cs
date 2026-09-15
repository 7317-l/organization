using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;

namespace PartySchoolApi.Services.Interfaces;

public interface IMeetingActivityService
{
    Task<PagedResponse> GetPagedAsync(MeetingActivityQueryParams query);
    Task<MeetingActivityDetailDto> GetByIdAsync(int id);
    Task<MeetingActivityDetailDto> CreateAsync(CreateMeetingActivityRequest request);
    Task DeleteAsync(int id);
    Task<ActivityHeartDto> SubmitHeartAsync(int memberId, SubmitActivityHeartRequest request);
    Task<List<ActivityHeartDto>> GetHeartsByActivityAsync(int activityId);
    Task<AiMeetingSummaryDto> GenerateAiSummaryAsync(int activityId);
}
