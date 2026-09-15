using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using PartySchoolApi.Models.Common;

namespace PartySchoolApi.Models.DTOs;

/// <summary>内容分页查询参数</summary>
public class ContentQueryParams : PagedQueryParams
{
    /// <summary>标题关键词（兼容前端 keyword）</summary>
    public string? Title { get; set; }

    /// <summary>关键词搜索别名（前端传 keyword）</summary>
    [FromQuery(Name = "keyword")]
    public string? Keyword
    {
        get => Title;
        set => Title = value;
    }

    /// <summary>内容类型</summary>
    public ContentType? ContentType { get; set; }

    /// <summary>内容类型别名（前端传 type）</summary>
    [FromQuery(Name = "type")]
    public ContentType? Type
    {
        get => ContentType;
        set => ContentType = value;
    }

    public int? CategoryId { get; set; }
    public bool? IsPublic { get; set; }

    /// <summary>标签ID筛选</summary>
    public int? TagId { get; set; }
}

/// <summary>内容列表项</summary>
public class ContentListItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public ContentType ContentType { get; set; }
    public string ContentTypeName { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public bool IsPublic { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

/// <summary>内容详情</summary>
public class ContentDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Body { get; set; }
    public string? VideoUrl { get; set; }
    public ContentType ContentType { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public bool IsPublic { get; set; }
    public List<TagDto> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

/// <summary>创建内容请求</summary>
public class CreateContentRequest
{
    [Required(ErrorMessage = "标题不能为空")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Body { get; set; }
    public string? VideoUrl { get; set; }
    public ContentType ContentType { get; set; } = ContentType.Article;
    public int? CategoryId { get; set; }
    public bool IsPublic { get; set; } = true;
    public List<int> TagIds { get; set; } = new();
}

/// <summary>更新内容请求</summary>
public class UpdateContentRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    public string? Body { get; set; }
    public string? VideoUrl { get; set; }
    public ContentType ContentType { get; set; }
    public int? CategoryId { get; set; }
    public bool IsPublic { get; set; }
    public List<int> TagIds { get; set; } = new();
}

/// <summary>标签DTO</summary>
public class TagDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>创建标签请求</summary>
public class CreateTagRequest
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
}

/// <summary>内容分类树节点</summary>
public class ContentCategoryTreeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public List<ContentCategoryTreeDto> Children { get; set; } = new();
}

/// <summary>创建内容分类请求</summary>
public class CreateContentCategoryRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    public int? ParentId { get; set; }
}
