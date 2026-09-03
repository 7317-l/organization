using Microsoft.EntityFrameworkCore;
using PartySchoolApi.Data;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Services.Implementations;

public class DataPermissionService : IDataPermissionService
{
    private readonly AppDbContext _context;

    public DataPermissionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<int>> GetAccessibleOrgIdsAsync(int currentRole, int currentOrgId, int? targetOrgId = null)
    {
        var allOrgs = await _context.Organizations.AsNoTracking().ToListAsync();

        // SystemAdmin: 全部组织，或指定组织及下级
        if (currentRole == 2)
        {
            if (targetOrgId.HasValue)
                return Services.Common.OrgHierarchyHelper.CollectOrgAndDescendantIds(targetOrgId.Value, allOrgs);
            return allOrgs.Select(o => o.Id).ToList();
        }

        // BranchSecretary: 本支部及下级
        if (currentRole == 1)
        {
            return Services.Common.OrgHierarchyHelper.CollectOrgAndDescendantIds(currentOrgId, allOrgs);
        }

        // PartyMember: 仅本人所在组织
        return new List<int> { currentOrgId };
    }

    public async Task<List<int>> GetAccessibleMemberIdsAsync(int currentRole, int currentOrgId, int currentMemberId, int? targetOrgId = null)
    {
        // PartyMember: 仅本人
        if (currentRole == 0)
            return new List<int> { currentMemberId };

        var orgIds = await GetAccessibleOrgIdsAsync(currentRole, currentOrgId, targetOrgId);
        return await _context.PartyMembers.AsNoTracking()
            .Where(m => orgIds.Contains(m.OrganizationId))
            .Select(m => m.Id)
            .ToListAsync();
    }

    public async Task<bool> CanAccessMemberAsync(int memberId, int currentRole, int currentOrgId, int currentMemberId)
    {
        if (currentRole == 0)
            return memberId == currentMemberId;

        var accessibleIds = await GetAccessibleMemberIdsAsync(currentRole, currentOrgId, currentMemberId);
        return accessibleIds.Contains(memberId);
    }

    public async Task<bool> CanAccessOrgAsync(int orgId, int currentRole, int currentOrgId)
    {
        if (currentRole == 2)
            return true;

        var orgIds = await GetAccessibleOrgIdsAsync(currentRole, currentOrgId);
        return orgIds.Contains(orgId);
    }
}
