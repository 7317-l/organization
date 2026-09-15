using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Controllers;

[ApiController]
[Route("api/v1/ai-content")]
[Authorize(Roles = "SystemAdmin,BranchSecretary")]
public class AiContentGenerationController : ControllerBase
{
    private readonly IAiContentGenerationService _service;

    public AiContentGenerationController(IAiContentGenerationService service)
    {
        _service = service;
    }

    [HttpPost("generate")]
    public async Task<ApiResponse> Generate([FromBody] AiGenerateContentRequest request)
    {
        return ApiResponse.Success(await _service.GenerateAsync(request), "生成成功");
    }
}
