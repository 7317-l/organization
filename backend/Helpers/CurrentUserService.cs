using System.Security.Claims;
using PartySchoolApi.Models.Common;

namespace PartySchoolApi.Helpers;

/// <summary>
/// 当前用户服务，从HttpContext中解析用户信息
/// </summary>
public interface ICurrentUserService
{
    int UserId { get; }
    string UserName { get; }
    UserRole Role { get; }
    int OrganizationId { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin => Role == UserRole.SystemAdmin;
    bool IsBranchSecretary => Role == UserRole.BranchSecretary;
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public int UserId
    {
        get
        {
            var value = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(value, out var id) ? id : 0;
        }
    }

    public string UserName => User?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

    public UserRole Role
    {
        get
        {
            var value = User?.FindFirst(ClaimTypes.Role)?.Value;
            return Enum.TryParse<UserRole>(value, out var role) ? role : UserRole.PartyMember;
        }
    }

    public int OrganizationId
    {
        get
        {
            var value = User?.FindFirst("OrganizationId")?.Value;
            return int.TryParse(value, out var id) ? id : 0;
        }
    }

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}
