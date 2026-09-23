import {
  getWordBooks,
  createWordBook,
  updateWordBook,
  deleteWordBook,
} from '../api/word'
import { getCurrentWordBookId, setCurrentWordBookId } from './appSettings'

/**
 * 拉取云端单词本列表，并解析/持久化当前选中本。
 * 优先已保存 id → 默认本 → 第一本。
 */
export async function loadAndResolveWordBooks() {
  const books = (await getWordBooks()) || []
  if (!books.length) {
    await setCurrentWordBookId(null)
    return { books: [], currentId: null, current: null }
  }

  const saved = await getCurrentWordBookId()
  const match = saved ? books.find((b) => b.id === saved) : null
  const current = match || books.find((b) => b.isDefault) || books[0]
  await setCurrentWordBookId(current.id)
  return { books, currentId: current.id, current }
}

export async function createBook(payload) {
  const book = await createWordBook(payload)
  await setCurrentWordBookId(book.id)
  return book
}

export async function renameBook(id, payload) {
  return updateWordBook(id, payload)
}

export async function removeBook(id) {
  await deleteWordBook(id, { moveWordsToDefault: true })
  const current = await getCurrentWordBookId()
  if (current === id) await setCurrentWordBookId(null)
}

export { getWordBooks, setCurrentWordBookId, getCurrentWordBookId }
