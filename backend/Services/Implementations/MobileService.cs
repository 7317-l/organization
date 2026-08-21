using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PartySchoolApi.Data;
using PartySchoolApi.Middleware;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Models.Entities;
using PartySchoolApi.Services.Interfaces;
using System.Text.Json;

namespace PartySchoolApi.Services.Implementations;

public class MobileService : IMobileService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public MobileService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ContentListItemDto>> GetMyContentsAsync(int memberId, int orgId)
    {
        // 公共内容
        var publicContents = await _context.LearningContents
            .Include(c => c.Category)
            .Include(c => c.ContentTags).ThenInclude(ct => ct.Tag)
            .Where(c => c.IsPublic)
            .OrderByDescending(c => c.CreatedAt)
            .Take(50)
            .ToListAsync();

        // 所属支部任务中的内容
        var taskContentIds = await _context.LearningTasks
            .Where(t => t.TargetOrgId == orgId)
            .SelectMany(t => t.TaskContents.Select(tc => tc.ContentId))
            .Distinct()
            .ToListAsync();

        var taskContents = await _context.LearningContents
            .Include(c => c.Category)
            .Include(c => c.ContentTags).ThenInclude(ct => ct.Tag)
            .Where(c => taskContentIds.Contains(c.Id))
            .ToListAsync();

        // 合并去重
        var allContents = publicContents
            .Concat(taskContents)
            .GroupBy(c => c.Id)
            .Select(g => g.First())
            .ToList();

        return _mapper.Map<List<ContentListItemDto>>(allContents);
    }

    public async Task<ContentDetailDto> GetContentDetailAsync(int contentId)
    {
        var content = await _context.LearningContents
            .Include(c => c.Category)
            .Include(c => c.ContentTags).ThenInclude(ct => ct.Tag)
            .FirstOrDefaultAsync(c => c.Id == contentId);

        if (content == null)
            throw new BusinessException("内容不存在", 404);

        return _mapper.Map<ContentDetailDto>(content);
    }

    public async Task ReportProgressAsync(int memberId, ReportProgressRequest request)
    {
        var content = await _context.LearningContents.FindAsync(request.ContentId);
        if (content == null)
            throw new BusinessException("内容不存在", 404);

        // 查找已有进度记录
        var progress = await _context.MemberLearningProgress
            .FirstOrDefaultAsync(p => p.MemberId == memberId
                && p.ContentId == request.ContentId
                && p.TaskId == request.TaskId);

        if (progress == null)
        {
            progress = new MemberLearningProgress
            {
                MemberId = memberId,
                ContentId = request.ContentId,
                TaskId = request.TaskId,
                DurationSeconds = request.DurationSeconds,
                IsCompleted = request.IsCompleted,
                CompletedAt = request.IsCompleted ? DateTime.Now : null,
                UpdatedAt = DateTime.Now
            };
            _context.MemberLearningProgress.Add(progress);
        }
        else
        {
            progress.DurationSeconds += request.DurationSeconds;
            if (request.IsCompleted && !progress.IsCompleted)
            {
                progress.IsCompleted = true;
                progress.CompletedAt = DateTime.Now;
            }
            progress.UpdatedAt = DateTime.Now;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<MobileTaskDto>> GetMyTasksAsync(int memberId, bool completed)
    {
        var member = await _context.PartyMembers.FindAsync(memberId);
        if (member == null)
            throw new BusinessException("用户不存在", 404);

        var tasks = await _context.LearningTasks
            .Include(t => t.TaskContents)
            .Where(t => t.TargetOrgId == member.OrganizationId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        var result = new List<MobileTaskDto>();

        foreach (var task in tasks)
        {
            var totalContents = task.TaskContents.Count;
            var completedContents = await _context.MemberLearningProgress
                .CountAsync(p => p.MemberId == memberId && p.TaskId == task.Id && p.IsCompleted);

            var isAllCompleted = totalContents > 0 && completedContents >= totalContents;

            if (completed == isAllCompleted)
            {
                result.Add(new MobileTaskDto
                {
                    Id = task.Id,
                    TaskName = task.TaskName,
                    Deadline = task.Deadline,
                    TotalContents = totalContents,
                    CompletedContents = completedContents,
                    CompletionRate = totalContents > 0 ? Math.Round((double)completedContents / totalContents * 100, 2) : 0
                });
            }
        }

        return result;
    }

    public async Task CompleteTaskContentAsync(int memberId, CompleteTaskContentRequest request)
    {
        var progress = await _context.MemberLearningProgress
            .FirstOrDefaultAsync(p => p.MemberId == memberId
                && p.ContentId == request.ContentId
                && p.TaskId == request.TaskId);

        if (progress == null)
        {
            progress = new MemberLearningProgress
            {
                MemberId = memberId,
                ContentId = request.ContentId,
                TaskId = request.TaskId,
                DurationSeconds = 0,
                IsCompleted = true,
                CompletedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _context.MemberLearningProgress.Add(progress);
        }
        else
        {
            progress.IsCompleted = true;
            progress.CompletedAt = DateTime.Now;
            progress.UpdatedAt = DateTime.Now;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<MobileExamTestDto>> GetMyExamsAsync(int memberId, int orgId)
    {
        var tests = await _context.ExamTests
            .Include(t => t.Paper)
            .Where(t => t.TargetOrgId == orgId && t.Deadline >= DateTime.Now)
            .OrderBy(t => t.Deadline)
            .ToListAsync();

        var myRecords = await _context.MemberTestRecords
            .Where(r => r.MemberId == memberId)
            .ToListAsync();

        var result = new List<MobileExamTestDto>();

        foreach (var test in tests)
        {
            var record = myRecords.FirstOrDefault(r => r.TestId == test.Id);
            result.Add(new MobileExamTestDto
            {
                Id = test.Id,
                PaperName = test.Paper?.Name ?? string.Empty,
                TimeLimitMinutes = test.TimeLimitMinutes,
                Deadline = test.Deadline,
                IsSubmitted = record != null,
                MyScore = record?.Score,
                TotalScore = test.Paper?.TotalScore ?? 0
            });
        }

        return result;
    }

    public async Task<StartExamResponse> StartExamAsync(int testId)
    {
        var test = await _context.ExamTests
            .Include(t => t.Paper)
            .FirstOrDefaultAsync(t => t.Id == testId);

        if (test == null)
            throw new BusinessException("测验不存在", 404);

        if (test.Deadline < DateTime.Now)
            throw new BusinessException("测验已截止", 400);

        var questionIds = JsonSerializer.Deserialize<List<int>>(test.Paper?.QuestionIds ?? "[]") ?? new List<int>();
        var questions = await _context.Questions
            .Where(q => questionIds.Contains(q.Id))
            .ToListAsync();

        var orderedQuestions = questionIds
            .Select(qid => questions.FirstOrDefault(q => q.Id == qid))
            .Where(q => q != null)
            .Select(q => new ExamQuestionDto
            {
                Id = q!.Id,
                QuestionType = (int)q.QuestionType,
                Stem = q.Stem,
                Options = JsonSerializer.Deserialize<List<string>>(q.Options) ?? new List<string>(),
                Score = q.Score
            })
            .ToList();

        return new StartExamResponse
        {
            TestId = test.Id,
            PaperName = test.Paper?.Name ?? string.Empty,
            TimeLimitMinutes = test.TimeLimitMinutes,
            Deadline = test.Deadline,
            Questions = orderedQuestions
        };
    }

    public async Task<SubmitExamResponse> SubmitExamAsync(int memberId, SubmitExamRequest request)
    {
        var test = await _context.ExamTests
            .Include(t => t.Paper)
            .FirstOrDefaultAsync(t => t.Id == request.TestId);

        if (test == null)
            throw new BusinessException("测验不存在", 404);

        // 检查是否已提交
        if (await _context.MemberTestRecords.AnyAsync(r => r.MemberId == memberId && r.TestId == request.TestId))
            throw new BusinessException("您已提交过该测验", 400);

        var questionIds = JsonSerializer.Deserialize<List<int>>(test.Paper?.QuestionIds ?? "[]") ?? new List<int>();
        var questions = await _context.Questions
            .Where(q => questionIds.Contains(q.Id))
            .ToDictionaryAsync(q => q.Id);

        int totalScore = 0;
        int earnedScore = 0;

        foreach (var qid in questionIds)
        {
            if (!questions.TryGetValue(qid, out var question)) continue;
            totalScore += question.Score;

            var userAnswer = request.Answers.ContainsKey(qid.ToString()) ? request.Answers[qid.ToString()] : string.Empty;

            if (CheckAnswer(question, userAnswer))
                earnedScore += question.Score;
        }

        var record = new MemberTestRecord
        {
            MemberId = memberId,
            TestId = request.TestId,
            Answers = JsonSerializer.Serialize(request.Answers),
            Score = earnedScore,
            SubmittedAt = DateTime.Now
        };

        _context.MemberTestRecords.Add(record);
        await _context.SaveChangesAsync();

        return new SubmitExamResponse
        {
            RecordId = record.Id,
            Score = earnedScore,
            TotalScore = totalScore,
            IsPassed = earnedScore >= totalScore * 0.6
        };
    }

    /// <summary>
    /// 判题逻辑
    /// </summary>
    private bool CheckAnswer(Question question, string userAnswer)
    {
        if (string.IsNullOrWhiteSpace(userAnswer)) return false;

        switch (question.QuestionType)
        {
            case Models.Common.QuestionType.SingleChoice:
            case Models.Common.QuestionType.TrueFalse:
                return userAnswer.Trim() == question.CorrectAnswer.Trim();

            case Models.Common.QuestionType.MultiChoice:
                try
                {
                    var userSet = JsonSerializer.Deserialize<List<int>>(userAnswer)?.OrderBy(x => x).ToList();
                    var correctSet = JsonSerializer.Deserialize<List<int>>(question.CorrectAnswer)?.OrderBy(x => x).ToList();
                    if (userSet == null || correctSet == null) return false;
                    return userSet.SequenceEqual(correctSet);
                }
                catch
                {
                    return false;
                }

            default:
                return false;
        }
    }

    public async Task<ExamResultDetailDto> GetExamResultAsync(int memberId, int testId)
    {
        var record = await _context.MemberTestRecords
            .Include(r => r.Test).ThenInclude(t => t.Paper)
            .FirstOrDefaultAsync(r => r.MemberId == memberId && r.TestId == testId);

        if (record == null)
            throw new BusinessException("未找到考试记录", 404);

        var answers = JsonSerializer.Deserialize<Dictionary<string, string>>(record.Answers) ?? new Dictionary<string, string>();
        var questionIds = JsonSerializer.Deserialize<List<int>>(record.Test.Paper?.QuestionIds ?? "[]") ?? new List<int>();
        var questions = await _context.Questions
            .Where(q => questionIds.Contains(q.Id))
            .ToDictionaryAsync(q => q.Id);

        var questionAnswers = new List<QuestionAnswerDto>();
        int totalScore = 0;

        foreach (var qid in questionIds)
        {
            if (!questions.TryGetValue(qid, out var question)) continue;
            totalScore += question.Score;

            var userAnswer = answers.ContainsKey(qid.ToString()) ? answers[qid.ToString()] : string.Empty;
            var isCorrect = CheckAnswer(question, userAnswer);

            questionAnswers.Add(new QuestionAnswerDto
            {
                QuestionId = qid,
                Stem = question.Stem,
                UserAnswer = userAnswer,
                CorrectAnswer = question.CorrectAnswer,
                IsCorrect = isCorrect,
                Score = question.Score,
                EarnedScore = isCorrect ? question.Score : 0
            });
        }

        return new ExamResultDetailDto
        {
            TestId = testId,
            PaperName = record.Test.Paper?.Name ?? string.Empty,
            Score = record.Score,
            TotalScore = totalScore,
            IsPassed = record.Score >= totalScore * 0.6,
            SubmittedAt = record.SubmittedAt,
            QuestionAnswers = questionAnswers
        };
    }

    public async Task<PersonalLearningOverviewDto> GetPersonalOverviewAsync(int memberId)
    {
        var member = await _context.PartyMembers.FindAsync(memberId);
        if (member == null)
            throw new BusinessException("用户不存在", 404);

        var totalSeconds = await _context.MemberLearningProgress
            .Where(p => p.MemberId == memberId)
            .SumAsync(p => (int?)p.DurationSeconds) ?? 0;

        var completedContentCount = await _context.MemberLearningProgress
            .Where(p => p.MemberId == memberId && p.IsCompleted)
            .Select(p => p.ContentId)
            .Distinct()
            .CountAsync();

        var tasks = await _context.LearningTasks
            .Where(t => t.TargetOrgId == member.OrganizationId)
            .Include(t => t.TaskContents)
            .ToListAsync();

        int completedTaskCount = 0;
        foreach (var task in tasks)
        {
            var total = task.TaskContents.Count;
            var completed = await _context.MemberLearningProgress
                .CountAsync(p => p.MemberId == memberId && p.TaskId == task.Id && p.IsCompleted);
            if (total > 0 && completed >= total)
                completedTaskCount++;
        }

        var examRecords = await _context.MemberTestRecords
            .Where(r => r.MemberId == memberId)
            .ToListAsync();

        double avgScore = examRecords.Any() ? Math.Round(examRecords.Average(r => r.Score), 2) : 0;

        double taskCompletionRate = tasks.Any()
            ? Math.Round((double)completedTaskCount / tasks.Count * 100, 2)
            : 0;

        return new PersonalLearningOverviewDto
        {
            TotalLearningMinutes = totalSeconds / 60,
            CompletedContentCount = completedContentCount,
            CompletedTaskCount = completedTaskCount,
            TotalTaskCount = tasks.Count,
            CompletedExamCount = examRecords.Count,
            AverageExamScore = avgScore,
            TaskCompletionRate = taskCompletionRate
        };
    }
}

