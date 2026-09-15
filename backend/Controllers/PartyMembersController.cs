using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Controllers;

/// <summary>
/// 党员管理控制器
/// </summary>
[ApiController]
[Route("api/v1/members")]
[Authorize(Roles = "SystemAdmin,BranchSecretary")]
public class PartyMembersController : ControllerBase
{
    private readonly IPartyMemberService _service;

    public PartyMembersController(IPartyMemberService service)
    {
        _service = service;
    }

    /// <summary>分页查询党员列表</summary>
    [HttpGet]
    public async Task<PagedResponse> GetList([FromQuery] MemberQueryParams query)
    {
        return await _service.GetPagedAsync(query);
    }

    /// <summary>获取党员详情</summary>
    [HttpGet("{id}")]
    public async Task<ApiResponse> GetById(int id)
    {
        var member = await _service.GetByIdAsync(id);
        return ApiResponse.Success(member);
    }

    /// <summary>新增党员</summary>
    [HttpPost]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<ApiResponse> Create([FromBody] CreateMemberRequest request)
    {
        var result = await _service.CreateAsync(request);
        return ApiResponse.Success(result, "新增成功");
    }

    /// <summary>编辑党员</summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<ApiResponse> Update(int id, [FromBody] UpdateMemberRequest request)
    {
        await _service.UpdateAsync(id, request);
        return ApiResponse.Success(null, "更新成功");
    }

    /// <summary>删除党员</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<ApiResponse> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return ApiResponse.Success(null, "删除成功");
    }

    /// <summary>分配角色</summary>
    [HttpPut("{id}/role")]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<ApiResponse> AssignRole(int id, [FromBody] AssignRoleRequest request)
    {
        await _service.AssignRoleAsync(id, request.Role);
        return ApiResponse.Success(null, "角色分配成功");
    }

    /// <summary>批量导入党员（Excel/CSV）</summary>
    [HttpPost("import")]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<ApiResponse> Import(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return ApiResponse.Fail("请上传文件");

        using var stream = file.OpenReadStream();
        var result = await _service.ImportAsync(stream, file.FileName);
        return ApiResponse.Success(result, "导入完成");
    }

    /// <summary>导出党员列表</summary>
    [HttpGet("export")]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> Export()
    {
        var bytes = await _service.ExportAsync();
        return File(bytes, "text/csv; charset=utf-8", $"党员列表_{DateTime.Now:yyyyMMddHHmmss}.csv");
    }
}
