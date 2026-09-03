using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartySchoolApi.Helpers;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Controllers;

[ApiController]
[Route("api/v1/notifications")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _service;
    private readonly ICurrentUserService _currentUser;

    public NotificationController(INotificationService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpPost("send")]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> Send([FromBody] SendNotificationRequest request)
    {
        await _service.SendAsync(request);
        return ApiResponse.Success(null, "发送成功");
    }

    [HttpPost("batch-send")]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> BatchSend([FromBody] BatchSendNotificationRequest request)
    {
        await _service.BatchSendAsync(request);
        return ApiResponse.Success(null, "批量发送成功");
    }

    [HttpGet("unread")]
    public async Task<ApiResponse> GetUnread()
    {
        return ApiResponse.Success(await _service.GetUnreadAsync(_currentUser.UserId));
    }

    [HttpGet("all")]
    public async Task<ApiResponse> GetAll()
    {
        return ApiResponse.Success(await _service.GetAllAsync(_currentUser.UserId));
    }

    [HttpPut("{id}/read")]
    public async Task<ApiResponse> MarkAsRead(int id)
    {
        await _service.MarkReadAsync(id, _currentUser.UserId);
        return ApiResponse.Success(null, "已标记已读");
    }

    [HttpPut("read-all")]
    public async Task<ApiResponse> MarkAllAsRead()
    {
        await _service.MarkAllReadAsync(_currentUser.UserId);
        return ApiResponse.Success(null, "全部已读");
    }

    // ========== (14) 精准分层推送 ==========
    [HttpPost("targeted-send")]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> TargetedSend([FromBody] TargetedSendRequest request)
    {
        return ApiResponse.Success(await _service.TargetedSendAsync(request, (int)_currentUser.Role, _currentUser.OrganizationId));
    }
}
