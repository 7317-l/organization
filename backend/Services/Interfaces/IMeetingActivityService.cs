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
    Task<MeetingBriefResponse> GenerateBriefAsync(MeetingBriefRequest request, int currentRole, int currentOrgId);

    // 组织生活闭环：编辑、审核、归档、上报
    Task<MeetingActivityDetailDto> UpdateAsync(int id, UpdateMeetingActivityRequest request);
    Task<MeetingActivityDetailDto> ReviewAsync(int id, ReviewMeetingActivityRequest request);
    Task<MeetingActivityDetailDto> ArchiveAsync(int id);
    Task<MeetingActivityDetailDto> ReportAsync(int id);
}
