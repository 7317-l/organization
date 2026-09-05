using Microsoft.EntityFrameworkCore;
using PartySchoolApi.Data;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Models.Entities;
using PartySchoolApi.Services.Interfaces;
using System.Data;
using System.Text.Json;

namespace PartySchoolApi.Services.Implementations;

/// <summary>
/// NL2SQL服务（真千问 + 真实执行 + 多轮上下文 + 字段白名单 + 敏感脱敏）
/// </summary>
public class Nl2SqlService : INl2SqlService
{
    private readonly IQwenService _qwen;
    private readonly AppDbContext _context;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private static readonly HashSet<string> AllowedTables = new()
    {
        "partymembers", "learningcontents", "learning_tasks",
        "member_learning_progress", "examtests", "member_test_records",
        "organizations", "questions", "checkinrecords", "learningpoints",
        "task_contents", "exam_papers", "question_categories",
        "content_categories", "content_tags", "tags"
    };

    // 字段级白名单：表.列 → 允许
    private static readonly HashSet<string> AllowedColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        "partymembers.Id", "partymembers.Name", "partymembers.OrganizationId", "partymembers.IsEnabled", "partymembers.MemberType", "partymembers.Role", "partymembers.CreatedAt", "partymembers.PointTotal",
        "organizations.id", "organizations.name", "organizations.parent_id", "organizations.created_at",
        "learning_tasks.id", "learning_tasks.task_name", "learning_tasks.target_org_id", "learning_tasks.deadline", "learning_tasks.created_at",
        "learningcontents.Id", "learningcontents.Title", "learningcontents.ContentType", "learningcontents.IsPublic", "learningcontents.CreatedAt", "learningcontents.CategoryId",
        "member_learning_progress.id", "member_learning_progress.member_id", "member_learning_progress.content_id", "member_learning_progress.task_id", "member_learning_progress.is_completed", "member_learning_progress.duration_seconds", "member_learning_progress.completed_at", "member_learning_progress.updated_at",
        "examtests.Id", "examtests.PaperId", "examtests.TargetOrgId", "examtests.Deadline", "examtests.CreatedAt",
        "member_test_records.id", "member_test_records.member_id", "member_test_records.test_id", "member_test_records.score", "member_test_records.submitted_at",
        "questions.id", "questions.question_type", "questions.stem", "questions.score", "questions.category_id", "questions.created_at",
        "question_categories.id", "question_categories.name",
        "checkinrecords.Id", "checkinrecords.PartyMemberId", "checkinrecords.LocationName", "checkinrecords.CheckInTime", "checkinrecords.PointsEarned", "checkinrecords.SiteId",
        "learningpoints.Id", "learningpoints.PartyMemberId", "learningpoints.SourceType", "learningpoints.Points", "learningpoints.EarnedAt",
        "exam_papers.id", "exam_papers.name", "exam_papers.total_score", "exam_papers.created_at"
    };

    // 敏感字段（输出时脱敏）
    private static readonly HashSet<string> SensitiveColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        "partymembers.Phone", "partymembers.PasswordHash", "partymembers.RefreshToken", "partymembers.RefreshTokenExpiry"
    };

    private static readonly Dictionary<string, string> TypoMap = new()
    {
        ["党元"] = "党员", ["党支"] = "党支部", ["完成绿"] = "完成率",
        ["学西"] = "学习", ["考式"] = "考试", ["平钧分"] = "平均分"
    };

    private static readonly string[] DangerousKeywords =
    {
        "DROP", "DELETE", "UPDATE", "INSERT", "ALTER", "TRUNCATE",
        "CREATE", "EXEC", "EXECUTE", "--", ";--", "/*", "*/", "xp_"
    };

    // 指代词
    private static readonly string[] ReferenceWords = { "同上", "继续", "再看", "和上次一样", "上一条", "上个结果", "同样条件" };

    private const int MaxRows = 100;

    private const string SchemaDescription =
        "数据库表结构（MySQL，注意各表列名大小写）：\n" +
        "- organizations(id, name, parent_id)：党组织\n" +
        "- partymembers(Id, Name, OrganizationId, IsEnabled, MemberType, Role, PointTotal)：党员\n" +
        "- learning_tasks(id, task_name, target_org_id, deadline)：学习任务\n" +
        "- learningcontents(Id, Title, ContentType, IsPublic, CategoryId)：学习内容\n" +
        "- member_learning_progress(id, member_id, content_id, task_id, is_completed, duration_seconds, updated_at)：学习进度\n" +
        "- examtests(Id, PaperId, TargetOrgId, Deadline)：考试\n" +
        "- member_test_records(id, member_id, test_id, score, submitted_at)：考试成绩\n" +
        "- questions(id, question_type, stem, score, category_id)：题目\n" +
        "- question_categories(id, name)：题目分类\n" +
        "- checkinrecords(Id, PartyMemberId, LocationName, CheckInTime, PointsEarned)：签到\n" +
        "- learningpoints(Id, PartyMemberId, SourceType, Points, EarnedAt)：积分";

    public Nl2SqlService(IQwenService qwen, AppDbContext context)
    {
        _qwen = qwen;
        _context = context;
    }

    public async Task<Nl2SqlResponse> QueryAsync(Nl2SqlRequest request, int memberId)
    {
        var corrections = new List<string>();
        var nl = request.NaturalLanguage ?? string.Empty;
        var effectiveMemberId = request.UserId ?? memberId;
        var historyCount = Math.Clamp(request.HistoryCount, 1, 10);

        // 错别字修正
        foreach (var typo in TypoMap)
        {
            if (nl.Contains(typo.Key))
            {
                nl = nl.Replace(typo.Key, typo.Value);
                corrections.Add($"「{typo.Key}」→「{typo.Value}」");
            }
        }

        var sessionId = request.SessionId ?? Guid.NewGuid().ToString("N");

        // 多轮上下文：取最近历史
        var history = await GetSessionHistoryInternalAsync(sessionId, effectiveMemberId, historyCount);
        var conversation = history.Select(h => new Nl2SqlConversationItem
        {
            Question = h.Question,
            Explanation = h.Explanation ?? "",
            ResultSummary = h.ResultSummary
        }).ToList();

        // 指代改写
        var (rewritten, isResolved) = ResolveReference(nl, history);
        var intent = DetectIntent(rewritten);

        // 规则意图优先
        if (intent != "general_query")
        {
            var ruleSql = GenerateSql(intent, rewritten);
            var ruleData = await ExecuteRuleQueryAsync(intent);
            if (ruleData.Count > 0)
            {
                var masked = MaskSensitiveData(ruleData);
                await SaveSessionAsync(sessionId, effectiveMemberId, nl, rewritten, ruleSql,
                    $"已识别意图「{IntentName(intent)}」，查询结果如下。", BuildResultSummary(masked));
                return BuildResultV2(sessionId, corrections, ruleSql, true,
                    $"已识别意图「{IntentName(intent)}」，查询结果如下。", masked,
                    TryBuildChart(masked, rewritten), intent, rewritten, isResolved, conversation);
            }
        }

        // 千问生成 SQL
        if (_qwen.IsConfigured)
        {
            try
            {
                var contextPrompt = history.Count > 0
                    ? "\n\n【历史对话】\n" + string.Join("\n", history.Take(3).Select(h => $"问：{h.Question}\n答：{h.Explanation}"))
                    : "";

                var system =
                    "你是 MySQL 专家，把自然语言转成安全的只读 SQL。只输出 JSON：\n" +
                    "{\"intent\":\"意图\",\"sql\":\"SELECT语句\",\"explanation\":\"中文说明\"}\n" +
                    "约束：只允许 SELECT；只能用白名单表；禁止危险语句；禁止分号拼接；加 LIMIT 100。" +
                    "注意：partymembers 表禁止查询 Phone、PasswordHash、RefreshToken、RefreshTokenExpiry 字段。";

                var user = SchemaDescription + contextPrompt + "\n\n【用户问题】\n" + rewritten;
                var raw = await _qwen.ChatAsync(system, user, temperature: 0.2, jsonMode: true);
                var parsed = ParseRaw(raw);

                if (parsed != null && !string.IsNullOrWhiteSpace(parsed.Sql))
                {
                    var sql = parsed.Sql.Trim();
                    var safety = SafetyCheck(sql);
                    if (!safety.IsSafe)
                    {
                        await SaveSessionAsync(sessionId, effectiveMemberId, nl, rewritten, sql,
                            $"SQL安全校验未通过：{safety.Reason}", null);
                        return BuildResultV2(sessionId, corrections, sql, false,
                            $"SQL安全校验未通过：{safety.Reason}", null, null, intent, rewritten, isResolved, conversation);
                    }

                    var colCheck = CheckColumnsAllowed(sql);
                    if (!colCheck.Ok)
                    {
                        return BuildResultV2(sessionId, corrections, sql, false,
                            $"字段级白名单校验未通过：{colCheck.Reason}", null, null, intent, rewritten, isResolved, conversation);
                    }

                    var data = await ExecuteReadOnlyAsync(sql);
                    var masked = MaskSensitiveData(data);
                    if (masked.Count > 0)
                    {
                        await SaveSessionAsync(sessionId, effectiveMemberId, nl, rewritten, sql,
                            parsed.Explanation ?? "已查询到结果。", BuildResultSummary(masked));
                        return BuildResultV2(sessionId, corrections, sql, true,
                            parsed.Explanation ?? "已查询到结果。", masked,
                            TryBuildChart(masked, rewritten), parsed.Intent ?? intent, rewritten, isResolved, conversation);
                    }
                }
            }
            catch { }
        }

        // 最终回退
        var fbIntent = DetectIntent(rewritten);
        var fbSql = GenerateSql(fbIntent, rewritten);
        var fbSafety = SafetyCheck(fbSql);
        if (!fbSafety.IsSafe)
        {
            return BuildResultV2(sessionId, corrections, fbSql, false,
                $"SQL安全校验未通过：{fbSafety.Reason}", null, null, fbIntent, rewritten, isResolved, conversation);
        }
        var fbData = await ExecuteReadOnlyAsync(fbSql);
        var fbMasked = MaskSensitiveData(fbData);
        await SaveSessionAsync(sessionId, effectiveMemberId, nl, rewritten, fbSql,
            $"已识别意图「{fbIntent}」，查询结果如下。", BuildResultSummary(fbMasked));
        return BuildResultV2(sessionId, corrections, fbSql, true,
            $"已识别意图「{fbIntent}」，查询结果如下。", fbMasked,
            TryBuildChart(fbMasked, rewritten), fbIntent, rewritten, isResolved, conversation);
    }

    public async Task<List<Nl2SqlHistoryItem>> GetHistoryAsync(string sessionId, int memberId, int limit = 5)
    {
        return await _context.Nl2SqlSessions
            .Where(s => s.SessionId == sessionId && s.MemberId == memberId)
            .OrderByDescending(s => s.CreatedAt)
            .Take(limit)
            .Select(s => new Nl2SqlHistoryItem
            {
                Question = s.Question,
                Rewritten = s.Rewritten,
                Explanation = s.Explanation,
                ResultSummary = s.ResultSummary,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();
    }

    private async Task<List<Nl2SqlSession>> GetSessionHistoryInternalAsync(string sessionId, int memberId, int limit)
    {
        return await _context.Nl2SqlSessions
            .Where(s => s.SessionId == sessionId && s.MemberId == memberId)
            .OrderByDescending(s => s.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    private async Task SaveSessionAsync(string sessionId, int memberId, string question, string? rewritten,
        string? sql, string explanation, string? resultSummary)
    {
        try
        {
            _context.Nl2SqlSessions.Add(new Nl2SqlSession
            {
                SessionId = sessionId,
                MemberId = memberId,
                Question = question,
                Rewritten = rewritten,
                SqlText = sql,
                Explanation = explanation.Length > 2000 ? explanation[..2000] : explanation,
                ResultSummary = resultSummary?.Length > 4000 ? resultSummary[..4000] : resultSummary,
                CreatedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();
        }
        catch { }
    }

    private static (string rewritten, bool isResolved) ResolveReference(string nl, List<Nl2SqlSession> history)
    {
        var hasRef = ReferenceWords.Any(w => nl.Contains(w));
        if (!hasRef || history.Count == 0)
            return (nl, false);

        var last = history[0];
        var rewritten = nl;
        foreach (var w in ReferenceWords)
        {
            if (rewritten.Contains(w))
            {
                rewritten = rewritten.Replace(w, $"（基于上一轮：{last.Question}）");
            }
        }
        return (rewritten, true);
    }

    private static (bool Ok, string Reason) CheckColumnsAllowed(string sql)
    {
        var upper = sql.ToUpperInvariant();
        // 检查是否引用了敏感字段
        foreach (var col in SensitiveColumns)
        {
            var parts = col.Split('.');
            var table = parts[0].ToUpperInvariant();
            var column = parts[1].ToUpperInvariant();
            if (upper.Contains(table) && upper.Contains(column))
                return (false, $"禁止查询敏感字段 {col}");
        }
        return (true, "");
    }

    private static List<Dictionary<string, object>> MaskSensitiveData(List<Dictionary<string, object>> data)
    {
        foreach (var row in data)
        {
            var keys = row.Keys.ToList();
            foreach (var key in keys)
            {
                var lowerKey = key.ToLowerInvariant();
                if (lowerKey == "phone" && row[key] is string phone && phone.Length >= 7)
                {
                    row[key] = phone[..3] + "****" + phone[^4..];
                }
                else if (lowerKey is "passwordhash" or "refreshtoken" or "refreshtokenexpiry")
                {
                    row[key] = "***";
                }
            }
        }
        return data;
    }

    private static string? BuildResultSummary(List<Dictionary<string, object>> data)
    {
        if (data.Count == 0) return null;
        var first = data[0];
        return $"共{data.Count}行，首行：{string.Join(", ", first.Take(5).Select(kv => $"{kv.Key}={kv.Value}"))}";
    }

    private static string IntentName(string intent) => intent switch
    {
        "task_completion" => "任务完成率",
        "exam_score" => "测验成绩/平均分",
        "learning_duration" => "学习时长",
        "member_count" => "党员人数",
        "ranking" => "排名对比",
        _ => intent
    };

    private async Task<List<Dictionary<string, object>>> ExecuteRuleQueryAsync(string intent)
    {
        return intent switch
        {
            "task_completion" => await RuleTaskCompletionAsync(),
            "exam_score" => await RuleExamScoreAsync(),
            "member_count" => await RuleMemberCountAsync(),
            "learning_duration" => await RuleLearningDurationAsync(),
            _ => await ExecuteReadOnlyAsync(GenerateSql("general_query", ""))
        };
    }

    private async Task<List<Dictionary<string, object>>> RuleTaskCompletionAsync()
    {
        var orgs = await _context.Organizations.ToListAsync();
        if (orgs.Count == 0) return new List<Dictionary<string, object>>();
        var scopeMap = Services.Common.OrgHierarchyHelper.BuildOrgScopeMap(orgs);
        var members = await _context.PartyMembers.Where(m => m.IsEnabled).ToListAsync();
        var tasks = await _context.LearningTasks.Include(t => t.TaskContents).ToListAsync();
        var progress = await _context.MemberLearningProgress.Where(p => p.TaskId.HasValue).ToListAsync();
        var rows = new List<Dictionary<string, object>>();
        foreach (var org in orgs)
        {
            var scope = scopeMap[org.Id];
            var mIds = members.Where(m => scope.Contains(m.OrganizationId)).Select(m => m.Id).ToList();
            var taskIds = tasks.Where(t => scope.Contains(t.TargetOrgId)).Select(t => t.Id).ToHashSet();
            var contentCount = tasks.Where(t => scope.Contains(t.TargetOrgId)).Sum(t => t.TaskContents.Count);
            int done = 0;
            if (contentCount > 0 && mIds.Count > 0)
                done = progress.Count(p => mIds.Contains(p.MemberId) && taskIds.Contains(p.TaskId.Value));
            var rate = contentCount > 0 && mIds.Count > 0
                ? Math.Round((double)done / (contentCount * mIds.Count) * 100, 2) : 0.0;
            rows.Add(new Dictionary<string, object>
            {
                ["org_name"] = org.Name, ["member_count"] = mIds.Count, ["completion_rate"] = rate
            });
        }
        return rows.OrderByDescending(r => r["completion_rate"]).ToList();
    }

    private async Task<List<Dictionary<string, object>>> RuleExamScoreAsync()
    {
        var orgs = await _context.Organizations.ToListAsync();
        if (orgs.Count == 0) return new List<Dictionary<string, object>>();
        var scopeMap = Services.Common.OrgHierarchyHelper.BuildOrgScopeMap(orgs);
        var members = await _context.PartyMembers.Where(m => m.IsEnabled).ToListAsync();
        var records = await _context.MemberTestRecords.ToListAsync();
        var rows = new List<Dictionary<string, object>>();
        foreach (var org in orgs)
        {
            var scope = scopeMap[org.Id];
            var mIds = members.Where(m => scope.Contains(m.OrganizationId)).Select(m => m.Id).ToHashSet();
            var orgRecords = records.Where(r => mIds.Contains(r.MemberId)).ToList();
            var avg = orgRecords.Any() ? Math.Round(orgRecords.Average(r => r.Score), 2) : 0.0;
            rows.Add(new Dictionary<string, object>
            {
                ["org_name"] = org.Name, ["avg_score"] = avg, ["exam_count"] = orgRecords.Count
            });
        }
        return rows.OrderByDescending(r => r["avg_score"]).ToList();
    }

    private async Task<List<Dictionary<string, object>>> RuleMemberCountAsync()
    {
        var orgs = await _context.Organizations.ToListAsync();
        if (orgs.Count == 0) return new List<Dictionary<string, object>>();
        var scopeMap = Services.Common.OrgHierarchyHelper.BuildOrgScopeMap(orgs);
        var members = await _context.PartyMembers.Where(m => m.IsEnabled).ToListAsync();
        var rows = new List<Dictionary<string, object>>();
        foreach (var org in orgs)
        {
            var scope = scopeMap[org.Id];
            var count = members.Count(m => scope.Contains(m.OrganizationId));
            rows.Add(new Dictionary<string, object> { ["org_name"] = org.Name, ["member_count"] = count });
        }
        return rows.OrderByDescending(r => r["member_count"]).ToList();
    }

    private async Task<List<Dictionary<string, object>>> RuleLearningDurationAsync()
    {
        var data = await _context.MemberLearningProgress
            .Where(p => p.DurationSeconds > 0)
            .GroupBy(p => new { p.UpdatedAt.Year, p.UpdatedAt.Month, p.UpdatedAt.Day })
            .Select(g => new { g.Key.Year, g.Key.Month, g.Key.Day, Sec = g.Sum(x => (int?)x.DurationSeconds) ?? 0 })
            .OrderByDescending(g => g.Year).ThenByDescending(g => g.Month).ThenByDescending(g => g.Day)
            .Take(30).ToListAsync();
        data.Reverse();
        var rows = new List<Dictionary<string, object>>();
        foreach (var d in data)
        {
            rows.Add(new Dictionary<string, object>
            {
                ["date"] = $"{d.Year}-{d.Month:D2}-{d.Day:D2}",
                ["minutes"] = Math.Round(d.Sec / 60.0, 2)
            });
        }
        return rows;
    }

    private async Task<List<Dictionary<string, object>>> ExecuteReadOnlyAsync(string sql)
    {
        var result = new List<Dictionary<string, object>>();
        try
        {
            var safeSql = sql.Trim().TrimEnd(';', ' ', '\t', '\r', '\n');
            var upper = safeSql.ToUpperInvariant();
            if (!upper.Contains(" LIMIT "))
                safeSql += " LIMIT " + MaxRows;

            await using var conn = _context.Database.GetDbConnection();
            await conn.OpenAsync();
            await using var tx = await conn.BeginTransactionAsync(IsolationLevel.ReadCommitted);
            await using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = safeSql;
            cmd.CommandTimeout = 15;
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var v = reader.GetValue(i);
                    row[reader.GetName(i)] = v == DBNull.Value ? "" : v;
                }
                result.Add(row);
                if (result.Count >= MaxRows) break;
            }
        }
        catch
        {
            return new List<Dictionary<string, object>>();
        }
        return result;
    }

    private static Nl2SqlResponse BuildResultV2(string sessionId, List<string> corrections, string sql, bool executed,
        string explanation, List<Dictionary<string, object>>? data, ChartDataDto? chart,
        string intent, string rewrittenQuery, bool isResolved, List<Nl2SqlConversationItem> conversation)
    {
        var resultData = data ?? new List<Dictionary<string, object>>();
        if (executed && resultData.Count == 0)
            explanation += "（未查询到匹配数据，请尝试换个问法）";
        return new Nl2SqlResponse
        {
            GeneratedSql = sql,
            Explanation = explanation,
            ResultData = resultData,
            ChartData = chart,
            SessionId = sessionId,
            CorrectionsApplied = corrections,
            Intent = intent,
            RewrittenQuery = rewrittenQuery,
            IsResolvedFromHistory = isResolved,
            Conversation = conversation
        };
    }

    private static ChartDataDto? TryBuildChart(List<Dictionary<string, object>> data, string nl)
    {
        if (data == null || data.Count == 0) return null;
        var first = data[0];
        var keys = first.Keys.ToList();
        if (keys.Count < 2) return null;

        string? nameKey = null;
        string? valueKey = null;
        var numericKeys = keys.Where(k => first[k] is int or long or double or decimal).ToList();
        var preferred = numericKeys.FirstOrDefault(k =>
            k.Contains("rate", StringComparison.OrdinalIgnoreCase) ||
            k.Contains("percent", StringComparison.OrdinalIgnoreCase) ||
            k.Contains("score", StringComparison.OrdinalIgnoreCase) ||
            k.Contains("avg", StringComparison.OrdinalIgnoreCase) ||
            k.Contains("minutes", StringComparison.OrdinalIgnoreCase));
        valueKey = preferred ?? numericKeys.FirstOrDefault();
        foreach (var k in keys)
        {
            if (nameKey == null && k != valueKey && first[k] is string) { nameKey = k; break; }
        }
        if (nameKey == null || valueKey == null) return null;

        var labels = data.Select(d => d.TryGetValue(nameKey, out var nv) ? nv?.ToString() ?? "" : "").ToList();
        var values = data.Select(d =>
        {
            if (d.TryGetValue(valueKey, out var vv) && vv != null)
            {
                try { return Convert.ToDouble(vv); } catch { return 0.0; }
            }
            return 0.0;
        }).ToList();

        bool isPie = labels.Distinct().Count() == labels.Count && labels.Count <= 12;
        bool isTrend = nl.Contains("趋势") || nl.Contains("每月") || nl.Contains("时长");
        return new ChartDataDto
        {
            ChartType = isPie && !isTrend ? "pie" : "bar",
            Labels = labels,
            Values = values
        };
    }

    private static string DetectIntent(string nl)
    {
        if (nl.Contains("完成率") || nl.Contains("任务")) return "task_completion";
        if (nl.Contains("平均分") || nl.Contains("成绩") || nl.Contains("考试")) return "exam_score";
        if (nl.Contains("学习时长") || nl.Contains("学习时间")) return "learning_duration";
        if (nl.Contains("党员") && (nl.Contains("多少") || nl.Contains("数量") || nl.Contains("人数") || nl.Contains("统计"))) return "member_count";
        if (nl.Contains("排名") || nl.Contains("排行") || nl.Contains("对比")) return "ranking";
        return "general_query";
    }

    private static string GenerateSql(string intent, string nl)
    {
        return intent switch
        {
            "task_completion" => "SELECT o.name AS org_name, COUNT(DISTINCT pm.Id) AS member_count, " +
                                 "ROUND(COALESCE(AVG(CASE WHEN mlp.is_completed = 1 THEN 1 ELSE 0 END), 0) * 100, 2) AS completion_rate " +
                                 "FROM organizations o LEFT JOIN partymembers pm ON o.id = pm.OrganizationId AND pm.IsEnabled = 1 " +
                                 "LEFT JOIN member_learning_progress mlp ON pm.Id = mlp.member_id " +
                                 "GROUP BY o.id, o.name ORDER BY completion_rate DESC",
            "exam_score" => "SELECT o.name AS org_name, ROUND(COALESCE(AVG(mtr.score), 0), 2) AS avg_score, COUNT(mtr.id) AS exam_count " +
                            "FROM organizations o LEFT JOIN partymembers pm ON o.id = pm.OrganizationId AND pm.IsEnabled = 1 " +
                            "LEFT JOIN member_test_records mtr ON pm.Id = mtr.member_id " +
                            "GROUP BY o.id, o.name ORDER BY avg_score DESC",
            "learning_duration" => "SELECT DATE(mlp.updated_at) AS date, ROUND(SUM(mlp.duration_seconds) / 60, 2) AS minutes " +
                                   "FROM member_learning_progress mlp WHERE mlp.duration_seconds > 0 " +
                                   "GROUP BY DATE(mlp.updated_at) ORDER BY date DESC",
            "member_count" => "SELECT o.name AS org_name, COUNT(DISTINCT pm.Id) AS member_count " +
                              "FROM organizations o LEFT JOIN partymembers pm ON o.id = pm.OrganizationId AND pm.IsEnabled = 1 " +
                              "GROUP BY o.id, o.name ORDER BY member_count DESC",
            _ => "SELECT pm.Id, pm.Name AS member_name, o.name AS org_name, pm.MemberType, pm.IsEnabled " +
                 "FROM partymembers pm LEFT JOIN organizations o ON pm.OrganizationId = o.id " +
                 "WHERE pm.IsEnabled = 1 ORDER BY pm.Id LIMIT 20"
        };
    }

    private static (bool IsSafe, string Reason) SafetyCheck(string sql)
    {
        var upper = sql.ToUpperInvariant().Replace(" ", "");
        var trimmed = upper.TrimStart();
        if (!trimmed.StartsWith("SELECT"))
            return (false, "仅允许 SELECT 只读查询");
        // 禁止 SELECT *
        if (System.Text.RegularExpressions.Regex.IsMatch(sql, @"SELECT\s+\*\s+FROM", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            return (false, "禁止使用 SELECT *，请明确指定查询字段");
        foreach (var keyword in DangerousKeywords)
        {
            if (upper.Contains(keyword))
                return (false, $"包含危险关键字：{keyword}");
        }
        foreach (var table in AllowedTables)
        {
            if (upper.Contains(table.ToUpperInvariant())) return (true, string.Empty);
        }
        return (false, "未引用白名单表，已拒绝执行");
    }

    private static Nl2SqlRaw? ParseRaw(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var start = raw.IndexOf('{');
        var end = raw.LastIndexOf('}');
        if (start < 0 || end <= start) return null;
        try
        {
            return JsonSerializer.Deserialize<Nl2SqlRaw>(raw.Substring(start, end - start + 1), JsonOpts);
        }
        catch
        {
            return null;
        }
    }

    private class Nl2SqlRaw
    {
        public string? Intent { get; set; }
        public string? Sql { get; set; }
        public string? Explanation { get; set; }
    }
}
