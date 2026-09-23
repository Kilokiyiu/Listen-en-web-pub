const VISITOR_ID_KEY = 'app_visitor_id'

const ALLOWED_EVENTS = new Set([
  'page_view',
  'listen_complete',
  'article_read',
  'add_word',
  'enter_review',
  'finish_review',
])

function getVisitorId() {
  let id = localStorage.getItem(VISITOR_ID_KEY)
  if (!id) {
    id = crypto.randomUUID()
    localStorage.setItem(VISITOR_ID_KEY, id)
  }
  return id
}

/**
 * @param {string} eventType
 * @param {string} [path]
 */
export function trackEvent(eventType, path) {
  const type = (eventType || '').trim().toLowerCase()
  if (!ALLOWED_EVENTS.has(type)) return

  let resolvedPath = path
  if (!resolvedPath || typeof resolvedPath !== 'string') {
    resolvedPath = window.location.hash?.replace(/^#/, '') || window.location.pathname || '/'
  }
  if (!resolvedPath.startsWith('/')) resolvedPath = `/${resolvedPath}`
  if (resolvedPath.length > 200) resolvedPath = resolvedPath.slice(0, 200)

  const payload = {
    eventType: type,
    path: resolvedPath,
    visitorId: getVisitorId(),
    userId: localStorage.getItem('userId') || null,
  }

  const url = '/api/identity/Analytics/Track'
  const body = JSON.stringify(payload)

  if (navigator.sendBeacon) {
    const blob = new Blob([body], { type: 'application/json' })
    navigator.sendBeacon(url, blob)
    return
  }

  fetch(url, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body,
    keepalive: true,
  }).catch(() => {})
}

export function trackPageView(path) {
  trackEvent('page_view', path)
}
