namespace PartySchoolApi.Models.Common;

/// <summary>
/// 统一API响应结构
/// </summary>
public class ApiResponse
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }

    public static ApiResponse Success(object? data = null, string message = "操作成功")
        => new() { Code = 200, Message = message, Data = data };

    public static ApiResponse Fail(string message = "操作失败", int code = 400)
        => new() { Code = code, Message = message, Data = null };

    public static ApiResponse Unauthorized(string message = "未授权")
        => new() { Code = 401, Message = message, Data = null };

    public static ApiResponse Forbidden(string message = "无权限")
        => new() { Code = 403, Message = message, Data = null };

    public static ApiResponse NotFound(string message = "资源不存在")
        => new() { Code = 404, Message = message, Data = null };
}

/// <summary>
/// 分页响应结构
/// </summary>
public class PagedResponse
{
    public int Code { get; set; } = 200;
    public string Message { get; set; } = "查询成功";
    public object? Data { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
    public long Total { get; set; }

    public static PagedResponse Ok(object? data, int page, int size, long total)
        => new() { Data = data, Page = page, Size = size, Total = total };
}

/// <summary>
/// 分页查询参数基类
/// </summary>
public class PagedQueryParams
{
    private int _page = 1;
    private int _size = 10;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int Size
    {
        get => _size;
        set => _size = value is < 1 or > 100 ? 10 : value;
    }
}
