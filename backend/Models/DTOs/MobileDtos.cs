using System.ComponentModel.DataAnnotations;

namespace PartySchoolApi.Models.DTOs;

/// <summary>学习进度上报请求</summary>
public class ReportProgressRequest
{
    public int ContentId { get; set; }
    public int? TaskId { get; set; }
    public int DurationSeconds { get; set; }
    public bool IsCompleted { get; set; }
}

/// <summary>任务完成确认请求</summary>
public class CompleteTaskContentRequest
{
    public int TaskId { get; set; }
    public int ContentId { get; set; }
}

/// <summary>移动端任务列表项</summary>
public class MobileTaskDto
{
    public int Id { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public DateTime Deadline { get; set; }
    public int TotalContents { get; set; }
    public int CompletedContents { get; set; }
    public double CompletionRate { get; set; }
    public bool IsCompleted => CompletedContents >= TotalContents && TotalContents > 0;
}

/// <summary>移动端内容列表项（含个人学习进度）</summary>
public class MobileContentListItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int ContentType { get; set; }
    public string ContentTypeName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    /// <summary>学习进度百分比 0-100</summary>
    public int Progress { get; set; }
    /// <summary>学习状态：new / learning / done</summary>
    public string Status { get; set; } = "new";
    /// <summary>累计学习秒数</summary>
    public int DurationSeconds { get; set; }
    /// <summary>是否已完成</summary>
    public bool IsCompleted { get; set; }
}

/// <summary>开始测验响应（题目不含答案）</summary>
public class StartExamResponse
{
    public int TestId { get; set; }
    public string PaperName { get; set; } = string.Empty;
    public int TimeLimitMinutes { get; set; }
    public DateTime Deadline { get; set; }
    public List<ExamQuestionDto> Questions { get; set; } = new();
}

/// <summary>测验题目（不含答案）</summary>
public class ExamQuestionDto
{
    public int Id { get; set; }
    public int QuestionType { get; set; }
    public string Stem { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int Score { get; set; }
}

/// <summary>单题答案项（前端提交格式：{questionId, answer}）</summary>
public class SubmitAnswerItem
{
    public int QuestionId { get; set; }
    public string Answer { get; set; } = string.Empty;
}

/// <summary>提交答案请求</summary>
public class SubmitExamRequest
{
    public int TestId { get; set; }

    /// <summary>答案列表，每项包含 questionId 和 answer</summary>
    public List<SubmitAnswerItem> Answers { get; set; } = new();
}

/// <summary>提交测验响应</summary>
public class SubmitExamResponse
{
    public int RecordId { get; set; }
    public int Score { get; set; }
    public int TotalScore { get; set; }
    public bool IsPassed { get; set; }
}

/// <summary>测验结果详情</summary>
public class ExamResultDetailDto
{
    public int TestId { get; set; }
    public string PaperName { get; set; } = string.Empty;
    public int Score { get; set; }
    public int TotalScore { get; set; }
    public bool IsPassed { get; set; }
    public DateTime SubmittedAt { get; set; }
    public List<QuestionAnswerDto> QuestionAnswers { get; set; } = new();
}

/// <summary>题目作答详情</summary>
public class QuestionAnswerDto
{
    public int QuestionId { get; set; }
    public string Stem { get; set; } = string.Empty;
    public string UserAnswer { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int Score { get; set; }
    public int EarnedScore { get; set; }
}

/// <summary>个人学习概览</summary>
public class PersonalLearningOverviewDto
{
    public int TotalLearningMinutes { get; set; }
    public int CompletedContentCount { get; set; }
    public int CompletedTaskCount { get; set; }
    public int TotalTaskCount { get; set; }
    public int CompletedExamCount { get; set; }
    public double AverageExamScore { get; set; }
    public double TaskCompletionRate { get; set; }

    /// <summary>待办任务数（前端首页统计卡片用）</summary>
    public int PendingCount { get; set; }

    /// <summary>整体学习进度百分比（前端首页统计卡片用）</summary>
    public double LearningProgress { get; set; }

    /// <summary>总积分（前端首页统计卡片用）</summary>
    public int TotalPoints { get; set; }
}

/// <summary>移动端测验列表项</summary>
public class MobileExamTestDto
{
    public int Id { get; set; }
    public string PaperName { get; set; } = string.Empty;
    public int TimeLimitMinutes { get; set; }
    public DateTime Deadline { get; set; }
    public bool IsSubmitted { get; set; }
    public int? MyScore { get; set; }
    public int TotalScore { get; set; }

    /// <summary>测验状态：pending（待考）/ completed（已完成）/ expired（已截止）</summary>
    public string Status { get; set; } = "pending";
}
