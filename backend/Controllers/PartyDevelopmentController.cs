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
        var (items, total) = await _service.GetListAsync(query);
        return PagedResponse.Ok(items, query.Page, query.Size, total);
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
        return ApiResponse.Success(await _service.SubmitAsync(id, request), "提交成功，等待审核");
    }

    [HttpPut("{id}/review")]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> Review(int id, [FromBody] ReviewPartyDevelopmentRequest request)
    {
        return ApiResponse.Success(await _service.ReviewAsync(id, request), "审核完成");
    }

    [HttpPut("{id}/advance")]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> AdvanceStage(int id)
    {
        return ApiResponse.Success(await _service.AdvanceStageAsync(id), "阶段已推进");
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

    // ========== (6) 思想汇报 AI 建议 ==========
    [HttpPost("{id}/report-suggestion")]
    public async Task<ApiResponse> GetReportSuggestion(int id, [FromBody] ReportSuggestionRequest request)
    {
        return ApiResponse.Success(await _service.GetReportSuggestionAsync(id, request, _currentUser.UserId, (int)_currentUser.Role));
    }

    // ========== (7) 发展材料 AI 校验 ==========
    [HttpPost("{id}/material-check")]
    public async Task<ApiResponse> CheckMaterials(int id, [FromBody] MaterialCheckRequest request)
    {
        return ApiResponse.Success(await _service.CheckMaterialsV2Async(id, request, _currentUser.UserId, (int)_currentUser.Role));
    }

    // ========== (8) 到期提醒触发 ==========
    [HttpPost("reminders/trigger")]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> TriggerReminders([FromBody] ReminderTriggerRequest request)
    {
        return ApiResponse.Success(await _service.TriggerRemindersAsync(request, (int)_currentUser.Role, _currentUser.OrganizationId));
    }

    [HttpGet("reminders/list")]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<PagedResponse> GetRemindersList([FromQuery] ReminderQueryParams query)
    {
        var (items, total) = await _service.GetRemindersListAsync(query, (int)_currentUser.Role, _currentUser.OrganizationId);
        return PagedResponse.Ok(items, query.Page, query.Size, total);
    }
}
