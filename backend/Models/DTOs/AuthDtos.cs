using System.ComponentModel.DataAnnotations;

namespace PartySchoolApi.Models.DTOs;

/// <summary>登录请求</summary>
public class LoginRequest
{
    [Required(ErrorMessage = "手机号不能为空")]
    [Phone(ErrorMessage = "手机号格式不正确")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "密码不能为空")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>登录响应</summary>
public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public UserInfoDto User { get; set; } = new();
}

/// <summary>用户信息</summary>
public class UserInfoDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
}

/// <summary>刷新令牌请求</summary>
public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>修改密码请求</summary>
public class ChangePasswordRequest
{
    [Required(ErrorMessage = "原密码不能为空")]
    public string OldPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "新密码不能为空")]
    [MinLength(6, ErrorMessage = "新密码至少6位")]
    public string NewPassword { get; set; } = string.Empty;
}
