# 更新日志 (CHANGELOG)

本项目的所有重要变更都记录在此文件。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/)，
版本号遵循 [语义化版本](https://semver.org/lang/zh-CN/)。

## [Unreleased]

### 安全 (Security)
- **重构 JWT 启动保护**：未配置 `JWT_KEY` 时服务启动失败（杜绝弱密钥风险）
- **个人邮箱清除**：从 `appsettings.json`、`FeedbackSettings.cs`、`docker-compose.yml`、`deploy/.env.example` 中移除个人邮箱硬编码
- **域名占位化**：`listenease.online` 替换为 `your-domain.com` 占位
- **.gitignore 强化**：新增 `*.apk`、`*.keystore`、个人求职文档等规则

### 新增 (Added)
- **Feedback 反馈系统**：Web 用户端反馈页 + 后端 SMTP/SendCloud 双通道邮件发送
- **Study Activity 学习追踪**：监听学习活动（听力完成、文章阅读、单词加词/复习等），写入数据库
- **Analytics 事件埋点**：用户行为事件通过 RabbitMQ 异步持久化
- **管理后台 Dashboard**：ECharts 可视化关键指标
- **WordService 用户词本**：云端单词本管理、SM-2 间隔复习、跨设备同步
- **AdminUsersController**：管理员对账号的管理操作（重置密码、启用/禁用等）
- **EaseWord Android App**：`Front end/wordbook-app`（Capacitor 8 + Vue 3 + Android）
  - 应用内更新检查（`/downloads/version.json`）
  - 云端/本地双模式单词本（逐步收敛为单一数据源）
  - SM-2 复习算法（与 WordService 后端同步）
- **APK 下载通道**：Nginx 提供 `/downloads/` 静态目录（apk + version.json）
- **`/downloads/` 静态目录**：用户端 Web 提供 `version.json`、`APK` 文件
- **`docs/消息队列学习指南.md`**：RabbitMQ 集成事件总线深度讲解
- **`SECURITY.md`**：公开仓库安全策略与部署清单

### 变更 (Changed)
- `README.md`：完整重写，加入新模块、架构图、多端支持说明
- `docs/1.1 项目说明.md`：架构图更新（加入 RabbitMQ、APK 通道）
- `deploy/.env.example`：加入 SMTP / SendCloud / FEEDBACK_RECIPIENT_EMAIL 配置项
- `deploy/docker-compose.yml`：加入邮件相关环境变量；CORS 改为 `${CORS_ORIGIN_*}` 占位
- `deploy/nginx/nginx.conf`：加入 `/downloads/` 静态服务
- `CommonInit`：服务名规范化

### 修复 (Fixed)
- 重置密码/创建账号流程安全改造（占位修复，明文密码仍然在事件中传递，待 v1.1 重构为重置链接）
- RabbitMQ 默认 `mem_limit` 从 256m 提升到 512m
- 多个 EF Core Migration 增量更新（StudyActivity、UserWordBooks）

### 移除 (Removed)
- 百度站点验证文件 `21a35796e1ac741f8ed5b7dddb30bd80.txt`
- Google 站点验证文件 `deploy/google11436b5ff0397bd5.html`
- `deploy/audios/`、`deploy/admin-ui/`、`deploy/user-ui/` 等构建产物被 `.gitignore` 排除

---

## 历史版本

详细提交历史请查看 [GitHub Commits](#)（公开仓库后填入链接）。