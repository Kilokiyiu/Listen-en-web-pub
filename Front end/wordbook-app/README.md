# 单词本 APP

ListenEase 单词本的独立手机应用。

## 使用流程

1. **打开应用** → 进入登录页
2. **登录账号** → 可使用「云端单词本」和「本地单词本」，随时切换
3. **离线模式** → 无需登录，仅使用「本地单词本」

| 功能 | 登录后 | 离线模式 |
|------|--------|---------|
| 云端单词本 | ✅ 多设备同步 | ❌ |
| 本地单词本 | ✅ | ✅ 可离线 |
| 添加/复习/统计 | ✅ | ✅（仅本地） |

> 云端与本地是两套独立数据，可在单词本页或「我的」中切换。

## 开发

```bash
cd "Front end/wordbook-app"
npm install
npm run dev
```

开发时 API 通过 Vite 代理到本地后端（Identity 5263、Word 5215）。

## 打包 Android 安装包

详细步骤见 **[BUILD_APK.md](./BUILD_APK.md)**。

**快速打包（需已安装 Android Studio）：**

```powershell
cd "Front end/wordbook-app"
npm install
.\scripts\build-apk.ps1
```

生成的 APK：`android/app/build/outputs/apk/debug/app-debug.apk`，传到手机即可安装。

或用 Android Studio 打开：

```bash
npm run cap:android
```

## 项目结构

```
src/
  api/           # 云端 API（认证、单词）
  services/
    localWordStore.js   # 本地存储实现
    serverWordStore.js  # 云端 API 封装
    wordService.js      # 统一入口（按模式分发）
  views/         # 页面：引导、单词列表、添加、复习、设置、登录
  utils/sm2.js   # SM-2 复习算法（与后端一致）
```
