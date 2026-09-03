using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartySchoolApi.Helpers;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Controllers;

[ApiController]
[Route("api/v1/rectifications")]
[Authorize(Roles = "SystemAdmin,BranchSecretary")]
public class RectificationController : ControllerBase
{
    private readonly IRectificationService _service;
    private readonly ICurrentUserService _currentUser;

    public RectificationController(IRectificationService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<PagedResponse> GetList([FromQuery] int organizationId, [FromQuery] string? quarter,
        [FromQuery] int? status, [FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        var (items, total) = await _service.GetRectificationsAsync(organizationId, quarter, status, page, size);
        return PagedResponse.Ok(items, page, size, total);
    }

    [HttpPost]
    public async Task<ApiResponse> Create([FromBody] CreateRectificationRequest request)
    {
        return ApiResponse.Success(await _service.CreateRectificationAsync(_currentUser.OrganizationId, request), "创建成功");
    }

    [HttpPut("{id}/complete")]
    public async Task<ApiResponse> Complete(int id, [FromBody] CompleteRectificationRequest request)
    {
        return ApiResponse.Success(await _service.CompleteRectificationAsync(id, request), "整改已完成");
    }

    [HttpPut("{id}/status")]
    public async Task<ApiResponse> UpdateStatus(int id, [FromBody] UpdateRectificationStatusRequest request)
    {
        return ApiResponse.Success(await _service.UpdateStatusAsync(id, request), "状态已更新");
    }
}
