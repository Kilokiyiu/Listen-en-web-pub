import { Capacitor } from '@capacitor/core'
import { Filesystem, Directory, Encoding } from '@capacitor/filesystem'
import { Share } from '@capacitor/share'
import { getActiveWordbook, getCurrentWordBookId } from './appSettings'
import { getAllWords, importWords } from './localWordStore'
import { fetchAllCloudWords } from './wordSyncService'
import { importUserWords } from '../api/word'

function stamp() {
  const d = new Date()
  const p = (n) => String(n).padStart(2, '0')
  return `${d.getFullYear()}${p(d.getMonth() + 1)}${p(d.getDate())}_${p(d.getHours())}${p(d.getMinutes())}`
}

/** 音标、词性无独立字段，统一并入释义 */
const PHONETIC_RE = /([\/\[].*?[\/\]])/g
const POS_ONLY_RE = /^(?:[a-z]{1,6}\.)(?:&[a-z]{1,6}\.)*$/i
const FIELD_KEY_RE =
  /^(word|definition|example|phonetic|phonetics|pos|partofspeech|repetitionCount|easeFactor|interval|nextReview)\s*:\s*(.*)$/i

function extractPhonetics(text) {
  const phonetics = []
  const cleaned = String(text || '')
    .replace(PHONETIC_RE, (_, p) => {
      const t = String(p || '').trim()
      if (t) phonetics.push(t)
      return ' '
    })
    .replace(/\s+/g, ' ')
    .trim()
  return { text: cleaned, phonetics }
}

function isPosOnly(s) {
  const t = String(s || '').trim()
  return !!t && t.length <= 24 && POS_ONLY_RE.test(t)
}

function mergeDefinitionParts({ phonetics = [], pos = '', definition = '' } = {}) {
  const bits = []
  for (const p of phonetics) {
    const t = String(p || '').trim()
    if (t) bits.push(t)
  }
  const posText = String(pos || '').trim()
  if (posText) bits.push(posText.endsWith('.') ? posText : `${posText}.`)
  const def = String(definition || '').trim()
  if (def) bits.push(def)
  return bits.join(' ').replace(/\s+/g, ' ').trim()
}

function normalizeWord(raw) {
  if (!raw || typeof raw !== 'object') return null

  let wordRaw = String(raw.word || raw.Word || '').trim()
  let definition = String(raw.definition ?? raw.Definition ?? '').trim()
  const example = String(raw.example ?? raw.Example ?? '').trim()
  let pos = String(
    raw.pos ?? raw.Pos ?? raw.partOfSpeech ?? raw.PartOfSpeech ?? ''
  ).trim()
  const phoneticField = String(
    raw.phonetic ?? raw.Phonetic ?? raw.phonetics ?? raw.Phonetics ?? ''
  ).trim()

  const fromWord = extractPhonetics(wordRaw)
  wordRaw = fromWord.text
  if (!wordRaw) return null

  const fromDef = extractPhonetics(definition)
  definition = fromDef.text

  const phonetics = [...fromWord.phonetics]
  if (phoneticField) {
    const fromField = extractPhonetics(phoneticField)
    if (fromField.phonetics.length) phonetics.push(...fromField.phonetics)
    else phonetics.push(phoneticField)
  }
  phonetics.push(...fromDef.phonetics)

  definition = mergeDefinitionParts({ phonetics, pos, definition })

  return {
    word: wordRaw,
    definition,
    example,
    repetitionCount: Number(raw.repetitionCount ?? raw.RepetitionCount) || 0,
    easeFactor: Number(raw.easeFactor ?? raw.EaseFactor) || 2.5,
    interval: Number(raw.interval ?? raw.Interval) || 0,
    nextReview: raw.nextReview ?? raw.NextReview ?? null,
    lastQuality: (() => {
      const q = raw.lastQuality ?? raw.LastQuality
      return q === 0 || q === 3 || q === 5 ? q : null
    })(),
    isFavorite: !!(raw.isFavorite ?? raw.IsFavorite),
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
      lastQuality: w.lastQuality ?? null,
      isFavorite: !!w.isFavorite,
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

/**
 * 把一行拆成字段。支持：| 、Tab、；、：、——、空格破折号、首个逗号/分号。
 */
function splitDelimitedParts(line) {
  const raw = String(line || '').trim()
  if (!raw) return null

  if (raw.includes('|')) {
    return raw.split('|').map((s) => s.trim())
  }
  if (raw.includes('\t')) {
    return raw.split('\t').map((s) => s.trim()).filter(Boolean)
  }

  const onceSeps = ['；', '：', '——', ' — ', ' – ', ' - ', '—', '–']
  for (const sep of onceSeps) {
    const idx = raw.indexOf(sep)
    if (idx > 0) {
      const left = raw.slice(0, idx).trim()
      const right = raw.slice(idx + sep.length).trim()
      if (left && right) return [left, right]
    }
  }

  const m = raw.match(
    /^([A-Za-z][A-Za-z\-'.]*(?:\s+[A-Za-z][A-Za-z\-']*){0,5})\s*[,，;]\s+(.+)$/
  )
  if (m) return [m[1].trim(), m[2].trim()]

  return null
}

function parseDelimitedLine(line) {
  const parts = splitDelimitedParts(line)
  if (!parts || parts.length < 2 || !parts[0]) return null

  if (parts.length >= 3 && isPosOnly(parts[1])) {
    return normalizeWord({
      word: parts[0],
      pos: parts[1],
      definition: parts[2] || '',
      example: parts.slice(3).join(' | ') || '',
    })
  }

  return normalizeWord({
    word: parts[0],
    definition: parts[1] || '',
    example: parts.slice(2).join(' | ') || '',
  })
}

function parseRichPlainLine(line) {
  let s = String(line || '')
    .replace(/^\d+[\.\)、]\s*/, '')
    .trim()
  if (!s || s.includes('|')) return null

  const wordM = s.match(
    /^([A-Za-z][A-Za-z\-']*(?:\s+[A-Za-z][A-Za-z\-']*){0,5})\s+(.+)$/
  )
  if (!wordM) return null

  const word = wordM[1].trim()
  let rest = wordM[2].trim()
  const phonetics = []

  rest = rest
    .replace(/^([\/\[].*?[\/\]])\s*/, (_, p) => {
      phonetics.push(p.trim())
      return ''
    })
    .trim()
  rest = rest
    .replace(/^([\/\[].*?[\/\]])\s*/, (_, p) => {
      phonetics.push(p.trim())
      return ''
    })
    .trim()

  let pos = ''
  const posM = rest.match(/^((?:[a-z]{1,6}\.)(?:&[a-z]{1,6}\.)*)\s*(.*)$/i)
  if (posM) {
    pos = posM[1].trim()
    rest = (posM[2] || '').trim()
  }

  const hasCn = /[\u4e00-\u9fff]/.test(rest)
  if (!phonetics.length && !pos && !hasCn) return null

  return normalizeWord({
    word,
    pos,
    phonetic: phonetics.join(' '),
    definition: rest,
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

    const m = line.match(FIELD_KEY_RE)
    if (m) {
      let key = m[1].toLowerCase()
      const value = m[2] ?? ''
      if (key === 'phonetics') key = 'phonetic'
      if (key === 'partofspeech') key = 'pos'
      if (key === 'word') {
        flush()
        current = { word: value }
      } else {
        if (!current) current = { word: '' }
        current[key] = value
      }
      continue
    }

    const delimited = parseDelimitedLine(line)
    if (delimited) {
      flush()
      words.push(delimited)
      continue
    }

    flush()
    const rich = parseRichPlainLine(line)
    if (rich) {
      words.push(rich)
      continue
    }
    const item = normalizeWord({ word: line })
    if (item) words.push(item)
  }
  flush()

  const seen = new Set()
  const deduped = []
  for (const w of words) {
    const key = (w.word || '').toLowerCase()
    if (!key || seen.has(key)) continue
    seen.add(key)
    deduped.push(w)
  }
  return deduped
}

/** 从文件内容或粘贴文本解析单词列表（自动识别 JSON / TXT / 每行一词） */
export function parseWordFile(content, filename = '') {
  const text = String(content || '').replace(/^\uFEFF/, '').trim()
  if (!text) throw new Error('内容为空')

  const lower = filename.toLowerCase()
  const tryJson = lower.endsWith('.json') || text.startsWith('{') || text.startsWith('[')

  if (tryJson) {
    try {
      const data = JSON.parse(text)
      let list = []
      if (Array.isArray(data)) list = data
      else if (Array.isArray(data?.words)) list = data.words
      else if (Array.isArray(data?.entries)) list = data.entries
      else throw new Error('JSON 中未找到 words / entries 数组')

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

export const IMPORT_FORMAT_HINT =
  '支持：PDF / Word(.docx) / JSON / TXT；分隔符可用 |、Tab、：、； 等；音标与词性写入释义'

/** 每次打开批量导入时展示的格式说明 */
export const IMPORT_FORMAT_GUIDE = [
  {
    title: '推荐：用分隔符（不必是管道 |）',
    example:
      'abandon | 放弃\nabandon：放弃\nabandon；vt. 放弃\nabandon\t放弃\nabandon, 放弃\nabandon | vt. | 放弃',
    note: '可用：竖线 |、Excel 制表符 Tab、中文冒号：、分号；、逗号（单词后）。也支持 单词 | 词性 | 释义。音标、词性都会存进「释义」。',
  },
  {
    title: '也支持：单词 + 音标 + 词性 + 释义（同一行）',
    example: 'abandon /əˈbændən/ vt. 放弃\napple [ˈæpl] n. 苹果',
    note: '音标请用 /.../ 或 [...] 包裹。导入后释义类似：/əˈbændən/ vt. 放弃',
  },
  {
    title: '文件类型 / AI 整理',
    example: 'PDF 词表 · Word(.docx) · JSON · TXT · 直接粘贴\n搞不定格式 → 点「AI 整理」',
    note: 'PDF 支持常见词表版式；Word 请用 .docx。扫描版 PDF 需先 OCR。格式很乱时可登录后用 AI 整理再导入。',
  },
  {
    title: '或：字段块 / 每行一词',
    example:
      'word: abandon\nphonetic: /əˈbændən/\npos: vt.\ndefinition: 放弃\n\n# 或仅单词：\nabandon\napple',
    note: '字段块支持 word / phonetic / pos / definition / example；仅单词时一行一个。',
  },
]

export function readFileAsArrayBuffer(file) {
  return new Promise((resolve, reject) => {
    const reader = new FileReader()
    reader.onload = () => resolve(reader.result)
    reader.onerror = () => reject(new Error('读取文件失败'))
    reader.readAsArrayBuffer(file)
  })
}

async function extractDocxText(arrayBuffer) {
  const mammoth = await import('mammoth')
  const result = await mammoth.extractRawText({ arrayBuffer })
  return String(result?.value || '').trim()
}

/** 解析导入文件（含 PDF / Word.docx） */
export async function parseImportFile(file) {
  if (!file) throw new Error('未选择文件')
  const name = file.name || ''
  const lower = name.toLowerCase()

  if (lower.endsWith('.doc') && !lower.endsWith('.docx')) {
    throw new Error('暂不支持旧版 .doc，请另存为 .docx、.pdf 或 .txt 后再导入')
  }

  if (lower.endsWith('.pdf') || file.type === 'application/pdf') {
    const { extractPdfText } = await import('../utils/pdfTextExtract')
    const { parseVocabPdfText } = await import('../utils/vocabPdfParse')
    const buf = await readFileAsArrayBuffer(file)
    const text = await extractPdfText(buf)
    if (!String(text || '').trim()) {
      throw new Error('PDF 未提取到文字（可能是扫描件，需先 OCR）')
    }
    const structured = parseVocabPdfText(text, name)
    if (structured.length) return structured
    return parseWordFile(text, '.txt')
  }

  if (
    lower.endsWith('.docx') ||
    file.type ===
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document'
  ) {
    const buf = await readFileAsArrayBuffer(file)
    const text = await extractDocxText(buf)
    if (!text) throw new Error('Word 文档未提取到文字')
    return parseWordFile(text, '.txt')
  }

  const text = await readFileAsText(file)
  return parseWordFile(text, name)
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

/** 将词条序列化为 AI 整理输入（勿用截断预览文本） */
export function serializeWordsForAi(words) {
  return (words || [])
    .map((w) => {
      const word = String(w?.word || '').trim()
      if (!word) return ''
      const def = String(w?.definition || '').trim()
      const ex = String(w?.example || '').trim()
      if (ex) return `${word} | ${def} | ${ex}`
      if (def) return `${word} | ${def}`
      return word
    })
    .filter(Boolean)
    .join('\n')
}

/**
 * 将解析出的单词导入单词本（合并，不覆盖已有）
 * @param {object[]} words
 * @param {{ wordBookId?: string|null }} [options] 指定云端目标词本；不传则用当前词本
 */
export async function importIntoActiveWordbook(words, { wordBookId: targetBookId } = {}) {
  const list = (words || []).map(normalizeWord).filter(Boolean)
  if (!list.length) throw new Error('没有可导入的单词')

  const active = await getActiveWordbook()
  if (active === 'local') {
    const result = await importWords(list, { wordBookId: targetBookId || undefined })
    return {
      source: 'local',
      total: list.length,
      ...result,
      failed: 0,
      wordBookId: targetBookId || null,
    }
  }

  const wordBookId = targetBookId || (await getCurrentWordBookId())
  const payload = {
    ...(wordBookId ? { wordBookId } : {}),
    entries: list.map((w) => ({
      word: w.word,
      definition: w.definition || '',
      example: w.example || '',
    })),
  }
  const r = await importUserWords(payload)
  return {
    source: 'server',
    total: r.total ?? list.length,
    added: r.added ?? 0,
    skipped: r.skipped ?? 0,
    updated: 0,
    failed: r.failed ?? 0,
    wordBookId: wordBookId || null,
  }
}

/** 分享分享码文本（系统分享面板） */
export async function shareShareCodeText({ code, title, wordCount, expiresAt }) {
  const expireText = expiresAt
    ? new Date(expiresAt).toLocaleDateString('zh-CN')
    : ''
  const text = [
    `EaseWord 单词本分享`,
    title ? `《${title}》` : '',
    wordCount != null ? `${wordCount} 个词` : '',
    `分享码：${code}`,
    expireText ? `有效期至 ${expireText}` : '',
    `在 App / 网站「我的单词本」点「用分享码导入」，输入分享码即可导入单词`,
  ]
    .filter(Boolean)
    .join('\n')

  if (Capacitor.isNativePlatform()) {
    await Share.share({
      title: '分享单词本',
      text,
      dialogTitle: '分享单词本',
    })
    return { mode: 'share' }
  }

  if (typeof navigator !== 'undefined' && navigator.clipboard?.writeText) {
    await navigator.clipboard.writeText(text)
    return { mode: 'clipboard' }
  }
  return { mode: 'text', text }
}

export function readFileAsText(file) {
  return new Promise((resolve, reject) => {
    const reader = new FileReader()
    reader.onload = () => resolve(String(reader.result || ''))
    reader.onerror = () => reject(new Error('读取文件失败'))
    reader.readAsText(file, 'utf-8')
  })
}
