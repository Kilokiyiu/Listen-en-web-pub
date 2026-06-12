<template>
  <div class="admin-page">
    <PageHeader
      title="数据概览"
      description="网站用户增长与访问情况（数据约 5 分钟刷新一次）"
    >
      <template #extra>
        <el-button text type="primary" @click="loadAll" :loading="loading">
          <el-icon><Refresh /></el-icon>
          刷新
        </el-button>
      </template>
    </PageHeader>

    <div class="admin-stat-grid">
      <div class="admin-stat-card">
        <div>
          <div class="admin-stat-card__label">总用户数</div>
          <div class="admin-stat-card__value">{{ overview.totalUsers ?? '-' }}</div>
        </div>
        <div class="admin-stat-card__icon admin-stat-card__icon--blue">
          <el-icon><User /></el-icon>
        </div>
      </div>
      <div class="admin-stat-card">
        <div>
          <div class="admin-stat-card__label">今日新用户</div>
          <div class="admin-stat-card__value">{{ overview.newUsersToday ?? '-' }}</div>
        </div>
        <div class="admin-stat-card__icon admin-stat-card__icon--green">
          <el-icon><UserFilled /></el-icon>
        </div>
      </div>
      <div class="admin-stat-card">
        <div>
          <div class="admin-stat-card__label">今日 PV</div>
          <div class="admin-stat-card__value">{{ overview.todayPageViews ?? '-' }}</div>
        </div>
        <div class="admin-stat-card__icon admin-stat-card__icon--orange">
          <el-icon><View /></el-icon>
        </div>
      </div>
      <div class="admin-stat-card">
        <div>
          <div class="admin-stat-card__label">今日 UV</div>
          <div class="admin-stat-card__value">{{ overview.todayUniqueVisitors ?? '-' }}</div>
        </div>
        <div class="admin-stat-card__icon admin-stat-card__icon--purple">
          <el-icon><Monitor /></el-icon>
        </div>
      </div>
    </div>

    <div class="chart-grid">
      <div class="admin-card chart-card">
        <div class="admin-card__header">
          <span class="admin-card__title">近 30 天用户注册</span>
        </div>
        <div class="admin-card__body">
          <div ref="registrationChartRef" class="chart-box"></div>
        </div>
      </div>
      <div class="admin-card chart-card">
        <div class="admin-card__header">
          <span class="admin-card__title">近 7 天访问趋势</span>
        </div>
        <div class="admin-card__body">
          <div ref="trafficChartRef" class="chart-box"></div>
        </div>
      </div>
    </div>

    <div class="bottom-grid">
      <div class="admin-card admin-table-card">
        <div class="admin-card__header">
          <span class="admin-card__title">热门页面 Top 10（近 7 天）</span>
        </div>
        <div class="admin-card__body table-body">
          <el-table :data="topPages" stripe style="width: 100%">
            <el-table-column label="页面" min-width="180">
              <template #default="{ row }">
                {{ formatPath(row.path) }}
                <span class="path-code">{{ row.path }}</span>
              </template>
            </el-table-column>
            <el-table-column prop="pageViews" label="PV" width="100" align="center" />
            <el-table-column prop="uniqueVisitors" label="UV" width="100" align="center" />
          </el-table>
        </div>
      </div>

      <div class="admin-card">
        <div class="admin-card__header">
          <span class="admin-card__title">业务活跃</span>
        </div>
        <div class="admin-card__body activity-list">
          <div class="activity-item">
            <span>7 日新用户</span>
            <strong>{{ overview.newUsersLast7Days ?? 0 }}</strong>
          </div>
          <div class="activity-item">
            <span>30 日新用户</span>
            <strong>{{ overview.newUsersLast30Days ?? 0 }}</strong>
          </div>
          <div class="activity-item">
            <span>文章总阅读</span>
            <strong>{{ articleStats.totalReads ?? 0 }}</strong>
          </div>
          <div class="activity-item">
            <span>7 日文章阅读</span>
            <strong>{{ articleStats.readsLast7Days ?? 0 }}</strong>
          </div>
          <div class="activity-item">
            <span>单词本用户</span>
            <strong>{{ wordStats.activeUsers ?? 0 }}</strong>
          </div>
          <div class="activity-item">
            <span>7 日单词复习</span>
            <strong>{{ wordStats.reviewsLast7Days ?? 0 }}</strong>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, onBeforeUnmount, nextTick } from 'vue'
import * as echarts from 'echarts'
import PageHeader from '../components/PageHeader.vue'
import {
  getStatsOverview,
  getRegistrationTrend,
  getTrafficTrend,
  getTopPages,
  getArticleReadingStats,
  getWordLearningStats,
} from '../api/Admin.js'

const loading = ref(false)
const overview = reactive({})
const articleStats = reactive({})
const wordStats = reactive({})
const topPages = ref([])

const registrationChartRef = ref(null)
const trafficChartRef = ref(null)
let registrationChart = null
let trafficChart = null

const pathLabels = {
  '/': '首页',
  '/exams': '听力真题',
  '/exam': '听力练习',
  '/login': '登录',
  '/profile': '个人中心',
  '/history': '学习记录',
  '/daily': '每日一篇',
  '/word-roots': '词根学习',
  '/my-words': '我的单词',
  '/word-review': '单词复习',
  '/bbc-news': 'BBC 新闻',
}

const formatPath = (path) => pathLabels[path] || path

const renderRegistrationChart = (data) => {
  if (!registrationChartRef.value) return
  if (!registrationChart) {
    registrationChart = echarts.init(registrationChartRef.value)
  }
  registrationChart.setOption({
    tooltip: { trigger: 'axis' },
    grid: { left: 40, right: 20, top: 30, bottom: 30 },
    xAxis: {
      type: 'category',
      data: data.map(d => d.date.slice(5)),
      boundaryGap: false,
    },
    yAxis: { type: 'value', minInterval: 1 },
    series: [{
      name: '新用户',
      type: 'line',
      smooth: true,
      areaStyle: { opacity: 0.15 },
      data: data.map(d => d.count),
      itemStyle: { color: '#1677ff' },
    }],
  })
}

const renderTrafficChart = (data) => {
  if (!trafficChartRef.value) return
  if (!trafficChart) {
    trafficChart = echarts.init(trafficChartRef.value)
  }
  trafficChart.setOption({
    tooltip: { trigger: 'axis' },
    legend: { data: ['PV', 'UV'], top: 0 },
    grid: { left: 40, right: 20, top: 40, bottom: 30 },
    xAxis: {
      type: 'category',
      data: data.map(d => d.date.slice(5)),
      boundaryGap: false,
    },
    yAxis: { type: 'value', minInterval: 1 },
    series: [
      {
        name: 'PV',
        type: 'line',
        smooth: true,
        data: data.map(d => d.pageViews),
        itemStyle: { color: '#1677ff' },
      },
      {
        name: 'UV',
        type: 'line',
        smooth: true,
        data: data.map(d => d.uniqueVisitors),
        itemStyle: { color: '#52c41a' },
      },
    ],
  })
}

const handleResize = () => {
  registrationChart?.resize()
  trafficChart?.resize()
}

const loadAll = async () => {
  loading.value = true
  try {
    const [overviewRes, regRes, trafficRes, topRes, articleRes, wordRes] = await Promise.all([
      getStatsOverview(),
      getRegistrationTrend(30),
      getTrafficTrend(7),
      getTopPages(7, 10),
      getArticleReadingStats(),
      getWordLearningStats(),
    ])

    Object.assign(overview, overviewRes.data || {})
    Object.assign(articleStats, articleRes.data || {})
    Object.assign(wordStats, wordRes.data || {})
    topPages.value = topRes.data || []

    await nextTick()
    renderRegistrationChart(regRes.data || [])
    renderTrafficChart(trafficRes.data || [])
  } catch (e) {
    // 错误已在拦截器中处理
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadAll()
  window.addEventListener('resize', handleResize)
})

onBeforeUnmount(() => {
  window.removeEventListener('resize', handleResize)
  registrationChart?.dispose()
  trafficChart?.dispose()
})
</script>

<style scoped>
.chart-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
  margin-bottom: 16px;
}

.bottom-grid {
  display: grid;
  grid-template-columns: 1fr 320px;
  gap: 16px;
}

.chart-box {
  height: 280px;
}

.table-body {
  padding: 0;
}

.path-code {
  display: block;
  font-size: 12px;
  color: var(--admin-text-secondary);
  margin-top: 2px;
}

.activity-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.activity-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 0;
  border-bottom: 1px solid var(--admin-border);
  font-size: 14px;
  color: var(--admin-text-secondary);
}

.activity-item:last-child {
  border-bottom: none;
}

.activity-item strong {
  color: var(--admin-text);
  font-size: 18px;
}

@media (max-width: 1100px) {
  .chart-grid,
  .bottom-grid {
    grid-template-columns: 1fr;
  }
}
</style>
