using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Controllers;

/// <summary>
/// 学习任务管理控制器
/// </summary>
[ApiController]
[Route("api/v1/tasks")]
[Authorize]
public class LearningTasksController : ControllerBase
{
    private readonly ILearningTaskService _service;

    public LearningTasksController(ILearningTaskService service)
    {
        _service = service;
    }

    /// <summary>分页查询任务列表</summary>
    [HttpGet]
    public async Task<PagedResponse> GetList([FromQuery] TaskQueryParams query)
    {
        return await _service.GetPagedAsync(query);
    }

    /// <summary>获取任务详情</summary>
    [HttpGet("{id}")]
    public async Task<ApiResponse> GetById(int id)
    {
        var task = await _service.GetByIdAsync(id);
        return ApiResponse.Success(task);
    }

    /// <summary>创建任务</summary>
    [HttpPost]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> Create([FromBody] CreateTaskRequest request)
    {
        var result = await _service.CreateAsync(request);
        return ApiResponse.Success(result, "创建成功");
    }

    /// <summary>更新任务</summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> Update(int id, [FromBody] UpdateTaskRequest request)
    {
        await _service.UpdateAsync(id, request);
        return ApiResponse.Success(null, "更新成功");
    }

    /// <summary>删除任务</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<ApiResponse> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return ApiResponse.Success(null, "删除成功");
    }

    /// <summary>查看任务完成详情（每个党员进度）</summary>
    [HttpGet("{id}/completion")]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> GetCompletionDetails(int id)
    {
        var details = await _service.GetCompletionDetailsAsync(id);
        return ApiResponse.Success(details);
    }
}
