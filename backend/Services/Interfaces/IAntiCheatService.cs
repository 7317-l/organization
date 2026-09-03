using PartySchoolApi.Models.DTOs;

namespace PartySchoolApi.Services.Interfaces;

public interface IAntiCheatService
{
    AntiCheatChallengeDto GenerateChallenge();
    AntiCheatVerifyResponse Verify(AntiCheatVerifyRequest request);
    Task<List<AntiCheatStatsDto>> GetStatsAsync(int? orgId);
    Task<AntiCheatChallengeResponse> GenerateChallengeV2Async(int memberId, int? contentId);
    Task<AntiCheatVerifyResponseV2> VerifyV2Async(int memberId, AntiCheatVerifyRequest request);
    Task<AntiCheatStatsOverviewDto> GetStatsOverviewAsync(int? orgId);
}
