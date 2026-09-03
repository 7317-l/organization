using PartySchoolApi.Models.DTOs;

namespace PartySchoolApi.Services.Interfaces;

public interface IPairHelpService
{
    Task<PairHelpRecommendResponse> RecommendAsync(int memberId, PairHelpRecommendRequest request);
    Task RequestPairAsync(int receiverId, PairHelpRequestDto request);
    Task AcceptRequestAsync(int requestId, int helperId);
    Task RejectRequestAsync(int requestId, int helperId);
    Task<PairHelpMyResponse> GetMyPairsAsync(int memberId);
    Task CompletePairAsync(int recordId, int memberId, PairHelpCompleteRequest request);
    Task LogHelpAsync(int recordId, int memberId, PairHelpLogRequest request);
}
