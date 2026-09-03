using Microsoft.EntityFrameworkCore;
using PartySchoolApi.Data;
using PartySchoolApi.Middleware;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Models.Entities;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Services.Implementations;

public class EducationSiteService : IEducationSiteService
{
    private readonly AppDbContext _db;

    public EducationSiteService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(List<EducationSiteDto> items, long total)> GetSitesAsync(EducationSiteQueryParams query)
    {
        var q = _db.EducationSites.AsQueryable();
        if (!string.IsNullOrEmpty(query.Keyword))
            q = q.Where(s => s.Name.Contains(query.Keyword) || (s.Address != null && s.Address.Contains(query.Keyword)));

        var total = await q.LongCountAsync();
        var items = await q
            .OrderByDescending(s => s.CreatedAt)
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size)
            .Select(s => new EducationSiteDto
            {
                Id = s.Id, Name = s.Name, Address = s.Address, Description = s.Description,
                HistoricalFacts = s.HistoricalFacts, AiInterpretation = s.AiInterpretation,
                CoverUrl = s.CoverUrl, Latitude = s.Latitude, Longitude = s.Longitude, CreatedAt = s.CreatedAt
            })
            .ToListAsync();
        return (items, total);
    }

    public async Task<EducationSiteDto?> GetSiteAsync(int id)
    {
        var s = await _db.EducationSites.FindAsync(id);
        if (s == null) return null;
        return new EducationSiteDto
        {
            Id = s.Id, Name = s.Name, Address = s.Address, Description = s.Description,
            HistoricalFacts = s.HistoricalFacts, AiInterpretation = s.AiInterpretation,
            CoverUrl = s.CoverUrl, Latitude = s.Latitude, Longitude = s.Longitude, CreatedAt = s.CreatedAt
        };
    }

    public async Task<EducationSiteDto> CreateSiteAsync(EducationSite site)
    {
        site.CreatedAt = DateTime.Now;
        _db.EducationSites.Add(site);
        await _db.SaveChangesAsync();
        return await GetSiteAsync(site.Id) ?? throw new BusinessException("创建失败");
    }

    public async Task<EducationSiteDto> UpdateSiteAsync(int id, EducationSite site)
    {
        var existing = await _db.EducationSites.FindAsync(id);
        if (existing == null) throw new BusinessException("站点不存在");
        existing.Name = site.Name;
        existing.Address = site.Address;
        existing.Description = site.Description;
        existing.HistoricalFacts = site.HistoricalFacts;
        existing.AiInterpretation = site.AiInterpretation;
        existing.CoverUrl = site.CoverUrl;
        existing.Latitude = site.Latitude;
        existing.Longitude = site.Longitude;
        await _db.SaveChangesAsync();
        return await GetSiteAsync(id) ?? throw new BusinessException("更新失败");
    }

    public async Task DeleteSiteAsync(int id)
    {
        var site = await _db.EducationSites.FindAsync(id);
        if (site == null) throw new BusinessException("站点不存在");
        _db.EducationSites.Remove(site);
        await _db.SaveChangesAsync();
    }

    public async Task<(List<EducationSiteCheckinDto> items, long total)> GetSiteCheckinsAsync(int siteId, int page, int size)
    {
        var q = _db.CheckInRecords.Where(c => c.SiteId == siteId);
        var total = await q.LongCountAsync();
        var items = await q
            .OrderByDescending(c => c.CheckInTime)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(c => new EducationSiteCheckinDto
            {
                Id = c.Id, PartyMemberId = c.PartyMemberId,
                MemberName = c.PartyMember != null ? c.PartyMember.Name : "",
                LocationName = c.LocationName, CheckInTime = c.CheckInTime,
                Note = c.Note, AiBackgroundInterpretation = c.AiBackgroundInterpretation
            })
            .ToListAsync();
        return (items, total);
    }
}
