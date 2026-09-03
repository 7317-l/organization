using PartySchoolApi.Models.DTOs;

namespace PartySchoolApi.Services.Interfaces;

public interface INl2SqlService
{
    Task<Nl2SqlResponse> QueryAsync(Nl2SqlRequest request, int memberId);
    Task<List<Nl2SqlHistoryItem>> GetHistoryAsync(string sessionId, int memberId, int limit = 5);
}
