import { getCategories, getAlbumsByCategoryId } from '../api/Listen.js'
import { getKaoyanPapers } from '../api/Kaoyan.js'
import { listCategoryMeta } from './categoryMeta.js'

const CACHE_KEY = 'le_paper_catalog_v1'
const CACHE_TTL = 10 * 60 * 1000

/** @type {{ items: Array, loadedAt: number } | null} */
let memoryCache = null

const LISTENING_CODES = new Set(['cet4', 'cet6'])

const ALIASES = {
  cet4: ['四级', 'cet4', 'cet-4', '大学英语四级', '英语四级', '四级真题', '四级往年试卷'],
  cet6: ['六级', 'cet6', 'cet-6', '大学英语六级', '英语六级', '六级听力', '六级真题', '六级考试卷'],
  eng1: ['英语一', '考研英语一', '英一', 'eng1'],
  eng2: ['英语二', '考研英语二', '英二', 'eng2', '考研英语2'],
  kaoyan: ['考研', '考研英语', 'kaoyan'],
}

function normalize (text) {
  return String(text || '')
    .toLowerCase()
    .replace(/\s+/g, '')
    .replace(/[＿_·•]/g, '')
}

function readSessionCache () {
  try {
    const raw = sessionStorage.getItem(CACHE_KEY)
    if (!raw) return null
    const parsed = JSON.parse(raw)
    if (!parsed?.loadedAt || Date.now() - parsed.loadedAt > CACHE_TTL) return null
    return parsed
  } catch {
    return null
  }
}

function writeSessionCache (payload) {
  try {
    sessionStorage.setItem(CACHE_KEY, JSON.stringify(payload))
  } catch {
    /* ignore quota */
  }
}

function albumTitle (album) {
  return album?.name?.chinese || album?.name?.english || album?.name || '未命名试卷'
}

function mapListeningAlbum (album, category) {
  const code = category?.code || ''
  const meta = listCategoryMeta[code] || {}
  const title = albumTitle(album)
  const label = meta.label || code.toUpperCase() || '听力'
  const kindLabel = meta.title || category?.name?.chinese || '听力真题'
  const alias = ALIASES[code] || []
  return {
    id: album.id || album.Id,
    kind: 'listening',
    code,
    title,
    label,
    subtitle: kindLabel,
    haystack: normalize([title, label, kindLabel, code, ...alias].join(' ')),
    route: { name: 'examDetail', query: { albumId: album.id || album.Id } },
  }
}

function mapKaoyanPaper (paper) {
  const series = paper.series || paper.Series || ''
  const seriesName = paper.seriesName || paper.SeriesName || (series === 'eng2' ? '英语二' : '英语一')
  const year = paper.year ?? paper.Year ?? ''
  const title = paper.title || paper.Title || `${year} ${seriesName}`
  const alias = [...(ALIASES.kaoyan || []), ...(ALIASES[series] || [])]
  return {
    id: paper.id || paper.Id,
    kind: 'kaoyan',
    code: series || 'kaoyan',
    title,
    label: '考研',
    subtitle: `${seriesName} · 完形 / 阅读`,
    haystack: normalize([title, seriesName, series, year, '考研', ...alias].join(' ')),
    route: { name: 'kaoyanPaper', query: { paperId: paper.id || paper.Id } },
  }
}

/**
 * 拉取四六级听力 + 考研试卷目录（带内存 / session 缓存）
 */
export async function loadPaperCatalog ({ force = false } = {}) {
  if (!force && memoryCache && Date.now() - memoryCache.loadedAt < CACHE_TTL) {
    return memoryCache.items
  }
  if (!force) {
    const cached = readSessionCache()
    if (cached?.items?.length) {
      memoryCache = cached
      return cached.items
    }
  }

  const categories = (await getCategories()) || []
  const listeningCats = categories.filter((c) => LISTENING_CODES.has(c.code))

  const albumGroups = await Promise.all(
    listeningCats.map(async (cat) => {
      try {
        const albums = (await getAlbumsByCategoryId(cat.id)) || []
        return albums.map((a) => mapListeningAlbum(a, cat))
      } catch {
        return []
      }
    })
  )

  let kaoyanItems = []
  try {
    const papers = (await getKaoyanPapers()) || []
    kaoyanItems = papers.map(mapKaoyanPaper)
  } catch {
    kaoyanItems = []
  }

  const items = [...albumGroups.flat(), ...kaoyanItems]
  memoryCache = { items, loadedAt: Date.now() }
  writeSessionCache(memoryCache)
  return items
}

/**
 * @param {string} query
 * @param {{ kind?: 'all'|'listening'|'kaoyan' }} [opts]
 */
export function searchPapers (items, query, opts = {}) {
  const kind = opts.kind || 'all'
  const q = normalize(query)
  let pool = items
  if (kind === 'listening') pool = items.filter((i) => i.kind === 'listening')
  if (kind === 'kaoyan') pool = items.filter((i) => i.kind === 'kaoyan')

  if (!q) return pool

  // 多关键词：全部命中才保留（如「四级 2023」）
  const tokens = String(query || '')
    .trim()
    .toLowerCase()
    .split(/[\s,，、+]+/)
    .map(normalize)
    .filter(Boolean)

  const scored = []
  for (const item of pool) {
    let score = 0
    const titleN = normalize(item.title)
    const allHit = tokens.every((t) => item.haystack.includes(t) || titleN.includes(t))
    if (!allHit) continue

    for (const t of tokens) {
      if (titleN.includes(t)) score += 8
      if (normalize(item.label).includes(t)) score += 4
      if (normalize(item.subtitle).includes(t)) score += 3
      if (item.haystack.includes(t)) score += 1
      if (titleN.startsWith(t)) score += 2
    }
    scored.push({ item, score })
  }

  scored.sort((a, b) => b.score - a.score || a.item.title.localeCompare(b.item.title, 'zh'))
  return scored.map((s) => s.item)
}

export function clearPaperCatalogCache () {
  memoryCache = null
  try {
    sessionStorage.removeItem(CACHE_KEY)
  } catch {
    /* ignore */
  }
}
