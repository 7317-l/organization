namespace PartySchoolApi.Models.DTOs;

public class AntiCheatChallengeDto
{
    public string ChallengeId { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public DateTime ExpireAt { get; set; }
}

public class AntiCheatVerifyRequest
{
    public string ChallengeId { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public int? ContentId { get; set; }
}

public class AntiCheatVerifyResponse
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class AntiCheatStatsDto
{
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    /// <summary>有效学习时长（分钟）</summary>
    public double ValidLearningMinutes { get; set; }
    /// <summary>挂机时长（分钟）</summary>
    public double IdleMinutes { get; set; }
    /// <summary>挂机占比</summary>
    public double IdleRate { get; set; }
    /// <summary>验证通过次数</summary>
    public int PassCount { get; set; }
    /// <summary>验证失败次数</summary>
    public int FailCount { get; set; }
}
