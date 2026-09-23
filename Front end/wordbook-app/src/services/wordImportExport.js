import { Capacitor } from '@capacitor/core'
import { Filesystem, Directory, Encoding } from '@capacitor/filesystem'
import { Share } from '@capacitor/share'
import { getActiveWordbook } from './appSettings'
import { getAllWords, importWords } from './localWordStore'
import { fetchAllCloudWords } from './wordSyncService'
import { addUserWord } from '../api/word'

function stamp() {
  const d = new Date()
  const p = (n) => String(n).padStart(2, '0')
  return `${d.getFullYear()}${p(d.getMonth() + 1)}${p(d.getDate())}_${p(d.getHours())}${p(d.getMinutes())}`
}

function normalizeWord(raw) {
  if (!raw || typeof raw !== 'object') return null
  const word = String(raw.word || raw.Word || '').trim()
  if (!word) return null
  return {
    word,
    definition: String(raw.definition ?? raw.Definition ?? '').trim(),
    example: String(raw.example ?? raw.Example ?? '').trim(),
    repetitionCount: Number(raw.repetitionCount ?? raw.RepetitionCount) || 0,
    easeFactor: Number(raw.easeFactor ?? raw.EaseFactor) || 2.5,
    interval: Number(raw.interval ?? raw.Interval) || 0,
    nextReview: raw.nextReview ?? raw.NextReview ?? null,
    creationTime: raw.creationTime ?? raw.CreationTime ?? undefined,
  }
}

export async function getActiveAllWords() {
  const active = await getActiveWordbook()
  if (active === 'local') return getAllWords()
  return fetchAllCloudWords()
}

/** 导出为 JSON 文本 */
export function serializeToJson(words, meta = {}) {
  const payload = {
    version: 1,
    app: 'EaseWord',
    exportedAt: new Date().toISOString(),
    source: meta.source || 'unknown',
    count: words.length,
    words: words.map((w) => ({
      word: w.word,
      definition: w.definition || '',
      example: w.example || '',
      repetitionCount: w.repetitionCount ?? 0,
      easeFactor: w.easeFactor ?? 2.5,
      interval: w.interval ?? 0,
      nextReview: w.nextReview || null,
      creationTime: w.creationTime || null,
    })),
  }
  return JSON.stringify(payload, null, 2)
}

/** 导出为 TXT 文本（块格式，便于往返解析） */
export function serializeToTxt(words) {
  const lines = [
    '# EaseWord Wordbook Export',
    '# 格式：每个单词一块，字段以 word: / definition: / example: 开头',
    `# exportedAt: ${new Date().toISOString()}`,
    `# count: ${words.length}`,
    '',
  ]

  for (const w of words) {
    lines.push(`word: ${w.word || ''}`)
    lines.push(`definition: ${w.definition || ''}`)
    lines.push(`example: ${w.example || ''}`)
    if (w.repetitionCount != null) lines.push(`repetitionCount: ${w.repetitionCount}`)
    if (w.easeFactor != null) lines.push(`easeFactor: ${w.easeFactor}`)
    if (w.interval != null) lines.push(`interval: ${w.interval}`)
    if (w.nextReview) lines.push(`nextReview: ${w.nextReview}`)
    lines.push('')
  }

  return lines.join('\n')
}

function parsePipeLine(line) {
  const parts = line.split('|').map((s) => s.trim())
  if (parts.length < 1 || !parts[0]) return null
  return normalizeWord({
    word: parts[0],
    definition: parts[1] || '',
    example: parts.slice(2).join(' | ') || '',
  })
}

function parseTxtBlocks(text) {
  const words = []
  let current = null

  const flush = () => {
    const item = normalizeWord(current)
    if (item) words.push(item)
    current = null
  }

  for (const rawLine of text.split(/\r?\n/)) {
    const line = rawLine.trim()
    if (!line || line.startsWith('#')) continue

    if (line.includes('|') && !/^(word|definition|example)\s*:/i.test(line)) {
      flush()
      const item = parsePipeLine(line)
      if (item) words.push(item)
      continue
    }

    const m = line.match(/^(word|definition|example|repetitionCount|easeFactor|interval|nextReview)\s*:\s*(.*)$/i)
    if (m) {
      const key = m[1].toLowerCase()
      const value = m[2] ?? ''
      if (key === 'word') {
        flush()
        current = { word: value }
      } else {
        if (!current) current = { word: '' }
        current[key] = value
      }
      continue
    }
  }
  flush()
  return words
}

/** 从文件内容解析单词列表（自动识别 JSON / TXT） */
export function parseWordFile(content, filename = '') {
  const text = String(content || '').replace(/^\uFEFF/, '').trim()
  if (!text) throw new Error('文件为空')

  const lower = filename.toLowerCase()
  const tryJson = lower.endsWith('.json') || text.startsWith('{') || text.startsWith('[')

  if (tryJson) {
    try {
      const data = JSON.parse(text)
      let list = []
      if (Array.isArray(data)) list = data
      else if (Array.isArray(data?.words)) list = data.words
      else throw new Error('JSON 中未找到 words 数组')

      const words = list.map(normalizeWord).filter(Boolean)
      if (!words.length) throw new Error('未解析到有效单词')
      return words
    } catch (e) {
      if (lower.endsWith('.json')) throw e instanceof Error ? e : new Error('JSON 解析失败')
      // 不是合法 JSON 时回退 TXT
    }
  }

  const words = parseTxtBlocks(text)
  if (!words.length) throw new Error('未解析到有效单词，请检查 TXT/JSON 格式')
  return words
}

async function saveAndShare(filename, content) {
  if (Capacitor.isNativePlatform()) {
    const file = await Filesystem.writeFile({
      path: filename,
      data: content,
      directory: Directory.Cache,
      encoding: Encoding.UTF8,
    })
    await Share.share({
      title: filename,
      text: `单词本导出：${filename}`,
      url: file.uri,
      dialogTitle: '导出单词本',
    })
    return { mode: 'share', filename }
  }

  const blob = new Blob([content], { type: 'text/plain;charset=utf-8' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  document.body.appendChild(a)
  a.click()
  a.remove()
  URL.revokeObjectURL(url)
  return { mode: 'download', filename }
}

/** 导出当前单词本 */
export async function exportActiveWordbook(format = 'json') {
  const source = await getActiveWordbook()
  const words = await getActiveAllWords()
  if (!words.length) throw new Error('当前单词本没有单词可导出')

  const name = `wordbook_${source}_${stamp()}.${format === 'txt' ? 'txt' : 'json'}`
  const content =
    format === 'txt' ? serializeToTxt(words) : serializeToJson(words, { source })

  const result = await saveAndShare(name, content)
  return { ...result, count: words.length, source }
}

/** 将解析出的单词导入当前单词本（合并，不覆盖已有） */
export async function importIntoActiveWordbook(words) {
  const list = (words || []).map(normalizeWord).filter(Boolean)
  if (!list.length) throw new Error('没有可导入的单词')

  const active = await getActiveWordbook()
  if (active === 'local') {
    const result = await importWords(list)
    return { source: 'local', total: list.length, ...result, failed: 0 }
  }

  const cloud = await fetchAllCloudWords()
  const keys = new Set(cloud.map((w) => (w.word || '').toLowerCase()))
  let added = 0
  let skipped = 0
  let failed = 0

  for (const item of list) {
    const key = item.word.toLowerCase()
    if (keys.has(key)) {
      skipped += 1
      continue
    }
    try {
      await addUserWord({
        word: item.word,
        definition: item.definition || '',
        example: item.example || '',
      })
      keys.add(key)
      added += 1
    } catch {
      failed += 1
    }
  }

  return { source: 'server', total: list.length, added, skipped, updated: 0, failed }
}

export function readFileAsText(file) {
  return new Promise((resolve, reject) => {
    const reader = new FileReader()
    reader.onload = () => resolve(String(reader.result || ''))
    reader.onerror = () => reject(new Error('读取文件失败'))
    reader.readAsText(file, 'utf-8')
  })
}
