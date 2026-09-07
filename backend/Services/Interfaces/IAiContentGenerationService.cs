using PartySchoolApi.Models.DTOs;

namespace PartySchoolApi.Services.Interfaces;

public interface IAiContentGenerationService
{
    Task<AiGenerateContentResponse> GenerateAsync(AiGenerateContentRequest request);

    /// <summary>结合指定组织的党员数据生成党建宣讲稿</summary>
    Task<SpeechGenerateResponse> GenerateSpeechWithDataAsync(SpeechGenerateRequest request);
}
