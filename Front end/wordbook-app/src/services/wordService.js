import { getActiveWordbook, getCurrentWordBookId } from './appSettings'
import { isFavoritesBookId } from '../config'
import * as local from './localWordStore'
import * as server from './serverWordStore'

async function store() {
  return (await getActiveWordbook()) === 'local' ? local : server
}

async function isCloud() {
  return (await getActiveWordbook()) !== 'local'
}

function asFavoritesParams(params = {}) {
  const next = { ...(params || {}) }
  delete next.wordBookId
  next.favoritesOnly = true
  return next
}

/** 云端附带 Preferences 中的词本 id；本地由 localWordStore 读 currentBookId */
async function withBookParams(params = {}) {
  const p = { ...(params || {}) }
  if (p.favoritesOnly || isFavoritesBookId(p.wordBookId)) {
    return asFavoritesParams(p)
  }
  if (!(await isCloud())) {
    // 本地：若当前选中收藏本，也走 favoritesOnly
    const data = await local.getLocalBooks()
    if (isFavoritesBookId(data.currentId) && p.wordBookId == null) {
      return asFavoritesParams(p)
    }
    return p
  }
  const wordBookId = await getCurrentWordBookId()
  if (isFavoritesBookId(wordBookId) && p.wordBookId == null) {
    return asFavoritesParams(p)
  }
  if (!wordBookId) return p
  return { ...p, wordBookId }
}

async function withBookData(data = {}) {
  if (!(await isCloud())) return data || {}
  const wordBookId = await getCurrentWordBookId()
  if (!wordBookId || isFavoritesBookId(wordBookId)) return data || {}
  return { ...(data || {}), wordBookId }
}

export async function getWords(params) {
  return (await store()).getWords(await withBookParams(params))
}

export async function addWord(data) {
  return (await store()).addWord(await withBookData(data))
}

export async function deleteWord(id) {
  return (await store()).deleteWord(id)
}

export async function deleteWords(ids = []) {
  return (await store()).deleteWords(ids)
}

export async function getDueWords(params) {
  return (await store()).getDueWords(await withBookParams(params))
}

export async function getRandomWords(params) {
  return (await store()).getRandomWords(await withBookParams(params))
}

export async function reviewWord(id, quality) {
  return (await store()).reviewWord(id, quality)
}

export async function toggleFavorite(id, isFavorite) {
  return (await store()).toggleFavorite(id, isFavorite)
}

export async function getStats(params) {
  return (await store()).getStats(await withBookParams(params))
}

export async function getStorageLabel() {
  return (await getActiveWordbook()) === 'local' ? '本地单词本' : '云端单词本'
}
