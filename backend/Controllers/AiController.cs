using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartySchoolApi.Helpers;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Controllers;

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

    [HttpGet("mobile/recommendations")]
    public async Task<ApiResponse> GetRecommendations([FromQuery] int limit = 5)
    {
        var result = await _aiService.GetRecommendationsAsync(_currentUser.UserId, limit);
        return ApiResponse.Success(result);
    }

    [HttpPost("ai/query")]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> Query([FromBody] AiQueryRequest request)
    {
        var result = await _aiService.QueryAsync(request);
        return ApiResponse.Success(result);
    }

    [HttpPost("mobile/report/ai-assessment")]
    public async Task<ApiResponse> GenerateAssessment([FromBody] AiAssessmentRequest? request = null)
    {
        var memberId = request?.MemberId ?? _currentUser.UserId;
        var result = await _aiService.GenerateAssessmentAsync(memberId);
        return ApiResponse.Success(result);
    }

    [HttpPost("ai/organization-report")]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> GenerateOrganizationReport([FromBody] OrganizationReportRequest request)
    {
        var result = await _aiService.GenerateOrganizationReportAsync(request.OrganizationId, request.Quarter);
        return ApiResponse.Success(result);
    }

    // ========== (4) AI 评选学习标兵 ==========
    [HttpPost("ai/star-members")]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> GenerateStarMembers([FromBody] StarMemberRequest request)
    {
        var result = await _aiService.GenerateStarMembersAsync(request, _currentUser.UserId, (int)_currentUser.Role, _currentUser.OrganizationId);
        return ApiResponse.Success(result);
    }

    // ========== (13) AI 分阶段学习路线图 ==========
    [HttpPost("ai/roadmap")]
    public async Task<ApiResponse> GenerateRoadmap([FromBody] LearningRoadmapRequest request)
    {
        var result = await _aiService.GenerateLearningRoadmapAsync(request, _currentUser.UserId, (int)_currentUser.Role);
        return ApiResponse.Success(result);
    }

    /// <summary>契约别名：/ai/learning-roadmap</summary>
    [HttpPost("ai/learning-roadmap")]
    public Task<ApiResponse> GenerateRoadmapAlias([FromBody] LearningRoadmapRequest request) => GenerateRoadmap(request);

    // ========== (12) AI 学习预警 ==========
    [HttpGet("ai/learning-warnings")]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> GetLearningWarnings([FromQuery] int? organizationId = null)
    {
        var result = await _aiService.GetLearningWarningsAsync(organizationId, _currentUser.UserId, (int)_currentUser.Role, _currentUser.OrganizationId);
        return ApiResponse.Success(result);
    }

    [HttpPost("ai/learning-warnings/trigger")]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> TriggerLearningWarnings([FromQuery] int? organizationId = null)
    {
        var result = await _aiService.TriggerLearningWarningsAsync(organizationId, _currentUser.UserId, (int)_currentUser.Role, _currentUser.OrganizationId);
        return ApiResponse.Success(result);
    }
}
