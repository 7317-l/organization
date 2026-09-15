# XX党建 · 党员学习平台（PC端）

基于 Vue 3 + Vite + Element Plus + ECharts 构建的党员学习平台前端项目。

## 技术栈

- **构建工具**: Vite 5
- **框架**: Vue 3 (Composition API)
- **语言**: JavaScript
- **路由**: Vue Router 4
- **状态管理**: Pinia
- **HTTP 客户端**: Axios
- **UI 组件库**: Element Plus
- **图表库**: ECharts 5
- **图标**: @element-plus/icons-vue

## 项目结构

```
party-school-member/
├── index.html                 # 入口 HTML
├── package.json               # 项目依赖
├── vite.config.js             # Vite 配置
├── README.md                  # 项目说明
└── src/
    ├── main.js                # 应用入口
    ├── App.vue                # 根组件
    ├── api/                   # API 接口层
    │   ├── request.js         # Axios 实例（拦截器）
    │   ├── auth.js            # 认证接口
    │   ├── content.js         # 内容接口
    │   ├── task.js            # 任务接口
    │   ├── exam.js            # 考试接口
    │   ├── mobile.js          # 移动端通用接口
    │   ├── ai.js              # AI 接口
    │   ├── points.js          # 积分接口
    │   ├── checkin.js         # 打卡接口
    │   └── notification.js    # 通知接口
    ├── router/                # 路由
    │   └── index.js
    ├── stores/                # Pinia 状态管理
    │   └── user.js
    ├── layouts/               # 布局组件
    │   └── MainLayout.vue
    ├── views/                 # 页面组件
    │   ├── Login.vue          # 登录页
    │   ├── Home.vue           # 首页
    │   ├── LearningCenter.vue # 学习中心
    │   ├── ContentDetail.vue  # 内容详情
    │   ├── ExamCenter.vue     # 考试中心
    │   ├── Quiz.vue           # 答题页
    │   ├── AiChat.vue         # AI 聊天
    │   ├── Report.vue         # 学习报告
    │   └── Profile.vue        # 我的
    ├── styles/                # 全局样式
    │   └── global.css
    └── utils/                 # 工具函数
        └── format.js
```

## 快速开始

### 安装依赖

```bash
npm install
```

### 启动开发服务器

```bash
npm run dev
```

访问 http://localhost:5173

### 构建生产版本

```bash
npm run build
```

### 预览生产构建

```bash
npm run preview
```

## 后端接口配置

- **Base URL**: `http://localhost:5091/api/v1`
- **认证方式**: JWT（请求头携带 `Authorization: Bearer <accessToken>`）
- 配置文件: `src/api/request.js`

如需修改后端地址，编辑 `src/api/request.js` 中的 `baseURL`。

## 主要功能

### 1. 登录页
- 手机号 + 密码登录
- 调用 `POST /auth/login`，保存 accessToken 到 localStorage

### 2. 主布局
- 顶部深红 Header（Logo、用户名、退出登录）
- 左侧白色 Sidebar（首页、学习中心、考试中心、我的）
- 右侧内容区

### 3. 首页
- 欢迎语 + 当前用户信息
- 统计卡片（待办任务数、学习进度、总积分）- 来自 `/mobile/overview`
- 待办提醒列表 - 来自 `/mobile/tasks/pending` 和 `/mobile/exams`
- AI 推荐内容 - 来自 `/mobile/recommendations`
- 积分排行榜 - 来自 `/points/ranking`

### 4. 学习中心
- **Tab1 公共素材**: `GET /mobile/contents`，支持分类筛选、搜索、分页
- **Tab2 支部专属任务**: `GET /mobile/tasks/pending` + `/mobile/tasks/completed`
- **Tab3 AI定制学习路线**: `POST /mobile/report/ai-assessment` 或 `/mobile/recommendations`

### 5. 内容详情页
- `GET /mobile/contents/{contentId}` 获取详情
- 视频使用原生 `<video>` 标签播放
- 自动/手动上报学习进度: `POST /mobile/progress`
- 任务内容可标记完成: `POST /mobile/tasks/complete`

### 6. 考试中心
- **Tab1 待考测验**: `GET /mobile/exams`（status=pending）
- **Tab2 历史试卷**: `GET /mobile/exams`（status=completed），可查看详情
- **Tab3 错题本**: 从测验结果中聚合提取错题（调用 `GET /mobile/exams/{testId}/result`）
- **Tab4 AI专项巩固练习**: `POST /kmeans/cluster` 获取薄弱知识点

### 7. 答题页
- `GET /mobile/exams/{testId}/start` 获取试卷题目
- 支持单选、多选、判断题
- 倒计时功能
- 答题卡快速跳转
- 提交答案: `POST /mobile/exams/submit`
- 显示得分和结果: `GET /mobile/exams/{testId}/result`

### 8. AI 聊天页
- `POST /ai-knowledge/query` 获取 AI 回答
- 对话气泡展示
- 快捷问题入口
- 打字加载动画

### 9. 学习报告页
- `POST /mobile/report/ai-assessment` 生成 AI 学习报告
- 综合评分展示
- ECharts 雷达图展示学习维度
- 维度分析进度条
- AI 评语 + 改进建议
- "生成学习路线"按钮

### 10. 我的页面
- 个人信息卡片
- 积分明细: `GET /points/records`（弹窗表格）
- 打卡记录: `GET /check-in/my`（弹窗表格）
- 消息通知: `GET /notifications/unread`（弹窗列表）
- 账号设置（UI 展示）

## 设计规范

- **主色调**: `#C8161D`（党建红）
- **整体风格**: 顶部深红 Header + 左侧白色 Sidebar + 右侧内容区
- **所有数据**: 均通过后端 API 获取，无写死的测试数据
- **列表页**: 均有 loading 状态和空状态（el-empty）
- **分页**: 统一使用 `page`、`size` 参数，返回 `data.items`、`data.total`

## 错题本数据来源说明

由于后端未提供独立的错题接口，错题本数据通过以下方式聚合获取：
1. 调用 `GET /mobile/exams` 获取已完成的测验列表
2. 对每个已完成测验调用 `GET /mobile/exams/{testId}/result` 获取详情
3. 从结果中提取答错的题目（`isCorrect === false`），聚合为错题本

如后端后续提供独立错题接口，可直接替换 `ExamCenter.vue` 中的 `loadWrongQuestions` 方法。
