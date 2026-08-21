using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartySchoolApi.Helpers;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Controllers;

[ApiController]
[Route("api/v1/check-in")]
[Authorize]
public class CheckInController : ControllerBase
{
    private readonly ICheckInService _service;
    private readonly ICurrentUserService _currentUser;

    public CheckInController(ICheckInService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<PagedResponse> GetList([FromQuery] CheckInQueryParams query)
    {
        return await _service.GetPagedAsync(query);
    }

    [HttpGet("my")]
    public async Task<PagedResponse> GetMyRecords([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        return await _service.GetPagedAsync(new CheckInQueryParams
        {
            Page = page,
            Size = size,
            PartyMemberId = _currentUser.UserId
        });
    }

    [HttpPost]
    public async Task<ApiResponse> CheckIn([FromBody] CreateCheckInRequest request)
    {
        return ApiResponse.Success(
            await _service.CreateAsync(_currentUser.UserId, request), "打卡成功，获得5积分");
    }

    [HttpGet("ai-background")]
    public async Task<ApiResponse> GetAiBackground([FromQuery] string locationName)
    {
        return ApiResponse.Success(await _service.GetAiBackgroundAsync(locationName));
    }
}
