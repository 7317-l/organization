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
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = ApiResponse.Fail(
            message: ex.InnerException != null ? ex.InnerException.Message : ex.Message,
            code: 500
        );

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
