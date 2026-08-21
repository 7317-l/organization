using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartySchoolApi.Helpers;
using PartySchoolApi.Middleware;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Controllers;

/// <summary>
/// 认证与授权控制器
/// </summary>
[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUser;

    public AuthController(IAuthService authService, ICurrentUserService currentUser)
    {
        _authService = authService;
        _currentUser = currentUser;
    }

    /// <summary>登录（手机号+密码）</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ApiResponse> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return ApiResponse.Success(result, "登录成功");
    }

    /// <summary>刷新令牌</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ApiResponse> Refresh([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);
        return ApiResponse.Success(result, "刷新成功");
    }

    /// <summary>修改密码</summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ApiResponse> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        await _authService.ChangePasswordAsync(_currentUser.UserId, request);
        return ApiResponse.Success(null, "密码修改成功，请重新登录");
    }

    /// <summary>获取当前用户信息</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ApiResponse> GetCurrentUser()
    {
        var user = await _authService.GetCurrentUserInfoAsync(_currentUser.UserId);
        return ApiResponse.Success(user);
    }
}
