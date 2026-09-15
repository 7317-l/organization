using System.ComponentModel.DataAnnotations;

namespace PartySchoolApi.Models.DTOs;

/// <summary>组织树节点</summary>
public class OrganizationTreeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrganizationTreeDto> Children { get; set; } = new();
}

/// <summary>创建组织请求</summary>
public class CreateOrganizationRequest
{
    [Required(ErrorMessage = "组织名称不能为空")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int? ParentId { get; set; }
}

/// <summary>更新组织请求</summary>
public class UpdateOrganizationRequest
{
    [Required(ErrorMessage = "组织名称不能为空")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}

/// <summary>组织统计概览</summary>
public class OrganizationStatsDto
{
    public int OrgId { get; set; }
    public string OrgName { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public int SubOrgCount { get; set; }
    public int OngoingTaskCount { get; set; }
    public double TaskCompletionRate { get; set; }
    public double TodayLearningMinutes { get; set; }
}
