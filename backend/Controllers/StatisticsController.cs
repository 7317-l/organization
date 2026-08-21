using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Implementations;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Controllers;

/// <summary>
/// 数据统计控制器（管理后台）
/// </summary>
[ApiController]
[Route("api/v1/statistics")]
[Authorize(Roles = "SystemAdmin,BranchSecretary")]
public class StatisticsController : ControllerBase
{
    private readonly IStatisticsService _service;

    public StatisticsController(IStatisticsService service)
    {
        _service = service;
    }

    /// <summary>仪表盘总览</summary>
    [HttpGet("dashboard")]
    public async Task<ApiResponse> GetDashboard()
    {
        var data = await _service.GetDashboardOverviewAsync();
        return ApiResponse.Success(data);
    }

    /// <summary>学习统计（按时间范围和支部）</summary>
    [HttpGet("learning")]
    public async Task<ApiResponse> GetLearningStats(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int? orgId = null)
    {
        var data = await _service.GetLearningStatisticsAsync(startDate, endDate, orgId);
        return ApiResponse.Success(data);
    }

    /// <summary>测验统计（按时间范围、测验或支部）</summary>
    [HttpGet("exam")]
    public async Task<ApiResponse> GetExamStats(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int? testId = null,
        [FromQuery] int? orgId = null)
    {
        var data = await _service.GetExamStatisticsAsync(startDate, endDate, testId, orgId);
        return ApiResponse.Success(data);
    }

    /// <summary>单个支部详细统计</summary>
    [HttpGet("branch/{orgId}")]
    public async Task<ApiResponse> GetBranchStats(int orgId)
    {
        var data = await _service.GetBranchStatisticsAsync(orgId);
        return ApiResponse.Success(data);
    }
    [HttpGet("dashboard-largescreen")]
    public async Task<IActionResult> GetLargeScreenDashboard()
    {
        var data = await _service.GetLargeScreenDashboardAsync();
        return Ok(ApiResponse.Success(data));
    }

    [HttpGet("anti-cheat")]
    public async Task<IActionResult> GetAntiCheatStats([FromQuery] int? orgId = null)
    {
        var data = await _service.GetAntiCheatStatsAsync(orgId);
        return Ok(ApiResponse.Success(data));
    }
    
}
