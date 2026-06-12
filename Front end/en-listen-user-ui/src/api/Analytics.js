const VISITOR_ID_KEY = 'listenease_visitor_id'

function getVisitorId() {
  let id = localStorage.getItem(VISITOR_ID_KEY)
  if (!id) {
    id = crypto.randomUUID()
    localStorage.setItem(VISITOR_ID_KEY, id)
  }
  return id
}

export function trackPageView(path) {
  if (!path || typeof path !== 'string') return

  const payload = {
    path: path.startsWith('/') ? path : `/${path}`,
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
