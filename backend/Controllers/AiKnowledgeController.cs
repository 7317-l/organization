using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Controllers;

[ApiController]
[Route("api/v1/ai-knowledge")]
[Authorize]
public class AiKnowledgeController : ControllerBase
{
    private readonly IAiKnowledgeService _service;
    private readonly IQwenService _qwen;

    private static readonly JsonSerializerOptions SseJsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AiKnowledgeController(IAiKnowledgeService service, IQwenService qwen)
    {
        _service = service;
        _qwen = qwen;
    }

    [HttpPost("query")]
    public async Task<ApiResponse> Query([FromBody] AiKnowledgeQueryRequest request)
    {
        return ApiResponse.Success(await _service.QueryAsync(request));
    }

    /// <summary>
    /// 流式问答（SSE）：先下发 meta 事件（会话ID/引用来源/置信度），再逐块下发 delta 事件（回答增量），最后 done。
    /// </summary>
    [HttpPost("query-stream")]
    public async Task QueryStream([FromBody] AiKnowledgeQueryRequest request, CancellationToken ct)
    {
        Response.Headers["Content-Type"] = "text/event-stream; charset=utf-8";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers["X-Accel-Buffering"] = "no";

        var prepared = await _service.PrepareStreamAsync(request);

        // 首帧：元数据
        await WriteEventAsync("meta", JsonSerializer.Serialize(prepared.Meta, SseJsonOpts));

        // 兜底回答（未配置Key / 空问题）
        if (!string.IsNullOrEmpty(prepared.FallbackAnswer))
        {
            await WriteEventAsync("done", JsonSerializer.Serialize(new { fallback = prepared.FallbackAnswer }, SseJsonOpts));
            return;
        }

        var messages = new List<QwenChatMessage>
        {
            QwenChatMessage.System(prepared.SystemPrompt),
            QwenChatMessage.User(prepared.UserPrompt)
        };

        try
        {
            // 寒暄类回答提高随机性（每次变换表达），知识类保持低温度保证准确
            var temperature = prepared.Meta.QuestionType == "chat" ? 0.95 : 0.4;
            await foreach (var piece in _qwen.ChatStreamAsync(messages, temperature: temperature, cancellationToken: ct))
            {
                await WriteEventAsync("delta", JsonSerializer.Serialize(new { text = piece }, SseJsonOpts));
            }
        }
        catch (OperationCanceledException)
        {
            // 客户端断开，正常结束
        }
        catch (Exception ex)
        {
            await WriteEventAsync("error", JsonSerializer.Serialize(new { message = "回答生成中断：" + ex.Message }, SseJsonOpts));
        }

        await WriteEventAsync("done", "{}");
    }

    /// <summary>
    /// 错题分析流式接口（SSE）：AI 感应当前题目后，对单题做深度解析。
    /// 事件序列：meta → delta（分析内容逐块） → done；出错下发 error。
    /// </summary>
    [HttpPost("analyze-stream")]
    public async Task AnalyzeStream([FromBody] AiQuestionAnalyzeRequest request, CancellationToken ct)
    {
        Response.Headers["Content-Type"] = "text/event-stream; charset=utf-8";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers["X-Accel-Buffering"] = "no";

        var prepared = await _service.PrepareAnalyzeStreamAsync(request);

        await WriteEventAsync("meta", JsonSerializer.Serialize(prepared.Meta, SseJsonOpts));

        if (!string.IsNullOrEmpty(prepared.FallbackAnswer))
        {
            await WriteEventAsync("done", JsonSerializer.Serialize(new { fallback = prepared.FallbackAnswer }, SseJsonOpts));
            return;
        }

        var messages = new List<QwenChatMessage>
        {
            QwenChatMessage.System(prepared.SystemPrompt),
            QwenChatMessage.User(prepared.UserPrompt)
        };

        try
        {
            // 错题分析需要条理清晰，温度适中
            await foreach (var piece in _qwen.ChatStreamAsync(messages, temperature: 0.6, cancellationToken: ct))
            {
                await WriteEventAsync("delta", JsonSerializer.Serialize(new { text = piece }, SseJsonOpts));
            }
        }
        catch (OperationCanceledException)
        {
            // 客户端断开，正常结束
        }
        catch (Exception ex)
        {
            await WriteEventAsync("error", JsonSerializer.Serialize(new { message = "分析生成中断：" + ex.Message }, SseJsonOpts));
        }

        await WriteEventAsync("done", "{}");
    }

    private async Task WriteEventAsync(string eventName, string data)
    {
        await Response.WriteAsync($"event: {eventName}\ndata: {data}\n\n");
        await Response.Body.FlushAsync();
    }
}
