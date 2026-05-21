<template>
  <div class="daily-page">
    <!-- 顶部导航 -->
    <div class="page-header">
      <el-button text @click="goBack" class="back-btn">
        <el-icon><ArrowLeft /></el-icon> {{ showDetail ? '返回列表' : '返回' }}
      </el-button>
      <div class="page-title">BBC 每日新闻</div>
      <div class="placeholder"></div>
    </div>

    <!-- 新闻列表页面 -->
    <template v-if="!showDetail">
      <!-- BBC 风格分类导航栏 -->
      <nav class="bbc-nav-bar">
        <div class="bbc-nav-inner">
          <a
            v-for="cat in categories"
            :key="cat.code"
            href="javascript:void(0)"
            class="bbc-nav-link"
            :class="{ active: currentCategory === cat.code }"
            @click.prevent="currentCategory = cat.code; loadBBCNews()"
          >
            {{ cat.name }}
          </a>
        </div>
      </nav>

      <!-- 加载状态 -->
      <div v-if="loading" class="loading-wrapper">
        <el-skeleton animated :rows="8" />
      </div>

      <!-- 新闻列表 -->
      <div v-else-if="newsList.length > 0" class="news-list">
        <article
          v-for="(item, index) in newsList"
          :key="index"
          class="news-card"
          @click="openNewsDetail(item)"
        >
          <div class="news-card-inner">
            <div class="news-content">
              <h3 class="news-title">{{ item.title }}</h3>
              <p class="news-description">{{ stripHtml(item.description) }}</p>
              <div class="news-meta">
                <span class="news-category">{{ currentCategoryName }}</span>
                <span class="news-time">{{ formatDate(item.pubDate) }}</span>
              </div>
            </div>
            <div class="news-arrow">
              <el-icon><ArrowRight /></el-icon>
            </div>
          </div>
        </article>
      </div>

      <!-- 无数据 -->
      <div v-else class="empty-wrapper">
        <el-empty description="暂无新闻，请稍后重试">
          <el-button type="primary" @click="loadBBCNews">刷新</el-button>
        </el-empty>
      </div>
    </template>

    <!-- 文章详情页面 -->
    <template v-else>
      <!-- 加载状态 -->
      <div v-if="detailLoading" class="loading-wrapper">
        <el-skeleton animated :rows="15" />
      </div>

      <!-- 文章内容 -->
      <div v-else-if="articleDetail" class="article-detail">
        <!-- 文章头部：标题 + 元信息 -->
        <header class="article-header">
          <h1 class="article-title">{{ articleDetail.title || currentNewsTitle || 'BBC News' }}</h1>
          <div class="article-byline">
            <div class="byline-main">
              <span class="byline-time">{{ formatDate(articleDetail.pubDate) }}</span>
            </div>
            <div class="byline-actions">
              <button class="action-btn share-btn" @click="shareArticle">
                <el-icon><Share /></el-icon>
                <span>分享</span>
              </button>
              <button class="action-btn save-btn" @click="saveArticle">
                <el-icon><Star /></el-icon>
                <span>收藏</span>
              </button>
            </div>
          </div>
        </header>

        <!-- 正文内容 -->
        <div class="article-body-wrapper">
          <div class="article-body" v-html="cleanContent(articleDetail.content)"></div>
        </div>

        <!-- 文章底部操作 -->
        <div class="article-footer">
          <el-button type="primary" size="large" @click="openOriginalUrl">
            <el-icon><Link /></el-icon> 阅读原文
          </el-button>
          <el-button size="large" @click="showDetail = false">
            <el-icon><ArrowLeft /></el-icon> 返回列表
          </el-button>
        </div>
      </div>

      <!-- 获取详情失败 -->
      <div v-else class="empty-wrapper">
        <el-empty description="获取文章详情失败">
          <el-button type="primary" @click="loadArticleDetail">重试</el-button>
          <el-button @click="showDetail = false">返回列表</el-button>
        </el-empty>
      </div>
    </template>

    <!-- 提示信息 -->
    <div class="tips-section" v-if="!showDetail">
      <el-alert
        type="info"
        :closable="false"
        show-icon
      >
        <template #title>
          <span>💡 学习提示：点击新闻标题可站内阅读全文，底部提供原文链接</span>
        </template>
      </el-alert>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { getBBCNews, getBBCCategories, getBBCArticleDetail } from '../api/DailyArticle.js'

const router = useRouter()

const loading = ref(false)
const detailLoading = ref(false)
const newsList = ref([])
const categories = ref([])
const currentCategory = ref('')
const showDetail = ref(false)
const articleDetail = ref(null)
const currentNewsUrl = ref('')
const currentNewsTitle = ref('')

// 当前分类名称
const currentCategoryName = computed(() => {
  const cat = categories.value.find(c => c.code === currentCategory.value)
  return cat?.name || 'BBC新闻'
})

// 加载分类
const loadCategories = async () => {
  try {
    const data = await getBBCCategories()
    if (data.code === 200 && data.data) {
      categories.value = data.data
      if (categories.value.length > 0) {
        currentCategory.value = categories.value[0].code
      }
    }
  } catch (e) {
    console.error('获取分类失败', e)
    // 使用默认分类
    categories.value = [
      { code: '', name: '热门' },
      { code: 'world', name: '世界' },
      { code: 'technology', name: '科技' },
      { code: 'business', name: '商业' },
      { code: 'science', name: '科学' }
    ]
    currentCategory.value = ''
  }
}

// 加载新闻
const loadBBCNews = async () => {
  loading.value = true
  try {
    const category = currentCategory.value || null
    const data = await getBBCNews(category)
    if (data.code === 200 && data.data) {
      newsList.value = data.data
    } else {
      newsList.value = []
    }
  } catch (e) {
    console.error('获取 BBC 新闻失败', e)
    newsList.value = []
    ElMessage.error('获取新闻失败，请稍后重试')
  } finally {
    loading.value = false
  }
}

// 打开新闻详情
const openNewsDetail = (item) => {
  currentNewsUrl.value = item.link
  currentNewsTitle.value = item.title || ''
  showDetail.value = true
  articleDetail.value = null
  loadArticleDetail()
}

// 加载文章详情
const loadArticleDetail = async () => {
  if (!currentNewsUrl.value) return

  detailLoading.value = true
  try {
    const data = await getBBCArticleDetail(currentNewsUrl.value)
    if (data.code === 200 && data.data) {
      articleDetail.value = data.data
    } else {
      articleDetail.value = null
      ElMessage.error('获取文章详情失败')
    }
  } catch (e) {
    console.error('获取文章详情失败', e)
    articleDetail.value = null
    ElMessage.error('获取文章详情失败，请稍后重试')
  } finally {
    detailLoading.value = false
  }
}

// 返回
const goBack = () => {
  if (showDetail.value) {
    showDetail.value = false
    articleDetail.value = null
    currentNewsTitle.value = ''
  } else {
    router.back()
  }
}

// 打开原文链接
const openOriginalUrl = () => {
  if (articleDetail.value?.originalUrl) {
    window.open(articleDetail.value.originalUrl, '_blank')
  }
}

// 分享文章
const shareArticle = () => {
  if (navigator.share) {
    navigator.share({
      title: articleDetail.value?.title,
      url: articleDetail.value?.originalUrl || window.location.href
    }).catch(() => {})
  } else {
    // 复制链接到剪贴板
    const url = articleDetail.value?.originalUrl || window.location.href
    navigator.clipboard.writeText(url).then(() => {
      ElMessage.success('链接已复制到剪贴板')
    }).catch(() => {
      ElMessage.info('请手动复制链接')
    })
  }
}

// 收藏文章
const saveArticle = () => {
  ElMessage.success('已添加到收藏')
}

// 去除HTML标签
const stripHtml = (html) => {
  if (!html) return ''
  return html.replace(/<[^>]*>/g, '').trim()
}

// 清理BBC文章内容：过滤掉开头的垃圾文本（分享按钮、作者名等元数据）
const cleanContent = (html) => {
  if (!html) return ''
  // 移除常见的BBC页面垃圾前缀文本
  const junkPatterns = [
    /^(?:ShareSaveAdd as preferred on Google)?/i,
    /^(?:ShareSave)?/i,
    /^(?:Add as preferred on Google)?/i,
  ]
  let cleaned = html
  // 移除以 "Share" "Save" 等开头的段落
  cleaned = cleaned.replace(/<p>\s*(?:Share|Save|Add as preferred|BBC News)\s*<\/p>/gi, '')
  // 移除包含 "correspondent" 且很短的段落（通常是作者署名行）
  cleaned = cleaned.replace(/<p>\s*[^<]{0,60}correspondent[^<]{0,60}\s*<\/p>/gi, '')
  // 移除包含 "Reuters" "AP" "AFP" 等通讯社名称的短段落
  cleaned = cleaned.replace(/<p>\s*[^<]{0,40}(?:Reuters|Associated Press|AFP|PA Media)[^<]{0,40}\s*<\/p>/gi, '')
  // 移除空段落
  cleaned = cleaned.replace(/<p>\s*<\/p>/g, '')
  return cleaned
}

// 格式化日期
const formatDate = (dateStr) => {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  return date.toLocaleDateString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

onMounted(async () => {
  await loadCategories()
  await loadBBCNews()
  // 滚动到页面顶部
  window.scrollTo(0, 0)
})
</script>

<style scoped>
.daily-page {
  padding: 0 24px 32px;
  max-width: 900px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 0;
  border-bottom: 1px solid #eef1f6;
  margin-bottom: 24px;
}

.page-title {
  font-size: 18px;
  font-weight: 600;
  color: var(--text-primary);
}

.placeholder {
  width: 60px;
}

.back-btn {
  color: var(--text-secondary) !important;
}

/* BBC 风格分类导航栏 */
.bbc-nav-bar {
  border-bottom: 1px solid #e2e2e2;
  margin-bottom: 24px;
  background: transparent;
}

.bbc-nav-inner {
  display: flex;
  gap: 0;
  overflow-x: auto;
  scrollbar-width: none;
  -ms-overflow-style: none;
}

.bbc-nav-inner::-webkit-scrollbar {
  display: none;
}

.bbc-nav-link {
  display: block;
  padding: 12px 16px;
  font-size: 14px;
  font-weight: 500;
  color: #333;
  text-decoration: none;
  white-space: nowrap;
  border-bottom: 3px solid transparent;
  transition: all 0.2s ease;
  font-family: "Helvetica Neue", Arial, sans-serif;
}

.bbc-nav-link:hover {
  color: #000;
  background: rgba(0, 0, 0, 0.04);
}

.bbc-nav-link.active {
  color: #000;
  border-bottom-color: #000;
  font-weight: 600;
}

/* 新闻列表 */
.news-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.news-card {
  cursor: pointer;
  transition: all 0.25s ease;
  background: var(--bg-card, #fff);
  border: 1px solid var(--border-glass, #e2e2e2);
  border-radius: 10px;
  padding: 16px 20px;
}

.news-card:hover {
  background: var(--bg-card, #fff);
  border-color: rgba(64, 158, 255, 0.3);
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.08);
  transform: translateY(-2px);
}

.news-card-inner {
  display: flex;
  align-items: flex-start;
}

.news-content {
  flex: 1;
  min-width: 0;
}

.news-title {
  font-family: "Helvetica Neue", Arial, sans-serif;
  font-size: 18px;
  font-weight: 700;
  color: #1a1a1a;
  margin-bottom: 8px;
  line-height: 1.3;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.news-description {
  font-family: "Helvetica Neue", Arial, sans-serif;
  font-size: 15px;
  color: #5a5a5a;
  line-height: 1.5;
  margin-bottom: 10px;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.news-meta {
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 13px;
  font-family: "Helvetica Neue", Arial, sans-serif;
}

.news-category {
  color: #8b0000;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.3px;
}

.news-time {
  color: #6b6b6b;
}

.news-arrow {
  color: #b0b0b0;
  font-size: 18px;
  margin-left: 16px;
  margin-top: 4px;
  transition: transform 0.2s, color 0.2s;
  flex-shrink: 0;
}

.news-card:hover .news-arrow {
  transform: translateX(3px);
  color: #333;
}

/* 文章详情 - BBC 风格 */
.article-detail {
  animation: fadeIn 0.3s ease;
  max-width: 800px;
  margin: 0 auto;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}

/* 文章头部 */
.article-header {
  margin-bottom: 24px;
}

/* 正文内容卡片 */
.article-body-wrapper {
  background: var(--bg-card, #fff);
  border: 1px solid var(--border-glass, #e2e2e2);
  border-radius: 12px;
  padding: 28px 32px;
  margin-bottom: 32px;
}

.article-title {
  font-family: "Georgia", "Times New Roman", serif;
  font-size: 32px;
  font-weight: 700;
  color: #1a1a1a;
  line-height: 1.2;
  margin-bottom: 20px;
  letter-spacing: -0.5px;
}

/* 作者信息栏 */
.article-byline {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-bottom: 20px;
  border-bottom: 1px solid #e2e2e2;
}

.byline-main {
  display: flex;
  align-items: center;
  gap: 12px;
}

.byline-time {
  font-size: 14px;
  color: #6b6b6b;
  font-weight: 400;
}

.byline-actions {
  display: flex;
  gap: 8px;
}

.action-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 16px;
  border: 1px solid #d0d0d0;
  border-radius: 4px;
  background: #fff;
  color: #333;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
}

.action-btn:hover {
  background: #f5f5f5;
  border-color: #b0b0b0;
}

.action-btn .el-icon {
  font-size: 16px;
}

/* 正文内容 */
.article-body {
  font-family: "Georgia", "Times New Roman", serif;
  font-size: 18px;
  line-height: 1.75;
  color: #333;
}

.article-body :deep(p) {
  margin-bottom: 20px;
  text-indent: 0;
}

.article-body :deep(h2) {
  font-family: "Helvetica Neue", Arial, sans-serif;
  font-size: 24px;
  font-weight: 700;
  color: #1a1a1a;
  margin: 32px 0 16px;
  line-height: 1.3;
}

.article-body :deep(h3) {
  font-family: "Helvetica Neue", Arial, sans-serif;
  font-size: 20px;
  font-weight: 600;
  color: #1a1a1a;
  margin: 24px 0 12px;
}

.article-body :deep(img) {
  max-width: 100%;
  height: auto;
  border-radius: 4px;
  margin: 24px 0;
}

.article-body :deep(figcaption) {
  font-size: 14px;
  color: #6b6b6b;
  margin-top: -16px;
  margin-bottom: 24px;
  font-style: italic;
}

.article-body :deep(a) {
  color: #0066cc;
  text-decoration: underline;
}

.article-body :deep(a:hover) {
  color: #004499;
}

.article-body :deep(ul),
.article-body :deep(ol) {
  margin-bottom: 20px;
  padding-left: 24px;
}

.article-body :deep(li) {
  margin-bottom: 8px;
}

.article-body :deep(blockquote) {
  border-left: 3px solid #333;
  padding-left: 20px;
  margin: 24px 0;
  font-style: italic;
  color: #555;
}

/* 文章底部 */
.article-footer {
  display: flex;
  justify-content: center;
  gap: 16px;
  margin-bottom: 40px;
}

/* 加载和空状态 */
.loading-wrapper {
  padding: 20px;
}

.empty-wrapper {
  padding: 60px 0;
}

/* 提示 */
.tips-section {
  margin-top: 32px;
}

.tips-section :deep(.el-alert) {
  border-radius: 12px;
}

/* 响应式 */
@media (max-width: 768px) {
  .daily-page {
    padding: 0 12px 24px;
  }

  .news-card {
    padding: 12px 16px;
  }

  .news-title {
    font-size: 15px;
  }

  .news-description {
    font-size: 13px;
  }

  .news-arrow {
    display: none;
  }

  .article-title {
    font-size: 24px;
    line-height: 1.25;
  }

  .article-byline {
    flex-direction: column;
    align-items: flex-start;
    gap: 12px;
  }

  .byline-actions {
    width: 100%;
  }

  .action-btn {
    flex: 1;
    justify-content: center;
  }

  .article-body-wrapper {
    padding: 20px 16px;
  }

  .article-body {
    font-size: 16px;
    line-height: 1.7;
  }

  .article-footer {
    flex-direction: column;
  }

  .article-footer .el-button {
    width: 100%;
  }
}
</style>
