using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PartySchoolApi.Data;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Models.Entities;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Services.Implementations;

public class PointService : IPointService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public PointService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResponse> GetRecordsAsync(PointRecordQueryParams query)
    {
        var q = _context.LearningPoints
            .Include(p => p.PartyMember)
            .AsQueryable();

        if (query.PartyMemberId.HasValue)
            q = q.Where(p => p.PartyMemberId == query.PartyMemberId.Value);
        if (query.SourceType.HasValue)
            q = q.Where(p => p.SourceType == query.SourceType.Value);

        var total = await q.LongCountAsync();
        var items = await q
            .OrderByDescending(p => p.EarnedAt)
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size)
            .ToListAsync();

        var dtos = items.Select(p => new PointRecordDto
        {
            Id = p.Id,
            PartyMemberId = p.PartyMemberId,
            MemberName = p.PartyMember != null ? p.PartyMember.Name : string.Empty,
            SourceType = p.SourceType,
            SourceTypeName = p.SourceType.ToString(),
            SourceId = p.SourceId,
            Points = p.Points,
            EarnedAt = p.EarnedAt
        }).ToList();

        return PagedResponse.Ok(dtos, query.Page, query.Size, total);
    }

    public async Task<List<PointRankingDto>> GetRankingAsync(int? orgId, string period = "all")
    {
        var q = _context.PartyMembers
            .Include(m => m.Organization)
            .Where(m => m.IsEnabled && m.Role != UserRole.SystemAdmin)
            .AsQueryable();

        if (orgId.HasValue)
            q = q.Where(m => m.OrganizationId == orgId.Value);

        var members = await q.ToListAsync();
        var memberIds = members.Select(m => m.Id).ToList();

        DateTime? startTime = null;
        if (period == "week")
            startTime = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
        else if (period == "month")
            startTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        var pointQuery = _context.LearningPoints.Where(p => memberIds.Contains(p.PartyMemberId));
        if (startTime.HasValue)
            pointQuery = pointQuery.Where(p => p.EarnedAt >= startTime.Value);

        var pointSums = await pointQuery
            .GroupBy(p => p.PartyMemberId)
            .Select(g => new { MemberId = g.Key, Points = g.Sum(p => p.Points) })
            .ToListAsync();

        var pointDict = pointSums.ToDictionary(x => x.MemberId, x => x.Points);

        return members
            .Select(m => new PointRankingDto
            {
                MemberId = m.Id,
                MemberName = m.Name,
                OrganizationId = m.OrganizationId,
                OrganizationName = m.Organization != null ? m.Organization.Name : string.Empty,
                TotalPoints = period == "all" ? m.PointTotal : (pointDict.TryGetValue(m.Id, out var pts) ? pts : 0),
                Rank = 0
            })
            .OrderByDescending(r => r.TotalPoints)
            .Select((r, i) => { r.Rank = i + 1; return r; })
            .ToList();
    }

    public async Task AddPointsAsync(int memberId, int points, PointSourceType sourceType, int? sourceId)
    {
        var member = await _context.PartyMembers.FindAsync(memberId);
        if (member == null) return;

        var record = new LearningPoint
        {
            PartyMemberId = memberId,
            SourceType = sourceType,
            SourceId = sourceId,
            Points = points,
            EarnedAt = DateTime.UtcNow
        };
        _context.LearningPoints.Add(record);

        member.PointTotal += points;
        await _context.SaveChangesAsync();
    }
}
