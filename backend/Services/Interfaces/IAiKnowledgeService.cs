using PartySchoolApi.Models.DTOs;

namespace PartySchoolApi.Services.Interfaces;

public interface IAiKnowledgeService
{
    Task<AiKnowledgeQueryResponse> QueryAsync(AiKnowledgeQueryRequest request);
}
