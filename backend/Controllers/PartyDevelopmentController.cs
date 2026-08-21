using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartySchoolApi.Helpers;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Controllers;

[ApiController]
[Route("api/v1/party-development")]
[Authorize]
public class PartyDevelopmentController : ControllerBase
{
    private readonly IPartyDevelopmentService _service;
    private readonly ICurrentUserService _currentUser;

    public PartyDevelopmentController(IPartyDevelopmentService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<PagedResponse> GetList([FromQuery] PartyDevelopmentQueryParams query)
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
    public async Task<ApiResponse> Create([FromBody] CreatePartyDevelopmentRequest request)
    {
        return ApiResponse.Success(await _service.CreateAsync(request), "创建成功");
    }

    [HttpPut("{id}/submit")]
    public async Task<ApiResponse> Submit(int id, [FromBody] SubmitPartyDevelopmentRequest request)
    {
        await _service.SubmitAsync(id, request);
        return ApiResponse.Success(null, "提交成功，等待审核");
    }

    [HttpPut("{id}/review")]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> Review(int id, [FromBody] ReviewPartyDevelopmentRequest request)
    {
        await _service.ReviewAsync(id, request);
        return ApiResponse.Success(null, "审核完成");
    }

    [HttpPut("{id}/advance")]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> AdvanceStage(int id)
    {
        await _service.AdvanceStageAsync(id);
        return ApiResponse.Success(null, "阶段已推进");
    }

    [HttpGet("{id}/ai-check")]
    public async Task<ApiResponse> AiCheck(int id)
    {
        return ApiResponse.Success(await _service.AiCheckMaterialsAsync(id));
    }

    [HttpGet("reminders")]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> GetReminders()
    {
        return ApiResponse.Success(await _service.GetRemindersAsync());
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<ApiResponse> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return ApiResponse.Success(null, "删除成功");
    }
}
