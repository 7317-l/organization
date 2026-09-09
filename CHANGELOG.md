# 党建学习系统 - 项目变更日志

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/)，
版本号遵循 [语义化版本](https://semver.org/lang/zh-CN/)。

## [Unreleased]

### 新增
- 数据权限分级：PartyMembersController/StatisticsController/OrganizationsController 接入 IDataPermissionService，书记仅可见本支部及下级
- 组织树递归构建：修复 EF Core Include 只加载一层的问题，完整显示三层组织架构
- IOrganizationService 新增 GetTreeAsync(List<int> orgIds) 过滤重载
- MemberQueryParams 新增 OrganizationIdList 字段用于数据权限过滤

### 修复
- 组织树接口只返回两层，第三层支部未显示（改为内存递归构建）
- 书记可越权查询任意组织党员和统计数据（Controller层加权限校验）
- 书记组织树返回空数组（修复 BuildTree 根节点判断逻辑）

---

## [1.2.0] - 2026-09-05

### 新增
- 防挂机主动验证：MobileService 接入 IAntiCheatService，学习超120秒需验证
- 组织生活闭环：MeetingActivityController 新增 signup/checkin/my-status 接口
- 党员端组织生活页面（OrgLife.vue）：活动列表+报名+签到+提交心得
- 党员端红色基地页面（EducationSites.vue）：基地浏览+打卡+历史记录
- 管理端全屏数字驾驶舱（FullScreenDashboard.vue）：暗色主题+ECharts多图表
- KMeans 真正聚类算法：K-means++质心初始化+欧氏距离+迭代收敛
- AI语音输入：AiChat.vue 集成 webkitSpeechRecognition
- RAG BM25 检索：KnowledgeSearchService 实现 TF+IDF+长度归一化

### 修复
- AI悬浮助手 4 处 localhost:3000 替换为 /api/v1/ai-knowledge/query
- 数字驾驶舱字段名不匹配（data.stats → data.overview/branchRankings/weaknessHeatmap）
- 党员发展页面 /party-members 404（改为 /members）
- 党员发展列表缺所属支部和审核人字段（DTO+Service 填充）
- PartyDevelopmentProcess 实体缺 ReviewerId 字段
- MeetingActivity 实体新增字段缺 [Column] snake_case 映射
- 党员端 OrgLife/EducationSites 数据解析为空（request.js 解包 data 后兼容 Array.isArray）
- 党史PK状态机：只有 status=1 才能答题，status=0 返回等待提示
- 个性化推荐/学习路线/个人报告：去掉伪随机数，改用真实错题数据
- AI数据脱敏：调用千问前对姓名/手机号脱敏，4处调用全覆盖
- 党员发展AI辅助：材料检查和思想汇报建议改用千问真正分析

---

## [1.1.0] - 2026-09-04

### 新增
- 31项党建功能（AI功能15项+基础功能16项）
- 后端25个业务Controller
- 前端管理端+党员端共22个页面
- 千问大模型集成（AI对话/报告/推荐/路线图）
- 党史PK双人对战
- 积分体系与排行榜
- 结对帮扶
- 红色教育基地打卡

### 基础功能
- 党员批量导入导出（Excel/CSV）
- 学习任务派发（素材→支部→截止时间）
- 消息通知中心
- 考试中心（待考/历史/错题/专项练习）
- 组织生活（发布/心得/AI总结）
- 党员发展台账
- 个人中心

---

## [1.0.0] - 2026-09-01

### 新增
- 项目初始化
- ASP.NET Core .NET 8 后端框架
- Vue3 + Element Plus 前端框架
- MySQL 数据库 + EF Core
- JWT 身份认证
- 基础党员/组织 CRUD
