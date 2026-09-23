# ListenEase

*让每一次聆听，都成为进步的阶梯*

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![Vue](https://img.shields.io/badge/Vue-3-4FC08D)](https://vuejs.org/)

一个开源的英语学习平台，覆盖**听力真题训练、每日外刊阅读、词根学习、学习追踪、用户反馈**等模块，支持 Web、小程序、Android 多端访问。

---

## ✨ 核心功能

| 模块 | 功能描述 |
|------|----------|
| 🎧 **听力训练** | 按分类浏览听力试卷（CET-4/6 等），逐题播放音频并显示同步字幕，支持下载试卷 PDF 与答案 |
| 📰 **外刊阅读** | 每日推送中英双语文章（BBC 等），支持已读标记、收藏、音频朗读 |
| 📝 **词根学习** | 基于词根词缀法的系统化单词学习；含示例词、释义、间隔复习算法（SM-2）、学习记录 |
| 👤 **用户系统** | 邮箱注册登录、JWT 认证、密码重置、学习进度追踪 |
| 🖥️ **后台管理** | 用户管理、试卷/文章/词根增删改查、音频上传、数据统计（Dashboard） |
| 📊 **学习追踪** | 学习活动（Study Activity）记录，Analytics 事件埋点，管理后台可视化看板 |
| 💬 **用户反馈** | Web/App 内置反馈入口，通过 SMTP 或 SendCloud 转发到管理员邮箱 |
| 📱 **多端支持** | 用户端（Vue 3）、管理端（Vue 3）、微信小程序、Capacitor Android App（EaseWord） |

---

## 🏗️ 技术架构

```
                    ┌──────────────────────┐
                    │  Nginx (HTTPS :443)  │  ← TLS 1.3 / Let's Encrypt / HTTP/2 / gzip
                    └──────────┬───────────┘
                               │
        ┌──────────┬───────────┼───────────┬──────────┐
        │          │           │           │          │
   ┌────▼───┐ ┌───▼────┐ ┌────▼────┐ ┌────▼───┐ ┌────▼────┐
   │user-ui │ │admin-ui│ │Identity │ │ Listen │ │ Article │
   │(SPA)   │ │(SPA)   │ │ Service │ │Service │ │ Service │
   └────────┘ └────────┘ │  :8080  │ │ :8080  │ │  :8080  │
                         └────┬────┘ └────┬───┘ └────┬────┘
                              │           │          │
                              └─────┬─────┘          │
                                    │                │
                              ┌─────▼──────┐  ┌──────▼─────┐
                              │ Word       │  │  RabbitMQ  │ ← 集成事件总线
                              │ Service    │  │   :5672    │
                              │  :8080     │  └────────────┘
                              └─────┬──────┘
                                    │
                              ┌─────▼──────┐
                              │ SQL Server │
                              │   2022     │  ← 每个微服务独立 Database
                              └────────────┘

   Capacitor Android App (EaseWord) ──┐
   微信小程序 (listen-miniapp) ──┐    │
                        │    │    │
                        └──┴────┴───────→ 通过 HTTPS 调用 Identity / Word API
```

### 后端技术栈

| 模块 | 技术 |
|------|------|
| 框架 | .NET 10 / ASP.NET Core |
| ORM | Entity Framework Core 10 |
| 数据库 | SQL Server 2022 |
| 认证 | ASP.NET Core Identity + JWT |
| 消息队列 | RabbitMQ 3（集成事件总线） |
| 文档 | Swagger / OpenAPI |
| 架构 | DDD（领域驱动设计）+ 微服务 |

### 前端技术栈

| 项目 | 技术栈 | 路径 |
|------|--------|------|
| 用户端 | Vue 3 + Element Plus + Axios + PDF.js | `Front end/en-listen-user-ui` |
| 管理端 | Vue 3 + Element Plus + Vite + Axios + ECharts | `Front end/en-listen-admin-ui` |
| 微信小程序 | 微信原生小程序框架 | `Front end/listen-miniapp` |
| Android App (EaseWord) | Vue 3 + Capacitor 8 + Android | `Front end/wordbook-app` |

### 基础设施

- **容器化**：Docker + Docker Compose
- **反向代理**：Nginx（HTTP/2 + TLS 1.3 + gzip）
- **HTTPS 证书**：Let's Encrypt / Certbot 自动续期
- **邮件服务**：SMTP（推荐 Outlook 587）或 SendCloud
- **CI/CD**：GitHub Actions

---

## 📁 项目结构

```
Listen-en-web/
├── Commons/                     # 公共类库（DDD 接口、EF Core 扩展、JWT、事件总线、缓存等）
├── IdentitySerivce/             # 用户认证微服务（含 SMTP 反馈发送、Analytics 埋点）
│   ├── IdentitySerivce.Domain/
│   ├── IdentitySerivce.Infrastructure/
│   └── IdentityService.WebAPI/
├── ListenService/               # 听力训练微服务
│   ├── ListenService.Domain/
│   ├── ListenService.Infrastrucure/
│   └── ListenService.WebAPI/
├── ArticleService/              # 文章阅读微服务
│   ├── ArticleService.Domain/
│   ├── ArticleService.Infrastructure/
│   └── ArticleService.WebAPI/
├── WordService/                 # 词根学习微服务（含用户词本 SM-2 复习）
│   ├── WordService.Domain/
│   ├── WordService.Infrastrucure/
│   └── WordService.WebAPI/
├── FileService/                 # 文件管理微服务
├── Front end/
│   ├── en-listen-user-ui/       # 用户端 Web（Vue 3）
│   ├── en-listen-admin-ui/      # 管理端 Web（Vue 3）
│   ├── listen-miniapp/          # 微信小程序
│   └── wordbook-app/            # EaseWord Android App（Capacitor）
├── deploy/                      # 部署配置
│   ├── docker-compose.yml
│   ├── nginx/nginx.conf
│   ├── *.WebAPI/Dockerfile
│   └── audios/                  # 听力音频文件目录
├── docs/                        # 项目文档
│   ├── 1.1 项目说明.md
│   ├── 2.1-2.5 服务详细说明
│   └── 3.1 Commons 公共库
├── .github/workflows/           # CI/CD 配置
└── README.md
```

---

## 🚀 快速开始

### 前置要求

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js](https://nodejs.org/) 18+
- [Docker](https://www.docker.com/) & Docker Compose
- [SQL Server](https://www.microsoft.com/sql-server) 2022（Docker 部署会自动拉取）

### 本地开发

#### 1. 启动数据库

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrongPassword123!" \
  -p 1433:1433 --name sqlserver \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

#### 2. 配置环境变量

```bash
cp .env.example .env
# 必须设置：SA_PASSWORD、JWT_KEY（使用 openssl rand -base64 32 生成）
```

修改各服务的 `appsettings.json` 中的数据库连接字符串（如需自定义）。

#### 3. 启动后端服务

```bash
# 以 ListenService 为例
cd ListenService/ListenService.WebAPI
dotnet run
```

每个服务默认监听 `http://localhost:8080`：
- IdentityService：5263
- ListenService：5215
- ArticleService：5216
- WordService：5217

#### 4. 启动前端

```bash
# 用户端（Vue 3 + Vite）
cd "Front end/en-listen-user-ui"
npm install
npm run serve

# 管理端（Vue 3 + Vite）
cd "Front end/en-listen-admin-ui"
npm install
npm run dev

# Android App（Capacitor，需 Android Studio）
cd "Front end/wordbook-app"
npm install
npm run dev
```

### Docker 一键部署

```bash
# 1. 配置环境变量
cp deploy/.env.example deploy/.env
# 编辑 deploy/.env：必须设置 SA_PASSWORD、JWT_KEY、CORS_ORIGIN_0/1、FEEDBACK_RECIPIENT_EMAIL

# 2. 替换域名
sed -i 's/your-domain.com/你的实际域名/g' deploy/nginx/nginx.conf

# 3. 构建前端
cd "Front end/en-listen-user-ui" && npm run build
cd "Front end/en-listen-admin-ui" && npm run dev build

# 4. 将构建产物复制到 deploy 目录
cp -r "Front end/en-listen-user-ui/dist/"* deploy/user-ui/
cp -r "Front end/en-listen-admin-ui/dist/"* deploy/admin-ui/

# 5. 上传听力音频（可选）
cp -r audios/* deploy/audios/

# 6. 一键启动
cd deploy
docker compose up -d
```

部署完成后访问 `https://your-domain.com` 即可。

> 💡 首次部署后，访问 `POST /api/identity/Login/CreateWorld` 创建初始管理员账号。

---

## 🔐 安全说明

- **所有敏感配置通过环境变量注入**，不硬编码到代码
- `.env` / `appsettings.Development.json` 已加入 `.gitignore`
- **JWT Key 强制环境变量注入**：未设置 `JWT_KEY` 时服务启动失败
- 生产环境务必修改默认管理员密码，并启用 HTTPS
- 详见 [SECURITY.md](SECURITY.md)

## 📊 监控与追踪

- IdentityService 内置 **Analytics 事件**（用户行为埋点）和 **Study Activity**（学习活动记录）
- 管理后台 Dashboard 可视化关键指标（DAU、内容消费、加词率、复习完成率等）
- 所有事件通过 RabbitMQ 异步写入，对业务无侵入

## 📱 多端开发

| 端 | 文档 |
|------|------|
| Web 用户端 | [docs/2.5 FrontEnd.md](docs/2.5%20FrontEnd.md) |
| Web 管理端 | [docs/2.5 FrontEnd.md](docs/2.5%20FrontEnd.md) |
| 微信小程序 | `Front end/listen-miniapp/README.md` |
| Android App | [Front end/wordbook-app/README.md](Front%20end/wordbook-app/README.md)、[BUILD_APK.md](Front%20end/wordbook-app/BUILD_APK.md) |

---

## 🤝 贡献指南

欢迎贡献代码、报告 Bug、提出改进建议。详见 [CONTRIBUTING.md](CONTRIBUTING.md)。

## 📄 许可证

本项目采用 [MIT 许可证](LICENSE) 开源。

## 📚 文档导航

- [项目介绍](docs/1.1%20项目说明.md)
- [DDD 讲解](docs/1.2%20DDD讲解.md)
- [IdentityService 详解](docs/2.1%20IdentityService.md)
- [ListenService 详解](docs/2.2%20ListenService.md)
- [ArticleService 详解](docs/2.3%20ArticleService.md)
- [WordService 详解](docs/2.4%20WordService.md)
- [前端项目详解](docs/2.5%20FrontEnd.md)
- [Commons 公共库总览](docs/3.1%20Commons.md)
- [消息队列学习指南](docs/消息队列学习指南.md)
- [安全策略](SECURITY.md)
- [更新日志](CHANGELOG.md)

---

Built with ❤️ for English learners everywhere