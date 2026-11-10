import { App } from '@capacitor/app'
import { Capacitor, CapacitorHttp } from '@capacitor/core'
import { Directory, Filesystem } from '@capacitor/filesystem'
import { FileOpener } from '@capacitor-community/file-opener'
import { API_BASE } from '../config'

const UPDATE_APK_NAME = 'EaseWord-update.apk'

/** 线上版本清单（与网站 downloads 目录同步） */
export function getVersionManifestUrl() {
  const base = (API_BASE || 'https://your-domain.com').replace(/\/$/, '')
  return `${base}/downloads/version.json`
}

export async function getLocalAppInfo() {
  if (!Capacitor.isNativePlatform()) {
    return { version: '0.9.16', build: '18', id: 'web' }
  }
  const info = await App.getInfo()
  return {
    version: info.version || '0.0.0',
    build: String(info.build || '0'),
    id: info.id || '',
  }
}

export async function fetchRemoteVersion() {
  // params + 防缓存头：避免 Android CapacitorHttp 复用旧 version.json（旧 changelog）
  const res = await CapacitorHttp.get({
    url: getVersionManifestUrl(),
    params: { t: String(Date.now()), v: '2' },
    headers: {
      Accept: 'application/json',
      'Cache-Control': 'no-cache, no-store, must-revalidate',
      Pragma: 'no-cache',
    },
    connectTimeout: 15000,
    readTimeout: 15000,
  })
  if (res.status < 200 || res.status >= 300) {
    throw new Error('无法获取版本信息')
  }
  const data = typeof res.data === 'string' ? JSON.parse(res.data) : res.data
  if (!data?.versionCode || !data?.apkUrl) {
    throw new Error('版本信息格式不正确')
  }
  return {
    versionName: String(data.versionName || data.version || ''),
    versionCode: Number(data.versionCode),
    apkUrl: String(data.apkUrl),
    forceUpdate: !!data.forceUpdate,
    changelog: String(data.changelog || ''),
  }
}

export async function checkForUpdate() {
  const [local, remote] = await Promise.all([getLocalAppInfo(), fetchRemoteVersion()])
  const localCode = Number(local.build) || 0
  const hasUpdate = remote.versionCode > localCode
  return { local, remote, hasUpdate }
}

/**
 * 流式下载到缓存目录，带真实进度（避免 CapacitorHttp 整包进内存导致假进度卡在 5%）
 */
async function downloadApkToCache(apkUrl, onProgress) {
  onProgress?.(1)
  try {
    await Filesystem.deleteFile({
      path: UPDATE_APK_NAME,
      directory: Directory.Cache,
    })
  } catch {
    // 首次下载无旧文件
  }

  let lastPct = 1
  const progressHandle = await Filesystem.addListener('progress', (status) => {
    if (!status?.lengthComputable || !status.contentLength) {
      lastPct = Math.min(90, lastPct + 1)
      onProgress?.(lastPct)
      return
    }
    // 1–95% 留给下载，末尾留给取 uri
    const pct = Math.min(95, Math.max(1, Math.round((status.bytes / status.contentLength) * 94) + 1))
    if (pct > lastPct) {
      lastPct = pct
      onProgress?.(pct)
    }
  })

  try {
    await Filesystem.downloadFile({
      url: apkUrl,
      path: UPDATE_APK_NAME,
      directory: Directory.Cache,
      progress: true,
    })
  } catch (e) {
    throw new Error(e?.message || '下载安装包失败')
  } finally {
    await progressHandle.remove()
  }

  onProgress?.(98)
  const { uri } = await Filesystem.getUri({
    path: UPDATE_APK_NAME,
    directory: Directory.Cache,
  })
  onProgress?.(100)
  return uri
}

export async function downloadAndInstallUpdate(remote, onProgress) {
  if (!Capacitor.isNativePlatform()) {
    window.open(remote.apkUrl, '_blank')
    return { mode: 'browser' }
  }

  const uri = await downloadApkToCache(remote.apkUrl, onProgress)
  await FileOpener.open({
    filePath: uri,
    contentType: 'application/vnd.android.package-archive',
    openWithDefault: true,
  })
  return { mode: 'install', uri }
}

/** 每次启动检测新版本（仅原生 App；失败静默忽略） */
export async function checkUpdateOnLaunch() {
  if (!Capacitor.isNativePlatform()) return null
  try {
    return await checkForUpdate()
  } catch {
    return null
  }
}

/** @deprecated 保留兼容；请改用 checkUpdateOnLaunch */
export async function maybeSilentCheckUpdate() {
  return checkUpdateOnLaunch()
}
