# 业务服务 (Services/)

接口定义在 `Interfaces/`，实现在 `Implementations/`。

## 核心服务

| 服务 | 职责 | 关键方法 |
|------|------|---------|
| IAuthService | 身份认证 | LoginAsync, RefreshTokenAsync, ChangePasswordAsync |
| IPartyMemberService | 党员管理 | GetPagedAsync（书记自动过滤本支部）, ImportAsync, ExportAsync |
| IOrganizationService | 组织管理 | GetTreeAsync（递归三层）, GetStatsAsync（递归汇总下级） |
| ILearningTaskService | 学习任务 | CreateAsync（自动发通知）, UrgeAsync（催办发通知） |
| IExamService | 考试管理 | CreateTestAsync（自动发通知）, GeneratePracticeAsync（随机抽题） |
| IMeetingActivityService | 组织生活 | GenerateBriefAsync（AI总结）, ReviewAsync, ArchiveAsync, ReportAsync |
| IPartyDevelopmentService | 党员发展 | CheckMaterialsV2Async（千问分析）, GetReportSuggestionAsync, AdvanceStageAsync |
| INotificationService | 消息通知 | SendAsync, TargetedSendAsync（精准分层推送）, GetUnreadAsync |
| IStatisticsService | 数据统计 | GetDashboardOverviewAsync, GetLargeScreenDashboardAsync, GetBranchStatisticsAsync |
| IPointService | 积分管理 | AddPointsAsync, GetRecordsAsync, GetRankingAsync |
| IBattleService | 党史PK | CreateBattleAsync, AcceptBattleAsync, SubmitAnswerAsync（状态机校验） |
| IPairHelpService | 结对帮扶 | RecommendAsync（互补评分）, RequestPairAsync, CompletePairAsync |
| IAntiCheatService | 防挂机 | GenerateChallengeV2Async（真实题库抽题）, VerifyV2Async |
| IMobileService | 党员端 | ReportProgressAsync（防挂机检测）, GetMyContentsAsync, SubmitExamAsync |
| IAiService | AI核心 | GetRecommendationsAsync, GenerateStarMembersAsync, GetLearningWarningsAsync |
| IAiKnowledgeService | RAG问答 | QueryAsync（BM25检索+千问生成+来源引用） |
| IAiContentGenerationService | AI内容生成 | GenerateAsync, UploadFileAsync, ApproveQuestionAsync |
| INl2SqlService | NL2SQL | QueryAsync（安全过滤）, GetHistoryAsync |
| IKMeansService | KMeans聚类 | ClusterAsync（K-means+++迭代收敛） |
| IDataPermissionService | 数据权限 | GetAccessibleOrgIdsAsync, CanAccessMemberAsync, CanAccessOrgAsync |
| IQwenService | 千问大模型 | ChatAsync, ChatWithJsonModeAsync |

## 公共工具

- `OrgHierarchyHelper.CollectOrgAndDescendantIds` — 递归收集组织及下级ID
- `MaskSensitiveBeforeAi` — AI调用前数据脱敏（姓名/手机号）
- `CalculateRealErrorStatsAsync` — 从考试记录逐题判题计算真实错题率
