# Security Policy

## 支持的版本

| 版本 | 支持状态 |
|------|----------|
| 最新 (`main`) | ✅ |
| 其余历史版本 | ❌ |

## 报告漏洞

如果你发现了潜在的安全漏洞，**请不要通过公开 Issue 报告**。

请通过以下方式私下联系：
- 📧 邮箱：[security@your-domain.com](mailto:security@your-domain.com)（公开后请替换为真实邮箱）
- 或在 GitHub 上使用 [Security Advisories](https://github.com/owner/repo/security/advisories/new)

我们会在 48 小时内确认收到，14 天内给出修复计划。

---

## 安全设计

### 1. 敏感配置管理

**所有敏感配置必须通过环境变量注入**，禁止硬编码到代码中：

| 配置项 | 环境变量 | 必填 |
|--------|----------|------|
| SQL Server SA 密码 | `SA_PASSWORD` | ✅ |
| JWT 签名密钥 | `JWT_KEY` | ✅ |
| RabbitMQ 用户名 | `RABBITMQ_USER` | ⚠️ 生产环境必须 |
| RabbitMQ 密码 | `RABBITMQ_PASSWORD` | ⚠️ 生产环境必须 |
| SMTP 主机 | `SMTP_HOST` | ⚠️ 启用邮件时必填 |
| SMTP 用户名/密码 | `SMTP_USERNAME` / `SMTP_PASSWORD` | ⚠️ 启用邮件时必填 |
| 反馈接收邮箱 | `FEEDBACK_RECIPIENT_EMAIL` | ⚠️ 启用反馈时必填 |
| SendCloud API User/Key | `SENDCLOUD_API_USER` / `SENDCLOUD_API_KEY` | 可选 |
| xxapi.cn API Key | `XXAPI_API_KEY` | 可选 |

### 2. JWT 强制保护

`Commons/MyJWT/WebApplicationBuilderExtensions.cs` 实现了**启动失败保护**：

```csharp
if (string.IsNullOrEmpty(jwtOptions.Key))
{
    throw new InvalidOperationException(
        "JWT Key is not configured. Set the 'JWT:Key' in appsettings.json or the 'JWT_KEY' environment variable."
    );
}
```

未配置 JWT Key 时服务**拒绝启动**，杜绝弱密钥带来的风险。

### 3. 已忽略的文件类型

`.gitignore` 已排除以下文件类型，**不会入库**：

- `.env`、`.env.*`（保留 `.env.example` 模板）
- `appsettings.Development.json` / `.Production.json` / `.Secrets.json`
- `**/bin/`、`**/obj/`、`.dll`、`.pdb`、`.exe`
- `node_modules/`、构建产物（`dist/`、`build/`）
- 媒体文件（`*.mp3`、`*.mp4`、`*.apk`）
- PowerShell/SQL 调试脚本（`*.ps1`、`check_*.sql` 等）
- 个人笔记（求职准备文档、HR 沟通记录）

### 4. 密码策略

当前实现（`IdentityService.WebAPI/Program.cs`）：

- 最低长度：6 位
- **建议生产环境调整**：至少 8 位，强制包含大小写字母 + 数字 + 特殊字符

### 5. RabbitMQ 安全

- 默认 `guest/guest` **仅用于本地开发**
- 生产环境必须通过 `RABBITMQ_USER` / `RABBITMQ_PASSWORD` 注入独立账号
- 生产 RabbitMQ 默认禁止 `guest` 远程连接

### 6. HTTPS / CORS

- 生产环境**必须启用 HTTPS**（Let's Encrypt 自动证书）
- CORS 仅允许显式声明的源（`CorsOrigins`）
- 开发环境允许 `http://localhost:8080` / `http://localhost:5173`

### 7. 重置密码流程改进建议

**当前实现（仅个人项目可接受）**：
- `ResetPwdEvent` 通过 RabbitMQ 以**明文**传递新密码
- 邮件内容包含明文新密码

**生产环境建议改造**：
1. 生成带时效（15 分钟）的随机重置链接（JWT 签名）
2. 用户点击链接后**自行设置**新密码
3. 邮件中**不出现明文密码**

### 8. 已知安全债务（计划修复）

| 项 | 风险等级 | 计划 |
|----|----------|------|
| 重置密码明文传递 | 中 | v1.1 重构为重置链接 |
| 邮件日志记录完整 SendCloud 响应 | 低 | 生产环境关闭 Information 级日志 |
| 密码策略偏弱 | 低 | v1.1 提升至 8 位 + 复杂度 |

---

## 部署安全清单

部署到生产环境前，请确认：

- [ ] `SA_PASSWORD` 已设置为强密码（≥12 位、含大小写+数字+特殊字符）
- [ ] `JWT_KEY` 已用 `openssl rand -base64 32` 生成
- [ ] `RABBITMQ_USER` / `RABBITMQ_PASSWORD` 已替换为生产专用账号
- [ ] `FEEDBACK_RECIPIENT_EMAIL` / `SMTP_*` 配置正确
- [ ] `CORS_ORIGIN_0/1` 已设置为真实域名
- [ ] `nginx.conf` 中 `your-domain.com` 已替换
- [ ] 启用 HTTPS 且 Let's Encrypt 证书正常续期
- [ ] 默认管理员账号密码已修改
- [ ] 数据库定期备份策略已就位
- [ ] `.env` 文件**未**提交到仓库

## 依赖安全

建议定期运行依赖检查：

```bash
# .NET 依赖漏洞扫描
dotnet list package --vulnerable --include-transitive

# 前端依赖审计
cd "Front end/en-listen-user-ui" && npm audit
cd "Front end/en-listen-admin-ui" && npm audit
cd "Front end/wordbook-app" && npm audit
```