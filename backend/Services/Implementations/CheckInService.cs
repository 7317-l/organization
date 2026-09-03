using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PartySchoolApi.Data;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Models.Entities;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Services.Implementations;

public class CheckInService : ICheckInService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPointService _pointService;

    public CheckInService(AppDbContext context, IMapper mapper, IPointService pointService)
    {
        _context = context;
        _mapper = mapper;
        _pointService = pointService;
    }

    public async Task<PagedResponse> GetPagedAsync(CheckInQueryParams query)
    {
        var q = _context.CheckInRecords
            .Include(c => c.PartyMember)
            .AsQueryable();

        if (query.PartyMemberId.HasValue)
            q = q.Where(c => c.PartyMemberId == query.PartyMemberId.Value);
        if (!string.IsNullOrWhiteSpace(query.LocationName))
            q = q.Where(c => c.LocationName.Contains(query.LocationName));

        var total = await q.LongCountAsync();
        var items = await q
            .OrderByDescending(c => c.CheckInTime)
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size)
            .ToListAsync();

        var dtos = items.Select(c => new CheckInRecordDto
        {
            Id = c.Id,
            PartyMemberId = c.PartyMemberId,
            MemberName = c.PartyMember != null ? c.PartyMember.Name : string.Empty,
            LocationName = c.LocationName,
            CheckInTime = c.CheckInTime,
            Note = c.Note,
            AiBackgroundInterpretation = c.AiBackgroundInterpretation,
            SiteId = c.SiteId,
            SiteName = c.SiteId.HasValue ? _context.EducationSites.Find(c.SiteId.Value)?.Name : null,
            HistoricalFacts = new List<string> { "革命历史悠久", "精神内涵丰富", "教育意义深远" },
            PointsEarned = c.PointsEarned
        }).ToList();

        return PagedResponse.Ok(dtos, query.Page, query.Size, total);
    }

    public async Task<CheckInRecordDto> CreateAsync(int memberId, CreateCheckInRequest request)
    {
        var locationName = request.LocationName;
        EducationSite? site = null;

        // 如果有 siteId，从教育基地获取名称和AI解读
        if (request.SiteId.HasValue)
        {
            site = await _context.EducationSites.FindAsync(request.SiteId.Value);
            if (site != null)
            {
                locationName = site.Name;
            }
        }

        var aiInterp = await GetAiBackgroundAsync(locationName);

        var record = new CheckInRecord
        {
            PartyMemberId = memberId,
            LocationName = locationName,
            CheckInTime = DateTime.UtcNow,
            Note = request.Note,
            AiBackgroundInterpretation = aiInterp.Interpretation,
            SiteId = request.SiteId,
            PointsEarned = 5
        };
        _context.CheckInRecords.Add(record);

        await _pointService.AddPointsAsync(memberId, 5, PointSourceType.ActivityCheckIn, record.Id);
        await _context.SaveChangesAsync();

        var member = await _context.PartyMembers.FindAsync(memberId);
        return new CheckInRecordDto
        {
            Id = record.Id,
            PartyMemberId = memberId,
            MemberName = member != null ? member.Name : string.Empty,
            LocationName = record.LocationName,
            CheckInTime = record.CheckInTime,
            Note = record.Note,
            AiBackgroundInterpretation = record.AiBackgroundInterpretation,
            SiteId = record.SiteId,
            SiteName = site?.Name,
            HistoricalFacts = aiInterp.HistoricalFacts,
            PointsEarned = record.PointsEarned
        };
    }

    public Task<AiBackgroundDto> GetAiBackgroundAsync(string locationName)
    {
        var dict = new Dictionary<string, string>
        {
            ["井冈山"] = "井冈山是中国革命的摇篮，1927年毛泽东同志在此创建了第一个农村革命根据地，开辟了农村包围城市、武装夺取政权的革命道路。",
            ["延安"] = "延安是中国革命的圣地，1935年至1948年，中共中央在此领导全国革命，孕育了伟大的延安精神。",
            ["遵义"] = "遵义会议于1935年召开，确立了毛泽东同志在党中央和红军的领导地位，是党的历史上生死攸关的转折点。",
            ["西柏坡"] = "西柏坡是解放战争时期中央工委、中共中央和解放军总部的所在地，党中央在此指挥了三大战役，召开了七届二中全会。"
        };

        var interpretation = dict.ContainsKey(locationName)
            ? dict[locationName]
            : $"{locationName}是重要的红色教育基地，承载着丰富的革命历史和精神内涵。建议结合实地参观，深入了解其历史背景和时代价值。";

        return Task.FromResult(new AiBackgroundDto
        {
            LocationName = locationName,
            Interpretation = interpretation,
            HistoricalFacts = new List<string> { "革命历史悠久", "精神内涵丰富", "教育意义深远" }
        });
    }
}
