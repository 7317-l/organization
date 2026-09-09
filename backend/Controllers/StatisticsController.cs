using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartySchoolApi.Helpers;
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
    private readonly ICurrentUserService _currentUser;
    private readonly IDataPermissionService _dataPermission;

    public StatisticsController(IStatisticsService service, ICurrentUserService currentUser, IDataPermissionService dataPermission)
    {
        _service = service;
        _currentUser = currentUser;
        _dataPermission = dataPermission;
    }

    /// <summary>校验组织访问权限，书记越权返回null表示用默认本支部</summary>
    private async Task<int?> GetScopedOrgIdAsync(int? orgId)
    {
        if (_currentUser.Role == UserRole.SystemAdmin)
            return orgId;
        // 书记：未指定则默认本支部；指定了则校验是否可访问
        if (!orgId.HasValue)
            return _currentUser.OrganizationId;
        if (!await _dataPermission.CanAccessOrgAsync(orgId.Value, (int)_currentUser.Role, _currentUser.OrganizationId))
            return _currentUser.OrganizationId; // 越权则降级为本支部
        return orgId;
    }

    /// <summary>仪表盘总览</summary>
    [HttpGet("dashboard")]
    public async Task<ApiResponse> GetDashboard()
    {
        var data = await _service.GetDashboardOverviewAsync();
        return ApiResponse.Success(data);
    }

    /// <summary>学习统计（按时间范围和支部，数据权限隔离）</summary>
    [HttpGet("learning")]
    public async Task<ApiResponse> GetLearningStats(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int? orgId = null)
    {
        orgId = await GetScopedOrgIdAsync(orgId);
        var data = await _service.GetLearningStatisticsAsync(startDate, endDate, orgId);
        return ApiResponse.Success(data);
    }

    /// <summary>测验统计（数据权限隔离）</summary>
    [HttpGet("exam")]
    public async Task<ApiResponse> GetExamStats(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int? testId = null,
        [FromQuery] int? orgId = null)
    {
        orgId = await GetScopedOrgIdAsync(orgId);
        var data = await _service.GetExamStatisticsAsync(startDate, endDate, testId, orgId);
        return ApiResponse.Success(data);
    }

    /// <summary>单个支部详细统计（数据权限校验）</summary>
    [HttpGet("branch/{orgId}")]
    public async Task<ApiResponse> GetBranchStats(int orgId)
    {
        if (_currentUser.Role == UserRole.BranchSecretary &&
            !await _dataPermission.CanAccessOrgAsync(orgId, (int)_currentUser.Role, _currentUser.OrganizationId))
            return ApiResponse.Fail("无权访问该支部数据");
        var data = await _service.GetBranchStatisticsAsync(orgId);
        return ApiResponse.Success(data);
    }

    /// <summary>全屏数字驾驶舱</summary>
    [HttpGet("dashboard-largescreen")]
    public async Task<IActionResult> GetLargeScreenDashboard()
    {
        var data = await _service.GetLargeScreenDashboardAsync();
        return Ok(ApiResponse.Success(data));
    }

    /// <summary>防挂机统计（数据权限隔离）</summary>
    [HttpGet("anti-cheat")]
    public async Task<IActionResult> GetAntiCheatStats([FromQuery] int? orgId = null)
    {
        orgId = await GetScopedOrgIdAsync(orgId);
        var data = await _service.GetAntiCheatStatsAsync(orgId);
        return Ok(ApiResponse.Success(data));
    }
}
