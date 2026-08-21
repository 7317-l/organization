using PartySchoolApi.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace PartySchoolApi.Models.DTOs;

/// <summary>任务分页查询参数</summary>
public class TaskQueryParams : PagedQueryParams
{
    public string? TaskName { get; set; }
    public int? TargetOrgId { get; set; }
}

/// <summary>任务列表项</summary>
public class TaskListItemDto
{
    public int Id { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public int TargetOrgId { get; set; }
    public string? TargetOrgName { get; set; }
    public DateTime Deadline { get; set; }
    public int ContentCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>任务详情</summary>
public class TaskDetailDto
{
    public int Id { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public int TargetOrgId { get; set; }
    public string? TargetOrgName { get; set; }
    public DateTime Deadline { get; set; }
    public List<ContentListItemDto> Contents { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

/// <summary>创建任务请求</summary>
public class CreateTaskRequest
{
    [Required(ErrorMessage = "任务名称不能为空")]
    [MaxLength(200)]
    public string TaskName { get; set; } = string.Empty;

    [Required(ErrorMessage = "目标支部不能为空")]
    public int TargetOrgId { get; set; }

    [Required(ErrorMessage = "截止时间不能为空")]
    public DateTime Deadline { get; set; }

    public List<int> ContentIds { get; set; } = new();
}

/// <summary>更新任务请求</summary>
public class UpdateTaskRequest
{
    [Required]
    [MaxLength(200)]
    public string TaskName { get; set; } = string.Empty;
    public int TargetOrgId { get; set; }
    public DateTime Deadline { get; set; }
    public List<int> ContentIds { get; set; } = new();
}

/// <summary>任务完成详情（每个党员进度）</summary>
public class TaskCompletionDetailDto
{
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public int TotalContents { get; set; }
    public int CompletedContents { get; set; }
    public double CompletionRate { get; set; }
    public int TotalLearningSeconds { get; set; }
}
