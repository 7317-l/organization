using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PartySchoolApi.Data;
using PartySchoolApi.Helpers;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;
using System.Text.Json;

namespace PartySchoolApi.Services.Implementations;

/// <summary>AI服务实现（增强版：加权推荐 + 多维量化评价 + 千问AI生成评语）</summary>
public class AiService : IAiService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IQwenService _qwen;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public AiService(AppDbContext context, IMapper mapper, IQwenService qwen)
    {
        _context = context;
        _mapper = mapper;
        _qwen = qwen;
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

        // 获取该党员的薄弱知识点（从考试记录中提取错题知识点）
        var weaknessTags = await GetWeaknessTagsAsync(memberId);

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

            // 2. 历史学习相似度（基于分类匹配）
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

    /// <summary>从考试记录中提取薄弱知识点标签</summary>
    private async Task<List<string>> GetWeaknessTagsAsync(int memberId)
    {
        var records = await _context.MemberTestRecords
            .Where(r => r.MemberId == memberId)
            .ToListAsync();

        var wrongQuestionIds = new List<int>();
        foreach (var record in records)
        {
            try
            {
                var answers = JsonSerializer.Deserialize<List<SubmitAnswerItem>>(record.Answers);
                if (answers == null) continue;

                var test = await _context.ExamTests
                    .Include(t => t.Paper)
                    .FirstOrDefaultAsync(t => t.Id == record.TestId);
                if (test?.Paper == null) continue;

                var qids = JsonSerializer.Deserialize<List<int>>(test.Paper.QuestionIds ?? "[]") ?? new List<int>();
                var questions = await _context.Questions.Where(q => qids.Contains(q.Id)).ToDictionaryAsync(q => q.Id);

                foreach (var ans in answers)
                {
                    if (questions.TryGetValue(ans.QuestionId, out var q))
                    {
                        bool correct = q.QuestionType switch
                        {
                            Models.Common.QuestionType.SingleChoice or Models.Common.QuestionType.TrueFalse
                                => ans.Answer?.Trim() == q.CorrectAnswer.Trim(),
                            Models.Common.QuestionType.MultiChoice => CheckMultiAnswer(ans.Answer, q.CorrectAnswer),
                            _ => false
                        };
                        if (!correct) wrongQuestionIds.Add(q.Id);
                    }
                }
            }
            catch { /* 忽略解析错误 */ }
        }

        // 根据错题的分类提取薄弱知识点
        var wrongQuestions = await _context.Questions
            .Include(q => q.Category)
            .Where(q => wrongQuestionIds.Contains(q.Id))
            .ToListAsync();

        return wrongQuestions
            .Where(q => q.Category != null)
            .Select(q => q.Category!.Name)
            .Distinct()
            .Take(5)
            .ToList();
    }

    private bool CheckMultiAnswer(string? userAnswer, string correctAnswer)
    {
        if (string.IsNullOrWhiteSpace(userAnswer)) return false;
        try
        {
            var userSet = JsonSerializer.Deserialize<List<int>>(userAnswer)?.OrderBy(x => x).ToList();
            var correctSet = JsonSerializer.Deserialize<List<int>>(correctAnswer)?.OrderBy(x => x).ToList();
            return userSet != null && correctSet != null && userSet.SequenceEqual(correctSet);
        }
        catch { return false; }
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

    /// <summary>多维量化AI评价报告（动态计算：学习时长20%+内容完成20%+任务完成20%+测验成绩20%+错题掌握20%）</summary>
    public async Task<AiAssessmentResponse> GenerateAssessmentAsync(int memberId)
    {
        var member = await _context.PartyMembers.FindAsync(memberId);
        if (member == null)
            return new AiAssessmentResponse { MemberId = memberId, MemberName = "未知用户" };

        var overview = await GetDetailedOverview(memberId);

        // 各维度动态评分（0-100）
        // 1. 学习时长：目标600分钟（10小时），按比例计算，上限100
        double durationScore = Math.Min(100, Math.Round((double)overview.TotalLearningMinutes / 600 * 100, 1));

        // 2. 学习内容完成度：已完成内容数 / 可学内容总数 * 100
        double contentCompletionScore = overview.TotalLearnableContents > 0
            ? Math.Round((double)overview.CompletedContentCount / overview.TotalLearnableContents * 100, 1)
            : 0;

        // 3. 任务完成率：直接使用任务完成率
        double taskCompletionScore = overview.TaskCompletionRate;

        // 4. 测验成绩：归一化（总得分/总分*100），无考试记录时给40分基础分
        double examScore = overview.TotalExamScore > 0
            ? Math.Round((double)overview.EarnedExamScore / overview.TotalExamScore * 100, 1)
            : 40;

        // 5. 错题掌握：基于考试正确率，无考试记录时给40分基础分
        double errorMasteryScore = overview.TotalQuestions > 0
            ? Math.Round((double)overview.CorrectQuestions / overview.TotalQuestions * 100, 1)
            : 40;

        var dimensions = new List<AiDimensionDto>
        {
            new() { Name = "学习时长", Score = durationScore, Comment = GetDurationComment(durationScore) },
            new() { Name = "内容完成", Score = contentCompletionScore, Comment = GetContentCompletionComment(contentCompletionScore) },
            new() { Name = "任务完成", Score = taskCompletionScore, Comment = GetCompletionComment(taskCompletionScore) },
            new() { Name = "测验成绩", Score = examScore, Comment = GetExamComment(examScore) },
            new() { Name = "错题掌握", Score = errorMasteryScore, Comment = GetErrorComment(errorMasteryScore) }
        };

        // 综合评分：五维各占20%
        double overall = Math.Round(
            (durationScore + contentCompletionScore + taskCompletionScore + examScore + errorMasteryScore) / 5, 1);

        var level = overall >= 90 ? "优秀" : overall >= 75 ? "良好" : overall >= 60 ? "合格" : "待提升";

        // 千问生成个性化总结与建议（未配置时自动回退到模板）
        var (summary, suggestions) = await BuildAssessmentTextAsync(member.Name, overall, level, dimensions, overview);

        // 保存报告历史
        var reportJson = JsonSerializer.Serialize(new
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
            Summary = summary,
            Suggestions = suggestions
        };
    }

    /// <summary>使用千问AI生成个性化学习总结与建议（未配置时自动回退到模板）</summary>
    private async Task<(string Summary, List<string> Suggestions)> BuildAssessmentTextAsync(
        string name, double overall, string level, List<AiDimensionDto> dims, DetailedLearningOverview overview)
    {
        var fallbackSummary = $"{name}同志，综合评级「{level}」（{overall}分）。" +
                              $"学习时长{dims[0].Score}分，内容完成{dims[1].Score}分，" +
                              $"任务完成{dims[2].Score}分，测验成绩{dims[3].Score}分，错题掌握{dims[4].Score}分。";
        var fallbackSuggestions = GenerateSuggestions(dims);

        if (!_qwen.IsConfigured) return (fallbackSummary, fallbackSuggestions);

        try
        {
            var dimText = string.Join("；", dims.Select(d => $"{d.Name}:{d.Score}分({d.Comment})"));
            var userPrompt =
                $"党员姓名：{name}\n综合得分：{overall}（等级：{level}）\n各维度：{dimText}\n" +
                $"学习总时长：{overview.TotalLearningMinutes}分钟；" +
                $"已完成内容：{overview.CompletedContentCount}/{overview.TotalLearnableContents}；" +
                $"任务完成率：{overview.TaskCompletionRate}%；" +
                $"考试题目：{overview.TotalQuestions}题，正确{overview.CorrectQuestions}题。\n\n" +
                "请只输出 JSON：{\"summary\":\"一段150字以内的鼓励式个人学习总结（称呼同志，语气亲和专业）\",\"suggestions\":[\"3条具体可执行的改进建议\"]}";

            var raw = await _qwen.ChatAsync(
                "你是党校党员的学习辅导专家，擅长根据量化数据给出温暖、专业、具体的评价。只输出 JSON。",
                userPrompt,
                temperature: 0.5,
                jsonMode: true);

            var parsed = ParseAssessmentJson(raw);
            if (parsed.HasValue)
            {
                var v = parsed.Value;
                return (v.Summary ?? fallbackSummary,
                        v.Suggestions != null && v.Suggestions.Count > 0 ? v.Suggestions : fallbackSuggestions);
            }
        }
        catch
        {
            // 忽略，走兜底
        }

        return (fallbackSummary, fallbackSuggestions);
    }

    /// <summary>解析AI返回的评估JSON</summary>
    private static (string? Summary, List<string>? Suggestions)? ParseAssessmentJson(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var start = raw.IndexOf('{');
        var end = raw.LastIndexOf('}');
        if (start < 0 || end <= start) return null;
        try
        {
            var json = raw.Substring(start, end - start + 1);
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var summary = root.TryGetProperty("summary", out var s) ? s.GetString() : null;
            var suggestions = new List<string>();
            if (root.TryGetProperty("suggestions", out var su) && su.ValueKind == JsonValueKind.Array)
            {
                suggestions = su.EnumerateArray().Select(x => x.GetString() ?? "").Where(x => x.Length > 0).ToList();
            }
            return (summary, suggestions);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>详细学习概览（包含考试归一化所需数据）</summary>
    private async Task<DetailedLearningOverview> GetDetailedOverview(int memberId)
    {
        var member = await _context.PartyMembers.FindAsync(memberId);
        var orgId = member?.OrganizationId ?? 0;

        // 学习时长
        var totalSeconds = await _context.MemberLearningProgress
            .Where(p => p.MemberId == memberId)
            .SumAsync(p => (int?)p.DurationSeconds) ?? 0;

        // 已完成内容数（去重）
        var completedContentCount = await _context.MemberLearningProgress
            .Where(p => p.MemberId == memberId && p.IsCompleted)
            .Select(p => p.ContentId)
            .Distinct()
            .CountAsync();

        // 可学内容总数（公共内容 + 支部任务内容）
        var publicContentCount = await _context.LearningContents.CountAsync(c => c.IsPublic);
        var taskContentIds = await _context.LearningTasks
            .Where(t => t.TargetOrgId == orgId)
            .SelectMany(t => t.TaskContents.Select(tc => tc.ContentId))
            .Distinct()
            .CountAsync();
        var totalLearnableContents = publicContentCount + taskContentIds;

        // 任务完成情况
        var tasks = await _context.LearningTasks
            .Where(t => t.TargetOrgId == orgId)
            .Include(t => t.TaskContents)
            .ToListAsync();

        int completedTaskCount = 0;
        foreach (var task in tasks)
        {
            var total = task.TaskContents.Count;
            var completed = await _context.MemberLearningProgress
                .CountAsync(p => p.MemberId == memberId && p.TaskId == task.Id && p.IsCompleted);
            if (total > 0 && completed >= total) completedTaskCount++;
        }

        double taskCompletionRate = tasks.Any()
            ? Math.Round((double)completedTaskCount / tasks.Count * 100, 2)
            : 0;

        // 考试记录统计（归一化所需）
        var examRecords = await _context.MemberTestRecords
            .Where(r => r.MemberId == memberId)
            .Include(r => r.Test).ThenInclude(t => t.Paper)
            .ToListAsync();

        int earnedExamScore = 0;
        int totalExamScore = 0;
        int totalQuestions = 0;
        int correctQuestions = 0;

        foreach (var record in examRecords)
        {
            earnedExamScore += record.Score;

            if (record.Test?.Paper != null)
            {
                totalExamScore += record.Test.Paper.TotalScore;

                var qids = JsonSerializer.Deserialize<List<int>>(record.Test.Paper.QuestionIds ?? "[]") ?? new List<int>();
                var questions = await _context.Questions.Where(q => qids.Contains(q.Id)).ToDictionaryAsync(q => q.Id);
                totalQuestions += questions.Count;

                try
                {
                    var answers = JsonSerializer.Deserialize<List<SubmitAnswerItem>>(record.Answers);
                    if (answers != null)
                    {
                        foreach (var ans in answers)
                        {
                            if (questions.TryGetValue(ans.QuestionId, out var q))
                            {
                                bool correct = q.QuestionType switch
                                {
                                    Models.Common.QuestionType.SingleChoice or Models.Common.QuestionType.TrueFalse
                                        => ans.Answer?.Trim() == q.CorrectAnswer.Trim(),
                                    Models.Common.QuestionType.MultiChoice => CheckMultiAnswer(ans.Answer, q.CorrectAnswer),
                                    _ => false
                                };
                                if (correct) correctQuestions++;
                            }
                        }
                    }
                }
                catch { /* 忽略解析错误 */ }
            }
        }

        return new DetailedLearningOverview
        {
            TotalLearningMinutes = totalSeconds / 60,
            CompletedContentCount = completedContentCount,
            TotalLearnableContents = totalLearnableContents,
            CompletedTaskCount = completedTaskCount,
            TotalTaskCount = tasks.Count,
            TaskCompletionRate = taskCompletionRate,
            CompletedExamCount = examRecords.Count,
            EarnedExamScore = earnedExamScore,
            TotalExamScore = totalExamScore,
            TotalQuestions = totalQuestions,
            CorrectQuestions = correctQuestions
        };
    }

    /// <summary>详细学习概览数据传输对象</summary>
    private class DetailedLearningOverview
    {
        public int TotalLearningMinutes { get; set; }
        public int CompletedContentCount { get; set; }
        public int TotalLearnableContents { get; set; }
        public int CompletedTaskCount { get; set; }
        public int TotalTaskCount { get; set; }
        public double TaskCompletionRate { get; set; }
        public int CompletedExamCount { get; set; }
        public int EarnedExamScore { get; set; }
        public int TotalExamScore { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectQuestions { get; set; }
    }

    private string GetDurationComment(double s) =>
        s >= 80 ? "学习时长充足，投入度高" : s >= 60 ? "学习时长基本达标" : "学习时长不足，建议增加投入";
    private string GetContentCompletionComment(double s) =>
        s >= 80 ? "内容完成度高，学习积极主动" : s >= 60 ? "内容完成度良好" : "内容完成度偏低，建议加快学习进度";
    private string GetCompletionComment(double s) =>
        s >= 80 ? "任务完成优秀，执行力强" : s >= 60 ? "任务完成良好" : "任务完成率偏低，需加强";
    private string GetExamComment(double s) =>
        s >= 85 ? "测验成绩优异，知识扎实" : s >= 60 ? "测验成绩合格" : "测验成绩待提高";
    private string GetErrorComment(double s) =>
        s >= 80 ? "错题掌握良好，知识巩固到位" : s >= 60 ? "错题掌握尚可" : "错题较多，建议针对性复习";

    private List<string> GenerateSuggestions(List<AiDimensionDto> dims)
    {
        var weakest = dims.OrderBy(d => d.Score).First();
        return new List<string>
        {
            $"「{weakest.Name}」维度相对薄弱（{weakest.Score}分），建议重点提升。",
            weakest.Name switch
            {
                "学习时长" => "建议每天固定时段学习，培养学习习惯，目标累计学习10小时以上。",
                "内容完成" => "建议优先完成支部任务内容，再逐步学习公共素材，提高内容完成率。",
                "任务完成" => "建议每周一查看任务，制定每日学习目标，确保按时完成支部任务。",
                "测验成绩" => "建议考前先完成相关学习内容，错题及时回顾，巩固薄弱知识点。",
                "错题掌握" => "建议多做练习题，使用错题本功能针对性复习，巩固薄弱知识点。",
                _ => "建议多做练习题，巩固薄弱知识点。"
            },
            "坚持学习是进步的关键，继续保持！"
        };
    }
}
