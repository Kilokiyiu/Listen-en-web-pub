<template>
  <div class="word-roots-page le-page">
    <div class="page-header">
      <h1>
        <el-icon :size="28" color="#67c23a"><Collection /></el-icon>
        词根学习
      </h1>
      <p class="subtitle">掌握词根词缀，轻松记忆单词</p>
    </div>

    <!-- 学习进度 -->
    <el-card class="progress-card" v-if="progress">
      <div class="progress-header">
        <span>学习进度</span>
        <el-tag type="success">已掌握 {{ progress.masteredCount }}/{{ progress.totalRoots }}</el-tag>
      </div>
      <el-progress
        :percentage="Math.round((progress.masteredCount / progress.totalRoots) * 100)"
        :stroke-width="16"
        status="success"
      />
      <div class="progress-actions">
        <el-button type="primary" @click="goToNext">
          <el-icon><ArrowRight /></el-icon>
          继续学习
        </el-button>
        <el-button @click="goToMyWords">
          <el-icon><Notebook /></el-icon>
          我的单词本
        </el-button>
        <el-button @click="goToReview">
          <el-icon><Timer /></el-icon>
          开始复习
        </el-button>
      </div>
    </el-card>

    <!-- 筛选和搜索 -->
    <div class="filter-bar">
      <el-radio-group v-model="filterOrigin" @change="handleFilterChange">
        <el-radio-button label="">全部</el-radio-button>
        <el-radio-button label="Latin">拉丁语</el-radio-button>
        <el-radio-button label="Greek">希腊语</el-radio-button>
      </el-radio-group>
      <el-input
        v-model="searchKeyword"
        placeholder="搜索词根或含义..."
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
    </div>

    <!-- 词根列表 -->
    <el-row :gutter="16">
      <el-col
        v-for="root in wordRoots"
        :key="root.id"
        :xs="12" :sm="8" :md="6"
      >
        <el-card
          shadow="hover"
          class="root-card"
          :class="{ mastered: isMastered(root.id) }"
          @click="goToDetail(root.id)"
        >
          <div class="root-header">
            <span class="root-text">{{ root.root }}</span>
            <el-tag size="small" :type="root.origin === 'Latin' ? 'primary' : 'warning'">
              {{ root.origin === 'Latin' ? '拉丁' : '希腊' }}
            </el-tag>
          </div>
          <div class="root-meaning">{{ root.meaning }}</div>
          <div class="root-desc" v-if="root.meaningEn">{{ root.meaningEn }}</div>
          <div class="root-footer">
            <el-icon><Document /></el-icon>
            <span>{{ root.exampleCount }} 个例词</span>
          </div>
          <el-icon v-if="isMastered(root.id)" class="mastered-icon" color="#67c23a"><CircleCheck /></el-icon>
        </el-card>
      </el-col>
    </el-row>

    <!-- 分页 -->
    <div class="pagination">
      <el-pagination
        v-model:current-page="page"
        v-model:page-size="pageSize"
        :total="total"
        :page-sizes="[20, 40, 60]"
        layout="total, sizes, prev, pager, next"
        @size-change="handleSizeChange"
        @current-change="handlePageChange"
      />
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getWordRoots, getWordRootProgress, getNextWordRoot } from '../api/Word.js'

const router = useRouter()

const wordRoots = ref([])
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)
const filterOrigin = ref('')
const searchKeyword = ref('')
const progress = ref(null)
const masteredIds = ref([])

const loadWordRoots = async () => {
  try {
    const res = await getWordRoots({
      page: page.value,
      pageSize: pageSize.value,
      origin: filterOrigin.value,
      search: searchKeyword.value
    })
    wordRoots.value = res.items || []
    total.value = res.total || 0
  } catch (e) {
    console.error('获取词根失败', e)
  }
}

const loadProgress = async () => {
  try {
    const res = await getWordRootProgress()
    progress.value = res
    masteredIds.value = res.masteredIds || []
  } catch (e) {
    console.error('获取进度失败', e)
  }
}

const isMastered = (id) => masteredIds.value.includes(id)

const goToDetail = (id) => {
  router.push({ name: 'wordRootDetail', params: { id } })
}

const goToNext = async () => {
  try {
    const res = await getNextWordRoot()
    if (res) {
      goToDetail(res)
    } else {
      ElMessage.success('恭喜！你已经学完了所有词根')
    }
  } catch (e) {
    console.error(e)
  }
}

const goToMyWords = () => {
  router.push({ name: 'myWords' })
}

const goToReview = () => {
  router.push({ name: 'wordReview' })
}

const handleFilterChange = () => {
  page.value = 1
  loadWordRoots()
}

const handleSearch = () => {
  page.value = 1
  loadWordRoots()
}

const handleSizeChange = (size) => {
  pageSize.value = size
  loadWordRoots()
}

const handlePageChange = () => {
  loadWordRoots()
}

onMounted(() => {
  loadWordRoots()
  // 只有登录用户才加载个人进度
  if (localStorage.getItem('token') && localStorage.getItem('userId')) {
    loadProgress()
  }
})
</script>

<style scoped>
.word-roots-page {
  padding: 24px;
  max-width: 1200px;
  margin: 0 auto;
}

.page-header {
  text-align: center;
  margin-bottom: 32px;
}

.page-header h1 {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  font-size: 28px;
  color: var(--text-primary);
  margin-bottom: 8px;
}

.subtitle {
  color: var(--text-secondary);
  font-size: 16px;
}

.progress-card {
  margin-bottom: 24px;
}

.progress-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
  font-weight: 500;
}

.progress-actions {
  margin-top: 16px;
  display: flex;
  gap: 12px;
}

.filter-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.root-card {
  margin-bottom: 16px;
  cursor: pointer;
  transition: all 0.3s ease;
  position: relative;
}

.root-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
}

.root-card.mastered {
  border-color: #67c23a;
  background: rgba(103, 194, 58, 0.05);
}

.root-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.root-text {
  font-size: 20px;
  font-weight: 600;
  color: var(--text-primary);
}

.root-meaning {
  font-size: 14px;
  color: var(--text-secondary);
  margin-bottom: 4px;
}

.root-desc {
  font-size: 12px;
  color: var(--text-muted);
  margin-bottom: 8px;
}

.root-footer {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 12px;
  color: var(--text-muted);
}

.mastered-icon {
  position: absolute;
  top: 8px;
  right: 8px;
  font-size: 20px;
}

.pagination {
  display: flex;
  justify-content: center;
  margin-top: 32px;
}

/* 手机端适配 */
@media (max-width: 768px) {
  .word-roots-page {
    padding: 12px;
  }

  .page-header h1 {
    font-size: 22px;
  }

  .subtitle {
    font-size: 14px;
  }

  .progress-actions {
    flex-wrap: wrap;
  }

  .progress-actions .el-button {
    flex: 1;
    min-width: 0;
  }

  .filter-bar {
    flex-direction: column;
    gap: 12px;
    align-items: stretch;
  }

  .filter-bar .el-input {
    width: 100% !important;
  }

  .filter-bar :deep(.el-radio-group) {
    display: flex;
    width: 100%;
  }

  .filter-bar :deep(.el-radio-button) {
    flex: 1;
  }

  .pagination :deep(.el-pagination) {
    flex-wrap: wrap;
    justify-content: center;
    gap: 4px;
  }

  .pagination :deep(.el-pagination .el-pagination__jump) {
    display: none;
  }

  .pagination :deep(.el-pagination .el-pagination__sizes) {
    display: none;
  }
}
</style>
