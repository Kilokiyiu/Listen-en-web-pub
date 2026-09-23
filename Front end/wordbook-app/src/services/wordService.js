import { getActiveWordbook, getCurrentWordBookId } from './appSettings'
import * as local from './localWordStore'
import * as server from './serverWordStore'

async function store() {
  return (await getActiveWordbook()) === 'local' ? local : server
}

async function isCloud() {
  return (await getActiveWordbook()) !== 'local'
}

/** 云端请求自动附带当前单词本 id */
async function withBookParams(params = {}) {
  if (!(await isCloud())) return params || {}
  const wordBookId = await getCurrentWordBookId()
  if (!wordBookId) return params || {}
  return { ...(params || {}), wordBookId }
}

async function withBookData(data = {}) {
  if (!(await isCloud())) return data || {}
  const wordBookId = await getCurrentWordBookId()
  if (!wordBookId) return data || {}
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

export async function getDueWords(params) {
  return (await store()).getDueWords(await withBookParams(params))
}

export async function getRandomWords(params) {
  return (await store()).getRandomWords(await withBookParams(params))
}

export async function reviewWord(id, quality) {
  return (await store()).reviewWord(id, quality)
}

export async function getStats(params) {
  const s = await store()
  if (s === local) return s.getStats()
  return s.getStats(await withBookParams(params))
}

export async function getStorageLabel() {
  return (await getActiveWordbook()) === 'local' ? '本地单词本' : '云端单词本'
}
