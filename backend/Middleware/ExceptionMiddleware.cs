using System.Net;
using System.Text.Json;
using PartySchoolApi.Models.Common;

namespace PartySchoolApi.Middleware;

/// <summary>
/// 全局异常处理中间件
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "未处理的异常: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        int statusCode;
        int code;
        string message;

        if (ex is BusinessException bizEx)
        {
            // 业务异常：使用自定义状态码
            statusCode = bizEx.Code;
            code = bizEx.Code;
            message = bizEx.Message;
        }
        else
        {
            // 未处理异常：500
            statusCode = (int)HttpStatusCode.InternalServerError;
            code = 500;
            message = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        context.Response.StatusCode = statusCode;

        var response = ApiResponse.Fail(message: message, code: code);

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return context.Response.WriteAsync(json);
    }
}

/// <summary>
/// 业务异常类
/// </summary>
public class BusinessException : Exception
{
    public int Code { get; }

    public BusinessException(string message, int code = 400) : base(message)
    {
        Code = code;
    }
}
