/**
 * 用站点自带的 PDF.js（public/pdfjs）从 ArrayBuffer 抽纯文本
 */

const loadPdfJs = () =>
  new Promise((resolve, reject) => {
    if (window.pdfjsLib) {
      resolve(window.pdfjsLib)
      return
    }

    const existing = document.querySelector('script[data-listen-pdfjs="true"]')
    if (existing) {
      existing.addEventListener(
        'load',
        () => {
          window.pdfjsLib ? resolve(window.pdfjsLib) : reject(new Error('PDF.js 加载失败'))
        },
        { once: true }
      )
      existing.addEventListener('error', () => reject(new Error('PDF.js 脚本加载失败')), {
        once: true,
      })
      return
    }

    const base = `${process.env.BASE_URL}pdfjs/`
    const script = document.createElement('script')
    script.src = `${base}pdf.min.js`
    script.async = true
    script.dataset.listenPdfjs = 'true'
    script.onload = () => {
      if (!window.pdfjsLib) {
        reject(new Error('PDF.js 加载失败'))
        return
      }
      if (!window.__listenPdfWorkerConfigured) {
        window.pdfjsLib.GlobalWorkerOptions.workerSrc = `${base}pdf.worker.min.js`
        window.__listenPdfWorkerConfigured = true
      }
      resolve(window.pdfjsLib)
    }
    script.onerror = () => reject(new Error('PDF.js 脚本加载失败'))
    document.head.appendChild(script)
  })

/** PDF.js 缺 CMap 时常见「i n i t i a l l y」逐字母空格 */
export function collapseSpacedLatin(text) {
  let t = String(text || '').replace(/\b(?:[A-Za-z]\s+){1,}[A-Za-z]\b/g, (m) =>
    m.replace(/\s+/g, '')
  )
  const pos =
    '(?:vt|vi|adj|adv|prep|conj|pron|aux|num|art|int|ad|pl|n|v|a)\\.(?:&(?:vt|vi|adj|adv|n|v|a)\\.)*'
  t = t.replace(new RegExp(`([A-Za-z]{2,})(${pos})(?=[\\s\\u4e00-\\u9fff]|$)`, 'gi'), '$1 $2')
  return t
}

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
  const pdfjsLib = await loadPdfJs()
  const data =
    arrayBuffer instanceof Uint8Array ? arrayBuffer : new Uint8Array(arrayBuffer)
  const base = `${process.env.BASE_URL}pdfjs/`
  const doc = await pdfjsLib.getDocument({
    data,
    cMapUrl: `${base}cmaps/`,
    cMapPacked: true,
    standardFontDataUrl: `${base}standard_fonts/`,
  }).promise

  const parts = []
  for (let i = 1; i <= doc.numPages; i++) {
    const page = await doc.getPage(i)
    const content = await page.getTextContent()
    parts.push(textContentToLines(content))
  }
  return collapseSpacedLatin(parts.join('\n'))
}
