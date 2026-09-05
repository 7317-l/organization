using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;

namespace PartySchoolApi.Services.Interfaces;

public interface IExamService
{
    // 试卷
    Task<List<ExamPaperListItemDto>> GetPapersAsync();
    Task<ExamPaperDetailDto> GetPaperByIdAsync(int id);
    Task<ExamPaperDetailDto> CreatePaperAsync(CreateExamPaperRequest request);
    Task UpdatePaperAsync(int id, UpdateExamPaperRequest request);
    Task DeletePaperAsync(int id);

    // 测验
    Task<PagedResponse> GetTestsAsync(int page, int size, int? orgId);
    Task<ExamTestListItemDto> GetTestByIdAsync(int id);
    Task<ExamTestListItemDto> CreateTestAsync(CreateExamTestRequest request, int publisherId);
    Task DeleteTestAsync(int id);
    Task<ExamTestResultDto> GetTestResultAsync(int testId, int? orgId);

    // 专项练习：随机抽题
    Task<PracticePaperDto> GeneratePracticeAsync(string? category, int count);
}
