using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Controllers;

[ApiController]
[Route("api/v1/kmeans")]
[Authorize]
public class KMeansController : ControllerBase
{
    private readonly IKMeansService _service;

    public KMeansController(IKMeansService service)
    {
        _service = service;
    }

    [HttpPost("cluster")]
    public async Task<ApiResponse> Cluster([FromBody] KMeansClusteringRequest request)
    {
        return ApiResponse.Success(await _service.ClusterAsync(request));
    }
}
