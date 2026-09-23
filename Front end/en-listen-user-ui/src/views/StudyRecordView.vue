<template>
  <div class="study-page le-page">
    <div class="page-header">
      <el-button text @click="$router.back()" class="back-btn">
        <el-icon><ArrowLeft /></el-icon> 返回
      </el-button>
    </div>

    <div class="title-section">
      <div class="title-icon">
        <el-icon :size="28" color="#409eff"><TrendCharts /></el-icon>
      </div>
      <div class="title-text">
        <h1 class="page-title">学习记录</h1>
        <p class="page-subtitle">听力与阅读完成记录，真实追踪你的进度</p>
      </div>
    </div>

    <div class="stats-row">
      <div class="stat-card">
        <div class="stat-card-inner">
          <div class="stat-icon-wrap icon-blue">
            <el-icon :size="22" color="#409eff"><Headset /></el-icon>
          </div>
          <div class="stat-info">
            <div class="stat-num">{{ summary.totalListen }}</div>
            <div class="stat-desc">完成听力</div>
          </div>
        </div>
        <div class="stat-bar bar-blue"></div>
      </div>
      <div class="stat-card">
        <div class="stat-card-inner">
          <div class="stat-icon-wrap icon-green">
            <el-icon :size="22" color="#67c23a"><Reading /></el-icon>
          </div>
          <div class="stat-info">
            <div class="stat-num">{{ summary.totalArticle }}</div>
            <div class="stat-desc">完成阅读</div>
          </div>
        </div>
        <div class="stat-bar bar-green"></div>
      </div>
      <div class="stat-card">
        <div class="stat-card-inner">
          <div class="stat-icon-wrap icon-orange">
            <el-icon :size="22" color="#e6a23c"><Timer /></el-icon>
          </div>
          <div class="stat-info">
            <div class="stat-num">{{ summary.totalMinutes }}</div>
            <div class="stat-desc">学习时长(分钟)</div>
          </div>
        </div>
        <div class="stat-bar bar-orange"></div>
      </div>
      <div class="stat-card">
        <div class="stat-card-inner">
          <div class="stat-icon-wrap icon-red">
            <el-icon :size="22" color="#f56c6c"><Calendar /></el-icon>
          </div>
          <div class="stat-info">
            <div class="stat-num">{{ summary.streakDays }}</div>
            <div class="stat-desc">连续学习(天)</div>
          </div>
        </div>
        <div class="stat-bar bar-red"></div>
      </div>
    </div>

    <div class="record-section">
      <div class="record-header">
        <div class="record-title-wrap">
          <div class="title-bar"></div>
          <span class="record-title">最近学习</span>
          <span class="record-count">共 {{ total }} 条</span>
        </div>
        <el-radio-group v-model="filterType" size="small" class="filter-group" @change="onFilterChange">
          <el-radio-button label="all">全部</el-radio-button>
          <el-radio-button label="listen">听力</el-radio-button>
          <el-radio-button label="article">阅读</el-radio-button>
          <el-radio-button label="CET-4">CET-4</el-radio-button>
          <el-radio-button label="CET-6">CET-6</el-radio-button>
        </el-radio-group>
      </div>

      <div v-if="loading" class="empty-wrap">
        <el-icon class="is-loading" :size="28"><Loading /></el-icon>
      </div>

      <el-empty v-else-if="records.length === 0" description="还没有学习记录，去听一套真题或读一篇短文吧" />

      <div v-else class="record-list">
        <div
          v-for="item in records"
          :key="item.id"
          class="record-item"
          @click="openRecord(item)"
        >
          <div class="record-left">
            <div class="record-name-row">
              <span class="record-name">{{ item.title }}</span>
              <span class="record-tag" :class="tagClass(item)">
                {{ tagLabel(item) }}
              </span>
            </div>
            <div class="record-meta">
              <span class="meta-item">
                <el-icon><Clock /></el-icon>
                {{ formatTime(item.updatedAt) }}
              </span>
              <span class="meta-item">
                <el-icon><Timer /></el-icon>
                {{ formatDuration(item.durationSeconds) }}
              </span>
            </div>
          </div>
          <div class="record-right">
            <span class="done-badge">已完成</span>
          </div>
        </div>
      </div>

      <div v-if="total > pageSize" class="pagination-wrap">
        <el-pagination
          :current-page="currentPage"
          :page-size="pageSize"
          :total="total"
          layout="prev, pager, next"
          @current-change="handlePageChange"
        />
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { Headset, Timer, TrendCharts, Calendar, Clock, ArrowLeft, Reading, Loading } from '@element-plus/icons-vue'
import { getStudySummary, getStudyList } from '@/api/Study'

const router = useRouter()

const filterType = ref('all')
const currentPage = ref(1)
const pageSize = ref(10)
const total = ref(0)
const loading = ref(false)
const records = ref([])

const summary = ref({
  totalListen: 0,
  totalArticle: 0,
  totalMinutes: 0,
  streakDays: 0,
})

const unwrap = (res) => res?.data ?? res

const loadSummary = async () => {
  try {
    const res = await getStudySummary()
    const data = unwrap(res) || {}
    summary.value = {
      totalListen: data.totalListen ?? 0,
      totalArticle: data.totalArticle ?? 0,
      totalMinutes: data.totalMinutes ?? 0,
      streakDays: data.streakDays ?? 0,
    }
  } catch {
    /* interceptor */
  }
}

const buildListParams = () => {
  const params = { page: currentPage.value, pageSize: pageSize.value }
  if (filterType.value === 'listen' || filterType.value === 'article') {
    params.activityType = filterType.value
  } else if (filterType.value === 'CET-4' || filterType.value === 'CET-6') {
    params.category = filterType.value
  }
  return params
}

const loadList = async () => {
  loading.value = true
  try {
    const res = await getStudyList(buildListParams())
    const data = unwrap(res) || {}
    records.value = data.items || []
    total.value = data.total ?? 0
  } catch {
    records.value = []
    total.value = 0
  } finally {
    loading.value = false
  }
}

const onFilterChange = () => {
  currentPage.value = 1
  loadList()
}

const handlePageChange = (page) => {
  currentPage.value = page
  loadList()
}

const tagLabel = (item) => {
  if (item.category && item.category !== 'other' && item.category !== 'daily') return item.category
  return item.activityType === 'article' ? '阅读' : '听力'
}

const tagClass = (item) => {
  const cat = (item.category || '').toUpperCase()
  if (cat.includes('4')) return 'tag-cet4'
  if (cat.includes('6')) return 'tag-cet6'
  if (item.activityType === 'article') return 'tag-article'
  return 'tag-cet4'
}

const formatTime = (dateStr) => {
  if (!dateStr) return '-'
  const d = new Date(dateStr)
  if (Number.isNaN(d.getTime())) return dateStr
  const pad = (n) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`
}

const formatDuration = (seconds) => {
  const s = Math.max(0, Number(seconds) || 0)
  const minutes = Math.round(s / 60)
  if (minutes < 1) return `${s}秒`
  const h = Math.floor(minutes / 60)
  const m = minutes % 60
  if (h > 0) return `${h}时${m}分`
  return `${m}分钟`
}

const openRecord = (item) => {
  if (item.activityType === 'article') {
    router.push({ name: 'dailyArticle' })
    return
  }
  router.push({ name: 'examDetail', query: { albumId: item.contentId } })
}

onMounted(async () => {
  await Promise.all([loadSummary(), loadList()])
})
</script>

<style scoped>
.study-page {
  padding: 0;
}

.page-header {
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

.title-section {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 28px 28px 20px;
}

.title-icon {
  width: 48px;
  height: 48px;
  border-radius: 14px;
  background: linear-gradient(135deg, rgba(64, 158, 255, 0.1) 0%, rgba(0, 168, 232, 0.1) 100%);
  display: flex;
  align-items: center;
  justify-content: center;
}

.title-text {
  flex: 1;
}

.page-title {
  margin: 0;
  font-size: 24px;
  font-weight: 700;
  color: var(--text-primary);
}

.page-subtitle {
  margin: 4px 0 0;
  font-size: 13px;
  color: var(--text-muted);
}

.stats-row {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 16px;
  padding: 0 28px;
  margin-bottom: 24px;
}

.stat-card {
  position: relative;
  overflow: hidden;
  background: var(--bg-card);
  border: 1px solid var(--border-glass);
  border-radius: 14px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.04);
  transition: all 0.3s ease;
}

.stat-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.08);
}

.stat-card-inner {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 20px 18px 16px;
}

.stat-icon-wrap {
  width: 44px;
  height: 44px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.stat-icon-wrap.icon-blue {
  background: linear-gradient(135deg, rgba(64, 158, 255, 0.12) 0%, rgba(64, 158, 255, 0.06) 100%);
}

.stat-icon-wrap.icon-green {
  background: linear-gradient(135deg, rgba(103, 194, 58, 0.12) 0%, rgba(103, 194, 58, 0.06) 100%);
}

.stat-icon-wrap.icon-orange {
  background: linear-gradient(135deg, rgba(230, 162, 60, 0.12) 0%, rgba(230, 162, 60, 0.06) 100%);
}

.stat-icon-wrap.icon-red {
  background: linear-gradient(135deg, rgba(245, 108, 108, 0.12) 0%, rgba(245, 108, 108, 0.06) 100%);
}

.stat-info {
  flex: 1;
}

.stat-num {
  font-size: 26px;
  font-weight: 700;
  color: var(--text-primary);
  line-height: 1.2;
}

.stat-desc {
  font-size: 12px;
  color: var(--text-muted);
  margin-top: 2px;
}

.stat-bar {
  height: 3px;
  border-radius: 0 0 14px 14px;
  opacity: 0.6;
}

.stat-bar.bar-blue { background: linear-gradient(90deg, #409eff, #79bbff); }
.stat-bar.bar-green { background: linear-gradient(90deg, #67c23a, #95d475); }
.stat-bar.bar-orange { background: linear-gradient(90deg, #e6a23c, #eebe77); }
.stat-bar.bar-red { background: linear-gradient(90deg, #f56c6c, #f89898); }

.record-section {
  margin: 0 28px 24px;
  background: var(--bg-card);
  border: 1px solid var(--border-glass);
  border-radius: 14px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.04);
  overflow: hidden;
}

.record-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px 24px;
  border-bottom: 1px solid var(--le-border);
  flex-wrap: wrap;
  gap: 12px;
}

.record-title-wrap {
  display: flex;
  align-items: center;
  gap: 10px;
}

.title-bar {
  width: 4px;
  height: 18px;
  border-radius: 2px;
  background: linear-gradient(180deg, var(--accent-blue), var(--accent-cyan));
}

.record-title {
  font-size: 16px;
  font-weight: 600;
  color: var(--text-primary);
}

.record-count {
  font-size: 12px;
  color: var(--text-muted);
  background: var(--le-bg-muted);
  border: 1px solid var(--le-border);
  padding: 2px 10px;
  border-radius: 10px;
}

.filter-group :deep(.el-radio-button__inner) {
  background: var(--le-bg-muted);
  border-color: var(--border-glass);
  color: var(--text-secondary);
}

.filter-group :deep(.el-radio-button__original-radio:checked + .el-radio-button__inner) {
  background: linear-gradient(135deg, var(--accent-blue) 0%, var(--accent-cyan) 100%);
  border-color: transparent;
  color: #fff;
}

.empty-wrap {
  display: flex;
  justify-content: center;
  padding: 48px 0;
  color: var(--text-muted);
}

.record-list {
  padding: 8px 0;
}

.record-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 18px 24px;
  cursor: pointer;
  transition: all 0.25s ease;
  border-bottom: 1px solid var(--le-border);
}

.record-item:last-child {
  border-bottom: none;
}

.record-item:hover {
  background: linear-gradient(90deg, rgba(37, 99, 235, 0.12) 0%, rgba(6, 182, 212, 0.06) 100%);
}

.record-left {
  flex: 1;
  min-width: 0;
}

.record-name-row {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 8px;
}

.record-name {
  font-size: 15px;
  font-weight: 500;
  color: var(--text-primary);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.record-tag {
  display: inline-block;
  padding: 2px 10px;
  border-radius: 20px;
  font-size: 11px;
  font-weight: 600;
  flex-shrink: 0;
}

.record-tag.tag-cet4 {
  background: rgba(64, 158, 255, 0.1);
  color: var(--accent-blue);
}

.record-tag.tag-cet6 {
  background: rgba(139, 92, 246, 0.1);
  color: #8b5cf6;
}

.record-tag.tag-article {
  background: rgba(103, 194, 58, 0.12);
  color: #67c23a;
}

.record-meta {
  display: flex;
  gap: 16px;
}

.meta-item {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 12px;
  color: var(--text-muted);
}

.meta-item :deep(.el-icon) {
  font-size: 13px;
}

.record-right {
  margin-left: 20px;
}

.done-badge {
  display: inline-block;
  padding: 4px 12px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 600;
  color: #67c23a;
  background: rgba(103, 194, 58, 0.1);
}

.pagination-wrap {
  display: flex;
  justify-content: center;
  padding: 20px 0 24px;
}

@media (max-width: 768px) {
  .stats-row {
    grid-template-columns: repeat(2, 1fr);
  }
  .record-item {
    flex-direction: column;
    align-items: flex-start;
    gap: 12px;
  }
  .record-right {
    margin-left: 0;
  }
}
</style>
