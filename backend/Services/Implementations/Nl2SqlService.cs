using Microsoft.EntityFrameworkCore;
using PartySchoolApi.Data;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;
using System.Data;
using System.Text.Json;

namespace PartySchoolApi.Services.Implementations;

/// <summary>
/// NL2SQL服务（真千问 + 真实执行）：
/// 流程：错别字修正 → 千问生成 SQL（附带安全约束）→ 危险关键字/白名单安全校验 → 只读执行并返回真实数据。
/// 千问不可用时回退到规则式 SQL，同样真实执行返回数据；绝不执行非 SELECT 语句。
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

    // 白名单表（使用数据库真实表名）
    private static readonly HashSet<string> AllowedTables = new()
    {
        "partymembers", "learningcontents", "learning_tasks",
        "member_learning_progress", "examtests", "member_test_records",
        "organizations", "questions", "checkinrecords", "learningpoints",
        "task_contents", "exam_papers", "question_categories",
        "content_categories", "content_tags", "tags"
    };

    // 常见错别字映射
    private static readonly Dictionary<string, string> TypoMap = new()
    {
        ["党元"] = "党员",
        ["党支"] = "党支部",
        ["完成绿"] = "完成率",
        ["学西"] = "学习",
        ["考式"] = "考试",
        ["平钧分"] = "平均分"
    };

    // SQL危险关键字黑名单
    private static readonly string[] DangerousKeywords =
    {
        "DROP", "DELETE", "UPDATE", "INSERT", "ALTER", "TRUNCATE",
        "CREATE", "EXEC", "EXECUTE", "--", ";--", "/*", "*/", "xp_"
    };

    // 单次查询返回的最大行数
    private const int MaxRows = 100;

    private const string SchemaDescription =
        "数据库表结构（MySQL，注意各表列名大小写）：\n" +
        "- organizations(id, name, parent_id)：党组织（党委/总支/支部，多级，id/name/parent_id 小写）\n" +
        "- partymembers(Id, Name, OrganizationId, IsEnabled, MemberType)：党员（列名 PascalCase；MemberType 为 正式党员/预备党员；OrganizationId 关联 organizations.id）\n" +
        "- learning_tasks(id, task_name, target_org_id, deadline)：学习任务（target_org_id 关联 organizations.id）\n" +
        "- learningcontents(Id, Title, Body, VideoUrl, IsPublic)：学习内容（列名 PascalCase）\n" +
        "- task_contents(task_id, content_id)：任务与内容关联\n" +
        "- member_learning_progress(id, member_id, content_id, task_id, is_completed, duration_seconds, updated_at)：党员学习进度（小写）\n" +
        "- exam_papers(id, name, question_ids)：试卷（question_ids 为题目Id数组）\n" +
        "- examtests(Id, PaperId, TargetOrgId, Deadline)：考试（列名 PascalCase）\n" +
        "- member_test_records(id, member_id, test_id, answers, score, submitted_at)：党员考试成绩（小写）\n" +
        "- questions(id, question_type, stem, options, correct_answer, score, category_id)：题目\n" +
        "- question_categories(id, name)：题目知识点分类\n" +
        "- checkinrecords(Id, PartyMemberId)：签到\n" +
        "- learningpoints(Id, PartyMemberId, Points, EarnedAt)：积分";

    public Nl2SqlService(IQwenService qwen, AppDbContext context)
    {
        _qwen = qwen;
        _context = context;
    }

    public async Task<Nl2SqlResponse> QueryAsync(Nl2SqlRequest request)
    {
        var corrections = new List<string>();
        var nl = request.NaturalLanguage ?? string.Empty;

        // 步骤1：错别字修正
        foreach (var typo in TypoMap)
        {
            if (nl.Contains(typo.Key))
            {
                nl = nl.Replace(typo.Key, typo.Value);
                corrections.Add($"「{typo.Key}」→「{typo.Value}」");
            }
        }

        var sessionId = request.SessionId ?? Guid.NewGuid().ToString("N");

        // 步骤2：规则意图优先（稳定可靠、不依赖千问）。常见问题（完成率/平均分/时长/党员数）走统一组织口径内存聚合
        var intent = DetectIntent(nl);
        if (intent != "general_query")
        {
            var ruleSql = GenerateSql(intent, nl);
            var ruleData = await ExecuteRuleQueryAsync(intent);
            if (ruleData.Count > 0)
            {
                return BuildResult(sessionId, corrections, ruleSql, true,
                    $"已识别意图「{IntentName(intent)}」，查询结果如下。", ruleData, TryBuildChart(ruleData, nl));
            }
        }

        // 步骤3：千问生成 SQL 并真实执行（处理复杂/非规则意图）
        if (_qwen.IsConfigured)
        {
            try
            {
                var system =
                    "你是 MySQL 专家，负责把自然语言查询转成安全的只读 SQL。只输出一个 JSON 对象，禁止多余文字：\n" +
                    "{\"intent\":\"意图标识\",\"sql\":\"只读SELECT语句\",\"explanation\":\"对查询含义的一句话中文说明\"}\n" +
                    "约束：只允许 SELECT 查询；只能使用上述白名单表；禁止 DELETE/UPDATE/INSERT/ALTER/DROP/TRUNCATE/CREATE/EXEC 等危险语句；禁止分号拼接多条语句；禁止注释符；结果建议加 LIMIT 100。";

                var user =
                    SchemaDescription +
                    "\n\n【用户自然语言】\n" + nl;

                var raw = await _qwen.ChatAsync(system, user, temperature: 0.2, jsonMode: true);
                var parsed = ParseRaw(raw);

                if (parsed != null && !string.IsNullOrWhiteSpace(parsed.Sql))
                {
                    var sql = parsed.Sql.Trim();
                    var safety = SafetyCheck(sql);
                    if (!safety.IsSafe)
                    {
                        return BuildResult(sessionId, corrections, sql, false,
                            $"SQL安全校验未通过：{safety.Reason}");
                    }

                    var data = await ExecuteReadOnlyAsync(sql);
                    if (data.Count > 0)
                    {
                        return BuildResult(sessionId, corrections, sql, true,
                            parsed.Explanation ?? "已查询到结果。", data, TryBuildChart(data, nl));
                    }
                    // 千问 SQL 执行无结果 → 回退到规则式
                }
            }
            catch
            {
                // 千问异常 → 回退到规则式
            }
        }

        // 步骤4：最终回退：规则式生成 SQL 并真实执行
        return await FallbackQueryAsync(nl, corrections, sessionId);
    }

    private static string IntentName(string intent)
    {
        return intent switch
        {
            "task_completion" => "任务完成率",
            "exam_score" => "测验成绩/平均分",
            "learning_duration" => "学习时长",
            "member_count" => "党员人数",
            "ranking" => "排名对比",
            _ => intent
        };
    }

    private async Task<Nl2SqlResponse> FallbackQueryAsync(string nl, List<string> corrections, string sessionId)
    {
        var intent = DetectIntent(nl);
        var sql = GenerateSql(intent, nl);
        var safetyResult = SafetyCheck(sql);
        if (!safetyResult.IsSafe)
        {
            return BuildResult(sessionId, corrections, sql, false,
                $"SQL安全校验未通过：{safetyResult.Reason}");
        }

        var data = await ExecuteReadOnlyAsync(sql);
        return BuildResult(sessionId, corrections, sql, true,
            $"已识别意图「{intent}」，查询结果如下。", data, TryBuildChart(data, nl));
    }

    // ============ 规则意图：统一组织口径的内存聚合（党总支递归汇总其下支部） ============

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
                ["org_name"] = org.Name,
                ["member_count"] = mIds.Count,
                ["completion_rate"] = rate
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
                ["org_name"] = org.Name,
                ["avg_score"] = avg,
                ["exam_count"] = orgRecords.Count
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
            rows.Add(new Dictionary<string, object>
            {
                ["org_name"] = org.Name,
                ["member_count"] = count
            });
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

    // ============ 真实执行 ============

    /// <summary>只读执行 SELECT，返回最多 MaxRows 行结果</summary>
    private async Task<List<Dictionary<string, object>>> ExecuteReadOnlyAsync(string sql)
    {
        var result = new List<Dictionary<string, object>>();
        try
        {
            // 兜底：未带 LIMIT 的 SELECT 强制追加 LIMIT，防止超大结果
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
            // 执行失败（如列名/表名不合法）：返回空，由上层提示
            return new List<Dictionary<string, object>>();
        }
        return result;
    }

    /// <summary>根据结果行数生成回答文本；无结果时给出可读提示</summary>
    private static Nl2SqlResponse BuildResult(string sessionId, List<string> corrections, string sql, bool executed,
        string explanation, List<Dictionary<string, object>>? data = null, ChartDataDto? chart = null)
    {
        var resultData = data ?? new List<Dictionary<string, object>>();
        if (executed && resultData.Count == 0)
        {
            explanation += "（未查询到匹配数据，请尝试换个问法或检查是否该维度暂无记录）";
        }
        return new Nl2SqlResponse
        {
            GeneratedSql = sql,
            Explanation = explanation,
            ResultData = resultData,
            ChartData = chart,
            SessionId = sessionId,
            CorrectionsApplied = corrections
        };
    }

    /// <summary>把查询结果转为图表（按前两列：名称+数值）</summary>
    private static ChartDataDto? TryBuildChart(List<Dictionary<string, object>> data, string nl)
    {
        if (data == null || data.Count == 0) return null;
        var first = data[0];
        var keys = first.Keys.ToList();
        if (keys.Count < 2) return null;

        // 找到字符串列（名称）和数值列（值）：数值列优先选含 rate/score/avg/count 语义的
        string? nameKey = null;
        string? valueKey = null;
        var numericKeys = keys.Where(k => first[k] is int || first[k] is long || first[k] is double || first[k] is decimal).ToList();
        var preferred = numericKeys.FirstOrDefault(k =>
            k.Contains("rate", StringComparison.OrdinalIgnoreCase) ||
            k.Contains("percent", StringComparison.OrdinalIgnoreCase) ||
            k.Contains("score", StringComparison.OrdinalIgnoreCase) ||
            k.Contains("avg", StringComparison.OrdinalIgnoreCase) ||
            k.Contains("minutes", StringComparison.OrdinalIgnoreCase) ||
            k.Contains("hours", StringComparison.OrdinalIgnoreCase));
        valueKey = preferred ?? numericKeys.FirstOrDefault();
        foreach (var k in keys)
        {
            if (nameKey == null && k != valueKey && first[k] is string s) { nameKey = k; break; }
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

        // 前两列数量接近说明是趋势型数据（按行）
        bool isPie = labels.Distinct().Count() == labels.Count && labels.Count <= 12;
        bool isTrend = nl.Contains("趋势") || nl.Contains("每月") || nl.Contains("时长");
        return new ChartDataDto
        {
            ChartType = isPie && !isTrend ? "pie" : "bar",
            Labels = labels,
            Values = values
        };
    }

    // ============ 意图识别与 SQL 生成（回退路径） ============

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
        switch (intent)
        {
            case "task_completion":
                return "SELECT o.name AS org_name, " +
                       "COUNT(DISTINCT pm.Id) AS member_count, " +
                       "ROUND(COALESCE(AVG(CASE WHEN mlp.is_completed = 1 THEN 1 ELSE 0 END), 0) * 100, 2) AS completion_rate " +
                       "FROM organizations o " +
                       "LEFT JOIN partymembers pm ON o.id = pm.OrganizationId AND pm.IsEnabled = 1 " +
                       "LEFT JOIN member_learning_progress mlp ON pm.Id = mlp.member_id " +
                       "GROUP BY o.id, o.name " +
                       "ORDER BY completion_rate DESC";
            case "exam_score":
                return "SELECT o.name AS org_name, " +
                       "ROUND(COALESCE(AVG(mtr.score), 0), 2) AS avg_score, " +
                       "COUNT(mtr.id) AS exam_count " +
                       "FROM organizations o " +
                       "LEFT JOIN partymembers pm ON o.id = pm.OrganizationId AND pm.IsEnabled = 1 " +
                       "LEFT JOIN member_test_records mtr ON pm.Id = mtr.member_id " +
                       "GROUP BY o.id, o.name " +
                       "ORDER BY avg_score DESC";
            case "learning_duration":
                return "SELECT DATE(mlp.updated_at) AS date, " +
                       "ROUND(SUM(mlp.duration_seconds) / 60, 2) AS minutes " +
                       "FROM member_learning_progress mlp " +
                       "WHERE mlp.duration_seconds > 0 " +
                       "GROUP BY DATE(mlp.updated_at) " +
                       "ORDER BY date DESC";
            case "member_count":
                return "SELECT o.name AS org_name, COUNT(DISTINCT pm.Id) AS member_count " +
                       "FROM organizations o " +
                       "LEFT JOIN partymembers pm ON o.id = pm.OrganizationId AND pm.IsEnabled = 1 " +
                       "GROUP BY o.id, o.name " +
                       "ORDER BY member_count DESC";
            default:
                return "SELECT pm.Id, pm.Name AS member_name, o.name AS org_name, pm.MemberType, pm.IsEnabled " +
                       "FROM partymembers pm " +
                       "LEFT JOIN organizations o ON pm.OrganizationId = o.id " +
                       "WHERE pm.IsEnabled = 1 " +
                       "ORDER BY pm.Id LIMIT 20";
        }
    }

    // ============ 安全校验 ============

    private static (bool IsSafe, string Reason) SafetyCheck(string sql)
    {
        var upper = sql.ToUpperInvariant().Replace(" ", "");
        // 必须以 SELECT 开头（允许 WITH? 不，仅 SELECT）
        var trimmed = upper.TrimStart();
        if (!trimmed.StartsWith("SELECT"))
            return (false, "仅允许 SELECT 只读查询");
        foreach (var keyword in DangerousKeywords)
        {
            if (upper.Contains(keyword))
                return (false, $"包含危险关键字：{keyword}");
        }
        // 白名单表校验（至少引用一张白名单表）
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
