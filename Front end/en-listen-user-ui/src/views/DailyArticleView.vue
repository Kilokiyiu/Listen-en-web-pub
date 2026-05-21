<template>
  <div class="daily-page">
    <!-- 顶部导航 -->
    <div class="page-header">
      <el-button text @click="router.back()" class="back-btn">
        <el-icon><ArrowLeft /></el-icon> 返回
      </el-button>
      <div class="page-title">每日一篇短文</div>
      <div class="placeholder"></div>
    </div>

    <!-- 日期切换 -->
    <div class="date-nav">
      <el-button circle @click="goPrevDay" :disabled="!prevDate">
        <el-icon><ArrowLeft /></el-icon>
      </el-button>
      <span class="current-date">{{ displayDate }}</span>
      <el-button circle @click="goNextDay" :disabled="!nextDate">
        <el-icon><ArrowRight /></el-icon>
      </el-button>
    </div>

    <!-- 加载状态 -->
    <div v-if="loading" class="loading-wrapper">
      <el-skeleton animated :rows="10" />
    </div>

    <!-- 短文内容 -->
    <div v-else-if="article" class="article-content">
      <!-- 音频播放器 -->
      <div v-if="article.audioUrl" class="audio-player">
        <audio controls :src="getAudioUrl(article.AudioUrl)" class="audio-control">
          您的浏览器不支持音频播放
        </audio>
      </div>

      <!-- 英文原文 -->
      <div class="article-section">
        <div class="section-header">
          <el-icon color="#409eff"><Document /></el-icon>
          <span>英文原文</span>
        </div>
        <div class="article-text english-text">
          <p v-for="(para, idx) in englishParagraphs" :key="'en-'+idx" class="article-paragraph">
            {{ para }}
          </p>
        </div>
      </div>

      <!-- 中文翻译 -->
      <div class="article-section">
        <div class="section-header">
          <el-icon color="#67c23a"><ChatDotRound /></el-icon>
          <span>中文翻译</span>
          <el-button
            text
            size="small"
            @click="showTranslation = !showTranslation"
            class="toggle-btn"
          >
            {{ showTranslation ? '隐藏翻译' : '显示翻译' }}
          </el-button>
        </div>
        <div v-show="showTranslation" class="article-text chinese-text">
          <p v-for="(para, idx) in chineseParagraphs" :key="'cn-'+idx" class="article-paragraph">
            {{ para }}
          </p>
        </div>
        <div v-show="!showTranslation" class="translation-hint">
          点击"显示翻译"查看中文翻译
        </div>
      </div>

      <!-- 操作按钮 -->
      <div class="article-actions">
        <el-button :type="article.isFavorite ? 'warning' : 'default'" @click="toggleFavorite">
          <el-icon><Star /></el-icon>
          {{ article.isFavorite ? '已收藏' : '收藏' }}
        </el-button>
        <el-button type="primary" @click="markAsRead" v-if="!article.isRead">
          <el-icon><Check /></el-icon>
          标记为已读
        </el-button>
        <el-tag v-else type="success" size="large">已完成今日阅读</el-tag>
      </div>
    </div>

    <!-- 无数据 -->
    <div v-else class="empty-wrapper">
      <el-empty description="暂无当日短文，请选择其他日期" />
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessage } from 'element-plus'
import { getDailyArticle, markArticleRead, toggleFavorite as toggleFavoriteApi } from '../api/DailyArticle.js'
import dayjs from 'dayjs'

const router = useRouter()
const route = useRoute()

const loading = ref(false)
const article = ref(null)
const showTranslation = ref(false)

// 将文本按换行分割为段落数组
const englishParagraphs = computed(() => {
  if (!article.value?.englishText) return []
  return article.value.englishText.split('\n').filter(p => p.trim() !== '')
})

const chineseParagraphs = computed(() => {
  if (!article.value?.chineseText) return []
  return article.value.chineseText.split('\n').filter(p => p.trim() !== '')
})

// 当前日期（从路由参数或今天）
const currentDate = computed(() => {
  if (route.query.date) {
    return dayjs(route.query.date)
  }
  return dayjs()
})

const displayDate = computed(() => {
  return currentDate.value.format('YYYY年MM月DD日')
})

const prevDate = computed(() => {
  const d = currentDate.value.subtract(1, 'day')
  // 最多往前30天
  if (d.isBefore(dayjs().subtract(30, 'day'))) return null
  return d
})

const nextDate = computed(() => {
  const d = currentDate.value.add(1, 'day')
  // 不能超过今天
  if (d.isAfter(dayjs(), 'day')) return null
  return d
})

const goPrevDay = () => {
  if (!prevDate.value) return
  router.push({ name: 'dailyArticle', query: { date: prevDate.value.format('YYYY-MM-DD') } })
}

const goNextDay = () => {
  if (!nextDate.value) return
  router.push({ name: 'dailyArticle', query: { date: nextDate.value.format('YYYY-MM-DD') } })
}

const loadArticle = async () => {
  loading.value = true
  try {
    const dateStr = currentDate.value.format('YYYY-MM-DD')
    const data = await getDailyArticle(dateStr)
    // 后端返回 PascalCase，做一次映射供模板使用
    article.value = {
      id: data.id,
      date: data.date,
      titleChinese: data.titleChinese,
      titleEnglish: data.titleEnglish,
      englishText: data.englishText,
      chineseText: data.chineseText,
      audioUrl: data.audioUrl,
      publicTime: data.publicTime,
      isRead: data.isRead,
      isFavorite: data.isFavorite
    }
    showTranslation.value = false
  } catch (e) {
    article.value = null
    console.error('获取每日短文失败', e)
  } finally {
    loading.value = false
  }
}

const getAudioUrl = (url) => {
  if (!url) return ''
  if (url.startsWith('http')) return url
  return `${import.meta.env.VITE_LISTEN_API}${url}`
}

const markAsRead = async () => {
  try {
    await markArticleRead(article.value.id)
    article.value.isRead = true
    ElMessage.success('已标记为已读')
  } catch (e) {
    // 错误已在拦截器中处理
  }
}

const toggleFavorite = async () => {
  try {
    await toggleFavoriteApi(article.value.id)
    article.value.isFavorite = !article.value.isFavorite
    ElMessage.success(article.value.isFavorite ? '已收藏' : '已取消收藏')
  } catch (e) {
    // 错误已在拦截器中处理
  }
}

onMounted(() => {
  loadArticle()
})

// 监听路由变化，重新加载
watch(() => route.query.date, () => {
  loadArticle()
})
</script>

<style scoped>
.daily-page {
  padding: 0 24px 32px;
  max-width: 800px;
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

/* 日期导航 */
.date-nav {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 20px;
  margin-bottom: 32px;
}

.current-date {
  font-size: 18px;
  font-weight: 600;
  color: var(--text-primary);
}

/* 音频播放器 */
.audio-player {
  margin-bottom: 24px;
  text-align: center;
}

.audio-control {
  width: 100%;
  max-width: 500px;
}

/* 文章内容 */
.article-content {
  animation: fadeIn 0.3s ease;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}

.article-section {
  margin-bottom: 24px;
  background: var(--bg-card);
  border: 1px solid var(--border-glass);
  border-radius: 12px;
  padding: 20px;
}

.section-header {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 16px;
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 16px;
  padding-bottom: 12px;
  border-bottom: 1px solid #f0f3f8;
}

.toggle-btn {
  margin-left: auto;
}

.article-text {
  line-height: 1.8;
  font-size: 15px;
}

.article-paragraph {
  text-indent: 2em;       /* 每段首行缩进两个字符 */
  margin-bottom: 0.8em;
}

.english-text {
  color: var(--text-primary);
}

.chinese-text {
  color: var(--text-secondary);
}

.translation-hint {
  color: var(--text-muted);
  font-size: 14px;
  text-align: center;
  padding: 20px 0;
}

/* 操作按钮 */
.article-actions {
  display: flex;
  justify-content: center;
  gap: 16px;
  margin-top: 32px;
  padding-top: 24px;
  border-top: 1px solid #eef1f6;
}

/* 加载和空状态 */
.loading-wrapper {
  padding: 20px;
}

.empty-wrapper {
  padding: 60px 0;
}
</style>
