using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Controllers;

[ApiController]
[Route("api/v1/ai-knowledge")]
[Authorize]
public class AiKnowledgeController : ControllerBase
{
    private readonly IAiKnowledgeService _service;

    public AiKnowledgeController(IAiKnowledgeService service)
    {
        _service = service;
    }

    [HttpPost("query")]
    public async Task<ApiResponse> Query([FromBody] AiKnowledgeQueryRequest request)
    {
        return ApiResponse.Success(await _service.QueryAsync(request));
    }
}
