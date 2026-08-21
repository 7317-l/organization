using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Controllers;

/// <summary>
/// 组织架构管理控制器
/// </summary>
[ApiController]
[Route("api/v1/organizations")]
[Authorize]
public class OrganizationsController : ControllerBase
{
    private readonly IOrganizationService _service;

    public OrganizationsController(IOrganizationService service)
    {
        _service = service;
    }

    /// <summary>获取组织树</summary>
    [HttpGet("tree")]
    public async Task<ApiResponse> GetTree()
    {
        var tree = await _service.GetTreeAsync();
        return ApiResponse.Success(tree);
    }

    /// <summary>创建组织</summary>
    [HttpPost]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<ApiResponse> Create([FromBody] CreateOrganizationRequest request)
    {
        var result = await _service.CreateAsync(request);
        return ApiResponse.Success(result, "创建成功");
    }

    /// <summary>更新组织</summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<ApiResponse> Update(int id, [FromBody] UpdateOrganizationRequest request)
    {
        await _service.UpdateAsync(id, request);
        return ApiResponse.Success(null, "更新成功");
    }

    /// <summary>删除组织</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<ApiResponse> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return ApiResponse.Success(null, "删除成功");
    }

    /// <summary>获取组织统计概览</summary>
    [HttpGet("{id}/stats")]
    public async Task<ApiResponse> GetStats(int id)
    {
        var stats = await _service.GetStatsAsync(id);
        return ApiResponse.Success(stats);
    }
}
