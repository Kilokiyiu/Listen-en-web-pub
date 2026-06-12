import request from './Request'

const CACHE_KEY = 'listenease_cache'
const CACHE_EXPIRE = 5 * 60 * 1000 // 缓存5分钟

// 获取缓存
const getCache = () => {
  try {
    const cache = localStorage.getItem(CACHE_KEY)
    if (cache) {
      const { data, timestamp } = JSON.parse(cache)
      if (Date.now() - timestamp < CACHE_EXPIRE) {
        return data
      }
    }
  } catch (e) {}
  return null
}

// 设置缓存
const setCache = (data) => {
  try {
    localStorage.setItem(CACHE_KEY, JSON.stringify({
      data,
      timestamp: Date.now()
    }))
  } catch (e) {}
}

// 获取所有分类
export const getCategories = () => {
  const cache = getCache()
  if (cache?.categories) {
    return Promise.resolve(cache.categories)
  }
  return request.get('/Listen/GetCategories').then(res => {
    const cacheData = getCache() || {}
    setCache({ ...cacheData, categories: res })
    return res
  })
}

// 获取分类下的所有试卷
export const getAlbumsByCategoryId = (categoryId) => {
  const cache = getCache()
  const cacheKey = `albums_${categoryId}`
  if (cache?.[cacheKey]) {
    return Promise.resolve(cache[cacheKey])
  }
  return request.get('/Listen/GetAlbumsByCategoryId', { params: { categoryId } }).then(res => {
    const cacheData = getCache() || {}
    setCache({ ...cacheData, [cacheKey]: res })
    return res
  })
}

// 获取试卷详情（含试卷/答案 PDF 地址）
export const getAlbumById = (albumId) =>
  request.get('/Listen/GetAlbumById', { params: { albumId } })

// 获取试卷下的所有题目（音频）
export const getEpisodesByAlbumId = (albumId) =>
  request.get('/Listen/GetEpisodesByAlbumId', { params: { albumId } })

// 清除缓存
export const clearCache = () => {
  localStorage.removeItem(CACHE_KEY)
}
