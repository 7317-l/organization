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

/// <summary>提交答案请求</summary>
public class SubmitExamRequest
{
    public int TestId { get; set; }

    /// <summary>答案字典，key为题目Id，value为所选答案</summary>
    public Dictionary<string, string> Answers { get; set; } = new();
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
}
