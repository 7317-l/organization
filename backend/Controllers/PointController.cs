using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartySchoolApi.Helpers;
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
    private readonly ICurrentUserService _currentUser;

    public PointController(IPointService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet("records")]
    public async Task<PagedResponse> GetRecords([FromQuery] PointRecordQueryParams query)
    {
        if (_currentUser.Role == UserRole.PartyMember)
        {
            query.PartyMemberId = _currentUser.UserId;
        }
        return await _service.GetRecordsAsync(query);
    }

    [HttpGet("ranking")]
    public async Task<ApiResponse> GetRanking([FromQuery] int? orgId = null)
    {
        if (_currentUser.Role == UserRole.BranchSecretary)
        {
            orgId = _currentUser.OrganizationId;
        }
        return ApiResponse.Success(await _service.GetRankingAsync(orgId));
    }

    [HttpGet("my")]
    public Task<PagedResponse> GetMyRecords([FromQuery] PointRecordQueryParams query)
    {
        query.PartyMemberId = _currentUser.UserId;
        return GetRecords(query);
    }
}
