using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public AntiCheatController(IAntiCheatService service)
    {
        _service = service;
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
}
