using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PartySchoolApi.Data;
using PartySchoolApi.Middleware;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Models.Entities;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Services.Implementations;

public class OrganizationService : IOrganizationService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public OrganizationService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<OrganizationTreeDto>> GetTreeAsync()
    {
        var allOrgs = await _context.Organizations.AsNoTracking().ToListAsync();
        return BuildTree(allOrgs, null);
    }

    /// <summary>按可访问组织ID列表过滤组织树（数据权限用）</summary>
    public async Task<List<OrganizationTreeDto>> GetTreeAsync(List<int> orgIds)
    {
        var allOrgs = await _context.Organizations.AsNoTracking().ToListAsync();
        var filtered = allOrgs.Where(o => orgIds.Contains(o.Id)).ToList();
        var filteredIds = filtered.Select(o => o.Id).ToHashSet();
        var rootOrgs = filtered.Where(o => o.ParentId == null || !filteredIds.Contains(o.ParentId.Value)).ToList();
        return rootOrgs.Select(o => BuildNode(filtered, o)).ToList();
    }

    /// <summary>递归构建组织树（从根节点开始）</summary>
    private List<OrganizationTreeDto> BuildTree(List<Organization> allOrgs, int? parentId)
    {
        return allOrgs
            .Where(o => o.ParentId == parentId)
            .Select(o => BuildNode(allOrgs, o))
            .ToList();
    }

    /// <summary>构建单个节点及其子树</summary>
    private OrganizationTreeDto BuildNode(List<Organization> allOrgs, Organization org)
    {
        return new OrganizationTreeDto
        {
            Id = org.Id,
            Name = org.Name,
            ParentId = org.ParentId,
            CreatedAt = org.CreatedAt,
            Children = BuildTree(allOrgs, org.Id)
        };
    }

    public async Task<OrganizationTreeDto> CreateAsync(CreateOrganizationRequest request)
    {
        if (request.ParentId.HasValue)
        {
            var parent = await _context.Organizations.FindAsync(request.ParentId.Value);
            if (parent == null)
                throw new BusinessException("父级组织不存在", 404);
        }

        var org = new Organization
        {
            Name = request.Name,
            ParentId = request.ParentId,
            CreatedAt = DateTime.Now
        };

        _context.Organizations.Add(org);
        await _context.SaveChangesAsync();
        return _mapper.Map<OrganizationTreeDto>(org);
    }

    public async Task UpdateAsync(int id, UpdateOrganizationRequest request)
    {
        var org = await _context.Organizations.FindAsync(id);
        if (org == null)
            throw new BusinessException("组织不存在", 404);

        org.Name = request.Name;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var org = await _context.Organizations
            .Include(o => o.Children)
            .Include(o => o.Members)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (org == null)
            throw new BusinessException("组织不存在", 404);

        if (org.Children.Any())
            throw new BusinessException("存在子组织，无法删除", 400);

        if (org.Members.Any())
            throw new BusinessException("组织下存在党员，无法删除", 400);

        _context.Organizations.Remove(org);
        await _context.SaveChangesAsync();
    }

    public async Task<OrganizationStatsDto> GetStatsAsync(int orgId)
    {
        var org = await _context.Organizations.FindAsync(orgId);
        if (org == null)
            throw new BusinessException("组织不存在", 404);

        var orgIds = await GetAllChildOrgIdsAsync(orgId);
        orgIds.Add(orgId);

        var memberCount = await _context.PartyMembers.CountAsync(m => orgIds.Contains(m.OrganizationId));
        var subOrgCount = await _context.Organizations.CountAsync(o => o.ParentId == orgId);

        var today = DateTime.Today;
        var todayLearningSeconds = await _context.MemberLearningProgress
            .Where(p => orgIds.Contains(p.Member.OrganizationId) && p.UpdatedAt >= today)
            .SumAsync(p => (int?)p.DurationSeconds) ?? 0;

        var tasks = await _context.LearningTasks
            .Where(t => orgIds.Contains(t.TargetOrgId))
            .Include(t => t.TaskContents)
            .ToListAsync();

        double completionRate = 0;
        if (tasks.Any())
        {
            var totalContents = tasks.Sum(t => t.TaskContents.Count);
            var completedContents = await _context.MemberLearningProgress
                .Where(p => p.TaskId.HasValue && tasks.Select(t => t.Id).Contains(p.TaskId.Value) && p.IsCompleted)
                .CountAsync();
            completionRate = totalContents > 0 ? (double)completedContents / (totalContents * memberCount > 0 ? memberCount : 1) * 100 : 0;
        }

        return new OrganizationStatsDto
        {
            OrgId = orgId,
            OrgName = org.Name,
            MemberCount = memberCount,
            SubOrgCount = subOrgCount,
            OngoingTaskCount = tasks.Count(t => t.Deadline >= DateTime.Now),
            TaskCompletionRate = Math.Round(completionRate, 2),
            TodayLearningMinutes = Math.Round(todayLearningSeconds / 60.0, 2)
        };
    }

    private async Task<List<int>> GetAllChildOrgIdsAsync(int parentId)
    {
        var result = new List<int>();
        var children = await _context.Organizations.Where(o => o.ParentId == parentId).ToListAsync();
        foreach (var child in children)
        {
            result.Add(child.Id);
            result.AddRange(await GetAllChildOrgIdsAsync(child.Id));
        }
        return result;
    }
}
