using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PartySchoolApi.Data;
using PartySchoolApi.Helpers;
using PartySchoolApi.Middleware;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Models.Entities;
using PartySchoolApi.Services.Interfaces;
using System.Text;

namespace PartySchoolApi.Services.Implementations;

public class PartyMemberService : IPartyMemberService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public PartyMemberService(AppDbContext context, IMapper mapper, ICurrentUserService currentUser)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<PagedResponse> GetPagedAsync(MemberQueryParams query)
    {
        var queryable = _context.PartyMembers
            .Include(m => m.Organization)
            .AsQueryable();

        // 数据权限：支部书记只能看本支部
        if (_currentUser.Role == UserRole.BranchSecretary)
        {
            queryable = queryable.Where(m => m.OrganizationId == _currentUser.OrganizationId);
        }

        if (!string.IsNullOrWhiteSpace(query.Name))
            queryable = queryable.Where(m => m.Name.Contains(query.Name));

        if (query.OrganizationId.HasValue)
            queryable = queryable.Where(m => m.OrganizationId == query.OrganizationId.Value);

        if (query.Role.HasValue)
            queryable = queryable.Where(m => m.Role == query.Role.Value);

        if (query.IsEnabled.HasValue)
            queryable = queryable.Where(m => m.IsEnabled == query.IsEnabled.Value);

        var total = await queryable.LongCountAsync();
        var items = await queryable
            .OrderByDescending(m => m.CreatedAt)
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size)
            .ToListAsync();

        return PagedResponse.Ok(_mapper.Map<List<MemberListItemDto>>(items), query.Page, query.Size, total);
    }

    public async Task<MemberListItemDto> GetByIdAsync(int id)
    {
        var member = await _context.PartyMembers
            .Include(m => m.Organization)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (member == null)
            throw new BusinessException("党员不存在", 404);

        return _mapper.Map<MemberListItemDto>(member);
    }

    public async Task<MemberListItemDto> CreateAsync(CreateMemberRequest request)
    {
        if (await _context.PartyMembers.AnyAsync(m => m.Phone == request.Phone))
            throw new BusinessException("该手机号已注册", 400);

        var org = await _context.Organizations.FindAsync(request.OrganizationId);
        if (org == null)
            throw new BusinessException("所属组织不存在", 404);

        var member = new PartyMember
        {
            Name = request.Name,
            Phone = request.Phone,
            PasswordHash = PasswordHelper.HashPassword(request.Password),
            Role = request.Role,
            OrganizationId = request.OrganizationId,
            IsEnabled = true,
            CreatedAt = DateTime.Now
        };

        _context.PartyMembers.Add(member);
        await _context.SaveChangesAsync();
        return _mapper.Map<MemberListItemDto>(member);
    }

    public async Task UpdateAsync(int id, UpdateMemberRequest request)
    {
        var member = await _context.PartyMembers.FindAsync(id);
        if (member == null)
            throw new BusinessException("党员不存在", 404);

        if (await _context.PartyMembers.AnyAsync(m => m.Phone == request.Phone && m.Id != id))
            throw new BusinessException("该手机号已被使用", 400);

        member.Name = request.Name;
        member.Phone = request.Phone;
        member.Role = request.Role;
        member.OrganizationId = request.OrganizationId;
        member.IsEnabled = request.IsEnabled;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var member = await _context.PartyMembers.FindAsync(id);
        if (member == null)
            throw new BusinessException("党员不存在", 404);

        _context.PartyMembers.Remove(member);
        await _context.SaveChangesAsync();
    }

    public async Task AssignRoleAsync(int id, UserRole role)
    {
        var member = await _context.PartyMembers.FindAsync(id);
        if (member == null)
            throw new BusinessException("党员不存在", 404);

        member.Role = role;
        await _context.SaveChangesAsync();
    }

    public async Task<ImportResultDto> ImportAsync(Stream fileStream, string fileName)
    {
        var result = new ImportResultDto();
        var rows = ExcelHelper.ParseMemberFile(fileStream, fileName);
        result.TotalCount = rows.Count;

        foreach (var row in rows)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(row.Name) || string.IsNullOrWhiteSpace(row.Phone))
                {
                    result.FailCount++;
                    result.Errors.Add($"姓名或手机号为空: {row.Name}-{row.Phone}");
                    continue;
                }

                if (await _context.PartyMembers.AnyAsync(m => m.Phone == row.Phone))
                {
                    result.FailCount++;
                    result.Errors.Add($"手机号已存在: {row.Phone}");
                    continue;
                }

                var org = await _context.Organizations.FindAsync(row.OrganizationId);
                if (org == null)
                {
                    result.FailCount++;
                    result.Errors.Add($"组织不存在: {row.OrganizationId}");
                    continue;
                }

                var member = new PartyMember
                {
                    Name = row.Name,
                    Phone = row.Phone,
                    PasswordHash = PasswordHelper.HashPassword(string.IsNullOrWhiteSpace(row.Password) ? "123456" : row.Password),
                    Role = (UserRole)row.Role,
                    OrganizationId = row.OrganizationId,
                    IsEnabled = true,
                    CreatedAt = DateTime.Now
                };

                _context.PartyMembers.Add(member);
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.FailCount++;
                result.Errors.Add($"{row.Name}-{row.Phone}: {ex.Message}");
            }
        }

        await _context.SaveChangesAsync();
        return result;
    }

    public async Task<byte[]> ExportAsync()
    {
        var members = await _context.PartyMembers
            .Include(m => m.Organization)
            .OrderBy(m => m.OrganizationId)
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("姓名,手机号,角色,组织名称,是否启用,创建时间");

        foreach (var m in members)
        {
            csv.AppendLine($"{m.Name},{m.Phone},{m.Role},{m.Organization?.Name ?? ""},{(m.IsEnabled ? "启用" : "禁用")},{m.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }
}
