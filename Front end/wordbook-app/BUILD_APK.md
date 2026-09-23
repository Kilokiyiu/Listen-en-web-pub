# 打包 Android 安装包 (APK)

## 一、环境准备（只需做一次）

1. 安装 [Android Studio](https://developer.android.com/studio)
2. 打开 Android Studio → **More Actions** → **SDK Manager**，确认已安装：
   - Android SDK Platform 36
   - Android SDK Build-Tools
3. 配置环境变量（Windows）：
   - `ANDROID_HOME` = `C:\Users\你的用户名\AppData\Local\Android\Sdk`
   - 将 `%ANDROID_HOME%\platform-tools` 加入 `Path`

4. 配置 SDK 路径（若 Gradle 找不到 SDK）：

```powershell
cd "Front end\wordbook-app\android"
copy local.properties.example local.properties
# 编辑 local.properties，填写你的 sdk.dir
```

5. 配置生产 API（打包前确认）：

```powershell
cd "Front end\wordbook-app"
copy .env.example .env
# .env 中 VITE_API_BASE=https://your-domain.com
```

6. 配置**固定签名**（应用内更新必须，只做一次）：

```powershell
cd "Front end\wordbook-app\android"
# 若尚未生成密钥：用 JDK keytool 生成 wordbook-release.keystore
# 再复制 keystore.properties.example 为 keystore.properties 并填写密码
copy keystore.properties.example keystore.properties
```

> `wordbook-release.keystore` 与 `keystore.properties` 已加入 `.gitignore`，请自行备份到安全位置。丢失后无法覆盖安装旧用户包。

## 二、打包调试版 APK（可直接安装测试）

```powershell
cd "Front end\wordbook-app"
npm install
npm run build:apk
```

成功后 APK 位置：

```
Front end/wordbook-app/android/app/build/outputs/apk/debug/app-debug.apk
```

将 `app-debug.apk` 传到手机安装即可（需允许「未知来源」）。

## 三、用 Android Studio 打包（推荐）

```powershell
cd "Front end\wordbook-app"
npm run cap:android
```

在 Android Studio 中：
- **调试包**：菜单 **Build → Build Bundle(s) / APK(s) → Build APK(s)**
- **发布包**：**Build → Generate Signed Bundle / APK**（需创建签名密钥）

## 四、发布版 APK（上架应用商店）

1. 生成签名密钥：

```powershell
keytool -genkey -v -keystore wordbook-release.keystore -alias wordbook -keyalg RSA -keysize 2048 -validity 10000
```

2. 在 `android/app/build.gradle` 的 `release` 中配置 `signingConfig`（或使用 Android Studio 向导）

3. 执行：

```powershell
npm run build:apk:release
```

输出：`android/app/build/outputs/apk/release/app-release.apk`

## 常见问题

| 问题 | 解决 |
|------|------|
| SDK location not found | 创建 `android/local.properties` 并设置 `sdk.dir` |
| Gradle 下载失败 | 检查网络/代理，或在 Android Studio 内首次打开项目让其下载 |
| 手机无法安装 | 使用 debug APK，并在设置中允许安装未知应用 |
| 登录/云端失败 | 确认 `.env` 中 `VITE_API_BASE` 指向正确域名 |
