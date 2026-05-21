const SITE_URL = 'https://listenease.online'
const DEFAULT_TITLE = 'ListenEase - 专业英语听力练习平台 | 四六级雅思托福真题'
const DEFAULT_DESCRIPTION =
  'ListenEase 是专业的英语听力练习平台，提供四六级、雅思、托福历年真题、每日短文与智能学习功能，助您高效提升听力水平。'

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
  upsertMeta('property', 'og:title', pageTitle)
  upsertMeta('property', 'og:description', pageDescription)
  upsertMeta('property', 'og:url', canonicalUrl)
  upsertMeta('name', 'twitter:title', pageTitle)
  upsertMeta('name', 'twitter:description', pageDescription)

  let canonical = document.querySelector('link[rel="canonical"]')
  if (!canonical) {
    canonical = document.createElement('link')
    canonical.rel = 'canonical'
    document.head.appendChild(canonical)
  }
  canonical.href = canonicalUrl
}

export { DEFAULT_TITLE, DEFAULT_DESCRIPTION, SITE_URL }
