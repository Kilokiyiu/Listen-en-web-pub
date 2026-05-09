<template>
  <div class="study-page">
    <!-- 顶部导航 -->
    <div class="page-header">
      <el-button text @click="$router.back()" class="back-btn">
        <el-icon><ArrowLeft /></el-icon> 返回
      </el-button>
    </div>

    <!-- 标题区 -->
    <div class="title-section">
      <div class="title-icon">
        <el-icon :size="28" color="#409eff"><TrendCharts /></el-icon>
      </div>
      <div class="title-text">
        <h1 class="page-title">学习记录</h1>
        <p class="page-subtitle">追踪你的学习进度，持续进步</p>
      </div>
    </div>

    <!-- 统计卡片 -->
    <div class="stats-row">
      <div class="stat-card">
        <div class="stat-card-inner">
          <div class="stat-icon-wrap icon-blue">
            <el-icon :size="22" color="#409eff"><Document /></el-icon>
          </div>
          <div class="stat-info">
            <div class="stat-num">{{ summary.totalExams }}</div>
            <div class="stat-desc">完成试卷</div>
          </div>
        </div>
        <div class="stat-bar bar-blue"></div>
      </div>
      <div class="stat-card">
        <div class="stat-card-inner">
          <div class="stat-icon-wrap icon-green">
            <el-icon :size="22" color="#67c23a"><Timer /></el-icon>
          </div>
          <div class="stat-info">
            <div class="stat-num">{{ summary.totalMinutes }}</div>
            <div class="stat-desc">学习时长(分钟)</div>
          </div>
        </div>
        <div class="stat-bar bar-green"></div>
      </div>
      <div class="stat-card">
        <div class="stat-card-inner">
          <div class="stat-icon-wrap icon-orange">
            <el-icon :size="22" color="#e6a23c"><TrendCharts /></el-icon>
          </div>
          <div class="stat-info">
            <div class="stat-num">{{ summary.avgAccuracy }}%</div>
            <div class="stat-desc">平均正确率</div>
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

    <!-- 记录列表 -->
    <div class="record-section">
      <div class="record-header">
        <div class="record-title-wrap">
          <div class="title-bar"></div>
          <span class="record-title">最近学习</span>
          <span class="record-count">共 {{ filteredRecords.length }} 条</span>
        </div>
        <el-radio-group v-model="filterType" size="small" class="filter-group">
          <el-radio-button label="all">全部</el-radio-button>
          <el-radio-button label="cet4">CET-4</el-radio-button>
          <el-radio-button label="cet6">CET-6</el-radio-button>
        </el-radio-group>
      </div>

      <div class="record-list">
        <div
          v-for="item in filteredRecords"
          :key="item.id"
          class="record-item"
          @click="reviewExam(item)"
        >
          <div class="record-left">
            <div class="record-name-row">
              <span class="record-name">{{ item.examName }}</span>
              <span class="record-tag" :class="item.type === 'CET-4' ? 'tag-cet4' : 'tag-cet6'">
                {{ item.type }}
              </span>
            </div>
            <div class="record-meta">
              <span class="meta-item">
                <el-icon><Clock /></el-icon>
                {{ item.studyTime }}
              </span>
              <span class="meta-item">
                <el-icon><Timer /></el-icon>
                {{ formatDuration(item.duration) }}
              </span>
            </div>
          </div>
          <div class="record-right">
            <div class="score-circle" :class="getScoreClass(item.score)">
              <span class="score-val">{{ item.score }}</span>
              <span class="score-unit">分</span>
            </div>
            <div class="accuracy-row">
              <div class="accuracy-bar-bg">
                <div class="accuracy-bar-fill" :style="{ width: item.accuracy + '%' }" :class="getScoreClass(item.score)"></div>
              </div>
              <span class="accuracy-text">{{ item.accuracy }}%</span>
            </div>
          </div>
        </div>
      </div>

      <div class="pagination-wrap">
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
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { Document, Timer, TrendCharts, Calendar, Clock, ArrowLeft } from '@element-plus/icons-vue'

const router = useRouter()

const filterType = ref('all')
const currentPage = ref(1)
const pageSize = ref(10)
const total = ref(0)

// Mock 统计数据
const summary = ref({
  totalExams: 12,
  totalMinutes: 356,
  avgAccuracy: 72,
  streakDays: 3
})

// Mock 学习记录
const allRecords = ref([
  { id: 1, examName: '2024年6月CET-4真题（第一套）', type: 'CET-4', score: 78, accuracy: 65, duration: 35, studyTime: '2026-04-30 14:20' },
  { id: 2, examName: '2024年6月CET-4真题（第二套）', type: 'CET-4', score: 82, accuracy: 70, duration: 32, studyTime: '2026-04-29 10:15' },
  { id: 3, examName: '2024年12月CET-6真题（第一套）', type: 'CET-6', score: 65, accuracy: 55, duration: 40, studyTime: '2026-04-28 16:30' },
  { id: 4, examName: '2024年6月CET-4真题（第三套）', type: 'CET-4', score: 85, accuracy: 75, duration: 30, studyTime: '2026-04-27 09:00' },
  { id: 5, examName: '2023年12月CET-6真题（第二套）', type: 'CET-6', score: 70, accuracy: 62, duration: 38, studyTime: '2026-04-26 20:10' },
  { id: 6, examName: '2023年6月CET-4真题（第一套）', type: 'CET-4', score: 90, accuracy: 82, duration: 28, studyTime: '2026-04-25 15:45' },
  { id: 7, examName: '2023年12月CET-4真题（第一套）', type: 'CET-4', score: 88, accuracy: 78, duration: 31, studyTime: '2026-04-24 11:20' },
  { id: 8, examName: '2024年12月CET-6真题（第二套）', type: 'CET-6', score: 72, accuracy: 68, duration: 36, studyTime: '2026-04-23 19:00' },
])

const filteredRecords = computed(() => {
  let list = allRecords.value
  if (filterType.value !== 'all') {
    const typeMap = { cet4: 'CET-4', cet6: 'CET-6' }
    list = list.filter(r => r.type === typeMap[filterType.value])
  }
  return list
})

const getScoreClass = (score) => {
  if (score >= 80) return 'score-high'
  if (score >= 60) return 'score-mid'
  return 'score-low'
}

const formatDuration = (minutes) => {
  const m = minutes % 60
  const h = Math.floor(minutes / 60)
  if (h > 0) return `${h}时${m}分`
  return `${m}分钟`
}

const reviewExam = (row) => {
  router.push({ name: 'examDetail', query: { albumId: row.id } })
}

const handlePageChange = (page) => {
  currentPage.value = page
  // 实际项目中这里调用接口获取分页数据
}

onMounted(() => {
  total.value = allRecords.value.length
})
</script>

<style scoped>
.study-page {
  padding: 0;
}

/* 顶部导航 */
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

/* 标题区 */
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

/* 统计卡片 */
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

.stat-bar.bar-blue {
  background: linear-gradient(90deg, #409eff, #79bbff);
}

.stat-bar.bar-green {
  background: linear-gradient(90deg, #67c23a, #95d475);
}

.stat-bar.bar-orange {
  background: linear-gradient(90deg, #e6a23c, #eebe77);
}

.stat-bar.bar-red {
  background: linear-gradient(90deg, #f56c6c, #f89898);
}

/* 记录区域 */
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
  border-bottom: 1px solid #f0f3f8;
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
  background: #f4f6fb;
  padding: 2px 10px;
  border-radius: 10px;
}

.filter-group :deep(.el-radio-button__inner) {
  background: #f5f5f5;
  border-color: var(--border-glass);
  color: var(--text-secondary);
}

.filter-group :deep(.el-radio-button__original-radio:checked + .el-radio-button__inner) {
  background: linear-gradient(135deg, var(--accent-blue) 0%, var(--accent-cyan) 100%);
  border-color: transparent;
  color: #fff;
}

/* 记录列表 */
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
  border-bottom: 1px solid #f5f7fa;
}

.record-item:last-child {
  border-bottom: none;
}

.record-item:hover {
  background: linear-gradient(90deg, rgba(64, 158, 255, 0.03) 0%, rgba(0, 168, 232, 0.02) 100%);
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
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  margin-left: 20px;
}

.score-circle {
  width: 56px;
  height: 56px;
  border-radius: 50%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  border: 2px solid;
}

.score-circle.score-high {
  border-color: rgba(103, 194, 58, 0.3);
  background: rgba(103, 194, 58, 0.06);
}

.score-circle.score-mid {
  border-color: rgba(230, 162, 60, 0.3);
  background: rgba(230, 162, 60, 0.06);
}

.score-circle.score-low {
  border-color: rgba(245, 108, 108, 0.3);
  background: rgba(245, 108, 108, 0.06);
}

.score-val {
  font-size: 18px;
  font-weight: 700;
  line-height: 1;
}

.score-circle.score-high .score-val { color: #67c23a; }
.score-circle.score-mid .score-val { color: #e6a23c; }
.score-circle.score-low .score-val { color: #f56c6c; }

.score-unit {
  font-size: 10px;
  color: var(--text-muted);
}

.accuracy-row {
  display: flex;
  align-items: center;
  gap: 6px;
}

.accuracy-bar-bg {
  width: 60px;
  height: 4px;
  border-radius: 2px;
  background: #f0f3f8;
  overflow: hidden;
}

.accuracy-bar-fill {
  height: 100%;
  border-radius: 2px;
  transition: width 0.3s;
}

.accuracy-bar-fill.score-high { background: linear-gradient(90deg, #67c23a, #95d475); }
.accuracy-bar-fill.score-mid { background: linear-gradient(90deg, #e6a23c, #eebe77); }
.accuracy-bar-fill.score-low { background: linear-gradient(90deg, #f56c6c, #f89898); }

.accuracy-text {
  font-size: 11px;
  color: var(--text-muted);
}

/* 分页 */
.pagination-wrap {
  display: flex;
  justify-content: center;
  padding: 20px 0 24px;
}

.pagination-wrap :deep(.el-pagination) {
  --el-pagination-text-color: var(--text-secondary);
  --el-pagination-button-color: var(--text-secondary);
}

.pagination-wrap :deep(.el-pager li) {
  background: var(--bg-card) !important;
  border: 1px solid var(--border-glass);
  color: var(--text-secondary);
  border-radius: 8px;
}

.pagination-wrap :deep(.el-pager li.is-active) {
  background: linear-gradient(135deg, var(--accent-blue) 0%, var(--accent-cyan) 100%) !important;
  border-color: transparent;
  color: #fff;
  box-shadow: 0 0 12px rgba(64, 158, 255, 0.3);
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
    flex-direction: row;
    margin-left: 0;
  }
}
</style>
