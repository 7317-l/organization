using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartySchoolApi.Helpers;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Controllers;

[ApiController]
[Route("api/v1/battle")]
[Route("api/v1/battles")]
[Authorize]
public class BattleController : ControllerBase
{
    private readonly IBattleService _service;
    private readonly ICurrentUserService _currentUser;

    public BattleController(IBattleService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpPost("create")]
    public async Task<ApiResponse> Create([FromBody] CreateBattleRequest request)
    {
        return ApiResponse.Success(await _service.CreateBattleAsync(_currentUser.UserId, request));
    }

    [HttpGet("pending")]
    public async Task<ApiResponse> GetPending()
    {
        return ApiResponse.Success(await _service.GetPendingBattlesAsync(_currentUser.UserId));
    }

    [HttpPost("{gameId}/accept")]
    public async Task<ApiResponse> Accept(int gameId)
    {
        await _service.AcceptBattleAsync(gameId, _currentUser.UserId);
        return ApiResponse.Success(null, "已接受对战");
    }

    [HttpPost("{gameId}/cancel")]
    public async Task<ApiResponse> Cancel(int gameId)
    {
        await _service.CancelBattleAsync(gameId, _currentUser.UserId);
        return ApiResponse.Success(null, "已取消对战");
    }

    [HttpGet("{gameId}/question")]
    public async Task<ApiResponse> GetCurrentQuestion(int gameId)
    {
        return ApiResponse.Success(await _service.GetCurrentQuestionAsync(gameId, _currentUser.UserId));
    }

    [HttpPost("{gameId}/answer")]
    public async Task<ApiResponse> SubmitAnswer(int gameId, [FromBody] BattleAnswerRequest request)
    {
        return ApiResponse.Success(await _service.SubmitAnswerAsync(gameId, _currentUser.UserId, request));
    }

    [HttpPost("{gameId}/finish")]
    public async Task<ApiResponse> Finish(int gameId)
    {
        return ApiResponse.Success(await _service.FinishBattleAsync(gameId, _currentUser.UserId));
    }

    [HttpPost("{gameId}/forfeit")]
    public async Task<ApiResponse> Forfeit(int gameId)
    {
        await _service.ForfeitBattleAsync(gameId, _currentUser.UserId);
        return ApiResponse.Success(null, "已弃权退出");
    }

    [HttpGet("{gameId}/result")]
    public async Task<ApiResponse> GetResult(int gameId)
    {
        return ApiResponse.Success(await _service.GetBattleResultAsync(gameId, _currentUser.UserId));
    }
}
