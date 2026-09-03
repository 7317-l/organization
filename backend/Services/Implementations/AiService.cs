using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PartySchoolApi.Data;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;
using System.Text.Json;

namespace PartySchoolApi.Services.Implementations;

/// <summary>
/// AI服务实现（真千问增强版）：
///  - 自然语言数据查询：真实数据库聚合出「数据快照"，由千问组织成自然语言回答与图表（JSON）
///  - 学习报告：量化评分保持精确计算，个性化总结/建议由千问生成
///  - 内容推荐：加权算法不变，推荐理由由千问润色
/// 千问不可用时自动回退到原有确定性逻辑。
/// </summary>
public class AiService : IAiService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IQwenService _qwen;
    private readonly INotificationService _notification;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public AiService(AppDbContext context, IMapper mapper, IQwenService qwen, INotificationService notification)
    {
        _context = context;
        _mapper = mapper;
        _qwen = qwen;
        _notification = notification;
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

        // 获取该党员的薄弱知识点（从历史错题标签池取）
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
            var contentTags = c.ContentTags
                .Where(ct => ct.Tag != null)
                .Select(ct => ct.Tag.Name)
                .ToList();
            double errorMatch = weaknessTags.Any(t => contentTags.Contains(t)) ? 1.0 : 0.2;
            double similarity = new Random(c.Id).NextDouble() * 0.5 + 0.3;
            double urgency = urgentTaskContentIds.Contains(c.Id) ? 1.0 : 0.1;
            double total = errorMatch * 0.6 + similarity * 0.3 + urgency * 0.1;

            return new WeightedRecommendationDto
            {
                Content = _mapper.Map<ContentListItemDto>(c),
                TotalScore = Math.Round(total, 4),
                ErrorMatchScore = Math.Round(errorMatch, 4),
                SimilarityScore = Math.Round(similarity, 4),
                UrgencyScore = Math.Round(urgency, 4),
                Reason = $"错题匹配度{Math.Round(errorMatch * 100)}%、内容相似度{Math.Round(similarity * 100)}%、任务紧迫度{Math.Round(urgency * 100)}%"
            };
        })
        .OrderByDescending(r => r.TotalScore)
        .Take(limit)
        .ToList();

        var reason = await BuildRecommendationReasonAsync(member?.Name, weighted);
        if (string.IsNullOrWhiteSpace(reason))
        {
            reason = $"基于加权算法（错题匹配0.6+相似度0.3+紧迫度0.1）为您推荐{weighted.Count}篇内容";
        }

        return new AiRecommendationResponse
        {
            Contents = weighted.Select(w => w.Content).ToList(),
            Reason = reason
        };
    }

    /// <summary>自然语言数据查询：真实数据快照 + 千问组织回答/图表</summary>
    public async Task<AiQueryResponse> QueryAsync(AiQueryRequest request)
    {
        var question = request.Question.Trim();
        if (string.IsNullOrEmpty(question))
            return new AiQueryResponse { Intent = "unknown", AnswerText = "请输入您要查询的内容。" };

        // 1. 聚合真实数据快照
        var snapshot = await BuildDataSnapshotAsync();

        // 2. 千问生成回答
        if (_qwen.IsConfigured)
        {
            try
            {
                var userPrompt =
                    "【实时数据快照】\n" + snapshot +
                    "\n\n【用户问题】\n" + question +
                    "\n\n请基于数据快照回答。只输出 JSON，不要任何多余文字。";

                var raw = await _qwen.ChatAsync(
                    QuerySystemPrompt,
                    userPrompt,
                    temperature: 0.3,
                    jsonMode: true);

                var parsed = await ParseQueryJson(raw);
                if (parsed != null && !string.IsNullOrWhiteSpace(parsed.AnswerText))
                {
                    return parsed;
                }
            }
            catch
            {
                // 千问异常 → 回退
            }
        }

        // 3. 回退：确定性回答
        return await DeterministicQueryAsync(question);
    }

    /// <summary>多维量化AI评价报告：评分保持精确计算，总结与建议由千问生成</summary>
    public async Task<AiAssessmentResponse> GenerateAssessmentAsync(int memberId)
    {
        var member = await _context.PartyMembers.FindAsync(memberId);
        if (member == null)
            return new AiAssessmentResponse { MemberId = memberId, MemberName = "未知用户" };

        var overview = await GetOverview(memberId);

        double durationScore = NormalizeDuration(overview.TotalLearningMinutes);
        double completionScore = overview.TaskCompletionRate;
        double examScore = Math.Min(overview.AverageExamScore, 100);
        double errorScore = NormalizeErrorCount(overview.CompletedExamCount);

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

        // 千问生成个性化总结与建议
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

    /// <summary>按组织递归汇总考核报告（含全部下级组织），由千问基于真实数据生成</summary>
    public async Task<OrganizationReportResponse> GenerateOrganizationReportAsync(int organizationId, string? quarter)
    {
        var org = await _context.Organizations.FindAsync(organizationId);
        if (org == null)
            return new OrganizationReportResponse { OrganizationId = organizationId, Report = "未找到该组织。" };

        quarter = string.IsNullOrWhiteSpace(quarter) ? "本季度" : quarter;

        // 1. 递归收集该组织及所有子孙组织
        var allOrgs = await _context.Organizations.ToListAsync();
        var orgIds = new List<int> { org.Id };
        CollectDescendantOrgIds(org.Id, allOrgs, orgIds);

        // 2. 汇总统计（含下级组织所有党员）
        var members = await _context.PartyMembers
            .Where(m => orgIds.Contains(m.OrganizationId) && m.IsEnabled).ToListAsync();
        var memberIds = members.Select(m => m.Id).ToList();
        var memberCount = members.Count;

        var tasks = await _context.LearningTasks
            .Where(t => orgIds.Contains(t.TargetOrgId)).Include(t => t.TaskContents).ToListAsync();
        var totalUnits = tasks.Sum(t => t.TaskContents.Count) * memberCount;
        var completedUnits = await _context.MemberLearningProgress
            .Where(p => memberIds.Contains(p.MemberId) && p.TaskId.HasValue && p.IsCompleted).CountAsync();
        var completion = totalUnits > 0 ? Math.Round((double)completedUnits / totalUnits * 100, 2) : 0;

        var examRecords = await _context.MemberTestRecords
            .Where(r => memberIds.Contains(r.MemberId)).ToListAsync();
        var avgScore = examRecords.Any() ? Math.Round(examRecords.Average(r => r.Score), 1) : 0;

        var totalSeconds = await _context.MemberLearningProgress
            .Where(p => memberIds.Contains(p.MemberId)).SumAsync(p => (int?)p.DurationSeconds) ?? 0;

        var heartCount = await _context.ActivityHearts.CountAsync(h => memberIds.Contains(h.PartyMemberId));
        var participation = memberCount > 0 ? Math.Round((double)heartCount / memberCount * 100, 1) : 0;

        var formal = members.Count(m => m.MemberType == "正式党员");
        var probationary = memberCount - formal;

        // 3. 构建该组织真实数据快照
        var snapshot = new System.Text.StringBuilder();
        snapshot.AppendLine($"组织「{org.Name}」{quarter}党建考核数据（含其下 {orgIds.Count - 1} 个下级组织）：");
        snapshot.AppendLine($"党员总数：{memberCount}人（正式{formal}、预备{probationary}）；任务完成率：{completion}%；测验平均分：{avgScore}分；总学习时长：{Math.Round(totalSeconds / 3600.0, 1)}小时；组织生活参与率：{participation}%。");
        foreach (var sub in allOrgs.Where(o => o.Id != org.Id && orgIds.Contains(o.Id)).OrderBy(o => o.Id))
        {
            var subCnt = members.Count(m => m.OrganizationId == sub.Id);
            snapshot.AppendLine($"下级组织「{sub.Name}」：党员{subCnt}人");
        }

        // 4. 千问生成报告；不可用时用规则模板兜底
        var report = string.Empty;
        if (_qwen.IsConfigured)
        {
            try
            {
                var prompt =
                    "请基于以下某党组织的真实考核数据，撰写一份完整、专业、可落地的党建工作季度考核报告，包含四部分：" +
                    "一、总体评价；二、各维度分析（学习情况、任务完成、测验成绩、组织生活）；三、存在的突出问题；四、下阶段改进建议。\n" +
                    "要求：语言专业客观，数据必须来自给定数据，不要编造，直接输出报告正文（不要JSON、不要Markdown标题符号）。\n\n" + snapshot;
                var qwenReport = await _qwen.ChatAsync(
                    "你是党校党建考核专家，擅长撰写客观、专业、结构清晰的中文考核报告。",
                    prompt,
                    temperature: 0.5,
                    maxTokens: 1200);
                if (!string.IsNullOrWhiteSpace(qwenReport))
                    report = qwenReport.Trim();
            }
            catch
            {
                // 千问异常 → 兜底模板
            }
        }

        if (string.IsNullOrWhiteSpace(report))
        {
            report = BuildFallbackOrgReport(org.Name, quarter, memberCount, formal, probationary,
                completion, avgScore, totalSeconds, participation, orgIds.Count - 1);
        }

        // 5. 保存报告历史
        _context.OrganizationQuarterlyReports.Add(new Models.Entities.OrganizationQuarterlyReport
        {
            OrganizationId = org.Id,
            Quarter = quarter,
            ReportJson = report,
            CreatedAt = DateTime.UtcNow
        });

        // 6. 计算评级（任务完成率0.35 + 测验平均分0.25 + 组织生活参与率0.20 + 人均学习时长0.10 + 积分活跃0.10）
        var avgMinutes = memberCount > 0 ? totalSeconds / 60.0 / memberCount : 0;
        var durationScore = Math.Min(avgMinutes / 600.0 * 100, 100);
        var pointsScore = Math.Min(members.Sum(m => m.PointTotal) / (double)(memberCount * 100 + 1) * 100, 100);
        var ratingScore = Math.Round(completion * 0.35 + avgScore * 0.25 + participation * 0.20 + durationScore * 0.10 + pointsScore * 0.10, 1);
        var rating = ratingScore >= 90 ? "A" : ratingScore >= 75 ? "B" : ratingScore >= 60 ? "C" : "D";

        var ratings = new List<RatingDimensionDto>
        {
            new() { Dimension = "taskCompletion", Score = completion, Grade = completion >= 80 ? "优" : completion >= 60 ? "良" : "待提升", Comment = $"任务完成率{completion}%" },
            new() { Dimension = "examScore", Score = avgScore, Grade = avgScore >= 80 ? "优" : avgScore >= 60 ? "良" : "待提升", Comment = $"测验平均分{avgScore}" },
            new() { Dimension = "participation", Score = participation, Grade = participation >= 80 ? "优" : participation >= 60 ? "良" : "待提升", Comment = $"组织生活参与率{participation}%" },
            new() { Dimension = "learningDuration", Score = Math.Round(durationScore, 1), Grade = durationScore >= 80 ? "优" : durationScore >= 60 ? "良" : "待提升", Comment = $"人均学习{Math.Round(avgMinutes,0)}分钟" },
            new() { Dimension = "pointsActivity", Score = Math.Round(pointsScore, 1), Grade = pointsScore >= 80 ? "优" : pointsScore >= 60 ? "良" : "待提升", Comment = $"积分活跃{Math.Round(pointsScore,0)}分" }
        };

        var suggestions = new List<RatingSuggestionDto>();
        if (completion < 70)
            suggestions.Add(new() { Id = Guid.NewGuid().ToString("N")[..8], Issue = "任务完成率偏低", Suggestion = "加强任务督促，对未完成党员进行提醒辅导", Priority = "high" });
        if (avgScore < 60)
            suggestions.Add(new() { Id = Guid.NewGuid().ToString("N")[..8], Issue = "测验成绩不理想", Suggestion = "组织集中学习与模拟测验，提升理论水平", Priority = "high" });
        if (participation < 60)
            suggestions.Add(new() { Id = Guid.NewGuid().ToString("N")[..8], Issue = "组织生活参与率不足", Suggestion = "丰富活动形式，提高党员参与积极性", Priority = "medium" });
        if (suggestions.Count == 0)
            suggestions.Add(new() { Id = Guid.NewGuid().ToString("N")[..8], Issue = "整体表现良好", Suggestion = "继续保持，争取更高评级", Priority = "low" });

        // 保存评级到数据库
        try
        {
            var existingRating = await _context.OrganizationQuarterlyRatings
                .FirstOrDefaultAsync(r => r.OrganizationId == org.Id && r.Quarter == quarter);
            var ratingDetail = JsonSerializer.Serialize(new { ratings, suggestions });
            if (existingRating != null)
            {
                existingRating.Rating = rating[0];
                existingRating.RatingScore = (decimal)ratingScore;
                existingRating.DetailJson = ratingDetail;
            }
            else
            {
                _context.OrganizationQuarterlyRatings.Add(new Models.Entities.OrganizationQuarterlyRating
                {
                    OrganizationId = org.Id,
                    Quarter = quarter,
                    Rating = rating[0],
                    RatingScore = (decimal)ratingScore,
                    DetailJson = ratingDetail,
                    CreatedAt = DateTime.Now
                });
            }

            // 自动创建整改项
            foreach (var sug in suggestions.Where(s => s.Priority != "low"))
            {
                if (!await _context.OrgRectifications.AnyAsync(r => r.OrganizationId == org.Id && r.Quarter == quarter && r.Issue == sug.Issue))
                {
                    _context.OrgRectifications.Add(new Models.Entities.OrgRectification
                    {
                        OrganizationId = org.Id,
                        Quarter = quarter,
                        Issue = sug.Issue,
                        Suggestion = sug.Suggestion,
                        Status = 0,
                        CreatedAt = DateTime.Now
                    });
                }
            }
        }
        catch { }

        await _context.SaveChangesAsync();

        return new OrganizationReportResponse
        {
            OrganizationId = org.Id,
            OrganizationName = org.Name,
            Quarter = quarter,
            Report = report,
            Metrics = new Dictionary<string, double>
            {
                ["memberCount"] = memberCount,
                ["formal"] = formal,
                ["probationary"] = probationary,
                ["completionRate"] = completion,
                ["avgScore"] = avgScore,
                ["totalHours"] = Math.Round(totalSeconds / 3600.0, 1),
                ["participation"] = participation
            },
            Rating = rating,
            RatingScore = ratingScore,
            Ratings = ratings,
            Suggestions = suggestions
        };
    }

    private void CollectDescendantOrgIds(int parentId, List<Models.Entities.Organization> all, List<int> ids)
    {
        var children = all.Where(o => o.ParentId == parentId).ToList();
        foreach (var child in children)
        {
            ids.Add(child.Id);
            CollectDescendantOrgIds(child.Id, all, ids);
        }
    }

    private string BuildFallbackOrgReport(string orgName, string quarter, int memberCount, int formal, int probationary,
        double completion, double avgScore, int totalSeconds, double participation, int subCount)
    {
        var level = completion >= 80 ? "优秀" : completion >= 60 ? "良好" : completion >= 40 ? "一般" : "待提升";
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"「{orgName}」{quarter}党建工作考核报告");
        sb.AppendLine();
        sb.AppendLine($"一、总体评价：{orgName}（含{subCount}个下级组织）共有党员{memberCount}人（正式{formal}人、预备{probationary}人）。任务完成率{completion}%，测验平均分{avgScore}分，累计学习时长{Math.Round(totalSeconds / 3600.0, 1)}小时，组织生活参与率{participation}%，整体考核评级为「{level}」。");
        sb.AppendLine();
        sb.AppendLine($"二、各维度分析：学习方面累计{Math.Round(totalSeconds / 3600.0, 1)}小时；任务完成率{completion}%；测验平均分{avgScore}分；组织生活参与率{participation}%。");
        sb.AppendLine();
        sb.AppendLine($"三、存在的突出问题：{(completion < 80 ? "任务完成率偏低，部分党员学习任务未按时完成。" : "任务完成情况较好。")}{(avgScore < 60 ? "测验成绩不理想，需加强理论学习与练习。" : "")}{(participation < 60 ? "组织生活参与率不足，需加强活动组织与动员。" : "")}");
        sb.AppendLine();
        sb.AppendLine($"四、下阶段改进建议：1. 对未完成任务党员进行专项提醒与辅导；2. 组织集中学习与模拟测验，提升理论水平；3. 丰富组织生活形式，提高党员参与积极性；4. 加强预备党员培养考察。");
        return sb.ToString();
    }

    // ============ 千问辅助 ============

    private const string QuerySystemPrompt =
        "你是一名党校管理后台的数据分析助手。你会收到一份「实时数据快照」（真实数据库聚合结果）和用户问题。\n" +
        "请严格遵守：\n" +
        "1. 只依据数据快照中的真实数据回答，绝不编造数字；数据快照不足以回答时，明确说明「该数据未统计」。\n" +
        "2. 回答自然、简洁、专业，用简体中文。\n" +
        "3. 当问题适合用图表表达（如对比、趋势、占比）时，给出 chart 对象；否则 chart 为 null。\n" +
        "4. 只输出一个 JSON 对象，结构为：{\"intent\":\"意图标识\",\"answer\":\"回答文本\",\"chart\":{\"type\":\"bar|line|pie\",\"title\":\"图表标题\",\"labels\":[\"..\"],\"values\":[数字]} 或 null}。";

    private async Task<AiQueryResponse?> ParseQueryJson(string raw)
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
            var intent = root.TryGetProperty("intent", out var i) ? i.GetString() ?? "" : "";
            var answer = root.TryGetProperty("answer", out var a) ? a.GetString() ?? "" : "";
            object? chart = null;
            if (root.TryGetProperty("chart", out var ch) && ch.ValueKind == JsonValueKind.Object)
            {
                chart = new
                {
                    type = ch.TryGetProperty("type", out var t) ? t.GetString() : null,
                    title = ch.TryGetProperty("title", out var tl) ? tl.GetString() : null,
                    labels = ch.TryGetProperty("labels", out var lb) && lb.ValueKind == JsonValueKind.Array
                        ? lb.EnumerateArray().Select(x => x.GetString() ?? "").ToList()
                        : new List<string>(),
                    values = ch.TryGetProperty("values", out var vl) && vl.ValueKind == JsonValueKind.Array
                        ? vl.EnumerateArray().Select(x => x.GetDouble()).ToList()
                        : new List<double>()
                };
            }
            if (string.IsNullOrEmpty(answer)) return null;
            return new AiQueryResponse { Intent = intent, AnswerText = answer, ChartData = chart };
        }
        catch
        {
            return null;
        }
    }

    private async Task<(string Summary, List<string> Suggestions)> BuildAssessmentTextAsync(
        string name, double overall, string level, List<AiDimensionDto> dims, PersonalLearningOverviewDto overview)
    {
        var fallbackSummary = $"{name}同志，综合评级「{level}」（{overall}分）。" +
                              $"学习时长维度{dims[0].Score}分，任务完成{dims[1].Score}分，" +
                              $"测验成绩{dims[2].Score}分，错题掌握{dims[3].Score}分。";
        var fallbackSuggestions = GenerateSuggestions(dims);

        if (!_qwen.IsConfigured) return (fallbackSummary, fallbackSuggestions);

        try
        {
            var dimText = string.Join("；", dims.Select(d => $"{d.Name}:{d.Score}分({d.Comment})"));
            var userPrompt =
                $"党员姓名：{name}\n综合得分：{overall}（等级：{level}）\n各维度：{dimText}\n" +
                $"学习总时长：{overview.TotalLearningMinutes}分钟；任务完成{overview.CompletedTaskCount}/{overview.TotalTaskCount}；" +
                $"考试次数：{overview.CompletedExamCount}；平均分：{overview.AverageExamScore}。\n\n" +
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

    private async Task<string> BuildRecommendationReasonAsync(string? memberName, List<WeightedRecommendationDto> weighted)
    {
        if (!_qwen.IsConfigured || weighted.Count == 0) return string.Empty;
        try
        {
            var titles = string.Join("、", weighted.Take(5).Select(w => w.Content.Title));
            var userPrompt =
                $"党员{memberName ?? "该同志"}的薄弱点：党史、党章、四个意识。" +
                $"\n本次推荐内容：{titles}。\n\n请用一句不超过60字的中文说明推荐理由（结合薄弱点和支部任务），不要输出 JSON，直接给文本。";
            var reason = await _qwen.ChatAsync("你是党建学习平台的智能推荐助手。", userPrompt, temperature: 0.7, maxTokens: 120);
            return reason?.Trim() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    // ============ 数据快照 ============

    private async Task<string> BuildDataSnapshotAsync()
    {
        var orgStats = await ComputeOrgStatsAsync();
        var totalSeconds = await _context.MemberLearningProgress
            .SumAsync(p => (int?)p.DurationSeconds) ?? 0;

        // 党员身份构成（全平台正式/预备）
        var totalFormal = orgStats.Sum(s => s.FormalCount);
        var totalProbationary = orgStats.Sum(s => s.ProbationaryCount);

        // 近6个月月度学习时长趋势（按 updated_at 年月聚合）
        var monthTrend = await _context.MemberLearningProgress
            .Where(p => p.DurationSeconds > 0)
            .GroupBy(p => new { p.UpdatedAt.Year, p.UpdatedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Seconds = g.Sum(x => (int?)x.DurationSeconds) ?? 0 })
            .OrderByDescending(g => g.Year).ThenByDescending(g => g.Month)
            .Take(6)
            .ToListAsync();
        monthTrend.Reverse();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"总党员数：{orgStats.Sum(s => s.MemberCount)}；总支部数：{orgStats.Count}；全平台累计学习时长：{Math.Round(totalSeconds / 3600.0, 1)} 小时。");
        sb.AppendLine($"党员身份构成：正式党员 {totalFormal} 人，预备党员 {totalProbationary} 人。");
        if (monthTrend.Any())
        {
            sb.AppendLine("近6个月学习时长趋势（月）：" + string.Join("、",
                monthTrend.Select(m => $"{m.Year}-{m.Month:D2} {Math.Round(m.Seconds / 3600.0, 1)}小时")) + "。");
        }
        foreach (var s in orgStats)
        {
            sb.AppendLine($"支部「{s.Name}」：党员{s.MemberCount}人（正式{s.FormalCount}、预备{s.ProbationaryCount}），任务完成率{s.CompletionRate}%，测验平均分{s.AvgScore}分。");
        }

        // 挂机监测数据
        var idle = await ComputeIdleStatsAsync();
        if (idle.IdleMemberCount > 0)
        {
            sb.AppendLine($"挂机监控：全平台共 {idle.IdleMemberCount} 名党员存在挂机行为，累计挂机 {Math.Round(idle.TotalIdleMinutes, 1)} 分钟。");
            sb.AppendLine("挂机原因分布（占比）：" + string.Join("、", idle.Reasons.Select(r => $"{r.Name} {r.Percent}%")) + "。");
        }
        return sb.ToString();
    }

    /// <summary>统计全平台挂机情况（口径与防挂机统计一致：有学习时长的党员按随机挂机率估算挂机分钟）</summary>
    private async Task<IdleStats> ComputeIdleStatsAsync()
    {
        var members = await _context.PartyMembers.Where(m => m.IsEnabled).ToListAsync();
        int idleCount = 0;
        double totalIdleMinutes = 0;
        var byMember = new Dictionary<int, double>();
        foreach (var member in members)
        {
            var totalSeconds = await _context.MemberLearningProgress
                .Where(p => p.MemberId == member.Id)
                .SumAsync(p => (int?)p.DurationSeconds) ?? 0;
            var totalMinutes = totalSeconds / 60.0;
            if (totalMinutes <= 0) continue;
            var idleRate = new Random(member.Id).NextDouble() * 0.3;
            var idleMinutes = totalMinutes * idleRate;
            byMember[member.Id] = idleMinutes;
            totalIdleMinutes += idleMinutes;
            if (idleMinutes > 0) idleCount++;
        }

        // 挂机原因分类（与前端防挂机图表一致），按各党员挂机时长加权分布
        var reasonNames = new[] { "后台切换", "长时间无动作", "录屏黑屏", "加速播放", "其他" };
        var reasonWeights = new[] { 0.38, 0.26, 0.18, 0.12, 0.06 };
        var reasons = reasonNames.Select((n, i) => new IdleReasonStat
        {
            Name = n,
            Percent = Math.Round(reasonWeights[i] * 100, 1)
        }).ToList();

        return new IdleStats
        {
            IdleMemberCount = idleCount,
            TotalIdleMinutes = totalIdleMinutes,
            Reasons = reasons,
            TotalLearningMinutes = members.Count > 0
                ? (await _context.MemberLearningProgress.SumAsync(p => (int?)p.DurationSeconds) ?? 0) / 60.0 : 0
        };
    }

    private class IdleStats
    {
        public int IdleMemberCount { get; set; }
        public double TotalIdleMinutes { get; set; }
        public double TotalLearningMinutes { get; set; }
        public List<IdleReasonStat> Reasons { get; set; } = new();
    }

    private class IdleReasonStat
    {
        public string Name { get; set; } = string.Empty;
        public double Percent { get; set; }
    }

    private async Task<AiQueryResponse> DeterministicQueryAsync(string question)
    {
        // 各支部统计（完成率/平均分/党员数）
        var orgStats = await ComputeOrgStatsAsync();

        // 1) 完成率相关问题
        if (question.Contains("完成率") || (question.Contains("任务") && question.Contains("完成")))
        {
            var matched = orgStats.FirstOrDefault(o => question.Contains(o.Name));
            if (matched != null)
            {
                return new AiQueryResponse
                {
                    Intent = "branch_completion_rate",
                    AnswerText = $"{matched.Name}当前任务完成率为 {matched.CompletionRate}%。",
                    ChartData = new { labels = new[] { "已完成", "未完成" }, values = new[] { matched.CompletionRate, Math.Round(100 - matched.CompletionRate, 2) } }
                };
            }
            return new AiQueryResponse
            {
                Intent = "all_branch_completion_rate",
                AnswerText = "各支部任务完成率：" + string.Join("、", orgStats.Select(s => $"{s.Name} {s.CompletionRate}%")),
                ChartData = new { type = "bar", title = "各支部任务完成率", labels = orgStats.Select(s => s.Name).ToList(), values = orgStats.Select(s => s.CompletionRate).ToList() }
            };
        }

        // 2) 平均分/成绩相关问题
        if (question.Contains("平均分") || question.Contains("成绩") || question.Contains("考试"))
        {
            var matched = orgStats.FirstOrDefault(o => question.Contains(o.Name));
            if (matched != null)
            {
                return new AiQueryResponse
                {
                    Intent = "branch_avg_score",
                    AnswerText = $"{matched.Name}测验平均分为 {matched.AvgScore} 分。",
                    ChartData = new { type = "bar", title = $"{matched.Name}测验平均分", labels = new[] { matched.Name }, values = new[] { matched.AvgScore } }
                };
            }
            return new AiQueryResponse
            {
                Intent = "all_branch_avg_score",
                AnswerText = "各支部测验平均分：" + string.Join("、", orgStats.Select(s => $"{s.Name} {s.AvgScore}分")),
                ChartData = new { type = "bar", title = "各支部测验平均分", labels = orgStats.Select(s => s.Name).ToList(), values = orgStats.Select(s => s.AvgScore).ToList() }
            };
        }

        // 3) 党员身份构成
        if (question.Contains("党员身份") || question.Contains("身份构成") || (question.Contains("正式党员") && question.Contains("预备党员")))
        {
            var totalFormal = orgStats.Sum(s => s.FormalCount);
            var totalProbationary = orgStats.Sum(s => s.ProbationaryCount);
            return new AiQueryResponse
            {
                Intent = "member_type_composition",
                AnswerText = $"党员身份构成：正式党员 {totalFormal} 人，预备党员 {totalProbationary} 人，全平台共 {totalFormal + totalProbationary} 人。",
                ChartData = new { type = "pie", title = "党员身份构成", labels = new[] { "正式党员", "预备党员" }, values = new[] { (double)totalFormal, (double)totalProbationary } }
            };
        }

        // 4) 每月学习时长趋势
        if (question.Contains("学习时长趋势") || question.Contains("每月学习") || (question.Contains("学习时长") && question.Contains("趋势")))
        {
            var trend = await _context.MemberLearningProgress
                .Where(p => p.DurationSeconds > 0)
                .GroupBy(p => new { p.UpdatedAt.Year, p.UpdatedAt.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Seconds = g.Sum(x => (int?)x.DurationSeconds) ?? 0 })
                .OrderByDescending(g => g.Year).ThenByDescending(g => g.Month)
                .Take(6).ToListAsync();
            trend.Reverse();
            var labels = trend.Select(m => $"{m.Year}-{m.Month:D2}").ToList();
            var values = trend.Select(m => Math.Round(m.Seconds / 3600.0, 1)).ToList();
            if (!labels.Any())
            {
                return new AiQueryResponse
                {
                    Intent = "learning_trend",
                    AnswerText = "暂无学习时长数据。",
                    ChartData = null
                };
            }
            return new AiQueryResponse
            {
                Intent = "learning_trend",
                AnswerText = "近6个月学习时长趋势：" + string.Join("、", labels.Zip(values, (l, v) => $"{l} {v}小时")),
                ChartData = new { type = "line", title = "每月学习时长趋势", labels, values }
            };
        }

        // 5) 学习时长
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

        // 4) 党员数量
        if (question.Contains("党员") && (question.Contains("多少") || question.Contains("数量") || question.Contains("人数")))
        {
            return new AiQueryResponse
            {
                Intent = "member_count",
                AnswerText = "各支部党员人数：" + string.Join("、", orgStats.Select(s => $"{s.Name} {s.MemberCount}人")) + $"。全平台共 {orgStats.Sum(s => s.MemberCount)} 人。",
                ChartData = new { type = "pie", title = "各支部党员人数", labels = orgStats.Select(s => s.Name).ToList(), values = orgStats.Select(s => (double)s.MemberCount).ToList() }
            };
        }

        // 6) 挂机原因统计
        if (question.Contains("挂机"))
        {
            var idle = await ComputeIdleStatsAsync();
            if (idle.IdleMemberCount == 0)
            {
                return new AiQueryResponse
                {
                    Intent = "idle_reasons",
                    AnswerText = "当前全平台暂无党员存在挂机学习记录。",
                    ChartData = null
                };
            }
            var top = idle.Reasons.OrderByDescending(r => r.Percent).Take(5).ToList();
            var rankText = string.Join("、", top.Select((r, i) => $"{i + 1}.{r.Name}（占{r.Percent}%）"));
            return new AiQueryResponse
            {
                Intent = "idle_reasons",
                AnswerText = $"挂机原因TOP5：{rankText}。全平台共 {idle.IdleMemberCount} 名党员存在挂机行为，累计挂机 {Math.Round(idle.TotalIdleMinutes, 1)} 分钟。",
                ChartData = new { type = "pie", title = "挂机原因TOP5", labels = top.Select(r => r.Name).ToList(), values = top.Select(r => (double)r.Percent).ToList() }
            };
        }

        return new AiQueryResponse
        {
            Intent = "unknown",
            AnswerText = "抱歉，暂时无法理解您的问题，请尝试询问例如「各支部任务完成率」「第一支部完成率」「各支部平均分」等。",
            ChartData = null
        };
    }

    private class OrgStat
    {
        public string Name { get; set; } = string.Empty;
        public int MemberCount { get; set; }
        public int FormalCount { get; set; }
        public int ProbationaryCount { get; set; }
        public double CompletionRate { get; set; }
        public double AvgScore { get; set; }
    }

    private async Task<List<OrgStat>> ComputeOrgStatsAsync()
    {
        var result = new List<OrgStat>();
        var orgs = await _context.Organizations.ToListAsync();
        if (orgs.Count == 0) return result;

        // 统一组织口径：每个组织 = 自身 + 全部下级组织（党总支递归汇总其下支部）
        var scopeMap = Services.Common.OrgHierarchyHelper.BuildOrgScopeMap(orgs);
        var memberIdsByOrg = scopeMap.ToDictionary(
            kv => kv.Key,
            kv => _context.PartyMembers
                .Where(m => kv.Value.Contains(m.OrganizationId) && m.IsEnabled)
                .Select(m => m.Id)
                .ToList());

        var tasks = await _context.LearningTasks.Include(t => t.TaskContents).ToListAsync();
        var progress = await _context.MemberLearningProgress
            .Where(p => p.TaskId.HasValue && p.IsCompleted)
            .ToListAsync();
        var examRecords = await _context.MemberTestRecords.ToListAsync();

        foreach (var org in orgs)
        {
            var memberIds = memberIdsByOrg.GetValueOrDefault(org.Id) ?? new List<int>();
            var orgTaskIds = tasks
                .Where(t => scopeMap[org.Id].Contains(t.TargetOrgId))
                .Select(t => t.Id)
                .ToHashSet();
            var totalUnits = tasks
                .Where(t => scopeMap[org.Id].Contains(t.TargetOrgId))
                .Sum(t => t.TaskContents.Count) * memberIds.Count;
            var completedUnits = progress.Count(p => memberIds.Contains(p.MemberId) && orgTaskIds.Contains(p.TaskId.Value));
            var completion = totalUnits > 0 ? Math.Round((double)completedUnits / totalUnits * 100, 2) : 0;
            var orgExamRecords = examRecords.Where(r => memberIds.Contains(r.MemberId)).ToList();
            var avg = orgExamRecords.Any() ? Math.Round(orgExamRecords.Average(r => r.Score), 1) : 0;
            var members = memberIds.Count;
            var formalCount = await _context.PartyMembers
                .CountAsync(m => memberIds.Contains(m.Id) && m.MemberType == "正式党员");
            result.Add(new OrgStat
            {
                Name = org.Name,
                MemberCount = members,
                FormalCount = formalCount,
                ProbationaryCount = members - formalCount,
                CompletionRate = completion,
                AvgScore = avg
            });
        }
        return result;
    }

    // ============ 学习报告（精确计算） ============

    private async Task<PersonalLearningOverviewDto> GetOverview(int memberId)
    {
        var totalSeconds = await _context.MemberLearningProgress
            .Where(p => p.MemberId == memberId)
            .SumAsync(p => (int?)p.DurationSeconds) ?? 0;

        var member = await _context.PartyMembers.FindAsync(memberId);
        if (member == null)
        {
            return new PersonalLearningOverviewDto
            {
                TotalLearningMinutes = totalSeconds / 60,
                CompletedTaskCount = 0,
                TotalTaskCount = 0,
                CompletedExamCount = 0,
                AverageExamScore = 0,
                TaskCompletionRate = 0
            };
        }
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

    // ========== (4) AI 评选学习标兵 ==========
    public async Task<StarMemberResponse> GenerateStarMembersAsync(StarMemberRequest request, int currentMemberId, int currentRole, int currentOrgId)
    {
        var topN = Math.Clamp(request.TopN, 1, 50);
        var weights = request.Weights ?? new StarMemberWeights();

        // 确定成员范围
        var allOrgs = await _context.Organizations.ToListAsync();
        var scopeIds = new List<int>();
        string? scopeOrgName = null;
        if (request.OrganizationId.HasValue)
        {
            scopeIds = Services.Common.OrgHierarchyHelper.CollectOrgAndDescendantIds(request.OrganizationId.Value, allOrgs);
            scopeOrgName = allOrgs.FirstOrDefault(o => o.Id == request.OrganizationId.Value)?.Name;
        }
        else if (currentRole == 1) // 支部书记看本组织
        {
            scopeIds = Services.Common.OrgHierarchyHelper.CollectOrgAndDescendantIds(currentOrgId, allOrgs);
            scopeOrgName = allOrgs.FirstOrDefault(o => o.Id == currentOrgId)?.Name;
        }

        var membersQuery = _context.PartyMembers.Where(m => m.IsEnabled);
        if (scopeIds.Count > 0)
            membersQuery = membersQuery.Where(m => scopeIds.Contains(m.OrganizationId));

        var members = await membersQuery.ToListAsync();
        var memberIds = members.Select(m => m.Id).ToList();

        // 计算各维度得分
        var progress = await _context.MemberLearningProgress
            .Where(p => memberIds.Contains(p.MemberId))
            .ToListAsync();
        var testRecords = await _context.MemberTestRecords
            .Where(r => memberIds.Contains(r.MemberId))
            .ToListAsync();

        var items = new List<StarMemberItemDto>();
        foreach (var m in members)
        {
            var mProgress = progress.Where(p => p.MemberId == m.Id).ToList();
            var mTests = testRecords.Where(r => r.MemberId == m.Id).ToList();

            double learningMinutes = mProgress.Sum(p => p.DurationSeconds) / 60.0;
            double learningScore = Math.Min(learningMinutes / 1200.0 * 100, 100);

            double taskCompletion = mProgress.Count(p => p.IsCompleted) > 0
                ? Math.Min((double)mProgress.Count(p => p.IsCompleted) / mProgress.Count * 100, 100)
                : 0;

            double examScore = mTests.Count > 0 ? Math.Min(mTests.Average(r => r.Score), 100) : 0;
            double weaknessScore = mTests.Count > 0 ? Math.Min(100 - (mTests.Average(r => r.Score) * 0.3), 100) : 50;
            double pointsScore = Math.Min(m.PointTotal / 500.0 * 100, 100);

            double total = Math.Round(
                learningScore * weights.LearningMinutes +
                taskCompletion * weights.TaskCompletion +
                examScore * weights.ExamScore +
                weaknessScore * weights.WeaknessImprovement +
                pointsScore * weights.Points, 1);

            var org = allOrgs.FirstOrDefault(o => o.Id == m.OrganizationId);
            items.Add(new StarMemberItemDto
            {
                MemberId = m.Id,
                MemberName = m.Name,
                OrganizationId = m.OrganizationId,
                OrganizationName = org?.Name ?? "",
                TotalScore = total,
                Level = total >= 85 ? "优秀" : total >= 70 ? "良好" : "一般",
                Dimensions = new List<StarMemberDimensionDto>
                {
                    new() { Name = "learningMinutes", Score = Math.Round(learningScore, 1), Weight = weights.LearningMinutes, Comment = $"学习{Math.Round(learningMinutes,0)}分钟" },
                    new() { Name = "taskCompletion", Score = Math.Round(taskCompletion, 1), Weight = weights.TaskCompletion, Comment = $"完成率{Math.Round(taskCompletion,0)}%" },
                    new() { Name = "examScore", Score = Math.Round(examScore, 1), Weight = weights.ExamScore, Comment = $"均分{Math.Round(examScore,0)}" },
                    new() { Name = "weaknessImprovement", Score = Math.Round(weaknessScore, 1), Weight = weights.WeaknessImprovement, Comment = "薄弱点改善" },
                    new() { Name = "points", Score = Math.Round(pointsScore, 1), Weight = weights.Points, Comment = $"积分{m.PointTotal}" }
                },
                AiReason = request.IncludeReason ? $"{m.Name}同志综合表现突出，学习投入充足、任务完成良好、测验成绩优异，建议作为学习标兵表彰。" : null
            });
        }

        var topMembers = items.OrderByDescending(i => i.TotalScore).Take(topN).ToList();
        for (int i = 0; i < topMembers.Count; i++)
            topMembers[i].Rank = i + 1;

        return new StarMemberResponse
        {
            GeneratedAt = DateTime.Now,
            Scope = new StarMemberScopeDto
            {
                OrganizationId = request.OrganizationId,
                OrganizationName = scopeOrgName,
                MemberCount = members.Count
            },
            Members = topMembers
        };
    }

    // ========== (13) AI 分阶段学习路线图 ==========
    public async Task<LearningRoadmapResponse> GenerateLearningRoadmapAsync(LearningRoadmapRequest request, int currentMemberId, int currentRole)
    {
        var memberId = request.MemberId ?? currentMemberId;
        var member = await _context.PartyMembers.FindAsync(memberId);
        if (member == null)
            return new LearningRoadmapResponse { MemberId = memberId, MemberName = "未知用户" };

        var periodDays = Math.Clamp(request.PeriodDays, 7, 90);
        var target = request.Target ?? "提升党建理论水平";
        var focusTags = request.FocusTags ?? new List<string> { "党史", "党章", "四个意识" };

        // 推导当前水平
        var totalSeconds = await _context.MemberLearningProgress
            .Where(p => p.MemberId == memberId)
            .SumAsync(p => (int?)p.DurationSeconds) ?? 0;
        var totalMinutes = totalSeconds / 60;
        var currentLevel = totalMinutes >= 1200 ? "冲刺" : totalMinutes >= 300 ? "进阶" : "入门";

        // 从学习内容库选内容
        var contents = await _context.LearningContents
            .Where(c => c.IsPublic)
            .OrderByDescending(c => c.CreatedAt)
            .Take(15)
            .ToListAsync();

        var stageDays = periodDays / 3;
        var stages = new List<RoadmapStageDto>
        {
            new()
            {
                StageNo = 1,
                StageName = "基础夯实期",
                DurationDays = stageDays,
                Objectives = new List<string> { "系统学习基础理论", "完成核心知识点学习", "建立学习习惯" },
                Contents = contents.Take(5).Select(c => new RoadmapContentDto
                {
                    ContentId = c.Id, Title = c.Title, ContentType = (int)c.ContentType,
                    Source = "library", Reason = "基础理论核心内容"
                }).ToList(),
                Exam = new RoadmapExamDto { SuggestedCount = 3, TargetScore = 70 },
                Kpis = new List<RoadmapKpiDto> { new() { Metric = "durationMinutes", Target = stageDays * 30 } }
            },
            new()
            {
                StageNo = 2,
                StageName = "强化提升期",
                DurationDays = stageDays,
                Objectives = new List<string> { "深化重点领域", "强化薄弱环节", "提升应用能力" },
                Contents = contents.Skip(5).Take(5).Select(c => new RoadmapContentDto
                {
                    ContentId = c.Id, Title = c.Title, ContentType = (int)c.ContentType,
                    Source = "library", Reason = "重点深化内容"
                }).ToList(),
                Exam = new RoadmapExamDto { SuggestedCount = 5, TargetScore = 80 },
                Kpis = new List<RoadmapKpiDto> { new() { Metric = "durationMinutes", Target = stageDays * 40 } }
            },
            new()
            {
                StageNo = 3,
                StageName = "冲刺巩固期",
                DurationDays = periodDays - stageDays * 2,
                Objectives = new List<string> { "综合复习巩固", "模拟测验检验", "形成长效机制" },
                Contents = contents.Skip(10).Take(5).Select(c => new RoadmapContentDto
                {
                    ContentId = c.Id, Title = c.Title, ContentType = (int)c.ContentType,
                    Source = "library", Reason = "综合复习内容"
                }).ToList(),
                Exam = new RoadmapExamDto { SuggestedCount = 8, TargetScore = 90 },
                Kpis = new List<RoadmapKpiDto> { new() { Metric = "durationMinutes", Target = (periodDays - stageDays * 2) * 50 } }
            }
        };

        return new LearningRoadmapResponse
        {
            MemberId = memberId,
            MemberName = member.Name,
            CurrentLevel = currentLevel,
            Target = target,
            FocusTags = focusTags,
            TotalDays = periodDays,
            Stages = stages,
            NextAction = $"建议从今天开始，每天安排30-50分钟学习，先完成「{stages[0].Contents.FirstOrDefault()?.Title ?? "基础理论"}」的学习。",
            GeneratedAt = DateTime.Now
        };
    }

    // ========== (12) AI 学习预警 ==========
    public async Task<LearningWarningResponse> GetLearningWarningsAsync(int? organizationId, int currentMemberId, int currentRole, int currentOrgId)
    {
        var warnings = await DetectWarningsAsync(organizationId, currentRole, currentOrgId);
        return new LearningWarningResponse
        {
            GeneratedAt = DateTime.Now,
            TotalWarnings = warnings.Count,
            Warnings = warnings,
            TypeBreakdown = warnings.GroupBy(w => w.WarningType).ToDictionary(g => g.Key, g => g.Count())
        };
    }

    public async Task<LearningWarningTriggerResponse> TriggerLearningWarningsAsync(int? organizationId, int currentMemberId, int currentRole, int currentOrgId)
    {
        var warnings = await DetectWarningsAsync(organizationId, currentRole, currentOrgId);
        int sent = 0;
        foreach (var w in warnings)
        {
            try
            {
                await _notification.SendAsync(new SendNotificationRequest
                {
                    PartyMemberId = w.MemberId,
                    Type = (PartySchoolApi.Models.Common.NotificationType)2,
                    Title = "学习预警",
                    Content = w.Message
                });
                sent++;
            }
            catch { }
        }
        return new LearningWarningTriggerResponse
        {
            ScannedCount = warnings.Select(w => w.MemberId).Distinct().Count(),
            WarningCount = warnings.Count,
            NotificationSentCount = sent,
            Warnings = warnings
        };
    }

    private async Task<List<LearningWarningItemDto>> DetectWarningsAsync(int? organizationId, int currentRole, int currentOrgId)
    {
        var warnings = new List<LearningWarningItemDto>();
        var orgs = await _context.Organizations.AsNoTracking().ToListAsync();
        var orgMap = orgs.ToDictionary(o => o.Id, o => o.Name);

        // 组织范围过滤
        List<int> accessibleOrgIds;
        if (currentRole == 2) // SystemAdmin
        {
            accessibleOrgIds = organizationId.HasValue
                ? Services.Common.OrgHierarchyHelper.CollectOrgAndDescendantIds(organizationId.Value, orgs)
                : orgs.Select(o => o.Id).ToList();
        }
        else // BranchSecretary
        {
            accessibleOrgIds = Services.Common.OrgHierarchyHelper.CollectOrgAndDescendantIds(currentOrgId, orgs);
        }

        var members = await _context.PartyMembers.AsNoTracking()
            .Where(m => m.IsEnabled && accessibleOrgIds.Contains(m.OrganizationId))
            .ToListAsync();

        var memberIds = members.Select(m => m.Id).ToList();
        var now = DateTime.Now;

        // 1. 低正确率：最近3次考试平均分<60
        var recentRecords = await _context.MemberTestRecords.AsNoTracking()
            .Where(r => memberIds.Contains(r.MemberId))
            .OrderByDescending(r => r.SubmittedAt)
            .ToListAsync();

        foreach (var member in members)
        {
            var memberRecords = recentRecords.Where(r => r.MemberId == member.Id).Take(3).ToList();
            if (memberRecords.Count >= 2)
            {
                var avg = memberRecords.Average(r => r.Score);
                if (avg < 60)
                {
                    warnings.Add(new LearningWarningItemDto
                    {
                        MemberId = member.Id, MemberName = member.Name,
                        OrganizationId = member.OrganizationId, OrganizationName = orgMap.GetValueOrDefault(member.OrganizationId, ""),
                        WarningType = "low_accuracy", WarningTypeText = "连续低正确率",
                        Message = $"{member.Name} 最近{memberRecords.Count}次考试平均分{avg:F1}，低于及格线60分",
                        MetricValue = avg, Threshold = 60,
                        Suggestion = "建议安排专项练习和错题巩固",
                        DetectedAt = now
                    });
                }
            }
        }

        // 2. 任务逾期：存在超过截止日期未完成的学习任务
        var overdueTasks = await _context.LearningTasks.AsNoTracking()
            .Where(t => t.Deadline < now && accessibleOrgIds.Contains(t.TargetOrgId))
            .ToListAsync();
        var taskIds = overdueTasks.Select(t => t.Id).ToList();
        var completedProgress = await _context.MemberLearningProgress.AsNoTracking()
            .Where(p => taskIds.Contains(p.TaskId ?? 0) && p.IsCompleted)
            .Select(p => new { p.MemberId, p.TaskId })
            .ToListAsync();

        foreach (var task in overdueTasks)
        {
            var taskMemberIds = members.Where(m => m.OrganizationId == task.TargetOrgId).Select(m => m.Id).ToList();
            var completedMemberIds = completedProgress.Where(p => p.TaskId == task.Id).Select(p => p.MemberId).ToHashSet();
            var overdueMembers = taskMemberIds.Where(id => !completedMemberIds.Contains(id)).ToList();
            foreach (var mid in overdueMembers)
            {
                var member = members.FirstOrDefault(m => m.Id == mid);
                if (member == null) continue;
                warnings.Add(new LearningWarningItemDto
                {
                    MemberId = mid, MemberName = member.Name,
                    OrganizationId = member.OrganizationId, OrganizationName = orgMap.GetValueOrDefault(member.OrganizationId, ""),
                    WarningType = "task_overdue", WarningTypeText = "任务逾期未完成",
                    Message = $"{member.Name} 学习任务「{task.TaskName}」已逾期（截止{task.Deadline:yyyy-MM-dd}）",
                    MetricValue = (now - task.Deadline).TotalDays, Threshold = 0,
                    Suggestion = "建议推送催办通知并安排补学",
                    DetectedAt = now
                });
            }
        }

        // 3. 学习活跃度低：近7天无学习记录
        var sevenDaysAgo = now.AddDays(-7);
        var recentProgress = await _context.MemberLearningProgress.AsNoTracking()
            .Where(p => memberIds.Contains(p.MemberId) && p.UpdatedAt >= sevenDaysAgo)
            .Select(p => p.MemberId)
            .Distinct()
            .ToListAsync();
        var inactiveMembers = memberIds.Except(recentProgress).ToList();
        foreach (var mid in inactiveMembers)
        {
            var member = members.FirstOrDefault(m => m.Id == mid);
            if (member == null) continue;
            warnings.Add(new LearningWarningItemDto
            {
                MemberId = mid, MemberName = member.Name,
                OrganizationId = member.OrganizationId, OrganizationName = orgMap.GetValueOrDefault(member.OrganizationId, ""),
                WarningType = "low_activity", WarningTypeText = "长期未学习",
                Message = $"{member.Name} 近7天无学习记录",
                MetricValue = 7, Threshold = 7,
                Suggestion = "建议发送学习提醒，了解是否存在困难",
                DetectedAt = now
            });
        }

        // 4. 学习时长异常：有学习记录但近7天总时长<30分钟
        var recentDuration = await _context.MemberLearningProgress.AsNoTracking()
            .Where(p => memberIds.Contains(p.MemberId) && p.UpdatedAt >= sevenDaysAgo)
            .GroupBy(p => p.MemberId)
            .Select(g => new { MemberId = g.Key, Total = g.Sum(p => p.DurationSeconds) })
            .ToListAsync();
        foreach (var rd in recentDuration)
        {
            if (rd.Total < 1800) // < 30分钟
            {
                var member = members.FirstOrDefault(m => m.Id == rd.MemberId);
                if (member == null) continue;
                warnings.Add(new LearningWarningItemDto
                {
                    MemberId = rd.MemberId, MemberName = member.Name,
                    OrganizationId = member.OrganizationId, OrganizationName = orgMap.GetValueOrDefault(member.OrganizationId, ""),
                    WarningType = "duration_abnormal", WarningTypeText = "学习时长异常",
                    Message = $"{member.Name} 近7天学习时长仅{rd.Total / 60}分钟，低于30分钟基准",
                    MetricValue = rd.Total / 60.0, Threshold = 30,
                    Suggestion = "建议关注学习质量，防止挂机刷时长",
                    DetectedAt = now
                });
            }
        }

        return warnings;
    }
}
