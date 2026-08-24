using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;

namespace PartySchoolApi.Services.Interfaces;

public interface IMobileService
{
    Task<PagedResponse> GetMyContentsAsync(int memberId, int orgId, int page, int size, string? type, string? keyword, string? sort);
    Task<ContentDetailDto> GetContentDetailAsync(int contentId);
    Task ReportProgressAsync(int memberId, ReportProgressRequest request);
    Task<List<MobileTaskDto>> GetMyTasksAsync(int memberId, bool completed);
    Task CompleteTaskContentAsync(int memberId, CompleteTaskContentRequest request);
    Task<PagedResponse> GetMyExamsAsync(int memberId, int orgId, int page, int size, string? status);
    Task<StartExamResponse> StartExamAsync(int testId);
    Task<SubmitExamResponse> SubmitExamAsync(int memberId, SubmitExamRequest request);
    Task<ExamResultDetailDto> GetExamResultAsync(int memberId, int testId);
    Task<PersonalLearningOverviewDto> GetPersonalOverviewAsync(int memberId);
}
