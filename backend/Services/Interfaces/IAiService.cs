using PartySchoolApi.Models.DTOs;

namespace PartySchoolApi.Services.Interfaces;

public interface IAiService
{
    Task<AiRecommendationResponse> GetRecommendationsAsync(int memberId, int limit = 5);
    Task<AiQueryResponse> QueryAsync(AiQueryRequest request);
    Task<AiAssessmentResponse> GenerateAssessmentAsync(int memberId);
}
