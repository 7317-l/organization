using PartySchoolApi.Models.DTOs;

namespace PartySchoolApi.Services.Interfaces;

public interface IKMeansService
{
    Task<KMeansClusteringResponse> ClusterAsync(KMeansClusteringRequest request);
}
