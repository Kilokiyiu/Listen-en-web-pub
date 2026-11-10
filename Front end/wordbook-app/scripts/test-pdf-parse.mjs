import fs from 'fs'
import path from 'path'
import { createRequire } from 'module'
import { fileURLToPath } from 'url'
import { parseVocabPdfText } from '../src/utils/vocabPdfParse.js'

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const require = createRequire(import.meta.url)
const pdfjsLib = require('pdfjs-dist/build/pdf.js')
pdfjsLib.GlobalWorkerOptions.workerSrc = require.resolve('pdfjs-dist/build/pdf.worker.js')

const cMapUrl = path.join(__dirname, '../public/pdfjs/cmaps/') + path.sep
const standardFontDataUrl = path.join(__dirname, '../public/pdfjs/standard_fonts/') + path.sep

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

const files = [
  ['G:/2.Projects_Large/Listen-en-web-production/英语资源/单词资源/大学英语六级词汇乱序版.pdf', 1900],
  ['G:/2.Projects_Large/Listen-en-web-production/英语资源/单词资源/考研英语超纲词.pdf', 200],
]

for (const [f, expectMin] of files) {
  const data = new Uint8Array(fs.readFileSync(f))
  const doc = await pdfjsLib.getDocument({
    data,
    cMapUrl: 'file:///' + cMapUrl.replace(/\\/g, '/'),
    cMapPacked: true,
    standardFontDataUrl: 'file:///' + standardFontDataUrl.replace(/\\/g, '/'),
  }).promise
  const parts = []
  for (let i = 1; i <= doc.numPages; i++) {
    const page = await doc.getPage(i)
    const c = await page.getTextContent()
    parts.push(textContentToLines(c))
  }
  const name = f.split('/').pop()
  const text = parts.join('\n')
  const parsed = parseVocabPdfText(text, name)
  console.log(name, 'parsed', parsed.length, 'expect>=', expectMin, parsed.length >= expectMin ? 'OK' : 'FAIL')
  console.log('  head:', text.slice(0, 140).replace(/\n/g, ' | '))
  console.log(
    '  sample:',
    parsed.slice(0, 3).map((w) => `${w.word}=${(w.definition || '').slice(0, 28)}`)
  )
}
