using PartySchoolApi.Models.DTOs;

namespace PartySchoolApi.Services.Interfaces;

public interface INl2SqlService
{
    Task<Nl2SqlResponse> QueryAsync(Nl2SqlRequest request);
}
