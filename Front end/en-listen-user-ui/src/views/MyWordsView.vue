<template>
  <div class="my-words-page le-page">
    <div class="page-header">
      <h1>
        <el-icon :size="28" color="#409eff"><Notebook /></el-icon>
        我的单词本
      </h1>
      <el-button type="primary" @click="showAddDialog = true">
        <el-icon><Plus /></el-icon>
        添加单词
      </el-button>
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
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getUserWords, addUserWord, deleteUserWord, getWordStats } from '../api/Word.js'

const router = useRouter()

const wordList = ref([])
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)
const searchKeyword = ref('')
const loading = ref(false)
const stats = ref(null)
const showAddDialog = ref(false)
const newWord = ref({ word: '', definition: '', example: '' })

const loadWords = async () => {
  loading.value = true
  try {
    const res = await getUserWords({
      page: page.value,
      pageSize: pageSize.value,
      search: searchKeyword.value
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
  try {
    const res = await getWordStats()
    stats.value = res
  } catch (e) {
    console.error('获取统计失败', e)
  }
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
    await addUserWord(newWord.value)
    ElMessage.success('添加成功')
    showAddDialog.value = false
    newWord.value = { word: '', definition: '', example: '' }
    loadWords()
    loadStats()
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
    loadWords()
    loadStats()
  } catch (e) {
    if (e !== 'cancel') {
      console.error(e)
    }
  }
}

const goToReview = () => {
  router.push({ name: 'wordReview' })
}

const goToFreeReview = () => {
  router.push({ name: 'wordReview', query: { mode: 'free' } })
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

onMounted(() => {
  loadWords()
  loadStats()
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
  margin-bottom: 24px;
}

.page-header h1 {
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 24px;
  color: var(--text-primary);
  margin: 0;
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

/* 操作列样式 */
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

/* 手机端适配 */
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

  /* 操作列缩小 - 只显示图标 */
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

  .el-dialog :deep(.el-form) {
    width: 100%;
  }

  .el-dialog :deep(.el-form-item) {
    margin-bottom: 12px;
  }

  .el-dialog :deep(.el-form-item__label) {
    width: 50px !important;
    padding-right: 6px;
    font-size: 13px;
  }

  .el-dialog :deep(.el-form-item__content) {
    margin-left: 50px !important;
    width: calc(100% - 50px);
  }

  .el-dialog :deep(.el-input),
  .el-dialog :deep(.el-textarea) {
    width: 100%;
  }

  .el-dialog :deep(.el-input__inner),
  .el-dialog :deep(.el-textarea__inner) {
    font-size: 14px;
  }

  /* 添加单词弹窗特殊处理 */
  .add-word-dialog :deep(.el-dialog) {
    width: 95% !important;
    max-width: 400px;
  }

  .add-word-dialog :deep(.el-dialog__body) {
    padding: 16px;
  }

  .add-word-dialog :deep(.el-form-item__label) {
    width: 50px !important;
  }

  .add-word-dialog :deep(.el-form-item__content) {
    margin-left: 50px !important;
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
