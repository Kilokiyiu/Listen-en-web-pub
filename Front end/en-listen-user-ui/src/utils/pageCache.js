/**
 * 简单页面数据缓存：返回同一页时先展示缓存，再后台刷新。
 */
const store = new Map()

export function getPageCache(key) {
  const hit = store.get(key)
  if (!hit) return null
  if (hit.expireAt && Date.now() > hit.expireAt) {
    store.delete(key)
    return null
  }
  return hit.data
}

export function setPageCache(key, data, ttlMs = 5 * 60 * 1000) {
  store.set(key, {
    data,
    expireAt: ttlMs > 0 ? Date.now() + ttlMs : 0,
  })
}

export function clearPageCache(prefix = '') {
  if (!prefix) {
    store.clear()
    return
  }
  for (const key of [...store.keys()]) {
    if (key.startsWith(prefix)) store.delete(key)
  }
}
