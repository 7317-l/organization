using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PartySchoolApi.Data;
using PartySchoolApi.Helpers;
using PartySchoolApi.Middleware;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Models.Entities;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Services.Implementations;

/// <summary>
/// 题库服务实现
/// </summary>
public class QuestionService : IQuestionService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public QuestionService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResponse> GetPagedAsync(QuestionQueryParams query)
    {
        var queryable = _context.Questions
            .Include(q => q.Category)
            .AsQueryable();

        if (query.QuestionType.HasValue)
            queryable = queryable.Where(q => q.QuestionType == query.QuestionType.Value);

        if (query.CategoryId.HasValue)
            queryable = queryable.Where(q => q.CategoryId == query.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(query.Keyword))
            queryable = queryable.Where(q => q.Stem.Contains(query.Keyword));

        var total = await queryable.LongCountAsync();
        var items = await queryable
            .OrderByDescending(q => q.CreatedAt)
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size)
            .ToListAsync();

        return PagedResponse.Ok(_mapper.Map<List<QuestionListItemDto>>(items), query.Page, query.Size, total);
    }

    public async Task<QuestionListItemDto> GetByIdAsync(int id)
    {
        var question = await _context.Questions
            .Include(q => q.Category)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (question == null)
            throw new BusinessException("题目不存在", 404);

        return _mapper.Map<QuestionListItemDto>(question);
    }

    public async Task<QuestionListItemDto> CreateAsync(CreateQuestionRequest request)
    {
        var question = new Question
        {
            QuestionType = request.QuestionType,
            Stem = request.Stem,
            Options = JsonMappingHelper.ToJson(request.Options ?? new List<string>()),
            CorrectAnswer = request.CorrectAnswer,
            Score = request.Score,
            CategoryId = request.CategoryId,
            CreatedAt = DateTime.Now
        };

        _context.Questions.Add(question);
        await _context.SaveChangesAsync();
        return _mapper.Map<QuestionListItemDto>(question);
    }

    public async Task UpdateAsync(int id, UpdateQuestionRequest request)
    {
        var question = await _context.Questions.FindAsync(id);
        if (question == null)
            throw new BusinessException("题目不存在", 404);

        question.QuestionType = request.QuestionType;
        question.Stem = request.Stem;
        question.Options = JsonMappingHelper.ToJson(request.Options ?? new List<string>());
        question.CorrectAnswer = request.CorrectAnswer;
        question.Score = request.Score;
        question.CategoryId = request.CategoryId;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var question = await _context.Questions.FindAsync(id);
        if (question == null)
            throw new BusinessException("题目不存在", 404);

        _context.Questions.Remove(question);
        await _context.SaveChangesAsync();
    }

    public async Task<ImportResultDto> ImportAsync(Stream fileStream, string fileName)
    {
        var result = new ImportResultDto();
        var rows = ExcelHelper.ParseQuestionFile(fileStream, fileName);
        result.TotalCount = rows.Count;

        foreach (var row in rows)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(row.Stem))
                {
                    result.FailCount++;
                    result.Errors.Add("题干为空");
                    continue;
                }

                var question = new Question
                {
                    QuestionType = (QuestionType)row.QuestionType,
                    Stem = row.Stem,
                    Options = string.IsNullOrWhiteSpace(row.Options) ? "[]" : row.Options,
                    CorrectAnswer = row.CorrectAnswer,
                    Score = row.Score,
                    CategoryId = row.CategoryId,
                    CreatedAt = DateTime.Now
                };

                _context.Questions.Add(question);
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.FailCount++;
                string stemPreview = row.Stem.Length > 20 ? row.Stem.Substring(0, 20) : row.Stem;
                result.Errors.Add(stemPreview + ": " + ex.Message);
            }
        }

        await _context.SaveChangesAsync();
        return result;
    }

    // ===== 分类 =====
    public async Task<List<QuestionCategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _context.QuestionCategories
            .Include(c => c.Questions)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return _mapper.Map<List<QuestionCategoryDto>>(categories);
    }

    public async Task<QuestionCategoryDto> CreateCategoryAsync(CreateQuestionCategoryRequest request)
    {
        if (await _context.QuestionCategories.AnyAsync(c => c.Name == request.Name))
            throw new BusinessException("分类已存在", 400);

        var category = new QuestionCategory { Name = request.Name, CreatedAt = DateTime.Now };
        _context.QuestionCategories.Add(category);
        await _context.SaveChangesAsync();
        return _mapper.Map<QuestionCategoryDto>(category);
    }

    public async Task UpdateCategoryAsync(int id, string name)
    {
        var category = await _context.QuestionCategories.FindAsync(id);
        if (category == null)
            throw new BusinessException("分类不存在", 404);

        category.Name = name;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var category = await _context.QuestionCategories
            .Include(c => c.Questions)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            throw new BusinessException("分类不存在", 404);

        if (category.Questions.Any())
            throw new BusinessException("分类下存在题目，无法删除", 400);

        _context.QuestionCategories.Remove(category);
        await _context.SaveChangesAsync();
    }
}
