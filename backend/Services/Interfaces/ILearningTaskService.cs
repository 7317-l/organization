using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;

namespace PartySchoolApi.Services.Interfaces;

public interface ILearningTaskService
{
    Task<PagedResponse> GetPagedAsync(TaskQueryParams query);
    Task<TaskDetailDto> GetByIdAsync(int id);
    Task<TaskDetailDto> CreateAsync(CreateTaskRequest request);
    Task UpdateAsync(int id, UpdateTaskRequest request);
    Task DeleteAsync(int id);
    Task<List<TaskCompletionDetailDto>> GetCompletionDetailsAsync(int taskId);
}
