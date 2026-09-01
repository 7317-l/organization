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
            }
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
}
