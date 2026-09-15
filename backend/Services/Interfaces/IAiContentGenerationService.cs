using PartySchoolApi.Models.DTOs;

namespace PartySchoolApi.Services.Interfaces;

public interface IAiContentGenerationService
{
    Task<AiGenerateContentResponse> GenerateAsync(AiGenerateContentRequest request);
}
