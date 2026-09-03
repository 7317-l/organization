using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartySchoolApi.Helpers;
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

    public EducationSiteController(IEducationSiteService service)
    {
        _service = service;
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
}
