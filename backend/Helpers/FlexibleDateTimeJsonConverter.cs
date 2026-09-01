using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PartySchoolApi.Helpers;

/// <summary>
/// 灵活的 DateTime 转换器：兼容前端 Element Plus 日期组件常见的
/// "yyyy-MM-dd HH:mm:ss"（空格分隔）、ISO 8601（T 分隔）、以及纯日期等格式。
/// 解决发布测验等接口因 deadline 格式不被 System.Text.Json 默认解析而报错的问题。
/// </summary>
public class FlexibleDateTimeJsonConverter : JsonConverter<DateTime>
{
    private static readonly string[] Formats =
    {
        "yyyy-MM-ddTHH:mm:ss.fffffffK",
        "yyyy-MM-ddTHH:mm:ss.fffK",
        "yyyy-MM-ddTHH:mm:ssK",
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-dd HH:mm:ss",
        "yyyy-MM-dd HH:mm",
        "yyyy-MM-dd",
        "yyyy/MM/dd HH:mm:ss",
        "yyyy/MM/dd HH:mm",
        "yyyy/MM/dd",
        "yyyy-M-d H:mm:ss",
        "yyyy-M-d H:mm"
    };

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();
            if (!string.IsNullOrWhiteSpace(s))
            {
                var trimmed = s.Trim();
                foreach (var f in Formats)
                {
                    if (DateTime.TryParseExact(trimmed, f, CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out var dt))
                        return dt;
                }
                // 兜底：宽松解析（处理未预见的格式），失败则抛异常由模型验证返回 400
                if (DateTime.TryParse(trimmed, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var dt2))
                    return dt2;
                throw new JsonException($"无法解析日期时间: {s}");
            }
            throw new JsonException("日期时间不能为空");
        }
        if (reader.TokenType == JsonTokenType.Null)
            return default;
        return reader.GetDateTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // 统一输出无时区的 ISO 格式，前端可直接展示或 new Date()
        writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture));
    }
}
