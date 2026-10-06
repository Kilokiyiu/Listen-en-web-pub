<template>
  <div class="home-page le-page">
    <!-- Hero: notebook paper sheet -->
    <section class="hero le-paper le-paper--ruled le-paper--margin le-paper-flip">
      <div class="hero-corner" aria-hidden="true"></div>
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

        <aside class="hero-app-card le-paper-flip le-paper-flip-delay-1">
          <div class="app-card-top">
            <span class="app-badge">ANDROID</span>
            <span class="app-badge app-badge--version">v0.9.10</span>
          </div>
          <div class="app-title-row">
            <h2 class="app-name">EaseWord</h2>
          </div>
          <p class="app-cn">听易词</p>
          <p class="app-desc">官方单词本 · 与网站同账号</p>
          <a
            class="app-download"
            href="/downloads/EaseWord-0.9.10.apk"
            download="EaseWord-0.9.10.apk"
          >
            下载 APK
          </a>
          <p class="app-note" title="旧签名版本（如 0.8.0）需先上传云端并卸载后再安装；之后可直接覆盖更新。">
            仅 Android · 旧版需先卸载再装
          </p>
        </aside>
      </div>
    </section>

    <!-- 进站祝福弹层：打字后自动消失 -->
    <Teleport to="body">
      <Transition name="welcome-popup">
        <div
          v-if="ENABLE_WELCOME_POPUP && welcomePopupVisible"
          class="welcome-popup"
          role="dialog"
          aria-live="polite"
          @click.self="dismissWelcomePopup"
        >
          <p class="welcome-popup__text">
            <span class="welcome-popup__line">{{ typedWelcome }}</span>
            <span
              v-show="!welcomeTypingDone"
              class="exam-blessing__cursor welcome-popup__cursor"
              aria-hidden="true"
            ></span>
          </p>
        </div>
      </Transition>
    </Teleport>

    <!-- 每日一句 -->
    <div v-if="dailyQuote" class="quote-card le-paper le-paper--ruled le-paper--margin le-paper-flip le-paper-flip-delay-1">
      <div class="quote-header">
        <span class="quote-label">DAILY · 每日一句</span>
        <span class="quote-date">{{ dailyQuote.date }}</span>
      </div>
      <p class="quote-en">{{ dailyQuote.content }}</p>
      <p class="quote-cn">{{ dailyQuote.note }}</p>
    </div>

    <!-- 分类 Tab -->
    <div class="category-tabs-wrap le-paper-flip le-paper-flip-delay-2">
      <div class="category-tabs" role="tablist">
        <button
          v-for="cat in displayCategories"
          :key="cat.code"
          type="button"
          role="tab"
          class="category-tab"
          :class="{ active: activeCategory === cat.code, 'is-coming-soon': cat.comingSoon }"
          @click="handleSelect(cat.code)"
        >
          {{ cat.name?.chinese || cat.name }}
          <span v-if="cat.comingSoon" class="tab-soon-badge">开发中</span>
        </button>
      </div>
    </div>

    <!-- 当前分类标题 + 祝福打字机 -->
    <div class="category-intro le-paper-flip le-paper-flip-delay-2">
      <h2>{{ currentCategory.title }}</h2>
      <div class="category-meta">
        <span class="meta-pill">{{ (activeCategory || 'CET').toUpperCase() }}</span>
        <span>{{ currentCategory.subtitle }}</span>
      </div>
      <p class="exam-blessing" aria-live="polite">
        <span class="exam-blessing__text">{{ typedBlessing || blessingFullText }}</span>
        <span
          v-show="ENABLE_BLESSING_TYPEWRITER && !blessingTypingDone"
          class="exam-blessing__cursor"
          aria-hidden="true"
        ></span>
      </p>
    </div>

    <!-- 试卷列表 / 考研精读入口 -->
    <section class="le-section le-paper-flip le-paper-flip-delay-3">
      <div class="le-section-header">
        <h2>
          <el-icon color="var(--le-accent)"><Document /></el-icon>
          {{ currentCategory.listTitle }}
        </h2>
        <a
          v-if="!isKaoyanCategory"
          class="view-all"
          href="javascript:;"
          @click.prevent="goExamList()"
        >查看全部 →</a>
      </div>

      <div v-if="isKaoyanCategory" class="coming-soon-panel le-paper">
        <p class="coming-soon-title">考研英语 · 开发中（模拟）</p>
        <p class="coming-soon-desc">
          当前试卷与题目仅用于功能模拟练习，非正式历年真题库。可体验完形填空与阅读理解在线做题。
        </p>
        <button type="button" class="kaoyan-cta" @click="goKaoyan">
          进入模拟练习
        </button>
      </div>

      <template v-else>
        <div v-if="albumsLoading" class="le-loading-wrap">
          <el-icon class="is-loading" :size="28"><Loading /></el-icon>
          <span>加载试卷中...</span>
        </div>

        <el-row v-else :gutter="16">
          <el-col v-for="(item, index) in currentList" :key="item.id" :xs="12" :sm="8" :md="6">
            <div
              class="exam-card le-paper-flip"
              :class="`le-paper-flip-delay-${Math.min(index % 4 + 1, 5)}`"
              @click="goAlbum(item.id)"
            >
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
      </template>
    </section>

    <!-- 快捷入口 -->
    <section class="le-section le-paper-flip le-paper-flip-delay-4">
      <div class="le-section-header">
        <h2><el-icon color="var(--le-accent)"><Star /></el-icon> 快捷入口</h2>
      </div>
      <el-row :gutter="16">
        <el-col v-for="(item, index) in quickLinks" :key="item.title" :xs="12" :sm="6">
          <div
            class="quick-card le-paper-flip"
            :class="`le-paper-flip-delay-${Math.min(index + 1, 5)}`"
            @click="item.action?.()"
          >
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
import { ref, computed, onMounted, onUnmounted, watch, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { getCategories, getAlbumsByCategoryId } from '../api/Listen.js'
import { queryEnglishWord, addUserWordToCurrentBook, getDailyEnglish, isValidEnglishQuery } from '../api/Word.js'
import { promptGoReviewAfterAdd } from '../utils/promptGoReview.js'
import { homeCategoryMeta, mergeHomeCategoriesWithExtras } from '../utils/categoryMeta.js'

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
const categoryMeta = homeCategoryMeta

const displayCategories = computed(() => mergeHomeCategoriesWithExtras(categories.value))

const activeDisplayCategory = computed(() =>
  displayCategories.value.find((c) => c.code === activeCategory.value)
)

const isKaoyanCategory = computed(() => activeDisplayCategory.value?.kind === 'kaoyan')

const currentCategory = computed(() => {
  return categoryMeta[activeCategory.value] || { title: '英语听力练习', subtitle: '选择分类开始练习', listTitle: '听力真题', color: '#409eff' }
})

const currentList = computed(() => albumList.value.slice(0, 8))

const DEFAULT_BLESSING = '今天也认真学一点，英语会记住你的努力。'
const WELCOME_TEXT = '欢迎回来。今天也要轻轻推自己一把，英语会慢慢回应你。'
const WELCOME_HOLD_MS = 1600
const WELCOME_STORAGE_KEY = 'le_welcome_popup_date'
/** 进站祝福弹层（打字机）。需要时改成 true。 */
const ENABLE_WELCOME_POPUP = false
/** 分类祝福文案始终展示；true=打字机，false=直接全文。 */
const ENABLE_BLESSING_TYPEWRITER = false

const todayKey = () => {
  const d = new Date()
  const pad = (n) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`
}

const hasShownWelcomeToday = () => {
  try {
    return localStorage.getItem(WELCOME_STORAGE_KEY) === todayKey()
  } catch {
    return false
  }
}

const markWelcomeShownToday = () => {
  try {
    localStorage.setItem(WELCOME_STORAGE_KEY, todayKey())
  } catch {
    /* ignore quota / private mode */
  }
}

const typedBlessing = ref('')
const blessingTypingDone = ref(false)
let blessingTimer = null

const typedWelcome = ref('')
const welcomeTypingDone = ref(false)
const welcomePopupVisible = ref(false)
let welcomeTimer = null
let welcomeDismissTimer = null

const blessingFullText = computed(() => {
  return currentCategory.value.blessing || DEFAULT_BLESSING
})

const startTypewriter = (text, onTick, onDone, getTimer, setTimer) => {
  const prev = getTimer()
  if (prev) clearInterval(prev)
  onTick('')
  onDone(false)
  if (!text) {
    setTimer(null)
    return
  }
  let i = 0
  const timer = setInterval(() => {
    onTick(text.slice(0, i + 1))
    i += 1
    if (i >= text.length) {
      clearInterval(timer)
      setTimer(null)
      onDone(true)
    }
  }, 42)
  setTimer(timer)
}

const dismissWelcomePopup = () => {
  if (welcomeDismissTimer) {
    clearTimeout(welcomeDismissTimer)
    welcomeDismissTimer = null
  }
  if (welcomeTimer) {
    clearInterval(welcomeTimer)
    welcomeTimer = null
  }
  welcomePopupVisible.value = false
  welcomeTypingDone.value = true
}

const runBlessingTypewriter = (text) => {
  if (!ENABLE_BLESSING_TYPEWRITER) {
    if (blessingTimer) {
      clearInterval(blessingTimer)
      blessingTimer = null
    }
    typedBlessing.value = text || ''
    blessingTypingDone.value = true
    return
  }
  startTypewriter(
    text,
    (v) => { typedBlessing.value = v },
    (v) => { blessingTypingDone.value = v },
    () => blessingTimer,
    (t) => { blessingTimer = t }
  )
}

const runWelcomeTypewriter = (text) => {
  if (!ENABLE_WELCOME_POPUP) return
  markWelcomeShownToday()
  welcomePopupVisible.value = true
  startTypewriter(
    text,
    (v) => { typedWelcome.value = v },
    (v) => {
      welcomeTypingDone.value = v
      if (v) {
        welcomeDismissTimer = setTimeout(() => {
          welcomePopupVisible.value = false
          welcomeDismissTimer = null
        }, WELCOME_HOLD_MS)
      }
    },
    () => welcomeTimer,
    (t) => { welcomeTimer = t }
  )
}

watch(
  blessingFullText,
  (text) => {
    runBlessingTypewriter(text)
  },
  { immediate: true }
)

onUnmounted(() => {
  if (blessingTimer) clearInterval(blessingTimer)
  if (welcomeTimer) clearInterval(welcomeTimer)
  if (welcomeDismissTimer) clearTimeout(welcomeDismissTimer)
})

// 加载分类数据
const loadCategories = async (retryCount = 0) => {
  try {
    const data = await getCategories()
    categories.value = data || []
    if (categories.value.length > 0) {
      activeCategory.value = categories.value[0].code
    } else if (displayCategories.value.length > 0) {
      activeCategory.value = displayCategories.value[0].code
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
  if (!activeCategory.value || isKaoyanCategory.value) {
    albumList.value = []
    return
  }
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
  if (!newVal) return
  if (isKaoyanCategory.value) {
    albumList.value = []
    albumsLoading.value = false
    return
  }
  if (categories.value.length > 0) {
    loadAlbums()
  }
})

onMounted(async () => {
  if (!hasShownWelcomeToday()) {
    runWelcomeTypewriter(WELCOME_TEXT)
  }
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

const goBBCNews = () => {
  router.push({ name: 'bbcNews' })
}

const goExamList = () => {
  if (isKaoyanCategory.value) {
    goKaoyan()
    return
  }
  const cat = categories.value.find(c => c.code === activeCategory.value)
  if (cat) {
    router.push({ name: 'exams', query: { categoryId: cat.id } })
  }
}

const goKaoyan = () => {
  router.push(activeDisplayCategory.value?.entryRoute || { name: 'kaoyan' })
}

const quickLinks = [
  { title: '每日短文', desc: '10 分钟保持语感', icon: 'Microphone', action: goDailyArticle },
  { title: '官方词本', desc: '四六级 / 考研高频', icon: 'Notebook', action: () => router.push({ name: 'wordPacks' }) },
  { title: '词根学习', desc: '系统扩展词汇', icon: 'Collection', action: () => router.push({ name: 'wordRoots' }) },
  { title: '单词复习', desc: '智能间隔复习', icon: 'Reading', action: () => router.push('/word-review') },
]
</script>

<style scoped>
.home-page {
  padding-top: 8px;
}

.hero {
  position: relative;
  padding: 32px 28px 36px 52px;
  margin-bottom: 16px;
  overflow: hidden;
}

.hero-corner {
  position: absolute;
  top: 0;
  right: 0;
  width: 36px;
  height: 36px;
  background:
    linear-gradient(225deg, transparent 48%, rgba(55, 75, 105, 0.08) 50%, rgba(55, 75, 105, 0.06) 100%),
    linear-gradient(225deg, transparent 50%, rgba(232, 238, 245, 0.95) 50%);
  pointer-events: none;
  z-index: 2;
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
  padding: 3px 0;
  background: transparent;
  border: none;
  border-radius: 0;
  color: var(--le-ink-rule);
  font-family: var(--le-font-mono);
  font-size: 12px;
  font-weight: 500;
  letter-spacing: 0.06em;
}

.hero-title {
  font-family: var(--le-font-display);
  font-size: clamp(1.75rem, 3.5vw, 2.35rem);
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

.welcome-popup {
  position: fixed;
  inset: 0;
  z-index: 3000;
  display: flex;
  align-items: flex-start;
  justify-content: center;
  padding: 18vh 20px 24px;
  background: rgba(26, 36, 51, 0.22);
  backdrop-filter: blur(1.5px);
  -webkit-backdrop-filter: blur(1.5px);
  cursor: pointer;
}

.welcome-popup__text {
  margin: 0;
  max-width: min(960px, 100%);
  font-family: var(--le-font-hand);
  font-size: clamp(1.65rem, 4.4vw, 2.45rem);
  line-height: 1.35;
  letter-spacing: 0.03em;
  color: #ffffff;
  opacity: 1;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 2px;
  text-align: center;
  cursor: default;
  text-shadow:
    0 1px 2px rgba(0, 0, 0, 0.35),
    0 2px 12px rgba(0, 0, 0, 0.22);
}

.welcome-popup__line {
  white-space: nowrap;
}

.welcome-popup__cursor {
  background: #ffffff !important;
  height: 1.15em;
  margin-bottom: 0;
  flex-shrink: 0;
}

.welcome-popup-enter-active,
.welcome-popup-leave-active {
  transition: opacity 0.35s ease;
}

.welcome-popup-enter-active .welcome-popup__text,
.welcome-popup-leave-active .welcome-popup__text {
  transition: transform 0.35s ease, opacity 0.35s ease;
}

.welcome-popup-enter-from,
.welcome-popup-leave-to {
  opacity: 0;
}

.welcome-popup-enter-from .welcome-popup__text,
.welcome-popup-leave-to .welcome-popup__text {
  opacity: 0;
  transform: translateY(-8px);
}

@media (max-width: 768px) {
  .welcome-popup {
    padding-top: 16vh;
  }

  .welcome-popup__line {
    white-space: normal;
  }

  .welcome-popup__text {
    font-size: clamp(1.4rem, 5.8vw, 1.85rem);
    line-height: 1.45;
  }
}

.hero-search :deep(.el-input__wrapper) {
  border-radius: 8px 0 0 8px;
  background: #fff !important;
  box-shadow: 0 0 0 1px var(--le-border) inset !important;
  padding-left: 8px;
}

.hero-search :deep(.el-input-group__append) {
  border-radius: 0 8px 8px 0;
  overflow: hidden;
  box-shadow: none;
  background: transparent;
  padding: 0;
}

.hero-search :deep(.search-btn) {
  height: 42px;
  width: 48px;
  margin: 3px;
  border-radius: 6px !important;
  background: var(--le-primary) !important;
  border: none !important;
  color: #fff !important;
}

.hero-search :deep(.el-input__wrapper.is-focus) {
  box-shadow: 0 0 0 1px var(--le-primary) inset !important;
}

.hero-app-card {
  justify-self: stretch;
  width: 100%;
  max-width: none;
  background: rgba(255, 255, 255, 0.94);
  background-image: var(--le-paper-grain);
  border: 1px solid var(--le-border);
  border-radius: 4px 8px 8px 4px;
  padding: 18px;
  box-shadow: var(--le-shadow-sm);
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
  color: var(--le-primary);
  background: rgba(37, 99, 235, 0.08);
  border: 1px solid rgba(37, 99, 235, 0.18);
  padding: 3px 8px;
  border-radius: 4px;
  letter-spacing: 0.04em;
}

.app-badge--version {
  color: var(--le-text-muted);
  background: var(--le-bg-muted);
  border-color: var(--le-border);
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
  color: #fff;
  font-weight: 700;
  font-size: 14px;
  padding: 10px;
  border-radius: 8px;
  transition: opacity 0.15s ease, background 0.15s ease;
}

.app-download:hover {
  opacity: 0.92;
  background: var(--le-primary-light);
  color: #fff;
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

.category-tab.is-coming-soon {
  display: inline-flex;
  align-items: center;
  gap: 6px;
}

.tab-soon-badge {
  font-size: 10px;
  font-weight: 600;
  line-height: 1;
  padding: 3px 5px;
  border-radius: 4px;
  color: var(--le-warning);
  background: rgba(217, 119, 6, 0.1);
  border: 1px solid rgba(217, 119, 6, 0.2);
}

.coming-soon-panel {
  padding: 36px 24px;
  text-align: center;
  margin-bottom: 8px;
}

.coming-soon-title {
  margin: 0 0 8px;
  font-size: 16px;
  font-weight: 600;
  color: var(--le-text);
}

.coming-soon-desc {
  margin: 0 0 18px;
  font-size: 14px;
  line-height: 1.6;
  color: var(--le-text-muted);
}

.kaoyan-cta {
  border: none;
  background: var(--le-primary);
  color: #fff;
  font-size: 14px;
  font-weight: 600;
  font-family: inherit;
  padding: 10px 22px;
  border-radius: 8px;
  cursor: pointer;
  transition: opacity 0.2s;
}

.kaoyan-cta:hover {
  opacity: 0.9;
}

.quote-card {
  position: relative;
  overflow: hidden;
  padding: 22px 24px 22px 52px;
  margin-bottom: 24px;
}

.quote-card:hover {
  box-shadow: var(--le-shadow);
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
  color: var(--le-primary);
  letter-spacing: 0.06em;
  background: transparent;
  border: none;
  padding: 0;
  border-radius: 0;
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

.exam-blessing {
  margin: 12px 0 0;
  min-height: 1.6em;
  font-size: 14px;
  line-height: 1.6;
  color: var(--le-text-muted);
  display: flex;
  align-items: flex-end;
  gap: 1px;
}

.exam-blessing__text {
  white-space: pre-wrap;
}

.exam-blessing__cursor {
  display: inline-block;
  width: 1.5px;
  height: 1em;
  margin-bottom: 2px;
  background: var(--le-primary);
  flex-shrink: 0;
  opacity: 1;
}

.meta-pill {
  font-family: var(--le-font-mono);
  font-size: 11px;
  color: var(--le-primary);
  background: rgba(37, 99, 235, 0.08);
  border: 1px solid rgba(37, 99, 235, 0.16);
  padding: 3px 8px;
  border-radius: 4px;
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
  color: var(--le-accent);
}

.exam-card {
  padding: 18px;
  margin-bottom: 16px;
  height: calc(100% - 16px);
  background-color: rgba(255, 255, 255, 0.92);
  background-image: var(--le-paper-grain);
  border: 1px solid var(--le-border);
  border-radius: 4px 10px 10px 4px;
  box-shadow: var(--le-shadow-sm);
  cursor: pointer;
  transition: transform 0.2s ease, border-color 0.2s ease, box-shadow 0.2s ease;
  transform-origin: left center;
}

.exam-card:hover {
  transform: translateY(-2px) rotateY(-2deg);
  border-color: var(--le-border-strong);
  box-shadow: var(--le-shadow);
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
  border-radius: 4px;
  font-size: 11px;
  font-weight: 600;
  letter-spacing: 0.04em;
  background: rgba(37, 99, 235, 0.08);
  color: var(--le-primary);
  border: 1px solid rgba(37, 99, 235, 0.16);
}

.exam-tag.cet6 {
  background: rgba(91, 106, 191, 0.08);
  color: var(--le-purple);
  border-color: rgba(91, 106, 191, 0.2);
}

.exam-tag.ielts {
  background: rgba(5, 150, 105, 0.08);
  color: var(--le-success);
  border-color: rgba(5, 150, 105, 0.2);
}

.exam-tag.toefl {
  background: rgba(217, 119, 6, 0.08);
  color: var(--le-warning);
  border-color: rgba(217, 119, 6, 0.2);
}

.exam-tag.kaoyan {
  background: rgba(239, 68, 68, 0.08);
  color: #dc2626;
  border-color: rgba(239, 68, 68, 0.2);
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
  border-top: 1px dashed var(--le-border);
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
  background: rgba(37, 99, 235, 0.08);
  border: 1px solid rgba(37, 99, 235, 0.16);
  color: var(--le-primary);
  border-radius: 6px;
  font-size: 12px;
  font-weight: 500;
  transition: all 0.2s ease;
}

.exam-card:hover .start-btn {
  background: var(--le-primary);
  color: #fff;
  border-color: transparent;
}

.quick-card {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 18px;
  margin-bottom: 16px;
  height: calc(100% - 16px);
  background-color: rgba(255, 255, 255, 0.92);
  background-image: var(--le-paper-grain);
  border: 1px solid var(--le-border);
  border-radius: 4px 10px 10px 4px;
  box-shadow: var(--le-shadow-sm);
  cursor: pointer;
  transition: transform 0.2s ease, border-color 0.2s ease, box-shadow 0.2s ease;
  text-align: left;
  transform-origin: left center;
}

.quick-card:hover {
  transform: translateY(-2px) rotateY(-2deg);
  border-color: var(--le-border-strong);
  box-shadow: var(--le-shadow);
}

.quick-icon {
  width: 44px;
  height: 44px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  background: rgba(37, 99, 235, 0.1);
  color: var(--le-primary);
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
  color: #fff;
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
    padding: 24px 16px 28px 44px;
  }
  .hero-title {
    font-size: 1.65rem;
  }
  .quote-card {
    padding: 18px 16px 18px 44px;
  }
  .category-intro h2 {
    font-size: 20px;
  }
  .exam-meta-text {
    display: none;
  }
}
</style>
