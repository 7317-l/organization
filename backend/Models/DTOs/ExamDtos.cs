using System.ComponentModel.DataAnnotations;

namespace PartySchoolApi.Models.DTOs;

// ===== 试卷 =====

/// <summary>试卷列表项</summary>
public class ExamPaperListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int QuestionCount { get; set; }
    public int TotalScore { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>试卷详情</summary>
public class ExamPaperDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<QuestionListItemDto> Questions { get; set; } = new();
    public int TotalScore { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>创建试卷请求</summary>
public class CreateExamPaperRequest
{
    [Required(ErrorMessage = "试卷名称不能为空")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required(ErrorMessage = "请选择题目")]
    public List<int> QuestionIds { get; set; } = new();
}

/// <summary>更新试卷请求</summary>
public class UpdateExamPaperRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<int> QuestionIds { get; set; } = new();
}

// ===== 测验 =====

/// <summary>测验列表项</summary>
public class ExamTestListItemDto
{
    public int Id { get; set; }
    public int PaperId { get; set; }
    public string PaperName { get; set; } = string.Empty;
    public int TargetOrgId { get; set; }
    public string? TargetOrgName { get; set; }
    public int TimeLimitMinutes { get; set; }
    public DateTime Deadline { get; set; }
    public int ParticipantCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>创建测验请求</summary>
public class CreateExamTestRequest
{
    [Required(ErrorMessage = "请选择试卷")]
    public int PaperId { get; set; }

    [Required(ErrorMessage = "请选择目标支部")]
    public int TargetOrgId { get; set; }

    public int TimeLimitMinutes { get; set; } = 60;

    [Required(ErrorMessage = "请设置截止时间")]
    public DateTime Deadline { get; set; }
}

/// <summary>测验参与结果</summary>
public class ExamTestResultDto
{
    public int TestId { get; set; }
    public string PaperName { get; set; } = string.Empty;
    public int TotalParticipants { get; set; }
    public double AverageScore { get; set; }
    public double PassRate { get; set; }
    public List<MemberTestRecordDto> Records { get; set; } = new();
}

/// <summary>党员考试记录DTO</summary>
public class MemberTestRecordDto
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public int Score { get; set; }
    public DateTime SubmittedAt { get; set; }
}

/// <summary>专项练习卷（随机抽题生成）</summary>
public class PracticePaperDto
{
    public string PracticeId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
    public int TotalScore { get; set; }
    public List<QuestionListItemDto> Questions { get; set; } = new();
}
