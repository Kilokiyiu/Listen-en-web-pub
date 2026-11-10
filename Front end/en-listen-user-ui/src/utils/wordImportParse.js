/**
 * 解析单词本文件 / 粘贴文本（JSON、块 TXT、管道行、每行一词）
 * 音标、词性无独立字段，统一并入释义。
 */

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

  // 释义开头若还带着音标，也拆出来统一排布
  const fromDef = extractPhonetics(definition)
  definition = fromDef.text

  const phonetics = [...fromWord.phonetics]
  if (phoneticField) {
    const fromField = extractPhonetics(phoneticField)
    if (fromField.phonetics.length) phonetics.push(...fromField.phonetics)
    else phonetics.push(phoneticField)
  }
  phonetics.push(...fromDef.phonetics)

  // 释义若以词性开头且未单独提供 pos，保留在释义中即可
  definition = mergeDefinitionParts({ phonetics, pos, definition })

  return {
    word: wordRaw,
    definition,
    example,
  }
}

/**
 * 把一行拆成字段。支持：| 、Tab、；、：、——、空格破折号、首个逗号/分号。
 * 中文冒号/破折号只按第一次分隔（释义里可再出现）。
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

  // abandon, 放弃  /  abandon，放弃  /  abandon; 放弃
  const m = raw.match(
    /^([A-Za-z][A-Za-z\-'.]*(?:\s+[A-Za-z][A-Za-z\-']*){0,5})\s*[,，;]\s+(.+)$/
  )
  if (m) return [m[1].trim(), m[2].trim()]

  return null
}

function parseDelimitedLine(line) {
  const parts = splitDelimitedParts(line)
  if (!parts || parts.length < 2 || !parts[0]) return null

  // 单词 | 词性 | 释义 [| 例句]
  if (parts.length >= 3 && isPosOnly(parts[1])) {
    return normalizeWord({
      word: parts[0],
      pos: parts[1],
      definition: parts[2] || '',
      example: parts.slice(3).join(' | ') || '',
    })
  }

  // 单词 | 释义 [| 例句]（释义里可含词性/音标）
  return normalizeWord({
    word: parts[0],
    definition: parts[1] || '',
    example: parts.slice(2).join(' | ') || '',
  })
}

/** 解析「单词 音标 词性 释义」挤在一行、无竖线的情况 */
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
  let sawStructured = false

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
      sawStructured = true
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
      sawStructured = true
      words.push(delimited)
      continue
    }

    flush()
    const rich = parseRichPlainLine(line)
    if (rich) {
      sawStructured = true
      words.push(rich)
      continue
    }
    // 每行一词（或短语）
    const item = normalizeWord({ word: line })
    if (item) words.push(item)
  }
  flush()

  // 去重（保留首次）
  const seen = new Set()
  const deduped = []
  for (const w of words) {
    const key = w.word.toLowerCase()
    if (seen.has(key)) continue
    seen.add(key)
    deduped.push(w)
  }
  return { words: deduped, sawStructured }
}

/** 从文件内容或粘贴文本解析单词列表 */
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
    }
  }

  const { words } = parseTxtBlocks(text)
  if (!words.length) throw new Error('未解析到有效单词，请检查格式')
  return words
}

export function serializeToJson(words, meta = {}) {
  return JSON.stringify(
    {
      version: 1,
      app: 'EaseWord',
      exportedAt: new Date().toISOString(),
      source: meta.source || 'web',
      count: words.length,
      words: words.map((w) => ({
        word: w.word,
        definition: w.definition || '',
        example: w.example || '',
      })),
    },
    null,
    2
  )
}

export function serializeToTxt(words) {
  const lines = [
    '# EaseWord Wordbook Export',
    `# exportedAt: ${new Date().toISOString()}`,
    `# count: ${words.length}`,
    '',
  ]
  for (const w of words) {
    lines.push(`word: ${w.word || ''}`)
    lines.push(`definition: ${w.definition || ''}`)
    lines.push(`example: ${w.example || ''}`)
    lines.push('')
  }
  return lines.join('\n')
}

export function readFileAsText(file) {
  return new Promise((resolve, reject) => {
    const reader = new FileReader()
    reader.onload = () => resolve(String(reader.result || ''))
    reader.onerror = () => reject(new Error('读取文件失败'))
    reader.readAsText(file, 'utf-8')
  })
}

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

/**
 * 解析导入文件（含 PDF / Word.docx）：结构化版式优先，失败则回退纯文本规则
 */
export async function parseImportFile(file) {
  if (!file) throw new Error('未选择文件')
  const name = file.name || ''
  const lower = name.toLowerCase()

  if (lower.endsWith('.doc') && !lower.endsWith('.docx')) {
    throw new Error('暂不支持旧版 .doc，请另存为 .docx、.pdf 或 .txt 后再导入')
  }

  if (lower.endsWith('.pdf') || file.type === 'application/pdf') {
    const { extractPdfText } = await import('./pdfTextExtract.js')
    const { parseVocabPdfText } = await import('./vocabPdfParse.js')
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
