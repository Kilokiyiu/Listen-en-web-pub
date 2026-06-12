const SITE_URL = import.meta.env.VITE_SITE_URL || 'https://your-domain.com'
const DEFAULT_TITLE = 'ListenEase - 专业英语听力练习平台 | 四六级雅思托福真题'
const DEFAULT_DESCRIPTION =
  'ListenEase 是专业的英语听力练习平台，提供大学英语四六级、雅思、托福历年真题听力在线练习，支持真题试卷与答案 PDF 下载、每日英语短文、BBC 新闻阅读、词根词缀学习与单词本，助您高效备考、提升听力水平。'
const DEFAULT_KEYWORDS =
  '英语听力,听力练习,四六级真题,雅思听力,托福听力,试卷下载,ListenEase'
const OG_IMAGE = `${SITE_URL}/og-cover.png`

function upsertMeta (attr, key, content) {
  let el = document.querySelector(`meta[${attr}="${key}"]`)
  if (!el) {
    el = document.createElement('meta')
    el.setAttribute(attr, key)
    document.head.appendChild(el)
  }
  el.content = content
}

export function setPageMeta ({ title, description, path = '/' } = {}) {
  const pageTitle = title || DEFAULT_TITLE
  const pageDescription = description || DEFAULT_DESCRIPTION
  const canonicalUrl = `${SITE_URL}${path === '/' ? '/' : path}`

  document.title = pageTitle
  upsertMeta('name', 'description', pageDescription)
  upsertMeta('name', 'keywords', DEFAULT_KEYWORDS)
  upsertMeta('property', 'og:title', pageTitle)
  upsertMeta('property', 'og:description', pageDescription)
  upsertMeta('property', 'og:url', canonicalUrl)
  upsertMeta('property', 'og:image', OG_IMAGE)
  upsertMeta('property', 'og:image:secure_url', OG_IMAGE)
  upsertMeta('name', 'twitter:title', pageTitle)
  upsertMeta('name', 'twitter:description', pageDescription)
  upsertMeta('name', 'twitter:image', OG_IMAGE)

  let canonical = document.querySelector('link[rel="canonical"]')
  if (!canonical) {
    canonical = document.createElement('link')
    canonical.rel = 'canonical'
    document.head.appendChild(canonical)
  }
  canonical.href = canonicalUrl
}

export { DEFAULT_TITLE, DEFAULT_DESCRIPTION, DEFAULT_KEYWORDS, SITE_URL, OG_IMAGE }
