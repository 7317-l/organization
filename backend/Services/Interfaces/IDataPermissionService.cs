namespace PartySchoolApi.Services.Interfaces;

/// <summary>
/// 数据权限分级服务：普通党员仅本人；书记仅本支部及下级；管理员按授权组织范围。
/// 在查询服务层做数据级过滤，不仅是页面授权。
/// </summary>
public interface IDataPermissionService
{
    /// <summary>获取当前用户可访问的组织ID列表（含下级）</summary>
    Task<List<int>> GetAccessibleOrgIdsAsync(int currentRole, int currentOrgId, int? targetOrgId = null);

    /// <summary>获取当前用户可访问的党员ID列表</summary>
    Task<List<int>> GetAccessibleMemberIdsAsync(int currentRole, int currentOrgId, int currentMemberId, int? targetOrgId = null);

    /// <summary>判断当前用户是否可访问指定党员数据</summary>
    Task<bool> CanAccessMemberAsync(int memberId, int currentRole, int currentOrgId, int currentMemberId);

    /// <summary>判断当前用户是否可访问指定组织数据</summary>
    Task<bool> CanAccessOrgAsync(int orgId, int currentRole, int currentOrgId);
}
