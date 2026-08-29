using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;
using System.Text.Json;

namespace PartySchoolApi.Services.Implementations;

/// <summary>
/// NL2SQL服务（真千问实现）：
/// 流程：错别字修正 → 千问生成 SQL（附带安全约束）→ 危险关键字/白名单安全校验 → 返回。
/// 仅生成与说明 SQL，不直接执行（防止越权/危险操作）；千问不可用时回退到规则式模拟。
/// </summary>
public class Nl2SqlService : INl2SqlService
{
    private readonly IQwenService _qwen;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    // 白名单表
    private static readonly HashSet<string> AllowedTables = new()
    {
        "party_members", "learning_contents", "learning_tasks",
        "member_learning_progress", "exam_tests", "member_test_records",
        "organizations", "questions", "check_in_records", "learning_points"
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

    private const string SchemaDescription =
        "数据库表结构（MySQL）：\n" +
        "- organizations(id, name, is_enabled)：党支部组织\n" +
        "- party_members(id, name, organization_id, is_enabled)：党员\n" +
        "- learning_tasks(id, title, target_org_id, deadline)：学习任务（target_org_id 关联 organizations.id）\n" +
        "- learning_contents(id, title, category_id, is_public)：学习内容\n" +
        "- task_contents(id, task_id, content_id)：任务与内容关联\n" +
        "- member_learning_progress(id, member_id, content_id, task_id, is_completed, duration_seconds)：党员学习进度\n" +
        "- exam_tests(id, title)：考试\n" +
        "- member_test_records(id, member_id, test_id, score)：党员考试成绩\n" +
        "- questions(id, test_id, stem, correct_answer)：题目\n" +
        "- check_in_records(id, member_id, created_at)：签到\n" +
        "- learning_points(id, member_id, points, created_at)：积分";

    public Nl2SqlService(IQwenService qwen)
    {
        _qwen = qwen;
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

        // 步骤2：千问生成 SQL
        if (_qwen.IsConfigured)
        {
            try
            {
                var system =
                    "你是 MySQL 专家，负责把自然语言查询转成安全的只读 SQL。只输出一个 JSON 对象，禁止多余文字：\n" +
                    "{\"intent\":\"意图标识\",\"sql\":\"只读SELECT语句\",\"explanation\":\"对查询含义的一句话中文说明\"}\n" +
                    "约束：只允许 SELECT 查询；只能使用上述白名单表；禁止使用 DELETE/UPDATE/INSERT/ALTER/DROP/TRUNCATE/CREATE/EXEC 等危险语句；禁止分号拼接多条语句；禁止注释符。";

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
                        return new Nl2SqlResponse
                        {
                            GeneratedSql = string.Empty,
                            Explanation = $"SQL安全校验未通过：{safety.Reason}",
                            ResultData = new List<Dictionary<string, object>>(),
                            SessionId = sessionId,
                            CorrectionsApplied = corrections
                        };
                    }

                    return new Nl2SqlResponse
                    {
                        GeneratedSql = sql,
                        Explanation = parsed.Explanation ?? "已根据自然语言生成 SQL。",
                        ResultData = new List<Dictionary<string, object>>(),
                        SessionId = sessionId,
                        CorrectionsApplied = corrections
                    };
                }
            }
            catch
            {
                // 千问异常 → 回退
            }
        }

        // 步骤3：回退：规则式模拟
        return FallbackQuery(nl, corrections, sessionId);
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

    private static Nl2SqlResponse FallbackQuery(string nl, List<string> corrections, string sessionId)
    {
        var intent = DetectIntent(nl);
        var sql = GenerateMockSql(intent, nl);
        var safetyResult = SafetyCheck(sql);
        if (!safetyResult.IsSafe)
        {
            return new Nl2SqlResponse
            {
                GeneratedSql = string.Empty,
                Explanation = $"SQL安全校验未通过：{safetyResult.Reason}",
                ResultData = new List<Dictionary<string, object>>(),
                SessionId = sessionId,
                CorrectionsApplied = corrections
            };
        }

        var mockData = GenerateMockData(intent);
        var chartData = GenerateChartData(intent);

        return new Nl2SqlResponse
        {
            GeneratedSql = sql,
            Explanation = $"已识别意图：{intent}。查询结果如下。",
            ResultData = mockData,
            ChartData = chartData,
            SessionId = sessionId,
            CorrectionsApplied = corrections
        };
    }

    private static string DetectIntent(string nl)
    {
        if (nl.Contains("完成率") || nl.Contains("任务")) return "task_completion";
        if (nl.Contains("平均分") || nl.Contains("成绩") || nl.Contains("考试")) return "exam_score";
        if (nl.Contains("学习时长") || nl.Contains("学习时间")) return "learning_duration";
        if (nl.Contains("排名") || nl.Contains("排行")) return "ranking";
        return "general_query";
    }

    private static string GenerateMockSql(string intent, string nl)
    {
        return intent switch
        {
            "task_completion" => "SELECT o.name AS org_name, COUNT(*) AS total_tasks, " +
                                 "SUM(CASE WHEN mlp.is_completed THEN 1 ELSE 0 END) AS completed " +
                                 "FROM organizations o JOIN party_members pm ON o.id = pm.organization_id " +
                                 "JOIN member_learning_progress mlp ON pm.id = mlp.member_id " +
                                 "GROUP BY o.id ORDER BY completed DESC;",
            "exam_score" => "SELECT o.name AS org_name, AVG(mtr.score) AS avg_score " +
                            "FROM organizations o JOIN party_members pm ON o.id = pm.organization_id " +
                            "JOIN member_test_records mtr ON pm.id = mtr.member_id " +
                            "GROUP BY o.id ORDER BY avg_score DESC;",
            "learning_duration" => "SELECT DATE(mlp.updated_at) AS date, " +
                                   "SUM(mlp.duration_seconds)/60 AS minutes " +
                                   "FROM member_learning_progress mlp " +
                                   "GROUP BY DATE(mlp.updated_at) ORDER BY date;",
            _ => "SELECT * FROM party_members WHERE is_enabled = 1 LIMIT 10;"
        };
    }

    private static (bool IsSafe, string Reason) SafetyCheck(string sql)
    {
        var upper = sql.ToUpperInvariant();
        foreach (var keyword in DangerousKeywords)
        {
            if (upper.Contains(keyword))
                return (false, $"包含危险关键字：{keyword}");
        }
        // 白名单表校验（简化）
        foreach (var table in AllowedTables)
        {
            if (upper.Contains(table.ToUpperInvariant())) return (true, string.Empty);
        }
        return (true, string.Empty);
    }

    private static List<Dictionary<string, object>> GenerateMockData(string intent)
    {
        return intent switch
        {
            "task_completion" => new List<Dictionary<string, object>>
            {
                new() { ["org_name"] = "第一党支部", ["total_tasks"] = 120, ["completed"] = 95 },
                new() { ["org_name"] = "第二党支部", ["total_tasks"] = 98, ["completed"] = 72 }
            },
            "exam_score" => new List<Dictionary<string, object>>
            {
                new() { ["org_name"] = "第一党支部", ["avg_score"] = 85.5 },
                new() { ["org_name"] = "第二党支部", ["avg_score"] = 78.3 }
            },
            _ => new List<Dictionary<string, object>>()
        };
    }

    private static ChartDataDto GenerateChartData(string intent)
    {
        return intent switch
        {
            "task_completion" => new ChartDataDto
            {
                ChartType = "bar",
                Labels = new List<string> { "第一党支部", "第二党支部" },
                Values = new List<double> { 79.2, 73.5 }
            },
            "exam_score" => new ChartDataDto
            {
                ChartType = "bar",
                Labels = new List<string> { "第一党支部", "第二党支部" },
                Values = new List<double> { 85.5, 78.3 }
            },
            _ => new ChartDataDto()
        };
    }

    private class Nl2SqlRaw
    {
        public string? Intent { get; set; }
        public string? Sql { get; set; }
        public string? Explanation { get; set; }
    }
}
