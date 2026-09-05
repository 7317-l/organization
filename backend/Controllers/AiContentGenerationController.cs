using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PartySchoolApi.Data;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Models.Entities;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Controllers;

[ApiController]
[Route("api/v1/ai-content")]
[Authorize(Roles = "SystemAdmin,BranchSecretary")]
public class AiContentGenerationController : ControllerBase
{
    private readonly IAiContentGenerationService _service;
    private readonly AppDbContext _context;
    private readonly IQwenService _qwen;

    public AiContentGenerationController(IAiContentGenerationService service, AppDbContext context, IQwenService qwen)
    {
        _service = service;
        _context = context;
        _qwen = qwen;
    }

    [HttpPost("generate")]
    public async Task<ApiResponse> Generate([FromBody] AiGenerateContentRequest request)
    {
        return ApiResponse.Success(await _service.GenerateAsync(request), "生成成功");
    }

    /// <summary>文件上传提取文本（支持txt/md，PDF/Word尝试提取）</summary>
    [HttpPost("upload")]
    public async Task<ApiResponse> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return ApiResponse.Fail("请选择文件");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        string text = string.Empty;

        if (ext == ".txt" || ext == ".md" || ext == ".csv")
        {
            using var reader = new StreamReader(file.OpenReadStream(), System.Text.Encoding.UTF8);
            text = await reader.ReadToEndAsync();
        }
        else if (ext == ".pdf" || ext == ".docx" || ext == ".doc")
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var bytes = ms.ToArray();
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] >= 32 && bytes[i] < 127)
                    sb.Append((char)bytes[i]);
                else if (bytes[i] == 10 || bytes[i] == 13)
                    sb.Append('\n');
            }
            text = sb.ToString();
            if (text.Length > 10000) text = text[..10000];
        }
        else
        {
            return ApiResponse.Fail("不支持的文件格式，请上传txt/md/pdf/docx文件");
        }

        return ApiResponse.Success(new
        {
            fileName = file.FileName,
            fileSize = file.Length,
            extractedText = text,
            charCount = text.Length
        }, "文件解析成功");
    }

    /// <summary>审核通过题目，一键入库到questions表</summary>
    [HttpPost("approve-question")]
    public async Task<ApiResponse> ApproveQuestion([FromBody] ApproveQuestionRequest request)
    {
        var category = await _context.QuestionCategories.FirstOrDefaultAsync(c => c.Name == request.Category || c.Id == request.CategoryId);
        var categoryId = category?.Id;

        var question = new Question
        {
            CategoryId = categoryId,
            QuestionType = Enum.Parse<QuestionType>(request.QuestionType ?? "SingleChoice"),
            Stem = request.Content,
            Options = request.Options != null ? System.Text.Json.JsonSerializer.Serialize(request.Options) : "[]",
            CorrectAnswer = request.CorrectAnswer,
            Score = request.Score ?? 5,
            CreatedAt = DateTime.Now
        };
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();
        return ApiResponse.Success(new { questionId = question.Id }, "题目已入库");
    }

    /// <summary>审核通过学习内容，一键入库到learningcontents表</summary>
    [HttpPost("approve-content")]
    public async Task<ApiResponse> ApproveContent([FromBody] ApproveContentRequest request)
    {
        var category = await _context.ContentCategories.FirstOrDefaultAsync(c => c.Name == request.Category || c.Id == request.CategoryId);
        var categoryId = category?.Id;

        var content = new LearningContent
        {
            CategoryId = categoryId,
            Title = request.Title,
            ContentType = Enum.Parse<ContentType>(request.ContentType ?? "Article"),
            Body = request.Content,
            IsPublic = true,
            CreatedAt = DateTime.Now
        };
        _context.LearningContents.Add(content);
        await _context.SaveChangesAsync();
        return ApiResponse.Success(new { contentId = content.Id }, "学习内容已入库");
    }
}

public class ApproveQuestionRequest
{
    public int? CategoryId { get; set; }
    public string? Category { get; set; }
    public string? QuestionType { get; set; }
    public string Content { get; set; } = string.Empty;
    public List<string>? Options { get; set; }
    public string CorrectAnswer { get; set; } = string.Empty;
    public int? Score { get; set; }
}

public class ApproveContentRequest
{
    public int? CategoryId { get; set; }
    public string? Category { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public string Content { get; set; } = string.Empty;
}
