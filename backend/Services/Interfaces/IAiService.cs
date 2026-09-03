using PartySchoolApi.Models.DTOs;

namespace PartySchoolApi.Services.Interfaces;

public interface IAiService
{
    Task<AiRecommendationResponse> GetRecommendationsAsync(int memberId, int limit = 5);
    Task<AiQueryResponse> QueryAsync(AiQueryRequest request);
    Task<AiAssessmentResponse> GenerateAssessmentAsync(int memberId);
    Task<OrganizationReportResponse> GenerateOrganizationReportAsync(int organizationId, string? quarter);
    Task<StarMemberResponse> GenerateStarMembersAsync(StarMemberRequest request, int currentMemberId, int currentRole, int currentOrgId);
    Task<LearningRoadmapResponse> GenerateLearningRoadmapAsync(LearningRoadmapRequest request, int currentMemberId, int currentRole);
    Task<LearningWarningResponse> GetLearningWarningsAsync(int? organizationId, int currentMemberId, int currentRole, int currentOrgId);
    Task<LearningWarningTriggerResponse> TriggerLearningWarningsAsync(int? organizationId, int currentMemberId, int currentRole, int currentOrgId);
}
