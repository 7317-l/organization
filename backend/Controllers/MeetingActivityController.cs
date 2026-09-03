using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartySchoolApi.Helpers;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Controllers;

[ApiController]
[Route("api/v1/meeting-activities")]
[Authorize]
public class MeetingActivityController : ControllerBase
{
    private readonly IMeetingActivityService _service;
    private readonly ICurrentUserService _currentUser;

    public MeetingActivityController(IMeetingActivityService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<PagedResponse> GetList([FromQuery] MeetingActivityQueryParams query)
    {
        return await _service.GetPagedAsync(query);
    }

    [HttpGet("{id}")]
    public async Task<ApiResponse> GetById(int id)
    {
        return ApiResponse.Success(await _service.GetByIdAsync(id));
    }

    [HttpPost]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> Create([FromBody] CreateMeetingActivityRequest request)
    {
        return ApiResponse.Success(await _service.CreateAsync(request), "创建成功");
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<ApiResponse> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return ApiResponse.Success(null, "删除成功");
    }

    [HttpPost("hearts")]
    public async Task<ApiResponse> SubmitHeart([FromBody] SubmitActivityHeartRequest request)
    {
        return ApiResponse.Success(
            await _service.SubmitHeartAsync(_currentUser.UserId, request), "提交成功");
    }

    [HttpGet("{id}/hearts")]
    public async Task<ApiResponse> GetHearts(int id)
    {
        return ApiResponse.Success(await _service.GetHeartsByActivityAsync(id));
    }

    [HttpPost("{id}/ai-summary")]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> GenerateAiSummary(int id)
    {
        return ApiResponse.Success(await _service.GenerateAiSummaryAsync(id), "AI总结已生成");
    }

    [HttpPost("brief")]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> GenerateBrief([FromBody] MeetingBriefRequest request)
    {
        return ApiResponse.Success(await _service.GenerateBriefAsync(request, (int)_currentUser.Role, _currentUser.OrganizationId));
    }

    /// <summary>Contract alias: /meeting-activities/ai-brief</summary>
    [HttpPost("ai-brief")]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public Task<ApiResponse> GenerateBriefAlias([FromBody] MeetingBriefRequest request) => GenerateBrief(request);
}