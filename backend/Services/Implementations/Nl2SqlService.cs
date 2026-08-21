using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Services.Implementations;

/// <summary>NL2SQL服务（占位实现，含安全过滤和错别字修正流程）</summary>
public class Nl2SqlService : INl2SqlService
{
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

    public Task<Nl2SqlResponse> QueryAsync(Nl2SqlRequest request)
    {
        var corrections = new List<string>();
        var nl = request.NaturalLanguage;

        // 步骤1：错别字修正
        foreach (var typo in TypoMap)
        {
            if (nl.Contains(typo.Key))
            {
                nl = nl.Replace(typo.Key, typo.Value);
                corrections.Add($"「{typo.Key}」→「{typo.Value}」");
            }
        }

        // 步骤2：意图识别（占位）
        var intent = DetectIntent(nl);

        // 步骤3：生成SQL（占位，模拟）
        var sql = GenerateMockSql(intent, nl);

        // 步骤4：安全校验
        var safetyResult = SafetyCheck(sql);
        if (!safetyResult.IsSafe)
        {
            return Task.FromResult(new Nl2SqlResponse
            {
                GeneratedSql = string.Empty,
                Explanation = $"SQL安全校验未通过：{safetyResult.Reason}",
                ResultData = new List<Dictionary<string, object>>(),
                SessionId = request.SessionId ?? Guid.NewGuid().ToString("N"),
                CorrectionsApplied = corrections
            });
        }

        // 步骤5：返回模拟数据
        var mockData = GenerateMockData(intent);
        var chartData = GenerateChartData(intent);

        return Task.FromResult(new Nl2SqlResponse
        {
            GeneratedSql = sql,
            Explanation = $"已识别意图：{intent}。查询结果如下。",
            ResultData = mockData,
            ChartData = chartData,
            SessionId = request.SessionId ?? Guid.NewGuid().ToString("N"),
            CorrectionsApplied = corrections
        });
    }

    private string DetectIntent(string nl)
    {
        if (nl.Contains("完成率") || nl.Contains("任务")) return "task_completion";
        if (nl.Contains("平均分") || nl.Contains("成绩") || nl.Contains("考试")) return "exam_score";
        if (nl.Contains("学习时长") || nl.Contains("学习时间")) return "learning_duration";
        if (nl.Contains("排名") || nl.Contains("排行")) return "ranking";
        return "general_query";
    }

    private string GenerateMockSql(string intent, string nl)
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

    private (bool IsSafe, string Reason) SafetyCheck(string sql)
    {
        var upper = sql.ToUpperInvariant();
        foreach (var keyword in DangerousKeywords)
        {
            if (upper.Contains(keyword))
                return (false, $"包含危险关键字：{keyword}");
        }
        // 白名单表校验（简化）
        return (true, string.Empty);
    }

    private List<Dictionary<string, object>> GenerateMockData(string intent)
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

    private ChartDataDto GenerateChartData(string intent)
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
}
