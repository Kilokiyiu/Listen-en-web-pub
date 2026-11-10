<template>
  <div class="words-page">
    <!-- 书架页：只显示词本 -->
    <template v-if="isShelfPage">
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
        <div class="header-actions">
          <button type="button" class="btn-import" @click="openImportPanel">导入</button>
        </div>
      </header>

      <section class="myshelf">
        <div class="myshelf__head">
          <div>
            <p class="myshelf__label">{{ isLocal ? '本地书架' : '云端书架' }}</p>
            <p class="myshelf__tip">点封面进入单词；下方可重命名或删除，顶部可导出</p>
          </div>
          <div class="myshelf__tools">
            <button type="button" class="book-btn" @click="openCreateBook">新建</button>
            <button type="button" class="book-btn" @click="openSharePanel">
              {{ isLocal ? '导出' : '分享' }}
            </button>
            <button
              v-if="canSwitch"
              type="button"
              class="book-btn"
              @click="openClaimPanel"
            >分享码</button>
          </div>
        </div>

        <div v-if="booksLoading" class="myshelf__hint">加载词本...</div>
        <div v-else class="shelf">
          <article
            v-for="(book, index) in wordBooks"
            :key="book.id"
            class="book"
            :class="[book.isFavorites ? 'tone-fav' : `tone-${index % 5}`]"
          >
            <span class="spine" aria-hidden="true"></span>
            <div class="cover" role="button" tabindex="0" @click="openBook(book.id)" @keyup.enter="openBook(book.id)">
              <div class="top">
                <span class="badge">{{
                  book.isFavorites ? '收藏' : (book.isDefault ? '默认' : (isLocal ? '本地' : '我的'))
                }}</span>
              </div>
              <h3>{{ book.name }}</h3>
              <p class="count">
                <strong>{{ book.wordCount ?? 0 }}</strong> 词
              </p>
              <div v-if="!book.isFavorites" class="book-card-actions" @click.stop>
                <button type="button" class="ghost" @click="openRenameBook(book)">重命名</button>
                <button type="button" class="danger-ghost" @click="handleDeleteBook(book)">删除</button>
              </div>
              <p v-else class="book-card-note">复习时点星加入</p>
            </div>
          </article>

          <button type="button" class="book book--add" @click="openCreateBook">
            <span class="spine" aria-hidden="true"></span>
            <div class="cover cover--add">
              <span class="add-plus">+</span>
              <span>新建词本</span>
            </div>
          </button>
        </div>
      </section>
    </template>

    <!-- 单词页：进入某本词本后 -->
    <section v-else class="open-book open-book--page">
      <header class="page-header">
        <div class="header-left open-book__title-wrap">
          <p class="open-book__label">{{
            isFavoritesBook ? '收藏词本' : (isLocal ? '本地词本' : '云端词本')
          }}</p>
          <h2 class="open-book__title">{{ currentBook?.name || '单词本' }}</h2>
        </div>
        <div v-if="!isFavoritesBook" class="header-actions">
          <button type="button" class="btn-import" @click="openImportPanel">导入</button>
          <router-link to="/add" class="btn-add">+ 添加</router-link>
        </div>
      </header>

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
        <button
          v-if="!isFavoritesBook"
          type="button"
          class="btn-import-tool"
          @click="openImportPanel"
        >
          一键导入
        </button>
        <button
          type="button"
          class="btn-batch"
          :class="{ active: selectMode }"
          @click="toggleSelectMode"
        >
          {{ selectMode ? '取消多选' : '批量删除' }}
        </button>
        <button v-if="stats?.dueCount > 0" class="btn-review" @click="goReview()">
          复习 ({{ stats.dueCount }})
        </button>
        <button v-if="stats?.totalWords > 0" class="btn-free" @click="goReview('free')">
          自由复习
        </button>
      </div>

      <div class="mark-filter" role="group" aria-label="按复习标记筛选">
        <button
          v-for="opt in reviewMarkOptions"
          :key="opt.value || 'all'"
          type="button"
          class="mark-chip"
          :class="[{ active: reviewMark === opt.value }, opt.cls]"
          @click="setReviewMark(opt.value)"
        >{{ opt.label }}</button>
      </div>

      <div v-if="selectMode" class="batch-toolbar">
        <button type="button" class="batch-tool-btn" @click="toggleSelectPage">
          {{ isPageAllSelected ? '取消本页' : '全选本页' }}
        </button>
        <button
          type="button"
          class="batch-tool-btn danger"
          :disabled="!selectedIds.length || batchDeleting"
          @click="batchDeleteSelected"
        >
          {{ batchDeleting ? '删除中...' : `删除所选 (${selectedIds.length})` }}
        </button>
      </div>

      <div v-if="loading" class="loading">加载中...</div>

      <div v-else-if="wordList.length === 0" class="empty">
        <p>{{
          search || reviewMark
            ? '没有找到匹配的单词'
            : (isFavoritesBook ? '还没有收藏单词，复习时点星加入' : '还没有单词')
        }}</p>
        <div v-if="!search && !reviewMark && !isFavoritesBook" class="empty-actions">
          <button type="button" class="empty-btn" @click="openImportPanel">一键导入文件/粘贴</button>
          <router-link v-if="!isLocal" to="/packs" class="empty-link">从词库领取</router-link>
          <button v-if="canSwitch" type="button" class="empty-link-btn" @click="openClaimPanel">用分享码导入词本</button>
        </div>
      </div>

      <ul v-else class="word-list">
        <li
          v-for="word in wordList"
          :key="word.id"
          class="word-item"
          :class="{ selected: selectMode && selectedIdSet.has(word.id) }"
          @click="selectMode ? toggleWordSelected(word.id) : undefined"
        >
          <div class="word-main">
            <div class="word-title">
              <input
                v-if="selectMode"
                type="checkbox"
                class="word-check"
                :checked="selectedIdSet.has(word.id)"
                @click.stop
                @change="toggleWordSelected(word.id)"
              />
              <span class="word-text">{{ word.word }}</span>
              <span v-if="word.isFavorite" class="fav-tag" title="已收藏">★</span>
              <span class="status-tag" :class="getStatus(word).type">{{ getStatus(word).text }}</span>
            </div>
            <button
              v-if="!selectMode"
              type="button"
              class="btn-delete"
              @click="onDelete(word)"
            >删除</button>
          </div>
          <p class="definition">{{ word.definition || '暂无释义' }}</p>
          <p v-if="word.example" class="example">{{ word.example }}</p>
        </li>
      </ul>

      <div v-if="total > pageSize" class="pagination">
        <button :disabled="page <= 1" @click="changePage(page - 1)">上一页</button>
        <span>{{ page }} / {{ totalPages }}</span>
        <button :disabled="page >= totalPages" @click="changePage(page + 1)">下一页</button>
      </div>
    </section>

    <!-- 面板挂到 body，并盖过底部导航 -->
    <Teleport to="body">
      <div v-if="panel === 'import'" class="panel-mask" @click.self="closePanel">
        <div class="panel panel--import">
          <div class="panel-scroll">
            <h3>一键导入</h3>
            <div class="import-guide">
              <p class="import-guide__title">导入格式说明（每次导入请先对照）</p>
              <div
                v-for="(item, idx) in importFormatGuide"
                :key="idx"
                class="import-guide__item"
                :class="{ 'is-warn': idx === importFormatGuide.length - 1 }"
              >
                <strong>{{ item.title }}</strong>
                <pre class="import-guide__example">{{ item.example }}</pre>
                <p>{{ item.note }}</p>
              </div>
              <p class="panel-hint">也支持 PDF / Word(.docx) / JSON。格式乱可点「AI 整理」（需登录）。</p>
            </div>

            <div class="import-target">
              <p class="import-target__label">导入到</p>
              <div class="import-target__modes">
                <label class="import-target__mode">
                  <input v-model="importTargetMode" type="radio" value="existing" />
                  已有单词本
                </label>
                <label class="import-target__mode">
                  <input v-model="importTargetMode" type="radio" value="new" />
                  新建单词本
                </label>
              </div>
              <select
                v-if="importTargetMode === 'existing'"
                v-model="importBookId"
                class="book-select import-target__select"
              >
                <option v-for="book in realWordBooks" :key="book.id" :value="book.id">
                  {{ book.name }} ({{ book.wordCount ?? 0 }})
                </option>
              </select>
              <input
                v-else
                v-model="importNewBookName"
                class="panel-input"
                type="text"
                maxlength="40"
                placeholder="新单词本名称，如：四级核心词"
              />
            </div>

            <input
              ref="importFileRef"
              type="file"
              class="file-input"
              accept=".json,.txt,.pdf,.docx,application/json,text/plain,application/pdf,application/vnd.openxmlformats-officedocument.wordprocessingml.document"
              @change="onImportFile"
            />
            <button type="button" class="panel-btn" :disabled="ioBusy || aiBusy" @click="importFileRef?.click()">
              选择文件（PDF / Word）
            </button>
            <textarea
              v-model="importPaste"
              class="panel-textarea"
              rows="8"
              placeholder="推荐：abandon | 放弃&#10;格式乱也没关系，可点「AI 整理」"
              @input="onImportPasteInput"
            />
            <p v-if="pendingImportWords?.length" class="panel-hint panel-hint--accent">
              已准备 {{ pendingImportWords.length }} 个词（预览仅显示前 200 条，AI 整理与导入会使用全部）
            </p>
            <p class="panel-hint">音标、词性会写入释义。AI 整理需登录，每天限用 1 次。</p>
          </div>
          <div class="panel-actions col">
            <button type="button" class="panel-btn" :disabled="ioBusy || aiBusy" @click="runAiOrganize">
              {{ aiBusy ? 'AI 整理中...' : 'AI 整理' }}
            </button>
            <div class="panel-actions-row">
              <button type="button" class="panel-btn ghost" @click="closePanel">取消</button>
              <button
                type="button"
                class="panel-btn primary"
                :disabled="ioBusy || aiBusy"
                @click="submitImportPaste"
              >
                {{ ioBusy ? '导入中...' : '开始导入' }}
              </button>
            </div>
          </div>
        </div>
      </div>

      <div v-if="panel === 'share'" class="panel-mask" @click.self="closePanel">
        <div class="panel">
          <div class="panel-scroll">
            <h3>{{ isLocal ? '导出 / 分享文件' : '分享单词本' }}</h3>
            <div class="import-target">
              <p class="import-target__label">{{ isLocal ? '导出单词本' : '分享单词本' }}</p>
              <select
                v-model="shareBookId"
                class="book-select import-target__select"
                @change="shareInfo = null"
              >
                <option v-for="book in realWordBooks" :key="book.id" :value="book.id">
                  {{ book.name }} ({{ book.wordCount ?? 0 }})
                </option>
              </select>
              <p v-if="!realWordBooks.length" class="panel-hint">暂无可分享的单词本，请先新建</p>
            </div>
            <template v-if="!isLocal">
              <p class="panel-hint">生成分享码发给好友，对方点「用分享码导入」即可导入单词；也可导出文件。</p>
              <div v-if="shareInfo" class="share-box">
                <div class="share-code">{{ shareInfo.code }}</div>
                <p>{{ shareInfo.title }} · {{ shareInfo.wordCount }} 词</p>
              </div>
              <button
                type="button"
                class="panel-btn primary"
                :disabled="ioBusy || !shareBookId"
                @click="generateShare"
              >
                {{ ioBusy ? '生成中...' : shareInfo ? '重新生成分享码' : '生成分享码' }}
              </button>
              <button
                v-if="shareInfo"
                type="button"
                class="panel-btn"
                :disabled="ioBusy"
                @click="shareCodeOut"
              >
                系统分享 / 复制
              </button>
            </template>
          </div>
          <div class="panel-actions col">
            <button
              type="button"
              class="panel-btn"
              :disabled="ioBusy || !shareBookId"
              @click="exportFile('json')"
            >
              导出 JSON
            </button>
            <button
              type="button"
              class="panel-btn"
              :disabled="ioBusy || !shareBookId"
              @click="exportFile('txt')"
            >
              导出 TXT
            </button>
            <button type="button" class="panel-btn ghost" @click="closePanel">关闭</button>
          </div>
        </div>
      </div>

      <div v-if="panel === 'claim'" class="panel-mask" @click.self="closePanel">
        <div class="panel">
          <div class="panel-scroll">
            <h3>用分享码导入词本</h3>
            <p class="panel-hint">输入好友发来的 6 位分享码，即可把对方的单词导入为一本新词本。</p>
            <input
              v-model="claimCode"
              class="panel-input"
              maxlength="16"
              placeholder="输入 6 位分享码"
              @keyup.enter="previewClaim"
            />
            <div v-if="claimInfo" class="claim-box">
              <strong>{{ claimInfo.title }}</strong>
              <span> · {{ claimInfo.wordCount }} 词</span>
            </div>
          </div>
          <div class="panel-actions">
            <button type="button" class="panel-btn ghost" @click="closePanel">取消</button>
            <button type="button" class="panel-btn" :disabled="ioBusy" @click="previewClaim">预览</button>
            <button
              type="button"
              class="panel-btn primary"
              :disabled="ioBusy || !claimInfo"
              @click="submitClaim"
            >
              {{ ioBusy ? '导入中...' : '导入到新词本' }}
            </button>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getWords, getStats, deleteWord, deleteWords, getStorageLabel } from '../services/wordService'
import {
  loadAndResolveWordBooks,
  createBook,
  renameBook,
  removeBook,
  selectBook,
  setCurrentWordBookId,
} from '../services/wordBookService'
import {
  isLocalWordbook,
  isLoggedIn,
  setActiveWordbook,
} from '../services/appSettings'
import { WORDBOOK_TYPES, isFavoritesBookId } from '../config'
import { getReviewStatus } from '../utils/sm2'
import { confirmDialog, promptDialog } from '../utils/dialog'
import { showToast } from '../utils/toast'
import {
  parseWordFile,
  parseImportFile,
  importIntoActiveWordbook,
  serializeWordsForAi,
  exportActiveWordbook,
  shareShareCodeText,
  IMPORT_FORMAT_GUIDE,
} from '../services/wordImportExport'
import {
  shareWordBook,
  getSharedWordBook,
  claimSharedWordBook,
  aiOrganizeUserWords,
} from '../api/word'

const route = useRoute()
const router = useRouter()
const isShelfPage = computed(() => route.name !== 'book')
const wordList = ref([])
const stats = ref(null)
const loading = ref(false)
const search = ref('')
const reviewMark = ref('')
const reviewMarkOptions = [
  { value: '', label: '全部', cls: '' },
  { value: 'forget', label: '忘记', cls: 'forget' },
  { value: 'fuzzy', label: '模糊', cls: 'fuzzy' },
  { value: 'remember', label: '记住', cls: 'remember' },
  { value: 'unreviewed', label: '未复习', cls: 'unreviewed' },
  { value: 'favorite', label: '收藏', cls: 'favorite' },
]
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)
const storageLabel = ref('')
const isLocal = ref(true)
const canSwitch = ref(false)
const wordBooks = ref([])
const currentBookId = ref(null)
const booksLoading = ref(false)

const panel = ref('')
const ioBusy = ref(false)
const aiBusy = ref(false)
const importPaste = ref('')
const importFileRef = ref(null)
const importFormatGuide = IMPORT_FORMAT_GUIDE
const importTargetMode = ref('existing') // existing | new
const importBookId = ref(null)
const importNewBookName = ref('')
const pendingImportWords = ref(null)
const shareBookId = ref(null)
const shareInfo = ref(null)
const claimCode = ref('')
const claimInfo = ref(null)

const selectMode = ref(false)
const selectedIds = ref([])
const batchDeleting = ref(false)

const totalPages = computed(() => Math.max(1, Math.ceil(total.value / pageSize.value)))
const modeClass = computed(() => (isLocal.value ? 'local' : 'server'))
const currentBook = computed(() =>
  wordBooks.value.find((b) => b.id === currentBookId.value) || null
)
const isFavoritesBook = computed(() => isFavoritesBookId(currentBookId.value))
const realWordBooks = computed(() => wordBooks.value.filter((b) => !b.isFavorites))
const selectedIdSet = computed(() => new Set(selectedIds.value))
const isPageAllSelected = computed(() => {
  if (!wordList.value.length) return false
  return wordList.value.every((w) => selectedIdSet.value.has(w.id))
})
const getStatus = (word) => getReviewStatus(word)

const refreshMeta = async () => {
  isLocal.value = await isLocalWordbook()
  canSwitch.value = await isLoggedIn()
  storageLabel.value = canSwitch.value ? '' : await getStorageLabel()
}

const ensureBooks = async () => {
  // 云端需登录；本地始终可有书架
  if (!isLocal.value && !canSwitch.value) {
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
  await ensureBooks()
  if (!isShelfPage.value) load()
}

const openBook = async (id) => {
  if (!id) return
  await selectBook(id)
  currentBookId.value = id
  page.value = 1
  search.value = ''
  reviewMark.value = ''
  selectMode.value = false
  selectedIds.value = []
  router.push({ name: 'book' })
}

const openCreateBook = async () => {
  const name = await promptDialog('新建单词本', { title: '新建', confirmText: '创建' })
  if (!name) return
  try {
    const book = await createBook({ name })
    showToast(`已创建「${book.name}」`)
    await ensureBooks()
    // 书架页新建后留在书架；单词页新建后进入该本
    if (!isShelfPage.value) await openBook(book.id)
  } catch (e) {
    showToast(typeof e === 'string' ? e : '创建失败')
  }
}

const openRenameBook = async (book) => {
  const target = book || currentBook.value
  if (!target || target.isFavorites) return
  const name = await promptDialog('重命名单词本', {
    title: '重命名',
    value: target.name,
    confirmText: '保存',
  })
  if (!name || name === target.name) return
  try {
    await renameBook(target.id, {
      name,
      description: target.description || '',
    })
    showToast('已重命名')
    await ensureBooks()
  } catch (e) {
    showToast(typeof e === 'string' ? e : '重命名失败')
  }
}

const afterDeleteBook = async () => {
  selectMode.value = false
  selectedIds.value = []
  if (isShelfPage.value) {
    await ensureBooks()
  } else {
    router.replace({ name: 'words' })
  }
}

const handleDeleteBook = async (book) => {
  const target = book || currentBook.value
  if (!target) return

  // 默认本：删除后自动新建空白默认本
  if (target.isDefault) {
    const ok = await confirmDialog(
      `删除默认单词本「${target.name}」？\n其中的单词将一并删除，并自动新建一本空白默认单词本。`,
      {
        title: '删除默认单词本',
        confirmText: '删除并重建',
        cancelText: '取消',
        danger: true,
      }
    )
    if (!ok) return
    try {
      await removeBook(target.id, { deleteWords: true })
      showToast('已删除，并新建空白默认单词本')
      await afterDeleteBook()
    } catch (e) {
      showToast(typeof e === 'string' ? e : '删除失败')
    }
    return
  }

  const migrate = await confirmDialog(
    `删除「${target.name}」？\n\n「迁移到默认本」：保留单词\n「连同单词删除」：永久清空这些词`,
    {
      title: '删除单词本',
      confirmText: '迁移到默认本',
      cancelText: '连同单词删除',
      danger: true,
    }
  )
  if (migrate) {
    try {
      await removeBook(target.id, { deleteWords: false })
      showToast('词本已删除，单词已移到默认本')
      await afterDeleteBook()
    } catch (e) {
      showToast(typeof e === 'string' ? e : '删除失败')
    }
    return
  }

  const wipe = await confirmDialog(
    `将永久删除「${target.name}」及其全部单词，不可恢复。`,
    {
      title: '连同单词删除',
      confirmText: '永久删除',
      cancelText: '返回',
      danger: true,
    }
  )
  if (!wipe) return
  try {
    await removeBook(target.id, { deleteWords: true })
    showToast('词本及单词已删除')
    await afterDeleteBook()
  } catch (e) {
    showToast(typeof e === 'string' ? e : '删除失败')
  }
}

const load = async () => {
  loading.value = true
  try {
    const [wordsRes, statsRes] = await Promise.all([
      getWords({
        page: page.value,
        pageSize: pageSize.value,
        search: search.value,
        reviewMark: reviewMark.value || undefined,
      }),
      getStats(),
    ])
    wordList.value = wordsRes.items || []
    total.value = wordsRes.total || 0
    stats.value = statsRes
    if (currentBookId.value) {
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
  selectedIds.value = []
  load()
}

const setReviewMark = (value) => {
  reviewMark.value = reviewMark.value === value ? '' : value
  page.value = 1
  selectedIds.value = []
  load()
}

const changePage = (p) => {
  page.value = p
  load()
}

const toggleSelectMode = () => {
  selectMode.value = !selectMode.value
  selectedIds.value = []
}

const toggleWordSelected = (id) => {
  const set = new Set(selectedIds.value)
  if (set.has(id)) set.delete(id)
  else set.add(id)
  selectedIds.value = [...set]
}

const toggleSelectPage = () => {
  if (isPageAllSelected.value) {
    const pageIds = new Set(wordList.value.map((w) => w.id))
    selectedIds.value = selectedIds.value.filter((id) => !pageIds.has(id))
  } else {
    const set = new Set(selectedIds.value)
    wordList.value.forEach((w) => set.add(w.id))
    selectedIds.value = [...set]
  }
}

const onDelete = async (word) => {
  const ok = await confirmDialog(`删除「${word.word}」？`, {
    title: '删除单词',
    confirmText: '删除',
    danger: true,
  })
  if (!ok) return
  try {
    await deleteWord(word.id)
    selectedIds.value = selectedIds.value.filter((x) => x !== word.id)
    showToast('已删除')
    load()
  } catch (e) {
    showToast(typeof e === 'string' ? e : '删除失败')
  }
}

const batchDeleteSelected = async () => {
  const ids = [...selectedIds.value]
  if (!ids.length) return
  const ok = await confirmDialog(`确定删除选中的 ${ids.length} 个单词？不可恢复。`, {
    title: '批量删除',
    confirmText: '删除',
    danger: true,
  })
  if (!ok) return
  batchDeleting.value = true
  try {
    const res = await deleteWords(ids)
    showToast(`已删除 ${res?.deleted ?? ids.length} 个单词`)
    selectedIds.value = []
    selectMode.value = false
    load()
  } catch (e) {
    showToast(typeof e === 'string' ? e : '批量删除失败')
  } finally {
    batchDeleting.value = false
  }
}

const goReview = (mode) => {
  const query = {}
  if (mode) query.mode = mode
  if (currentBookId.value) query.wordBookId = currentBookId.value
  router.push({ name: 'review', query })
}

const closePanel = () => {
  panel.value = ''
  ioBusy.value = false
}

const openImportPanel = () => {
  importPaste.value = ''
  pendingImportWords.value = null
  importTargetMode.value = 'existing'
  importBookId.value =
    (!isFavoritesBookId(currentBookId.value) && currentBookId.value) ||
    realWordBooks.value[0]?.id ||
    null
  importNewBookName.value = ''
  panel.value = 'import'
}

const openSharePanel = () => {
  shareInfo.value = null
  shareBookId.value =
    (!isFavoritesBookId(currentBookId.value) && currentBookId.value) ||
    realWordBooks.value[0]?.id ||
    null
  panel.value = 'share'
}

const ensureShareBookSelected = async () => {
  if (!shareBookId.value) throw new Error('请选择要分享的单词本')
  if (isFavoritesBookId(shareBookId.value)) {
    throw new Error('收藏本请用导出文件分享')
  }
  await selectBook(shareBookId.value)
  currentBookId.value = shareBookId.value
  return shareBookId.value
}

const openClaimPanel = () => {
  if (!canSwitch.value) {
    showToast('请先登录后再用分享码导入')
    return
  }
  claimCode.value = ''
  claimInfo.value = null
  panel.value = 'claim'
}

const onImportPasteInput = () => {
  // 用户手动改预览文本后，不再沿用旧的全量 pending 列表
  pendingImportWords.value = null
}

const resolveImportTargetBookId = async () => {
  if (importTargetMode.value === 'new') {
    const name = (importNewBookName.value || '').trim()
    if (!name) throw new Error('请填写新建单词本名称')
    const book = await createBook({ name })
    await ensureBooks()
    return book.id
  }
  if (!importBookId.value) throw new Error('请选择要导入的单词本')
  return importBookId.value
}

const runImportWords = async (words, { wordBookId } = {}) => {
  ioBusy.value = true
  try {
    const r = await importIntoActiveWordbook(words, { wordBookId })
    const failPart = r.failed ? `，失败 ${r.failed}` : ''
    showToast(`导入完成：新增 ${r.added}，跳过 ${r.skipped}${failPart}`)
    closePanel()
    page.value = 1
    if (wordBookId) {
      await selectBook(wordBookId)
      currentBookId.value = wordBookId
    }
    await ensureBooks()
    if (isShelfPage.value && (wordBookId || currentBookId.value)) {
      await openBook(wordBookId || currentBookId.value)
    } else {
      load()
    }
  } catch (e) {
    showToast(typeof e === 'string' ? e : e?.message || '导入失败')
  } finally {
    ioBusy.value = false
  }
}

const fillImportPreview = (words, label = '') => {
  pendingImportWords.value = words
  importPaste.value = words
    .slice(0, 200)
    .map((w) => {
      const def = w.definition || ''
      const ex = w.example || ''
      if (ex) return `${w.word} | ${def} | ${ex}`
      if (def) return `${w.word} | ${def}`
      return w.word
    })
    .join('\n')
  if (words.length > 200) {
    const tip = label ? `${label}结果` : ''
    importPaste.value += `\n# …共 ${words.length} 词，点「开始导入」将导入全部${tip}`
  }
}

const onImportFile = async (event) => {
  const file = event.target?.files?.[0]
  event.target.value = ''
  if (!file) return
  ioBusy.value = true
  try {
    const words = await parseImportFile(file)
    const lower = (file.name || '').toLowerCase()
    if (lower.endsWith('.pdf') || lower.endsWith('.docx') || lower.endsWith('.json')) {
      fillImportPreview(words)
    } else {
      const { readFileAsText } = await import('../services/wordImportExport')
      importPaste.value = await readFileAsText(file)
      // txt 也保留全量解析结果，避免 AI 整理只吃到预览
      pendingImportWords.value = words
    }
    showToast(`已解析 ${words.length} 个词，确认后点「开始导入」`)
  } catch (e) {
    pendingImportWords.value = null
    showToast(typeof e === 'string' ? e : e?.message || '文件解析失败')
  } finally {
    ioBusy.value = false
  }
}

const runAiOrganize = async () => {
  if (!canSwitch.value) {
    showToast('请先登录后再使用 AI 整理')
    return
  }
  // 优先用全量 pending，避免预览截断导致只整理 200 词
  const text = pendingImportWords.value?.length
    ? serializeWordsForAi(pendingImportWords.value)
    : (importPaste.value || '').trim()
  if (!text) {
    showToast('请先粘贴内容或选择文件')
    return
  }
  const expected = pendingImportWords.value?.length || 0
  aiBusy.value = true
  try {
    const res = await aiOrganizeUserWords(text)
    const entries = res?.entries || []
    if (!entries.length) {
      showToast('AI 未整理出有效词条')
      return
    }
    fillImportPreview(entries, 'AI 整理')
    if (expected > 0 && entries.length < expected) {
      showToast(`AI 已整理 ${entries.length}/${expected} 个词，请确认后导入`)
    } else {
      showToast(`AI 已整理 ${entries.length} 个词，请确认后导入`)
    }
  } catch (e) {
    showToast(typeof e === 'string' ? e : e?.message || 'AI 整理失败')
  } finally {
    aiBusy.value = false
  }
}

const submitImportPaste = async () => {
  let words
  try {
    if (pendingImportWords.value?.length) {
      words = pendingImportWords.value
    } else {
      words = parseWordFile(importPaste.value)
    }
  } catch (e) {
    showToast(typeof e === 'string' ? e : e?.message || '解析失败')
    return
  }

  let targetBookId = null
  let targetLabel = '当前单词本'
  try {
    if (importTargetMode.value === 'new') {
      const name = (importNewBookName.value || '').trim()
      if (!name) {
        showToast('请填写新建单词本名称')
        return
      }
      targetLabel = `新单词本「${name}」`
    } else {
      const book = wordBooks.value.find((b) => b.id === importBookId.value)
      if (!book) {
        showToast('请选择要导入的单词本')
        return
      }
      targetLabel = `「${book.name}」`
    }
  } catch (e) {
    showToast(typeof e === 'string' ? e : e?.message || '请选择导入目标')
    return
  }

  const ok = await confirmDialog(`导入 ${words.length} 个词到${targetLabel}？`, { title: '确认导入' })
  if (!ok) return

  try {
    targetBookId = await resolveImportTargetBookId()
  } catch (e) {
    showToast(typeof e === 'string' ? e : e?.message || '无法确定导入目标')
    return
  }

  pendingImportWords.value = null
  await runImportWords(words, { wordBookId: targetBookId })
}

const exportFile = async (format) => {
  ioBusy.value = true
  try {
    await ensureShareBookSelected()
    const r = await exportActiveWordbook(format)
    showToast(`已导出 ${r.count} 个单词`)
  } catch (e) {
    showToast(typeof e === 'string' ? e : e?.message || '导出失败')
  } finally {
    ioBusy.value = false
  }
}

const generateShare = async () => {
  if (isLocal.value) {
    showToast('本地词本请使用导出文件分享')
    return
  }
  ioBusy.value = true
  try {
    const bookId = await ensureShareBookSelected()
    shareInfo.value = await shareWordBook(bookId)
    showToast('分享码已生成')
  } catch (e) {
    showToast(typeof e === 'string' ? e : e?.message || '生成失败')
  } finally {
    ioBusy.value = false
  }
}

const shareCodeOut = async () => {
  if (!shareInfo.value) return
  try {
    const r = await shareShareCodeText(shareInfo.value)
    if (r.mode === 'clipboard') showToast('已复制到剪贴板')
    else if (r.mode === 'text') showToast(`分享码：${shareInfo.value.code}`)
  } catch (e) {
    showToast(typeof e === 'string' ? e : e?.message || '分享失败')
  }
}

const previewClaim = async () => {
  const code = claimCode.value.trim()
  if (!code) {
    showToast('请输入分享码')
    return
  }
  ioBusy.value = true
  try {
    claimInfo.value = await getSharedWordBook(code)
  } catch (e) {
    claimInfo.value = null
    showToast(typeof e === 'string' ? e : e?.message || '预览失败')
  } finally {
    ioBusy.value = false
  }
}

const submitClaim = async () => {
  const code = claimCode.value.trim()
  if (!code || !claimInfo.value) return
  if (isLocal.value) {
    const ok = await confirmDialog('导入后将切换到云端单词本，继续？', { title: '用分享码导入' })
    if (!ok) return
    await setActiveWordbook(WORDBOOK_TYPES.server)
    await refreshMeta()
  }
  ioBusy.value = true
  try {
    const res = await claimSharedWordBook(code)
    showToast(`已导入：新增 ${res.added}，跳过 ${res.skipped}`)
    if (res.userWordBookId) {
      await setCurrentWordBookId(res.userWordBookId)
      closePanel()
      await openBook(res.userWordBookId)
      return
    }
    closePanel()
    await ensureBooks()
  } catch (e) {
    showToast(typeof e === 'string' ? e : e?.message || '导入失败')
  } finally {
    ioBusy.value = false
  }
}

const bootstrapPage = async () => {
  await refreshMeta()
  await ensureBooks()
  if (!isShelfPage.value) {
    if (!currentBookId.value && wordBooks.value[0]) {
      await selectBook(wordBooks.value[0].id)
      currentBookId.value = wordBooks.value[0].id
    }
    if (!currentBookId.value) {
      router.replace({ name: 'words' })
      return
    }
    page.value = 1
    search.value = ''
    selectMode.value = false
    selectedIds.value = []
    await load()
  }
}

watch(() => route.name, bootstrapPage, { immediate: true })
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
  background: var(--le-gradient);
  color: #fff;
  font-weight: 500;
}

.mode-badge {
  font-size: 12px;
  padding: 2px 8px;
  border-radius: 20px;
}

.mode-badge.local {
  background: rgba(5, 150, 105, 0.12);
  color: var(--le-success);
}

.mode-badge.server {
  background: var(--le-gradient-soft);
  color: var(--le-primary);
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-shrink: 0;
}

.btn-import {
  padding: 8px 12px;
  border: 1px solid var(--le-border);
  border-radius: 8px;
  background: var(--bg-card, #fff);
  color: var(--le-primary);
  font-size: 14px;
  font-weight: 500;
}

.btn-add {
  padding: 8px 14px;
  background: var(--le-gradient);
  color: #fff;
  border-radius: 8px;
  text-decoration: none;
  font-size: 14px;
  font-weight: 500;
  flex-shrink: 0;
}

.btn-import-tool {
  padding: 10px 12px;
  border: 1px solid var(--le-primary);
  border-radius: 8px;
  background: var(--le-gradient-soft, rgba(37, 99, 235, 0.08));
  color: var(--le-primary);
  font-size: 14px;
  white-space: nowrap;
}

.btn-batch {
  padding: 10px 12px;
  border: 1px solid var(--border);
  border-radius: 8px;
  background: var(--bg-card, #fff);
  color: var(--text-secondary);
  font-size: 14px;
  white-space: nowrap;
}

.btn-batch.active {
  border-color: rgba(220, 38, 38, 0.35);
  color: var(--le-danger);
  background: rgba(220, 38, 38, 0.06);
}

.batch-toolbar {
  display: flex;
  gap: 8px;
  margin: -8px 0 14px;
}

.batch-tool-btn {
  flex: 1;
  padding: 10px 12px;
  border: 1px solid var(--border);
  border-radius: 8px;
  background: var(--bg-card, #fff);
  font-size: 14px;
  color: var(--text-secondary);
}

.batch-tool-btn.danger {
  color: #fff;
  background: var(--le-danger);
  border-color: var(--le-danger);
}

.batch-tool-btn:disabled {
  opacity: 0.45;
}

.word-item.selected {
  outline: 2px solid rgba(37, 99, 235, 0.35);
  outline-offset: 1px;
}

.word-check {
  width: 18px;
  height: 18px;
  flex-shrink: 0;
  accent-color: var(--le-primary);
}

.myshelf {
  margin-bottom: 16px;
}

.myshelf__head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 10px;
  margin-bottom: 12px;
}

.myshelf__label {
  margin: 0;
  font-size: 15px;
  font-weight: 700;
  color: var(--le-text);
}

.myshelf__tip {
  margin: 4px 0 0;
  font-size: 12px;
  color: var(--le-text-muted);
}

.myshelf__tools {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  justify-content: flex-end;
}

.myshelf__tools .book-btn {
  flex: 0 0 auto;
  padding: 6px 10px;
}

.myshelf__hint {
  text-align: center;
  padding: 28px 0;
  color: var(--le-text-muted);
  font-size: 13px;
}

.shelf {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px 10px;
}

.book {
  position: relative;
  min-height: 148px;
  padding: 0;
  border: none;
  background: transparent;
  text-align: left;
  cursor: pointer;
  -webkit-tap-highlight-color: transparent;
}

.book .spine {
  position: absolute;
  left: 0;
  top: 4px;
  bottom: 4px;
  width: 10px;
  border-radius: 3px 0 0 3px;
  z-index: 2;
}

.book .cover {
  height: 100%;
  min-height: 148px;
  padding: 12px 10px 12px 18px;
  border-radius: 4px 10px 10px 4px;
  border: 1px solid var(--le-border);
  background-image: var(--le-paper-grain);
  display: flex;
  flex-direction: column;
  transition: transform 0.15s ease, box-shadow 0.15s ease, border-color 0.15s ease;
}

.book.active .cover {
  border-color: color-mix(in srgb, var(--le-primary, #2563eb) 45%, var(--le-border));
  box-shadow: 0 8px 18px rgba(37, 99, 235, 0.14);
  transform: translateY(-2px);
}

.book.tone-0 .cover { background-color: #edf3fb; }
.book.tone-0 .spine { background: linear-gradient(180deg, #1d4ed8, #2563eb); }
.book.tone-1 .cover { background-color: #eef8f3; }
.book.tone-1 .spine { background: linear-gradient(180deg, #047857, #059669); }
.book.tone-2 .cover { background-color: #f8f3ec; }
.book.tone-2 .spine { background: linear-gradient(180deg, #9a6b3f, #c4894a); }
.book.tone-3 .cover { background-color: #f3f1f8; }
.book.tone-3 .spine { background: linear-gradient(180deg, #4a5aa8, #5b6abf); }
.book.tone-4 .cover { background-color: #f7f0ef; }
.book.tone-4 .spine { background: linear-gradient(180deg, #b45a5a, #c97878); }
.book.tone-fav .cover { background-color: #fff8eb; }
.book.tone-fav .spine { background: linear-gradient(180deg, #d97706, #f59e0b); }

.book-card-note {
  margin: 8px 0 0;
  font-size: 11px;
  color: var(--le-text-muted);
  line-height: 1.35;
}

.book .top {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
  gap: 6px;
}

.book .badge {
  font-size: 10px;
  letter-spacing: 0.04em;
  color: var(--le-text-muted);
  font-weight: 650;
}

.book .pill {
  font-size: 11px;
  color: var(--le-primary, #2563eb);
  font-weight: 650;
}

.book .cover h3 {
  margin: 0;
  font-size: 15px;
  line-height: 1.3;
  color: var(--le-text);
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.book .count {
  margin: 10px 0 0;
  font-size: 12px;
  color: var(--le-text-muted);
  flex: 1;
}

.book .count strong {
  font-size: 18px;
  color: var(--le-text);
}

.book-card-actions {
  display: flex;
  gap: 6px;
  margin-top: 10px;
}

.book-card-actions .ghost,
.book-card-actions .danger-ghost {
  flex: 1;
  height: 30px;
  border-radius: 6px;
  font-size: 12px;
  font-weight: 600;
}

.book-card-actions .ghost {
  border: 1px solid var(--le-border-strong, var(--le-border));
  background: rgba(255, 255, 255, 0.8);
  color: var(--le-text-secondary);
}

.book-card-actions .danger-ghost {
  border: 1px solid rgba(220, 38, 38, 0.28);
  background: rgba(255, 255, 255, 0.8);
  color: var(--le-danger, #dc2626);
}

.book--add .spine {
  background: linear-gradient(180deg, #94a3b8, #64748b);
}

.cover--add {
  align-items: center;
  justify-content: center;
  gap: 6px;
  color: var(--le-text-muted);
  font-size: 13px;
  font-weight: 650;
  background-color: #f1f5f9 !important;
  border-style: dashed !important;
}

.add-plus {
  font-size: 28px;
  line-height: 1;
  color: var(--le-text-secondary);
}

.book-bar {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-bottom: 14px;
  padding: 12px;
  background-color: rgba(255, 255, 255, 0.92);
  background-image: var(--le-paper-grain);
  border: 1px solid var(--le-border);
  border-radius: 12px;
}

.book-bar--local {
  padding: 10px 12px;
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
  color: var(--le-danger);
  border-color: rgba(220, 38, 38, 0.28);
}

.open-book {
  margin-top: 4px;
  padding: 14px 12px 4px;
  border-radius: 14px;
  border: 1px solid var(--le-border);
  background-color: rgba(255, 255, 255, 0.92);
  background-image: var(--le-paper-grain);
}

.open-book__head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 10px;
  margin-bottom: 12px;
}

.open-book__label {
  margin: 0;
  font-size: 11px;
  font-weight: 650;
  letter-spacing: 0.04em;
  color: var(--le-text-muted);
}

.open-book__title {
  margin: 4px 0 0;
  font-size: 18px;
  font-weight: 700;
  color: var(--le-text);
  line-height: 1.3;
}

.open-book--page {
  margin-top: 0;
  padding: 0;
  border: none;
  background: transparent;
}

.open-book__manage {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  justify-content: flex-end;
}

.open-book__manage--bar {
  justify-content: flex-start;
  margin-bottom: 12px;
}

.open-book__manage .book-btn {
  flex: 0 0 auto;
  padding: 6px 10px;
}

.stats {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 8px;
  margin-bottom: 16px;
}

.stat-item {
  background-color: rgba(255, 255, 255, 0.92);
  background-image: var(--le-paper-grain);
  border-radius: 12px;
  padding: 12px 8px;
  text-align: center;
  border: 1px solid var(--le-border);
}

.stat-value {
  font-size: 20px;
  font-weight: 600;
}

.stat-item.due .stat-value { color: var(--le-warning); }
.stat-item.mastered .stat-value { color: var(--le-success); }

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

.mark-filter {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin: -6px 0 14px;
}

.mark-chip {
  padding: 6px 12px;
  border-radius: 999px;
  border: 1px solid var(--le-border);
  background: var(--bg-card);
  color: var(--text-secondary);
  font-size: 13px;
}

.mark-chip.active {
  border-color: var(--le-primary);
  color: var(--le-primary);
  background: var(--le-gradient-soft);
}

.mark-chip.forget.active {
  border-color: var(--le-danger);
  color: var(--le-danger);
  background: rgba(220, 38, 38, 0.08);
}

.mark-chip.fuzzy.active {
  border-color: var(--le-warning);
  color: var(--le-warning);
  background: rgba(217, 119, 6, 0.1);
}

.mark-chip.remember.active {
  border-color: var(--le-success);
  color: var(--le-success);
  background: rgba(5, 150, 105, 0.1);
}

.mark-chip.favorite.active {
  border-color: #f59e0b;
  color: #b45309;
  background: rgba(245, 158, 11, 0.12);
}

.fav-tag {
  color: #f59e0b;
  font-size: 14px;
  line-height: 1;
  flex-shrink: 0;
}

.btn-review, .btn-free {
  padding: 10px 14px;
  border: none;
  border-radius: 8px;
  font-size: 14px;
  white-space: nowrap;
}

.btn-review {
  background: var(--le-success);
  color: #fff;
}

.btn-free {
  background: var(--le-warning);
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
  background-color: rgba(255, 255, 255, 0.92);
  background-image: var(--le-paper-grain);
  border: 1px solid var(--le-border);
  border-radius: 12px;
  padding: 14px;
  margin-bottom: 10px;
}

.word-main {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 10px;
  margin-bottom: 8px;
}

.word-title {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 8px;
  min-width: 0;
  flex: 1;
}

.word-text {
  font-size: 18px;
  font-weight: 600;
  word-break: break-word;
}

.status-tag {
  font-size: 11px;
  padding: 2px 8px;
  border-radius: 10px;
  flex-shrink: 0;
}

.status-tag.new { background: var(--le-bg-muted); color: var(--le-text-muted); }
.status-tag.due { background: rgba(217, 119, 6, 0.1); color: var(--le-warning); }
.status-tag.mastered { background: rgba(5, 150, 105, 0.1); color: var(--le-success); }
.status-tag.learning { background: var(--le-gradient-soft); color: var(--le-primary); }
.status-tag.forget { background: rgba(220, 38, 38, 0.1); color: var(--le-danger); }
.status-tag.fuzzy { background: rgba(217, 119, 6, 0.1); color: var(--le-warning); }
.status-tag.remember { background: rgba(5, 150, 105, 0.1); color: var(--le-success); }

.definition {
  font-size: 14px;
  color: var(--text-secondary);
  margin: 0;
  line-height: 1.55;
  word-break: break-word;
}

.example {
  margin: 8px 0 0;
  font-size: 13px;
  line-height: 1.5;
  color: var(--text-muted);
  word-break: break-word;
}

.btn-delete {
  flex-shrink: 0;
  background: none;
  border: none;
  color: var(--le-danger);
  font-size: 13px;
  padding: 2px 0 0;
}

.empty-link {
  display: inline-block;
  margin-top: 10px;
  color: var(--le-primary);
  font-size: 14px;
}

.empty-actions {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 10px;
  margin-top: 12px;
}

.empty-btn {
  padding: 10px 16px;
  border: none;
  border-radius: 8px;
  background: var(--le-gradient);
  color: #fff;
  font-size: 14px;
}

.empty-link-btn {
  background: none;
  border: none;
  color: var(--le-primary);
  font-size: 14px;
  padding: 0;
}

.file-input {
  display: none;
}

.panel-mask {
  position: fixed;
  inset: 0;
  /* 底部导航 z-index: 200，面板必须更高 */
  z-index: 400;
  background: rgba(15, 23, 42, 0.45);
  display: flex;
  align-items: flex-end;
  justify-content: center;
  padding: 0;
}

.panel {
  width: 100%;
  max-width: 480px;
  max-height: min(88vh, 720px);
  display: flex;
  flex-direction: column;
  background: var(--bg-card, #fff);
  border-radius: 16px 16px 0 0;
  box-shadow: 0 -8px 32px rgba(0, 0, 0, 0.12);
  /* 避开底部导航 + 安全区 */
  padding-bottom: calc(var(--le-mobile-nav-h, 62px) + env(safe-area-inset-bottom, 0) + 12px);
}

.panel-scroll {
  flex: 1;
  min-height: 0;
  overflow: auto;
  padding: 18px 16px 8px;
  -webkit-overflow-scrolling: touch;
}

.panel h3 {
  margin: 0 0 8px;
  font-size: 17px;
}

.panel-hint {
  margin: 0 0 12px;
  font-size: 12px;
  color: var(--text-muted);
  line-height: 1.5;
}

.panel-hint--accent {
  color: var(--le-primary, #2563eb);
  font-weight: 600;
}

.panel-input {
  width: 100%;
  box-sizing: border-box;
  border: 1px solid var(--le-border);
  border-radius: 10px;
  padding: 10px 12px;
  font-size: 14px;
  background: #fff;
  color: var(--le-text);
}

.import-target {
  margin-bottom: 14px;
  padding: 12px;
  border-radius: 10px;
  background: var(--le-bg-muted, #f1f5f9);
  border: 1px solid var(--le-border);
}

.import-target__label {
  margin: 0 0 8px;
  font-size: 13px;
  font-weight: 650;
  color: var(--le-text);
}

.import-target__modes {
  display: flex;
  flex-wrap: wrap;
  gap: 12px 16px;
  margin-bottom: 10px;
}

.import-target__mode {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  color: var(--le-text);
  cursor: pointer;
}

.import-target__select {
  width: 100%;
}

.panel--import {
  max-height: min(92vh, 780px);
}

.import-guide {
  margin-bottom: 12px;
  padding: 10px 12px;
  border-radius: 10px;
  background: var(--le-bg-muted, #f1f5f9);
  border: 1px solid var(--le-border, var(--border));
}

.import-guide__title {
  margin: 0 0 8px;
  font-size: 13px;
  font-weight: 600;
  color: var(--text);
}

.import-guide__item {
  margin-bottom: 10px;
  padding-bottom: 8px;
  border-bottom: 1px dashed rgba(55, 75, 105, 0.12);
}

.import-guide__item:last-of-type {
  border-bottom: none;
  margin-bottom: 4px;
}

.import-guide__item strong {
  display: block;
  font-size: 12px;
  margin-bottom: 4px;
}

.import-guide__item p {
  margin: 4px 0 0;
  font-size: 11px;
  color: var(--text-muted);
  line-height: 1.45;
}

.import-guide__item.is-warn {
  background: rgba(220, 38, 38, 0.05);
  border: 1px solid rgba(220, 38, 38, 0.18);
  border-radius: 8px;
  padding: 8px;
}

.import-guide__example {
  margin: 4px 0 0;
  padding: 8px;
  border-radius: 6px;
  background: var(--bg-card, #fff);
  border: 1px solid var(--border);
  font-family: ui-monospace, SFMono-Regular, Menlo, Consolas, monospace;
  font-size: 11px;
  line-height: 1.4;
  white-space: pre-wrap;
  color: var(--text);
}

.panel-textarea,
.panel-input {
  width: 100%;
  box-sizing: border-box;
  padding: 10px 12px;
  border: 1px solid var(--border);
  border-radius: 8px;
  font-size: 14px;
  background: var(--bg);
  color: var(--text);
  margin-bottom: 10px;
}

.panel-actions {
  display: flex;
  gap: 8px;
  flex-shrink: 0;
  padding: 10px 16px 0;
  border-top: 1px solid var(--le-border, var(--border));
  background: var(--bg-card, #fff);
}

.panel-actions.col {
  flex-direction: column;
}

.panel-actions-row {
  display: flex;
  gap: 8px;
  width: 100%;
}

.panel-btn {
  flex: 1;
  padding: 12px;
  border: 1px solid var(--border);
  border-radius: 8px;
  background: var(--bg);
  font-size: 14px;
  color: var(--text-secondary);
}

.panel-btn.primary {
  background: var(--le-gradient);
  border: none;
  color: #fff;
  font-weight: 500;
}

.panel-btn.ghost {
  background: transparent;
}

.panel-btn:disabled {
  opacity: 0.5;
}

.share-box,
.claim-box {
  text-align: center;
  padding: 12px;
  margin-bottom: 12px;
  background: var(--le-gradient-soft, #f1f5f9);
  border-radius: 10px;
}

.share-code {
  font-size: 26px;
  font-weight: 700;
  letter-spacing: 0.18em;
  font-family: ui-monospace, monospace;
  margin-bottom: 6px;
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
