import { Preferences } from '@capacitor/preferences'
import { STORAGE_KEYS, WORDBOOK_TYPES } from '../config'

async function get(key) {
  const { value } = await Preferences.get({ key })
  return value
}

async function set(key, value) {
  await Preferences.set({ key, value })
}

async function remove(key) {
  await Preferences.remove({ key })
}

/** 当前查看的单词本：local | server */
export async function getActiveWordbook() {
  return (await get(STORAGE_KEYS.activeWordbook)) || WORDBOOK_TYPES.local
}

export async function setActiveWordbook(type) {
  await set(STORAGE_KEYS.activeWordbook, type)
}

export async function isLocalWordbook() {
  return (await getActiveWordbook()) === WORDBOOK_TYPES.local
}

export async function isServerWordbook() {
  return (await getActiveWordbook()) === WORDBOOK_TYPES.server
}

/** 是否处于纯离线模式（未登录，仅本地单词本） */
export async function isOfflineOnly() {
  return (await get(STORAGE_KEYS.offlineOnly)) === 'true'
}

export async function setOfflineOnly(value) {
  if (value) {
    await set(STORAGE_KEYS.offlineOnly, 'true')
  } else {
    await remove(STORAGE_KEYS.offlineOnly)
  }
}

export async function enterOfflineMode() {
  await setOfflineOnly(true)
  await setActiveWordbook(WORDBOOK_TYPES.local)
}

export async function onLoginSuccess() {
  await setOfflineOnly(false)
  await setActiveWordbook(WORDBOOK_TYPES.server)
}

export async function getAuth() {
  const [token, userId, username] = await Promise.all([
    get(STORAGE_KEYS.token),
    get(STORAGE_KEYS.userId),
    get(STORAGE_KEYS.username),
  ])
  return { token, userId, username }
}

export async function setAuth({ token, userId, username }) {
  await Promise.all([
    set(STORAGE_KEYS.token, token),
    set(STORAGE_KEYS.userId, userId),
    set(STORAGE_KEYS.username, username),
  ])
}

export async function clearAuth() {
  await Promise.all([
    remove(STORAGE_KEYS.token),
    remove(STORAGE_KEYS.userId),
    remove(STORAGE_KEYS.username),
  ])
}

export async function isLoggedIn() {
  const { token, userId } = await getAuth()
  return !!(token && userId)
}

export async function canUseServerWordbook() {
  return (await isLoggedIn()) && !(await isOfflineOnly())
}

/** 当前选中的云端单词本 id */
export async function getCurrentWordBookId() {
  return (await get(STORAGE_KEYS.currentWordBookId)) || null
}

export async function setCurrentWordBookId(id) {
  if (id) {
    await set(STORAGE_KEYS.currentWordBookId, String(id))
  } else {
    await remove(STORAGE_KEYS.currentWordBookId)
  }
}
