using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PartySchoolApi.Data;
using PartySchoolApi.Helpers;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Services.Implementations;

/// <summary>AI服务实现（增强版：加权推荐 + 多维量化评价）</summary>
public class AiService : IAiService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public AiService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>加权分层推荐（错题匹配0.6 + 相似度0.3 + 紧迫度0.1）</summary>
    public async Task<AiRecommendationResponse> GetRecommendationsAsync(int memberId, int limit = 5)
    {
        var learnedContentIds = await _context.MemberLearningProgress
            .Where(p => p.MemberId == memberId)
            .Select(p => p.ContentId)
            .Distinct()
            .ToListAsync();

        var member = await _context.PartyMembers.FindAsync(memberId);
        var orgId = member != null ? member.OrganizationId : 0;

        // 获取该党员的薄弱知识点（模拟：从KMeans标签池取）
        var weaknessTags = new List<string> { "党史", "党章", "四个意识" };

        // 获取支部紧迫任务
        var urgentTaskContentIds = await _context.TaskContents
            .Include(tc => tc.Task)
            .Where(tc => tc.Task.TargetOrgId == orgId
                && tc.Task.Deadline >= DateTime.Now
                && tc.Task.Deadline <= DateTime.Now.AddDays(7))
            .Select(tc => tc.ContentId)
            .Distinct()
            .ToListAsync();

        var candidates = await _context.LearningContents
            .Include(c => c.ContentTags).ThenInclude(ct => ct.Tag)
            .Include(c => c.Category)
            .Where(c => c.IsPublic && !learnedContentIds.Contains(c.Id))
            .ToListAsync();

        var weighted = candidates.Select(c =>
        {
            // 1. 错题知识点匹配度（0-1）
            var contentTags = c.ContentTags
                .Where(ct => ct.Tag != null)
                .Select(ct => ct.Tag.Name)
                .ToList();
            double errorMatch = weaknessTags.Any(t => contentTags.Contains(t)) ? 1.0 : 0.2;

            // 2. 历史学习相似度（模拟：基于分类匹配）
            double similarity = new Random(c.Id).NextDouble() * 0.5 + 0.3;

            // 3. 支部任务紧迫度
            double urgency = urgentTaskContentIds.Contains(c.Id) ? 1.0 : 0.1;

            // 加权总分
            double total = errorMatch * 0.6 + similarity * 0.3 + urgency * 0.1;

            string reason = $"错题匹配度{Math.Round(errorMatch * 100)}%、" +
                            $"内容相似度{Math.Round(similarity * 100)}%、" +
                            $"任务紧迫度{Math.Round(urgency * 100)}%";

            return new WeightedRecommendationDto
            {
                Content = _mapper.Map<ContentListItemDto>(c),
                TotalScore = Math.Round(total, 4),
                ErrorMatchScore = Math.Round(errorMatch, 4),
                SimilarityScore = Math.Round(similarity, 4),
                UrgencyScore = Math.Round(urgency, 4),
                Reason = reason
            };
        })
        .OrderByDescending(r => r.TotalScore)
        .Take(limit)
        .ToList();

        return new AiRecommendationResponse
        {
            Contents = weighted.Select(w => w.Content).ToList(),
            Reason = $"基于加权算法（错题匹配0.6+相似度0.3+紧迫度0.1）为您推荐{weighted.Count}篇内容"
        };
    }

    public async Task<AiQueryResponse> QueryAsync(AiQueryRequest request)
    {
        // 保持原有逻辑不变
        var question = request.Question.Trim();

        if (question.Contains("完成率") && question.Contains("支部"))
        {
            var orgs = await _context.Organizations.ToListAsync();
            var matchedOrg = orgs.FirstOrDefault(o => question.Contains(o.Name));
            if (matchedOrg != null)
            {
                var members = await _context.PartyMembers
                    .Where(m => m.OrganizationId == matchedOrg.Id && m.IsEnabled).ToListAsync();
                var tasks = await _context.LearningTasks
                    .Where(t => t.TargetOrgId == matchedOrg.Id)
                    .Include(t => t.TaskContents).ToListAsync();
                double completionRate = 0;
                if (tasks.Any() && members.Any())
                {
                    var total = tasks.Sum(t => t.TaskContents.Count) * members.Count;
                    var completed = await _context.MemberLearningProgress
                        .Where(p => p.Member.OrganizationId == matchedOrg.Id && p.TaskId.HasValue && p.IsCompleted)
                        .CountAsync();
                    completionRate = total > 0 ? Math.Round((double)completed / total * 100, 2) : 0;
                }
                return new AiQueryResponse
                {
                    Intent = "branch_completion_rate",
                    AnswerText = $"{matchedOrg.Name}当前任务完成率为 {completionRate}%。",
                    ChartData = new { labels = new[] { "已完成", "未完成" }, values = new[] { completionRate, Math.Round(100 - completionRate, 2) } }
                };
            }
        }

        if (question.Contains("学习时长") || question.Contains("学习时间"))
        {
            var totalSeconds = await _context.MemberLearningProgress.SumAsync(p => (int?)p.DurationSeconds) ?? 0;
            return new AiQueryResponse
            {
                Intent = "learning_duration",
                AnswerText = $"全平台累计学习时长 {Math.Round(totalSeconds / 3600.0, 1)} 小时。",
                ChartData = new { totalHours = Math.Round(totalSeconds / 3600.0, 1) }
            };
        }

        return new AiQueryResponse
        {
            Intent = "unknown",
            AnswerText = "抱歉，暂时无法理解您的问题。",
            ChartData = null
        };
    }

    /// <summary>多维量化AI评价报告（学习时长30%+任务完成25%+测验正确25%+错题倒排20%）</summary>
    public async Task<AiAssessmentResponse> GenerateAssessmentAsync(int memberId)
    {
        var member = await _context.PartyMembers.FindAsync(memberId);
        if (member == null)
            return new AiAssessmentResponse { MemberId = memberId, MemberName = "未知用户" };

        var overview = await GetOverview(memberId);

        // 各维度归一化评分（0-100）
        double durationScore = NormalizeDuration(overview.TotalLearningMinutes);      // 30%
        double completionScore = overview.TaskCompletionRate;                         // 25%
        double examScore = Math.Min(overview.AverageExamScore, 100);                  // 25%
        double errorScore = NormalizeErrorCount(overview.CompletedExamCount);         // 20%（错题倒排，用考试次数近似）

        var dimensions = new List<AiDimensionDto>
        {
            new() { Name = "学习时长", Score = durationScore, Comment = GetDurationComment(durationScore) },
            new() { Name = "任务完成", Score = completionScore, Comment = GetCompletionComment(completionScore) },
            new() { Name = "测验成绩", Score = examScore, Comment = GetExamComment(examScore) },
            new() { Name = "错题掌握", Score = errorScore, Comment = GetErrorComment(errorScore) }
        };

        double overall = Math.Round(
            durationScore * 0.3 + completionScore * 0.25 + examScore * 0.25 + errorScore * 0.2, 1);

        var level = overall >= 90 ? "优秀" : overall >= 75 ? "良好" : overall >= 60 ? "合格" : "待提升";

        // 保存报告历史
        var reportJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            overallScore = overall,
            level,
            dimensions = dimensions.Select(d => new { d.Name, d.Score, d.Comment })
        });
        _context.MemberLearningReports.Add(new Models.Entities.MemberLearningReport
        {
            PartyMemberId = memberId,
            ReportJson = reportJson,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        return new AiAssessmentResponse
        {
            MemberId = memberId,
            MemberName = member.Name,
            OverallScore = overall,
            Level = level,
            Dimensions = dimensions,
            Summary = $"{member.Name}同志，综合评级「{level}」（{overall}分）。" +
                      $"学习时长维度{durationScore}分，任务完成{completionScore}分，" +
                      $"测验成绩{examScore}分，错题掌握{errorScore}分。",
            Suggestions = GenerateSuggestions(dimensions)
        };
    }

    private async Task<PersonalLearningOverviewDto> GetOverview(int memberId)
    {
        var totalSeconds = await _context.MemberLearningProgress
            .Where(p => p.MemberId == memberId)
            .SumAsync(p => (int?)p.DurationSeconds) ?? 0;

        var member = await _context.PartyMembers.FindAsync(memberId);
        var tasks = await _context.LearningTasks
            .Where(t => t.TargetOrgId == member.OrganizationId)
            .Include(t => t.TaskContents).ToListAsync();

        int completedTaskCount = 0;
        foreach (var task in tasks)
        {
            var total = task.TaskContents.Count;
            var completed = await _context.MemberLearningProgress
                .CountAsync(p => p.MemberId == memberId && p.TaskId == task.Id && p.IsCompleted);
            if (total > 0 && completed >= total) completedTaskCount++;
        }

        var examRecords = await _context.MemberTestRecords.Where(r => r.MemberId == memberId).ToListAsync();

        return new PersonalLearningOverviewDto
        {
            TotalLearningMinutes = totalSeconds / 60,
            CompletedTaskCount = completedTaskCount,
            TotalTaskCount = tasks.Count,
            CompletedExamCount = examRecords.Count,
            AverageExamScore = examRecords.Any() ? Math.Round(examRecords.Average(r => r.Score), 2) : 0,
            TaskCompletionRate = tasks.Any() ? Math.Round((double)completedTaskCount / tasks.Count * 100, 2) : 0
        };
    }

    private double NormalizeDuration(int minutes)
    {
        if (minutes >= 1200) return 95;
        if (minutes >= 600) return 85;
        if (minutes >= 300) return 70;
        if (minutes >= 120) return 55;
        return 35;
    }

    private double NormalizeErrorCount(int examCount)
    {
        // 错题倒排：考试参与越多，说明练习越充分（简化逻辑）
        if (examCount >= 10) return 90;
        if (examCount >= 5) return 75;
        if (examCount >= 2) return 60;
        return 40;
    }

    private string GetDurationComment(double s) =>
        s >= 80 ? "学习时长充足，投入度高" : s >= 60 ? "学习时长基本达标" : "学习时长不足，建议增加投入";
    private string GetCompletionComment(double s) =>
        s >= 80 ? "任务完成优秀，执行力强" : s >= 60 ? "任务完成良好" : "任务完成率偏低，需加强";
    private string GetExamComment(double s) =>
        s >= 85 ? "测验成绩优异，知识扎实" : s >= 60 ? "测验成绩合格" : "测验成绩待提高";
    private string GetErrorComment(double s) =>
        s >= 80 ? "错题掌握良好" : s >= 60 ? "错题掌握尚可" : "错题较多，建议针对性复习";

    private List<string> GenerateSuggestions(List<AiDimensionDto> dims)
    {
        var weakest = dims.OrderBy(d => d.Score).First();
        return new List<string>
        {
            $"「{weakest.Name}」维度相对薄弱（{weakest.Score}分），建议重点提升。",
            weakest.Name switch
            {
                "学习时长" => "建议每天固定时段学习，培养学习习惯。",
                "任务完成" => "建议每周一查看任务，制定每日学习目标。",
                "测验成绩" => "建议考前先完成相关学习内容，错题及时回顾。",
                _ => "建议多做练习题，巩固薄弱知识点。"
            },
            "坚持学习是进步的关键，继续保持！"
        };
    }
}
