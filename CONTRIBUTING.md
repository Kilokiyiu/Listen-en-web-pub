# 贡献指南 (CONTRIBUTING)

感谢你对 ListenEase 项目的关注！我们欢迎所有形式的贡献。

---

## 行为准则

- 友善、包容、专业
- 建设性反馈，避免人身攻击
- 关注问题本身，不针对个人

## 如何贡献

### 报告 Bug

1. 先在 [Issues](https://github.com/owner/repo/issues) 中搜索，确认未被报告
2. 使用 **Bug Report** 模板新建 Issue
3. 提供：复现步骤、期望行为、实际行为、截图/日志、环境信息

### 提出新功能

1. 在 Issue 中使用 **Feature Request** 模板
2. 说明：
   - 要解决的问题
   - 期望的解决方案
   - 替代方案
   - 影响范围（哪些服务/前端会涉及）

### 提交代码

1. **Fork** 本仓库
2. 从 `main` 创建分支：`git checkout -b feature/your-feature` 或 `fix/issue-number`
3. 提交改动，commit 信息清晰（参考下面的规范）
4. 推送分支并创建 **Pull Request**

---

## 开发规范

### 代码风格

| 模块 | 规范 |
|------|------|
| C# (.NET) | [Microsoft C# Coding Conventions](https://learn.microsoft.com/zh-cn/dotnet/csharp/fundamentals/coding-style/coding-conventions)、Google Style 兼容 |
| Vue / JS | [Vue 3 Style Guide](https://cn.vuejs.org/style-guide/) + ESLint（仓库自带） |
| 命名 | 优先中英结合的清晰命名（项目内已混合使用） |

### 提交规范

使用 [Conventional Commits](https://www.conventionalcommits.org/zh-hans/)：

```
feat: 新增用户反馈功能
fix: 修复 JWT 空值启动崩溃
docs: 更新 README 部署章节
refactor: 重构 WordService 数据访问层
test: 补充 UserWordBook 单元测试
chore: 升级 NuGet 依赖
```

### 分支命名

- `feature/xxx` — 新功能
- `fix/xxx` — Bug 修复
- `docs/xxx` — 文档
- `refactor/xxx` — 重构
- `chore/xxx` — 杂项（依赖、配置等）

---

## 敏感信息保护

提交代码前请确认：

- [ ] **未硬编码**任何密钥、密码、邮箱、Token
- [ ] **未提交** `.env` / `appsettings.Development.json`
- [ ] **未包含**个人邮箱、电话、姓名等 PII
- [ ] **未出现**真实生产域名（如有请替换为 `your-domain.com`）
- [ ] **未误提交** `*.dll`、`.pdb`、`*.apk`、`.mp3` 等二进制文件

如不确定，请参考 [SECURITY.md](SECURITY.md)。

---

## 测试

提交前请确保：

- 后端：`dotnet build` 通过，无新增警告
- 前端：相关项目 `npm run build` 通过
- 手动验证：起本地环境跑通核心流程（登录、听一题、加一个词）

## 文档

- 修改代码时**同步更新**对应文档
- 新增功能请在 `CHANGELOG.md` 的 `[Unreleased]` 段登记
- 新增公共配置请在 `docs/` 中补充说明

---

## 仓库结构说明

| 路径 | 维护者 |
|------|--------|
| `*/Domain/*` | 领域模型，少改 | 
| `*/Infrastructure/*` | 仓储、迁移、外部集成 |
| `*/WebAPI/*` | API 控制器、DTO |
| `Commons/*` | 公共类库，谨慎改动（影响全部服务） |
| `Front end/*/src/` | 前端业务代码 |
| `deploy/*` | 部署配置、Dockerfile |
| `docs/*` | 项目文档 |

---

## 联系方式

- **项目仓库**：[GitHub Repo](#)（公开后填入）
- **Issues**：用于 Bug 报告、功能请求、问题交流
- **Discussions**：用于一般性讨论、问题求助

---

再次感谢你的贡献！