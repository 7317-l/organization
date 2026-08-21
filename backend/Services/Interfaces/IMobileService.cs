using PartySchoolApi.Models.DTOs;

namespace PartySchoolApi.Services.Interfaces;

public interface IMobileService
{
    Task<List<ContentListItemDto>> GetMyContentsAsync(int memberId, int orgId);
    Task<ContentDetailDto> GetContentDetailAsync(int contentId);
    Task ReportProgressAsync(int memberId, ReportProgressRequest request);
    Task<List<MobileTaskDto>> GetMyTasksAsync(int memberId, bool completed);
    Task CompleteTaskContentAsync(int memberId, CompleteTaskContentRequest request);
    Task<List<MobileExamTestDto>> GetMyExamsAsync(int memberId, int orgId);
    Task<StartExamResponse> StartExamAsync(int testId);
    Task<SubmitExamResponse> SubmitExamAsync(int memberId, SubmitExamRequest request);
    Task<ExamResultDetailDto> GetExamResultAsync(int memberId, int testId);
    Task<PersonalLearningOverviewDto> GetPersonalOverviewAsync(int memberId);
}
