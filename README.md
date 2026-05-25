

# ListenEase

*让每一次聆听，都成为进步的阶梯*

[🌐 在线体验](https://listenease.online)

---

## 📖 项目简介

ListenEase 是一个专注于英语听力学习的开源平台，目前融合了**听力考试训练**、**每日外刊阅读**和**词根学习**三大模块，帮助学习者在碎片化时间内高效提升英语能力。后续会上线更多的学习功能。

## ✨ 核心功能


| 模块           | 功能描述                                         |
| ------------ | -------------------------------------------- |
| 🎧 **听力训练**  | 按分类浏览听力试卷，逐题播放音频并显示同步字幕，支持 CET-4/CET-6 等考试练习 |
| 📰 **外刊阅读**  | 每日推送中英双语文章，支持已读标记和收藏，附带音频朗读                  |
| 📝 **词根学习**  | 基于词根词缀法的系统化单词学习，包含示例词、释义和在线测验                |
| 👤 **用户系统**  | 邮箱注册登录，JWT 认证，学习进度追踪                         |
| 🖥️ **后台管理** | 试卷/文章/词根内容的增删改查，音频文件上传管理                     |


## 🏗️ 技术架构

```
┌───────────────────────────────────────────────────┐
│                    Nginx (HTTPS)                   │
│         静态资源服务 + API 反向代理 + 负载均衡        │
└───────┬───────┬────────┬────────┬─────────────────┘
        │       │        │        │
   ┌────▼──┐ ┌─▼───┐ ┌──▼───┐ ┌─▼─────┐
   │Listen │ │Ident│ │Artic │ │Word   │
   │Service│ │ity  │ │le    │ │Service│
   │ :8080 │ │:8080│ │:8080 │ │ :8080 │
   └───┬───┘ └──┬──┘ └──┬───┘ └──┬────┘
       │        │       │        │
       └────────┴───────┴────────┘
                    │
              ┌─────▼─────┐
              │ SQL Server │
              │   2022     │
              └───────────┘

   Certbot ─── Let's Encrypt 自动证书续期
```

### 后端

- **框架**: .NET 10 / ASP.NET Core
- **ORM**: Entity Framework Core 10
- **数据库**: SQL Server 2022（每个微服务独立 Database）
- **认证**: ASP.NET Core Identity + JWT
- **文档**: Swagger / OpenAPI

### 前端


| 项目                         | 技术栈                                                 |
| -------------------------- | --------------------------------------------------- |
| 用户端 (`en-listen-user-ui`)  | Vue 3 + Element Plus + Vue Router + Vue CLI + Axios |
| 管理端 (`en-listen-admin-ui`) | Vue 3 + Element Plus + Vue Router + Vite + Axios    |


### 基础设施

- **容器化**: Docker + Docker Compose
- **反向代理**: Nginx（HTTP/2 + TLS 1.3 + gzip）
- **HTTPS 证书**: Let's Encrypt / Certbot 自动续期
- **CI/CD**: GitHub Actions

## 📁 项目结构

```
Listen-en-web/
├── Commons/                     # 公共类库（DDD 接口、EF Core 扩展、JWT、事件总线等）
├── IdentitySerivce/             # 用户认证微服务
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
├── WordService/                 # 词根学习微服务
│   ├── WordService.Domain/
│   ├── WordService.Infrastrucure/
│   └── WordService.WebAPI/
├── FileService/                 # 文件管理微服务
├── Front end/
│   ├── en-listen-user-ui/       # 用户端前端
│   └── en-listen-admin-ui/      # 管理端前端
├── deploy/                      # 部署配置
│   ├── docker-compose.yml
│   ├── nginx/nginx.conf
│   ├── *.WebAPI/Dockerfile
│   └── audios/                  # 音频文件目录
└── .github/workflows/           # CI/CD 配置
```

## 🚀 快速开始

### 前置要求

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js](https://nodejs.org/) 18+
- [Docker](https://www.docker.com/) & Docker Compose
- [SQL Server](https://www.microsoft.com/sql-server)（Docker 部署会自动拉取）

### 本地开发

#### 1. 启动数据库

```bash
# 使用 Docker 启动 SQL Server
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrongPassword123!" \
  -p 1433:1433 --name sqlserver \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

#### 2. 配置环境变量

复制并编辑环境配置：

```bash
cp .env.example .env
# 修改 SA_PASSWORD 和 JWT_KEY
```

修改各服务的 `appsettings.json` 中的数据库连接字符串。

#### 3. 启动后端服务

```bash
# 还原依赖并运行（以 ListenService 为例）
cd ListenService/ListenService.WebAPI
dotnet run
```

每个服务默认监听 `http://localhost:8080`。

#### 4. 启动前端

```bash
# 用户端
cd "Front end/en-listen-user-ui"
npm install
npm run serve

# 管理端
cd "Front end/en-listen-admin-ui"
npm install
npm run dev
```

### Docker 一键部署

```bash
# 1. 配置环境变量
cp deploy/.env.example deploy/.env
# 编辑 deploy/.env：设置 SA_PASSWORD、JWT_KEY、SERVER_IP

# 2. 构建前端
cd "Front end/en-listen-user-ui" && npm run build
cd "Front end/en-listen-admin-ui" && npm run build

# 3. 将构建产物复制到 deploy 目录
cp -r "Front end/en-listen-user-ui/dist/*" deploy/user-ui/
cp -r "Front end/en-listen-admin-ui/dist/*" deploy/admin-ui/

# 4. 一键启动
cd deploy
docker compose up -d
```

部署完成后访问 `https://your-server-ip` 即可。

> 💡 首次部署后，访问 `POST /api/identity/Login/CreateWorld` 创建初始管理员账号。

## 🔐 安全说明

- 所有敏感配置通过环境变量注入，不硬编码
- `.env` 文件已加入 `.gitignore`，仅保留 `.env.example` 模板
- JWT Key 建议使用 `openssl rand -base64 32` 生成
- 生产环境务必修改默认管理员密码

## 📄 License

本项目采用 MIT 许可证开源。
如果你想基于本项目实现自己的二次开发，请务必阅读许可证。另外，我们还准备的有关的文档对各个服务进行详细的说明，方便您快速了解项目：[点击查看项目文档](../1.1项目说明.md)

---

Built with ❤️ for English learners everywhere