using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Controllers;

/// <summary>
/// 学习内容管理控制器
/// </summary>
[ApiController]
[Route("api/v1/contents")]
[Authorize]
public class LearningContentsController : ControllerBase
{
    private readonly ILearningContentService _service;

    public LearningContentsController(ILearningContentService service)
    {
        _service = service;
    }

    /// <summary>分页查询内容列表</summary>
    [HttpGet]
    public async Task<PagedResponse> GetList([FromQuery] ContentQueryParams query)
    {
        return await _service.GetPagedAsync(query);
    }

    /// <summary>获取内容详情</summary>
    [HttpGet("{id}")]
    public async Task<ApiResponse> GetById(int id)
    {
        var content = await _service.GetByIdAsync(id);
        return ApiResponse.Success(content);
    }

    /// <summary>创建内容</summary>
    [HttpPost]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> Create([FromBody] CreateContentRequest request)
    {
        var result = await _service.CreateAsync(request);
        return ApiResponse.Success(result, "创建成功");
    }

    /// <summary>更新内容</summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> Update(int id, [FromBody] UpdateContentRequest request)
    {
        await _service.UpdateAsync(id, request);
        return ApiResponse.Success(null, "更新成功");
    }

    /// <summary>删除内容</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<ApiResponse> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return ApiResponse.Success(null, "删除成功");
    }

    // ===== 分类 =====
    /// <summary>获取内容分类树</summary>
    [HttpGet("categories/tree")]
    public async Task<ApiResponse> GetCategoryTree()
    {
        var tree = await _service.GetCategoryTreeAsync();
        return ApiResponse.Success(tree);
    }

    /// <summary>创建分类</summary>
    [HttpPost("categories")]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<ApiResponse> CreateCategory([FromBody] CreateContentCategoryRequest request)
    {
        var result = await _service.CreateCategoryAsync(request);
        return ApiResponse.Success(result, "创建成功");
    }

    /// <summary>更新分类</summary>
    [HttpPut("categories/{id}")]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<ApiResponse> UpdateCategory(int id, [FromBody] CreateContentCategoryRequest request)
    {
        await _service.UpdateCategoryAsync(id, request.Name);
        return ApiResponse.Success(null, "更新成功");
    }

    /// <summary>删除分类</summary>
    [HttpDelete("categories/{id}")]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<ApiResponse> DeleteCategory(int id)
    {
        await _service.DeleteCategoryAsync(id);
        return ApiResponse.Success(null, "删除成功");
    }

    // ===== 标签 =====
    /// <summary>获取所有标签</summary>
    [HttpGet("tags")]
    public async Task<ApiResponse> GetTags()
    {
        var tags = await _service.GetAllTagsAsync();
        return ApiResponse.Success(tags);
    }

    /// <summary>创建标签</summary>
    [HttpPost("tags")]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<ApiResponse> CreateTag([FromBody] CreateTagRequest request)
    {
        var result = await _service.CreateTagAsync(request);
        return ApiResponse.Success(result, "创建成功");
    }

    /// <summary>删除标签</summary>
    [HttpDelete("tags/{id}")]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<ApiResponse> DeleteTag(int id)
    {
        await _service.DeleteTagAsync(id);
        return ApiResponse.Success(null, "删除成功");
    }
}
