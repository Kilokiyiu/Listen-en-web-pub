import { Preferences } from '@capacitor/preferences'
import { API_BASE } from '../config'
import { getAuth } from '../services/appSettings'

const VISITOR_KEY = 'app_visitor_id'
const ALLOWED = new Set([
  'add_word',
  'enter_review',
  'finish_review',
])

async function getVisitorId() {
  try {
    const { value } = await Preferences.get({ key: VISITOR_KEY })
    if (value) return value
    const id = crypto.randomUUID()
    await Preferences.set({ key: VISITOR_KEY, value: id })
    return id
  } catch {
    return crypto.randomUUID()
  }
}

/**
 * App 漏斗埋点（fire-and-forget）
 * @param {'add_word'|'enter_review'|'finish_review'} eventType
 * @param {string} [path]
 */
export async function trackEvent(eventType, path = '/app') {
  const type = String(eventType || '').toLowerCase()
  if (!ALLOWED.has(type)) return

  try {
    const { userId } = await getAuth()
    const visitorId = await getVisitorId()
    const payload = {
      eventType: type,
      path: path.startsWith('/') ? path.slice(0, 200) : `/${path}`.slice(0, 200),
      visitorId: String(visitorId).slice(0, 64),
      userId: userId || null,
    }

    const url = `${API_BASE}/api/identity/Analytics/Track`
    const body = JSON.stringify(payload)

    if (typeof navigator !== 'undefined' && navigator.sendBeacon && !API_BASE) {
      navigator.sendBeacon(url, new Blob([body], { type: 'application/json' }))
      return
    }

    fetch(url, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body,
      keepalive: true,
    }).catch(() => {})
  } catch {
    /* ignore */
  }
}
