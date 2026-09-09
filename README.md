# 党建学习系统 (Party School System)

基于 ASP.NET Core .NET 8 + Vue3 + Element Plus 的党建学习管理平台，集成千问大模型AI能力。

## 技术栈

| 层 | 技术 | 端口 |
|----|------|------|
| 后端 | ASP.NET Core .NET 8 + EF Core + MySQL 8.0 | 5091 |
| 管理端 | Vue3 + Element Plus + ECharts | 5173 |
| 党员端 | Vue3 + Element Plus + ECharts | 5174 |
| AI | 千问大模型（DashScope API） | - |

## 快速启动

```bash
# 1. 启动后端
cd backend
dotnet run --urls http://localhost:5091

# 2. 启动管理端（新终端）
cd frontend/frontend-admin
npm install
npm run dev

# 3. 启动党员端（新终端）
cd frontend/frontend-member
npm install
npm run dev
```

## 测试账号

| 角色 | 手机号 | 密码 | 说明 |
|------|--------|------|------|
| 系统管理员 | 13800000000 | 123456 | 管理端登录 |
| 支部书记 | 13800000001 | 123456 | 第一支部书记 |
| 普通党员 | 13800000002 | 123456 | 第一支部党员 |

## 目录结构

```
party-school-system/
├── backend/                    # ASP.NET Core 后端
│   ├── Controllers/            # 26个API控制器（174端点）
│   ├── Services/               # 业务服务（接口+实现）
│   ├── Models/                 # 实体/DTO/枚举
│   ├── Data/                   # EF Core DbContext
│   ├── Helpers/                # 辅助类
│   ├── Middleware/             # 中间件
│   └── Program.cs              # 入口
├── frontend/
│   ├── frontend-admin/         # 管理端（12页面）
│   └── frontend-member/        # 党员端（14页面）
├── docs/                       # 项目文档
├── releases/                   # 版本发布说明
├── seed_data.sql               # 种子数据（各表≥10条）
├── CHANGELOG.md                # 项目变更日志
└── .gitignore
```

## 功能概览

### AI功能（15项）
RAG知识问答(BM25) / K-Means错题聚类 / 个性化推荐 / 学习路线图 / 个人报告 / NL2SQL / 数据脱敏 / 内容生成 / 组织生活总结 / 支部考核 / 发展辅助 / 学习预警 / 互助匹配 / 学习标兵 / 语音输入

### 基础功能（16项）
党员管理 / 学习任务 / 消息通知 / 考试中心 / 组织生活 / 党员发展 / 数字驾驶舱 / 数据权限 / 防挂机 / 红色基地 / 积分体系 / 党史PK / 结对帮扶 / 分层任务 / 个人中心 / 任务统计

## 工程规范

1. **版本管理**：releases/ 目录按版本号分文件夹存放发布说明，完整代码通过 git tag 管理
2. **测试文件**：*.test.cs / test_*.py / *.spec.js 不上传，已加入 .gitignore
3. **目录说明**：每个关键目录都有 README.md 说明用途和结构
4. **变更日志**：CHANGELOG.md 记录每次修改，避免AI上下文丢失导致的幻觉
5. **路径规范**：前端 request.js 使用相对路径 /api/v1，通过 vite 代理转发；禁止硬编码 localhost

## 千问API配置

在 `backend/` 目录创建 `.env` 文件：
```
DASHSCOPE_API_KEY=你的千问API密钥
```
`.env` 已加入 .gitignore，不会提交到仓库。

## 数据库

MySQL 8.0，数据库名 `party_school`，32张业务表。
种子数据见 `seed_data.sql`，各表≥10条记录。
