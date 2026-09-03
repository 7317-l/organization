using PartySchoolApi.Models.DTOs;

namespace PartySchoolApi.Services.Interfaces;

public interface IBattleService
{
    Task<CreateBattleResponse> CreateBattleAsync(int challengerId, CreateBattleRequest request);
    Task<List<BattlePendingDto>> GetPendingBattlesAsync(int memberId);
    Task AcceptBattleAsync(int gameId, int memberId);
    Task CancelBattleAsync(int gameId, int memberId);
    Task<BattleCurrentQuestionResponse> GetCurrentQuestionAsync(int gameId, int memberId);
    Task<BattleAnswerResponse> SubmitAnswerAsync(int gameId, int memberId, BattleAnswerRequest request);
    Task<BattleResultResponse> FinishBattleAsync(int gameId, int memberId);
    Task<BattleResultResponse> GetBattleResultAsync(int gameId, int memberId);
}
