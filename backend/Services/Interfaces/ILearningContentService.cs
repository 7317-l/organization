using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;

namespace PartySchoolApi.Services.Interfaces;

public interface ILearningContentService
{
    Task<PagedResponse> GetPagedAsync(ContentQueryParams query);
    Task<ContentDetailDto> GetByIdAsync(int id);
    Task<ContentDetailDto> CreateAsync(CreateContentRequest request);
    Task UpdateAsync(int id, UpdateContentRequest request);
    Task DeleteAsync(int id);

    // 分类
    Task<List<ContentCategoryTreeDto>> GetCategoryTreeAsync();
    Task<ContentCategoryTreeDto> CreateCategoryAsync(CreateContentCategoryRequest request);
    Task UpdateCategoryAsync(int id, string name);
    Task DeleteCategoryAsync(int id);

    // 标签
    Task<List<TagDto>> GetAllTagsAsync();
    Task<TagDto> CreateTagAsync(CreateTagRequest request);
    Task DeleteTagAsync(int id);
}
