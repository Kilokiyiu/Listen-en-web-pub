<template>
  <div class="home-page le-page">
    <!-- Hero -->
    <section class="hero">
      <div class="hero-content">
        <p class="hero-tag">英语听力 · 智能学习</p>
        <h1 class="hero-title">ListenEase</h1>
        <p class="hero-desc">四六级真题听力、BBC 外刊阅读、词根单词与每日一句，一站提升听力水平</p>
        <div class="hero-search">
          <el-input
            v-model="searchWord"
            placeholder="输入单词，即点即查释义与例句"
            size="large"
            class="search-input"
            @keyup.enter="doSearch"
          >
            <template #append>
              <el-button type="primary" class="le-btn-gradient" @click="doSearch" :loading="searchLoading">
                <el-icon><Search /></el-icon>
              </el-button>
            </template>
          </el-input>
        </div>
      </div>
    </section>

    <!-- 分类 Tab -->
    <div class="category-tabs-wrap">
      <div class="category-tabs" role="tablist">
        <button
          v-for="cat in categories"
          :key="cat.code"
          type="button"
          role="tab"
          class="category-tab"
          :class="{ active: activeCategory === cat.code }"
          @click="handleSelect(cat.code)"
        >
          {{ cat.name?.chinese || cat.name }}
        </button>
      </div>
    </div>

    <!-- 每日一句 -->
    <div v-if="dailyQuote" class="quote-card le-card">
      <div class="quote-header">
        <span class="quote-label">每日一句</span>
        <span class="quote-date">{{ dailyQuote.date }}</span>
      </div>
      <p class="quote-en">{{ dailyQuote.content }}</p>
      <p class="quote-cn">{{ dailyQuote.note }}</p>
    </div>

    <!-- 当前分类标题 -->
    <div class="category-intro">
      <h2>{{ currentCategory.title }}</h2>
      <p>{{ currentCategory.subtitle }}</p>
    </div>

    <!-- 试卷列表 -->
    <section class="le-section">
      <div class="le-section-header">
        <h2>
          <el-icon :color="currentCategory.color"><Document /></el-icon>
          {{ currentCategory.listTitle }}
        </h2>
        <el-link type="primary" @click="goExamList()">查看全部</el-link>
      </div>

      <div v-if="albumsLoading" class="le-loading-wrap">
        <el-icon class="is-loading" :size="28"><Loading /></el-icon>
        <span>加载试卷中...</span>
      </div>

      <el-row v-else :gutter="16">
        <el-col v-for="item in currentList" :key="item.id" :xs="12" :sm="8" :md="6">
          <div class="exam-card le-card le-card-interactive" @click="goAlbum(item.id)">
            <span class="exam-tag" :class="activeCategory">{{ item.tag }}</span>
            <h3 class="exam-title">{{ item.title }}</h3>
            <div class="exam-meta">
              <el-icon><Headset /></el-icon>
              <span>开始练习</span>
            </div>
          </div>
        </el-col>
      </el-row>

      <el-empty v-if="!albumsLoading && currentList.length === 0" description="暂无试卷" />
    </section>

    <!-- 快捷入口 -->
    <section class="le-section">
      <div class="le-section-header">
        <h2><el-icon color="#f59e0b"><Star /></el-icon> 快捷入口</h2>
      </div>
      <el-row :gutter="16">
        <el-col v-for="item in quickLinks" :key="item.title" :xs="12" :sm="6">
          <div class="quick-card le-card le-card-interactive" @click="item.action?.()">
            <div class="quick-icon" :style="{ background: item.bg }">
              <el-icon :size="24" color="#fff"><component :is="item.icon" /></el-icon>
            </div>
            <h3>{{ item.title }}</h3>
            <p>{{ item.desc }}</p>
          </div>
        </el-col>
      </el-row>
    </section>

    <!-- 单词详情弹窗 -->
    <el-dialog v-model="wordDialogVisible" :title="wordDetail?.word || '单词详情'" width="90%" class="word-detail-dialog">
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
import { queryEnglishWord, addUserWord, getDailyEnglish, isValidEnglishQuery } from '../api/Word.js'

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

  if (!isValidEnglishQuery(word)) {
    ElMessage.warning('请输入有效的英语单词、短语或句子')
    return
  }

  searchLoading.value = true
  try {
    const res = await queryEnglishWord(word)
    if (res.code === 200 && res.data) {
      wordDetail.value = res.data
      wordDialogVisible.value = true
    } else {
      ElMessage.warning('未找到相关释义')
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

const quickLinks = [
  { title: '每日短文', desc: '10 分钟保持语感', icon: 'Microphone', bg: 'linear-gradient(135deg,#2563eb,#3b82f6)', action: goDailyArticle },
  { title: '词根学习', desc: '系统扩展词汇', icon: 'Collection', bg: 'linear-gradient(135deg,#10b981,#34d399)', action: goWordRoots },
  { title: 'BBC 外刊', desc: '精选新闻阅读', icon: 'Document', bg: 'linear-gradient(135deg,#ef4444,#f87171)', action: goBBCNews },
  { title: '单词复习', desc: '智能间隔复习', icon: 'Reading', bg: 'linear-gradient(135deg,#7c3aed,#a78bfa)', action: () => router.push('/word-review') },
]
</script>

<style scoped>
.home-page {
  padding-top: 8px;
}

.hero {
  background: var(--le-gradient);
  border-radius: var(--le-radius);
  padding: 32px 28px;
  margin-bottom: 20px;
  color: #fff;
  position: relative;
  overflow: hidden;
}

.hero::after {
  content: '';
  position: absolute;
  right: -40px;
  top: -40px;
  width: 200px;
  height: 200px;
  border-radius: 50%;
  background: rgba(255,255,255,0.1);
}

.hero-content {
  position: relative;
  z-index: 1;
  max-width: 640px;
}

.hero-tag {
  font-size: 13px;
  opacity: 0.9;
  margin: 0 0 8px;
  letter-spacing: 0.05em;
}

.hero-title {
  font-size: clamp(1.75rem, 5vw, 2.5rem);
  font-weight: 800;
  margin: 0 0 8px;
  letter-spacing: -0.03em;
}

.hero-desc {
  font-size: 14px;
  opacity: 0.92;
  margin: 0 0 20px;
  line-height: 1.6;
}

.hero-search :deep(.el-input__wrapper) {
  border-radius: 99px 0 0 99px;
  box-shadow: none;
}

.hero-search :deep(.el-input-group__append) {
  border-radius: 0 99px 99px 0;
  overflow: hidden;
  box-shadow: none;
}

.category-tabs-wrap {
  margin-bottom: 20px;
  overflow: hidden;
}

.category-tabs {
  display: flex;
  gap: 8px;
  overflow-x: auto;
  padding-bottom: 4px;
  -webkit-overflow-scrolling: touch;
  scrollbar-width: none;
}

.category-tabs::-webkit-scrollbar {
  display: none;
}

.category-tab {
  flex-shrink: 0;
  border: 1px solid var(--le-border);
  background: var(--le-bg-elevated);
  color: var(--le-text-secondary);
  padding: 10px 18px;
  border-radius: 99px;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.2s;
}

.category-tab.active {
  background: var(--le-gradient);
  border-color: transparent;
  color: #fff;
  font-weight: 600;
  box-shadow: 0 4px 12px rgba(37, 99, 235, 0.3);
}

.quote-card {
  padding: 20px 24px;
  margin-bottom: 24px;
}

.quote-header {
  display: flex;
  justify-content: space-between;
  margin-bottom: 12px;
}

.quote-label {
  font-size: 12px;
  font-weight: 700;
  color: var(--le-primary);
  text-transform: uppercase;
  letter-spacing: 0.08em;
}

.quote-date {
  font-size: 12px;
  color: var(--le-text-muted);
}

.quote-en {
  font-size: 16px;
  font-weight: 500;
  margin: 0 0 8px;
  color: var(--le-text);
  line-height: 1.6;
}

.quote-cn {
  font-size: 14px;
  color: var(--le-text-secondary);
  margin: 0;
}

.category-intro {
  margin-bottom: 20px;
}

.category-intro h2 {
  font-size: 20px;
  margin: 0 0 4px;
}

.category-intro p {
  font-size: 14px;
  color: var(--le-text-muted);
  margin: 0;
}

.exam-card {
  padding: 18px;
  margin-bottom: 16px;
  height: calc(100% - 16px);
}

.exam-tag {
  display: inline-block;
  padding: 3px 10px;
  border-radius: 99px;
  font-size: 11px;
  font-weight: 600;
  margin-bottom: 10px;
}

.exam-tag.cet4 { background: rgba(37,99,235,0.1); color: var(--le-primary); }
.exam-tag.cet6 { background: rgba(124,58,237,0.1); color: var(--le-purple); }
.exam-tag.ielts { background: rgba(16,185,129,0.1); color: var(--le-success); }
.exam-tag.toefl { background: rgba(245,158,11,0.1); color: var(--le-warning); }

.exam-title {
  font-size: 15px;
  font-weight: 600;
  margin: 0 0 12px;
  line-height: 1.5;
  min-height: 44px;
}

.exam-meta {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  color: var(--le-text-muted);
  padding-top: 12px;
  border-top: 1px solid var(--le-border);
}

.quick-card {
  padding: 20px;
  margin-bottom: 16px;
  text-align: center;
  height: calc(100% - 16px);
}

.quick-icon {
  width: 48px;
  height: 48px;
  border-radius: 14px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 12px;
}

.quick-card h3 {
  font-size: 15px;
  margin: 0 0 4px;
}

.quick-card p {
  font-size: 12px;
  color: var(--le-text-muted);
  margin: 0;
}

.word-detail-content {
  max-height: 60vh;
  overflow-y: auto;
}

.phonetic-section {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
  margin-bottom: 16px;
  padding-bottom: 16px;
  border-bottom: 1px solid var(--le-border);
}

.phonetic-item {
  display: flex;
  align-items: center;
  gap: 8px;
}

.phonetic-label {
  font-size: 11px;
  color: #fff;
  background: var(--le-primary);
  padding: 2px 6px;
  border-radius: 4px;
}

.detail-section {
  margin-bottom: 16px;
}

.detail-section h4 {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 14px;
  margin: 0 0 10px;
}

.translation-list, .phrase-list {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.sentence-item {
  padding: 12px;
  background: var(--le-bg-muted);
  border-radius: var(--le-radius-sm);
  margin-bottom: 8px;
}

.sentence-en { font-size: 14px; margin: 0 0 4px; }
.sentence-cn { font-size: 13px; color: var(--le-text-muted); margin: 0; }

.relword-group, .synonym-group {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-bottom: 8px;
  align-items: center;
}

@media (max-width: 768px) {
  .hero {
    padding: 24px 18px;
    border-radius: var(--le-radius-sm);
  }
  .quote-card {
    padding: 16px;
  }
}
</style>
