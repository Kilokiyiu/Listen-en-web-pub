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
        <span class="home-announcement-text">🎧 ListenEase — 专注英语听力学习，提供四六级真题听力、BBC外刊阅读、智能单词复习与每日一句。<br>本站接入BBC新闻资源，所有听力素材均配备原文，即点即查，助你高效提升英语听力水平。</span>
      </div>

      <!-- 每日一句卡片 -->
      <div class="daily-quote-card" v-if="dailyQuote">
        <div class="quote-card-inner">
          <div class="quote-card-header">
            <span class="quote-card-label">每日一句</span>
            <span class="quote-card-date">{{ dailyQuote.date }}</span>
          </div>
          <div class="quote-card-body">
            <el-icon class="quote-card-icon"><Quote /></el-icon>
            <div class="quote-card-text">
              <p class="quote-card-en">{{ dailyQuote.content }}</p>
              <p class="quote-card-cn">{{ dailyQuote.note }}</p>
            </div>
          </div>
        </div>
      </div>

      <!-- 搜索区域 -->
      <div class="search-section">
        <el-input
          v-model="searchWord"
          placeholder="输入英语单词，查询释义、例句、同根词..."
          size="large"
          class="search-input word-search"
          @keyup.enter="doSearch"
        >
          <template #append>
            <el-button type="primary" @click="doSearch" :loading="searchLoading">
              <el-icon><Search /></el-icon>
            </el-button>
          </template>
        </el-input>
        <h2 class="category-heading">{{ currentCategory.title }}</h2>
        <p class="subtitle">{{ currentCategory.subtitle }}</p>
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
            <el-card shadow="hover" class="quick-card" @click="goWordRoots">
              <el-icon :size="32" color="#67c23a"><Collection /></el-icon>
              <div class="quick-title">词根学习</div>
              <div class="quick-desc">提高你的词汇量</div>
            </el-card>
          </el-col>
          <el-col :xs="12" :sm="8" :md="6">
            <el-card shadow="hover" class="quick-card">
              <el-icon :size="32" color="#e6a23c"><Trophy /></el-icon>
              <div class="quick-title">考研英语(开发中)</div>
              <div class="quick-desc">进一步提高水平</div>
            </el-card>
          </el-col>
          <el-col :xs="12" :sm="8" :md="6">
            <el-card shadow="hover" class="quick-card" @click="goBBCNews">
              <el-icon :size="32" color="#f56c6c"><Document /></el-icon>
              <div class="quick-title">BBC外刊</div>
              <div class="quick-desc">精选BBC新闻阅读</div>
            </el-card>
          </el-col>
        </el-row>
      </div>
    </main>

    <!-- 单词详情弹窗 -->
    <el-dialog
      v-model="wordDialogVisible"
      :title="wordDetail?.word || '单词详情'"
      width="90%"
      :max-width="700"
      class="word-detail-dialog"
      :style="{ maxWidth: '700px' }"
    >
      <div v-if="wordDetail" class="word-detail-content">
        <!-- 音标和发音 -->
        <div class="phonetic-section">
          <div class="phonetic-item" v-if="wordDetail.ukphone">
            <span class="phonetic-label">英</span>
            <span class="phonetic-text">/{{ wordDetail.ukphone }}/</span>
            <el-button
              v-if="wordDetail.ukspeech"
              link
              type="primary"
              @click="playAudio(wordDetail.ukspeech)"
            >
              <el-icon><VideoPlay /></el-icon>
            </el-button>
          </div>
          <div class="phonetic-item" v-if="wordDetail.usphone">
            <span class="phonetic-label">美</span>
            <span class="phonetic-text">/{{ wordDetail.usphone }}/</span>
            <el-button
              v-if="wordDetail.usspeech"
              link
              type="primary"
              @click="playAudio(wordDetail.usspeech)"
            >
              <el-icon><VideoPlay /></el-icon>
            </el-button>
          </div>
        </div>

        <!-- 翻译 -->
        <div class="detail-section" v-if="wordDetail.translations?.length">
          <h4><el-icon><Collection /></el-icon> 释义</h4>
          <div class="translation-list">
            <el-tag
              v-for="(t, i) in wordDetail.translations"
              :key="i"
              class="translation-tag"
            >
              {{ t.pos }}. {{ t.tran_cn }}
            </el-tag>
          </div>
        </div>

        <!-- 例句 -->
        <div class="detail-section" v-if="wordDetail.sentences?.length">
          <h4><el-icon><Document /></el-icon> 例句</h4>
          <div
            v-for="(s, i) in wordDetail.sentences"
            :key="i"
            class="sentence-item"
          >
            <p class="sentence-en">{{ s.s_content }}</p>
            <p class="sentence-cn">{{ s.s_cn }}</p>
          </div>
        </div>

        <!-- 短语 -->
        <div class="detail-section" v-if="wordDetail.phrases?.length">
          <h4><el-icon><Link /></el-icon> 短语</h4>
          <div class="phrase-list">
            <el-tag
              v-for="(p, i) in wordDetail.phrases.slice(0, 10)"
              :key="i"
              type="info"
              class="phrase-tag"
            >
              {{ p.p_content }}
            </el-tag>
          </div>
        </div>

        <!-- 同根词 -->
        <div class="detail-section" v-if="wordDetail.relWords?.length">
          <h4><el-icon><Connection /></el-icon> 同根词</h4>
          <div
            v-for="(group, i) in wordDetail.relWords"
            :key="i"
            class="relword-group"
          >
            <el-tag size="small" type="warning">{{ group.Pos }}</el-tag>
            <span
              v-for="(w, j) in group.Hwds"
              :key="j"
              class="relword-item"
            >
              {{ w.hwd }}
              <span class="relword-tran">{{ w.tran }}</span>
            </span>
          </div>
        </div>

        <!-- 近义词 -->
        <div class="detail-section" v-if="wordDetail.synonyms?.length">
          <h4><el-icon><Share /></el-icon> 近义词</h4>
          <div
            v-for="(group, i) in wordDetail.synonyms"
            :key="i"
            class="synonym-group"
          >
            <el-tag size="small" type="success">{{ group.pos }}</el-tag>
            <span
              v-for="(w, j) in group.Hwds"
              :key="j"
              class="synonym-item"
            >
              {{ w.word }}
            </span>
          </div>
        </div>
      </div>

      <template #footer>
        <el-button @click="wordDialogVisible = false">关闭</el-button>
        <el-button
          type="primary"
          @click="addToWordBook"
          :loading="addingWord"
          :disabled="!isLoggedIn"
        >
          <el-icon><Plus /></el-icon>
          {{ isLoggedIn ? '加入单词本' : '请先登录' }}
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { getCategories, getAlbumsByCategoryId } from '../api/Listen.js'
import { queryEnglishWord, addUserWord, getDailyEnglish } from '../api/Word.js'

const router = useRouter()
const searchWord = ref('')
const searchLoading = ref(false)
const wordDialogVisible = ref(false)
const wordDetail = ref(null)
const addingWord = ref(false)
const isLoggedIn = computed(() => !!localStorage.getItem('token'))

// 每日一句
const dailyQuote = ref(null)
const loadingQuote = ref(false)

const loadDailyQuote = async () => {
  loadingQuote.value = true
  try {
    const res = await getDailyEnglish()
    if (res.code === 200 && res.data) {
      dailyQuote.value = res.data
    }
  } catch (e) {
    console.error('获取每日一句失败', e)
  } finally {
    loadingQuote.value = false
  }
}

const refreshDailyQuote = () => {
  loadDailyQuote()
}

const activeCategory = ref('')
const albumsLoading = ref(false)

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
const loadCategories = async (retryCount = 0) => {
  try {
    const data = await getCategories()
    categories.value = data || []
    if (categories.value.length > 0) {
      activeCategory.value = categories.value[0].code
    }
  } catch (e) {
    console.error('获取分类失败', e)
    // 重试最多3次
    if (retryCount < 3) {
      setTimeout(() => loadCategories(retryCount + 1), 1000)
    }
  }
}

// 加载试卷数据
const loadAlbums = async (retryCount = 0) => {
  if (!activeCategory.value) return
  const cat = categories.value.find(c => c.code === activeCategory.value)
  if (!cat) return

  try {
    albumsLoading.value = true
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
    // 重试最多3次
    if (retryCount < 3) {
      setTimeout(() => loadAlbums(retryCount + 1), 1000)
    }
  } finally {
    albumsLoading.value = false
  }
}

// 切换分类时重新加载试卷
watch(activeCategory, (newVal) => {
  if (newVal && categories.value.length > 0) {
    loadAlbums()
  }
})

onMounted(async () => {
  await loadCategories()
  // 确保分类加载完成后再加载试卷
  await nextTick()
  if (activeCategory.value) {
    loadAlbums()
  }
  // 加载每日一句
  loadDailyQuote()
})

const handleSelect = (index) => {
  activeCategory.value = index
}

const doSearch = async () => {
  const word = searchWord.value.trim()
  if (!word) return

  // 只允许输入英文单词
  if (!/^[a-zA-Z\s-]+$/.test(word)) {
    ElMessage.warning('请输入有效的英语单词')
    return
  }

  searchLoading.value = true
  try {
    const res = await queryEnglishWord(word)
    if (res.code === 200 && res.data) {
      wordDetail.value = res.data
      wordDialogVisible.value = true
    } else {
      ElMessage.warning('未找到该单词的详细信息')
    }
  } catch (e) {
    ElMessage.error('查询失败，请稍后重试')
  } finally {
    searchLoading.value = false
  }
}

const playAudio = (url) => {
  const audio = new Audio(url)
  audio.play().catch(() => {
    ElMessage.warning('音频播放失败')
  })
}

const addToWordBook = async () => {
  if (!isLoggedIn.value) {
    ElMessage.warning('请先登录')
    router.push({ name: 'login' })
    return
  }

  const word = wordDetail.value.word
  // 提取释义作为 definition
  const definition = wordDetail.value.translations
    ?.map(t => `${t.pos}. ${t.tran_cn}`)
    .join('; ') || ''

  // 提取第一个例句
  const example = wordDetail.value.sentences?.[0]
    ? `${wordDetail.value.sentences[0].s_content}\n${wordDetail.value.sentences[0].s_cn}`
    : ''

  addingWord.value = true
  try {
    await addUserWord({ word, definition, example })
    ElMessage.success(`"${word}" 已加入单词本`)
    wordDialogVisible.value = false
  } catch (e) {
    if (e.response?.status === 409) {
      ElMessage.warning('该单词已在单词本中')
    } else {
      ElMessage.error('添加失败，请稍后重试')
    }
  } finally {
    addingWord.value = false
  }
}

const goAlbum = (albumId) => {
  router.push({ name: 'examDetail', query: { albumId } })
}

const goDailyArticle = () => {
  router.push({ name: 'dailyArticle' })
}

const goWordRoots = () => {
  router.push({ name: 'wordRoots' })
}

const goBBCNews = () => {
  router.push({ name: 'bbcNews' })
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

/* 每日一句卡片 */
.daily-quote-card {
  max-width: 800px;
  margin: 0 auto 24px;
}

.quote-card-inner {
  background: var(--bg-card, #fff);
  border: 1px solid var(--border-glass, #e2e2e2);
  border-radius: 12px;
  padding: 20px 24px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.04);
  transition: box-shadow 0.3s ease;
}

.daily-quote-card:hover .quote-card-inner {
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
}

.quote-card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 14px;
}

.quote-card-label {
  font-size: 12px;
  font-weight: 600;
  color: var(--accent-blue);
  text-transform: uppercase;
  letter-spacing: 1px;
}

.quote-card-date {
  font-size: 12px;
  color: var(--text-muted);
}

.quote-card-body {
  display: flex;
  gap: 12px;
  align-items: flex-start;
}

.quote-card-icon {
  font-size: 22px;
  color: var(--accent-blue);
  margin-top: 2px;
  flex-shrink: 0;
  opacity: 0.6;
}

.quote-card-text {
  flex: 1;
}

.quote-card-en {
  font-size: 16px;
  font-weight: 500;
  color: var(--text-primary);
  line-height: 1.6;
  margin-bottom: 8px;
  font-style: italic;
}

.quote-card-cn {
  font-size: 14px;
  color: var(--text-secondary);
  line-height: 1.5;
}

.search-section {
  text-align: center;
  padding: 24px 20px 40px;
}

.search-section .category-heading {
  font-size: 34px;
  font-weight: 700;
  color: var(--text-primary);
  margin-bottom: 10px;
  margin-top: 24px;
}

.subtitle {
  color: var(--text-secondary);
  margin-bottom: 0;
  font-size: 16px;
}

.search-input {
  max-width: 600px;
  margin: 0 auto;
}

.word-search {
  margin-bottom: 28px;
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
  margin-bottom: 20px;
}

.section-header h2 {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 18px;
  font-weight: 600;
  color: var(--text-primary);
}

.section-header :deep(.el-link) {
  color: var(--accent-blue);
  font-size: 14px;
}

/* 试卷卡片 */
.exam-card {
  margin-bottom: 16px;
  cursor: pointer;
  transition: all 0.25s ease;
  position: relative;
  background: var(--bg-card) !important;
  border: 1px solid var(--border-glass) !important;
  border-radius: 12px !important;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
  overflow: hidden;
}

.exam-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.08);
  border-color: rgba(64, 158, 255, 0.2) !important;
}

.exam-tag {
  display: inline-block;
  padding: 3px 10px;
  border-radius: 6px;
  font-size: 11px;
  font-weight: 600;
  margin-bottom: 10px;
  letter-spacing: 0.3px;
}

.exam-tag.cet4,
.exam-tag.cet6 {
  background: rgba(64, 158, 255, 0.08);
  color: var(--accent-blue);
}

.exam-tag.ielts {
  background: rgba(103, 194, 58, 0.08);
  color: #67c23a;
}

.exam-tag.toefl {
  background: rgba(230, 162, 60, 0.08);
  color: #e6a23c;
}

.exam-title {
  font-size: 14px;
  font-weight: 500;
  color: var(--text-primary);
  line-height: 1.5;
  margin-bottom: 10px;
  min-height: 42px;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.exam-info {
  display: flex;
  align-items: center;
  gap: 6px;
  color: var(--text-muted);
  font-size: 12px;
}

/* 快捷练习卡片 */
.quick-card {
  text-align: center;
  padding: 24px 0;
  cursor: pointer;
  transition: all 0.25s ease;
  margin-bottom: 16px;
  background: var(--bg-card) !important;
  border: 1px solid var(--border-glass) !important;
  border-radius: 12px !important;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
}

.quick-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.08);
  border-color: rgba(64, 158, 255, 0.2) !important;
}

.quick-title {
  margin-top: 14px;
  font-size: 14px;
  font-weight: 500;
  color: var(--text-primary);
}

.quick-desc {
  margin-top: 4px;
  font-size: 12px;
  color: var(--text-muted);
}

/* 首页公告 */
.home-announcement {
  max-width: 800px;
  margin: 0 auto 20px;
  padding: 14px 24px;
  background: var(--bg-card, #fff);
  border: 1px solid var(--border-glass, #e2e2e2);
  border-radius: 10px;
  text-align: center;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.03);
}

.home-announcement-text {
  font-size: 13px;
  color: var(--text-secondary);
  letter-spacing: 0.3px;
  line-height: 1.6;
}

/* 单词详情弹窗样式 */
.word-detail-dialog :deep(.el-dialog) {
  width: 90% !important;
  max-width: 700px !important;
  margin: 5vh auto !important;
  max-height: 90vh;
  display: flex;
  flex-direction: column;
}

.word-detail-dialog :deep(.el-dialog__body) {
  flex: 1;
  overflow: hidden;
  padding: 16px 20px;
}

.word-detail-content {
  max-height: calc(90vh - 120px);
  overflow-y: auto;
  padding-right: 8px;
}

.phonetic-section {
  display: flex;
  gap: 24px;
  margin-bottom: 20px;
  padding-bottom: 16px;
  border-bottom: 1px solid #eee;
}

.phonetic-item {
  display: flex;
  align-items: center;
  gap: 8px;
}

.phonetic-label {
  font-size: 12px;
  color: #fff;
  background: #409eff;
  padding: 2px 6px;
  border-radius: 4px;
}

.phonetic-text {
  font-size: 16px;
  color: var(--text-primary);
  font-family: 'Times New Roman', serif;
}

.detail-section {
  margin-bottom: 20px;
}

.detail-section h4 {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 15px;
  color: var(--text-primary);
  margin-bottom: 12px;
  padding-bottom: 8px;
  border-bottom: 1px solid #f0f0f0;
}

.translation-list {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.translation-tag {
  font-size: 14px;
}

.sentence-item {
  margin-bottom: 12px;
  padding: 12px;
  background: #f8f9fa;
  border-radius: 8px;
}

.sentence-en {
  font-size: 14px;
  color: var(--text-primary);
  margin-bottom: 4px;
}

.sentence-cn {
  font-size: 13px;
  color: var(--text-muted);
}

.phrase-list {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.phrase-tag {
  font-size: 13px;
}

.relword-group,
.synonym-group {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 8px;
  margin-bottom: 8px;
}

.relword-item,
.synonym-item {
  font-size: 14px;
  color: var(--text-primary);
}

.relword-tran {
  font-size: 12px;
  color: var(--text-muted);
}

/* 手机端适配 */
@media (max-width: 768px) {
  .daily-quote-card {
    margin: 0 12px 16px;
  }

  .quote-card-inner {
    padding: 16px;
  }

  .quote-card-en {
    font-size: 14px;
  }

  .quote-card-cn {
    font-size: 13px;
  }

  .home-announcement {
    margin: 0 12px 16px;
    padding: 12px 16px;
  }

  .word-detail-dialog :deep(.el-dialog) {
    width: 95% !important;
    max-width: 95% !important;
    margin: 2vh auto !important;
    max-height: 96vh;
    display: flex;
    flex-direction: column;
  }

  .word-detail-dialog :deep(.el-dialog__body) {
    padding: 12px 16px;
    flex: 1;
    overflow: hidden;
  }

  .word-detail-content {
    max-height: calc(96vh - 120px);
    overflow-y: auto;
  }

  .phonetic-section {
    flex-direction: column;
    gap: 8px;
    margin-bottom: 12px;
    padding-bottom: 12px;
  }

  .phonetic-text {
    font-size: 14px;
  }

  .detail-section {
    margin-bottom: 12px;
  }

  .detail-section h4 {
    font-size: 14px;
    margin-bottom: 8px;
    padding-bottom: 6px;
  }

  .sentence-item {
    padding: 8px;
    margin-bottom: 8px;
  }

  .sentence-en {
    font-size: 13px;
  }

  .sentence-cn {
    font-size: 12px;
  }

  .translation-tag,
  .phrase-tag {
    font-size: 12px;
  }

  .relword-item,
  .synonym-item {
    font-size: 13px;
  }
}
</style>
