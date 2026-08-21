using System.ComponentModel.DataAnnotations;
using PartySchoolApi.Models.Common;

namespace PartySchoolApi.Models.DTOs;

/// <summary>题目分页查询参数</summary>
public class QuestionQueryParams : PagedQueryParams
{
    public QuestionType? QuestionType { get; set; }
    public int? CategoryId { get; set; }
    public string? Keyword { get; set; }
}

/// <summary>题目列表项（含答案，管理端用）</summary>
public class QuestionListItemDto
{
    public int Id { get; set; }
    public QuestionType QuestionType { get; set; }
    public string QuestionTypeName { get; set; } = string.Empty;
    public string Stem { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public string CorrectAnswer { get; set; } = string.Empty;
    public int Score { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>创建题目请求</summary>
public class CreateQuestionRequest
{
    public QuestionType QuestionType { get; set; } = QuestionType.SingleChoice;

    [Required(ErrorMessage = "题干不能为空")]
    public string Stem { get; set; } = string.Empty;

    public List<string> Options { get; set; } = new();

    [Required(ErrorMessage = "正确答案不能为空")]
    public string CorrectAnswer { get; set; } = string.Empty;

    public int Score { get; set; } = 5;
    public int? CategoryId { get; set; }
}

/// <summary>更新题目请求</summary>
public class UpdateQuestionRequest
{
    public QuestionType QuestionType { get; set; }

    [Required]
    public string Stem { get; set; } = string.Empty;

    public List<string> Options { get; set; } = new();

    [Required]
    public string CorrectAnswer { get; set; } = string.Empty;

    public int Score { get; set; }
    public int? CategoryId { get; set; }
}

/// <summary>题目分类DTO</summary>
public class QuestionCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
}

/// <summary>创建题目分类请求</summary>
public class CreateQuestionCategoryRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}
