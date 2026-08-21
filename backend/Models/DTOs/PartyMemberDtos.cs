using System.ComponentModel.DataAnnotations;
using PartySchoolApi.Models.Common;

namespace PartySchoolApi.Models.DTOs;

/// <summary>党员分页查询参数</summary>
public class MemberQueryParams : PagedQueryParams
{
    public string? Name { get; set; }
    public int? OrganizationId { get; set; }
    public UserRole? Role { get; set; }
    public bool? IsEnabled { get; set; }
}

/// <summary>党员列表项</summary>
public class MemberListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public int OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>创建党员请求</summary>
public class CreateMemberRequest
{
    [Required(ErrorMessage = "姓名不能为空")]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "手机号不能为空")]
    [Phone]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "初始密码不能为空")]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.PartyMember;

    [Required(ErrorMessage = "所属支部不能为空")]
    public int OrganizationId { get; set; }
}

/// <summary>更新党员请求</summary>
public class UpdateMemberRequest
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Phone]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    public UserRole Role { get; set; }
    public int OrganizationId { get; set; }
    public bool IsEnabled { get; set; } = true;
}

/// <summary>分配角色请求</summary>
public class AssignRoleRequest
{
    public UserRole Role { get; set; }
}

/// <summary>批量导入结果</summary>
public class ImportResultDto
{
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailCount { get; set; }
    public List<string> Errors { get; set; } = new();
}
