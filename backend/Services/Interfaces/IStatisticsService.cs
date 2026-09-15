using PartySchoolApi.Models.DTOs;

namespace PartySchoolApi.Services.Interfaces;

public interface IStatisticsService
{
    Task<DashboardOverviewDto> GetDashboardOverviewAsync();
    Task<LearningStatisticsDto> GetLearningStatisticsAsync(DateTime startDate, DateTime endDate, int? orgId);
    Task<ExamStatisticsDto> GetExamStatisticsAsync(DateTime startDate, DateTime endDate, int? testId, int? orgId);
    Task<BranchStatisticsDto> GetBranchStatisticsAsync(int orgId);

    // ===== 新增 =====
    Task<LargeScreenDashboardDto> GetLargeScreenDashboardAsync();
    Task<List<AntiCheatStatsDto>> GetAntiCheatStatsAsync(int? orgId);
}
