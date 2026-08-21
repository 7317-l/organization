namespace PartySchoolApi.Helpers;

/// <summary>
/// 密码哈希工具类（使用BCrypt）
/// </summary>
public static class PasswordHelper
{
    /// <summary>
    /// 哈希密码
    /// </summary>
    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 10);
    }

    /// <summary>
    /// 验证密码
    /// </summary>
    public static bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}
