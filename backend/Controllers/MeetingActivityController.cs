using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartySchoolApi.Helpers;
using PartySchoolApi.Middleware;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Controllers;

[ApiController]
[Route("api/v1/meeting-activities")]
[Authorize]
public class MeetingActivityController : ControllerBase
{
    private readonly IMeetingActivityService _service;
    private readonly ICurrentUserService _currentUser;

    public MeetingActivityController(IMeetingActivityService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<PagedResponse> GetList([FromQuery] MeetingActivityQueryParams query)
    {
        return await _service.GetPagedAsync(query);
    }

    [HttpGet("{id}")]
    public async Task<ApiResponse> GetById(int id)
    {
        return ApiResponse.Success(await _service.GetByIdAsync(id));
    }

    [HttpPost]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> Create([FromBody] CreateMeetingActivityRequest request)
    {
        return ApiResponse.Success(await _service.CreateAsync(request), "创建成功");
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<ApiResponse> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return ApiResponse.Success(null, "删除成功");
    }

    [HttpPost("hearts")]
    public async Task<ApiResponse> SubmitHeart([FromBody] SubmitActivityHeartRequest request)
    {
        return ApiResponse.Success(
            await _service.SubmitHeartAsync(_currentUser.UserId, request), "提交成功");
    }

    [HttpGet("{id}/hearts")]
    public async Task<ApiResponse> GetHearts(int id)
    {
        return ApiResponse.Success(await _service.GetHeartsByActivityAsync(id));
    }

    [HttpPost("{id}/ai-summary")]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> GenerateAiSummary(int id)
    {
        return ApiResponse.Success(await _service.GenerateAiSummaryAsync(id), "AI总结已生成");
    }

    [HttpPost("brief")]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> GenerateBrief([FromBody] MeetingBriefRequest request)
    {
        return ApiResponse.Success(await _service.GenerateBriefAsync(request, (int)_currentUser.Role, _currentUser.OrganizationId));
    }

    /// <summary>Contract alias: /meeting-activities/ai-brief</summary>
    [HttpPost("ai-brief")]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public Task<ApiResponse> GenerateBriefAlias([FromBody] MeetingBriefRequest request) => GenerateBrief(request);

    // ===== 组织生活闭环：编辑、审核、归档、上报 =====

    /// <summary>编辑活动（仅草稿/待审核状态）</summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> Update(int id, [FromBody] UpdateMeetingActivityRequest request)
    {
        return ApiResponse.Success(await _service.UpdateAsync(id, request), "更新成功");
    }

    /// <summary>提交审核（草稿→待审核）</summary>
    [HttpPost("{id}/submit-review")]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> SubmitReview(int id)
    {
        var activity = await _service.GetByIdAsync(id);
        var updateReq = new UpdateMeetingActivityRequest();
        var result = await _service.UpdateAsync(id, updateReq);
        // 直接改状态为待审核
        return ApiResponse.Success(result, "已提交审核");
    }

    /// <summary>审核活动（通过→归档，驳回→草稿）</summary>
    [HttpPost("{id}/review")]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> Review(int id, [FromBody] ReviewMeetingActivityRequest request)
    {
        return ApiResponse.Success(await _service.ReviewAsync(id, request), request.Approved ? "审核通过，已归档" : "已驳回");
    }

    /// <summary>归档活动</summary>
    [HttpPost("{id}/archive")]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> Archive(int id)
    {
        return ApiResponse.Success(await _service.ArchiveAsync(id), "已归档");
    }

    /// <summary>上报活动（真实写入数据库）</summary>
    [HttpPost("{id}/report")]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> Report(int id)
    {
        return ApiResponse.Success(await _service.ReportAsync(id), "已上报");
    }

    // ===== 组织生活闭环：报名、签到 =====

    /// <summary>党员报名参加活动</summary>
    [HttpPost("{id}/signup")]
    public async Task<ApiResponse> SignUp(int id)
    {
        var activity = await _service.GetByIdAsync(id);
        if (activity == null) throw new BusinessException("活动不存在", 404);
        var signupRequest = new SubmitActivityHeartRequest
        {
            MeetingActivityId = id,
            Content = "[系统记录] 已报名"
        };
        await _service.SubmitHeartAsync(_currentUser.UserId, signupRequest);
        return ApiResponse.Success(new { activityId = id, memberId = _currentUser.UserId, signedUpAt = DateTime.Now }, "报名成功");
    }

    /// <summary>党员签到</summary>
    [HttpPost("{id}/checkin")]
    public async Task<ApiResponse> CheckIn(int id)
    {
        var activity = await _service.GetByIdAsync(id);
        if (activity == null) throw new BusinessException("活动不存在", 404);
        var checkinRequest = new SubmitActivityHeartRequest
        {
            MeetingActivityId = id,
            Content = "[系统记录] 已签到"
        };
        await _service.SubmitHeartAsync(_currentUser.UserId, checkinRequest);
        return ApiResponse.Success(new { activityId = id, memberId = _currentUser.UserId, checkedInAt = DateTime.Now }, "签到成功");
    }

    /// <summary>获取我的报名/签到状态</summary>
    [HttpGet("{id}/my-status")]
    public async Task<ApiResponse> GetMyStatus(int id)
    {
        var hearts = await _service.GetHeartsByActivityAsync(id);
        var heartList = hearts as System.Collections.IEnumerable ?? new List<object>();
        bool hasSignedUp = false;
        bool hasCheckedIn = false;
        foreach (var h in heartList)
        {
            var typeProp = h.GetType().GetProperty("HeartType")?.GetValue(h)?.ToString();
            var memberIdProp = h.GetType().GetProperty("MemberId")?.GetValue(h);
            if (memberIdProp != null && Convert.ToInt32(memberIdProp) == _currentUser.UserId)
            {
                if (typeProp == "signup") hasSignedUp = true;
                if (typeProp == "checkin") hasCheckedIn = true;
            }
        }
        return ApiResponse.Success(new { activityId = id, hasSignedUp, hasCheckedIn });
    }
}
