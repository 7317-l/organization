using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Implementations;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Controllers;

[ApiController]
[Route("api/v1/nl2sql")]
[Authorize(Roles = "SystemAdmin,BranchSecretary")]
public class Nl2SqlController : ControllerBase
{
    private readonly INl2SqlService _service;

    public Nl2SqlController(INl2SqlService service)
    {
        _service = service;
    }

    [HttpPost("query")]
    public async Task<ApiResponse> Query([FromBody] Nl2SqlRequest request)
    {
        return ApiResponse.Success(await _service.QueryAsync(request));
    }
}
