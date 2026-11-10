const SITE_URL = 'https://your-domain.com'
const DEFAULT_TITLE =
  'ListenEase - 四六级备考网站_英语四级真题汇总与六级听力真题'
const DEFAULT_DESCRIPTION =
  'ListenEase 是免费四六级备考网站与英语刷题网站：英语四级真题汇总、四级往年试卷与六级听力真题在线练，大学英语六级考试卷 PDF 下载，考研阅读原文练习；含听力原文、六级作文真题整卷查阅、每日短文与词本。'
const DEFAULT_KEYWORDS =
  '四六级备考网站,英语四级真题汇总,六级听力真题,大学英语六级考试卷,免费四级刷题软件,四级往年试卷,大学四六级练习网站,英语刷题网站,英语六级听力,六级作文真题,考研英语二阅读原文,免费刷题,ListenEase'
const OG_IMAGE = `${SITE_URL}/og-cover.webp`

function upsertMeta (attr, key, content) {
  let el = document.querySelector(`meta[${attr}="${key}"]`)
  if (!el) {
    el = document.createElement('meta')
    el.setAttribute(attr, key)
    document.head.appendChild(el)
  }
  el.setAttribute('content', content)
}

export function setPageMeta ({ title, description, keywords, path = '/', noindex = false } = {}) {
  const pageTitle = title || DEFAULT_TITLE
  const pageDescription = description || DEFAULT_DESCRIPTION
  const pageKeywords = keywords || DEFAULT_KEYWORDS
  const canonicalUrl = `${SITE_URL}${path === '/' ? '/' : path}`

  document.title = pageTitle
  upsertMeta('name', 'description', pageDescription)
  upsertMeta('name', 'keywords', pageKeywords)
  upsertMeta('name', 'robots', noindex ? 'noindex, nofollow' : 'index, follow, max-image-preview:large')
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
