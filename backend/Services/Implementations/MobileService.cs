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

public class MobileService : IMobileService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IAntiCheatService _antiCheat;

    private const int AntiCheatThresholdSeconds = 120;
    private const int AntiCheatValidMinutes = 30;

    public MobileService(AppDbContext context, IMapper mapper, IAntiCheatService antiCheat)
    {
        _context = context;
        _mapper = mapper;
        _antiCheat = antiCheat;
    }

    public async Task<PagedResponse> GetMyContentsAsync(int memberId, int orgId, int page, int size, string? type, string? keyword, string? sort)
    {
        var publicQuery = _context.LearningContents
            .Include(c => c.Category)
            .Include(c => c.ContentTags).ThenInclude(ct => ct.Tag)
            .Where(c => c.IsPublic)
            .AsQueryable();

        var taskContentIds = await _context.LearningTasks
            .Where(t => t.TargetOrgId == orgId)
            .SelectMany(t => t.TaskContents.Select(tc => tc.ContentId))
            .Distinct()
            .ToListAsync();

        var taskQuery = _context.LearningContents
            .Include(c => c.Category)
            .Include(c => c.ContentTags).ThenInclude(ct => ct.Tag)
            .Where(c => taskContentIds.Contains(c.Id))
            .AsQueryable();

        var publicContents = await publicQuery.ToListAsync();
        var taskContents = await taskQuery.ToListAsync();

        var allContents = publicContents
            .Concat(taskContents)
            .GroupBy(c => c.Id)
            .Select(g => g.First())
            .ToList();

        if (!string.IsNullOrWhiteSpace(type))
        {
            if (Enum.TryParse<ContentType>(type, true, out var contentType))
                allContents = allContents.Where(c => c.ContentType == contentType).ToList();
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            allContents = allContents.Where(c =>
                c.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        allContents = sort?.ToLower() switch
        {
            "hot" => allContents.OrderByDescending(c => c.CreatedAt).ToList(),
            _ => allContents.OrderByDescending(c => c.CreatedAt).ToList()
        };

        var total = allContents.Count;

        var myProgresses = await _context.MemberLearningProgress
            .Where(p => p.MemberId == memberId)
            .ToListAsync();

        var pagedContents = allContents
            .Skip((page - 1) * size)
            .Take(size)
            .ToList();

        var items = pagedContents.Select(c =>
        {
            var progress = myProgresses.FirstOrDefault(p => p.ContentId == c.Id);
            var isCompleted = progress?.IsCompleted ?? false;
            var durationSeconds = progress?.DurationSeconds ?? 0;
            var progressPercent = isCompleted ? 100 : (durationSeconds > 0 ? 50 : 0);
            var status = isCompleted ? "done" : (durationSeconds > 0 ? "learning" : "new");

            return new MobileContentListItemDto
            {
                Id = c.Id,
                Title = c.Title,
                ContentType = (int)c.ContentType,
                ContentTypeName = c.ContentType.ToString(),
                CategoryName = c.Category?.Name,
                Tags = c.ContentTags.Select(ct => ct.Tag?.Name ?? string.Empty).Where(t => !string.IsNullOrEmpty(t)).ToList(),
                CreatedAt = c.CreatedAt,
                Progress = progressPercent,
                Status = status,
                DurationSeconds = durationSeconds,
                IsCompleted = isCompleted
            };
        }).ToList();

        return PagedResponse.Ok(items, page, size, total);
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

        // 防挂机验证：单次上报时长超过阈值时，检查最近是否有通过的防挂机验证
        if (request.DurationSeconds > AntiCheatThresholdSeconds)
        {
            var validSince = DateTime.Now.AddMinutes(-AntiCheatValidMinutes);
            var hasValidVerification = await _context.AntiCheatRecords
                .AnyAsync(r => r.PartyMemberId == memberId
                    && r.IsPass
                    && r.VerifiedAt >= validSince);

            if (!hasValidVerification)
            {
                throw new BusinessException("需要先通过防挂机验证，请完成验证后再继续学习", 403);
            }
        }

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

    public async Task<PagedResponse> GetMyExamsAsync(int memberId, int orgId, int page, int size, string? status)
    {
        var tests = await _context.ExamTests
            .Include(t => t.Paper)
            .Where(t => t.TargetOrgId == orgId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        var myRecords = await _context.MemberTestRecords
            .Where(r => r.MemberId == memberId)
            .ToListAsync();

        var allExams = new List<MobileExamTestDto>();

        foreach (var test in tests)
        {
            var record = myRecords.FirstOrDefault(r => r.TestId == test.Id);
            var isSubmitted = record != null;
            var isExpired = test.Deadline < DateTime.Now;

            string examStatus;
            if (isSubmitted)
                examStatus = "completed";
            else if (isExpired)
                examStatus = "expired";
            else
                examStatus = "pending";

            allExams.Add(new MobileExamTestDto
            {
                Id = test.Id,
                PaperName = test.Paper?.Name ?? string.Empty,
                TimeLimitMinutes = test.TimeLimitMinutes,
                Deadline = test.Deadline,
                IsSubmitted = isSubmitted,
                MyScore = record?.Score,
                TotalScore = test.Paper?.TotalScore ?? 0,
                Status = examStatus
            });
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            allExams = allExams.Where(e => e.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var total = allExams.Count;
        var items = allExams
            .Skip((page - 1) * size)
            .Take(size)
            .ToList();

        return PagedResponse.Ok(items, page, size, total);
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

        if (await _context.MemberTestRecords.AnyAsync(r => r.MemberId == memberId && r.TestId == request.TestId))
            throw new BusinessException("您已提交过该测验", 400);

        var questionIds = JsonSerializer.Deserialize<List<int>>(test.Paper?.QuestionIds ?? "[]") ?? new List<int>();
        var questions = await _context.Questions
            .Where(q => questionIds.Contains(q.Id))
            .ToDictionaryAsync(q => q.Id);

        var answerDict = request.Answers.ToDictionary(a => a.QuestionId, a => a.Answer);

        int totalScore = 0;
        int earnedScore = 0;

        foreach (var qid in questionIds)
        {
            if (!questions.TryGetValue(qid, out var question)) continue;
            totalScore += question.Score;

            var userAnswer = answerDict.ContainsKey(qid) ? answerDict[qid] : string.Empty;

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

    private bool CheckAnswer(Question question, string userAnswer)
    {
        if (string.IsNullOrWhiteSpace(userAnswer)) return false;

        switch (question.QuestionType)
        {
            case Models.Common.QuestionType.SingleChoice:
            case Models.Common.QuestionType.TrueFalse:
                var u = NormalizeSingleAnswer(question, userAnswer);
                var c = NormalizeSingleAnswer(question, question.CorrectAnswer);
                return string.Equals(u, c, StringComparison.OrdinalIgnoreCase);

            case Models.Common.QuestionType.MultiChoice:
                var userSet = ParseMultiAnswer(question, userAnswer);
                var correctSet = ParseMultiAnswer(question, question.CorrectAnswer);
                if (userSet == null || correctSet == null || userSet.Count == 0) return false;
                return userSet.SetEquals(correctSet);

            default:
                return false;
        }
    }

    private string NormalizeSingleAnswer(Question question, string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
        var text = raw.Trim();
        var options = GetQuestionOptions(question);

        if (options.Contains(text, StringComparer.OrdinalIgnoreCase)) return text;

        if (text.Length == 1 && char.IsLetter(text[0]))
        {
            var idx = char.ToUpper(text[0]) - 'A';
            if (idx >= 0 && idx < options.Count) return options[idx];
        }

        if (int.TryParse(text, out var num) && num >= 0 && num < options.Count) return options[num];

        if (string.Equals(text, "true", StringComparison.OrdinalIgnoreCase) || text == "对") return "正确";
        if (string.Equals(text, "false", StringComparison.OrdinalIgnoreCase) || text == "错") return "错误";

        return text;
    }

    private HashSet<string>? ParseMultiAnswer(Question question, string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var options = GetQuestionOptions(question);
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var tokens = new List<string>();

        try
        {
            var strArr = JsonSerializer.Deserialize<List<string>>(raw);
            if (strArr != null) tokens.AddRange(strArr);
        }
        catch
        {
            try
            {
                var intArr = JsonSerializer.Deserialize<List<int>>(raw);
                if (intArr != null) tokens.AddRange(intArr.Select(i => i.ToString()));
            }
            catch
            {
                tokens.AddRange(raw.Split(new[] { ',', '，', ';', '；', ' ' }, StringSplitOptions.RemoveEmptyEntries));
            }
        }

        foreach (var t in tokens)
        {
            var norm = NormalizeSingleAnswer(question, t);
            if (!string.IsNullOrEmpty(norm)) set.Add(norm);
        }

        return set;
    }

    private List<string> GetQuestionOptions(Question question)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(question.Options) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    public async Task<ExamResultDetailDto> GetExamResultAsync(int memberId, int testId)
    {
        var record = await _context.MemberTestRecords
            .Include(r => r.Test).ThenInclude(t => t.Paper)
            .FirstOrDefaultAsync(r => r.MemberId == memberId && r.TestId == testId);

        if (record == null)
            throw new BusinessException("未找到考试记录", 404);

        var answerDict = new Dictionary<int, string>();
        try
        {
            var answerList = JsonSerializer.Deserialize<List<SubmitAnswerItem>>(record.Answers);
            if (answerList != null)
            {
                answerDict = answerList.ToDictionary(a => a.QuestionId, a => a.Answer);
            }
        }
        catch
        {
            try
            {
                var oldDict = JsonSerializer.Deserialize<Dictionary<string, string>>(record.Answers);
                if (oldDict != null)
                {
                    answerDict = oldDict.ToDictionary(kv => int.Parse(kv.Key), kv => kv.Value);
                }
            }
            catch { /* 忽略无法解析的答案 */ }
        }

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

            var userAnswer = answerDict.ContainsKey(qid) ? answerDict[qid] : string.Empty;
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
        int pendingTaskCount = 0;
        foreach (var task in tasks)
        {
            var total = task.TaskContents.Count;
            var completed = await _context.MemberLearningProgress
                .CountAsync(p => p.MemberId == memberId && p.TaskId == task.Id && p.IsCompleted);
            if (total > 0 && completed >= total)
                completedTaskCount++;
            else
                pendingTaskCount++;
        }

        var examRecords = await _context.MemberTestRecords
            .Where(r => r.MemberId == memberId)
            .ToListAsync();

        double avgScore = examRecords.Any() ? Math.Round(examRecords.Average(r => r.Score), 2) : 0;

        double taskCompletionRate = tasks.Any()
            ? Math.Round((double)completedTaskCount / tasks.Count * 100, 2)
            : 0;

        var totalLearnableContents = await _context.LearningContents
            .CountAsync(c => c.IsPublic)
            + tasks.SelectMany(t => t.TaskContents).Select(tc => tc.ContentId).Distinct().Count();
        double learningProgress = totalLearnableContents > 0
            ? Math.Round((double)completedContentCount / totalLearnableContents * 100, 2)
            : 0;

        return new PersonalLearningOverviewDto
        {
            TotalLearningMinutes = totalSeconds / 60,
            CompletedContentCount = completedContentCount,
            CompletedTaskCount = completedTaskCount,
            TotalTaskCount = tasks.Count,
            CompletedExamCount = examRecords.Count,
            AverageExamScore = avgScore,
            TaskCompletionRate = taskCompletionRate,
            PendingCount = pendingTaskCount,
            LearningProgress = learningProgress,
            TotalPoints = member.PointTotal
        };
    }
}
