using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Controllers;

/// <summary>
/// 试卷管理控制器
/// </summary>
[ApiController]
[Route("api/v1/exam-papers")]
[Authorize(Roles = "SystemAdmin,BranchSecretary")]
public class ExamPapersController : ControllerBase
{
    private readonly IExamService _service;

    public ExamPapersController(IExamService service)
    {
        _service = service;
    }

    /// <summary>获取试卷列表</summary>
    [HttpGet]
    public async Task<ApiResponse> GetList()
    {
        var papers = await _service.GetPapersAsync();
        return ApiResponse.Success(papers);
    }

    /// <summary>获取试卷详情</summary>
    [HttpGet("{id}")]
    public async Task<ApiResponse> GetById(int id)
    {
        var paper = await _service.GetPaperByIdAsync(id);
        return ApiResponse.Success(paper);
    }

    /// <summary>创建试卷</summary>
    [HttpPost]
    public async Task<ApiResponse> Create([FromBody] CreateExamPaperRequest request)
    {
        var result = await _service.CreatePaperAsync(request);
        return ApiResponse.Success(result, "创建成功");
    }

    /// <summary>更新试卷</summary>
    [HttpPut("{id}")]
    public async Task<ApiResponse> Update(int id, [FromBody] UpdateExamPaperRequest request)
    {
        await _service.UpdatePaperAsync(id, request);
        return ApiResponse.Success(null, "更新成功");
    }

    /// <summary>删除试卷</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<ApiResponse> Delete(int id)
    {
        await _service.DeletePaperAsync(id);
        return ApiResponse.Success(null, "删除成功");
    }
}
