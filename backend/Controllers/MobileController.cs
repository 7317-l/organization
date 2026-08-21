using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartySchoolApi.Helpers;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Controllers;

/// <summary>
/// 移动端学习接口控制器
/// </summary>
[ApiController]
[Route("api/v1/mobile")]
[Authorize(Roles = "PartyMember,BranchSecretary,SystemAdmin")]
public class MobileController : ControllerBase
{
    private readonly IMobileService _service;
    private readonly ICurrentUserService _currentUser;

    public MobileController(IMobileService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    /// <summary>获取本人可学内容列表</summary>
    [HttpGet("contents")]
    public async Task<ApiResponse> GetMyContents()
    {
        var contents = await _service.GetMyContentsAsync(_currentUser.UserId, _currentUser.OrganizationId);
        return ApiResponse.Success(contents);
    }

    /// <summary>查看内容详情</summary>
    [HttpGet("contents/{contentId}")]
    public async Task<ApiResponse> GetContentDetail(int contentId)
    {
        var content = await _service.GetContentDetailAsync(contentId);
        return ApiResponse.Success(content);
    }

    /// <summary>上报学习进度</summary>
    [HttpPost("progress")]
    public async Task<ApiResponse> ReportProgress([FromBody] ReportProgressRequest request)
    {
        await _service.ReportProgressAsync(_currentUser.UserId, request);
        return ApiResponse.Success(null, "进度已上报");
    }

    /// <summary>获取本人待完成任务</summary>
    [HttpGet("tasks/pending")]
    public async Task<ApiResponse> GetPendingTasks()
    {
        var tasks = await _service.GetMyTasksAsync(_currentUser.UserId, completed: false);
        return ApiResponse.Success(tasks);
    }

    /// <summary>获取本人已完成任务</summary>
    [HttpGet("tasks/completed")]
    public async Task<ApiResponse> GetCompletedTasks()
    {
        var tasks = await _service.GetMyTasksAsync(_currentUser.UserId, completed: true);
        return ApiResponse.Success(tasks);
    }

    /// <summary>任务中内容完成确认</summary>
    [HttpPost("tasks/complete")]
    public async Task<ApiResponse> CompleteTaskContent([FromBody] CompleteTaskContentRequest request)
    {
        await _service.CompleteTaskContentAsync(_currentUser.UserId, request);
        return ApiResponse.Success(null, "已标记完成");
    }

    /// <summary>获取待参加测验列表</summary>
    [HttpGet("exams")]
    public async Task<ApiResponse> GetMyExams()
    {
        var exams = await _service.GetMyExamsAsync(_currentUser.UserId, _currentUser.OrganizationId);
        return ApiResponse.Success(exams);
    }

    /// <summary>开始测验（获取题目不含答案）</summary>
    [HttpGet("exams/{testId}/start")]
    public async Task<ApiResponse> StartExam(int testId)
    {
        var exam = await _service.StartExamAsync(testId);
        return ApiResponse.Success(exam);
    }

    /// <summary>提交测验答案（自动评分）</summary>
    [HttpPost("exams/submit")]
    public async Task<ApiResponse> SubmitExam([FromBody] SubmitExamRequest request)
    {
        var result = await _service.SubmitExamAsync(_currentUser.UserId, request);
        return ApiResponse.Success(result, "提交成功");
    }

    /// <summary>查看测验结果</summary>
    [HttpGet("exams/{testId}/result")]
    public async Task<ApiResponse> GetExamResult(int testId)
    {
        var result = await _service.GetExamResultAsync(_currentUser.UserId, testId);
        return ApiResponse.Success(result);
    }

    /// <summary>个人学习数据概览</summary>
    [HttpGet("overview")]
    public async Task<ApiResponse> GetPersonalOverview()
    {
        var overview = await _service.GetPersonalOverviewAsync(_currentUser.UserId);
        return ApiResponse.Success(overview);
    }
}
