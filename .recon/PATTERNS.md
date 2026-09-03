# PATTERNS.md — 代码模式与工程惯例（含绝对路径、代表代码、命令、坑）

> 只读核查结论。所有路径为绝对路径。代码片段为实际源码摘录（保留原样）。供后续后端/前端子任务按此风格施工。

---

## 0. 目录结构速览

```
C:\Users\Kee\Desktop\party-school-system\
├─ backend\                         # ASP.NET Core .NET8，端口 5091
│  ├─ Program.cs                    # DI 注册（手工逐行 AddScoped，115-143 行）+ JSON/枚举/日期转换 + JWT + CORS
│  ├─ Date\AppDbContext.cs          # 注意目录名是 Date（非 Data），命名空间 PartySchoolApi.Data
│  ├─ Controllers\                  # 22 个 Controller，统一路由前缀 /api/v1/...
│  ├─ Services\Interfaces\          # 服务接口（IxxxService）
│  ├─ Services\Implementations\     # 服务实现
│  ├─ Services\Common\              # OrgHierarchyHelper、PasswordHelper、JwtHelper 等
│  ├─ Models\Entities\              # 23 个实体
│  ├─ Models\DTOs\                  # 21 个 DTO 文件
│  ├─ Models\Common\                # ApiResponse、PagedResponse、Enums
│  ├─ Middleware\                   # BusinessException、异常中间件
│  └─ Migrations\                   # InitialCreate + ExtendedFeatures + Snapshot
├─ frontend\frontend-admin\         # 管理端 Vite+ElementPlus+ECharts，vite port 5173(open)
├─ frontend\frontend-member\        # 党员端 Vite+ElementPlus+ECharts，vite port 5173(/api 代理)
├─ knowledge\documents\             # 知识库文件（当前仅 测试知识库.txt）
├─ docs\                            # system-document.html、architecture.html
└─ .recon\                          # 本核查产物（db_*.txt/ps1、STATE/PATTERNS/CONTRACTS.md）
```

## 1. 统一响应结构

**文件**：`backend\Models\Common\ApiResponse.cs`

```csharp
public class ApiResponse
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
    public static ApiResponse Success(object? data = null, string message = "操作成功") => new() { Code = 200, ... };
    public static ApiResponse Fail(string message = "操作失败", int code = 400) => ...;
    public static ApiResponse Unauthorized(...) / Forbidden(...) / NotFound(...);
}

public class PagedResponse  // 分页：额外带 page/size/total，前端拦截器据此保留外层
{ public int Code=200; public string Message="查询成功"; public object? Data; public int Page; public int Size; public long Total; }
```

- 约定：**非分页接口一律 `ApiResponse.Success(data)`；分页接口一律 `PagedResponse.Ok(data, page, size, total)`**。
- 业务错误抛 `BusinessException(msg, code)`（`backend\Middleware\BusinessException.cs`），由异常中间件转成 `{code, message}`。
- JSON 序列化：Program.cs 配置 **camelCase**（响应字段自动小驼峰），前端按小驼峰读取。

## 2. QwenService（AI 调用唯一出口）

**文件**：`backend\Services\Implementations\QwenService.cs`；接口 `backend\Services\Interfaces\IQwenService.cs`

```csharp
// 核心签名
Task<string> ChatAsync(IEnumerable<QwenChatMessage> messages, double temperature = 0.7,
                       bool jsonMode = false, int maxTokens = 4096, CancellationToken ct = default);
Task<string> ChatAsync(string system, string user, double temperature = 0.7, bool jsonMode = false,
                       int maxTokens = 4096, CancellationToken ct = default);
Task<T?> ChatJsonAsync<T>(string system, string user, double temperature = 0.3,
                          int maxTokens = 4096, CancellationToken ct = default);
bool IsConfigured { get; }   // ApiKey 非空
```

- jsonMode=true 时走 `response_format={type:"json_object"}`，并要求 prompt 内"只输出 JSON"。
- 未配置 ApiKey 时 `IsConfigured=false`；各业务服务已用 `if(_qwen.IsConfigured)` 兜底降级（见 AiService/MeetingActivityService）。
- **新功能一律经 IQwenService 调用，勿直接 HttpClient 打 dashscope**；解析 JSON 推荐 `JsonDocument` 先掐头去尾（`IndexOf('{')/LastIndexOf('}')`，见 MeetingActivityService.ParseSummaryJson），容忍千问回显代码块。

## 3. Nl2SqlService（NL2SQL）

**文件**：`backend\Services\Implementations\Nl2SqlService.cs`；接口 `INl2SqlService`

```csharp
// 关键成员
Dictionary<string,string> TypoMap;   // 党元→党员、党支→党支部、完成绿→完成率、学西→学习、考式→考试、平钧分→平均分
(string? Table, ...) DetectIntent(string q);  // task_completion / exam_score / learning_duration / member_count / ranking / general_query
HashSet<string> AllowedTables;       // 白名单 16 表（仅允许 SELECT 的表）
HashSet<string> DangerousKeywords;   // 危险关键字黑名单（drop/truncate/delete/update/insert 等）
(bool ok, string reason) SafetyCheck(string sql);
Task<Nl2SqlResponse> ExecuteReadOnlyAsync(Nl2SqlRequest req);  // 自动补 LIMIT 100、ReadCommitted 事务、15s 超时
```

- 端点：`POST /api/v1/nl2sql/query`；DTO `Nl2SqlRequest{ NaturalLanguage, SessionId? }` → `Nl2SqlResponse{ GeneratedSql, Explanation, ResultData, ChartData?, SessionId, CorrectionsApplied }`。
- **SessionId 仅透传生成，无任何记忆**（(1) 需新增会话表 + 历史回带）。
- 已具备：SELECT 限制 / 危险关键字黑名单 / 表白名单 / LIMIT / 真实只读执行；**缺：字段级白名单与敏感字段脱敏**（partymembers.Phone / PasswordHash / RefreshToken 等需脱敏，见 CONTRACTS (1)）。
- 规则意图（task_completion 等）走 `OrgHierarchyHelper` 内存聚合，不走 SQL。

## 4. KnowledgeSearchService + AiKnowledgeService（知识库检索）

**文件**：`backend\Services\Implementations\KnowledgeSearchService.cs`、`AiKnowledgeService.cs`

```csharp
// KnowledgeSearchService：加载 knowledge/documents 下 .txt/.md → 按段落 + 800 字切块
KnowledgeDocument { string Id /* 文件名-块号 */; string File; string Content; }
IReadOnlyList<KnowledgeDocument> Search(string query, int limit = 5);  // 打分=整词匹配×2 + 连续4字子串 +1
string BuildContext(IEnumerable<KnowledgeDocument> results);  // "资料N/来源:xxx/内容:xxx" 拼接

// AiKnowledgeService：端点 POST /api/v1/ai-knowledge/query
AiKnowledgeQueryRequest { string Question; string? SessionId; }
AiKnowledgeQueryResponse { string Answer; List<string> SourceReferences; double Confidence /* 硬编码 0.92/0.65/0.3/0.9 */; string SessionId; }
```

- **现状：无向量检索、无重排、无逐条置信度**；Confidence 是写死的；有内置 `FallbackKnowledgeBase` 兜底；无多轮记忆。→ (2) 两级检索+重排在此扩展。
- 前端调用：admin `src\api\ai.js` `aiKnowledgeQuery(data)`；member `src\api\ai.js` `aiQuery(question)` → `/ai-knowledge/query`。

## 5. AiContentGenerationService（AI 素材生成）

**文件**：`backend\Services\Implementations\AiContentGenerationService.cs`

```csharp
// 端点 POST /api/v1/ai-content/generate
AiGenerateContentRequest { string? SourceText; string? PdfUrl; int SingleChoiceCount=5; int MultiChoiceCount=3;
                           int TrueFalseCount=2; bool GenerateFlashCards=true; int? CategoryId; }
AiGenerateContentResponse { List<Question> Questions; List<FlashCard> FlashCards; string Summary; }
```

- **无 contentType**（不做 article/speech/quizcard 区分）；PdfUrl 只是 `GetStringAsync` 拉文本，**无文档解析**。→ (3) 扩展 contentType。
- 前端：admin `api\ai.js` `aiContentGenerate(data)`。

## 6. AiService（推荐 / AI 查询 / 个人评估 / 组织报告）

**文件**：`backend\Services\Implementations\AiService.cs`

```csharp
// GET  /api/v1/mobile/recommendations?limit=       加权推荐：0.6 错题匹配 + 0.3 相似度 + 0.1 紧迫度；weaknessTags 硬编码 ["党史","党章","四个意识"]
// POST /api/v1/ai/query                            数据快照 + 千问（AiQueryRequest{Question,Context?} → {AnswerText,ChartData?,Intent}）
// POST /api/v1/mobile/report/ai-assessment         个人 AI 评估（AiAssessmentRequest{MemberId?} → 四维评分
//                                                    时长0.3 + 任务0.25 + 测验0.25 + 错题0.2，写入 memberlearningreports.ReportJson）
// POST /api/v1/ai/organization-report              OrganizationReportRequest{OrganizationId, Quarter?}
//                                                  → {OrganizationId, OrganizationName, Quarter, Report, Metrics{string:double}}
```

- `OrganizationReportResponse` **无 A/B/C/D 评级、无整改建议数组、无整改闭环**。→ (9) 扩展。
- 前端：admin `api\ai.js` `generateOrganizationReport(data)`、`aiQuery(data)`。

## 7. OrgHierarchyHelper（组织范围统一口径）

**文件**：`backend\Services\Common\OrgHierarchyHelper.cs`

```csharp
Dictionary<int, List<int>> BuildOrgScopeMap(List<Organization> all);      // 每组织 → 自身 + 全部子孙 id
List<int> CollectOrgAndDescendantIds(int rootId, List<Organization> all); // 含自身
List<int> CollectDescendantOrgIds(int parentId, List<Organization> all);  // 不含自身
```

- AiService / Nl2SqlService / StatisticsService / NotificationService 统一用它计算"本支部及下级"成员范围，**新功能涉及组织过滤时必须复用**，保证口径一致。

## 8. AntiCheatService（防挂机）

**文件**：`backend\Services\Implementations\AntiCheatService.cs`

```csharp
// GET  /api/v1/anti-cheat/challenge   内存 ConcurrentDictionary 挑战，3 个固定选择题，2 分钟过期
// POST /api/v1/anti-cheat/verify      {challengeId, answers} → {isValid, message}
// GET  /api/v1/anti-cheat/stats?orgId= 挂机率/通过/失败次数 —— ⚠️ 用 new Random(member.Id) 模拟，非真实
```

- `StatisticsService.GetAntiCheatStatsAsync` 同款模拟。→ (15) 需改为真实记录表 + 答对才累计有效时长。

## 9. StatisticsService（统计 / 大屏 / TopLearners）

**文件**：`backend\Services\Implementations\StatisticsService.cs`

```csharp
// GET /api/v1/statistics/dashboard               DashboardOverviewDto（含 Warnings/Trend，字段与前端 KPI 对齐）
// GET /api/v1/statistics/learning / exam         趋势统计
// GET /api/v1/statistics/branch/{orgId}          BranchStatisticsDto.TopLearners：按 LearningMinutes 排序 Take(10)（非 AI 多维评选）
// GET /api/v1/statistics/dashboard-largescreen   LargeScreenDashboardDto（Overview/BranchRankings/WeaknessHeatmap/LearningTrend）
// GET /api/v1/statistics/anti-cheat              模拟数据
```

- TopLearners 是"学习时长 Top10"，缺 AI 多维评选。→ (4) 新端点 AI 评选学习标兵。

## 10. NotificationService（通知）

**文件**：`backend\Services\Implementations\NotificationService.cs`

```csharp
// POST /api/v1/notifications/send              SendNotificationRequest{PartyMemberId, Type(枚举), Title, Content}（单发）
// POST /api/v1/notifications/batch-send        BatchSendNotificationRequest{PartyMemberIds[], Type, Title, Content}
// GET  /api/v1/notifications/unread|all        当前用户通知
// PUT  /api/v1/notifications/{id}/read  、  read-all
```

- NotificationType 枚举：0 任务提醒 / 1 测验提醒 / 2 预警提醒 / 3 系统通知。
- **无按组织/角色/党员类型筛选的推送**。→ (14) 新增 targeted-send 端点。

## 11. PartyDevelopmentService（党员发展）

**文件**：`backend\Services\Implementations\PartyDevelopmentService.cs`；路由前缀 `api/v1/party-development`

- CRUD + `submit / {id}/review / {id}/advance`；Stage 枚举（0 积极分子 1 发展对象 2 预备党员 3 正式党员）。
- `GetRemindersAsync`（GET `/party-development/reminders`）：查 `Stage=ProbationaryMember && ReviewedAt≤1年前 && !IsReminderSent`，返回提醒并**置位 IsReminderSent**（已有到期提醒雏形，但仅预备党员转正一类，无专项触发端点）。
- **`AiCheckMaterialsAsync` 为占位**：恒返回 `IsComplete=true` + 固定建议，未真正校验材料。→ (7) 增强。
- **无思想汇报 AI 建议端点**（`ReportContent` 字段在，业务只用其存正文）。→ (6) 新增。

## 12. CheckInService（打卡）

**文件**：`backend\Services\Implementations\CheckInService.cs`

```csharp
// POST /api/v1/check-in            CreateCheckInRequest{LocationName, Note?} → CheckInRecordDto（PointsEarned=5，写 learningpoints）
// GET  /api/v1/check-in/ai-background?locationName=  GetAiBackgroundAsync —— ⚠️ 静态字典占位（井冈山/延安/遵义/西柏坡 4 条），未真正调千问
// GET  /api/v1/check-in/my | list  （分页，CheckInQueryParams{PartyMemberId?, LocationName?}）
```

- `checkinrecords` 已有 `AiBackgroundInterpretation` 字段但业务不写千问；**无 siteId 列**。→ (12) 扩展。

## 13. MobileService（移动端学习/测验/概览）

**文件**：`backend\Services\Implementations\MobileService.cs`

```csharp
// GET  /api/v1/mobile/contents       个人内容列表（公共+支部任务内容，带进度/状态）
// GET  /api/v1/mobile/contents/{id}  内容详情
// POST /api/v1/mobile/progress       ReportProgressRequest{ContentId, TaskId?, DurationSeconds, IsCompleted} → member_learning_progress
//                                     （存在则 duration_seconds 累加，is_completed 置位）【有效时长上报链路】
// POST /api/v1/mobile/tasks/{id}/complete
// GET  /api/v1/mobile/tasks?completed=
// GET  /api/v1/mobile/exams          测验列表（pending/completed/expired）
// POST /api/v1/mobile/exams/{testId}/start   取题（不含答案）
// POST /api/v1/mobile/exams/{testId}/submit  SubmitExamRequest{TestId, Answers:[{QuestionId,Answer}]} → 判题（CheckAnswer 兼容
//                                     字母/索引/选项文本/对错词，多选 SetEquals）→ {RecordId, Score, TotalScore, IsPassed(≥60%)}
// GET  /api/v1/mobile/exams/{testId}/result   ExamResultDetailDto（兼容新旧 answers 存储格式）
// GET  /api/v1/mobile/report/overview        PersonalLearningOverviewDto（TotalLearningMinutes=sum(duration)/60、
//                                     CompletedContentCount、平均分、TaskCompletionRate、LearningProgress、TotalPoints）
```

- **判题逻辑 `CheckAnswer` 是全局判题范式**（KMeansService 与其一致），新对战/防挂机判题可直接复用该逻辑。

## 14. KMeansService（错题知识点聚类）

**文件**：`backend\Services\Implementations\KMeansService.cs`；端点 `POST /api/v1/kmeans/cluster`

```csharp
KMeansClusteringRequest { int PartyMemberId; int ClusterCount=3; }
// 读 member_test_records.answers + questions 判错题 → 按 question_categories.name（或题干关键词标签池）聚类
// → WeaknessHeatmapDto{Tag, ErrorCount, Intensity} 列表（Severity=错误率）
```

- 前端：member `api\ai.js` `kmeansCluster(memberId)`。→ 弱项标签是 (4)(10)(13) 的输入源。

## 15. 前端请求层 request.js（两端同构）

**文件**：`frontend\frontend-member\src\api\request.js`、`frontend\frontend-admin\src\api\request.js`

```js
// baseURL: 'http://localhost:5091/api/v1'
// 请求拦截器：localStorage.getItem('accessToken') → headers.Authorization = `Bearer ${token}`
// 响应拦截器：code===200 || code===0 视为成功；普通响应解包返回 res.data；
//            分页响应（存在 total 且含 page/size）保留外层 {code,data,page,size,total} 供分页组件使用；
//            401 → 清 token 跳 /login
```

- ⚠️ baseURL 为**绝对地址**，member vite 的 `/api` 代理实际未生效（请求直连 5091）；跨端口 CORS 已全开。

## 16. 前端 API / 视图 / 路由 / ECharts 模式

- **API 模块**：每个功能一个文件，`import request from './request'`，导出函数 `export function xxx(data){ return request.post('/path', data) }`。
  - member `src\api\`：ai / auth / checkin / content / exam / mobile / notification / points / request / task
  - admin `src\api\`：ai / auth / checkin / content / exam / meeting / member / mobile / notification / organization / partyDevelopment / points / question / request / statistics / task
- **视图**：`<script setup>` + Element Plus `<el-card>/<el-table>/<el-form>`；`import { ref, onMounted } from 'vue'`；`onMounted(loadData)` 模式。
- **路由**：member `src\router\index.js`（createWebHistory）：/login /home /learning /content/:id /exam /quiz/:testId /ai-chat /report /profile；admin：/login /dashboard /organization /learning-content /exam-management /org-life /data-analysis。守卫仅查 token（无角色细分）。新增页面需在此注册 + `MainLayout.vue` 菜单项（`frontend-*/src/layouts/MainLayout.vue`）。
- **ECharts 引入**：`import * as echarts from 'echarts'` → `const chartInstance = echarts.init(chartRef.value)` → `chartInstance.setOption(option)`；`DataAnalysis.vue` 兼容后端直回 `chartData.option / chartData.echartsOption`（`setOption(option, true)` 全量替换）。
- **Pinia**：member `stores\user.js`、`stores\aiData.js`；admin `stores\user.js`。
- **`@` 别名**：两端 vite.config.js `resolve.alias['@'] = './src'`。

## 17. Program.cs 服务注册（backend\Program.cs 115-143 行）

```csharp
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IPartyMemberService, PartyMemberService>();
builder.Services.AddScoped<ILearningContentService, LearningContentService>();
builder.Services.AddScoped<ILearningTaskService, LearningTaskService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<IMobileService, MobileService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IAiService, AiService>();
builder.Services.AddScoped<IPartyDevelopmentService, PartyDevelopmentService>();
builder.Services.AddScoped<IMeetingActivityService, MeetingActivityService>();
builder.Services.AddScoped<ICheckInService, CheckInService>();
builder.Services.AddScoped<IPointService, PointService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IQwenService, QwenService>();
builder.Services.AddScoped<IKnowledgeSearchService, KnowledgeSearchService>();
builder.Services.AddScoped<IAiKnowledgeService, AiKnowledgeService>();
builder.Services.AddScoped<INl2SqlService, Nl2SqlService>();
builder.Services.AddScoped<IAiContentGenerationService, AiContentGenerationService>();
builder.Services.AddScoped<IAntiCheatService, AntiCheatService>();
builder.Services.AddScoped<IKMeansService, KMeansService>();
```

- **新增服务需手工在此 AddScoped**（无反射自动注册）。JSON 全局 camelCase + `FlexibleEnumJsonConverterFactory`（字符串/数字枚举入参兼容）+ `FlexibleDateTimeJsonConverter`（兼容 `"yyyy-MM-dd HH:mm:ss"` 入参）。CORS AllowAll。JWT Bearer。
- 控制器属性：`[Authorize]`（角色细分用 `[Authorize(Roles="...")]` 或 `ICurrentUserService.Role` 判断）；从 `ICurrentUserService` 取当前用户（MemberId/OrganizationId/Role），**勿自行解析 JWT**。

## 18. 启动 / 重启 / 迁移命令（Windows PowerShell）

```powershell
# 后端构建（只读验证）
cd C:\Users\Kee\Desktop\party-school-system\backend
dotnet build

# 后端启动（不重启 5091 时的命令）
cd backend
Start-Process dotnet "run --no-build --urls http://localhost:5091"
# 重启 = 先杀占用 5091 的进程再启动：Get-NetTCPConnection -LocalPort 5091 | ForEach-Object { Stop-Process -Id $_.OwningProcess -Force }

# 前端
cd frontend\frontend-member ; npm run dev     # vite（config port 5173）
cd frontend\frontend-admin  ; npm run dev     # vite（config port 5173, open:true）

# 迁移
cd backend
dotnet ef --version                # 10.0.11 可用
dotnet ef migrations add <Name>    # 新增表/改列后生成（本工程单一项目，无需 --project/--startup-project）
dotnet ef database update          # 应用
# 若机器缺 dotnet ef：dotnet tool install --global dotnet-ef；安装后刷新 PATH/重开终端
```

## 19. 常见坑（施工必读）

1. **表命名混合大小写**：新增/修改表前务必 `SHOW COLUMNS` 实测列名大小写（partymembers 列 PascalCase、learning_tasks 列 snake），实体注解与列名必须逐字对齐；**新表统一小写下划线 + `[Table]/[Column]` 注解**。
2. **实体注解不统一**：有 `[Table]/[Column]` 注解的实体（Organization/LearningTask/Question/QuestionCategory/ExamPaper/ContentCategory/ContentTag/Tag/TaskContent/MemberLearningProgress/MemberTestRecord）走 snake；无注解实体（PartyMember/LearningContent/ExamTest/LearningPoint/MeetingActivity/CheckInRecord/BattleRecord/PairHelpRecord/PartyDevelopmentProcess/MemberLearningReport/OrganizationQuarterlyReport/ActivityHeart）走 EF 默认 PascalCase。**AppDbContext.cs 无 OnModelCreating 的 ToTable 配置**。
3. **AppDbContext 在 `backend\Date\` 目录**（非 Data），新增 DbSet 时注意路径与命名空间 `PartySchoolApi.Data`。
4. **DI 手工注册**：新服务三件套（接口→实现→Program.cs AddScoped）缺一不可。
5. **千问未配置时静默降级**：所有 AI 服务都用 `_qwen.IsConfigured` 分支，未配置 key 时走兜底/固定文案；真实验收前确认 `.env` 的 DASHSCOPE_API_KEY 存在（存在但**禁止本核查调用**）。
6. **多处"模拟/占位"数据**：AntiCheatService stats（Random 模拟）、StatisticsService.GetAntiCheatStatsAsync（模拟）、CheckInService.GetAiBackgroundAsync（静态字典）、AiContentGenerationService（无 contentType）、PartyDevelopmentService.AiCheckMaterialsAsync（占位）、ActivityHeart.AiPolishSuggestion（固定文案）、AiKnowledgeService.Confidence（写死）。这些是 (3)(5)(7)(12)(15) 的改造点。
7. **battlerecords.ResultJson / pairhelprecords.HelpContentJson 为 JSON 字符串列**，无既定 schema，新契约需自定 JSON 结构并注明。
8. **前端响应拦截器分页特判**：返回含 `total` 且含 `page/size` 时保留外层，普通响应解包 `data`——新分页接口必须用 `PagedResponse.Ok` 否则前端取数错位。
9. **时间格式**：后端 DateTime 序列化格式与前端展示需核对；入参兼容 `"yyyy-MM-dd HH:mm:ss"`（FlexibleDateTimeJsonConverter）。
10. **两端 vite 端口同为 5173**（member 配了 5173 而非任务所述 5174），同时启动会端口冲突；member 的 `/api` 代理因 request.js 用绝对地址 5091 实际未生效。施工时如需本地并行联调，可把 member 端口改为 5174 或去掉 admin 的 open。
11. **测试账号受限**：仅 `13800000000/123456`（系统管理员）与 `13800000002/123456`（党员/第一支部）明文确定；其他种子用户哈希明文未知。
12. **构建 16 个可空警告**（CS8602/CS8629）不影响编译，但新代码应避免新增同类警告。
