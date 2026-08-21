namespace PartySchoolApi.Models.DTOs;

/// <summary>大屏总览数据</summary>
public class LargeScreenDashboardDto
{
    public LargeScreenOverviewDto Overview { get; set; } = new();
    public List<BranchRankingDto> BranchRankings { get; set; } = new();
    public List<WeaknessHeatmapDto> WeaknessHeatmap { get; set; } = new();
    public List<LearningTrendItem> LearningTrend { get; set; } = new();
}

public class LargeScreenOverviewDto
{
    public int TotalMembers { get; set; }
    public int ActiveMembersToday { get; set; }
    public double TotalLearningHours { get; set; }
    public double OverallCompletionRate { get; set; }
    public double AverageExamScore { get; set; }
    public int TotalCheckIns { get; set; }
    public int OngoingTasks { get; set; }
    public int OngoingExams { get; set; }
}

public class BranchRankingDto
{
    public int OrgId { get; set; }
    public string OrgName { get; set; } = string.Empty;
    public double CompletionRate { get; set; }
    public double AverageScore { get; set; }
    public int MemberCount { get; set; }
    public int Rank { get; set; }
}

public class WeaknessHeatmapDto
{
    public string Tag { get; set; } = string.Empty;
    public int ErrorCount { get; set; }
    public double Intensity { get; set; }
}
