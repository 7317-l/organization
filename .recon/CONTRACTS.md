# CONTRACTS.md — 15 项功能补齐 REST 契约（精确、可执行）

> 适用范围：后续所有后端/前端子任务按此契约并行施工。**契约为准，实现服从契约**。
> 通用约定（与现状一致）：
> - 路径前缀一律 `/api/v1`；认证 `Authorization: Bearer <token>`（JWT，AccessToken 120min）。
> - 响应统一 `ApiResponse{ code, message, data }`；分页接口用 `PagedResponse{ code, message, data, page, size, total }`。JSON 字段自动 camelCase。
> - 角色：`PartyMember`(0 党员) / `BranchSecretary`(1 支部书记) / `SystemAdmin`(2 系统管理员)。权限标注"角色"。
> - 新增表一律**小写下划线命名**，实体用 `[Table]/[Column]` 注解（与 organizations/learning_tasks 风格一致）；Id 主键 `BIGINT AUTO_INCREMENT`（与现有 int 对齐用 INT，保持一致用 `INT PK AUTO_INCREMENT`，时间列 `datetime(6)`）。
> - 所有"AI 生成/校验"均经 `IQwenService`（已注入，`IsConfigured` 分支降级）。
> - 现有效验基线见 `.recon\STATE.md` / `.recon\PATTERNS.md`。

---

## (1) NL2SQL 多轮上下文

**现状**：`POST /api/v1/nl2sql/query` 已存在，`SessionId` 仅透传无记忆；已有 TypoMap/DetectIntent/SELECT 白名单黑名单/LIMIT/真实只读执行；缺字段级白名单与敏感字段脱敏。

### 1.1 扩展现有端点 `POST /api/v1/nl2sql/query`

**请求体**（在现有 `Nl2SqlRequest` 基础上**新增/修改**）：
```jsonc
{
  "naturalLanguage": "string, 必填",          // 现有字段
  "sessionId": "string|null, 选填",           // 现有字段；为空则新建会话，非空则回带上下文
  "historyCount": "int, 选填, 默认5",         // 新增：回带最近历史条数 1-10
  "userId": "int, 选填, 默认取当前登录用户"     // 新增：会话归属（默认 ICurrentUserService.MemberId）
}
```

**响应体 data**（在现有 `Nl2SqlResponse` 基础上**新增/修改**）：
```jsonc
{
  "generatedSql": "string",
  "explanation": "string",
  "resultData": "object|null",                 // 现有
  "chartData": "object|null",                  // 现有
  "sessionId": "string, 必返",                 // 现有
  "correctionsApplied": ["string"],            // 现有：本次修正的错别字列表
  "intent": "string",                          // 新增：DetectIntent 结果
  "rewrittenQuery": "string",                  // 新增：指代改写后的完整问句（无指代时=原文）
  "isResolvedFromHistory": "bool",             // 新增：是否命中历史指代
  "conversation": [                            // 新增：本轮会话最近 historyCount 条
    { "question": "string", "explanation": "string", "resultSummary": "string|null" }
  ]
}
```

**指代处理规则**：
- 命中指代词（"同上""继续""再看下X支部""和上次一样""上一条结果里…"等）时，取会话最近一条历史，拼入改写：把"X支部"解析为组织名→`organizationId` 回填查询条件；"继续/同上"沿用上一轮 `naturalLanguage` 主谓结构（如"再按平均分排序"→ 上一轮问句 + 追加排序条件）。
- 改写可先用规则（TypoMap + 指代词表）处理，再交给千问润色（失败则用规则结果兜底）。
- 会话存储：写入新表 `nl2sql_sessions`（见下），`SELECT` 结果只存 `result_summary`（首个结果行摘要），不存全量。

### 1.2 新增表 `nl2sql_sessions`
```sql
CREATE TABLE nl2sql_sessions (
  id          INT PRIMARY KEY AUTO_INCREMENT,
  session_id  VARCHAR(64)  NOT NULL UNIQUE,     -- 回传客户端的会话 ID
  member_id   INT          NOT NULL,            -- 归属党员（FK partymembers.Id）
  question    TEXT         NOT NULL,            -- 用户问句（改写前原始句）
  rewritten   TEXT         NULL,                -- 指代改写后问句
  sql_text    TEXT         NULL,                -- 生成的 SQL（可空：规则意图无 SQL）
  explanation VARCHAR(2000) NULL,
  result_summary VARCHAR(4000) NULL,            -- 结果摘要（脱敏后）
  created_at  DATETIME(6)  NOT NULL,
  INDEX idx_nl2sql_member (member_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

### 1.3 新增会话历史端点
`GET /api/v1/nl2sql/sessions/{sessionId}/history?limit=5`（党员本人）
- 响应 `data: [ { question, rewritten, explanation, resultSummary, createdAt } ]`（倒序 limit 条）。

### 1.4 字段级白名单 + 敏感字段脱敏（安全增强，随本端点一起落地）
- 新增**字段级白名单**：`SELECT` 列必须命中 `AllowedColumns` 映射表（表.列 → 允许输出），未命中列剔除或拒绝。
- 新增**敏感字段脱敏表**：`partymembers.Phone`（中间 4 位 `*`）、`partymembers.PasswordHash`、`partymembers.RefreshToken`、`partymembers.RefreshTokenExpiry`（输出 `"***"`）。所有 `resultData` 输出前递归脱敏。
- 校验通过后才允许执行；不改变现有 LIMIT 100 / 只读事务 / 15s 超时行为。

---

## (2) RAG 两级检索 + 重排

**现状**：`POST /api/v1/ai-knowledge/query` 已存在，无向量、无重排、无逐条置信度、无多轮记忆、Confidence 写死。

### 2.1 扩展现有端点 `POST /api/v1/ai-knowledge/query`

**请求体**（在现有 `AiKnowledgeQueryRequest{ Question, SessionId? }` 基础上**新增/修改**）：
```jsonc
{
  "question": "string, 必填",
  "sessionId": "string|null, 选填",             // 新增：启用多轮记忆（存同库，见 2.3）
  "topK": "int, 选填, 默认5, 范围1-10",          // 新增：重排后返回条数（默认 Top5）
  "rerank": "bool, 选填, 默认true",              // 新增：是否二级重排（false 则只做关键词召回）
  "filterFile": "string|null, 选填"              // 新增：仅检索指定知识库文件（文件名）
}
```

**响应体 data**（在现有 `AiKnowledgeQueryResponse{ Answer, SourceReferences[], Confidence, SessionId }` 基础上**新增/修改**）：
```jsonc
{
  "answer": "string",
  "sessionId": "string, 必返",
  "sourceReferences": ["string"],                // 现有：兼容保留（= results[].file + 块号）
  "results": [                                   // 新增：两级检索+重排结果（TopN，N=topK）
    {
      "id": "string",                            // "文件名-块号"
      "file": "string",
      "snippet": "string",                       // 命中文片段（限 200 字）
      "score": "double",                         // 一级召回分
      "rerankScore": "double",                   // 二级重排分（0-1，千问或交叉编码打分）
      "confidence": "double",                    // 逐条置信度 0-1（不再写死）
      "matchedKeywords": ["string"]              // 命中的关键词/主题词
    }
  ],
  "confidence": "double"                         // 保留字段=results[0].confidence
}
```

**两级检索实现**：
1. **一级召回**：`KnowledgeSearchService.Search(question, limit=20)`（关键词 + 连续 4 字子串）。
2. **二级重排**：对 20 条候选，用千问（`ChatJsonAsync`）给出 `[{index, score(0-1), reason}]`，或降级用 BM25 近似（查询词频×位置），取 TopK；空候选时用 `FallbackKnowledgeBase`。
3. **置信度**：`rerankScore` 经 sigmoid/归一化 → confidence；来源为空时 confidence=0。
4. 多轮记忆：`sessionId` 复用（nl2sql_sessions 同库不同业务），把上一轮 question+answer 摘要拼入本轮 prompt，提升指代追问命中。

### 2.2 新增知识库文档管理端点（可选，建议）
- `GET /api/v1/ai-knowledge/documents`（党员）：`data: [ { id, file, chunkCount, loadedAt } ]`。
- `POST /api/v1/ai-knowledge/documents/reload`（SystemAdmin）：重载 knowledge/documents 目录。`data: { reloaded: true, docCount, chunkCount }`。

---

## (3) AI 生成宣讲稿 / 文章 / 知识卡片

**现状**：`POST /api/v1/ai-content/generate` 只生成题目/卡片，无 contentType 区分，无文档解析。

### 3.1 扩展现有端点 `POST /api/v1/ai-content/generate`

**请求体**（在现有 `AiGenerateContentRequest` 基础上**新增/修改**）：
```jsonc
{
  "contentType": "string, 必填",                 // 新增：枚举字符串，兼容现有前端
  //   "questions"  -> 生成题目（= 现状，保留 singleChoiceCount/multiChoiceCount/trueFalseCount 语义）
  //   "article"    -> 生成文章
  //   "speech"     -> 生成宣讲稿
  //   "quizcard"   -> 生成知识卡片（沿用现有 GenerateFlashCards）
  "sourceText": "string|null, 选填",
  "pdfUrl": "string|null, 选填",
  "topic": "string|null, 选填",                  // 新增：主题（article/speech/quizcard 必填其一，否则用 sourceText 提炼）
  "audience": "string|null, 选填",               // 新增：受众（宣讲稿/文章用）：党员|积极分子|预备党员|群众
  "durationMinutes": "int|null, 选填",           // 新增：宣讲稿目标时长（分钟）
  "tone": "string|null, 选填",                   // 新增：风格：正式|通俗|激昂|平实（默认正式）
  "maxWords": "int|null, 选填",                  // 新增：字数上限
  "keywords": ["string"],                        // 新增：要求涵盖的关键词
  "singleChoiceCount": 5, "multiChoiceCount": 3, "trueFalseCount": 2, "generateFlashCards": true,
  "categoryId": "int|null"
}
```

**响应体 data**（在现有 `AiGenerateContentResponse{ Questions, FlashCards, Summary }` 基础上**新增/修改**）：
```jsonc
{
  "contentType": "string, 必返",
  "summary": "string",                           // 现有
  "content": {                                   // 新增：contentType != questions 时必返
    "title": "string",
    "text": "string",                            // 正文/宣讲稿全文（含分段）
    "outline": ["string"],                       // 大纲/小标题
    "keyPoints": ["string"],                     // 核心要点
    "targetAudience": "string",
    "estimatedMinutes": "int|null",              // 宣讲稿：估算时长（=durationMinutes 目标）
    "wordCount": "int",
    "sections": [                                // 宣讲稿：按时间轴分段，供前端逐段展示
      { "heading": "string", "minutes": "int", "content": "string" }
    ]
  },
  "questions": [...],                            // contentType=questions 时返（现有结构）
  "flashCards": [...]                            // contentType=quizcard 或 generateFlashCards 时返（现有 FlashCard 结构）
}
```

**宣讲稿生成规则**：按 `topic`（必填）`+ durationMinutes`（默认 15 分钟≈2500 字，可按 180-220 字/分钟折算）`+ audience` 生成"开场-主体-总结"三段式，`sections` 每段标注时长；结合党史/政策素材（可先 `KnowledgeSearchService.Search(topic)` 注入）。
**知识卡片**：`topic` 或 `sourceText` → FlashCard 列表 `{ front, back, tag }`。
**文章**：`topic` → 标题 + 正文（maxWords 约束）+ outline。
**PDF/长文**：`pdfUrl` 仍为文本抓取（现状），如需文档解析单独列后续任务；`sourceText` 超长（>6000 字）自动截断首尾各 3000 字并提示。

---

## (4) AI 评选学习标兵

**现状**：仅 `StatisticsService` 按学习时长 Top10，非 AI 多维评选。

### 4.1 新增端点 `POST /api/v1/ai/star-members`（SystemAdmin / BranchSecretary）
- 用途：按多维打分 + AI 理由评选学习标兵 TopN。
- **请求体**：
```jsonc
{
  "organizationId": "int|null, 选填",            // 为空=全局；指定则含其下级（用 OrgHierarchyHelper）
  "topN": "int, 选填, 默认10, 范围1-50",
  "weights": {                                   // 选填：自定义维度权重（缺省用默认）
    "learningMinutes": 0.30,
    "taskCompletion": 0.25,
    "examScore": 0.25,
    "weaknessImprovement": 0.10,
    "points": 0.10
  },
  "includeReason": "bool, 选填, 默认true"        // false 时跳过千问，只算分（省 token）
}
```
- **响应体 data**：
```jsonc
{
  "generatedAt": "datetime",
  "scope": { "organizationId": "int|null", "organizationName": "string|null", "memberCount": "int" },
  "members": [
    {
      "rank": 1,
      "memberId": "int", "memberName": "string",
      "organizationId": "int", "organizationName": "string",
      "totalScore": "double",                    // 0-100 加权总分
      "level": "string",                         // 优秀/良好/一般
      "dimensions": [                            // 每个维度：名称、得分(0-100)、权重、评语
        { "name": "learningMinutes", "score": 90.5, "weight": 0.3, "comment": "string" }
      ],
      "aiReason": "string|null"                  // 千问生成的推荐理由（includeReason=true 时）
    }
  ]
}
```
- 数据源：`member_learning_progress`（时长/完成）、`member_test_records`（平均分）、`memberlearningreports`/KMeans（薄弱点改善）、`partymembers.PointTotal`。先算分排序，再仅对 TopN 调千问写理由，失败降级为模板文案。

---

## (5) 三会一课 AI 总结简报

**现状**：单活动总结 `POST /api/v1/meeting-activities/{id}/ai-summary` 已有（GenerateAiSummaryAsync 写 IsAiSummaryGenerated/AiSummaryContent）。

### 5.1 新增端点 `POST /api/v1/meeting-activities/ai-brief`（SystemAdmin / BranchSecretary）
- 用途：按活动聚合生成一段时间的"三会一课"简报。
- **请求体**：
```jsonc
{
  "organizationId": "int|null, 选填",            // 为空=当前管理员可见范围；含下级
  "startDate": "date, 必填",                     // 例如 2026-07-01
  "endDate": "date, 必填",                       // 例如 2026-09-30
  "type": "int|null, 选填"                       // MeetingType：0支部党员大会 1支部委员会 2党小组会 3党课 4主题党日
}
```
- **响应体 data**：
```jsonc
{
  "period": { "startDate": "date", "endDate": "date" },
  "organizationId": "int|null", "organizationName": "string|null",
  "activityCount": "int",
  "typeBreakdown": [ { "type": "int", "typeName": "string", "count": "int" } ],
  "totalHearts": "int",
  "attendanceRate": "double|null",               // 有参与人数数据则给，否则 null
  "brief": "string",                             // 千问聚合生成的整体简报（含要点/不足/下一步）
  "keyPoints": ["string"],
  "perActivity": [                               // 逐活动：复用单活动总结结果，无则现场生成
    { "activityId": "int", "title": "string", "typeName": "string",
      "activityTime": "datetime", "summary": "string", "keyPoints": ["string"] }
  ]
}
```
- 实现：查时间窗内活动 → 对未生成 summary 的活动调 `GenerateAiSummaryAsync` → 汇总拼给千问生成简报。

---

## (6) 思想汇报 AI 建议

**现状**：`PartyDevelopmentService` 有 `ReportContent` 字段，无思想汇报建议端点。

### 6.1 新增端点 `POST /api/v1/party-development/{id}/ai-report-suggestion`（党员本人/支部书记）
- 用途：思想汇报正文 → 结构/内容建议 + 评分。
- **请求体**：
```jsonc
{
  "reportContent": "string, 必填",               // 思想汇报正文（若为空，服务端取该 process 的 ReportContent）
  "stage": "int|null, 选填"                      // PartyDevelopmentStage，影响建议口径
}
```
- **响应体 data**：
```jsonc
{
  "processId": "int",
  "overallScore": "double",                      // 0-100
  "dimensions": [                                // 结构/内容/语言/党性 四维
    { "name": "structure", "score": "double", "comment": "string" },
    { "name": "content", "score": "double", "comment": "string" },
    { "name": "language", "score": "double", "comment": "string" },
    { "name": "partySpirit", "score": "double", "comment": "string" }
  ],
  "strengths": ["string"],
  "suggestions": ["string"],                     // 逐条可执行建议（如"第2段缺少结合本职工作的实例"）
  "rewrittenExcerpt": "string|null"              // 选填：代表性段落的润色示例（1 段即可）
}
```
- 实现：正文分段 → 千问按四维打分 + 建议（`ChatJsonAsync`），失败降级为规则评分（长度/是否有实例/是否有理论引用）。

---

## (7) 党员发展 AI 材料校验

**现状**：`GET /api/v1/party-development/{id}/ai-check` 为占位（恒 IsComplete=true）。

### 7.1 新增端点 `POST /api/v1/party-development/{id}/ai-material-check`（SystemAdmin / BranchSecretary / 本人）
- 用途：材料 → 校验结论 + 整改项。
- **请求体**（可选覆盖）：
```jsonc
{
  "materials": ["string|null"],                  // 选填：实际已提交材料清单（如 ["入党申请书","思想汇报-2026Q3"]）；为空则读 process.MaterialsJson
  "stage": "int|null, 选填"                      // 校验基准阶段；为空用 process.Stage
}
```
- **响应体 data**：
```jsonc
{
  "processId": "int",
  "stage": "int", "stageName": "string",
  "isComplete": "bool",
  "requiredMaterials": ["string"],               // 该阶段应提交的材料清单
  "missingMaterials": ["string"],
  "issues": [                                    // 逐材料校验
    { "material": "string", "status": "ok|missing|invalid|expired", "checkResult": "string", "suggestion": "string" }
  ],
  "score": "double|null",                        // 完整性得分 0-100
  "suggestion": "string",                        // 千问综合整改建议
  "checkedAt": "datetime"
}
```
- 必交材料规则（按 Stage）：
  - 积极分子：入党申请书、本人情况汇报、党组织谈话记录、思想汇报（≥1 篇/季度）。
  - 发展对象：积极分子培养考察表、政治审查材料、公示情况、集中培训结业证。
  - 预备党员：发展对象阶段全部材料 + 入党志愿书 + 转正申请书（满一年）。
  - 材料真实性与格式由千问辅助判断（缺失/日期过期/格式异常）。
- 旧占位端点 `GET /{id}/ai-check` 保留兼容（内部调用新逻辑或维持占位），前端切到新端点。

---

## (8) 党员发展到期提醒

**现状**：`GET /api/v1/party-development/reminders` 已有（预备党员满一年，列表 + 置位 IsReminderSent），无专项触发端点，提醒类型单一。

### 8.1 新增端点 `POST /api/v1/party-development/reminders/trigger`（SystemAdmin / BranchSecretary）
- 用途：触发扫描到期事项并批量生成通知（写 messagenotifications）。
- **请求体**：
```jsonc
{
  "organizationId": "int|null, 选填",            // 为空=全局扫描
  "types": ["string"],                           // 选填：["probationary_due","material_missing","report_due"]，缺省全类型
  "sendNotification": "bool, 选填, 默认true"     // false 时只统计不落通知
}
```
- **响应体 data**：
```jsonc
{
  "scanned": { "probationaryDue": "int", "materialMissing": "int", "reportDue": "int" },
  "reminders": [
    { "reminderId": "int", "processId": "int", "partyMemberId": "int", "memberName": "string",
      "type": "string",                          // probationary_due | material_missing | report_due
      "dueDate": "datetime|null", "message": "string", "status": "int", "sentAt": "datetime|null" }
  ],
  "sentCount": "int"
}
```
- 触发规则：
  - `probationary_due`：Stage=2 预备党员且 ReviewedAt ≤ 当前-365 天 且 未标记。
  - `material_missing`：材料清单校验不完整（复用 (7) 规则）。
  - `report_due`：距上次思想汇报超过 90 天。
- 通知落 `messagenotifications`（Type=2 预警提醒），并标记 `party_development_reminders.status=1`、process.IsReminderSent=true。

### 8.2 新增表 `party_development_reminders`
```sql
CREATE TABLE party_development_reminders (
  id              INT PRIMARY KEY AUTO_INCREMENT,
  process_id      INT          NOT NULL,          -- FK partydevelopmentprocesses.Id
  party_member_id INT          NOT NULL,          -- FK partymembers.Id
  reminder_type   VARCHAR(20)  NOT NULL,          -- probationary_due | material_missing | report_due
  due_date        DATETIME(6)  NULL,
  message         VARCHAR(500) NOT NULL,
  status          INT          NOT NULL DEFAULT 0,-- 0=未发送 1=已发送
  created_at      DATETIME(6)  NOT NULL,
  sent_at         DATETIME(6)  NULL,
  INDEX idx_pdr_member (party_member_id), INDEX idx_pdr_process (process_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

### 8.3 新增列表端点
`GET /api/v1/party-development/reminders/list?organizationId=&status=&type=&page=&size=`（SystemAdmin / BranchSecretary）
- `data`：分页 `PagedResponse`，项结构同 trigger 的 `reminders[]`。

---

## (9) 支部季度量化评级 + 整改闭环

**现状**：`POST /api/v1/ai/organization-report` 有 Metrics dict，无 A/B/C/D 评级、无整改闭环。

### 9.1 扩展现有端点 `POST /api/v1/ai/organization-report`

**请求体**（不变）：`{ organizationId(必填), quarter?("2026Q3") }`

**响应体 data**（在现有 `OrganizationReportResponse` 基础上**新增/修改**）：
```jsonc
{
  "organizationId": "int", "organizationName": "string", "quarter": "string",
  "report": "string",                            // 现有
  "metrics": { "key": "double" },                // 现有（保持维度 key 不变，避免前端破坏）
  "rating": "string",                            // 新增：A|B|C|D
  "ratingScore": "double",                       // 新增：0-100 综合分
  "ratings": [                                   // 新增：逐维度评级
    { "dimension": "taskCompletion", "score": "double", "grade": "string", "comment": "string" }
  ],
  "suggestions": [                               // 新增：整改建议列表
    { "id": "string", "issue": "string", "suggestion": "string", "priority": "high|medium|low" }
  ]
}
```
- **评级算法（默认权重）**：任务完成率 0.35 + 测验平均分 0.25 + 组织生活参与率 0.20 + 人均学习时长 0.10 + 积分活跃 0.10 → 总分 ≥90=A、≥75=B、≥60=C、<60=D。
- 生成时把评级与 suggestions **落库**到 `organization_quarterly_ratings`（每季度每组织一条），供前端历史查询。

### 9.2 新增表
```sql
CREATE TABLE organization_quarterly_ratings (
  id             INT PRIMARY KEY AUTO_INCREMENT,
  organization_id INT         NOT NULL,          -- FK organizations.id
  quarter        VARCHAR(20)  NOT NULL,          -- 如 2026Q3
  rating         CHAR(1)      NOT NULL,          -- A/B/C/D
  rating_score   DECIMAL(5,2) NOT NULL,
  detail_json    VARCHAR(4000) NULL,             -- 逐维度 + suggestions JSON
  created_at     DATETIME(6)  NOT NULL,
  UNIQUE KEY uk_oqr (organization_id, quarter)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE org_rectifications (
  id              INT PRIMARY KEY AUTO_INCREMENT,
  organization_id INT         NOT NULL,
  quarter         VARCHAR(20) NOT NULL,
  issue           VARCHAR(500) NOT NULL,         -- 问题项（源自评级 suggestions）
  suggestion      VARCHAR(500) NOT NULL,         -- 整改建议
  status          INT         NOT NULL DEFAULT 0,-- 0=待整改 1=整改中 2=已完成 3=已关闭
  remark          VARCHAR(500) NULL,             -- 完成说明
  created_at      DATETIME(6) NOT NULL,
  completed_at    DATETIME(6) NULL,
  INDEX idx_rect_org (organization_id, quarter)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

### 9.3 整改闭环端点
- `GET /api/v1/organizations/{organizationId}/rectifications?quarter=&status=&page=&size=`（SystemAdmin / BranchSecretary）
  - 响应 `data`：分页，项 `{ id, organizationId, quarter, issue, suggestion, status, statusName, remark, createdAt, completedAt }`。
- `POST /api/v1/organizations/{organizationId}/rectifications`（SystemAdmin / BranchSecretary）
  - 请求：`{ quarter(必填), issue(必填), suggestion(必填) }` → 创建整改项。
- `PUT /api/v1/organizations/rectifications/{id}/complete`（SystemAdmin / BranchSecretary）
  - 请求：`{ remark?: string }` → 状态置 2 已完成，写 completedAt。
- `PUT /api/v1/organizations/rectifications/{id}/status`（SystemAdmin / BranchSecretary）
  - 请求：`{ status: int, remark?: string }` → 任意状态流转（0/1/2/3）。

---

## (10) 党员薄弱点互助智能匹配

**现状**：`pairhelprecords` 表存在，无业务端点。

### 10.1 新增表 `pair_help_requests`（结对申请）
```sql
CREATE TABLE pair_help_requests (
  id                INT PRIMARY KEY AUTO_INCREMENT,
  helper_id         INT NOT NULL,                 -- 帮扶者 FK partymembers.Id
  help_receiver_id  INT NOT NULL,                 -- 被帮扶者 FK partymembers.Id
  status            INT NOT NULL DEFAULT 0,       -- 0=待接受 1=已接受 2=已拒绝 3=已结束
  match_reason      VARCHAR(1000) NULL,           -- AI/规则匹配理由（发起方视角）
  created_at        DATETIME(6) NOT NULL,
  updated_at        DATETIME(6) NOT NULL,
  INDEX idx_phr_receiver (help_receiver_id), INDEX idx_phr_helper (helper_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

### 10.2 端点（全部 PartyMember 角色）
- **`POST /api/v1/pair-help/recommend`** — AI 推荐帮扶对象（基于薄弱点互补）
  - 请求：`{ myWeaknessTags?: ["string"], count?: int=5 }`（缺省取 KMeans 聚类出的本人薄弱标签）。
  - 响应 `data: { recommendations: [ { memberId, memberName, organizationName, weaknessTags, score, matchReason } ] }`。
  - 规则：在本人所在支部+可及范围（OrgHierarchyHelper）内，找"薄弱标签与我互补"（我在 A 弱、对方 A 强）的党员，按互补分排序；千问生成 matchReason，失败用模板。
- **`POST /api/v1/pair-help/request`** — 发起结对申请
  - 请求：`{ helperId: int(必填) }` → 写 `pair_help_requests`（status=0，rec 为当前用户）。
- **`PUT /api/v1/pair-help/request/{id}/accept`** — 接受（helper 同意）
  - 成功则 status=1，并写 `pairhelprecords{ HelperId, HelpReceiverId, StartTime=now }`。
- **`PUT /api/v1/pair-help/request/{id}/reject`** — 拒绝（status=2）。
- **`GET /api/v1/pair-help/my`** — 我的结对（两个方向）
  - 响应 `data: { active: [ { recordId, partnerId, partnerName, partnerOrgName, role: "helper"|"receiver", startTime, weaknessTags } ], history: [...] }`。
- **`PUT /api/v1/pair-help/{recordId}/complete`** — 结束结对
  - 请求：`{ outcomeSummary?: string }` → 写 `pairhelprecords.OutcomeSummary`、`EndTime`，并把对应 request 置 status=3。
- **`POST /api/v1/pair-help/{recordId}/log`** — 记录帮扶过程（可选）
  - 请求：`{ content: string }` → 追加进 `pairhelprecords.HelpContentJson`（JSON 数组：`[{time, content}]`）。

---

## (11) 党史 PK 双人对战

**现状**：`battlerecords` 表存在（ResultJson varchar(1000)，schema 未定），无业务端点。积分类目 `PointSourceType.BattleVictory` 已定义。

### 11.1 新增表 `battle_games`（对局）
```sql
CREATE TABLE battle_games (
  id                    INT PRIMARY KEY AUTO_INCREMENT,
  challenger_id         INT NOT NULL,             -- 发起方
  opponent_id           INT NOT NULL,             -- 应战方
  status                INT NOT NULL DEFAULT 0,   -- 0=待应战 1=进行中 2=已完成 3=已取消 4=已超时
  question_ids          JSON NOT NULL,            -- 对局锁定题目 id 数组（开局时随机抽，双人同卷）
  challenger_score      INT NOT NULL DEFAULT 0,
  opponent_score        INT NOT NULL DEFAULT 0,
  current_question_index INT NOT NULL DEFAULT 0,  -- 双人同题同步推进（同一题答完才进下一题）
  timeout_minutes       INT NOT NULL DEFAULT 10,
  created_at            DATETIME(6) NOT NULL,
  started_at            DATETIME(6) NULL,
  finished_at           DATETIME(6) NULL,
  INDEX idx_bg_challenger (challenger_id), INDEX idx_bg_opponent (opponent_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```
- **对战题库**：复用现有 `questions` 表即可（随机抽 `question_type IN (0,2)` 的党史相关题）。如需**对战专用题库**，建议：
```sql
CREATE TABLE battle_questions (
  id              INT PRIMARY KEY AUTO_INCREMENT,
  question_type   INT NOT NULL,                   -- 0单选 2判断（对战用客观题）
  stem            TEXT NOT NULL,
  options         JSON NOT NULL,
  correct_answer  VARCHAR(200) NOT NULL,
  score           INT NOT NULL DEFAULT 10,
  difficulty      INT NOT NULL DEFAULT 1,         -- 1/2/3
  tag             VARCHAR(50) NULL,               -- 党史/党章/时政
  created_at      DATETIME(6) NOT NULL,
  INDEX idx_bq_type (question_type, difficulty)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```
- 对局同步采用**简单轮询**（前端每 2s 拉当前题号/对手分数），不做 websocket（与现有技术栈一致）。

### 11.2 端点（PartyMember）
- **`POST /api/v1/battles`** — 创建对局
  - 请求：`{ opponentId: int(必填), questionCount?: int=5, difficulty?: int|null }`
  - 响应 `data: { gameId, questionCount, expiresAt }`；服务端随机抽题锁定 `question_ids`，status=0（待应战），生成通知给对手（Type=3 系统通知）。
- **`GET /api/v1/battles/pending`** — 我收到的待应战列表
  - 响应 `data: [ { gameId, challengerId, challengerName, questionCount, createdAt } ]`。
- **`POST /api/v1/battles/{id}/accept`** — 应战（status 0→1，started_at=now）
  - 响应 `data: { gameId }`。
- **`POST /api/v1/battles/{id}/cancel`** — 取消（status→3，仅创建方可）。
- **`GET /api/v1/battles/{id}/questions`** — 取当前题（双人同步）
  - 响应 `data: { gameId, index, total, question: { questionId, stem, options, score } | null(对战结束) , myScore, opponentScore, opponentName }`（**不含答案**）。
- **`POST /api/v1/battles/{id}/answer`** — 提交单题答案
  - 请求：`{ questionId: int(必填), answer: string(必填), durationMs: int }`
  - 响应 `data: { correct: bool, correctAnswer: "string(答完后返回)", myScore, opponentScore, nextIndex }`。判题复用 MobileService.CheckAnswer 逻辑；两人都答完该题（或超时）才 index+1。
- **`POST /api/v1/battles/{id}/finish`** — 提前结束/正常收尾判分
  - 响应 `data: { winnerId, winnerName, myScore, opponentScore, isDraw, result: "win|lose|draw" }`。
- **`GET /api/v1/battles/{id}/result`** — 结果查看
  - 响应同上；服务端最终把对局写入 `battlerecords`：`{ ChallengerId, OpponentId, ResultJson: { "winnerId", "challengerScore", "opponentScore", "questions":[{questionId,result}] }, BattleTime }`。
- **积分**：胜利方 `PointService.AddPointsAsync(winner, 10, PointSourceType.BattleVictory, gameId)`；平局双方各 +3。失败时不发积分。

---

## (12) 红色教育基地打卡 + AI 解读

**现状**：`POST /api/v1/check-in` 只存 LocationName，`GetAiBackgroundAsync` 为静态字典占位；`checkinrecords` 无 siteId。

### 12.1 新增表 `education_sites`
```sql
CREATE TABLE education_sites (
  id                 INT PRIMARY KEY AUTO_INCREMENT,
  name               VARCHAR(200) NOT NULL UNIQUE, -- 基地名称
  address            VARCHAR(500) NULL,
  description        TEXT NULL,
  historical_facts   VARCHAR(2000) NULL,           -- 历史事件要点（逗号分隔或 JSON 数组字符串）
  ai_interpretation  VARCHAR(2000) NULL,           -- 预置 AI 解读文案（人工维护，打卡时优先取）
  cover_url          VARCHAR(500) NULL,
  latitude           DECIMAL(10,6) NULL,
  longitude          DECIMAL(10,6) NULL,
  created_at         DATETIME(6) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

### 12.2 打卡表新增列（迁移）
`ALTER TABLE checkinrecords ADD COLUMN site_id INT NULL;`（FK education_sites.Id，可空，兼容旧记录）。

### 12.3 扩展现有端点 `POST /api/v1/check-in`
**请求体**（在现有 `CreateCheckInRequest{ LocationName, Note? }` 基础上**新增**）：
```jsonc
{
  "locationName": "string, 必填",                // 现有（兼容手输）
  "note": "string|null",
  "siteId": "int|null, 选填"                     // 新增：教育站点 id（提供则 locationName 可自动回填）
}
```
**响应体 data**（`CheckInRecordDto` 基础上**新增**）：
```jsonc
{
  "id": "int", "partyMemberId": "int", "memberName": "string",
  "locationName": "string", "checkInTime": "datetime", "note": "string|null",
  "siteId": "int|null",                          // 新增
  "siteName": "string|null",                     // 新增
  "aiBackgroundInterpretation": "string|null",   // 现有字段，本次真正填充
  "historicalFacts": ["string"],                 // 新增：站点历史要点
  "pointsEarned": 5
}
```
**AI 解读规则**：
1. 提供 `siteId`：优先取 `education_sites.ai_interpretation`；为空时调千问生成（用 historical_facts 当素材）并回写站点。
2. 未提供 siteId：`locationName` 命中站点名则按站点处理；否则调千问（`ChatJsonAsync` 返回 `{interpretation, historicalFacts[]}`），**替换现有静态字典占位**。
3. 千问失败 → 降级用静态字典兜底（保留现有 4 条）。
4. 打卡积分仍 +5，写 learningpoints（SourceType=ActivityCheckIn）。

### 12.4 新增站点端点（PartyMember 只读 / SystemAdmin 可维护）
- `GET /api/v1/education-sites?keyword=&page=&size=`：分页列表，`data` 项含上述站点字段（不含 ai_interpretation 可含 description）。
- `GET /api/v1/education-sites/{id}`：详情（含 ai_interpretation）。
- `POST /api/v1/education-sites` / `PUT /api/v1/education-sites/{id}` / `DELETE /api/v1/education-sites/{id}`（SystemAdmin）：站点维护。
- `GET /api/v1/education-sites/{id}/checkins`：该站点打卡记录（分页，含成员名/时间/解读）。

---

## (13) AI 分阶段学习路线图

**现状**：仅"推荐 + 学习报告"，无独立路线规划。

### 13.1 新增端点 `POST /api/v1/ai/learning-roadmap`（PartyMember 本人 / SystemAdmin 代查）
- **请求体**：
```jsonc
{
  "memberId": "int|null, 选填",                  // 默认当前登录用户
  "target": "string|null, 选填",                 // 目标描述（如"通过预备党员转正考试""系统学习二十大精神"）
  "focusTags": ["string"],                       // 选填：指定重点（缺省取 KMeans 薄弱标签）
  "periodDays": "int, 选填, 默认30, 范围7-90",
  "difficulty": "string|null, 选填"              // easy|normal|hard
}
```
- **响应体 data**：
```jsonc
{
  "memberId": "int", "memberName": "string",
  "currentLevel": "string",                      // 由当前进度推导：入门|进阶|冲刺
  "target": "string",
  "focusTags": ["string"],
  "totalDays": "int",
  "stages": [
    {
      "stageNo": 1, "stageName": "string",       // 如 "基础夯实期"
      "durationDays": "int",
      "objectives": ["string"],
      "contents": [                              // 从 learningcontents 按 tag/难度筛选 + 知识库补内容
        { "contentId": "int|null", "title": "string", "contentType": "int", "source": "library|knowledge", "reason": "string" }
      ],
      "exam": { "suggestedCount": "int", "targetScore": "double|null" } | null,
      "kpis": [ { "metric": "durationMinutes", "target": 300 } ]
    }
  ],
  "nextAction": "string",                        // 千问给出的第一步建议
  "generatedAt": "datetime"
}
```
- 实现：读 `member_learning_progress`/KMeans 薄弱点 → 分 3 阶段（基础/强化/冲刺）→ 每阶段从 `learningcontents`（IsPublic + 标签）与 `knowledge/documents` 选内容 → 千问润色 objectives/kpis（失败用模板）。

---

## (14) 精准分层消息推送

**现状**：`POST /api/v1/notifications/send`（单发）、`batch-send`（按 ID 列表），无按组织/角色/党员类型筛选。

### 14.1 新增端点 `POST /api/v1/notifications/targeted-send`（SystemAdmin / BranchSecretary）
- **请求体**：
```jsonc
{
  "title": "string, 必填",
  "content": "string, 必填",
  "type": "int, 必填",                           // NotificationType：0任务 1测验 2预警 3系统
  "filter": {
    "organizationId": "int|null",                // 组织筛选（含下级）
    "includeDescendants": "bool, 选填, 默认true", // false 时仅本组织
    "roles": ["int"],                            // 选填：UserRole 枚举数组（默认全部党员 0）
    "memberTypes": ["string"],                   // 选填：["正式党员","预备党员","发展对象","积极分子"]
    "excludeMemberIds": ["int"],                 // 选填：排除项
    "onlyEnabled": "bool, 选填, 默认true"        // 仅 IsEnabled
  },
  "dryRun": "bool, 选填, 默认false"              // true 只返回匹配名单不落库
}
```
- **响应体 data**：
```jsonc
{
  "matchedCount": "int",
  "matchedMemberIds": ["int"],                   // dryRun 时给全量
  "sentCount": "int",                            // 非 dryRun = matchedCount（批量写入成功数）
  "skippedCount": "int"
}
```
- 实现：`OrgHierarchyHelper.CollectOrgAndDescendantIds` 展开组织 → partymembers 按 roles/memberTypes/IsEnabled 过滤 → 批量插 `messagenotifications`（单条 IsRead=0）。

---

## (15) 随机弹窗问答防挂机

**现状**：`GET /api/v1/anti-cheat/challenge`（3 个固定题，内存 2 分钟过期）、`POST /anti-cheat/verify`、`GET /anti-cheat/stats`（Random 模拟）。

### 15.1 新增表 `anticheat_records`
```sql
CREATE TABLE anticheat_records (
  id             INT PRIMARY KEY AUTO_INCREMENT,
  party_member_id INT NOT NULL,                  -- FK partymembers.Id
  content_id     INT NULL,                       -- 关联正在学习的内容（可选）
  question_id    INT NULL,                       -- 从 questions 随机抽的题
  challenge_id   VARCHAR(64) NOT NULL,
  is_pass        TINYINT(1) NOT NULL,
  verified_at    DATETIME(6) NOT NULL,
  INDEX idx_acr_member (party_member_id, verified_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

### 15.2 扩展现有端点
- **`GET /api/v1/anti-cheat/challenge?contentId=&random=true`**（PartyMember）
  - 从 `questions` 随机抽 1 题（单选/判断，不含答案）替换固定 3 题；`random=true` 时每次不同题。响应 `data: { challengeId, question: { questionId, stem, options, questionType }, expiresAt(2分钟内), contentId }`。挑战仍存内存 `ConcurrentDictionary`（2 分钟过期）。
- **`POST /api/v1/anti-cheat/verify`**（PartyMember）
  - 请求：`{ challengeId(必填), answer: string(必填), contentId?: int }`
  - 响应 `data: { isValid: bool, correct: bool, message: "string", validSeconds: "int(本次有效时长，答对=剩余窗口或固定30s，答错=0)" }`。
  - 逻辑：判题复用 MobileService.CheckAnswer；`is_pass` 写 `anticheat_records`；**仅 isValid=true 时向调用方返回有效时长，前端才将该时段计入 progress 上报**（与 `POST /api/v1/mobile/progress` 的 duration_seconds 衔接：前端把验证通过后发生的时长才上报；后端可选在 progress 上报时校验最近一次 verify 通过）。
  - 防重放：challengeId 单次有效；连续答错可限制频次（如 1 分钟内最多 3 次验证）。
- **`GET /api/v1/anti-cheat/stats?orgId=`**（SystemAdmin / BranchSecretary）
  - 由 `anticheat_records` 统计真实数据，替换 `new Random(member.Id)` 模拟：
  - 响应 `data: { totalChecks, passCount, failCount, passRate, effectiveMinutes, byMember: [ { memberId, memberName, organizationName, checks, passes, fails, effectiveMinutes } ] }`。

---

## 前端页面 ↔ 端点映射表（新增/修改）

| # | 功能 | 前端页面（新增/修改） | API 模块（前端） | 依赖后端端点 |
|---|---|---|---|---|
| 1 | NL2SQL 多轮上下文 | 修改 admin `AiChat.vue`（会话历史区+指代示例） | admin `api\ai.js`（改 `aiQuery`） | `POST /nl2sql/query`（扩展）、`GET /nl2sql/sessions/{id}/history` |
| 2 | RAG 两级检索+重排 | 修改 member `AiChat.vue`（来源卡片+置信度条）、admin `AiChat.vue` | member `api\ai.js`、admin `api\ai.js` | `POST /ai-knowledge/query`（扩展）、`GET/POST /ai-knowledge/documents*` |
| 3 | 宣讲稿/文章/知识卡片 | 新增 member `ContentGenerate.vue`（或并入学习中心）、admin 新增素材生成页 | 两端 `api\ai.js`（改 `aiContentGenerate`） | `POST /ai-content/generate`（扩展 contentType） |
| 4 | AI 评选学习标兵 | 新增 admin `StarMembers.vue`（入口 DataAnalysis.vue） | admin `api\ai.js`（新增 `starMembers`） | `POST /ai/star-members` |
| 5 | 三会一课 AI 简报 | 修改 admin `OrgLife.vue`（增加"生成简报"按钮+简报预览） | admin `api\meeting.js`（新增 `aiBrief`） | `POST /meeting-activities/ai-brief`、`POST /meeting-activities/{id}/ai-summary` |
| 6 | 思想汇报 AI 建议 | 修改 member `Profile.vue`/新增汇报页（编辑器旁"AI 建议"）、admin 详情 | member `api\partyDevelopment.js`（admin 同） | `POST /party-development/{id}/ai-report-suggestion` |
| 7 | 发展材料 AI 校验 | 修改 admin `Organization.vue`（发展管理 tab）+ member 本人页 | admin `api\partyDevelopment.js` | `POST /party-development/{id}/ai-material-check` |
| 8 | 发展到期提醒 | 修改 admin `Organization.vue`（提醒 tab：列表+触发按钮） | admin `api\partyDevelopment.js` | `POST /party-development/reminders/trigger`、`GET .../reminders/list`、`GET .../reminders` |
| 9 | 支部评级+整改 | 修改 admin `Organization.vue`/`DataAnalysis.vue`（评级徽标+整改表+完成勾选） | admin `api\ai.js`、`api\organization.js` | `POST /ai/organization-report`（扩展）、`GET/POST /organizations/{id}/rectifications`、`PUT .../rectifications/{id}/complete`、`PUT .../rectifications/{id}/status` |
| 10 | 薄弱点互助 | 新增 member `PairHelp.vue`（推荐/申请/我的结对） | member 新增 `api\pairHelp.js` | `POST /pair-help/recommend`、`/request`、`PUT /pair-help/request/{id}/accept|reject`、`GET /pair-help/my`、`PUT /pair-help/{recordId}/complete`、`POST /pair-help/{recordId}/log` |
| 11 | 党史 PK | 新增 member `Battle.vue`（大厅/对局/结果） | member 新增 `api\battle.js` | `POST /battles`、`GET /battles/pending`、`POST /battles/{id}/accept|cancel|answer|finish`、`GET /battles/{id}/questions|result` |
| 12 | 红色基地打卡 | 修改 member `Home.vue`（打卡页增加站点选择+AI 解读展示） | member `api\checkin.js`（扩展）、新增 `api\educationSite.js` | `POST /check-in`（扩展）、`GET /education-sites`、`GET /education-sites/{id}` |
| 13 | 学习路线图 | 新增 member `LearningRoadmap.vue`（入口 Home/Report） | member 新增 `api\ai.js` 函数 | `POST /ai/learning-roadmap` |
| 14 | 精准推送 | 修改 admin `Organization.vue`（消息推送弹窗：组织/角色/类型筛选） | admin `api\notification.js`（新增 `targetedSend`） | `POST /notifications/targeted-send` |
| 15 | 弹窗防挂机 | 修改 member `LearningCenter.vue`（随机弹窗组件）、admin `Organization.vue`（防挂机 tab 真实统计） | member 新增 `api\antiCheat.js`、admin `api\statistics.js` | `GET /anti-cheat/challenge`、`POST /anti-cheat/verify`、`GET /anti-cheat/stats`（均扩展） |

**新增路由/菜单落点**：member 端在 `src\router\index.js` 注册 + `layouts\MainLayout.vue` 加菜单；admin 端同样在 router + MainLayout 注册。新页面统一 `<script setup>` + Element Plus + 复用 `request.js`。

---

## 附：契约设计不确定点（供开工前确认）

1. **(11) 对战同步方式**：采用双人同卷 + 前端轮询（与现状一致、无 websocket），若要求实时需额外引入 SignalR——默认按轮询设计。
2. **(11) 对战题目来源**：默认复用现有 `questions` 表；是否需要独立 `battle_questions` 表（建议结构已给出）由产品定。
3. **(15) "答对才累计有效时长"的核算边界**：后端严格模式需在 `POST /mobile/progress` 校验最近 verify 记录；默认采用"前端答对后把该窗口时长上报 + 后端按 verify 记录统计有效时长"的软约束，如需强约束改为硬校验（会要求前端改动上报时机）。
4. **(9) 评级维度与权重**：默认权重已给出；如需组织生活参与率等额外维度，需补充活动签到数据（当前 activityhearts/meetingactivities 无签到名单，仅心得）。
5. **(8) 到期提醒类型**：默认 3 类（预备转正/材料缺失/思想汇报逾期）；材料与汇报"逾期"判断基准（90 天/季度）可按需调整。
6. **(3) PDF 解析**：现状仅文本抓取；如需真文档解析（版面/图表）需引入解析库，本次契约按纯文本处理。
7. **(2) 多轮记忆存储**：与 (1) 共用 `nl2sql_sessions` 或单开知识问答会话表？默认共用一张会话表 + 加 `biz_type` 区分（若单开请告知，改表结构）。
8. **(14) 推送去重**：同一 member 在 filter 中既命中组织又命中角色时按 union 去重（默认）；是否需要"每人每任务限一条"需业务规则。
9. **(4)(10) 弱项标签口径**：KMeans 输出为 `WeaknessHeatmapDto{Tag,ErrorCount,Intensity}`，作为 (4)(10)(13) 的通用输入，需确认标签字典来源（question_categories vs 题干关键词）统一。
10. **(12) 打卡重复限制**：同一人同一天同一 site 是否限一次打卡/是否重复给积分——默认限一次、不重复给分，需确认。
