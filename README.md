# 基于AI的数智党校学习系统 v2.0

## 版本说明

这是v2.0版本，在原有版本基础上进行了大量改进和优化。

## 主要更新

### 1. UI设计统一
- 管理后台菜单样式统一为党员端的白色圆角卡片式设计
- 两个前端的页面设计全部采用统一的卡片式菜单风格
- 红色长城背景图统一应用到两个前端

### 2. AI功能增强
- 集成通义千问（Qwen）大模型，所有AI接口通过AiModelService统一调用
- AI学习报告实现五维动态评分（学习时长、内容完成、任务完成、测验成绩、错题掌握）
- AI评语和建议由大模型生成（配置API Key后自动使用，未配置时回退到模板模式）
- 6个AI接口全部测试通过：AI学习报告、AI党建知识问答、AI内容生成、NL2SQL、AI个性化推荐、AI错题聚类

### 3. 功能完善
- 管理后台五项子菜单收纳（工作台、组织管理、学习管理、组织生活、数据中心）
- 待办提醒完成实际上报进度（ContentDetail.vue每30秒自动上报，视频结束自动上报完成）
- 任务关联试卷完成后自动关闭任务
- 学习中心"公共素材"改为"学习内容"
- 动态随机组卷测验（3份随机试卷+6个随机测验，题目随机抽取）

### 4. 数据修复
- 删除学习内容表中的乱码记录（ID 34-39）
- 更新视频内容的VideoUrl为公开可访问的示例视频
- 积分排行榜排除系统管理员，按积分降序排列
- AI学习报告维度名称前后端统一

### 5. 接口验证
- 36个后端接口全部测试通过，0个404/500错误
- 涵盖认证、组织管理、党员管理、学习内容、学习任务、题库、试卷测验、移动端、统计、AI功能、其他功能等11个模块

## 项目结构

```
├── backend/                    # 后端 ASP.NET Core 8 API
│   ├── Controllers/           # 控制器
│   ├── Services/              # 服务层
│   │   ├── Interfaces/        # 服务接口
│   │   └── Implementations/   # 服务实现（含QwenService）
│   ├── Models/                # 数据模型
│   │   └── Common/            # 通用模型（含QwenChatMessage）
│   ├── Data/                  # 数据上下文
│   └── Helpers/               # 辅助工具
└── frontend/
    ├── frontend-admin/        # 管理后台 Vue 3
    └── frontend-member/       # 党员端 Vue 3
```

## 技术栈

- **后端**: ASP.NET Core 8 + EF Core + MySQL + JWT
- **前端**: Vue 3 + Vite + Element Plus + Axios + Pinia + Vue Router + ECharts
- **AI模型**: 通义千问（Qwen）- DashScope OpenAI兼容模式

## 快速开始

### 后端
```bash
cd backend
dotnet restore
dotnet run
```

### 管理后台前端
```bash
cd frontend/frontend-admin
npm install
npm run dev
```

### 党员端前端
```bash
cd frontend/frontend-member
npm install
npm run dev
```

## AI模型配置

在 `backend/appsettings.json` 中配置通义千问API Key：

```json
{
  "Qwen": {
    "BaseUrl": "https://dashscope.aliyuncs.com/compatible-mode/v1",
    "Model": "qwen-plus",
    "ApiKey": "your-api-key-here"
  }
}
```

配置API Key后，所有AI接口自动使用真实大模型；未配置时自动回退到模拟/模板模式。

## 测试账号

- **管理员**: 手机号 13800000000，密码 123456
- **党员**: 手机号 13800000002，密码 123456

## 版本历史

- **v1.0**: 初始版本，基础功能实现
- **v2.0**: UI设计统一、AI功能增强、功能完善、数据修复、接口全面验证
