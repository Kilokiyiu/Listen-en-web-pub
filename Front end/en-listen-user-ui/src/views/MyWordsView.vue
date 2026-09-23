<template>
  <div class="my-words-page le-page">
    <div class="page-header">
      <h1>
        <el-icon :size="28" color="#409eff"><Notebook /></el-icon>
        我的单词本
      </h1>
      <el-button type="primary" @click="showAddDialog = true" :disabled="!currentBookId">
        <el-icon><Plus /></el-icon>
        添加单词
      </el-button>
    </div>

    <!-- 单词本切换 -->
    <div class="book-bar">
      <div class="book-select-wrap">
        <span class="book-label">当前单词本</span>
        <el-select
          v-model="currentBookId"
          placeholder="选择单词本"
          filterable
          style="width: 220px"
          @change="onBookChange"
        >
          <el-option
            v-for="book in wordBooks"
            :key="book.id"
            :label="`${book.name} (${book.wordCount})`"
            :value="book.id"
          />
        </el-select>
      </div>
      <div class="book-actions">
        <el-button @click="openCreateBook">
          <el-icon><FolderAdd /></el-icon>
          新建
        </el-button>
        <el-button @click="openRenameBook" :disabled="!currentBook">
          <el-icon><Edit /></el-icon>
          重命名
        </el-button>
        <el-button
          type="danger"
          plain
          @click="handleDeleteBook"
          :disabled="!currentBook || currentBook.isDefault"
        >
          <el-icon><Delete /></el-icon>
          删除
        </el-button>
      </div>
    </div>

    <!-- 统计信息 -->
    <el-row :gutter="16" class="stats-row" v-if="stats">
      <el-col :xs="12" :sm="6">
        <el-card class="stat-card">
          <div class="stat-value">{{ stats.totalWords }}</div>
          <div class="stat-label">总单词数</div>
        </el-card>
      </el-col>
      <el-col :xs="12" :sm="6">
        <el-card class="stat-card">
          <div class="stat-value" style="color: #e6a23c">{{ stats.dueCount }}</div>
          <div class="stat-label">待复习</div>
        </el-card>
      </el-col>
      <el-col :xs="12" :sm="6">
        <el-card class="stat-card">
          <div class="stat-value" style="color: #67c23a">{{ stats.masteredCount }}</div>
          <div class="stat-label">已掌握</div>
        </el-card>
      </el-col>
      <el-col :xs="12" :sm="6">
        <el-card class="stat-card">
          <div class="stat-value" style="color: #409eff">{{ stats.reviewLogsCount }}</div>
          <div class="stat-label">复习次数</div>
        </el-card>
      </el-col>
    </el-row>

    <!-- 搜索和操作 -->
    <div class="toolbar">
      <el-input
        v-model="searchKeyword"
        placeholder="搜索单词..."
        clearable
        style="width: 240px"
        @keyup.enter="handleSearch"
      >
        <template #append>
          <el-button @click="handleSearch">
            <el-icon><Search /></el-icon>
          </el-button>
        </template>
      </el-input>
      <el-button type="success" @click="goToReview" v-if="stats?.dueCount > 0">
        <el-icon><Timer /></el-icon>
        开始复习 ({{ stats.dueCount }})
      </el-button>
      <el-button type="warning" @click="goToFreeReview" v-if="stats?.totalWords > 0">
        <el-icon><Refresh /></el-icon>
        自由复习
      </el-button>
    </div>

    <!-- 单词列表 -->
    <el-table :data="wordList" style="width: 100%" v-loading="loading">
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
      <el-table-column prop="nextReview" label="下次复习" width="120">
        <template #default="{ row }">
          <el-tag
            :type="getReviewStatus(row).type"
            size="small"
          >
            {{ getReviewStatus(row).text }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="80" fixed="right" class-name="action-column">
        <template #default="{ row }">
          <el-button
            type="danger"
            link
            size="small"
            class="delete-btn"
            @click="deleteWord(row.id)"
          >
            <el-icon><Delete /></el-icon>
          </el-button>
        </template>
      </el-table-column>
    </el-table>

    <!-- 分页 -->
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
        <el-button type="primary" @click="addWord">添加</el-button>
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
        <el-button type="primary" @click="submitBookForm">确定</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
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
  setCurrentWordBookId
} from '../api/Word.js'

const router = useRouter()

const wordList = ref([])
const wordBooks = ref([])
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

const getReviewStatus = (word) => {
  if (!word.nextReview) {
    return { type: 'info', text: '新词' }
  }
  const now = new Date()
  const next = new Date(word.nextReview)
  if (next <= now) {
    return { type: 'warning', text: '待复习' }
  }
  if (word.interval >= 21) {
    return { type: 'success', text: '已掌握' }
  }
  return { type: 'primary', text: '复习中' }
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
    await Promise.all([loadWords(), loadStats()])
  } catch (e) {
    console.error(e)
  }
})
</script>

<style scoped>
.my-words-page {
  padding: 24px;
  max-width: 1200px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.page-header h1 {
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 24px;
  color: var(--text-primary);
  margin: 0;
}

.book-bar {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 20px;
  padding: 12px 16px;
  background: var(--bg-elevated);
  border-radius: 8px;
}

.book-select-wrap {
  display: flex;
  align-items: center;
  gap: 10px;
}

.book-label {
  font-size: 14px;
  color: var(--text-secondary);
  white-space: nowrap;
}

.book-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.stats-row {
  margin-bottom: 24px;
}

.stat-card {
  text-align: center;
}

.stat-value {
  font-size: 28px;
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 4px;
}

.stat-label {
  font-size: 13px;
  color: var(--text-muted);
}

.toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.word-text {
  font-weight: 500;
  color: var(--text-primary);
}

.definition-text {
  color: var(--text-secondary);
}

.example-text {
  color: var(--text-muted);
  font-size: 13px;
}

.pagination {
  display: flex;
  justify-content: center;
  margin-top: 24px;
}

:deep(.action-column) {
  text-align: center;
}

:deep(.action-column .cell) {
  padding: 0 4px;
  text-align: center;
}

.delete-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

@media (max-width: 768px) {
  .my-words-page {
    padding: 12px;
  }

  .page-header {
    flex-direction: row;
    gap: 12px;
    align-items: center;
    justify-content: space-between;
  }

  .page-header .el-button {
    flex-shrink: 0;
    width: auto;
    min-width: 100px;
  }

  .page-header h1 {
    font-size: 20px;
  }

  .page-header h1 :deep(.el-icon) {
    font-size: 22px !important;
  }

  .book-bar {
    flex-direction: column;
    align-items: stretch;
  }

  .book-select-wrap {
    width: 100%;
  }

  .book-select-wrap .el-select {
    flex: 1;
    width: auto !important;
  }

  .book-actions {
    width: 100%;
  }

  .book-actions .el-button {
    flex: 1;
  }

  .stats-row {
    margin-bottom: 12px;
  }

  .stat-value {
    font-size: 22px;
  }

  .toolbar {
    flex-wrap: wrap;
    gap: 8px;
    justify-content: stretch;
  }

  .toolbar .el-input {
    width: 100% !important;
  }

  .toolbar .el-button {
    flex: 0 0 auto;
    min-width: 100px;
  }

  .el-table {
    font-size: 13px;
  }

  .el-table :deep(.el-table__cell) {
    padding: 6px 4px;
  }

  .el-table :deep(.el-table__cell:nth-child(3)),
  .el-table :deep(.el-table__cell:nth-child(4)) {
    display: none;
  }

  .el-table :deep(.action-column) {
    width: 50px !important;
    min-width: 50px !important;
    max-width: 50px !important;
    padding: 6px 4px !important;
  }

  .el-table :deep(.action-column .cell) {
    padding: 0 !important;
    text-align: center;
  }

  .el-table :deep(.action-column .delete-btn) {
    padding: 4px;
    font-size: 16px;
    margin: 0 auto;
    display: inline-flex;
  }

  .el-dialog {
    width: 95% !important;
    max-height: 80vh;
    margin-top: 10vh !important;
    margin-bottom: 10vh !important;
  }

  .el-dialog :deep(.el-dialog__body) {
    padding: 12px 16px;
    max-height: calc(80vh - 120px);
    overflow-y: auto;
  }

  .pagination :deep(.el-pagination) {
    flex-wrap: wrap;
    justify-content: center;
    gap: 4px;
  }

  .pagination :deep(.el-pagination .el-pagination__jump) {
    display: none;
  }
}
</style>
