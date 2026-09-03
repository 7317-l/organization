using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Controllers;

[ApiController]
[Route("api/v1/points")]
[Authorize]
public class PointController : ControllerBase
{
    private readonly IPointService _service;

    public PointController(IPointService service)
    {
        _service = service;
    }

    [HttpGet("records")]
    public async Task<PagedResponse> GetRecords([FromQuery] PointRecordQueryParams query)
    {
        return await _service.GetRecordsAsync(query);
    }

    [HttpGet("ranking")]
    public async Task<ApiResponse> GetRanking([FromQuery] int? orgId = null)
    {
        return ApiResponse.Success(await _service.GetRankingAsync(orgId));
    }

    /// <summary>Contract alias: /points/my</summary>
    [HttpGet("my")]
    public Task<PagedResponse> GetMyRecords([FromQuery] PointRecordQueryParams query) => GetRecords(query);
}