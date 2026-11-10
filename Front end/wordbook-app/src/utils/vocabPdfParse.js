/**
 * 从 PDF 抽取的纯文本中解析词表（对齐 tools/word-pack-import/parse_vocab_pdf.py）
 */

const CET6_RE =
  /([A-Za-z][A-Za-z\-']{1,40})\s+((?:[a-z]{1,6}\.)+(?:&[a-z]{1,6}\.)*[\s\S]*?)(?=(?:\s+[A-Za-z][A-Za-z\-']{1,40}\s+(?:[a-z]{1,6}\.)+)|\s*$)/gim

const POS_START =
  /^(?:n\.|v\.|vt\.|vi\.|a\.|adj\.|adv\.|prep\.|conj\.|pron\.|num\.|art\.|int\.|aux\.|modal\.|pl\.|sb\.|sth\.)/i

const POS_IN_REST =
  /((?:n\.|v\.|vt\.|vi\.|a\.|adj\.|adv\.|prep\.|conj\.|pron\.|num\.|art\.|int\.|v\.aux\.|aux\.).*)$/i

function collapseSpacedLatin(text) {
  let t = String(text || '').replace(/\b(?:[A-Za-z]\s+){1,}[A-Za-z]\b/g, (m) =>
    m.replace(/\s+/g, '')
  )
  // 仅在词性后接空白/中文时拆分，避免拆坏 exam.com 一类域名
  const pos =
    '(?:vt|vi|adj|adv|prep|conj|pron|aux|num|art|int|ad|pl|n|v|a)\\.(?:&(?:vt|vi|adj|adv|n|v|a)\\.)*'
  t = t.replace(new RegExp(`([A-Za-z]{2,})(${pos})(?=[\\s\\u4e00-\\u9fff]|$)`, 'gi'), '$1 $2')
  return t
}

function finalize(found, keepOrder = false) {
  let items = Object.values(found)
  if (keepOrder) {
    items.sort((a, b) => (a.rank || 0) - (b.rank || 0))
  } else {
    items.sort((a, b) => a.word.toLowerCase().localeCompare(b.word.toLowerCase()))
    items.forEach((item, i) => {
      item.rank = i + 1
    })
  }
  return items.map((item) => {
    let word = item.word
    if (word === word.toUpperCase() || word === word.toLowerCase()) {
      word = word.toLowerCase()
    }
    return {
      word,
      definition: String(item.definition || '')
        .replace(/\s+/g, ' ')
        .trim()
        .slice(0, 1000),
      example: String(item.example || '').slice(0, 2000),
      repetitionCount: 0,
      easeFactor: 2.5,
      interval: 0,
      nextReview: null,
    }
  })
}

/** 把被换行拆开的考研行拼回「序号 单词 CEFR 释义 句频」 */
function joinKaoyanBrokenLines(text) {
  const lines = String(text || '').split(/\r?\n/)
  const out = []
  for (const raw of lines) {
    const line = raw.trim()
    if (!line) continue
    if (/^\d+\s+[A-Za-z]/.test(line) || !out.length) out.push(line)
    else out[out.length - 1] += ' ' + line
  }
  return out.join('\n')
}

function parseKaoyan(text) {
  const found = {}
  const src = joinKaoyanBrokenLines(text.replace(/\u3000/g, ' '))
  const lineRe = /^(\d+)\s+([A-Za-z][A-Za-z\-']*)\s+([A-C][12])\s+(.+)$/
  for (const raw of src.split(/\n/)) {
    const line = raw.trim()
    if (!line) continue
    const m = line.match(lineRe)
    if (!m) continue
    const word = m[2].trim()
    const key = word.toLowerCase()
    if (found[key]) continue
    let definition = m[4].replace(/\s+/g, ' ').trim()
    // 去掉行尾句频数字
    definition = definition.replace(/\s+\d+\s*$/, '').replace(/\s*\|\s*$/, '').trim()
    found[key] = {
      word,
      definition,
      example: '',
      rank: Number(m[1]) || 0,
    }
  }
  return finalize(found, true)
}

function parseCet4Phonetic(text) {
  let src = text.replace(/([A-Za-z])\n([a-z]{1,10})\b/g, '$1$2')
  src = src.replace(/\u3000/g, ' ')
  const found = {}
  const lineRe = /^\s*(\d+)\s+([A-Za-z][A-Za-z\-'.]*)\s+(.+)$/gm
  let m
  while ((m = lineRe.exec(src))) {
    let word = m[2].trim().replace(/\.$/, '')
    const rest = m[3].trim()
    if (!word || word.length < 2) continue
    let definition = rest
    const posM = rest.match(POS_IN_REST)
    if (posM) definition = posM[1].trim()
    else if (!POS_START.test(rest)) definition = rest

    const key = word.toLowerCase()
    if (!found[key]) {
      found[key] = { word, definition, example: '', rank: Number(m[1]) || 0 }
    }
  }
  return finalize(found, true)
}

function parseCet6Columns(text) {
  // 先按行，再压成空格，避免跨行截断释义
  const cleaned = text
    .replace(/\u3000/g, ' ')
    .replace(/\r?\n+/g, ' ')
    .replace(/[ \t]+/g, ' ')
  const found = {}
  let m
  CET6_RE.lastIndex = 0
  while ((m = CET6_RE.exec(cleaned))) {
    const word = m[1].trim()
    let definition = m[2].trim()
    if (word.length < 2) continue
    if (['the', 'and', 'for', 'with', 'from'].includes(word.toLowerCase())) continue
    if (!/\./.test(definition)) continue
    if (!/^[A-Za-z][A-Za-z\-']{0,40}$/.test(word)) continue
    if ((word.match(/'/g) || []).length > 1) continue
    // 释义截断到下一个疑似词前已由 lookahead 处理；去掉过长噪声
    definition = definition.replace(/\s+/g, ' ').slice(0, 1000)
    const key = word.toLowerCase()
    if (!found[key] || definition.length > found[key].definition.length) {
      found[key] = { word, definition, example: '', rank: 0 }
    }
  }
  return finalize(found, false)
}

function pickBest(candidates) {
  return candidates.reduce((best, cur) => (!best || cur.length > best.length ? cur : best), null) || []
}

/** 从 PDF 文本解析词表 */
export function parseVocabPdfText(text, filename = '') {
  const raw = collapseSpacedLatin(String(text || ''))
  if (!raw.trim()) return []

  const name = filename || ''
  const head = raw.slice(0, 2500)

  const results = []

  if (/考研|超纲/.test(name) || /CEFR|完整总榜|句频|FULL\s*RANKING/i.test(head)) {
    results.push(parseKaoyan(raw))
  }

  if (/四级/.test(name) || /注音/.test(raw.slice(0, 800)) || /序号\s+单词\s+注音/.test(raw.slice(0, 800))) {
    results.push(parseCet4Phonetic(raw))
  }

  if (/六级|乱序|cet[- ]?6/i.test(name) || /六级|乱序/.test(head)) {
    results.push(parseCet6Columns(raw))
  }

  // 通用兜底：三种都试，取最多
  results.push(parseKaoyan(raw), parseCet6Columns(raw), parseCet4Phonetic(raw))

  const best = pickBest(results.filter((x) => x && x.length))
  return best
}
