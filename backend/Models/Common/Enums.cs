namespace PartySchoolApi.Models.Common;

/// <summary>用户角色</summary>
public enum UserRole
{
    PartyMember = 0,
    BranchSecretary = 1,
    SystemAdmin = 2
}

/// <summary>内容类型</summary>
public enum ContentType
{
    Article = 0,
    Video = 1
}

/// <summary>题目类型</summary>
public enum QuestionType
{
    SingleChoice = 0,
    MultiChoice = 1,
    TrueFalse = 2
}

// ===== 以下为新增枚举 =====

/// <summary>党员发展阶段</summary>
public enum PartyDevelopmentStage
{
    Activist = 0,        // 积极分子
    DevelopmentTarget = 1, // 发展对象
    ProbationaryMember = 2, // 预备党员
    FullMember = 3        // 正式党员
}

/// <summary>流程审批状态</summary>
public enum ProcessStatus
{
    PendingSubmit = 0,   // 待提交
    UnderReview = 1,     // 审核中
    Approved = 2,        // 已通过
    Rejected = 3         // 已驳回
}

/// <summary>三会一课/主题党日活动类型</summary>
public enum MeetingType
{
    BranchGeneralMeeting = 0,  // 支部党员大会
    BranchCommittee = 1,       // 支部委员会
    PartyGroupMeeting = 2,     // 党小组会
    PartyLecture = 3,          // 党课
    ThemePartyDay = 4          // 主题党日
}

/// <summary>积分来源类型</summary>
public enum PointSourceType
{
    WatchVideo = 0,     // 观看视频
    CompleteExam = 1,   // 完成答题
    ActivityCheckIn = 2, // 活动打卡
    BattleVictory = 3,  // 党史PK胜利
    Other = 4           // 其他
}

/// <summary>消息通知类型</summary>
public enum NotificationType
{
    TaskReminder = 0,    // 任务提醒
    ExamReminder = 1,    // 测验提醒
    WarningReminder = 2, // 预警提醒
    SystemNotice = 3     // 系统通知
}

/// <summary>学习内容来源类型</summary>
public enum ContentSourceType
{
    Manual = 0,             // 手动录入
    AiGenerated = 1,        // AI生成
    DocumentConverted = 2   // 文档转换
}
