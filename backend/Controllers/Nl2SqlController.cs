using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartySchoolApi.Helpers;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Controllers;

[ApiController]
[Route("api/v1/nl2sql")]
[Authorize(Roles = "SystemAdmin,BranchSecretary")]
public class Nl2SqlController : ControllerBase
{
    private readonly INl2SqlService _service;
    private readonly ICurrentUserService _currentUser;

    public Nl2SqlController(INl2SqlService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpPost("query")]
    public async Task<ApiResponse> Query([FromBody] Nl2SqlRequest request)
    {
        return ApiResponse.Success(await _service.QueryAsync(request, _currentUser.UserId));
    }

    [HttpGet("history")]
    public async Task<ApiResponse> GetHistory([FromQuery] string sessionId, [FromQuery] int limit = 5)
    {
        return ApiResponse.Success(await _service.GetHistoryAsync(sessionId, _currentUser.UserId, limit));
    }
}
