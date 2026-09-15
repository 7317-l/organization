using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PartySchoolApi.Data;
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

    public async Task MarkAsReadAsync(int memberId, int notificationId)
    {
        var n = await _context.MessageNotifications
            .FirstOrDefaultAsync(x => x.Id == notificationId && x.PartyMemberId == memberId);
        if (n != null)
        {
            n.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(int memberId)
    {
        var unread = await _context.MessageNotifications
            .Where(n => n.PartyMemberId == memberId && !n.IsRead)
            .ToListAsync();
        foreach (var n in unread) n.IsRead = true;
        await _context.SaveChangesAsync();
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
