using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PartySchoolApi.Data;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Models.Entities;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Services.Implementations;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public NotificationService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task SendAsync(SendNotificationRequest request)
    {
        var notification = new MessageNotification
        {
            PartyMemberId = request.PartyMemberId,
            Type = request.Type,
            Title = request.Title,
            Content = request.Content,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        _context.MessageNotifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    public async Task BatchSendAsync(BatchSendNotificationRequest request)
    {
        var notifications = request.PartyMemberIds.Select(id => new MessageNotification
        {
            PartyMemberId = id,
            Type = request.Type,
            Title = request.Title,
            Content = request.Content,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        _context.MessageNotifications.AddRange(notifications);
        await _context.SaveChangesAsync();
    }

    public async Task<List<NotificationDto>> GetUnreadAsync(int memberId)
    {
        var list = await _context.MessageNotifications
            .Where(n => n.PartyMemberId == memberId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return list.Select(MapToDto).ToList();
    }

    public async Task<List<NotificationDto>> GetAllAsync(int memberId)
    {
        var list = await _context.MessageNotifications
            .Where(n => n.PartyMemberId == memberId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(100)
            .ToListAsync();

        return list.Select(MapToDto).ToList();
    }

    public async Task MarkReadAsync(int id, int memberId)
    {
        var n = await _context.MessageNotifications
            .FirstOrDefaultAsync(x => x.Id == id && x.PartyMemberId == memberId);
        if (n != null)
        {
            n.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkAllReadAsync(int memberId)
    {
        var unread = await _context.MessageNotifications
            .Where(n => n.PartyMemberId == memberId && !n.IsRead)
            .ToListAsync();
        foreach (var n in unread) n.IsRead = true;
        await _context.SaveChangesAsync();
    }

    // ========== (14) 精准分层推送 ==========
    public async Task<TargetedSendResponse> TargetedSendAsync(TargetedSendRequest request, int currentRole, int currentOrgId)
    {
        var filter = request.Filter ?? new TargetedSendFilter();
        var allOrgs = await _context.Organizations.ToListAsync();

        // 确定组织范围
        var orgIds = new List<int>();
        if (filter.OrganizationId.HasValue)
        {
            if (filter.IncludeDescendants)
                orgIds = Services.Common.OrgHierarchyHelper.CollectOrgAndDescendantIds(filter.OrganizationId.Value, allOrgs);
            else
                orgIds.Add(filter.OrganizationId.Value);
        }
        else if (currentRole == 1) // 支部书记默认本组织
        {
            orgIds = Services.Common.OrgHierarchyHelper.CollectOrgAndDescendantIds(currentOrgId, allOrgs);
        }

        // 查询匹配党员
        var query = _context.PartyMembers.AsQueryable();
        if (orgIds.Count > 0)
            query = query.Where(m => orgIds.Contains(m.OrganizationId));
        if (filter.OnlyEnabled)
            query = query.Where(m => m.IsEnabled);
        if (filter.Roles != null && filter.Roles.Count > 0)
            query = query.Where(m => filter.Roles.Contains((int)m.Role));
        if (filter.MemberTypes != null && filter.MemberTypes.Count > 0)
            query = query.Where(m => filter.MemberTypes.Contains(m.MemberType));
        if (filter.ExcludeMemberIds != null && filter.ExcludeMemberIds.Count > 0)
            query = query.Where(m => !filter.ExcludeMemberIds.Contains(m.Id));

        var matchedMembers = await query.Select(m => m.Id).ToListAsync();
        var matchedCount = matchedMembers.Count;

        if (request.DryRun)
        {
            return new TargetedSendResponse
            {
                MatchedCount = matchedCount,
                MatchedMemberIds = matchedMembers,
                SentCount = 0,
                SkippedCount = 0
            };
        }

        // 批量发送
        var notifications = matchedMembers.Select(id => new MessageNotification
        {
            PartyMemberId = id,
            Type = (NotificationType)request.Type,
            Title = request.Title,
            Content = request.Content,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        _context.MessageNotifications.AddRange(notifications);
        await _context.SaveChangesAsync();

        return new TargetedSendResponse
        {
            MatchedCount = matchedCount,
            MatchedMemberIds = matchedMembers,
            SentCount = matchedCount,
            SkippedCount = 0
        };
    }

    private NotificationDto MapToDto(MessageNotification n)
    {
        return new NotificationDto
        {
            Id = n.Id,
            PartyMemberId = n.PartyMemberId,
            Type = n.Type,
            TypeName = n.Type.ToString(),
            Title = n.Title,
            Content = n.Content,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        };
    }
}
