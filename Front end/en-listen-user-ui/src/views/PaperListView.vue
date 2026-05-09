<template>
  <div class="exam-list-page">
    <!-- 顶部导航 -->
    <div class="list-header">
      <el-button text @click="router.back()" class="back-btn">
        <el-icon><ArrowLeft /></el-icon> 返回首页
      </el-button>
    </div>

    <!-- 标题区 -->
    <div class="title-section">
      <div class="title-bar"></div>
      <h1 class="page-title">{{ pageTitle }}</h1>
      <span class="page-count">共 {{ totalCount }} 套</span>
    </div>

    <!-- 真题列表 -->
    <el-row :gutter="20">
      <el-col
        v-for="item in examList"
        :key="item.id"
        :xs="12" :sm="8" :md="6"
      >
        <div class="exam-card" @click="goDetail(item.id)">
          <div class="exam-card-top">
            <span class="exam-tag" :class="categoryTag">{{ categoryLabel }}</span>
          </div>
          <div class="exam-title">{{ item.title }}</div>
          <div class="exam-card-bottom">
            <div class="exam-info">
              <el-icon><Headset /></el-icon>
              <span>完整听力</span>
            </div>
            <el-icon class="exam-arrow"><ArrowRight /></el-icon>
          </div>
        </div>
      </el-col>
    </el-row>

    <!-- 分页 -->
    <div class="pagination-wrapper">
      <el-pagination
        :current-page="currentPage"
        :page-size="pageSize"
        :total="totalCount"
        layout="prev, pager, next, total"
        @current-change="handlePageChange"
      />
    </div>
  </div>
</template>

<script setup>
import { computed, ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getAlbumsByCategoryId } from '../api/Listen.js'

const route = useRoute()
const router = useRouter()

// 从 URL 获取分类ID
const categoryId = computed(() => route.query.categoryId)

// 页面标题
const pageTitle = ref('听力真题')
const categoryTag = ref('cet6')
const categoryLabel = ref('CET-6')

// 试卷列表（从后端获取）
const albumList = ref([])
const loading = ref(false)

// 分页
const currentPage = ref(1)
const pageSize = ref(8)

const examList = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return albumList.value.slice(start, end)
})

const totalCount = computed(() => albumList.value.length)

const handlePageChange = (page) => {
  currentPage.value = page
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

const goDetail = (albumId) => {
  router.push({ name: 'examDetail', query: { albumId } })
}

// 加载试卷数据
const loadAlbums = async () => {
  if (!categoryId.value) return
  loading.value = true
  try {
    const data = await getAlbumsByCategoryId(categoryId.value)
    albumList.value = (data || []).map(a => ({
      id: a.id,
      title: a.name?.chinese || a.name,
      date: ''
    }))
    if (albumList.value.length > 0) {
      pageTitle.value = '六级听力真题'
      categoryTag.value = 'cet6'
      categoryLabel.value = 'CET-6'
    }
  } catch (e) {
    console.error('获取试卷失败', e)
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadAlbums()
})
</script>

<style scoped>
.exam-list-page {
  padding: 0;
}

/* 顶部导航 */
.list-header {
  padding: 16px 24px;
  border-bottom: 1px solid #eef1f6;
}

.back-btn {
  color: var(--text-secondary) !important;
  font-size: 14px;
}

.back-btn:hover {
  color: var(--accent-blue) !important;
}

/* 标题区 */
.title-section {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 24px 28px 20px;
}

.title-bar {
  width: 4px;
  height: 22px;
  border-radius: 2px;
  background: linear-gradient(180deg, var(--accent-blue), var(--accent-cyan));
}

.page-title {
  font-size: 24px;
  font-weight: 700;
  color: var(--text-primary);
}

.page-count {
  font-size: 13px;
  color: var(--text-muted);
  background: #f4f6fb;
  padding: 3px 12px;
  border-radius: 12px;
}

/* 试卷卡片 */
.exam-card {
  margin-bottom: 20px;
  cursor: pointer;
  transition: all 0.3s ease;
  background: var(--bg-card);
  border: 1px solid var(--border-glass);
  border-radius: 14px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.05);
  padding: 20px;
  position: relative;
  overflow: hidden;
}

.exam-card::after {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 3px;
  background: linear-gradient(90deg, var(--accent-blue), var(--accent-cyan));
  opacity: 0;
  transition: opacity 0.3s;
}

.exam-card:hover {
  transform: translateY(-6px);
  box-shadow: 0 12px 32px rgba(0, 0, 0, 0.1);
  border-color: rgba(64, 158, 255, 0.15);
}

.exam-card:hover::after {
  opacity: 1;
}

.exam-card-top {
  margin-bottom: 12px;
}

.exam-tag {
  display: inline-block;
  padding: 4px 14px;
  border-radius: 20px;
  font-size: 11px;
  font-weight: 600;
  letter-spacing: 0.5px;
}

.exam-tag.cet4 {
  background: rgba(64, 158, 255, 0.1);
  color: var(--accent-blue);
}

.exam-tag.cet6 {
  background: rgba(139, 92, 246, 0.1);
  color: var(--accent-purple);
}

.exam-title {
  font-size: 15px;
  font-weight: 500;
  color: var(--text-primary);
  line-height: 1.5;
  margin-bottom: 16px;
  min-height: 45px;
}

.exam-card-bottom {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-top: 1px solid #f0f3f8;
  padding-top: 12px;
}

.exam-info {
  display: flex;
  align-items: center;
  gap: 6px;
  color: var(--text-muted);
  font-size: 13px;
}

.exam-arrow {
  color: #d0d5e0;
  transition: all 0.3s;
}

.exam-card:hover .exam-arrow {
  color: var(--accent-blue);
  transform: translateX(3px);
}

/* 分页 */
.pagination-wrapper {
  display: flex;
  justify-content: center;
  margin-top: 40px;
  padding-bottom: 20px;
}

.pagination-wrapper :deep(.el-pagination) {
  --el-pagination-text-color: var(--text-secondary);
  --el-pagination-button-color: var(--text-secondary);
}

.pagination-wrapper :deep(.el-pager li) {
  background: var(--bg-card) !important;
  border: 1px solid var(--border-glass);
  color: var(--text-secondary);
  border-radius: 8px;
}

.pagination-wrapper :deep(.el-pager li.is-active) {
  background: linear-gradient(135deg, var(--accent-blue) 0%, var(--accent-cyan) 100%) !important;
  border-color: transparent;
  color: #fff;
  box-shadow: 0 0 12px rgba(64, 158, 255, 0.3);
}
</style>
