import {
  getWordBooks,
  createWordBook,
  updateWordBook,
  deleteWordBook,
  getWordStats,
} from '../api/word'
import {
  getCurrentWordBookId,
  setCurrentWordBookId,
  isLocalWordbook,
} from './appSettings'
import {
  getLocalBooks,
  setCurrentLocalBookId,
  createLocalBook,
  renameLocalBook,
  deleteLocalBook,
} from './localWordStore'
import {
  FAVORITES_BOOK_ID,
  isFavoritesBookId,
  makeFavoritesBook,
} from '../config'

async function withFavoritesBook(books, currentId, favoriteCount) {
  const fav = makeFavoritesBook(favoriteCount)
  const list = [fav, ...(books || [])]
  if (isFavoritesBookId(currentId)) {
    return { books: list, currentId: FAVORITES_BOOK_ID, current: fav }
  }
  const current =
    list.find((b) => b.id === currentId) ||
    list.find((b) => b.isDefault) ||
    list.find((b) => !b.isFavorites) ||
    list[0]
  return { books: list, currentId: current?.id || null, current: current || null }
}

/**
 * 拉取当前模式（本地/云端）单词本列表，并解析选中本。
 */
export async function loadAndResolveWordBooks() {
  if (await isLocalWordbook()) {
    const { books, currentId, favoriteCount } = await getLocalBooks()
    return withFavoritesBook(books, currentId, favoriteCount)
  }

  const books = (await getWordBooks()) || []
  if (!books.length) {
    await setCurrentWordBookId(null)
    return { books: [], currentId: null, current: null }
  }

  let favoriteCount = 0
  try {
    const stats = await getWordStats({ favoritesOnly: true })
    favoriteCount = stats?.totalWords ?? 0
  } catch {
    favoriteCount = 0
  }

  const saved = await getCurrentWordBookId()
  const resolved = await withFavoritesBook(books, saved, favoriteCount)
  if (resolved.currentId) await setCurrentWordBookId(resolved.currentId)
  return resolved
}

export async function createBook(payload) {
  if (await isLocalWordbook()) {
    return createLocalBook(payload)
  }
  const book = await createWordBook(payload)
  await setCurrentWordBookId(book.id)
  return book
}

export async function renameBook(id, payload) {
  if (isFavoritesBookId(id)) throw new Error('收藏本不可重命名')
  if (await isLocalWordbook()) {
    return renameLocalBook(id, payload)
  }
  return updateWordBook(id, payload)
}

export async function removeBook(id, { deleteWords = false } = {}) {
  if (isFavoritesBookId(id)) throw new Error('收藏本不可删除')
  if (await isLocalWordbook()) {
    return deleteLocalBook(id, { deleteWords: !!deleteWords })
  }
  await deleteWordBook(id, {
    moveWordsToDefault: !deleteWords,
    deleteWords: !!deleteWords,
  })
  const current = await getCurrentWordBookId()
  if (current === id) await setCurrentWordBookId(null)
}

export async function selectBook(id) {
  if (await isLocalWordbook()) {
    return setCurrentLocalBookId(id)
  }
  await setCurrentWordBookId(id)
  return id
}

export { isFavoritesBookId, FAVORITES_BOOK_ID }

export { getWordBooks, setCurrentWordBookId, getCurrentWordBookId }
