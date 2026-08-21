using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Controllers;

/// <summary>
/// 题库管理控制器
/// </summary>
[ApiController]
[Route("api/v1/questions")]
[Authorize(Roles = "SystemAdmin,BranchSecretary")]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionService _service;

    public QuestionsController(IQuestionService service)
    {
        _service = service;
    }

    /// <summary>分页查询题目</summary>
    [HttpGet]
    public async Task<PagedResponse> GetList([FromQuery] QuestionQueryParams query)
    {
        return await _service.GetPagedAsync(query);
    }

    /// <summary>获取题目详情</summary>
    [HttpGet("{id}")]
    public async Task<ApiResponse> GetById(int id)
    {
        var question = await _service.GetByIdAsync(id);
        return ApiResponse.Success(question);
    }

    /// <summary>创建题目</summary>
    [HttpPost]
    public async Task<ApiResponse> Create([FromBody] CreateQuestionRequest request)
    {
        var result = await _service.CreateAsync(request);
        return ApiResponse.Success(result, "创建成功");
    }

    /// <summary>更新题目</summary>
    [HttpPut("{id}")]
    public async Task<ApiResponse> Update(int id, [FromBody] UpdateQuestionRequest request)
    {
        await _service.UpdateAsync(id, request);
        return ApiResponse.Success(null, "更新成功");
    }

    /// <summary>删除题目</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<ApiResponse> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return ApiResponse.Success(null, "删除成功");
    }

    /// <summary>批量导入题目</summary>
    [HttpPost("import")]
    public async Task<ApiResponse> Import(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return ApiResponse.Fail("请上传文件");

        using var stream = file.OpenReadStream();
        var result = await _service.ImportAsync(stream, file.FileName);
        return ApiResponse.Success(result, "导入完成");
    }

    // ===== 分类 =====
    /// <summary>获取题目分类列表</summary>
    [HttpGet("categories")]
    public async Task<ApiResponse> GetCategories()
    {
        var categories = await _service.GetAllCategoriesAsync();
        return ApiResponse.Success(categories);
    }

    /// <summary>创建题目分类</summary>
    [HttpPost("categories")]
    public async Task<ApiResponse> CreateCategory([FromBody] CreateQuestionCategoryRequest request)
    {
        var result = await _service.CreateCategoryAsync(request);
        return ApiResponse.Success(result, "创建成功");
    }

    /// <summary>更新题目分类</summary>
    [HttpPut("categories/{id}")]
    public async Task<ApiResponse> UpdateCategory(int id, [FromBody] CreateQuestionCategoryRequest request)
    {
        await _service.UpdateCategoryAsync(id, request.Name);
        return ApiResponse.Success(null, "更新成功");
    }

    /// <summary>删除题目分类</summary>
    [HttpDelete("categories/{id}")]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<ApiResponse> DeleteCategory(int id)
    {
        await _service.DeleteCategoryAsync(id);
        return ApiResponse.Success(null, "删除成功");
    }
}
