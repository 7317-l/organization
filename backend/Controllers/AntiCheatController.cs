using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartySchoolApi.Helpers;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Controllers;

[ApiController]
[Route("api/v1/anti-cheat")]
[Authorize]
public class AntiCheatController : ControllerBase
{
    private readonly IAntiCheatService _service;
    private readonly ICurrentUserService _currentUser;

    public AntiCheatController(IAntiCheatService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet("challenge")]
    public ApiResponse GetChallenge()
    {
        return ApiResponse.Success(_service.GenerateChallenge());
    }

    [HttpPost("verify")]
    public ApiResponse Verify([FromBody] AntiCheatVerifyRequest request)
    {
        return ApiResponse.Success(_service.Verify(request));
    }

    [HttpGet("stats")]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> GetStats([FromQuery] int? orgId = null)
    {
        return ApiResponse.Success(await _service.GetStatsAsync(orgId));
    }

    // ========== (15) 真实题库抽题防挂机 ==========
    [HttpGet("challenge-v2")]
    public async Task<ApiResponse> GetChallengeV2([FromQuery] int? contentId = null)
    {
        return ApiResponse.Success(await _service.GenerateChallengeV2Async(_currentUser.UserId, contentId));
    }

    [HttpPost("verify-v2")]
    public async Task<ApiResponse> VerifyV2([FromBody] AntiCheatVerifyRequest request)
    {
        return ApiResponse.Success(await _service.VerifyV2Async(_currentUser.UserId, request));
    }

    [HttpGet("stats-overview")]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> GetStatsOverview([FromQuery] int? orgId = null)
    {
        return ApiResponse.Success(await _service.GetStatsOverviewAsync(orgId));
    }
}
