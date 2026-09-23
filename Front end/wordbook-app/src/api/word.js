import { wordApi } from './http'

export const getWordBooks = () => wordApi.get('/word-books')
export const createWordBook = (data) => wordApi.post('/word-books', data)
export const updateWordBook = (id, data) => wordApi.put(`/word-books/${id}`, data)
export const deleteWordBook = (id, params) => wordApi.delete(`/word-books/${id}`, { params })

export const getUserWords = (params) => wordApi.get('/user-words', { params })
export const addUserWord = (data) => wordApi.post('/user-words', data)
export const deleteUserWord = (id) => wordApi.delete(`/user-words/${id}`)
export const moveUserWord = (id, wordBookId) =>
  wordApi.post(`/user-words/${id}/move`, { wordBookId })
export const getDueWords = (params) => wordApi.get('/user-words/due', { params })
export const getRandomWords = (params) => wordApi.get('/user-words/random', { params })
export const reviewWord = (id, quality) => wordApi.post(`/user-words/${id}/review`, { quality })
export const getWordStats = (params) => wordApi.get('/user-words/stats', { params })

/** 查询单词/短语/句子（后端 xxapi，与网站共用） */
export const queryEnglishWord = (word) =>
  wordApi.get('/dictionary', { params: { word: String(word || '').trim() } })

export function isValidEnglishQuery(text) {
  const query = text?.trim()
  if (!query || query.length > 500) return false
  if (!/[a-zA-Z]/.test(query)) return false
  if (/[\u4e00-\u9fff]/.test(query)) return false
  return /^[a-zA-Z0-9\s\-'.,!?;:()"\/]+$/.test(query)
}
