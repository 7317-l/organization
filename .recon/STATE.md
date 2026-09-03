# STATE.md — 项目现状基线（只读核查快照）

> 生成时间：2026-09-02（核查会话）；生成方式：全只读（dotnet build 仅编译验证、MySQL 仅 SELECT/SHOW、未跑迁移、未调千问、未 git 提交）
> 用途：为"大型功能补齐工程"的 15 项 AI/业务功能设计契约与并行施工提供基线。配套文件：`PATTERNS.md`（代码模式）、`CONTRACTS.md`（REST 契约）。

---

## 1. 基线核查结果

| 项 | 结果 |
|---|---|
| 后端构建 | `dotnet build`（backend 目录）**0 错误 / 16 警告**。警告全部为可空类 CS8602/CS8629，分布在 AiService、KMeansService、StatisticsService、MobileService、Nl2SqlService、OrganizationService、LearningTaskService、PartyDevelopmentService、MappingProfile，不影响编译与运行 |
| 前端依赖 | `frontend\frontend-admin\node_modules` ✅、`frontend\frontend-member\node_modules` ✅、根目录 `node_modules` ✅（均已安装） |
| git 分支 | `main`，与 `origin/main` 同步（无 ahead/behind）；未跟踪项仅 `docs/` 目录 |
| git 远端 | `https://github.com/7317-l/organization.git` |
| 后端运行态 | **5091 端口当前未监听**（后端未运行；未重启） |
| 前端端口 | admin `vite.config.js` port=5173（`open:true`）；member `vite.config.js` port=5173（带 `/api` 代理到 5091）。⚠️ 注意：member 实际配置为 5173 而非任务描述中的 5174，两端口冲突问题见 PATTERNS 坑 #10 |

## 2. 技术栈与环境

- 后端：ASP.NET Core **.NET 8**（SDK 10.0.302 可编译 net8.0 目标）；EF Core + MySQL；AutoMapper；BCrypt（PasswordHelper workFactor=10）；JWT Bearer
- 数据库：MySQL **8.0.46**（服务 MySQL80 运行中，localhost:3306），库 `party_school`，用户 root/123456
- 连接串：`Server=localhost;Port=3306;Database=party_school;User=root;Password=123456;CharSet=utf8mb4;SslMode=None`（appsettings.json）
- JWT：Key=`ThisIsASuperSecretKeyForPartySchoolJwtToken2026!@#$%`、Issuer=PartySchoolApi、Audience=PartySchoolClient、AccessToken 有效期 120min、RefreshToken 7 天
- 千问：BaseUrl=`https://dashscope.aliyuncs.com/compatible-mode/v1`、Model=qwen-plus；ApiKey 解析顺序 appsettings → 环境变量 `DASHSCOPE_API_KEY` → 根目录 `.env`。**appsettings.json 中 ApiKey 为空，实际靠根目录 `.env` 的 `DASHSCOPE_API_KEY=sk-w…`（已配置）**，另有 `.env` 覆盖 QWEN_BASE_URL/QWEN_MODEL。`IQwenService.IsConfigured` 据此判断是否启用真 AI
- 前端：Vite5 + Vue3（script setup）+ Element Plus + ECharts5 + Pinia + vue-router4 + axios
- 知识库：**文件系统** `knowledge\documents\`（当前仅 `测试知识库.txt` 一个文件，.txt/.md 均支持），**数据库无知识库表**

## 3. 数据库全量表结构（25 张表，实测 SHOW COLUMNS）

> 命名风格为**混合大小写**：`organizations`/`learning_tasks`/`member_learning_progress`/`member_test_records`/`questions`/`question_categories`/`exam_papers`/`content_categories`/`content_tags`/`tags`/`task_contents` 为小写下划线（实体带 `[Table]/[Column]` 注解）；`partymembers`/`learningcontents`/`examtests`/`learningpoints`/`meetingactivities`/`checkinrecords`/`battlerecords`/`pairhelprecords`/`partydevelopmentprocesses`/`messagenotifications`/`activityhearts`/`memberlearningreports`/`organizationquarterlyreports` 为 EF 默认 PascalCase（实体无注解）。**新表一律采用小写下划线命名（用户约定）**。

### 3.1 业务核心表
**partymembers**（党员/用户，PascalCase 列）
| 列 | 类型 | 说明 |
|---|---|---|
| Id | int PK AUTO | |
| Name | varchar(50) | 姓名 |
| Phone | varchar(20) UNIQUE | 手机号（登录账号） |
| PasswordHash | varchar(200) | bcrypt |
| Role | int | 0=党员 1=支部书记 2=系统管理员（Enums.UserRole） |
| OrganizationId | int | 所属组织 |
| IsEnabled | tinyint(1) | 是否启用 |
| RefreshToken | varchar(500) NULL | |
| RefreshTokenExpiry | datetime(6) NULL | |
| CreatedAt | datetime(6) | |
| PointTotal | int default 0 | 累计积分 |
| MemberType | varchar(20) default '正式党员' | 党员类型（预置枚举字符串：正式党员/预备党员/发展对象/积极分子…） |

**organizations**（组织树，snake 列）：id int PK / name varchar(100) / parent_id int NULL / created_at datetime(6)。实测组织树：1 中共党校委员会（根）→ 2 第一党总支、3 第二党总支 → 4 第一支部(父2)、5 第二支部(父2)、6 第三支部(父3)、7 第四支部(父3)。

**learningcontents**（学习内容，PascalCase）：Id PK / Title varchar(200) / Body varchar(8000) / VideoUrl varchar(500) / ContentType int(0=文章 1=视频) / CategoryId int NULL / IsPublic tinyint(1) / CreatedAt / RelatedDocumentUrl varchar(500) NULL / SourceType int(0=手动 1=AI生成 2=文档转换)。

**content_categories**（内容分类，snake）：id / name varchar(100) UNIQUE / parent_id NULL / created_at。
**content_tags**（内容-标签关联，snake 复合主键）：content_id + tag_id。
**tags**（snake）：id / name UNIQUE / created_at。

### 3.2 学习任务与进度
**learning_tasks**（snake）：id / task_name varchar(200) / target_org_id int / deadline datetime(6) / created_at。
**task_contents**（snake 复合主键）：task_id + content_id。
**member_learning_progress**（snake）：id PK / member_id / content_id / task_id NULL / duration_seconds int / is_completed tinyint(1) / completed_at NULL / updated_at。**学习时长与完成状态的主表**。

### 3.3 测验与题库
**examtests**（PascalCase）：Id PK / PaperId / PublisherId / TargetOrgId / TimeLimitMinutes / Deadline / CreatedAt / IsAiGenerated tinyint(1) / TargetWeaknessTags varchar(1000) NULL。
**exam_papers**（snake）：id / name / description text NULL / question_ids json / total_score / created_at。
**questions**（snake）：id PK / question_type int(0单选 1多选 2判断) / stem text / options json / correct_answer varchar(200) / score int / category_id NULL / created_at。
**question_categories**（snake）：id / name UNIQUE / created_at。
**member_test_records**（snake）：id PK / member_id / test_id / answers json / score int / submitted_at datetime(6)。**测验作答与得分主表**（错题判定由 answers+questions 反推，无独立错题表）。

### 3.4 积分 / 通知 / 打卡 / 对战 / 结对 / 发展 / 三会一课
**learningpoints**（积分记录，PascalCase）：Id PK / PartyMemberId / SourceType int(0看视频 1完成答题 2活动打卡 3党史PK胜利 4其他) / SourceId NULL / Points int / EarnedAt。
**messagenotifications**（PascalCase）：Id PK / PartyMemberId / Type int(0任务提醒 1测验提醒 2预警提醒 3系统通知) / Title varchar(200) / Content varchar(2000) / IsRead tinyint(1) / CreatedAt。
**checkinrecords**（PascalCase）：Id PK / PartyMemberId / LocationName varchar(200) / CheckInTime datetime(6) / Note varchar(2000) NULL / **AiBackgroundInterpretation varchar(2000) NULL（字段已在，业务为占位）** / PointsEarned int(默认5)。**无 siteId 列**（(12) 需新增）。
**battlerecords**（PascalCase）：Id PK / ChallengerId / OpponentId / **ResultJson varchar(1000)** / BattleTime datetime(6)。**ResultJson 结构未定（无种子数据）**，见 CONTRACTS (11)。
**pairhelprecords**（PascalCase）：Id PK / HelperId / HelpReceiverId / StartTime / **EndTime NULL** / HelpContentJson varchar(4000) NULL / OutcomeSummary varchar(2000) NULL。**无业务端点**（(10) 需新建）。
**partydevelopmentprocesses**（PascalCase）：Id PK / PartyMemberId / Stage int(0积极分子 1发展对象 2预备党员 3正式党员) / Status int(0待提交 1审核中 2已通过 3已驳回) / MaterialsJson varchar(2000) NULL / **ReportContent varchar(4000) NULL（思想汇报正文）** / SubmittedAt NULL / ReviewComment varchar(1000) NULL / ReviewedAt NULL / **IsReminderSent tinyint(1)（到期提醒已发标记）** / CreatedAt。
**meetingactivities**（PascalCase）：Id PK / OrganizationId / Type int(0支部党员大会 1支部委员会 2党小组会 3党课 4主题党日) / Title varchar(200) / Description varchar(4000) NULL / ActivityTime / CreatedAt / **IsAiSummaryGenerated tinyint(1) / AiSummaryContent varchar(4000) NULL（单活动 AI 总结字段已在）**。
**activityhearts**（活动心得，PascalCase）：Id PK / MeetingActivityId / PartyMemberId / Content varchar(4000) / SubmittedAt / AiPolishSuggestion varchar(2000) NULL（占位文案）。
**memberlearningreports**（AI 个人评估报告，PascalCase）：Id PK / PartyMemberId / ReportJson varchar(4000) / CreatedAt。
**organizationquarterlyreports**（支部季度报告，PascalCase）：Id PK / OrganizationId / Quarter varchar(20) / ReportJson varchar(4000) / CreatedAt。
**__efmigrationshistory**：MigrationId / ProductVersion。

## 4. 迁移机制

- `backend\Migrations\` 存在 **2 个历史迁移**：`20260812060113_InitialCreate`、`20260812082806_ExtendedFeatures`，以及 `AppDbContextModelSnapshot.cs`。
- `dotnet ef --version` = **10.0.11 可用**（无需安装；若未来机器缺失：`dotnet tool install --global dotnet-ef`，装后需刷新 PATH / 重开终端）。
- `backend\PartySchoolApi.csproj` 为单一项目，Migrations 默认 Assembly 即本项目，**无需 --project/--startup-project 参数**：
  - 新增表/改列后：`cd backend; dotnet ef migrations add <名称>`（可追加 `--verbose`）
  - 应用：`cd backend; dotnet ef database update`
  - 回滚/脚本：`dotnet ef migrations script`
- **迁移映射来源**：现有 snake 表依赖实体上的 `[Table]`/`[Column]` 数据注解（如 Organization、LearningTask、Question 等），**AppDbContext.cs 无 OnModelCreating 的 ToTable 配置**；PascalCase 表则无注解走 EF 默认。新表按用户约定采用小写下划线 + `[Table]/[Column]` 注解风格。
- 当前 AppDbContext 位于 `backend\Date\AppDbContext.cs`（**目录名是 Date 不是 Data**，命名空间 `PartySchoolApi.Data`）。

## 5. 账号与登录

| 账号 | 密码 | 角色 | 归属 |
|---|---|---|---|
| 13800000000 | 123456 | 系统管理员（SystemAdmin=2） | 党校委员会（org 1） |
| 13800000002 | 123456 | 党员（PartyMember=0，MemberType=正式党员） | 第一支部（org 4） |

- 证据：`docs\system-document.html` 第 9.3 节默认账号表；`13800000000` 的 bcrypt 哈希已用 python+bcrypt 实测匹配明文 `123456`（哈希 workFactor=10 与 PasswordHelper 一致）。
- 其他种子账号（13800000001/赵…书记、13800000004、13800000005 等）的 bcrypt 哈希**未命中候选明文列表**，明文未知，不建议作为测试账号。
- 登录接口：`POST /api/v1/auth/login`，请求 `{ phone, password }`，响应 `data: { accessToken, refreshToken, expiresIn, user }`（详见 PATTERNS）。前端存 `localStorage.accessToken`，请求头 `Authorization: Bearer <token>`。

## 6. 关键配置与既有文档

- `appsettings.json`：连接串、JWT、Qwen（BaseUrl/Model/ApiKey 空）、CORS 全开、JSON camelCase。
- `docs\system-document.html`：系统设计文档（含 9.3 默认账号）；`docs\architecture.html`：架构文档。两者为既有技术文书，与"当前技术文书状态"核对表逐项对照（缺项详见 CONTRACTS）。
- 根目录 `.env`：`DASHSCOPE_API_KEY`（有效）、`QWEN_BASE_URL`、`QWEN_MODEL` 覆盖项。
- `.recon\db_inspect.ps1`：数据库只读盘点脚本；`.recon\db_tables.txt` / `.recon\db_columns.txt`：表名与逐列结构原始导出（本文件第 3 节的结构依据）。
