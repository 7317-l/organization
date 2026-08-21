using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;

namespace PartySchoolApi.Services.Interfaces;

public interface IPointService
{
    Task<PagedResponse> GetRecordsAsync(PointRecordQueryParams query);
    Task<List<PointRankingDto>> GetRankingAsync(int? orgId);
    Task AddPointsAsync(int memberId, int points, Models.Common.PointSourceType sourceType, int? sourceId);
}
