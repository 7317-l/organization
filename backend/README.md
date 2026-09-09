# 后端服务 (backend/)

ASP.NET Core .NET 8 Web API，端口 5091。

## 目录结构

```
backend/
├── Controllers/      # 26个API控制器（25个业务+1个占位）
├── Services/         # 业务服务接口和实现
│   ├── Interfaces/   # 服务接口定义
│   └── Implementations/  # 服务实现
├── Models/           # 实体、DTO、枚举、通用模型
│   ├── Entities/     # EF Core实体（数据库表映射）
│   ├── DTOs/         # 数据传输对象
│   └── Common/       # 通用模型（ApiResponse、PagedResponse等）
├── Data/             # AppDbContext 数据库上下文
├── Helpers/          # 辅助类（CurrentUserService、JWT等）
├── Middleware/       # 中间件（异常处理、BusinessException）
├── Migrations/       # EF Core迁移文件
├── Program.cs        # 应用入口和DI注册
└── appsettings.json  # 配置文件（数据库连接、千问API等）
```

## 启动方式

```bash
cd backend
dotnet run --urls http://localhost:5091
```

## 数据库

MySQL 8.0，数据库名 `party_school`，32张业务表。
连接字符串在 `appsettings.json` 的 `ConnectionStrings.DefaultConnection`。

## 千问API配置

密钥从 `.env` 文件读取（`DASHSCOPE_API_KEY`），`.env` 不提交到git。
