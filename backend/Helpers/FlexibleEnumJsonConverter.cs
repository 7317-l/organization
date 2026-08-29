using System.Text.Json;
using System.Text.Json.Serialization;

namespace PartySchoolApi.Helpers;

/// <summary>
/// 灵活枚举 JSON 转换器：
/// - 序列化：输出数字（与前端现有展示逻辑兼容，避免大面积改动）
/// - 反序列化：同时兼容「数字」「枚举名称（不区分大小写）」「数字字符串」以及常见前端简写别名
/// 解决前端以字符串传枚举（role / contentType / questionType 等）时接口返回 400 的问题。
/// </summary>
public class FlexibleEnumJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert.IsEnum;

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(FlexibleEnumJsonConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter?)Activator.CreateInstance(converterType);
    }

    private class FlexibleEnumJsonConverter<T> : JsonConverter<T> where T : struct, Enum
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
                return (T)Enum.ToObject(typeToConvert, reader.GetInt32());

            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString()?.Trim();
                if (string.IsNullOrEmpty(str)) return default;

                // 1) 按枚举名称解析（不区分大小写）
                if (Enum.TryParse(typeToConvert, str, ignoreCase: true, out var parsed))
                    return (T)parsed;

                // 2) 按数字字符串解析
                if (int.TryParse(str, out var num) && Enum.IsDefined(typeToConvert, num))
                    return (T)Enum.ToObject(typeToConvert, num);

                // 3) 常见前端简写别名映射
                if (TryGetAlias(typeToConvert, str, out var alias))
                    return (T)alias;

                throw new JsonException($"无法将值 \"{str}\" 转换为枚举 {typeToConvert.Name}");
            }

            throw new JsonException($"不支持的反序列化类型 {reader.TokenType}");
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(Convert.ToInt32(value));
        }

        private static bool TryGetAlias(Type enumType, string raw, out object result)
        {
            result = default!;
            var key = raw.ToLowerInvariant();
            if (enumType == typeof(Models.Common.UserRole))
            {
                if (key == "member") { result = Models.Common.UserRole.PartyMember; return true; }
                if (key == "secretary") { result = Models.Common.UserRole.BranchSecretary; return true; }
                if (key == "admin") { result = Models.Common.UserRole.SystemAdmin; return true; }
            }
            else if (enumType == typeof(Models.Common.QuestionType))
            {
                if (key == "single") { result = Models.Common.QuestionType.SingleChoice; return true; }
                if (key == "multiple") { result = Models.Common.QuestionType.MultiChoice; return true; }
                if (key == "judge") { result = Models.Common.QuestionType.TrueFalse; return true; }
            }
            else if (enumType == typeof(Models.Common.ContentType))
            {
                if (key == "article") { result = Models.Common.ContentType.Article; return true; }
                if (key == "video") { result = Models.Common.ContentType.Video; return true; }
            }
            return false;
        }
    }
}
