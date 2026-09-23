# EaseWord 发布目录

## 用户下载
- 当前推荐：https://your-domain.com/downloads/EaseWord-0.8.9.2.apk
- 兼容旧链接：https://your-domain.com/downloads/EaseWord-beta.apk
- 版本清单：https://your-domain.com/downloads/version.json

## 发新版流程（应用内更新依赖 version.json）
1. 修改 `wordbook-app/android/app/build.gradle` 的 `versionCode`（必须递增）和 `versionName`
2. 打包 APK 后放到本目录，建议同时写两个文件：
   - `EaseWord-x.y.z.apk`（带版本号，避免 CDN 缓存旧包）
   - `EaseWord-beta.apk`（覆盖，兼容旧链接）
3. 更新 `version.json`：
   - `versionCode` / `versionName` 与 APK 一致
   - `apkUrl` 指向带版本号的新文件
   - `changelog` 写更新说明
4. 更新首页 `HomeView.vue` 下载链接为新文件名
5. 部署用户站前端；若用了 Cloudflare，必要时清除 `/downloads/*` 缓存

## 注意
应用内更新只读 `version.json`。若清单已指向新 APK，但服务器上文件不存在，会报「下载安装包失败」。
