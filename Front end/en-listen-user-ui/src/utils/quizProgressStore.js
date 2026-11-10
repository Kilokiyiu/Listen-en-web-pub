/** 本会话内保留作答进度（刷新不丢；关标签页后清除） */

export function loadQuizProgress(key) {
  if (!key || typeof sessionStorage === 'undefined') return null
  try {
    const raw = sessionStorage.getItem(key)
    if (!raw) return null
    const data = JSON.parse(raw)
    return data && typeof data === 'object' ? data : null
  } catch {
    return null
  }
}

export function saveQuizProgress(key, data) {
  if (!key || typeof sessionStorage === 'undefined') return
  try {
    sessionStorage.setItem(key, JSON.stringify(data))
  } catch {
    /* quota / private mode */
  }
}

export function clearQuizProgress(key) {
  if (!key || typeof sessionStorage === 'undefined') return
  try {
    sessionStorage.removeItem(key)
  } catch {
    /* */
  }
}
