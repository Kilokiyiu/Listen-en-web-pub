<template>
  <div class="words-page">
    <header class="page-header">
      <div class="header-left">
        <div v-if="canSwitch" class="wordbook-switch">
          <button
            type="button"
            class="switch-btn"
            :class="{ active: !isLocal }"
            @click="switchWordbook('server')"
          >云端</button>
          <button
            type="button"
            class="switch-btn"
            :class="{ active: isLocal }"
            @click="switchWordbook('local')"
          >本地</button>
        </div>
        <span v-else class="mode-badge" :class="modeClass">{{ storageLabel }}</span>
      </div>
      <router-link to="/add" class="btn-add">+ 添加</router-link>
    </header>

    <div v-if="!isLocal && canSwitch" class="book-bar">
      <select
        class="book-select"
        :value="currentBookId || ''"
        :disabled="booksLoading || !wordBooks.length"
        @change="onBookChange($event.target.value)"
      >
        <option v-for="book in wordBooks" :key="book.id" :value="book.id">
          {{ book.name }} ({{ book.wordCount ?? 0 }})
        </option>
      </select>
      <div class="book-actions">
        <button type="button" class="book-btn" @click="openCreateBook">新建</button>
        <button type="button" class="book-btn" :disabled="!currentBook" @click="openRenameBook">重命名</button>
        <button
          type="button"
          class="book-btn danger"
          :disabled="!currentBook || currentBook.isDefault"
          @click="handleDeleteBook"
        >删除</button>
      </div>
    </div>

    <div class="stats" v-if="stats">
      <div class="stat-item">
        <div class="stat-value">{{ stats.totalWords }}</div>
        <div class="stat-label">总词数</div>
      </div>
      <div class="stat-item due">
        <div class="stat-value">{{ stats.dueCount }}</div>
        <div class="stat-label">待复习</div>
      </div>
      <div class="stat-item mastered">
        <div class="stat-value">{{ stats.masteredCount }}</div>
        <div class="stat-label">已掌握</div>
      </div>
      <div class="stat-item">
        <div class="stat-value">{{ stats.reviewLogsCount }}</div>
        <div class="stat-label">复习次数</div>
      </div>
    </div>

    <div class="toolbar">
      <input
        v-model="search"
        type="search"
        placeholder="搜索单词..."
        @keyup.enter="doSearch"
      />
      <button v-if="stats?.dueCount > 0" class="btn-review" @click="goReview()">
        复习 ({{ stats.dueCount }})
      </button>
      <button v-if="stats?.totalWords > 0" class="btn-free" @click="goReview('free')">
        自由复习
      </button>
    </div>

    <div v-if="loading" class="loading">加载中...</div>

    <div v-else-if="wordList.length === 0" class="empty">
      <p>{{ search ? '没有找到匹配的单词' : '还没有单词，点击右上角添加' }}</p>
    </div>

    <ul v-else class="word-list">
      <li v-for="word in wordList" :key="word.id" class="word-item">
        <div class="word-main">
          <span class="word-text">{{ word.word }}</span>
          <span class="status-tag" :class="getStatus(word).type">{{ getStatus(word).text }}</span>
        </div>
        <p class="definition">{{ word.definition || '暂无释义' }}</p>
        <button class="btn-delete" @click="onDelete(word)">删除</button>
      </li>
    </ul>

    <div v-if="total > pageSize" class="pagination">
      <button :disabled="page <= 1" @click="changePage(page - 1)">上一页</button>
      <span>{{ page }} / {{ totalPages }}</span>
      <button :disabled="page >= totalPages" @click="changePage(page + 1)">下一页</button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getWords, getStats, deleteWord, getStorageLabel } from '../services/wordService'
import {
  loadAndResolveWordBooks,
  createBook,
  renameBook,
  removeBook,
  setCurrentWordBookId,
} from '../services/wordBookService'
import {
  isLocalWordbook,
  isLoggedIn,
  setActiveWordbook,
} from '../services/appSettings'
import { WORDBOOK_TYPES } from '../config'
import { getReviewStatus } from '../utils/sm2'
import { showToast } from '../utils/toast'

const router = useRouter()
const wordList = ref([])
const stats = ref(null)
const loading = ref(false)
const search = ref('')
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)
const storageLabel = ref('')
const isLocal = ref(true)
const canSwitch = ref(false)
const wordBooks = ref([])
const currentBookId = ref(null)
const booksLoading = ref(false)

const totalPages = computed(() => Math.max(1, Math.ceil(total.value / pageSize.value)))
const modeClass = computed(() => (isLocal.value ? 'local' : 'server'))
const currentBook = computed(() =>
  wordBooks.value.find((b) => b.id === currentBookId.value) || null
)
const getStatus = (word) => getReviewStatus(word)

const refreshMeta = async () => {
  isLocal.value = await isLocalWordbook()
  canSwitch.value = await isLoggedIn()
  storageLabel.value = canSwitch.value ? '' : await getStorageLabel()
}

const ensureCloudBooks = async () => {
  if (isLocal.value || !canSwitch.value) {
    wordBooks.value = []
    currentBookId.value = null
    return
  }
  booksLoading.value = true
  try {
    const { books, currentId } = await loadAndResolveWordBooks()
    wordBooks.value = books
    currentBookId.value = currentId
  } catch (e) {
    showToast(typeof e === 'string' ? e : '加载单词本失败')
  } finally {
    booksLoading.value = false
  }
}

const switchWordbook = async (type) => {
  const toLocal = type === 'local'
  if (toLocal === isLocal.value) return
  await setActiveWordbook(toLocal ? WORDBOOK_TYPES.local : WORDBOOK_TYPES.server)
  page.value = 1
  search.value = ''
  await refreshMeta()
  await ensureCloudBooks()
  load()
}

const onBookChange = async (id) => {
  if (!id || id === currentBookId.value) return
  await setCurrentWordBookId(id)
  currentBookId.value = id
  page.value = 1
  search.value = ''
  load()
}

const openCreateBook = async () => {
  const name = prompt('新建单词本名称')?.trim()
  if (!name) return
  try {
    const book = await createBook({ name })
    showToast(`已创建「${book.name}」`)
    await ensureCloudBooks()
    page.value = 1
    search.value = ''
    load()
  } catch (e) {
    showToast(typeof e === 'string' ? e : '创建失败')
  }
}

const openRenameBook = async () => {
  if (!currentBook.value) return
  const name = prompt('重命名单词本', currentBook.value.name)?.trim()
  if (!name || name === currentBook.value.name) return
  try {
    await renameBook(currentBook.value.id, {
      name,
      description: currentBook.value.description || '',
    })
    showToast('已重命名')
    await ensureCloudBooks()
  } catch (e) {
    showToast(typeof e === 'string' ? e : '重命名失败')
  }
}

const handleDeleteBook = async () => {
  if (!currentBook.value || currentBook.value.isDefault) return
  if (
    !confirm(
      `确定删除「${currentBook.value.name}」吗？其中的单词会移到默认单词本。`
    )
  ) {
    return
  }
  try {
    await removeBook(currentBook.value.id)
    showToast('已删除')
    await ensureCloudBooks()
    page.value = 1
    search.value = ''
    load()
  } catch (e) {
    showToast(typeof e === 'string' ? e : '删除失败')
  }
}

const load = async () => {
  loading.value = true
  try {
    const [wordsRes, statsRes] = await Promise.all([
      getWords({ page: page.value, pageSize: pageSize.value, search: search.value }),
      getStats(),
    ])
    wordList.value = wordsRes.items || []
    total.value = wordsRes.total || 0
    stats.value = statsRes
    if (!isLocal.value && currentBookId.value) {
      const book = wordBooks.value.find((b) => b.id === currentBookId.value)
      if (book) book.wordCount = statsRes?.totalWords ?? book.wordCount
    }
  } catch (e) {
    showToast(typeof e === 'string' ? e : '加载失败')
  } finally {
    loading.value = false
  }
}

const doSearch = () => {
  page.value = 1
  load()
}

const changePage = (p) => {
  page.value = p
  load()
}

const onDelete = async (word) => {
  if (!confirm(`确定删除「${word.word}」？`)) return
  try {
    await deleteWord(word.id)
    showToast('已删除')
    load()
  } catch (e) {
    showToast(typeof e === 'string' ? e : '删除失败')
  }
}

const goReview = (mode) => {
  const query = {}
  if (mode) query.mode = mode
  if (!isLocal.value && currentBookId.value) query.wordBookId = currentBookId.value
  router.push({ name: 'review', query })
}

onMounted(async () => {
  await refreshMeta()
  await ensureCloudBooks()
  load()
})
</script>

<style scoped>
.words-page {
  padding: 16px;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.header-left {
  flex: 1;
  min-width: 0;
}

.wordbook-switch {
  display: inline-flex;
  background: var(--bg-card);
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 3px;
}

.switch-btn {
  padding: 6px 16px;
  border: none;
  border-radius: 8px;
  font-size: 13px;
  background: transparent;
  color: var(--text-secondary);
}

.switch-btn.active {
  background: var(--primary);
  color: #fff;
  font-weight: 500;
}

.mode-badge {
  font-size: 12px;
  padding: 2px 8px;
  border-radius: 20px;
}

.mode-badge.local {
  background: #e8f5e9;
  color: #2e7d32;
}

.mode-badge.server {
  background: #e3f2fd;
  color: #1565c0;
}

.btn-add {
  padding: 8px 14px;
  background: var(--primary);
  color: #fff;
  border-radius: 8px;
  text-decoration: none;
  font-size: 14px;
  font-weight: 500;
  flex-shrink: 0;
}

.book-bar {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-bottom: 14px;
  padding: 12px;
  background: var(--bg-card);
  border: 1px solid var(--border);
  border-radius: 12px;
}

.book-select {
  width: 100%;
  padding: 10px 12px;
  border: 1px solid var(--border);
  border-radius: 8px;
  font-size: 14px;
  background: var(--bg);
  color: var(--text);
}

.book-actions {
  display: flex;
  gap: 8px;
}

.book-btn {
  flex: 1;
  padding: 8px 0;
  border: 1px solid var(--border);
  border-radius: 8px;
  background: var(--bg);
  font-size: 13px;
  color: var(--text-secondary);
}

.book-btn:disabled {
  opacity: 0.4;
}

.book-btn.danger {
  color: #f56c6c;
  border-color: #fbc4c4;
}

.stats {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 8px;
  margin-bottom: 16px;
}

.stat-item {
  background: var(--bg-card);
  border-radius: 12px;
  padding: 12px 8px;
  text-align: center;
  border: 1px solid var(--border);
}

.stat-value {
  font-size: 20px;
  font-weight: 600;
}

.stat-item.due .stat-value { color: #e6a23c; }
.stat-item.mastered .stat-value { color: #67c23a; }

.stat-label {
  font-size: 11px;
  color: var(--text-muted);
  margin-top: 2px;
}

.toolbar {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-bottom: 16px;
}

.toolbar input {
  flex: 1;
  min-width: 140px;
  padding: 10px 12px;
  border: 1px solid var(--border);
  border-radius: 8px;
  font-size: 15px;
  background: var(--bg-card);
}

.btn-review, .btn-free {
  padding: 10px 14px;
  border: none;
  border-radius: 8px;
  font-size: 14px;
  white-space: nowrap;
}

.btn-review {
  background: #67c23a;
  color: #fff;
}

.btn-free {
  background: #e6a23c;
  color: #fff;
}

.loading, .empty {
  text-align: center;
  padding: 40px 0;
  color: var(--text-muted);
}

.word-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.word-item {
  background: var(--bg-card);
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 14px;
  margin-bottom: 10px;
  position: relative;
}

.word-main {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 6px;
}

.word-text {
  font-size: 18px;
  font-weight: 600;
}

.status-tag {
  font-size: 11px;
  padding: 2px 8px;
  border-radius: 10px;
}

.status-tag.new { background: #f0f0f0; color: #666; }
.status-tag.due { background: #fdf6ec; color: #e6a23c; }
.status-tag.mastered { background: #f0f9eb; color: #67c23a; }
.status-tag.learning { background: #ecf5ff; color: #409eff; }

.definition {
  font-size: 14px;
  color: var(--text-secondary);
  margin: 0;
  padding-right: 48px;
}

.btn-delete {
  position: absolute;
  right: 14px;
  top: 14px;
  background: none;
  border: none;
  color: #f56c6c;
  font-size: 13px;
}

.pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 16px;
  margin-top: 16px;
}

.pagination button {
  padding: 8px 16px;
  border: 1px solid var(--border);
  border-radius: 8px;
  background: var(--bg-card);
}

.pagination button:disabled {
  opacity: 0.4;
}
</style>
