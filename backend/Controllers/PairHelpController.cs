using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartySchoolApi.Helpers;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Controllers;

[ApiController]
[Route("api/v1/pair-help")]
[Authorize]
public class PairHelpController : ControllerBase
{
    private readonly IPairHelpService _service;
    private readonly ICurrentUserService _currentUser;

    public PairHelpController(IPairHelpService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpPost("recommend")]
    public async Task<ApiResponse> Recommend([FromBody] PairHelpRecommendRequest request)
    {
        return ApiResponse.Success(await _service.RecommendAsync(_currentUser.UserId, request));
    }

    [HttpPost("request")]
    public async Task<ApiResponse> RequestPair([FromBody] PairHelpRequestDto request)
    {
        await _service.RequestPairAsync(_currentUser.UserId, request);
        return ApiResponse.Success(null, "结对申请已发送");
    }

    [HttpPost("requests/{requestId}/accept")]
    public async Task<ApiResponse> Accept(int requestId)
    {
        await _service.AcceptRequestAsync(requestId, _currentUser.UserId);
        return ApiResponse.Success(null, "已接受结对申请");
    }

    [HttpPost("requests/{requestId}/reject")]
    public async Task<ApiResponse> Reject(int requestId)
    {
        await _service.RejectRequestAsync(requestId, _currentUser.UserId);
        return ApiResponse.Success(null, "已拒绝结对申请");
    }

    [HttpGet("my")]
    public async Task<ApiResponse> GetMyPairs()
    {
        return ApiResponse.Success(await _service.GetMyPairsAsync(_currentUser.UserId));
    }

    [HttpPost("records/{recordId}/complete")]
    public async Task<ApiResponse> Complete(int recordId, [FromBody] PairHelpCompleteRequest request)
    {
        await _service.CompletePairAsync(recordId, _currentUser.UserId, request);
        return ApiResponse.Success(null, "结对已完成");
    }

    [HttpPost("records/{recordId}/log")]
    public async Task<ApiResponse> LogHelp(int recordId, [FromBody] PairHelpLogRequest request)
    {
        await _service.LogHelpAsync(recordId, _currentUser.UserId, request);
        return ApiResponse.Success(null, "帮扶记录已添加");
    }
}
