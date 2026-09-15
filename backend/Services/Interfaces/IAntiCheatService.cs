using PartySchoolApi.Models.DTOs;

namespace PartySchoolApi.Services.Interfaces;

public interface IAntiCheatService
{
    AntiCheatChallengeDto GenerateChallenge();
    AntiCheatVerifyResponse Verify(AntiCheatVerifyRequest request);
    Task<List<AntiCheatStatsDto>> GetStatsAsync(int? orgId);
}
