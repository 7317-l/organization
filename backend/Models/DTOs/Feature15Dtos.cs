using PartySchoolApi.Models.Common;

namespace PartySchoolApi.Models.DTOs;

// ========== (1) NL2SQL 多轮上下文 ==========
public class Nl2SqlConversationItem
{
    public string Question { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public string? ResultSummary { get; set; }
}

public class Nl2SqlHistoryItem
{
    public string Question { get; set; } = string.Empty;
    public string? Rewritten { get; set; }
    public string? Explanation { get; set; }
    public string? ResultSummary { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ========== (2) RAG 两级检索 ==========
public class RagResultItem
{
    public string Id { get; set; } = string.Empty;
    public string File { get; set; } = string.Empty;
    public string Snippet { get; set; } = string.Empty;
    public double Score { get; set; }
    public double RerankScore { get; set; }
    public double Confidence { get; set; }
    public List<string> MatchedKeywords { get; set; } = new();
}

public class KnowledgeDocumentDto
{
    public string Id { get; set; } = string.Empty;
    public string File { get; set; } = string.Empty;
    public int ChunkCount { get; set; }
    public DateTime? LoadedAt { get; set; }
}

// ========== (3) AI 内容生成 ==========
public class AiContentSectionDto
{
    public string Heading { get; set; } = string.Empty;
    public int Minutes { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class AiGeneratedContentDto
{
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public List<string> Outline { get; set; } = new();
    public List<string> KeyPoints { get; set; } = new();
    public string TargetAudience { get; set; } = string.Empty;
    public int? EstimatedMinutes { get; set; }
    public int WordCount { get; set; }
    public List<AiContentSectionDto> Sections { get; set; } = new();
}

// ========== (4) AI 学习标兵 ==========
public class StarMemberRequest
{
    public int? OrganizationId { get; set; }
    public int TopN { get; set; } = 10;
    public StarMemberWeights? Weights { get; set; }
    public bool IncludeReason { get; set; } = true;
}

public class StarMemberWeights
{
    public double LearningMinutes { get; set; } = 0.30;
    public double TaskCompletion { get; set; } = 0.25;
    public double ExamScore { get; set; } = 0.25;
    public double WeaknessImprovement { get; set; } = 0.10;
    public double Points { get; set; } = 0.10;
}

public class StarMemberDimensionDto
{
    public string Name { get; set; } = string.Empty;
    public double Score { get; set; }
    public double Weight { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class StarMemberItemDto
{
    public int Rank { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public double TotalScore { get; set; }
    public string Level { get; set; } = string.Empty;
    public List<StarMemberDimensionDto> Dimensions { get; set; } = new();
    public string? AiReason { get; set; }
}

public class StarMemberResponse
{
    public DateTime GeneratedAt { get; set; }
    public StarMemberScopeDto Scope { get; set; } = new();
    public List<StarMemberItemDto> Members { get; set; } = new();
}

public class StarMemberScopeDto
{
    public int? OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    public int MemberCount { get; set; }
}

// ========== (5) 三会一课简报 ==========
public class MeetingBriefRequest
{
    public int? OrganizationId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? Type { get; set; }
}

public class MeetingBriefTypeBreakdown
{
    public int Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class MeetingBriefPerActivity
{
    public int ActivityId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public DateTime ActivityTime { get; set; }
    public string Summary { get; set; } = string.Empty;
    public List<string> KeyPoints { get; set; } = new();
}

public class MeetingBriefResponse
{
    public MeetingBriefPeriodDto Period { get; set; } = new();
    public int? OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    public int ActivityCount { get; set; }
    public List<MeetingBriefTypeBreakdown> TypeBreakdown { get; set; } = new();
    public int TotalHearts { get; set; }
    public double? AttendanceRate { get; set; }
    public string Brief { get; set; } = string.Empty;
    public List<string> KeyPoints { get; set; } = new();
    public List<MeetingBriefPerActivity> PerActivity { get; set; } = new();
}

public class MeetingBriefPeriodDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

// ========== (6) 思想汇报 AI 建议 ==========
public class ReportSuggestionRequest
{
    public string? ReportContent { get; set; }
    public int? Stage { get; set; }
}

public class ReportSuggestionDimensionDto
{
    public string Name { get; set; } = string.Empty;
    public double Score { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class ReportSuggestionResponse
{
    public int ProcessId { get; set; }
    public double OverallScore { get; set; }
    public List<ReportSuggestionDimensionDto> Dimensions { get; set; } = new();
    public List<string> Strengths { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
    public string? RewrittenExcerpt { get; set; }
}

// ========== (7) 发展材料 AI 校验 ==========
public class MaterialCheckRequest
{
    public List<string?>? Materials { get; set; }
    public int? Stage { get; set; }
}

public class MaterialCheckIssueDto
{
    public string Material { get; set; } = string.Empty;
    public string Status { get; set; } = "ok";
    public string CheckResult { get; set; } = string.Empty;
    public string Suggestion { get; set; } = string.Empty;
}

public class MaterialCheckResponse
{
    public int ProcessId { get; set; }
    public int Stage { get; set; }
    public string StageName { get; set; } = string.Empty;
    public bool IsComplete { get; set; }
    public List<string> RequiredMaterials { get; set; } = new();
    public List<string> MissingMaterials { get; set; } = new();
    public List<MaterialCheckIssueDto> Issues { get; set; } = new();
    public double? Score { get; set; }
    public string Suggestion { get; set; } = string.Empty;
    public DateTime CheckedAt { get; set; }
}

// ========== (8) 到期提醒 ==========
public class ReminderTriggerRequest
{
    public int? OrganizationId { get; set; }
    public List<string>? Types { get; set; }
    public bool SendNotification { get; set; } = true;
}

public class ReminderItemDto
{
    public int ReminderId { get; set; }
    public int ProcessId { get; set; }
    public int PartyMemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string Message { get; set; } = string.Empty;
    public int Status { get; set; }
    public DateTime? SentAt { get; set; }
}

public class ReminderTriggerResponse
{
    public ReminderScannedDto Scanned { get; set; } = new();
    public List<ReminderItemDto> Reminders { get; set; } = new();
    public int SentCount { get; set; }
}

public class ReminderScannedDto
{
    public int ProbationaryDue { get; set; }
    public int MaterialMissing { get; set; }
    public int ReportDue { get; set; }
}

public class ReminderQueryParams : PagedQueryParams
{
    public int? OrganizationId { get; set; }
    public int? Status { get; set; }
    public string? Type { get; set; }
}

// ========== (9) 支部评级 + 整改 ==========
public class RatingDimensionDto
{
    public string Dimension { get; set; } = string.Empty;
    public double Score { get; set; }
    public string Grade { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
}

public class RatingSuggestionDto
{
    public string Id { get; set; } = string.Empty;
    public string Issue { get; set; } = string.Empty;
    public string Suggestion { get; set; } = string.Empty;
    public string Priority { get; set; } = "medium";
}

public class RectificationDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string Quarter { get; set; } = string.Empty;
    public string Issue { get; set; } = string.Empty;
    public string Suggestion { get; set; } = string.Empty;
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string? Remark { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class CreateRectificationRequest
{
    public string Quarter { get; set; } = string.Empty;
    public string Issue { get; set; } = string.Empty;
    public string Suggestion { get; set; } = string.Empty;
}

public class CompleteRectificationRequest
{
    public string? Remark { get; set; }
}

public class UpdateRectificationStatusRequest
{
    public int Status { get; set; }
    public string? Remark { get; set; }
}

// ========== (10) 薄弱点互助 ==========
public class PairHelpRecommendRequest
{
    public List<string>? MyWeaknessTags { get; set; }
    public int Count { get; set; } = 5;
}

public class PairHelpRecommendationDto
{
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string OrganizationName { get; set; } = string.Empty;
    public List<string> WeaknessTags { get; set; } = new();
    public double Score { get; set; }
    public string MatchReason { get; set; } = string.Empty;
}

public class PairHelpRecommendResponse
{
    public List<PairHelpRecommendationDto> Recommendations { get; set; } = new();
}

public class PairHelpRequestDto
{
    public int HelperId { get; set; }
}

public class PairHelpMyDto
{
    public int RecordId { get; set; }
    public int PartnerId { get; set; }
    public string PartnerName { get; set; } = string.Empty;
    public string PartnerOrgName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public List<string> WeaknessTags { get; set; } = new();
}

public class PairHelpMyResponse
{
    public List<PairHelpMyDto> Active { get; set; } = new();
    public List<PairHelpMyDto> History { get; set; } = new();
}

public class PairHelpCompleteRequest
{
    public string? OutcomeSummary { get; set; }
}

public class PairHelpLogRequest
{
    public string Content { get; set; } = string.Empty;
}

// ========== (11) 党史 PK 对战 ==========
public class CreateBattleRequest
{
    public int OpponentId { get; set; }
    public int QuestionCount { get; set; } = 5;
    public int? Difficulty { get; set; }
}

public class CreateBattleResponse
{
    public int GameId { get; set; }
    public int QuestionCount { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public class BattlePendingDto
{
    public int GameId { get; set; }
    public int ChallengerId { get; set; }
    public string ChallengerName { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class BattleQuestionDto
{
    public int QuestionId { get; set; }
    public string Stem { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int Score { get; set; }
}

public class BattleCurrentQuestionResponse
{
    public int GameId { get; set; }
    public int Index { get; set; }
    public int Total { get; set; }
    public BattleQuestionDto? Question { get; set; }
    public int MyScore { get; set; }
    public int OpponentScore { get; set; }
    public string OpponentName { get; set; } = string.Empty;
}

public class BattleAnswerRequest
{
    public int QuestionId { get; set; }
    public string Answer { get; set; } = string.Empty;
    public int? DurationMs { get; set; }
}

public class BattleAnswerResponse
{
    public bool Correct { get; set; }
    public string CorrectAnswer { get; set; } = string.Empty;
    public int MyScore { get; set; }
    public int OpponentScore { get; set; }
    public int NextIndex { get; set; }
}

public class BattleResultResponse
{
    public int? WinnerId { get; set; }
    public string WinnerName { get; set; } = string.Empty;
    public int MyScore { get; set; }
    public int OpponentScore { get; set; }
    public bool IsDraw { get; set; }
    public string Result { get; set; } = string.Empty;
}

// ========== (12) 红色教育基地 ==========
public class EducationSiteDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Description { get; set; }
    public string? HistoricalFacts { get; set; }
    public string? AiInterpretation { get; set; }
    public string? CoverUrl { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class EducationSiteQueryParams : PagedQueryParams
{
    public string? Keyword { get; set; }
}

public class EducationSiteCheckinDto
{
    public int Id { get; set; }
    public int PartyMemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public DateTime CheckInTime { get; set; }
    public string? Note { get; set; }
    public string? AiBackgroundInterpretation { get; set; }
}

// ========== (13) 学习路线图 ==========
public class LearningRoadmapRequest
{
    public int? MemberId { get; set; }
    public string? Target { get; set; }
    public List<string>? FocusTags { get; set; }
    public int PeriodDays { get; set; } = 30;
    public string? Difficulty { get; set; }
}

public class RoadmapContentDto
{
    public int? ContentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int ContentType { get; set; }
    public string Source { get; set; } = "library";
    public string Reason { get; set; } = string.Empty;
}

public class RoadmapExamDto
{
    public int SuggestedCount { get; set; }
    public double? TargetScore { get; set; }
}

public class RoadmapKpiDto
{
    public string Metric { get; set; } = string.Empty;
    public double Target { get; set; }
}

public class RoadmapStageDto
{
    public int StageNo { get; set; }
    public string StageName { get; set; } = string.Empty;
    public int DurationDays { get; set; }
    public List<string> Objectives { get; set; } = new();
    public List<RoadmapContentDto> Contents { get; set; } = new();
    public RoadmapExamDto? Exam { get; set; }
    public List<RoadmapKpiDto> Kpis { get; set; } = new();
}

public class LearningRoadmapResponse
{
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string CurrentLevel { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public List<string> FocusTags { get; set; } = new();
    public int TotalDays { get; set; }
    public List<RoadmapStageDto> Stages { get; set; } = new();
    public string NextAction { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
}

// ========== (14) 精准分层推送 ==========
public class TargetedSendFilter
{
    public int? OrganizationId { get; set; }
    public bool IncludeDescendants { get; set; } = true;
    public List<int>? Roles { get; set; }
    public List<string>? MemberTypes { get; set; }
    public List<int>? ExcludeMemberIds { get; set; }
    public bool OnlyEnabled { get; set; } = true;
}

public class TargetedSendRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Type { get; set; }
    public TargetedSendFilter? Filter { get; set; }
    public bool DryRun { get; set; } = false;
}

public class TargetedSendResponse
{
    public int MatchedCount { get; set; }
    public List<int> MatchedMemberIds { get; set; } = new();
    public int SentCount { get; set; }
    public int SkippedCount { get; set; }
}

// ========== (15) 防挂机 ==========
public class AntiCheatChallengeQuestionDto
{
    public int QuestionId { get; set; }
    public string Stem { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int QuestionType { get; set; }
}

public class AntiCheatChallengeResponse
{
    public string ChallengeId { get; set; } = string.Empty;
    public AntiCheatChallengeQuestionDto? Question { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int? ContentId { get; set; }
}

public class AntiCheatVerifyResponseV2
{
    public bool IsValid { get; set; }
    public bool Correct { get; set; }
    public string Message { get; set; } = string.Empty;
    public int ValidSeconds { get; set; }
}

public class AntiCheatStatsOverviewDto
{
    public int TotalChecks { get; set; }
    public int PassCount { get; set; }
    public int FailCount { get; set; }
    public double PassRate { get; set; }
    public double EffectiveMinutes { get; set; }
    public List<AntiCheatStatsMemberDto> ByMember { get; set; } = new();
}

public class AntiCheatStatsMemberDto
{
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string OrganizationName { get; set; } = string.Empty;
    public int Checks { get; set; }
    public int Passes { get; set; }
    public int Fails { get; set; }
    public double EffectiveMinutes { get; set; }
}

// ========== (12) AI 学习预警 ==========
public class LearningWarningItemDto
{
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public string WarningType { get; set; } = string.Empty; // low_accuracy | task_overdue | low_activity | duration_abnormal
    public string WarningTypeText { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public double? MetricValue { get; set; }
    public double? Threshold { get; set; }
    public string? Suggestion { get; set; }
    public DateTime DetectedAt { get; set; }
}

public class LearningWarningResponse
{
    public DateTime GeneratedAt { get; set; }
    public int TotalWarnings { get; set; }
    public List<LearningWarningItemDto> Warnings { get; set; } = new();
    public Dictionary<string, int> TypeBreakdown { get; set; } = new();
}

public class LearningWarningTriggerResponse
{
    public int ScannedCount { get; set; }
    public int WarningCount { get; set; }
    public int NotificationSentCount { get; set; }
    public List<LearningWarningItemDto> Warnings { get; set; } = new();
}
