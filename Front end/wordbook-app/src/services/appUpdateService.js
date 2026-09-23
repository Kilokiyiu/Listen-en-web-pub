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
    return { version: '0.8.9.2', build: '11', id: 'web' }
  }
  const info = await App.getInfo()
  return {
    version: info.version || '0.0.0',
    build: String(info.build || '0'),
    id: info.id || '',
  }
}

export async function fetchRemoteVersion() {
  const url = `${getVersionManifestUrl()}?t=${Date.now()}`
  const res = await CapacitorHttp.get({
    url,
    headers: { Accept: 'application/json' },
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

async function downloadApkToCache(apkUrl, onProgress) {
  onProgress?.(5)
  const res = await CapacitorHttp.get({
    url: apkUrl,
    responseType: 'blob',
    connectTimeout: 30000,
    readTimeout: 300000,
  })
  if (res.status < 200 || res.status >= 300) {
    throw new Error('下载安装包失败')
  }
  onProgress?.(70)

  const raw = res.data
  const base64 = typeof raw === 'string' ? raw.replace(/^data:[^;]+;base64,/, '') : null
  if (!base64) throw new Error('安装包数据无效')

  await Filesystem.writeFile({
    path: UPDATE_APK_NAME,
    data: base64,
    directory: Directory.Cache,
  })
  onProgress?.(90)

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
