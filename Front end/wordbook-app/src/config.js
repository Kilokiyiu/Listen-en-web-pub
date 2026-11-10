/** API 根地址，生产环境在 .env 中配置 VITE_API_BASE */
export const API_BASE = import.meta.env.VITE_API_BASE || ''

export const STORAGE_KEYS = {
  activeWordbook: 'wordbook_active_wordbook',
  offlineOnly: 'wordbook_offline_only',
  token: 'wordbook_token',
  userId: 'wordbook_userId',
  username: 'wordbook_username',
  localData: 'wordbook_local_data',
  /** 当前选中的云端单词本 id（与网站 currentWordBookId 对应） */
  currentWordBookId: 'wordbook_current_word_book_id',
}

export const WORDBOOK_TYPES = {
  local: 'local',
  server: 'server',
}

/** 虚拟「收藏」单词本（汇总所有 isFavorite 单词） */
export const FAVORITES_BOOK_ID = '__favorites__'

export function isFavoritesBookId(id) {
  return id === FAVORITES_BOOK_ID
}

export function makeFavoritesBook(wordCount = 0) {
  return {
    id: FAVORITES_BOOK_ID,
    name: '收藏',
    description: '复习时收藏的单词',
    isDefault: false,
    isFavorites: true,
    wordCount: Number(wordCount) || 0,
  }
}
