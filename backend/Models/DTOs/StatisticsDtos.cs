namespace PartySchoolApi.Models.DTOs;

/// <summary>仪表盘总览</summary>
public class DashboardOverviewDto
{
    public int TotalMembers { get; set; }
    public double TodayLearningMinutes { get; set; }
    public double OverallTaskCompletionRate { get; set; }
    public double AverageExamScore { get; set; }
    public int TotalContents { get; set; }
    public int OngoingTasks { get; set; }
    public int OngoingExams { get; set; }

    // ===== 以下字段与管理端 Dashboard.vue 前端 KPI / 预警 / 趋势模块对齐 =====
    /// <summary>待办任务数（前端 pendingTasks）</summary>
    public int PendingTasks { get; set; }
    /// <summary>待批阅测验数（前端 pendingExams）</summary>
    public int PendingExams { get; set; }
    /// <summary>挂机学习人数（前端 afkMembers）</summary>
    public int AfkMembers { get; set; }
    /// <summary>支部完成率均值%（前端 avgCompletionRate）</summary>
    public double AvgCompletionRate { get; set; }
    /// <summary>预警提醒列表（前端 warnings）</summary>
    public List<DashboardWarningDto> Warnings { get; set; } = new();
    /// <summary>近7天学习趋势（前端 trend）</summary>
    public DashboardTrendDto Trend { get; set; } = new();
}

/// <summary>仪表盘预警项</summary>
public class DashboardWarningDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    /// <summary>high / medium / low</summary>
    public string Level { get; set; } = "medium";
    public string Route { get; set; } = string.Empty;
    public string Tab { get; set; } = string.Empty;
}

/// <summary>仪表盘学习趋势（近7天）</summary>
public class DashboardTrendDto
{
    public List<string> Dates { get; set; } = new();
    public List<int> Learners { get; set; } = new();
    public List<int> Completed { get; set; } = new();
}

/// <summary>学习统计趋势项</summary>
public class LearningTrendItem
{
    public string Date { get; set; } = string.Empty;
    public double LearningMinutes { get; set; }
    public double CompletionRate { get; set; }
}

/// <summary>学习统计结果</summary>
public class LearningStatisticsDto
{
    public List<LearningTrendItem> Trend { get; set; } = new();
    public double TotalMinutes { get; set; }
    public double AverageCompletionRate { get; set; }
}

/// <summary>测验统计趋势项</summary>
public class ExamTrendItem
{
    public string Date { get; set; } = string.Empty;
    public double AverageScore { get; set; }
    public double PassRate { get; set; }
    public int ParticipantCount { get; set; }
}

/// <summary>测验统计结果</summary>
public class ExamStatisticsDto
{
    public List<ExamTrendItem> Trend { get; set; } = new();
    public double OverallAverageScore { get; set; }
    public double OverallPassRate { get; set; }
    public int TotalParticipants { get; set; }
}

/// <summary>支部详细统计</summary>
public class BranchStatisticsDto
{
    public int OrgId { get; set; }
    public string OrgName { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public double AverageLearningMinutes { get; set; }
    public double TaskCompletionRate { get; set; }
    public double AverageExamScore { get; set; }
    public double ExamPassRate { get; set; }
    public List<MemberRankingItem> TopLearners { get; set; } = new();
}

/// <summary>党员学习排行</summary>
public class MemberRankingItem
{
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public double LearningMinutes { get; set; }
    public double CompletionRate { get; set; }
}
