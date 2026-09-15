using PartySchoolApi.Models.DTOs;

namespace PartySchoolApi.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<LoginResponse> RefreshTokenAsync(string refreshToken);
    Task ChangePasswordAsync(int userId, ChangePasswordRequest request);
    Task<UserInfoDto> GetCurrentUserInfoAsync(int userId);
}
