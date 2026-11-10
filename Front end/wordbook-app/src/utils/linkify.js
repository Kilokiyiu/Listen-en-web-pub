const URL_RE = /(https?:\/\/[^\s<>"'，。；、）】》]+)/g

/** 把纯文本拆成 text / link 片段，便于模板里渲染可点链接 */
export function splitLinkParts(text) {
  const raw = String(text || '')
  if (!raw) return []
  const parts = []
  let last = 0
  let m
  URL_RE.lastIndex = 0
  while ((m = URL_RE.exec(raw)) !== null) {
    if (m.index > last) {
      parts.push({ type: 'text', value: raw.slice(last, m.index) })
    }
    let url = m[1]
    // 去掉末尾常见标点
    url = url.replace(/[),.!?:;]+$/g, '')
    parts.push({ type: 'link', value: url })
    last = m.index + url.length
    URL_RE.lastIndex = last
  }
  if (last < raw.length) {
    parts.push({ type: 'text', value: raw.slice(last) })
  }
  return parts.length ? parts : [{ type: 'text', value: raw }]
}

/** 在系统浏览器打开（APK 下载链路由系统处理） */
export function openExternalUrl(url) {
  if (!url) return
  try {
    const opened = window.open(url, '_blank', 'noopener,noreferrer')
    if (opened) return
  } catch {
    /* fall through */
  }
  const a = document.createElement('a')
  a.href = url
  a.target = '_blank'
  a.rel = 'noopener noreferrer'
  a.style.display = 'none'
  document.body.appendChild(a)
  a.click()
  a.remove()
}
