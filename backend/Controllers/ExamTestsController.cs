using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartySchoolApi.Helpers;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Controllers;

/// <summary>
/// 测验管理控制器
/// </summary>
[ApiController]
[Route("api/v1/exam-tests")]
[Authorize]
public class ExamTestsController : ControllerBase
{
    private readonly IExamService _service;
    private readonly ICurrentUserService _currentUser;

    public ExamTestsController(IExamService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    /// <summary>分页查询测验列表</summary>
    [HttpGet]
    public async Task<PagedResponse> GetList([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] int? orgId = null)
    {
        return await _service.GetTestsAsync(page, size, orgId);
    }

    /// <summary>获取测验详情</summary>
    [HttpGet("{id}")]
    public async Task<ApiResponse> GetById(int id)
    {
        var test = await _service.GetTestByIdAsync(id);
        return ApiResponse.Success(test);
    }

    /// <summary>发布测验</summary>
    [HttpPost]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> Create([FromBody] CreateExamTestRequest request)
    {
        var result = await _service.CreateTestAsync(request, _currentUser.UserId);
        return ApiResponse.Success(result, "发布成功");
    }

    /// <summary>删除测验</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<ApiResponse> Delete(int id)
    {
        await _service.DeleteTestAsync(id);
        return ApiResponse.Success(null, "删除成功");
    }

    /// <summary>查看测验参与结果</summary>
    [HttpGet("{id}/results")]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> GetResults(int id, [FromQuery] int? orgId = null)
    {
        var result = await _service.GetTestResultAsync(id, orgId);
        return ApiResponse.Success(result);
    }
}
