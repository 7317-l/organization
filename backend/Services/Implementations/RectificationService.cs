using Microsoft.EntityFrameworkCore;
using PartySchoolApi.Data;
using PartySchoolApi.Middleware;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Models.Entities;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Services.Implementations;

public class RectificationService : IRectificationService
{
    private readonly AppDbContext _db;

    public RectificationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(List<RectificationDto> items, long total)> GetRectificationsAsync(int organizationId, string? quarter, int? status, int page, int size)
    {
        var q = _db.OrgRectifications.Where(r => r.OrganizationId == organizationId);
        if (!string.IsNullOrEmpty(quarter)) q = q.Where(r => r.Quarter == quarter);
        if (status.HasValue) q = q.Where(r => r.Status == status.Value);

        var total = await q.LongCountAsync();
        var items = await q
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(r => new RectificationDto
            {
                Id = r.Id, OrganizationId = r.OrganizationId, Quarter = r.Quarter,
                Issue = r.Issue, Suggestion = r.Suggestion, Status = r.Status,
                StatusName = GetStatusName(r.Status), Remark = r.Remark,
                CreatedAt = r.CreatedAt, CompletedAt = r.CompletedAt
            })
            .ToListAsync();
        return (items, total);
    }

    public async Task<RectificationDto> CreateRectificationAsync(int organizationId, CreateRectificationRequest request)
    {
        var item = new OrgRectification
        {
            OrganizationId = organizationId,
            Quarter = request.Quarter,
            Issue = request.Issue,
            Suggestion = request.Suggestion,
            Status = 0,
            CreatedAt = DateTime.Now
        };
        _db.OrgRectifications.Add(item);
        await _db.SaveChangesAsync();
        return await MapDtoAsync(item);
    }

    public async Task<RectificationDto> CompleteRectificationAsync(int id, CompleteRectificationRequest request)
    {
        var item = await _db.OrgRectifications.FindAsync(id);
        if (item == null) throw new BusinessException("整改项不存在");
        item.Status = 2;
        item.Remark = request.Remark;
        item.CompletedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        return await MapDtoAsync(item);
    }

    public async Task<RectificationDto> UpdateStatusAsync(int id, UpdateRectificationStatusRequest request)
    {
        var item = await _db.OrgRectifications.FindAsync(id);
        if (item == null) throw new BusinessException("整改项不存在");
        item.Status = request.Status;
        item.Remark = request.Remark;
        if (request.Status == 2 && item.CompletedAt == null)
            item.CompletedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        return await MapDtoAsync(item);
    }

    private async Task<RectificationDto> MapDtoAsync(OrgRectification r)
    {
        return new RectificationDto
        {
            Id = r.Id, OrganizationId = r.OrganizationId, Quarter = r.Quarter,
            Issue = r.Issue, Suggestion = r.Suggestion, Status = r.Status,
            StatusName = GetStatusName(r.Status), Remark = r.Remark,
            CreatedAt = r.CreatedAt, CompletedAt = r.CompletedAt
        };
    }

    private static string GetStatusName(int status) => status switch
    {
        0 => "待整改",
        1 => "整改中",
        2 => "已完成",
        3 => "已关闭",
        _ => "未知"
    };
}
