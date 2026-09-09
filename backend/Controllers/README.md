# API 控制器 (Controllers/)

26个控制器，174个API端点。路由前缀统一为 `/api/v1`。

## 业务控制器（25个）

| 控制器 | 路由前缀 | 职责 |
|--------|---------|------|
| AuthController | /auth | 登录、刷新Token、改密码、当前用户 |
| PartyMembersController | /members | 党员CRUD、批量导入导出、角色分配（数据权限） |
| OrganizationsController | /organizations | 组织树、CRUD、组织统计（数据权限） |
| LearningContentsController | /contents | 学习内容CRUD、分类标签管理 |
| LearningTasksController | /tasks | 学习任务派发、催办、完成情况 |
| ExamPapersController | /exam-papers | 试卷管理 |
| ExamTestsController | /exam-tests | 考试发布、成绩、专项练习随机抽题 |
| QuestionsController | /questions | 题库CRUD、分类管理 |
| MeetingActivityController | /meeting-activities | 组织生活发布/报名/签到/心得/AI总结/审核/归档/上报 |
| PartyDevelopmentController | /party-development | 党员发展台账、审核、阶段推进、AI材料校验 |
| NotificationController | /notifications | 消息通知、已读未读、精准分层推送 |
| StatisticsController | /statistics | 统计概览、学习/考试趋势、支部统计、大屏、防挂机统计 |
| PointController | /points | 积分明细、我的积分、排行榜（数据权限） |
| CheckInController | /check-in | 签到、AI签到背景知识 |
| EducationSiteController | /education-sites | 红色基地管理、打卡、我的打卡记录 |
| BattleController | /battle | 党史PK创建/应战/答题/弃权/结果（状态机） |
| PairHelpController | /pair-help | 结对帮扶推荐/申请/接受/记录/完成 |
| AntiCheatController | /anti-cheat | 防挂机验证题获取/提交、统计概览 |
| MobileController | /mobile | 党员端接口（内容/任务/考试/进度上报，防挂机检测） |
| RectificationController | /rectifications | 支部整改项管理 |
| AiController | /ai | AI推荐/报告/标兵/路线图/预警 |
| AiKnowledgeController | /ai-knowledge | RAG知识问答（BM25检索+千问生成） |
| AiContentGenerationController | /ai-content | AI内容生成、文件上传、审核入库 |
| Nl2SqlController | /nl2sql | 自然语言查数据（安全过滤+多轮会话） |
| KMeansController | /kmeans | 错题K-Means聚类 |

## 占位控制器

- WeatherForecastController — 模板默认，无业务用途

## 数据权限说明

- `[Authorize(Roles = "SystemAdmin,BranchSecretary")]` — 管理端接口
- 书记角色：PartyMembers/Statistics/Organizations/Point 已接入数据权限，仅可见本支部及下级
- 党员角色：Mobile/Notification/Battle/AntiCheat 等使用 `_currentUser.UserId` 强制限定本人
