<template>
  <div class="home-page">
    <!-- 左侧触发条 -->
    <div class="sidebar-trigger"></div>

    <!-- 左侧导航栏 -->
    <aside class="sidebar">
      <div class="sidebar-title">听力分类</div>
      <el-menu
        :default-active="activeCategory"
        class="category-menu"
        @select="handleSelect"
      >
        <el-menu-item
          v-for="cat in categories"
          :key="cat.code"
          :index="cat.code"
        >
          <el-icon><Document /></el-icon>
          <span>{{ cat.name?.chinese || cat.name }}</span>
        </el-menu-item>
      </el-menu>
    </aside>

    <!-- 右侧内容区 -->
    <main class="main-content">
      <!-- 首页公告 -->
      <div class="home-announcement">
        <span class="home-announcement-text">🔔 欢迎访问英语听力练习平台！本站正在持续更新中，如有问题请联系管理员。</span>
      </div>

      <!-- 搜索区域 -->
      <div class="search-section">
        <h1>{{ currentCategory.title }}</h1>
        <p class="subtitle">{{ currentCategory.subtitle }}</p>
        <el-input
          v-model="searchWord"
          placeholder="搜索真题、模拟题..."
          size="large"
          class="search-input"
          @keyup.enter="doSearch"
        >
          <template #append>
            <el-button type="primary" @click="doSearch">
              <el-icon><Search /></el-icon>
            </el-button>
          </template>
        </el-input>
      </div>

      <!-- 试卷列表 -->
      <div class="section">
        <div class="section-header">
          <h2>
            <el-icon :color="currentCategory.color"><Document /></el-icon>
            {{ currentCategory.listTitle }}
          </h2>
          <el-link type="primary" @click="goExamList()">查看全部</el-link>
        </div>
        <el-row :gutter="20">
          <el-col
            v-for="item in currentList"
            :key="item.id"
            :xs="12" :sm="8" :md="6"
          >
            <el-card shadow="hover" class="exam-card" @click="goAlbum(item.id)">
              <div class="exam-tag" :class="activeCategory">{{ item.tag }}</div>
              <div class="exam-title">{{ item.title }}</div>
              <div class="exam-info">
                <el-icon><Headset /></el-icon>
                <span>{{ item.count }} 道题</span>
              </div>
            </el-card>
          </el-col>
        </el-row>
      </div>

      <!-- 快捷入口 -->
      <div class="section">
        <div class="section-header">
          <h2>
            <el-icon color="#e6a23c"><Star /></el-icon>
            快捷练习(部分功能暂未开放)
          </h2>
        </div>
        <el-row :gutter="20">
          <el-col :xs="12" :sm="8" :md="6">
            <el-card shadow="hover" class="quick-card" @click="goDailyArticle">
              <el-icon :size="32" color="#409eff"><Microphone /></el-icon>
              <div class="quick-title">每日一篇短文</div>
              <div class="quick-desc">每天10分钟，保持状态！</div>
            </el-card>
          </el-col>
          <el-col :xs="12" :sm="8" :md="6">
            <el-card shadow="hover" class="quick-card">
              <el-icon :size="32" color="#67c23a"><Collection /></el-icon>
              <div class="quick-title">错题回顾(开发中)</div>
              <div class="quick-desc">复习做错的题目</div>
            </el-card>
          </el-col>
          <el-col :xs="12" :sm="8" :md="6">
            <el-card shadow="hover" class="quick-card">
              <el-icon :size="32" color="#e6a23c"><Trophy /></el-icon>
              <div class="quick-title">模拟考试(开发中)</div>
              <div class="quick-desc">全真模拟，检验水平</div>
            </el-card>
          </el-col>
          <el-col :xs="12" :sm="8" :md="6">
            <el-card shadow="hover" class="quick-card">
              <el-icon :size="32" color="#f56c6c"><TrendCharts /></el-icon>
              <div class="quick-title">学习统计(开发中)</div>
              <div class="quick-desc">查看学习进度</div>
            </el-card>
          </el-col>
        </el-row>
      </div>
    </main>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { getCategories, getAlbumsByCategoryId } from '../api/Listen.js'

const router = useRouter()
const searchWord = ref('')

const activeCategory = ref('')

// 从后端获取的分类列表
const categories = ref([])
// 当前分类下的试卷列表
const albumList = ref([])

// 分类配置（标题、颜色等）
const categoryMeta = {
  cet6: { title: '英语六级听力练习', subtitle: '历年真题，助你轻松过级', listTitle: '六级听力真题', color: '#8b5cf6' },
  cet4: { title: '英语四级听力练习', subtitle: '历年真题 + 模拟试题', listTitle: '四级听力真题', color: '#409eff' },
  ielts: { title: '雅思听力练习', subtitle: '剑桥雅思真题 + 模拟训练', listTitle: '雅思真题', color: '#67c23a' },
  toefl: { title: '托福听力练习', subtitle: 'TPO真题 + 专项训练', listTitle: '托福真题', color: '#e6a23c' }
}

const currentCategory = computed(() => {
  return categoryMeta[activeCategory.value] || { title: '英语听力练习', subtitle: '选择分类开始练习', listTitle: '听力真题', color: '#409eff' }
})

const currentList = computed(() => albumList.value.slice(0, 8))

// 加载分类数据
const loadCategories = async () => {
  try {
    const data = await getCategories()
    categories.value = data || []
    if (categories.value.length > 0) {
      activeCategory.value = categories.value[0].code
    }
  } catch (e) {
    console.error('获取分类失败', e)
  }
}

// 加载试卷数据
const loadAlbums = async () => {
  if (!activeCategory.value) return
  const cat = categories.value.find(c => c.code === activeCategory.value)
  if (!cat) return
  try {
    const data = await getAlbumsByCategoryId(cat.id)
    albumList.value = (data || []).map(a => ({
      id: a.id,
      title: a.name?.chinese || a.name,
      tag: cat.name?.english || cat.name,
      count: 1
    }))
  } catch (e) {
    console.error('获取试卷失败', e)
    albumList.value = []
  }
}

// 切换分类时重新加载试卷
watch(activeCategory, () => {
  loadAlbums()
})

onMounted(() => {
  loadCategories()
})

const handleSelect = (index) => {
  activeCategory.value = index
}

const doSearch = () => {
  if (searchWord.value.trim()) {
    console.log('搜索：', searchWord.value)
  }
}

const goAlbum = (albumId) => {
  router.push({ name: 'examDetail', query: { albumId } })
}

const goDailyArticle = () => {
  router.push({ name: 'dailyArticle' })
}

const goExamList = () => {
  const cat = categories.value.find(c => c.code === activeCategory.value)
  if (cat) {
    router.push({ name: 'exams', query: { categoryId: cat.id } })
  }
}
</script>

<style scoped>
.home-page {
  position: relative;
  min-height: calc(100vh - 60px);
}

/* 左侧导航栏 - 默认完全隐藏在屏幕外 */
.sidebar {
  position: fixed;
  left: -260px;
  top: 76px;
  bottom: 16px;
  width: 260px;
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  padding: 16px 0;
  z-index: 50;
  transition: left 0.3s ease;
  box-shadow: 4px 0 24px rgba(0, 0, 0, 0.12);
  border-radius: 0 12px 12px 0;
}

/* 触发条 */
.sidebar-trigger {
  position: fixed;
  left: 0;
  top: 76px;
  bottom: 16px;
  width: 56px;
  z-index: 51;
  cursor: pointer;
  background: transparent;
}

/* 触发条上的指示箭头 */
.sidebar-trigger::before {
  content: '›';
  position: absolute;
  left: 6px;
  top: 50%;
  transform: translateY(-50%);
  font-size: 40px;
  font-weight: 900;
  color: var(--accent-blue);
  text-shadow: 0 2px 8px rgba(64, 158, 255, 0.4);
  z-index: 1;
  transition: all 0.3s;
}

.sidebar-trigger:hover::before {
  color: var(--accent-cyan);
  text-shadow: 0 2px 16px rgba(64, 158, 255, 0.6);
  transform: translateY(-50%) scale(1.2);
}

.sidebar-trigger:hover + .sidebar,
.sidebar:hover {
  left: 0;
}

.sidebar-title {
  padding: 0 20px 12px;
  font-size: 14px;
  font-weight: 600;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 1px;
}

:deep(.category-menu) {
  background: transparent !important;
  border-right: none;
}

:deep(.category-menu .el-menu-item) {
  color: var(--text-secondary);
}

:deep(.category-menu .el-menu-item:hover) {
  background: rgba(64, 158, 255, 0.08) !important;
  color: var(--accent-blue);
}

:deep(.category-menu .el-menu-item.is-active) {
  background: rgba(64, 158, 255, 0.12) !important;
  color: var(--accent-blue);
}

/* 右侧内容区 */
.main-content {
  width: 100%;
  padding: 0 24px 32px;
  box-sizing: border-box;
}

.search-section {
  text-align: center;
  padding: 48px 20px 40px;
  position: relative;
  background: linear-gradient(180deg, #f8faff 0%, var(--bg-content) 100%);
}

.search-section h1 {
  font-size: 38px;
  font-weight: 700;
  color: var(--text-primary);
  margin-bottom: 12px;
  background: linear-gradient(135deg, var(--text-primary) 0%, var(--accent-blue) 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.subtitle {
  color: var(--text-secondary);
  margin-bottom: 28px;
  font-size: 16px;
}

.search-input {
  max-width: 600px;
  margin: 0 auto;
}

.search-input :deep(.el-input__wrapper) {
  background: #ffffff !important;
  box-shadow: 0 0 0 1px var(--border-glass) inset, 0 4px 16px rgba(0,0,0,0.06) !important;
  border-radius: 28px;
  padding: 4px 8px 4px 20px;
}

.search-input :deep(.el-input__wrapper:hover) {
  box-shadow: 0 0 0 1px rgba(64, 158, 255, 0.3) inset, 0 6px 20px rgba(0,0,0,0.08) !important;
}

.search-input :deep(.el-input__wrapper.is-focus) {
  box-shadow: 0 0 0 1px var(--accent-blue) inset, 0 0 12px rgba(64, 158, 255, 0.15) !important;
}

.search-input :deep(.el-input__inner) {
  color: var(--text-primary);
  font-size: 15px;
}

.search-input :deep(.el-input__inner::placeholder) {
  color: var(--text-muted);
}

.search-input :deep(.el-input-group__append) {
  background: linear-gradient(135deg, var(--accent-blue) 0%, var(--accent-cyan) 100%);
  border: none;
  border-radius: 0 24px 24px 0;
}

.section {
  margin-bottom: 48px;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
  padding-bottom: 12px;
  border-bottom: 1px solid #eef1f6;
}

.section-header h2 {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 20px;
  color: var(--text-primary);
  position: relative;
  padding-left: 14px;
}

.section-header h2::before {
  content: '';
  position: absolute;
  left: 0;
  top: 3px;
  bottom: 3px;
  width: 4px;
  border-radius: 2px;
  background: linear-gradient(180deg, var(--accent-blue), var(--accent-cyan));
}

.section-header :deep(.el-link) {
  color: var(--accent-blue);
}

/* 试卷卡片 */
.exam-card {
  margin-bottom: 16px;
  cursor: pointer;
  transition: all 0.3s ease;
  position: relative;
  background: var(--bg-card) !important;
  border: 1px solid var(--border-glass) !important;
  border-radius: 14px !important;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.05);
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
  border-color: rgba(64, 158, 255, 0.15) !important;
}

.exam-card:hover::after {
  opacity: 1;
}

.exam-tag {
  display: inline-block;
  padding: 4px 12px;
  border-radius: 20px;
  font-size: 11px;
  font-weight: 600;
  margin-bottom: 12px;
  letter-spacing: 0.5px;
}

.exam-tag.cet {
  background: rgba(64, 158, 255, 0.1);
  color: var(--accent-blue);
}

.exam-tag.ielts {
  background: rgba(139, 92, 246, 0.1);
  color: var(--accent-purple);
}

.exam-tag.toefl {
  background: rgba(0, 168, 232, 0.1);
  color: var(--accent-cyan);
}

.exam-title {
  font-size: 15px;
  font-weight: 500;
  color: var(--text-primary);
  line-height: 1.5;
  margin-bottom: 12px;
  min-height: 45px;
}

.exam-info {
  display: flex;
  align-items: center;
  gap: 6px;
  color: var(--text-muted);
  font-size: 13px;
}

/* 快捷练习卡片 */
.quick-card {
  text-align: center;
  padding: 28px 0;
  cursor: pointer;
  transition: all 0.3s ease;
  margin-bottom: 16px;
  background: var(--bg-card) !important;
  border: 1px solid var(--border-glass) !important;
  border-radius: 14px !important;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.05);
  position: relative;
  overflow: hidden;
}

.quick-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(135deg, rgba(64, 158, 255, 0.03) 0%, transparent 60%);
  opacity: 0;
  transition: opacity 0.3s;
}

.quick-card:hover {
  transform: translateY(-6px);
  box-shadow: 0 12px 32px rgba(0, 0, 0, 0.1);
  border-color: rgba(64, 158, 255, 0.15) !important;
}

.quick-card:hover::before {
  opacity: 1;
}

.quick-card :deep(.el-icon) {
  position: relative;
  z-index: 1;
}

.quick-title {
  margin-top: 16px;
  font-size: 15px;
  font-weight: 500;
  color: var(--text-primary);
}

.quick-desc {
  margin-top: 6px;
  font-size: 12px;
  color: var(--text-muted);
}

/* 首页公告 */
.home-announcement {
  padding: 16px 28px;
  background: linear-gradient(135deg, #eef2ff 0%, #f0f7ff 100%);
  border-bottom: 1px solid #e0e8f5;
  text-align: center;
}

.home-announcement-text {
  font-size: 14px;
  color: var(--text-secondary);
  letter-spacing: 0.3px;
}
</style>
