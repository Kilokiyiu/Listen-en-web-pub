import { Preferences } from '@capacitor/preferences'
import { STORAGE_KEYS } from '../config'
import { updateReview } from '../utils/sm2'

function newId() {
  return crypto.randomUUID()
}

async function loadData() {
  const { value } = await Preferences.get({ key: STORAGE_KEYS.localData })
  if (!value) return { words: [], reviewLogsCount: 0 }
  try {
    return JSON.parse(value)
  } catch {
    return { words: [], reviewLogsCount: 0 }
  }
}

async function saveData(data) {
  await Preferences.set({ key: STORAGE_KEYS.localData, value: JSON.stringify(data) })
}

export async function getWords({ page = 1, pageSize = 20, search = '' } = {}) {
  const data = await loadData()
  let words = [...data.words].sort(
    (a, b) => new Date(b.creationTime) - new Date(a.creationTime)
  )
  if (search) {
    const kw = search.toLowerCase()
    words = words.filter((w) => w.word.toLowerCase().includes(kw))
  }
  const total = words.length
  const items = words.slice((page - 1) * pageSize, page * pageSize)
  return { items, total, page, pageSize }
}

export async function addWord({ word, definition, example }) {
  const data = await loadData()
  const exists = data.words.some((w) => w.word.toLowerCase() === word.toLowerCase())
  if (exists) throw new Error('该单词已存在')

  const item = {
    id: newId(),
    word: word.trim(),
    definition: definition?.trim() || '',
    example: example?.trim() || '',
    repetitionCount: 0,
    easeFactor: 2.5,
    interval: 0,
    nextReview: null,
    creationTime: new Date().toISOString(),
  }
  data.words.push(item)
  await saveData(data)
  return item
}

export async function deleteWord(id) {
  const data = await loadData()
  const idx = data.words.findIndex((w) => w.id === id)
  if (idx === -1) throw new Error('单词不存在')
  data.words.splice(idx, 1)
  await saveData(data)
}

export async function getDueWords({ limit = 50 } = {}) {
  const data = await loadData()
  const now = new Date()
  return data.words
    .filter((w) => !w.nextReview || new Date(w.nextReview) <= now)
    .sort((a, b) => {
      if (!a.nextReview) return -1
      if (!b.nextReview) return 1
      return new Date(a.nextReview) - new Date(b.nextReview)
    })
    .slice(0, limit)
}

export async function getRandomWords({ limit = 50 } = {}) {
  const data = await loadData()
  const pool = data.words.filter((w) => w.interval < 21)
  return pool.sort(() => Math.random() - 0.5).slice(0, limit)
}

export async function reviewWord(id, quality) {
  const data = await loadData()
  const idx = data.words.findIndex((w) => w.id === id)
  if (idx === -1) throw new Error('单词不存在')

  data.words[idx] = updateReview(data.words[idx], quality)
  data.reviewLogsCount = (data.reviewLogsCount || 0) + 1
  await saveData(data)
  return data.words[idx]
}

export async function getStats() {
  const data = await loadData()
  const now = new Date()
  const dueCount = data.words.filter(
    (w) => !w.nextReview || new Date(w.nextReview) <= now
  ).length
  const masteredCount = data.words.filter((w) => w.interval >= 21).length
  return {
    totalWords: data.words.length,
    dueCount,
    masteredCount,
    reviewLogsCount: data.reviewLogsCount || 0,
  }
}

export async function getAllWords() {
  const data = await loadData()
  return [...data.words]
}

/**
 * 批量导入单词（按单词文本合并，不覆盖已有条目的复习进度）
 * @param {Array} words
 * @returns {{ added: number, skipped: number, updated: number }}
 */
export async function importWords(words = []) {
  const data = await loadData()
  const byKey = new Map(data.words.map((w) => [w.word.toLowerCase(), w]))
  let added = 0
  let skipped = 0
  let updated = 0

  for (const raw of words) {
    const word = (raw.word || '').trim()
    if (!word) continue

    const key = word.toLowerCase()
    const existing = byKey.get(key)
    const definition = raw.definition?.trim?.() || raw.definition || ''
    const example = raw.example?.trim?.() || raw.example || ''

    if (existing) {
      let changed = false
      if (!existing.definition && definition) {
        existing.definition = definition
        changed = true
      }
      if (!existing.example && example) {
        existing.example = example
        changed = true
      }
      if (changed) updated += 1
      else skipped += 1
      continue
    }

    const item = {
      id: newId(),
      word,
      definition,
      example,
      repetitionCount: Number(raw.repetitionCount) || 0,
      easeFactor: Number(raw.easeFactor) || 2.5,
      interval: Number(raw.interval) || 0,
      nextReview: raw.nextReview || null,
      creationTime: raw.creationTime || new Date().toISOString(),
    }
    data.words.push(item)
    byKey.set(key, item)
    added += 1
  }

  await saveData(data)
  return { added, skipped, updated }
}

export async function clearAllData() {
  await Preferences.remove({ key: STORAGE_KEYS.localData })
}
