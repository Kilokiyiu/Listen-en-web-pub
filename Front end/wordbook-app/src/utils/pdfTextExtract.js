import * as pdfjsLib from 'pdfjs-dist/build/pdf.js'

const base = import.meta.env.BASE_URL || '/'
pdfjsLib.GlobalWorkerOptions.workerSrc = `${base}pdf.worker.min.js`

/**
 * PDF.js 缺 CMap 时常见「i n i t i a l l y」逐字母空格；导入前合并，
 * 并在单词与词性（n./vt. 等）之间补回空格。
 */
export function collapseSpacedLatin(text) {
  let t = String(text || '').replace(/\b(?:[A-Za-z]\s+){1,}[A-Za-z]\b/g, (m) =>
    m.replace(/\s+/g, '')
  )
  const pos =
    '(?:vt|vi|adj|adv|prep|conj|pron|aux|num|art|int|ad|pl|n|v|a)\\.(?:&(?:vt|vi|adj|adv|n|v|a)\\.)*'
  t = t.replace(new RegExp(`([A-Za-z]{2,})(${pos})(?=[\\s\\u4e00-\\u9fff]|$)`, 'gi'), '$1 $2')
  return t
}

/**
 * 按 Y 坐标 / hasEOL 重建行，避免整页拼成一行导致「考研」行正则失效。
 */
function textContentToLines(content) {
  let lastY = null
  let buf = []
  const lines = []

  const flush = () => {
    if (!buf.length) return
    lines.push(buf.join(''))
    buf = []
  }

  const pushStr = (s) => {
    if (!s) return
    if (buf.length) {
      const prev = buf[buf.length - 1]
      if (/[A-Za-z0-9]$/.test(prev) && /^[A-Za-z0-9]/.test(s)) buf.push(' ')
      else if (/[A-Za-z.]$/.test(prev) && /^[\u4e00-\u9fff]/.test(s)) buf.push(' ')
      else if (/[\u4e00-\u9fff]$/.test(prev) && /^[A-Za-z]/.test(s)) buf.push(' ')
      else if (/\d$/.test(prev) && /^[A-Za-z]/.test(s)) buf.push(' ')
      else if (/[A-Za-z]$/.test(prev) && /^\d/.test(s)) buf.push(' ')
    }
    buf.push(s)
  }

  for (const it of content.items) {
    const y = it.transform?.[5] ?? 0
    if (lastY != null && Math.abs(y - lastY) > 3) flush()
    pushStr(it.str || '')
    if (it.hasEOL) {
      flush()
      lastY = null
      continue
    }
    lastY = y
  }
  flush()
  return lines.join('\n')
}

export async function extractPdfText(arrayBuffer) {
  const data =
    arrayBuffer instanceof Uint8Array ? arrayBuffer : new Uint8Array(arrayBuffer)

  const doc = await pdfjsLib.getDocument({
    data,
    // 尽量加载 CMap，减轻中文/子集字体拆字（public 下可选）
    cMapUrl: `${base}pdfjs/cmaps/`,
    cMapPacked: true,
    standardFontDataUrl: `${base}pdfjs/standard_fonts/`,
  }).promise

  const parts = []
  for (let i = 1; i <= doc.numPages; i++) {
    const page = await doc.getPage(i)
    const content = await page.getTextContent()
    parts.push(textContentToLines(content))
  }

  return collapseSpacedLatin(parts.join('\n'))
}
