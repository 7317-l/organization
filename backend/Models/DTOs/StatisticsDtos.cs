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
