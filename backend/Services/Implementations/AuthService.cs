using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PartySchoolApi.Data;
using PartySchoolApi.Helpers;
using PartySchoolApi.Middleware;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Models.Entities;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly JwtHelper _jwtHelper;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, JwtHelper jwtHelper, IMapper mapper, IConfiguration configuration)
    {
        _context = context;
        _jwtHelper = jwtHelper;
        _mapper = mapper;
        _configuration = configuration;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var member = await _context.PartyMembers
            .Include(m => m.Organization)
            .FirstOrDefaultAsync(m => m.Phone == request.Phone);

        if (member == null || !PasswordHelper.VerifyPassword(request.Password, member.PasswordHash))
            throw new BusinessException("手机号或密码错误", 401);

        if (!member.IsEnabled)
            throw new BusinessException("账号已被禁用，请联系管理员", 403);

        var accessToken = _jwtHelper.GenerateAccessToken(member);
        var refreshToken = JwtHelper.GenerateRefreshToken();

        member.RefreshToken = refreshToken;
        member.RefreshTokenExpiry = DateTime.UtcNow.AddDays(double.Parse(_configuration["Jwt:RefreshTokenExpirationDays"]!));
        await _context.SaveChangesAsync();

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"]!) * 60,
            User = _mapper.Map<UserInfoDto>(member)
        };
    }

    public async Task<LoginResponse> RefreshTokenAsync(string refreshToken)
    {
        var member = await _context.PartyMembers
            .Include(m => m.Organization)
            .FirstOrDefaultAsync(m => m.RefreshToken == refreshToken);

        if (member == null || member.RefreshTokenExpiry < DateTime.UtcNow)
            throw new BusinessException("刷新令牌无效或已过期", 401);

        var newAccessToken = _jwtHelper.GenerateAccessToken(member);
        var newRefreshToken = JwtHelper.GenerateRefreshToken();

        member.RefreshToken = newRefreshToken;
        member.RefreshTokenExpiry = DateTime.UtcNow.AddDays(double.Parse(_configuration["Jwt:RefreshTokenExpirationDays"]!));
        await _context.SaveChangesAsync();

        return new LoginResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"]!) * 60,
            User = _mapper.Map<UserInfoDto>(member)
        };
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var member = await _context.PartyMembers.FindAsync(userId);
        if (member == null)
            throw new BusinessException("用户不存在", 404);

        if (!PasswordHelper.VerifyPassword(request.OldPassword, member.PasswordHash))
            throw new BusinessException("原密码错误", 400);

        member.PasswordHash = PasswordHelper.HashPassword(request.NewPassword);
        member.RefreshToken = null;
        member.RefreshTokenExpiry = null;
        await _context.SaveChangesAsync();
    }

    public async Task<UserInfoDto> GetCurrentUserInfoAsync(int userId)
    {
        var member = await _context.PartyMembers
            .Include(m => m.Organization)
            .FirstOrDefaultAsync(m => m.Id == userId);

        if (member == null)
            throw new BusinessException("用户不存在", 404);

        return _mapper.Map<UserInfoDto>(member);
    }
}
