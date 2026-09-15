using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PartySchoolApi.Data;
using PartySchoolApi.Middleware;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Models.Entities;
using PartySchoolApi.Services.Interfaces;
using System.Text.Json;

namespace PartySchoolApi.Services.Implementations;

public class LearningContentService : ILearningContentService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public LearningContentService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResponse> GetPagedAsync(ContentQueryParams query)
    {
        var queryable = _context.LearningContents
            .Include(c => c.Category)
            .Include(c => c.ContentTags).ThenInclude(ct => ct.Tag)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Title))
            queryable = queryable.Where(c => c.Title.Contains(query.Title));

        if (query.ContentType.HasValue)
            queryable = queryable.Where(c => c.ContentType == query.ContentType.Value);

        if (query.CategoryId.HasValue)
            queryable = queryable.Where(c => c.CategoryId == query.CategoryId.Value);

        if (query.IsPublic.HasValue)
            queryable = queryable.Where(c => c.IsPublic == query.IsPublic.Value);

        var total = await queryable.LongCountAsync();
        var items = await queryable
            .OrderByDescending(c => c.CreatedAt)
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size)
            .ToListAsync();

        return PagedResponse.Ok(_mapper.Map<List<ContentListItemDto>>(items), query.Page, query.Size, total);
    }

    public async Task<ContentDetailDto> GetByIdAsync(int id)
    {
        var content = await _context.LearningContents
            .Include(c => c.Category)
            .Include(c => c.ContentTags).ThenInclude(ct => ct.Tag)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (content == null)
            throw new BusinessException("内容不存在", 404);

        return _mapper.Map<ContentDetailDto>(content);
    }

    public async Task<ContentDetailDto> CreateAsync(CreateContentRequest request)
    {
        var content = new LearningContent
        {
            Title = request.Title,
            Body = request.Body,
            VideoUrl = request.VideoUrl,
            ContentType = request.ContentType,
            CategoryId = request.CategoryId,
            IsPublic = request.IsPublic,
            CreatedAt = DateTime.Now
        };

        if (request.TagIds != null && request.TagIds.Any())
        {
            content.ContentTags = request.TagIds.Select(tid => new ContentTag { TagId = tid }).ToList();
        }

        _context.LearningContents.Add(content);
        await _context.SaveChangesAsync();
        return await GetByIdAsync(content.Id);
    }

    public async Task UpdateAsync(int id, UpdateContentRequest request)
    {
        var content = await _context.LearningContents
            .Include(c => c.ContentTags)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (content == null)
            throw new BusinessException("内容不存在", 404);

        content.Title = request.Title;
        content.Body = request.Body;
        content.VideoUrl = request.VideoUrl;
        content.ContentType = request.ContentType;
        content.CategoryId = request.CategoryId;
        content.IsPublic = request.IsPublic;

        // 更新标签
        content.ContentTags.Clear();
        if (request.TagIds != null && request.TagIds.Any())
        {
            foreach (var tid in request.TagIds)
                content.ContentTags.Add(new ContentTag { TagId = tid });
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var content = await _context.LearningContents.FindAsync(id);
        if (content == null)
            throw new BusinessException("内容不存在", 404);

        _context.LearningContents.Remove(content);
        await _context.SaveChangesAsync();
    }

    // ===== 分类 =====
    public async Task<List<ContentCategoryTreeDto>> GetCategoryTreeAsync()
    {
        var all = await _context.ContentCategories
            .Include(c => c.Children)
            .ToListAsync();

        var roots = all.Where(c => c.ParentId == null).ToList();
        return _mapper.Map<List<ContentCategoryTreeDto>>(roots);
    }

    public async Task<ContentCategoryTreeDto> CreateCategoryAsync(CreateContentCategoryRequest request)
    {
        var category = new ContentCategory
        {
            Name = request.Name,
            ParentId = request.ParentId,
            CreatedAt = DateTime.Now
        };

        _context.ContentCategories.Add(category);
        await _context.SaveChangesAsync();
        return _mapper.Map<ContentCategoryTreeDto>(category);
    }

    public async Task UpdateCategoryAsync(int id, string name)
    {
        var category = await _context.ContentCategories.FindAsync(id);
        if (category == null)
            throw new BusinessException("分类不存在", 404);

        category.Name = name;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var category = await _context.ContentCategories
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            throw new BusinessException("分类不存在", 404);

        if (category.Children.Any())
            throw new BusinessException("存在子分类，无法删除", 400);

        _context.ContentCategories.Remove(category);
        await _context.SaveChangesAsync();
    }

    // ===== 标签 =====
    public async Task<List<TagDto>> GetAllTagsAsync()
    {
        var tags = await _context.Tags.OrderBy(t => t.Name).ToListAsync();
        return _mapper.Map<List<TagDto>>(tags);
    }

    public async Task<TagDto> CreateTagAsync(CreateTagRequest request)
    {
        if (await _context.Tags.AnyAsync(t => t.Name == request.Name))
            throw new BusinessException("标签已存在", 400);

        var tag = new Tag { Name = request.Name, CreatedAt = DateTime.Now };
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();
        return _mapper.Map<TagDto>(tag);
    }

    public async Task DeleteTagAsync(int id)
    {
        var tag = await _context.Tags.FindAsync(id);
        if (tag == null)
            throw new BusinessException("标签不存在", 404);

        _context.Tags.Remove(tag);
        await _context.SaveChangesAsync();
    }
}
