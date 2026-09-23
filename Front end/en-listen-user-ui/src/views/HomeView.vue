<template>
  <div class="home-page le-page">
    <!-- Hero -->
    <section class="hero">
      <div class="hero-inner">
        <div class="hero-content">
          <p class="hero-tag">今日学习</p>
          <h1 class="hero-title">ListenEase</h1>
          <p class="hero-desc">查词、练听力、记单词——从这里开始今天的学习</p>
          <div class="hero-search">
            <el-input
              v-model="searchWord"
              placeholder="输入单词，即点即查释义与例句"
              size="large"
              class="search-input"
              @keyup.enter="doSearch"
            >
              <template #append>
                <el-button type="primary" class="search-btn" @click="doSearch" :loading="searchLoading">
                  <el-icon><Search /></el-icon>
                </el-button>
              </template>
            </el-input>
          </div>
        </div>

        <aside class="hero-app-card">
          <div class="app-card-top">
            <span class="app-badge">ANDROID</span>
            <span class="app-badge app-badge--purple">v0.8.9.2</span>
          </div>
          <div class="app-title-row">
            <h2 class="app-name">EaseWord</h2>
          </div>
          <p class="app-cn">听易词</p>
          <p class="app-desc">官方单词本 · 与网站同账号</p>
          <a
            class="app-download"
            href="/downloads/EaseWord-0.8.9.2.apk"
            download="EaseWord-0.8.9.2.apk"
          >
            下载 APK
          </a>
          <p class="app-note" title="旧签名版本（如 0.8.0）需先上传云端并卸载后再安装；之后可直接覆盖更新。">
            仅 Android · 旧版需先卸载再装
          </p>
        </aside>
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
    <div v-if="dailyQuote" class="quote-card">
      <div class="quote-header">
        <span class="quote-label">DAILY · 每日一句</span>
        <span class="quote-date">{{ dailyQuote.date }}</span>
      </div>
      <p class="quote-en">{{ dailyQuote.content }}</p>
      <p class="quote-cn">{{ dailyQuote.note }}</p>
    </div>

    <!-- 当前分类标题 -->
    <div class="category-intro">
      <h2>{{ currentCategory.title }}</h2>
      <div class="category-meta">
        <span class="meta-pill">{{ (activeCategory || 'CET').toUpperCase() }}</span>
        <span>{{ currentCategory.subtitle }}</span>
      </div>
    </div>

    <!-- 试卷列表 -->
    <section class="le-section">
      <div class="le-section-header">
        <h2>
          <el-icon color="var(--le-accent)"><Document /></el-icon>
          {{ currentCategory.listTitle }}
        </h2>
        <a class="view-all" href="javascript:;" @click.prevent="goExamList()">查看全部 →</a>
      </div>

      <div v-if="albumsLoading" class="le-loading-wrap">
        <el-icon class="is-loading" :size="28"><Loading /></el-icon>
        <span>加载试卷中...</span>
      </div>

      <el-row v-else :gutter="16">
        <el-col v-for="item in currentList" :key="item.id" :xs="12" :sm="8" :md="6">
          <div class="exam-card" @click="goAlbum(item.id)">
            <div class="exam-tags">
              <span class="exam-tag" :class="activeCategory">{{ item.tag }}</span>
            </div>
            <h3 class="exam-title">{{ item.title }}</h3>
            <div class="exam-meta">
              <span class="exam-meta-text">听力练习</span>
              <span class="start-btn">开始练习 →</span>
            </div>
          </div>
        </el-col>
      </el-row>

      <el-empty v-if="!albumsLoading && currentList.length === 0" description="暂无试卷" />
    </section>

    <!-- 快捷入口 -->
    <section class="le-section">
      <div class="le-section-header">
        <h2><el-icon color="var(--le-accent)"><Star /></el-icon> 快捷入口</h2>
      </div>
      <el-row :gutter="16">
        <el-col v-for="item in quickLinks" :key="item.title" :xs="12" :sm="6">
          <div class="quick-card" @click="item.action?.()">
            <div class="quick-icon">
              <el-icon :size="22"><component :is="item.icon" /></el-icon>
            </div>
            <div class="quick-info">
              <h3>{{ item.title }}</h3>
              <p>{{ item.desc }}</p>
            </div>
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
import { queryEnglishWord, addUserWordToCurrentBook, getDailyEnglish, isValidEnglishQuery } from '../api/Word.js'
import { promptGoReviewAfterAdd } from '../utils/promptGoReview.js'

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
  cet6: { title: '英语六级听力练习', subtitle: '历年真题，助你轻松过级', listTitle: '六级听力真题', color: '#a78bfa' },
  cet4: { title: '英语四级听力练习', subtitle: '历年真题 + 模拟试题', listTitle: '四级听力真题', color: '#22d3ee' },
  ielts: { title: '雅思听力练习', subtitle: '剑桥雅思真题 + 模拟训练', listTitle: '雅思真题', color: '#22c55e' },
  toefl: { title: '托福听力练习', subtitle: 'TPO真题 + 专项训练', listTitle: '托福真题', color: '#fbbf24' }
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
    await addUserWordToCurrentBook({ word, definition, example })
    wordDialogVisible.value = false
    await promptGoReviewAfterAdd(router, word, '/')
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
  { title: '每日短文', desc: '10 分钟保持语感', icon: 'Microphone', action: goDailyArticle },
  { title: '词根学习', desc: '系统扩展词汇', icon: 'Collection', action: goWordRoots },
  { title: 'BBC 外刊', desc: '精选新闻阅读', icon: 'Document', action: goBBCNews },
  { title: '单词复习', desc: '智能间隔复习', icon: 'Reading', action: () => router.push('/word-review') },
]
</script>

<style scoped>
.home-page {
  padding-top: 8px;
}

.hero {
  position: relative;
  padding: 28px 24px 32px;
  margin-bottom: 8px;
  overflow: hidden;
  border-radius: var(--le-radius);
  background: var(--le-bg-surface);
  border: 1px solid var(--le-border);
}

.hero::before {
  content: '';
  position: absolute;
  inset: 0;
  background: linear-gradient(105deg, rgba(59, 130, 246, 0.08) 0%, transparent 55%);
  pointer-events: none;
}

.hero-inner {
  position: relative;
  z-index: 1;
  display: grid;
  grid-template-columns: 1.5fr 0.9fr;
  gap: 28px;
  align-items: center;
}

.hero-content {
  min-width: 0;
}

.hero-tag {
  display: inline-flex;
  align-items: center;
  margin: 0 0 12px;
  padding: 4px 10px;
  background: var(--le-bg-muted);
  border: 1px solid var(--le-border);
  border-radius: 6px;
  color: var(--le-text-secondary);
  font-family: var(--le-font-mono);
  font-size: 12px;
  font-weight: 500;
  letter-spacing: 0.04em;
}

.hero-title {
  font-family: var(--le-font-display);
  font-size: clamp(1.75rem, 3.5vw, 2.4rem);
  font-weight: 700;
  line-height: 1.15;
  letter-spacing: -0.02em;
  margin: 0 0 10px;
  color: var(--le-text);
}

.hero-desc {
  font-size: 15px;
  color: var(--le-text-secondary);
  max-width: 480px;
  margin: 0 0 20px;
  line-height: 1.65;
}

.hero-search :deep(.el-input__wrapper) {
  border-radius: 10px 0 0 10px;
  background: var(--le-bg-elev) !important;
  box-shadow: 0 0 0 1px var(--le-border) inset !important;
  padding-left: 8px;
}

.hero-search :deep(.el-input-group__append) {
  border-radius: 0 10px 10px 0;
  overflow: hidden;
  box-shadow: none;
  background: transparent;
  padding: 0;
}

.hero-search :deep(.search-btn) {
  height: 42px;
  width: 48px;
  margin: 3px;
  border-radius: 8px !important;
  background: var(--le-primary) !important;
  border: none !important;
  color: #0b1220 !important;
}

.hero-search :deep(.el-input__wrapper.is-focus) {
  box-shadow: 0 0 0 1px var(--le-primary) inset !important;
}

.hero-app-card {
  justify-self: stretch;
  width: 100%;
  max-width: none;
  background: var(--le-bg-elevated);
  border: 1px solid var(--le-border);
  border-radius: var(--le-radius);
  padding: 18px;
}

.app-card-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  margin-bottom: 12px;
}

.app-badge {
  font-family: var(--le-font-mono);
  font-size: 11px;
  font-weight: 500;
  color: #bfdbfe;
  background: rgba(59, 130, 246, 0.16);
  border: 1px solid rgba(96, 165, 250, 0.28);
  padding: 3px 8px;
  border-radius: 6px;
  letter-spacing: 0.04em;
}

.app-badge--purple {
  color: #ddd6fe;
  background: rgba(139, 92, 246, 0.16);
  border-color: rgba(167, 139, 250, 0.3);
}

.app-name {
  margin: 0 0 4px;
  font-family: var(--le-font-display);
  font-size: 22px;
  font-weight: 700;
  color: var(--le-text);
}

.app-cn {
  margin: 0 0 10px;
  color: var(--le-text-secondary);
  font-size: 13px;
  font-weight: 500;
}

.app-desc {
  margin: 0 0 16px;
  font-size: 13px;
  color: var(--le-text-secondary);
}

.app-download {
  display: block;
  text-align: center;
  text-decoration: none;
  background: var(--le-primary);
  color: #0b1220;
  font-weight: 700;
  font-size: 14px;
  padding: 10px;
  border-radius: 8px;
  transition: opacity 0.15s ease, background 0.15s ease;
}

.app-download:hover {
  opacity: 0.92;
  background: var(--le-primary-light);
  color: #0b1220;
}

.app-note {
  margin: 10px 0 0;
  font-size: 11px;
  color: var(--le-text-muted);
  text-align: center;
  line-height: 1.35;
}

.category-tabs-wrap {
  margin: 8px 0 24px;
  border-bottom: 1px solid var(--le-border);
}

.category-tabs {
  display: flex;
  gap: 24px;
  overflow-x: auto;
  -webkit-overflow-scrolling: touch;
  scrollbar-width: none;
}

.category-tabs::-webkit-scrollbar {
  display: none;
}

.category-tab {
  position: relative;
  flex-shrink: 0;
  border: none;
  background: transparent;
  color: var(--le-text-muted);
  padding: 12px 4px;
  font-size: 15px;
  font-weight: 500;
  cursor: pointer;
  transition: color 0.2s;
  font-family: inherit;
}

.category-tab:hover:not(.active) {
  color: var(--le-text);
}

.category-tab.active {
  color: var(--le-primary);
}

.category-tab.active::after {
  content: '';
  position: absolute;
  bottom: -1px;
  left: 0;
  right: 0;
  height: 2px;
  background: var(--le-primary);
  border-radius: 2px;
}

.quote-card {
  position: relative;
  overflow: hidden;
  background: var(--le-bg-elevated);
  border: 1px solid var(--le-border);
  border-radius: var(--le-radius);
  padding: 20px 22px 20px 26px;
  margin-bottom: 24px;
  transition: border-color 0.2s ease, box-shadow 0.2s ease;
}

.quote-card::before {
  content: '';
  position: absolute;
  left: 0;
  top: 14px;
  bottom: 14px;
  width: 3px;
  background: var(--le-primary);
  border-radius: 3px;
}

.quote-card:hover {
  border-color: var(--le-border-strong);
  box-shadow: var(--le-shadow-sm);
}

.quote-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
}

.quote-label {
  font-family: var(--le-font-mono);
  font-size: 11px;
  color: var(--le-primary-light);
  letter-spacing: 0.06em;
  background: rgba(59, 130, 246, 0.1);
  border: 1px solid rgba(96, 165, 250, 0.2);
  padding: 3px 8px;
  border-radius: 6px;
}

.quote-date {
  font-family: var(--le-font-mono);
  font-size: 12px;
  color: var(--le-text-muted);
}

.quote-en {
  font-family: var(--le-font-display);
  font-size: 18px;
  font-style: italic;
  font-weight: 500;
  margin: 0 0 8px;
  color: var(--le-text);
  line-height: 1.5;
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
  font-family: var(--le-font-display);
  font-size: 24px;
  font-weight: 700;
  margin: 0 0 8px;
  color: var(--le-text);
}

.category-meta {
  display: flex;
  align-items: center;
  gap: 12px;
  color: var(--le-text-secondary);
  font-size: 14px;
}

.meta-pill {
  font-family: var(--le-font-mono);
  font-size: 11px;
  color: #c7d2fe;
  background: var(--le-gradient-soft);
  border: 1px solid rgba(165, 180, 252, 0.22);
  padding: 3px 8px;
  border-radius: 6px;
  letter-spacing: 0.04em;
}

.view-all {
  color: var(--le-primary);
  font-size: 14px;
  font-weight: 500;
  text-decoration: none;
  transition: color 0.2s;
}

.view-all:hover {
  color: var(--le-primary-light);
}

.exam-card {
  padding: 18px;
  margin-bottom: 16px;
  height: calc(100% - 16px);
  background: var(--le-bg-elevated);
  border: 1px solid var(--le-border);
  border-radius: var(--le-radius);
  cursor: pointer;
  transition: transform 0.2s ease, border-color 0.2s ease, box-shadow 0.2s ease, background 0.2s ease;
}

.exam-card:hover {
  transform: translateY(-2px);
  border-color: var(--le-border-strong);
  box-shadow: var(--le-shadow);
  background: var(--le-bg-elevated-hover);
}

.exam-tags {
  display: flex;
  gap: 8px;
  margin-bottom: 12px;
}

.exam-tag {
  display: inline-block;
  font-family: var(--le-font-mono);
  padding: 3px 8px;
  border-radius: 6px;
  font-size: 11px;
  font-weight: 600;
  letter-spacing: 0.04em;
  background: rgba(59, 130, 246, 0.14);
  color: #93c5fd;
  border: 1px solid rgba(96, 165, 250, 0.25);
}

.exam-tag.cet6 {
  background: rgba(139, 92, 246, 0.14);
  color: #c4b5fd;
  border-color: rgba(167, 139, 250, 0.28);
}

.exam-tag.ielts {
  background: rgba(52, 211, 153, 0.1);
  color: var(--le-success);
  border-color: rgba(52, 211, 153, 0.25);
}

.exam-tag.toefl {
  background: rgba(251, 191, 36, 0.1);
  color: var(--le-warning);
  border-color: rgba(251, 191, 36, 0.25);
}

.exam-title {
  font-size: 15px;
  font-weight: 600;
  margin: 0 0 14px;
  line-height: 1.45;
  min-height: 44px;
  color: var(--le-text);
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.exam-meta {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  padding-top: 12px;
  border-top: 1px solid var(--le-border);
}

.exam-meta-text {
  font-family: var(--le-font-mono);
  font-size: 11px;
  color: var(--le-text-muted);
}

.start-btn {
  display: inline-flex;
  align-items: center;
  padding: 5px 12px;
  background: rgba(59, 130, 246, 0.1);
  border: 1px solid rgba(96, 165, 250, 0.22);
  color: var(--le-primary-light);
  border-radius: 8px;
  font-size: 12px;
  font-weight: 500;
  transition: all 0.2s ease;
}

.exam-card:hover .start-btn {
  background: var(--le-primary);
  color: #0b1220;
  border-color: transparent;
}

.quick-card {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 18px;
  margin-bottom: 16px;
  height: calc(100% - 16px);
  background: var(--le-bg-elevated);
  border: 1px solid var(--le-border);
  border-radius: var(--le-radius);
  cursor: pointer;
  transition: transform 0.2s ease, border-color 0.2s ease, box-shadow 0.2s ease, background 0.2s ease;
  text-align: left;
}

.quick-card:hover {
  transform: translateY(-2px);
  border-color: var(--le-border-strong);
  box-shadow: var(--le-shadow-sm);
  background: var(--le-bg-elevated-hover);
}

.quick-icon {
  width: 44px;
  height: 44px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  background: rgba(59, 130, 246, 0.14);
  color: var(--le-primary-light);
}

.quick-info h3 {
  font-size: 15px;
  font-weight: 600;
  margin: 0 0 2px;
  color: var(--le-text);
}

.quick-info p {
  font-size: 12px;
  color: var(--le-text-secondary);
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
  color: #0b1220;
  background: var(--le-primary);
  padding: 2px 6px;
  border-radius: 4px;
  font-weight: 600;
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
  border: 1px solid var(--le-border);
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

@media (max-width: 1024px) {
  .hero-inner {
    grid-template-columns: 1fr;
    gap: 24px;
  }
  .hero-app-card {
    max-width: none;
  }
}

@media (max-width: 768px) {
  .hero {
    padding: 20px 16px 24px;
  }
  .hero-title {
    font-size: 1.65rem;
  }
  .quote-card {
    padding: 16px 16px 16px 22px;
  }
  .category-intro h2 {
    font-size: 20px;
  }
  .exam-meta-text {
    display: none;
  }
}
</style>
