<template>
  <PageShell
    title="我的单词本"
    subtitle="书架上的每一本，都是你的词汇积累"
    :back="true"
    badge="WORDS"
  >
    <template #header-extra>
      <el-button class="header-btn header-btn--secondary" @click="$router.push({ name: 'wordPacks' })">
        <el-icon><Collection /></el-icon>
        <span class="btn-label">官方词本</span>
      </el-button>
      <el-button
        type="primary"
        class="le-btn-gradient add-btn header-btn"
        @click="showAddDialog = true"
        :disabled="!currentBookId"
      >
        <el-icon><Plus /></el-icon>
        <span class="btn-label">添加单词</span>
      </el-button>
    </template>

    <!-- 书架：实体书本选择 -->
    <section class="bookshelf le-paper-flip">
      <div class="shelf-header">
        <span class="sheet-label">BOOKSHELF · 我的书架</span>
        <div class="shelf-tools">
          <el-button size="small" @click="openCreateBook">
            <el-icon><FolderAdd /></el-icon>
            新建词本
          </el-button>
          <el-button size="small" :disabled="!currentBook" @click="openRenameBook">重命名</el-button>
          <el-button
            size="small"
            type="danger"
            plain
            :disabled="!currentBook || currentBook.isDefault"
            @click="handleDeleteBook"
          >
            删除
          </el-button>
        </div>
      </div>

      <div class="shelf-rail">
        <button
          v-for="(book, index) in wordBooks"
          :key="book.id"
          type="button"
          class="vocab-book le-paper-flip"
          :class="[
            `tone-${index % 5}`,
            `le-paper-flip-delay-${Math.min((index % 4) + 1, 5)}`,
            { active: book.id === currentBookId },
          ]"
          @click="selectBook(book.id)"
        >
          <span class="vocab-book__spine" aria-hidden="true"></span>
          <span class="vocab-book__edge" aria-hidden="true"></span>
          <div class="vocab-book__cover">
            <span class="vocab-book__badge">{{ book.isDefault ? 'DEFAULT' : 'MINE' }}</span>
            <h3 class="vocab-book__title">{{ book.name }}</h3>
            <p class="vocab-book__desc">{{ book.description || '个人词汇本' }}</p>
            <div class="vocab-book__foot">
              <strong>{{ book.wordCount ?? 0 }}</strong>
              <span>词</span>
            </div>
          </div>
        </button>

        <button type="button" class="vocab-book vocab-book--add" @click="openCreateBook">
          <span class="vocab-book__spine" aria-hidden="true"></span>
          <div class="vocab-book__cover vocab-book__cover--add">
            <el-icon :size="28"><Plus /></el-icon>
            <span>放一本新书</span>
          </div>
        </button>
      </div>
      <p class="shelf-hint mobile-only-inline">左右滑动切换词本</p>
      <div class="shelf-plank" aria-hidden="true"></div>

      <!-- 官方词本同一页书架 -->
      <div class="shelf-header shelf-header--official">
        <span class="sheet-label">OFFICIAL · 官方词本</span>
        <el-button text type="primary" size="small" @click="$router.push({ name: 'wordPacks' })">
          全部官方词本 →
        </el-button>
      </div>

      <div v-if="packsLoading" class="official-loading">加载官方词本…</div>
      <el-empty
        v-else-if="!officialPacks.length"
        description="暂无已发布的官方词本"
        :image-size="56"
      />
      <div v-else class="shelf-rail shelf-rail--official">
        <div
          v-for="(pack, index) in officialPacks"
          :key="pack.id"
          class="vocab-book vocab-book--official le-paper-flip"
          :class="[
            `tone-${packTone(pack, index)}`,
            `le-paper-flip-delay-${Math.min((index % 4) + 1, 5)}`,
            {
              active: pack.claimed && pack.userWordBookId === currentBookId,
              claimed: pack.claimed,
            },
          ]"
        >
          <span class="vocab-book__spine" aria-hidden="true"></span>
          <span class="vocab-book__edge" aria-hidden="true"></span>
          <div class="vocab-book__cover">
            <div class="vocab-book__top">
              <span class="vocab-book__badge">{{ packCategoryLabel(pack.category) }}</span>
              <span v-if="pack.claimed" class="claimed-pill">已领</span>
            </div>
            <h3 class="vocab-book__title">{{ pack.name }}</h3>
            <p class="vocab-book__desc">{{ pack.description || '官方高频词，领取后可复习' }}</p>
            <div class="vocab-book__foot">
              <strong>{{ pack.wordCount ?? 0 }}</strong>
              <span>词</span>
            </div>
            <div class="vocab-book__actions">
              <button
                v-if="pack.claimed && pack.userWordBookId"
                type="button"
                class="ghost-btn"
                @click="selectBook(pack.userWordBookId)"
              >
                打开
              </button>
              <button
                type="button"
                class="solid-btn"
                :disabled="claimingId === pack.id"
                @click="claimPack(pack)"
              >
                {{
                  claimingId === pack.id
                    ? '领取中…'
                    : pack.claimed
                      ? '同步'
                      : '领取'
                }}
              </button>
            </div>
          </div>
        </div>
      </div>
      <div class="shelf-plank" aria-hidden="true"></div>
    </section>

    <!-- 打开的书：当前词本内容 -->
    <section v-if="currentBook" class="open-book le-paper le-paper--ruled le-paper--margin le-paper-flip le-paper-flip-delay-1">
      <div class="open-book__head">
        <div>
          <p class="sheet-label">OPEN BOOK · 正在阅读</p>
          <h2 class="open-book__title">{{ currentBook.name }}</h2>
          <p v-if="currentBook.description" class="open-book__subtitle">{{ currentBook.description }}</p>
        </div>
        <div class="toolbar-actions">
          <el-button
            v-if="stats?.dueCount > 0"
            type="primary"
            class="le-btn-gradient"
            @click="goToReview"
          >
            <el-icon><Timer /></el-icon>
            开始复习 ({{ stats.dueCount }})
          </el-button>
          <el-button v-if="stats?.totalWords > 0" @click="goToFreeReview">
            <el-icon><Refresh /></el-icon>
            自由复习
          </el-button>
        </div>
      </div>

      <div v-if="stats" class="le-stats-row open-book__stats">
        <div class="stat-tile">
          <div class="stat-value">{{ stats.totalWords }}</div>
          <div class="stat-label">总单词数</div>
        </div>
        <div class="stat-tile">
          <div class="stat-value is-warning">{{ stats.dueCount }}</div>
          <div class="stat-label">待复习</div>
        </div>
        <div class="stat-tile">
          <div class="stat-value is-success">{{ stats.masteredCount }}</div>
          <div class="stat-label">已掌握</div>
        </div>
        <div class="stat-tile">
          <div class="stat-value is-primary">{{ stats.reviewLogsCount }}</div>
          <div class="stat-label">复习次数</div>
        </div>
      </div>

      <div class="search-bar">
        <el-input
          v-model="searchKeyword"
          placeholder="在这本书里搜索单词、释义…"
          clearable
          class="search-input"
          @keyup.enter="handleSearch"
          @clear="handleSearch"
        >
          <template #append>
            <el-button @click="handleSearch">
              <el-icon><Search /></el-icon>
            </el-button>
          </template>
        </el-input>
      </div>

      <div class="table-sheet desktop-only" v-loading="loading">
        <el-table :data="wordList" class="words-table" style="width: 100%">
          <el-table-column prop="word" label="单词" width="150">
            <template #default="{ row }">
              <span class="word-text">{{ row.word }}</span>
            </template>
          </el-table-column>
          <el-table-column prop="definition" label="释义" min-width="200">
            <template #default="{ row }">
              <span class="definition-text">{{ row.definition || '暂无释义' }}</span>
            </template>
          </el-table-column>
          <el-table-column prop="example" label="例句" min-width="250">
            <template #default="{ row }">
              <span class="example-text">{{ row.example || '暂无例句' }}</span>
            </template>
          </el-table-column>
          <el-table-column prop="nextReview" label="复习" width="110">
            <template #default="{ row }">
              <span class="status-pill" :class="getReviewStatus(row).cls">
                {{ getReviewStatus(row).text }}
              </span>
            </template>
          </el-table-column>
          <el-table-column label="操作" width="72" align="center">
            <template #default="{ row }">
              <button type="button" class="icon-delete" @click="deleteWord(row.id)" title="删除">
                <el-icon :size="16"><Delete /></el-icon>
              </button>
            </template>
          </el-table-column>
        </el-table>
        <el-empty v-if="!loading && !wordList.length" description="这本书还是空白的，点右上角加词，或去领取官方词本" :image-size="72" />
      </div>

      <div v-loading="loading" class="word-cards mobile-only">
        <el-empty v-if="!loading && !wordList.length" description="这本书还是空白的" :image-size="72" />
        <article
          v-for="(row, index) in wordList"
          :key="row.id"
          class="word-card le-paper le-paper-flip"
          :class="`le-paper-flip-delay-${Math.min((index % 4) + 1, 5)}`"
        >
          <div class="word-card-top">
            <div class="word-card-title">
              <span class="word-text">{{ row.word }}</span>
              <span class="status-pill" :class="getReviewStatus(row).cls">
                {{ getReviewStatus(row).text }}
              </span>
            </div>
            <button type="button" class="icon-delete" @click="deleteWord(row.id)" title="删除">
              <el-icon :size="18"><Delete /></el-icon>
            </button>
          </div>
          <p class="word-card-def">{{ row.definition || '暂无释义' }}</p>
          <p v-if="row.example" class="word-card-ex">{{ row.example }}</p>
        </article>
      </div>

      <div class="pagination">
        <el-pagination
          v-model:current-page="page"
          v-model:page-size="pageSize"
          :total="total"
          :page-sizes="[10, 20, 50]"
          layout="total, sizes, prev, pager, next"
          @size-change="handleSizeChange"
          @current-change="handlePageChange"
        />
      </div>
    </section>

    <!-- 添加单词弹窗 -->
    <el-dialog
      v-model="showAddDialog"
      title="添加单词"
      width="500px"
      class="add-word-dialog"
    >
      <el-form :model="newWord" label-width="80px">
        <el-form-item label="单词本">
          <el-select v-model="newWord.wordBookId" style="width: 100%">
            <el-option
              v-for="book in wordBooks"
              :key="book.id"
              :label="book.name"
              :value="book.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="单词" required>
          <el-input v-model="newWord.word" placeholder="输入单词" />
        </el-form-item>
        <el-form-item label="释义">
          <el-input
            v-model="newWord.definition"
            type="textarea"
            rows="2"
            placeholder="输入释义"
          />
        </el-form-item>
        <el-form-item label="例句">
          <el-input
            v-model="newWord.example"
            type="textarea"
            rows="3"
            placeholder="输入例句"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showAddDialog = false">取消</el-button>
        <el-button type="primary" class="le-btn-gradient" @click="addWord">添加</el-button>
      </template>
    </el-dialog>

    <!-- 新建 / 重命名单词本 -->
    <el-dialog
      v-model="showBookDialog"
      :title="bookDialogMode === 'create' ? '新建单词本' : '重命名单词本'"
      width="420px"
    >
      <el-form label-width="80px">
        <el-form-item label="名称" required>
          <el-input
            v-model="bookForm.name"
            maxlength="50"
            show-word-limit
            placeholder="例如：四级词汇"
          />
        </el-form-item>
        <el-form-item label="备注">
          <el-input
            v-model="bookForm.description"
            type="textarea"
            rows="2"
            maxlength="200"
            placeholder="可选"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showBookDialog = false">取消</el-button>
        <el-button type="primary" class="le-btn-gradient" @click="submitBookForm">确定</el-button>
      </template>
    </el-dialog>
  </PageShell>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import PageShell from '../components/PageShell.vue'
import { promptGoReviewAfterAdd } from '../utils/promptGoReview.js'
import {
  getUserWords,
  addUserWord,
  deleteUserWord,
  getWordStats,
  getWordBooks,
  createWordBook,
  updateWordBook,
  deleteWordBook,
  getCurrentWordBookId,
  setCurrentWordBookId,
  getOfficialWordPacks,
  claimOfficialWordPack,
} from '../api/Word.js'

const router = useRouter()

const wordList = ref([])
const wordBooks = ref([])
const officialPacks = ref([])
const packsLoading = ref(false)
const claimingId = ref(null)
const currentBookId = ref(null)
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)
const searchKeyword = ref('')
const loading = ref(false)
const stats = ref(null)
const showAddDialog = ref(false)
const newWord = ref({ word: '', definition: '', example: '', wordBookId: null })

const showBookDialog = ref(false)
const bookDialogMode = ref('create')
const bookForm = ref({ name: '', description: '' })

const currentBook = computed(() =>
  wordBooks.value.find(b => b.id === currentBookId.value) || null
)

const loadBooks = async () => {
  const books = await getWordBooks()
  wordBooks.value = books || []
  if (!wordBooks.value.length) return

  const saved = getCurrentWordBookId()
  const match = wordBooks.value.find(b => b.id === saved)
  currentBookId.value = match?.id || wordBooks.value.find(b => b.isDefault)?.id || wordBooks.value[0].id
  setCurrentWordBookId(currentBookId.value)
  newWord.value.wordBookId = currentBookId.value
}

const loadWords = async () => {
  if (!currentBookId.value) return
  loading.value = true
  try {
    const res = await getUserWords({
      page: page.value,
      pageSize: pageSize.value,
      search: searchKeyword.value,
      wordBookId: currentBookId.value
    })
    wordList.value = res.items || []
    total.value = res.total || 0
  } catch (e) {
    console.error('获取单词失败', e)
  } finally {
    loading.value = false
  }
}

const loadStats = async () => {
  if (!currentBookId.value) return
  try {
    stats.value = await getWordStats({ wordBookId: currentBookId.value })
  } catch (e) {
    console.error('获取统计失败', e)
  }
}

const refreshBookWordCounts = async () => {
  try {
    const books = await getWordBooks()
    wordBooks.value = books || []
  } catch (e) {
    console.error(e)
  }
}

const onBookChange = (id) => {
  setCurrentWordBookId(id)
  newWord.value.wordBookId = id
  page.value = 1
  loadWords()
  loadStats()
}

const selectBook = (id) => {
  if (currentBookId.value === id) return
  currentBookId.value = id
  onBookChange(id)
}

const packCategoryLabel = (c) =>
  ({ cet4: '四级', cet6: '六级', kaoyan: '考研', other: '官方' }[c] || '官方')

const packTone = (pack, index) => {
  if (pack.category === 'cet4') return 0
  if (pack.category === 'cet6') return 1
  if (pack.category === 'kaoyan') return 2
  return index % 5
}

const loadOfficialPacks = async () => {
  packsLoading.value = true
  try {
    officialPacks.value = (await getOfficialWordPacks()) || []
  } catch (e) {
    console.error('加载官方词本失败', e)
    officialPacks.value = []
  } finally {
    packsLoading.value = false
  }
}

const claimPack = async (pack) => {
  try {
    await ElMessageBox.confirm(
      `将「${pack.name}」复制到你的书架（已有单词会跳过）。词量较大时可能需要几秒。`,
      pack.claimed ? '同步官方词本' : '领取官方词本',
      { type: 'info', confirmButtonText: pack.claimed ? '开始同步' : '开始领取', cancelButtonText: '取消' }
    )
  } catch {
    return
  }

  claimingId.value = pack.id
  try {
    const res = await claimOfficialWordPack(pack.id)
    ElMessage.success(
      `完成：新增 ${res.added} 词，本中共 ${res.totalInBook} 词` +
        (res.remaining > 0 ? `（还可同步 ${res.remaining}）` : '')
    )
    await Promise.all([loadBooks(), loadOfficialPacks()])
    if (res?.userWordBookId) {
      selectBook(res.userWordBookId)
    }
  } catch (e) {
    console.error(e)
  } finally {
    claimingId.value = null
  }
}

const getReviewStatus = (word) => {
  if (!word.nextReview) {
    return { type: 'info', text: '新词', cls: 'is-new' }
  }
  const now = new Date()
  const next = new Date(word.nextReview)
  if (next <= now) {
    return { type: 'warning', text: '待复习', cls: 'is-due' }
  }
  if (word.interval >= 21) {
    return { type: 'success', text: '已掌握', cls: 'is-mastered' }
  }
  return { type: 'primary', text: '复习中', cls: 'is-learning' }
}

const addWord = async () => {
  if (!newWord.value.word.trim()) {
    ElMessage.warning('请输入单词')
    return
  }
  try {
    await addUserWord({
      ...newWord.value,
      wordBookId: newWord.value.wordBookId || currentBookId.value
    })
    const addedWord = newWord.value.word.trim()
    showAddDialog.value = false
    newWord.value = {
      word: '',
      definition: '',
      example: '',
      wordBookId: currentBookId.value
    }
    await refreshBookWordCounts()
    loadWords()
    loadStats()
    await promptGoReviewAfterAdd(router, addedWord, '/my-words')
  } catch (e) {
    console.error(e)
  }
}

const deleteWord = async (id) => {
  try {
    await ElMessageBox.confirm('确定要删除这个单词吗？', '提示', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    })
    await deleteUserWord(id)
    ElMessage.success('删除成功')
    await refreshBookWordCounts()
    loadWords()
    loadStats()
  } catch (e) {
    if (e !== 'cancel') {
      console.error(e)
    }
  }
}

const openCreateBook = () => {
  bookDialogMode.value = 'create'
  bookForm.value = { name: '', description: '' }
  showBookDialog.value = true
}

const openRenameBook = () => {
  if (!currentBook.value) return
  bookDialogMode.value = 'rename'
  bookForm.value = {
    name: currentBook.value.name,
    description: currentBook.value.description || ''
  }
  showBookDialog.value = true
}

const submitBookForm = async () => {
  const name = bookForm.value.name?.trim()
  if (!name) {
    ElMessage.warning('请输入单词本名称')
    return
  }
  try {
    if (bookDialogMode.value === 'create') {
      const book = await createWordBook({
        name,
        description: bookForm.value.description
      })
      ElMessage.success('单词本已创建')
      await loadBooks()
      currentBookId.value = book.id
      onBookChange(book.id)
    } else {
      await updateWordBook(currentBookId.value, {
        name,
        description: bookForm.value.description
      })
      ElMessage.success('已更新')
      await refreshBookWordCounts()
    }
    showBookDialog.value = false
  } catch (e) {
    console.error(e)
  }
}

const handleDeleteBook = async () => {
  if (!currentBook.value || currentBook.value.isDefault) return
  try {
    await ElMessageBox.confirm(
      `确定删除「${currentBook.value.name}」吗？其中的单词会自动移到默认单词本。`,
      '删除单词本',
      {
        confirmButtonText: '删除并迁移',
        cancelButtonText: '取消',
        type: 'warning'
      }
    )
    await deleteWordBook(currentBook.value.id, { moveWordsToDefault: true })
    ElMessage.success('已删除')
    await loadBooks()
    onBookChange(currentBookId.value)
  } catch (e) {
    if (e !== 'cancel') console.error(e)
  }
}

const goToReview = () => {
  router.push({ name: 'wordReview', query: { wordBookId: currentBookId.value } })
}

const goToFreeReview = () => {
  router.push({
    name: 'wordReview',
    query: { mode: 'free', wordBookId: currentBookId.value }
  })
}

const handleSearch = () => {
  page.value = 1
  loadWords()
}

const handleSizeChange = (size) => {
  pageSize.value = size
  loadWords()
}

const handlePageChange = () => {
  loadWords()
}

watch(showAddDialog, (open) => {
  if (open) newWord.value.wordBookId = currentBookId.value
})

onMounted(async () => {
  try {
    await loadBooks()
    await Promise.all([loadWords(), loadStats(), loadOfficialPacks()])
  } catch (e) {
    console.error(e)
  }
})
</script>


<style scoped>
.add-btn { flex-shrink: 0; }

:deep(.le-page-header) { width: 100%; }

.sheet-label {
  font-family: var(--le-font-mono);
  font-size: 11px;
  color: var(--le-primary);
  letter-spacing: 0.06em;
}

.bookshelf { margin-bottom: 22px; }

.shelf-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  flex-wrap: wrap;
  margin-bottom: 14px;
}

.shelf-tools { display: flex; flex-wrap: wrap; gap: 8px; }

.shelf-rail {
  display: flex;
  gap: 14px;
  overflow-x: auto;
  padding: 8px 4px 0;
  scroll-snap-type: x proximity;
  -webkit-overflow-scrolling: touch;
}

.shelf-rail::-webkit-scrollbar { height: 6px; }
.shelf-rail::-webkit-scrollbar-thumb {
  background: rgba(55, 75, 105, 0.2);
  border-radius: 999px;
}

.shelf-plank {
  height: 14px;
  margin-top: -2px;
  border-radius: 2px 2px 8px 8px;
  background:
    linear-gradient(180deg, rgba(120, 90, 60, 0.35), rgba(90, 65, 40, 0.55)),
    repeating-linear-gradient(90deg, rgba(255,255,255,0.04) 0 2px, transparent 2px 7px);
  box-shadow: 0 8px 16px rgba(30, 41, 59, 0.12), inset 0 1px 0 rgba(255,255,255,0.25);
}

.vocab-book {
  position: relative;
  flex: 0 0 auto;
  width: 148px;
  height: 196px;
  padding: 0;
  border: none;
  background: transparent;
  cursor: pointer;
  text-align: left;
  scroll-snap-align: start;
  transform-origin: left bottom;
  transition: transform 0.25s ease, filter 0.25s ease;
  filter: drop-shadow(0 6px 10px rgba(30, 41, 59, 0.12));
}

.vocab-book:hover {
  transform: translateY(-10px) rotate(-1.5deg);
  z-index: 2;
}

.vocab-book.active {
  transform: translateY(-16px) rotate(-2deg) scale(1.03);
  z-index: 3;
  filter: drop-shadow(0 12px 18px rgba(37, 99, 235, 0.22));
}

.vocab-book__spine {
  position: absolute;
  left: 0; top: 4px; bottom: 4px;
  width: 14px;
  border-radius: 3px 0 0 3px;
  background: linear-gradient(90deg, rgba(0,0,0,0.22), rgba(255,255,255,0.08));
  z-index: 2;
}

.vocab-book__edge {
  position: absolute;
  right: -3px; top: 8px; bottom: 8px;
  width: 6px;
  border-radius: 0 2px 2px 0;
  background: repeating-linear-gradient(#f4f1ea 0 2px, #e8e2d6 2px 3px);
  box-shadow: 1px 0 0 rgba(0,0,0,0.06);
  z-index: 1;
}

.vocab-book__cover {
  position: absolute;
  inset: 0;
  padding: 16px 14px 14px 22px;
  border-radius: 4px 10px 10px 4px;
  border: 1px solid rgba(55, 75, 105, 0.16);
  background-color: #f8fafc;
  background-image: var(--le-paper-grain), linear-gradient(145deg, rgba(255,255,255,0.55), transparent 55%);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.vocab-book.tone-0 .vocab-book__cover { background-color: #edf3fb; border-color: rgba(37,99,235,0.22); }
.vocab-book.tone-0 .vocab-book__spine { background: linear-gradient(180deg, #1d4ed8, #2563eb); }
.vocab-book.tone-1 .vocab-book__cover { background-color: #eef8f3; border-color: rgba(5,150,105,0.22); }
.vocab-book.tone-1 .vocab-book__spine { background: linear-gradient(180deg, #047857, #059669); }
.vocab-book.tone-2 .vocab-book__cover { background-color: #f8f3ec; border-color: rgba(180,120,60,0.25); }
.vocab-book.tone-2 .vocab-book__spine { background: linear-gradient(180deg, #9a6b3f, #c4894a); }
.vocab-book.tone-3 .vocab-book__cover { background-color: #f3f1f8; border-color: rgba(91,106,191,0.22); }
.vocab-book.tone-3 .vocab-book__spine { background: linear-gradient(180deg, #4a5aa8, #5b6abf); }
.vocab-book.tone-4 .vocab-book__cover { background-color: #f7f0ef; border-color: rgba(201,120,120,0.28); }
.vocab-book.tone-4 .vocab-book__spine { background: linear-gradient(180deg, #b45a5a, #c97878); }

.vocab-book__badge {
  font-family: var(--le-font-mono);
  font-size: 10px;
  letter-spacing: 0.08em;
  color: var(--le-text-muted);
  margin-bottom: 10px;
}

.vocab-book__title {
  margin: 0;
  font-family: var(--le-font-display);
  font-size: 16px;
  font-weight: 700;
  line-height: 1.3;
  color: var(--le-text);
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.vocab-book__desc {
  margin: 8px 0 0;
  font-size: 12px;
  line-height: 1.4;
  color: var(--le-text-muted);
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  flex: 1;
}

.vocab-book__foot {
  display: flex;
  align-items: baseline;
  gap: 4px;
  margin-top: auto;
  padding-top: 8px;
  border-top: 1px dashed rgba(55, 75, 105, 0.16);
}

.vocab-book__foot strong {
  font-family: var(--le-font-display);
  font-size: 20px;
  color: var(--le-text);
}

.vocab-book__foot span { font-size: 12px; color: var(--le-text-muted); }

.vocab-book--add .vocab-book__spine { background: linear-gradient(180deg, #94a3b8, #64748b); }

.vocab-book__cover--add {
  align-items: center;
  justify-content: center;
  gap: 8px;
  color: var(--le-text-secondary);
  background: repeating-linear-gradient(-45deg, rgba(255,255,255,0.65) 0 8px, rgba(238,242,247,0.9) 8px 16px);
  border-style: dashed;
}

.vocab-book__cover--add span { font-size: 13px; }

.shelf-header--official {
  margin-top: 22px;
}

.official-loading {
  padding: 16px 4px;
  font-size: 13px;
  color: var(--le-text-muted);
}

.vocab-book--official {
  height: 228px;
  width: 156px;
}

.vocab-book__top {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 6px;
  margin-bottom: 8px;
}

.claimed-pill {
  font-size: 11px;
  color: var(--le-success);
  background: rgba(5, 150, 105, 0.1);
  border: 1px solid rgba(5, 150, 105, 0.2);
  border-radius: 4px;
  padding: 1px 6px;
  flex-shrink: 0;
}

.vocab-book__actions {
  display: flex;
  gap: 6px;
  margin-top: 10px;
}

.ghost-btn,
.solid-btn {
  flex: 1;
  height: 28px;
  border-radius: 6px;
  font-size: 12px;
  cursor: pointer;
  border: 1px solid var(--le-border-strong);
  background: rgba(255, 255, 255, 0.8);
  color: var(--le-text-secondary);
}

.solid-btn {
  border: none;
  background: var(--le-gradient);
  color: #fff;
}

.solid-btn:disabled {
  opacity: 0.65;
  cursor: wait;
}

.open-book {
  position: relative;
  padding: 22px 22px 22px 48px;
  margin-bottom: 8px;
  overflow: hidden;
}

.open-book__head {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 16px;
  flex-wrap: wrap;
  margin-bottom: 16px;
}

.open-book__title {
  margin: 6px 0 0;
  font-family: var(--le-font-display);
  font-size: 26px;
  font-weight: 700;
  color: var(--le-text);
}

.open-book__subtitle {
  margin: 6px 0 0;
  font-size: 13px;
  color: var(--le-text-muted);
}

.open-book__stats { margin-bottom: 14px; }

.stat-tile {
  padding: 14px 12px;
  text-align: center;
  border-radius: 4px 10px 10px 4px;
  background: rgba(255,255,255,0.55);
  border: 1px solid var(--le-border);
}

.stat-value {
  font-family: var(--le-font-display);
  font-size: 26px;
  font-weight: 700;
  color: var(--le-text);
  line-height: 1.2;
}

.stat-value.is-warning { color: var(--le-warning); }
.stat-value.is-success { color: var(--le-success); }
.stat-value.is-primary { color: var(--le-primary); }
.stat-label { margin-top: 4px; font-size: 12px; color: var(--le-text-muted); }

.toolbar-actions { display: flex; flex-wrap: wrap; gap: 8px; justify-content: flex-end; }
.search-bar { margin-bottom: 14px; }
.search-input { width: min(360px, 100%); }
.search-input :deep(.el-input__wrapper) {
  background: #fff;
  box-shadow: 0 0 0 1px var(--le-border) inset;
}
.search-input :deep(.el-input__wrapper.is-focus) {
  box-shadow: 0 0 0 1px var(--le-primary) inset;
}

.table-sheet {
  overflow: hidden;
  border-radius: 4px 10px 10px 4px;
  min-height: 120px;
  background: rgba(255,255,255,0.72);
  border: 1px solid var(--le-border);
}

.words-table {
  --el-table-bg-color: transparent;
  --el-table-tr-bg-color: transparent;
  --el-table-header-bg-color: rgba(238, 242, 247, 0.65);
  --el-table-row-hover-bg-color: rgba(37, 99, 235, 0.04);
  --el-table-border-color: var(--le-border);
}
.words-table :deep(.el-table__inner-wrapper::before) { display: none; }
.words-table :deep(th.el-table__cell) {
  font-family: var(--le-font-mono);
  font-size: 12px;
  font-weight: 600;
  letter-spacing: 0.04em;
  color: var(--le-text-muted);
}

.word-text {
  font-family: var(--le-font-display);
  font-weight: 700;
  font-size: 15px;
  color: var(--le-text);
  word-break: break-word;
}
.definition-text { color: var(--le-text-secondary); }
.example-text { color: var(--le-text-muted); font-size: 13px; }

.status-pill {
  display: inline-flex;
  align-items: center;
  font-family: var(--le-font-mono);
  font-size: 11px;
  font-weight: 500;
  letter-spacing: 0.03em;
  padding: 3px 8px;
  border-radius: 4px;
  border: 1px solid var(--le-border);
  background: var(--le-bg-muted);
  color: var(--le-text-secondary);
  white-space: nowrap;
}
.status-pill.is-new { color: var(--le-text-muted); }
.status-pill.is-due {
  color: var(--le-warning);
  background: rgba(217, 119, 6, 0.08);
  border-color: rgba(217, 119, 6, 0.22);
}
.status-pill.is-mastered {
  color: var(--le-success);
  background: rgba(5, 150, 105, 0.08);
  border-color: rgba(5, 150, 105, 0.22);
}
.status-pill.is-learning {
  color: var(--le-primary);
  background: rgba(37, 99, 235, 0.08);
  border-color: rgba(37, 99, 235, 0.18);
}

.icon-delete {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: 4px;
  border: none;
  background: transparent;
  color: var(--le-danger);
  cursor: pointer;
  border-radius: 6px;
}
.icon-delete:hover { background: rgba(220, 38, 38, 0.08); }

.word-cards { display: none; flex-direction: column; gap: 10px; min-height: 80px; }
.word-card { padding: 14px 14px 12px; border-radius: 4px 10px 10px 4px; }
.word-card-top {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 10px;
  margin-bottom: 8px;
}
.word-card-title {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 8px;
  min-width: 0;
  flex: 1;
}
.word-card-def {
  margin: 0;
  font-size: 14px;
  line-height: 1.55;
  color: var(--le-text-secondary);
  word-break: break-word;
}
.word-card-ex {
  margin: 8px 0 0;
  font-size: 13px;
  line-height: 1.5;
  color: var(--le-text-muted);
  word-break: break-word;
}

.pagination { display: flex; justify-content: center; margin-top: 20px; }
.mobile-only { display: none; }

@media (max-width: 768px) {
  .desktop-only { display: none !important; }
  .mobile-only { display: flex; }
  .mobile-only-inline { display: block; }

  :deep(.le-page-header) {
    flex-wrap: wrap;
    gap: 10px;
  }

  .header-btn {
    padding: 8px 10px;
  }

  .btn-label {
    display: none;
  }

  .shelf-header {
    flex-direction: column;
    align-items: stretch;
    gap: 10px;
  }

  .shelf-tools {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: 6px;
  }

  .shelf-tools .el-button {
    margin: 0;
    width: 100%;
    padding: 8px 4px;
  }

  .shelf-rail {
    gap: 10px;
    padding: 10px 2px 4px;
    margin: 0 -4px;
    padding-left: 4px;
    padding-right: 20px;
    scroll-padding-left: 4px;
    scroll-snap-type: x mandatory;
  }

  .shelf-hint {
    margin: 6px 0 0;
    font-size: 11px;
    color: var(--le-text-muted);
    text-align: center;
  }

  .shelf-plank {
    height: 10px;
  }

  .vocab-book {
    width: 118px;
    height: 158px;
  }

  .vocab-book:hover {
    transform: none;
  }

  .vocab-book.active {
    transform: translateY(-8px);
  }

  .vocab-book__cover {
    padding: 12px 10px 10px 18px;
  }

  .vocab-book__badge {
    margin-bottom: 6px;
    font-size: 9px;
  }

  .vocab-book__title {
    font-size: 14px;
    -webkit-line-clamp: 2;
  }

  .vocab-book__desc {
    display: none;
  }

  .vocab-book__foot {
    padding-top: 6px;
  }

  .vocab-book__foot strong {
    font-size: 18px;
  }

  .vocab-book--official {
    width: calc(50% - 6px);
    height: auto;
    min-height: 200px;
    flex: 0 0 calc(50% - 6px);
  }

  .shelf-rail--official {
    display: flex;
    flex-wrap: wrap;
    overflow: visible;
    gap: 12px 12px;
    padding-right: 4px;
    scroll-snap-type: none;
  }

  .vocab-book--official .vocab-book__desc {
    display: -webkit-box;
    -webkit-line-clamp: 2;
    font-size: 11px;
  }

  .vocab-book--official .vocab-book__actions {
    margin-top: 8px;
  }

  .ghost-btn,
  .solid-btn {
    height: 34px;
    font-size: 13px;
  }

  .shelf-header--official {
    margin-top: 18px;
  }

  .open-book {
    padding: 16px 12px 14px 36px;
    margin-left: -2px;
    margin-right: -2px;
  }

  .open-book__title {
    font-size: 20px;
  }

  .open-book__stats {
    display: grid !important;
    grid-template-columns: 1fr 1fr;
    gap: 8px;
  }

  .open-book__stats .stat-tile {
    margin: 0;
  }

  .stat-value {
    font-size: 22px;
  }

  .toolbar-actions {
    width: 100%;
  }

  .toolbar-actions .el-button {
    flex: 1;
    min-height: 40px;
  }

  .search-input {
    width: 100%;
  }

  .word-card {
    padding: 12px;
  }

  .pagination {
    margin-top: 14px;
  }

  .pagination :deep(.el-pagination) {
    flex-wrap: wrap;
    justify-content: center;
    gap: 4px;
  }
  .pagination :deep(.el-pagination__sizes),
  .pagination :deep(.el-pagination__jump) { display: none; }

  :deep(.el-dialog) {
    width: 92% !important;
    max-height: 80vh;
    margin-top: 8vh !important;
  }
  :deep(.el-dialog__body) {
    padding: 12px 16px;
    max-height: calc(80vh - 120px);
    overflow-y: auto;
  }
}

.mobile-only-inline { display: none; }

@media (hover: none) {
  .vocab-book:hover {
    transform: none;
  }
}
</style>
