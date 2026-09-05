using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PartySchoolApi.Data;
using PartySchoolApi.Helpers;
using PartySchoolApi.Middleware;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Models.Entities;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Controllers;

[ApiController]
[Route("api/v1/education-sites")]
[Authorize]
public class EducationSiteController : ControllerBase
{
    private readonly IEducationSiteService _service;
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public EducationSiteController(IEducationSiteService service, AppDbContext context, ICurrentUserService currentUser)
    {
        _service = service;
        _context = context;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<PagedResponse> GetList([FromQuery] EducationSiteQueryParams query)
    {
        var (items, total) = await _service.GetSitesAsync(query);
        return PagedResponse.Ok(items, query.Page, query.Size, total);
    }

    [HttpGet("{id}")]
    public async Task<ApiResponse> GetById(int id)
    {
        var site = await _service.GetSiteAsync(id);
        if (site == null) return ApiResponse.NotFound("站点不存在");
        return ApiResponse.Success(site);
    }

    [HttpPost]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> Create([FromBody] EducationSite site)
    {
        return ApiResponse.Success(await _service.CreateSiteAsync(site), "创建成功");
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SystemAdmin,BranchSecretary")]
    public async Task<ApiResponse> Update(int id, [FromBody] EducationSite site)
    {
        return ApiResponse.Success(await _service.UpdateSiteAsync(id, site), "更新成功");
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<ApiResponse> Delete(int id)
    {
        await _service.DeleteSiteAsync(id);
        return ApiResponse.Success(null, "删除成功");
    }

    [HttpGet("{id}/checkins")]
    public async Task<PagedResponse> GetCheckins(int id, [FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        var (items, total) = await _service.GetSiteCheckinsAsync(id, page, size);
        return PagedResponse.Ok(items, page, size, total);
    }

    /// <summary>红色教育基地打卡</summary>
    [HttpPost("{id}/checkin")]
    public async Task<ApiResponse> CheckIn(int id, [FromBody] EducationSiteCheckinRequest request)
    {
        var site = await _context.EducationSites.FindAsync(id);
        if (site == null) throw new BusinessException("基地不存在", 404);

        var record = new CheckInRecord
        {
            PartyMemberId = _currentUser.UserId,
            SiteId = id,
            LocationName = site.Name,
            CheckInTime = DateTime.Now,
            PointsEarned = 5
        };
        _context.CheckInRecords.Add(record);

        // 记录感悟（如果有）
        if (!string.IsNullOrWhiteSpace(request.Content))
        {
            // 感悟存入签到记录备注或单独表，这里简化处理
        }

        // 增加积分
        var member = await _context.PartyMembers.FindAsync(_currentUser.UserId);
        if (member != null)
        {
            member.PointTotal += 5;
        }

        await _context.SaveChangesAsync();
        return ApiResponse.Success(new { checkinId = record.Id, siteId = id, pointsEarned = 5, checkedInAt = DateTime.Now }, "打卡成功，获得5积分");
    }

    /// <summary>获取我的打卡历史</summary>
    [HttpGet("my-checkins")]
    public async Task<ApiResponse> GetMyCheckins([FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        var query = _context.CheckInRecords
            .Where(r => r.PartyMemberId == _currentUser.UserId)
            .OrderByDescending(r => r.CheckInTime);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * size)
            .Take(size)
            .Select(r => new
            {
                id = r.Id,
                siteId = r.SiteId,
                locationName = r.LocationName,
                checkInTime = r.CheckInTime,
                pointsEarned = r.PointsEarned
            })
            .ToListAsync();

        return ApiResponse.Success(new { items, total, page, size });
    }
}

public class EducationSiteCheckinRequest
{
    public string? Content { get; set; }
    public string? PhotoUrl { get; set; }
}
