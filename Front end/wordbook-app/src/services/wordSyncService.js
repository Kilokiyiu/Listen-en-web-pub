import { getUserWords, addUserWord } from '../api/word'
import { getCurrentWordBookId } from './appSettings'
import { getAllWords, importWords } from './localWordStore'

const PAGE_SIZE = 100

async function cloudListParams(extra = {}) {
  const wordBookId = await getCurrentWordBookId()
  return wordBookId ? { ...extra, wordBookId } : { ...extra }
}

/** 分页拉取当前云端单词本的全部单词 */
export async function fetchAllCloudWords() {
  const all = []
  let page = 1
  let total = Infinity
  const base = await cloudListParams()

  while (all.length < total) {
    const res = await getUserWords({ ...base, page, pageSize: PAGE_SIZE })
    const items = res?.items || []
    total = Number(res?.total) || all.length + items.length
    all.push(...items)
    if (items.length === 0 || items.length < PAGE_SIZE) break
    page += 1
  }

  return all
}

/**
 * 云端 → 本地：合并保存（已存在的单词跳过，空释义可补全）
 * 仅同步当前选中的云端单词本。
 */
export async function downloadCloudToLocal() {
  const cloudWords = await fetchAllCloudWords()
  if (cloudWords.length === 0) {
    return { total: 0, added: 0, skipped: 0, updated: 0 }
  }

  const result = await importWords(cloudWords)
  return { total: cloudWords.length, ...result }
}

/**
 * 本地 → 云端：上传尚未存在的单词到当前云端单词本
 */
export async function uploadLocalToCloud() {
  const wordBookId = await getCurrentWordBookId()
  const [localWords, cloudWords] = await Promise.all([getAllWords(), fetchAllCloudWords()])
  const cloudKeys = new Set(cloudWords.map((w) => (w.word || '').toLowerCase()))

  let added = 0
  let skipped = 0
  let failed = 0

  for (const item of localWords) {
    const key = (item.word || '').toLowerCase()
    if (!key) continue
    if (cloudKeys.has(key)) {
      skipped += 1
      continue
    }

    try {
      await addUserWord({
        word: item.word,
        definition: item.definition || '',
        example: item.example || '',
        ...(wordBookId ? { wordBookId } : {}),
      })
      cloudKeys.add(key)
      added += 1
    } catch {
      failed += 1
    }
  }

  return { total: localWords.length, added, skipped, failed }
}
