using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartySchoolApi.Helpers;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Controllers;

/// <summary>
/// AI功能控制器（当前为模拟/规则实现，预留大模型对接）
/// </summary>
[ApiController]
[Route("api/v1")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IAiService _aiService;
    private readonly ICurrentUserService _currentUser;

    public AiController(IAiService aiService, ICurrentUserService currentUser)
    {
        _aiService = aiService;
        _currentUser = currentUser;
    }

    /// <summary>个性化学习推荐</summary>
    [HttpGet("mobile/recommendations")]
    public async Task<ApiResponse> GetRecommendations([FromQuery] int limit = 5)
    {
        var result = await _aiService.GetRecommendationsAsync(_currentUser.UserId, limit);
        return ApiResponse.Success(result);
    }

    /// <summary>自然语言组织数据查询</summary>
    [HttpPost("ai/query")]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> Query([FromBody] AiQueryRequest request)
    {
        var result = await _aiService.QueryAsync(request);
        return ApiResponse.Success(result);
    }

    /// <summary>生成党员个人AI评价报告</summary>
    [HttpPost("mobile/report/ai-assessment")]
    public async Task<ApiResponse> GenerateAssessment([FromBody] AiAssessmentRequest? request = null)
    {
        var memberId = request?.MemberId ?? _currentUser.UserId;
        var result = await _aiService.GenerateAssessmentAsync(memberId);
        return ApiResponse.Success(result);
    }

    /// <summary>生成组织（含下级）季度考核报告</summary>
    [HttpPost("ai/organization-report")]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> GenerateOrganizationReport([FromBody] OrganizationReportRequest request)
    {
        var result = await _aiService.GenerateOrganizationReportAsync(request.OrganizationId, request.Quarter);
        return ApiResponse.Success(result);
    }
}
