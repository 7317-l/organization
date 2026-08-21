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
/// 考试服务实现
/// </summary>
public class ExamService : IExamService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public ExamService(AppDbContext context, IMapper mapper, ICurrentUserService currentUser)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    // ===== 试卷 =====
    public async Task<List<ExamPaperListItemDto>> GetPapersAsync()
    {
        var papers = await _context.ExamPapers
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<ExamPaperListItemDto>>(papers);
    }

    public async Task<ExamPaperDetailDto> GetPaperByIdAsync(int id)
    {
        var paper = await _context.ExamPapers.FindAsync(id);
        if (paper == null)
            throw new BusinessException("试卷不存在", 404);

        var questionIds = JsonMappingHelper.ToIntList(paper.QuestionIds);
        var questions = await _context.Questions
            .Include(q => q.Category)
            .Where(q => questionIds.Contains(q.Id))
            .ToListAsync();

        // 按试卷中的顺序排列
        var orderedQuestions = questionIds
            .Select(qid => questions.FirstOrDefault(q => q.Id == qid))
            .Where(q => q != null)
            .Select(q => _mapper.Map<QuestionListItemDto>(q!))
            .ToList();

        return new ExamPaperDetailDto
        {
            Id = paper.Id,
            Name = paper.Name,
            Description = paper.Description,
            Questions = orderedQuestions,
            TotalScore = paper.TotalScore,
            CreatedAt = paper.CreatedAt
        };
    }

    public async Task<ExamPaperDetailDto> CreatePaperAsync(CreateExamPaperRequest request)
    {
        if (request.QuestionIds == null || !request.QuestionIds.Any())
            throw new BusinessException("请至少选择一道题目", 400);

        var questions = await _context.Questions
            .Where(q => request.QuestionIds.Contains(q.Id))
            .ToListAsync();

        var totalScore = questions.Sum(q => q.Score);

        var paper = new ExamPaper
        {
            Name = request.Name,
            Description = request.Description,
            QuestionIds = JsonMappingHelper.ToJson(request.QuestionIds),
            TotalScore = totalScore,
            CreatedAt = DateTime.Now
        };

        _context.ExamPapers.Add(paper);
        await _context.SaveChangesAsync();
        return await GetPaperByIdAsync(paper.Id);
    }

    public async Task UpdatePaperAsync(int id, UpdateExamPaperRequest request)
    {
        var paper = await _context.ExamPapers.FindAsync(id);
        if (paper == null)
            throw new BusinessException("试卷不存在", 404);

        var questions = await _context.Questions
            .Where(q => request.QuestionIds.Contains(q.Id))
            .ToListAsync();

        paper.Name = request.Name;
        paper.Description = request.Description;
        paper.QuestionIds = JsonMappingHelper.ToJson(request.QuestionIds);
        paper.TotalScore = questions.Sum(q => q.Score);

        await _context.SaveChangesAsync();
    }

    public async Task DeletePaperAsync(int id)
    {
        var paper = await _context.ExamPapers
            .Include(p => p.ExamTests)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (paper == null)
            throw new BusinessException("试卷不存在", 404);

        if (paper.ExamTests.Any())
            throw new BusinessException("该试卷已被测验使用，无法删除", 400);

        _context.ExamPapers.Remove(paper);
        await _context.SaveChangesAsync();
    }

    // ===== 测验 =====
    public async Task<PagedResponse> GetTestsAsync(int page, int size, int? orgId)
    {
        var queryable = _context.ExamTests
            .Include(t => t.Paper)
            .Include(t => t.TargetOrg)
            .Include(t => t.TestRecords)
            .AsQueryable();

        if (_currentUser.Role == UserRole.BranchSecretary)
            queryable = queryable.Where(t => t.TargetOrgId == _currentUser.OrganizationId);

        if (orgId.HasValue)
            queryable = queryable.Where(t => t.TargetOrgId == orgId.Value);

        var total = await queryable.LongCountAsync();
        var items = await queryable
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return PagedResponse.Ok(_mapper.Map<List<ExamTestListItemDto>>(items), page, size, total);
    }

    public async Task<ExamTestListItemDto> GetTestByIdAsync(int id)
    {
        var test = await _context.ExamTests
            .Include(t => t.Paper)
            .Include(t => t.TargetOrg)
            .Include(t => t.TestRecords)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (test == null)
            throw new BusinessException("测验不存在", 404);

        return _mapper.Map<ExamTestListItemDto>(test);
    }

    public async Task<ExamTestListItemDto> CreateTestAsync(CreateExamTestRequest request, int publisherId)
    {
        var paper = await _context.ExamPapers.FindAsync(request.PaperId);
        if (paper == null)
            throw new BusinessException("试卷不存在", 404);

        var org = await _context.Organizations.FindAsync(request.TargetOrgId);
        if (org == null)
            throw new BusinessException("目标支部不存在", 404);

        var test = new ExamTest
        {
            PaperId = request.PaperId,
            PublisherId = publisherId,
            TargetOrgId = request.TargetOrgId,
            TimeLimitMinutes = request.TimeLimitMinutes,
            Deadline = request.Deadline,
            CreatedAt = DateTime.Now
        };

        _context.ExamTests.Add(test);
        await _context.SaveChangesAsync();
        return _mapper.Map<ExamTestListItemDto>(test);
    }

    public async Task DeleteTestAsync(int id)
    {
        var test = await _context.ExamTests.FindAsync(id);
        if (test == null)
            throw new BusinessException("测验不存在", 404);

        _context.ExamTests.Remove(test);
        await _context.SaveChangesAsync();
    }

    public async Task<ExamTestResultDto> GetTestResultAsync(int testId, int? orgId)
    {
        var test = await _context.ExamTests
            .Include(t => t.Paper)
            .Include(t => t.TestRecords).ThenInclude(r => r.Member)
            .FirstOrDefaultAsync(t => t.Id == testId);

        if (test == null)
            throw new BusinessException("测验不存在", 404);

        var records = test.TestRecords.AsQueryable();
        if (orgId.HasValue)
            records = records.Where(r => r.Member != null && r.Member.OrganizationId == orgId.Value);

        var recordList = records.ToList();
        var totalParticipants = recordList.Count;

        double averageScore = totalParticipants > 0
            ? Math.Round(recordList.Average(r => r.Score), 2)
            : 0;

        // 修复：先计算及格线，再加括号确保比较正确
        int passLine = test.Paper != null ? (int)(test.Paper.TotalScore * 0.6) : 60;
        var passCount = recordList.Count(r => r.Score >= passLine);

        double passRate = totalParticipants > 0
            ? Math.Round((double)passCount / totalParticipants * 100, 2)
            : 0;

        return new ExamTestResultDto
        {
            TestId = testId,
            PaperName = test.Paper != null ? test.Paper.Name : string.Empty,
            TotalParticipants = totalParticipants,
            AverageScore = averageScore,
            PassRate = passRate,
            Records = _mapper.Map<List<MemberTestRecordDto>>(
                recordList.OrderByDescending(r => r.Score).ToList())
        };
    }
}
