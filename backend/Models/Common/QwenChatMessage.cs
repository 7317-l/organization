namespace PartySchoolApi.Models.Common;

/// <summary>
/// 千问聊天消息
/// </summary>
public class QwenChatMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    public static QwenChatMessage System(string content) => new() { Role = "system", Content = content };
    public static QwenChatMessage User(string content) => new() { Role = "user", Content = content };
    public static QwenChatMessage Assistant(string content) => new() { Role = "assistant", Content = content };
}
