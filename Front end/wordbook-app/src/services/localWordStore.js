import { Preferences } from '@capacitor/preferences'
import { STORAGE_KEYS, FAVORITES_BOOK_ID, isFavoritesBookId } from '../config'
import { updateReview } from '../utils/sm2'

const DEFAULT_BOOK_NAME = '默认单词本'

function newId() {
  return crypto.randomUUID()
}

function emptyState() {
  const id = newId()
  return {
    version: 2,
    currentBookId: id,
    books: [
      {
        id,
        name: DEFAULT_BOOK_NAME,
        description: '',
        isDefault: true,
        creationTime: new Date().toISOString(),
        reviewLogsCount: 0,
      },
    ],
    words: [],
  }
}

/** 旧版 { words, reviewLogsCount } → 多词本结构 */
function migrateIfNeeded(raw) {
  if (!raw || typeof raw !== 'object') return emptyState()
  if (raw.version === 2 && Array.isArray(raw.books) && Array.isArray(raw.words)) {
    if (!raw.books.length) return emptyState()
    if (isFavoritesBookId(raw.currentBookId)) {
      return { ...raw, currentBookId: FAVORITES_BOOK_ID }
    }
    const current =
      raw.books.find((b) => b.id === raw.currentBookId) ||
      raw.books.find((b) => b.isDefault) ||
      raw.books[0]
    return { ...raw, currentBookId: current.id }
  }

  const bookId = newId()
  const words = Array.isArray(raw.words)
    ? raw.words.map((w) => ({ ...w, wordBookId: w.wordBookId || bookId }))
    : []
  return {
    version: 2,
    currentBookId: bookId,
    books: [
      {
        id: bookId,
        name: DEFAULT_BOOK_NAME,
        description: '',
        isDefault: true,
        creationTime: new Date().toISOString(),
        reviewLogsCount: Number(raw.reviewLogsCount) || 0,
      },
    ],
    words,
  }
}

async function loadData() {
  const { value } = await Preferences.get({ key: STORAGE_KEYS.localData })
  if (!value) return emptyState()
  try {
    return migrateIfNeeded(JSON.parse(value))
  } catch {
    return emptyState()
  }
}

async function saveData(data) {
  await Preferences.set({
    key: STORAGE_KEYS.localData,
    value: JSON.stringify({ ...data, version: 2 }),
  })
}

function bookWords(data, bookId = data.currentBookId) {
  return (data.words || []).filter((w) => w.wordBookId === bookId)
}

function findBook(data, id) {
  return (data.books || []).find((b) => b.id === id) || null
}

function bumpBookWordCount(books, words) {
  return books.map((b) => ({
    ...b,
    wordCount: words.filter((w) => w.wordBookId === b.id).length,
  }))
}

export async function getLocalBooks() {
  const data = await loadData()
  const books = bumpBookWordCount(data.books, data.words)
  await saveData({ ...data, books })
  const current = isFavoritesBookId(data.currentBookId)
    ? { id: FAVORITES_BOOK_ID }
    : books.find((b) => b.id === data.currentBookId) || books[0] || null
  return {
    books,
    currentId: data.currentBookId,
    current,
    favoriteCount: data.words.filter((w) => w.isFavorite).length,
  }
}

export async function setCurrentLocalBookId(id) {
  const data = await loadData()
  if (!isFavoritesBookId(id) && !findBook(data, id)) throw new Error('单词本不存在')
  data.currentBookId = id
  await saveData(data)
  return id
}

function resolveWordPool(data, { wordBookId, favoritesOnly } = {}) {
  const bookId = wordBookId || data.currentBookId
  if (favoritesOnly || isFavoritesBookId(bookId)) {
    return data.words.filter((w) => w.isFavorite)
  }
  return bookWords(data, bookId)
}

export async function createLocalBook({ name, description = '' } = {}) {
  const title = String(name || '').trim()
  if (!title) throw new Error('请输入单词本名称')
  const data = await loadData()
  const book = {
    id: newId(),
    name: title,
    description: String(description || '').trim(),
    isDefault: false,
    creationTime: new Date().toISOString(),
    reviewLogsCount: 0,
    wordCount: 0,
  }
  data.books.push(book)
  data.currentBookId = book.id
  await saveData(data)
  return book
}

export async function renameLocalBook(id, { name, description } = {}) {
  const data = await loadData()
  const book = findBook(data, id)
  if (!book) throw new Error('单词本不存在')
  if (name != null) {
    const title = String(name).trim()
    if (!title) throw new Error('请输入单词本名称')
    book.name = title
  }
  if (description != null) book.description = String(description).trim()
  await saveData(data)
  return { ...book, wordCount: bookWords(data, id).length }
}

export async function deleteLocalBook(id, { deleteWords = true } = {}) {
  const data = await loadData()
  const book = findBook(data, id)
  if (!book) throw new Error('单词本不存在')

  if (book.isDefault) {
    data.words = data.words.filter((w) => w.wordBookId !== id)
    data.books = data.books.filter((b) => b.id !== id)
    const freshId = newId()
    data.books.forEach((b) => {
      b.isDefault = false
    })
    data.books.unshift({
      id: freshId,
      name: DEFAULT_BOOK_NAME,
      description: '',
      isDefault: true,
      creationTime: new Date().toISOString(),
      reviewLogsCount: 0,
    })
    data.currentBookId = freshId
    await saveData(data)
    return { recreatedDefault: true }
  }

  const defaultBook =
    data.books.find((b) => b.isDefault) ||
    data.books.find((b) => b.id !== id) ||
    null

  if (deleteWords) {
    data.words = data.words.filter((w) => w.wordBookId !== id)
  } else if (defaultBook) {
    data.words.forEach((w) => {
      if (w.wordBookId === id) w.wordBookId = defaultBook.id
    })
  } else {
    data.words = data.words.filter((w) => w.wordBookId !== id)
  }

  data.books = data.books.filter((b) => b.id !== id)
  if (!data.books.length) {
    const state = emptyState()
    await saveData(state)
    return { recreatedDefault: true }
  }
  if (data.currentBookId === id) {
    data.currentBookId =
      data.books.find((b) => b.isDefault)?.id || data.books[0].id
  }
  await saveData(data)
  return { recreatedDefault: false }
}

export async function getWords({
  page = 1,
  pageSize = 20,
  search = '',
  wordBookId,
  reviewMark,
  favoritesOnly = false,
} = {}) {
  const data = await loadData()
  let words = resolveWordPool(data, { wordBookId, favoritesOnly }).sort(
    (a, b) => new Date(b.creationTime) - new Date(a.creationTime)
  )
  if (search) {
    const kw = search.toLowerCase()
    words = words.filter((w) => w.word.toLowerCase().includes(kw))
  }
  const mark = String(reviewMark || '').trim().toLowerCase()
  if (mark === 'forget') words = words.filter((w) => w.lastQuality === 0)
  else if (mark === 'fuzzy') words = words.filter((w) => w.lastQuality === 3)
  else if (mark === 'remember') words = words.filter((w) => w.lastQuality === 5)
  else if (mark === 'unreviewed') {
    words = words.filter((w) => w.lastQuality == null)
  } else if (mark === 'favorite') {
    words = words.filter((w) => !!w.isFavorite)
  }
  const total = words.length
  const items = words.slice((page - 1) * pageSize, page * pageSize)
  return { items, total, page, pageSize }
}

export async function addWord({ word, definition, example, wordBookId }) {
  const data = await loadData()
  const bookId = wordBookId || data.currentBookId
  if (!findBook(data, bookId)) throw new Error('单词本不存在')
  const exists = bookWords(data, bookId).some(
    (w) => w.word.toLowerCase() === word.toLowerCase()
  )
  if (exists) throw new Error('该单词已存在')

  const item = {
    id: newId(),
    wordBookId: bookId,
    word: word.trim(),
    definition: definition?.trim() || '',
    example: example?.trim() || '',
    repetitionCount: 0,
    easeFactor: 2.5,
    interval: 0,
    nextReview: null,
    lastQuality: null,
    isFavorite: false,
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

/** 批量删除本地单词，返回实际删除数量 */
export async function deleteWords(ids = []) {
  const idSet = new Set((ids || []).map(String).filter(Boolean))
  if (!idSet.size) return { deleted: 0 }
  const data = await loadData()
  const before = data.words.length
  data.words = data.words.filter((w) => !idSet.has(String(w.id)))
  const deleted = before - data.words.length
  if (deleted > 0) await saveData(data)
  return { deleted }
}

export async function getDueWords({ limit = 50, wordBookId, favoritesOnly = false } = {}) {
  const data = await loadData()
  const now = new Date()
  return resolveWordPool(data, { wordBookId, favoritesOnly })
    .filter((w) => !w.nextReview || new Date(w.nextReview) <= now)
    .sort((a, b) => {
      if (!a.nextReview) return -1
      if (!b.nextReview) return 1
      return new Date(a.nextReview) - new Date(b.nextReview)
    })
    .slice(0, limit)
}

export async function getRandomWords({ limit = 50, wordBookId, favoritesOnly = false } = {}) {
  const data = await loadData()
  const pool = resolveWordPool(data, { wordBookId, favoritesOnly }).filter(
    (w) => w.interval < 21
  )
  return pool.sort(() => Math.random() - 0.5).slice(0, limit)
}

export async function reviewWord(id, quality) {
  const data = await loadData()
  const idx = data.words.findIndex((w) => w.id === id)
  if (idx === -1) throw new Error('单词不存在')

  data.words[idx] = updateReview(data.words[idx], quality)
  const book = findBook(data, data.words[idx].wordBookId)
  if (book) book.reviewLogsCount = (book.reviewLogsCount || 0) + 1
  await saveData(data)
  return data.words[idx]
}

export async function toggleFavorite(id, isFavorite) {
  const data = await loadData()
  const idx = data.words.findIndex((w) => w.id === id)
  if (idx === -1) throw new Error('单词不存在')
  const next =
    typeof isFavorite === 'boolean' ? isFavorite : !data.words[idx].isFavorite
  data.words[idx] = { ...data.words[idx], isFavorite: next }
  await saveData(data)
  return { id: data.words[idx].id, isFavorite: next }
}

export async function getStats({ wordBookId, favoritesOnly = false } = {}) {
  const data = await loadData()
  const bookId = wordBookId || data.currentBookId
  const favMode = favoritesOnly || isFavoritesBookId(bookId)
  const words = resolveWordPool(data, { wordBookId, favoritesOnly: favMode })
  const book = favMode ? null : findBook(data, bookId)
  const now = new Date()
  const dueCount = words.filter(
    (w) => !w.nextReview || new Date(w.nextReview) <= now
  ).length
  const masteredCount = words.filter((w) => w.interval >= 21).length
  return {
    totalWords: words.length,
    dueCount,
    masteredCount,
    reviewLogsCount: favMode
      ? words.reduce((n, w) => n + (w.repetitionCount || 0), 0)
      : book?.reviewLogsCount || 0,
    wordBookName: favMode ? '收藏' : book?.name,
  }
}

export async function getAllWords({ wordBookId, favoritesOnly = false } = {}) {
  const data = await loadData()
  return resolveWordPool(data, { wordBookId, favoritesOnly }).map((w) => ({ ...w }))
}

/**
 * 批量导入单词（按单词文本合并，不覆盖已有条目的复习进度）
 * @param {Array} words
 * @param {{ wordBookId?: string }} [options]
 */
export async function importWords(words = [], { wordBookId } = {}) {
  const data = await loadData()
  const bookId = wordBookId || data.currentBookId
  if (!findBook(data, bookId)) throw new Error('单词本不存在')

  const byKey = new Map(
    bookWords(data, bookId).map((w) => [w.word.toLowerCase(), w])
  )
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

    const q = raw.lastQuality
    const item = {
      id: newId(),
      wordBookId: bookId,
      word,
      definition,
      example,
      repetitionCount: Number(raw.repetitionCount) || 0,
      easeFactor: Number(raw.easeFactor) || 2.5,
      interval: Number(raw.interval) || 0,
      nextReview: raw.nextReview || null,
      lastQuality: q === 0 || q === 3 || q === 5 ? q : null,
      isFavorite: !!(raw.isFavorite ?? raw.IsFavorite),
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
