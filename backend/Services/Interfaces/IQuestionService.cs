using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;

namespace PartySchoolApi.Services.Interfaces;

/// <summary>
/// 题库服务接口
/// </summary>
public interface IQuestionService
{
    /// <summary>分页查询题目</summary>
    Task<PagedResponse> GetPagedAsync(QuestionQueryParams query);

    /// <summary>根据Id获取题目</summary>
    Task<QuestionListItemDto> GetByIdAsync(int id);

    /// <summary>创建题目</summary>
    Task<QuestionListItemDto> CreateAsync(CreateQuestionRequest request);

    /// <summary>更新题目</summary>
    Task UpdateAsync(int id, UpdateQuestionRequest request);

    /// <summary>删除题目</summary>
    Task DeleteAsync(int id);

    /// <summary>批量导入题目</summary>
    Task<ImportResultDto> ImportAsync(Stream fileStream, string fileName);

    /// <summary>获取所有题目分类</summary>
    Task<List<QuestionCategoryDto>> GetAllCategoriesAsync();

    /// <summary>创建题目分类</summary>
    Task<QuestionCategoryDto> CreateCategoryAsync(CreateQuestionCategoryRequest request);

    /// <summary>更新题目分类</summary>
    Task UpdateCategoryAsync(int id, string name);

    /// <summary>删除题目分类</summary>
    Task DeleteCategoryAsync(int id);
}
