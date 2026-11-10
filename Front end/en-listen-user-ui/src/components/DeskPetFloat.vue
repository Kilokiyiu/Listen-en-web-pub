<template>
  <div class="desk-pet" :style="floatStyle" @mousedown="startDrag" @touchstart="startDrag">
    <!-- 气泡：鼓励 / 反馈邀请 -->
    <Transition name="pet-bubble">
      <div
        v-if="bubble"
        class="pet-bubble"
        :class="`pet-bubble--${bubble.kind}`"
        @mousedown.stop
        @touchstart.stop
      >
        <button type="button" class="pet-bubble-close" aria-label="关闭" @click.stop="dismissBubble">×</button>
        <template v-if="bubble.kind === 'gift' || bubble.kind === 'feedback'">
          <p class="pet-bubble-title">学完啦～</p>
          <p class="pet-bubble-text">
            <template v-if="bubble.kind === 'gift'">
              {{ bubble.context ? `「${bubble.context}」` : '这一轮' }}写几句反馈，可领学习资料
            </template>
            <template v-else>
              {{ bubble.context ? `「${bubble.context}」` : '这一轮' }}完成了，来写一句反馈吧
            </template>
          </p>
          <button type="button" class="pet-bubble-cta" @click.stop="openFeedbackFromBubble">
            {{ bubble.kind === 'gift' ? '去反馈领资料' : '去反馈' }}
          </button>
        </template>
        <template v-else>
          <button type="button" class="pet-bubble-cheer" @click.stop="onCheerClick">
            {{ bubble.text }}
          </button>
        </template>
      </div>
    </Transition>

    <!-- 点击菜单：查词 / 反馈 -->
    <Transition name="pet-menu">
      <div v-if="menuOpen" class="pet-menu" @mousedown.stop @touchstart.stop>
        <button type="button" class="pet-menu-item" @click.stop="chooseSearch">
          查不会的单词
        </button>
        <button type="button" class="pet-menu-item pet-menu-item--accent" @click.stop="chooseFeedback">
          反馈领资料
        </button>
      </div>
    </Transition>

    <button
      type="button"
      class="pet-btn"
      :class="{
        attention: bubble?.kind === 'gift' || bubble?.kind === 'feedback',
        bounce: petBounce,
      }"
      aria-label="桌宠小猫：点我查词或反馈"
      @click.stop="onPetClick"
    >
      <svg
        class="pixel-cat"
        viewBox="0 0 40 40"
        width="48"
        height="48"
        shape-rendering="crispEdges"
        aria-hidden="true"
      >
        <!-- outline ears -->
        <rect x="8" y="4" width="6" height="2" fill="#5b3a29" />
        <rect x="26" y="4" width="6" height="2" fill="#5b3a29" />
        <rect x="8" y="6" width="2" height="6" fill="#5b3a29" />
        <rect x="12" y="6" width="2" height="4" fill="#5b3a29" />
        <rect x="26" y="6" width="2" height="4" fill="#5b3a29" />
        <rect x="30" y="6" width="2" height="6" fill="#5b3a29" />
        <!-- ear fill + pink -->
        <rect x="10" y="6" width="2" height="4" fill="#f6c57a" />
        <rect x="28" y="6" width="2" height="4" fill="#f6c57a" />
        <rect x="10" y="8" width="2" height="2" fill="#ffb3c6" />
        <rect x="28" y="8" width="2" height="2" fill="#ffb3c6" />
        <!-- head -->
        <rect x="10" y="10" width="20" height="14" fill="#ffd089" />
        <rect x="8" y="12" width="2" height="10" fill="#f6c57a" />
        <rect x="30" y="12" width="2" height="10" fill="#f6c57a" />
        <rect x="12" y="24" width="16" height="2" fill="#ffd089" />
        <!-- cheeks blush -->
        <rect x="10" y="18" width="4" height="2" fill="#ffb3c6" opacity="0.85" />
        <rect x="26" y="18" width="4" height="2" fill="#ffb3c6" opacity="0.85" />
        <!-- big sparkly eyes -->
        <rect x="13" y="14" width="5" height="5" fill="#3d2b1f" />
        <rect x="22" y="14" width="5" height="5" fill="#3d2b1f" />
        <rect x="14" y="15" width="2" height="2" fill="#fff" />
        <rect x="23" y="15" width="2" height="2" fill="#fff" />
        <rect x="16" y="17" width="1" height="1" fill="#fff" opacity="0.7" />
        <rect x="25" y="17" width="1" height="1" fill="#fff" opacity="0.7" />
        <!-- nose -->
        <rect x="19" y="19" width="2" height="2" fill="#ff8fab" />
        <!-- smile -->
        <rect x="17" y="21" width="1" height="1" fill="#5b3a29" />
        <rect x="18" y="22" width="4" height="1" fill="#5b3a29" />
        <rect x="22" y="21" width="1" height="1" fill="#5b3a29" />
        <!-- body / belly -->
        <rect x="12" y="26" width="16" height="8" fill="#ffd089" />
        <rect x="14" y="28" width="12" height="4" fill="#fff1d0" />
        <!-- paws -->
        <rect x="12" y="34" width="5" height="3" fill="#f6c57a" />
        <rect x="23" y="34" width="5" height="3" fill="#f6c57a" />
        <rect x="13" y="35" width="1" height="1" fill="#ffb3c6" />
        <rect x="15" y="35" width="1" height="1" fill="#ffb3c6" />
        <rect x="24" y="35" width="1" height="1" fill="#ffb3c6" />
        <rect x="26" y="35" width="1" height="1" fill="#ffb3c6" />
        <!-- curly tail -->
        <rect x="28" y="28" width="4" height="2" fill="#f6c57a" />
        <rect x="30" y="26" width="2" height="2" fill="#ffd089" />
        <rect x="32" y="24" width="2" height="2" fill="#f6c57a" />
        <!-- tiny bow -->
        <rect x="18" y="11" width="4" height="2" fill="#7aa7ff" />
        <rect x="17" y="10" width="2" height="2" fill="#5b8def" />
        <rect x="21" y="10" width="2" height="2" fill="#5b8def" />
      </svg>
      <span
        v-if="bubble?.kind === 'gift' || bubble?.kind === 'feedback'"
        class="pet-dot"
        aria-hidden="true"
      />
    </button>

    <!-- 查词弹窗（原功能） -->
    <el-dialog
      v-model="dialogVisible"
      title="单词查询"
      width="90%"
      :max-width="700"
      class="word-search-dialog"
      :style="{ maxWidth: '700px' }"
      destroy-on-close
      @mousedown.stop
      @touchstart.stop
    >
      <div class="search-input-wrapper">
        <el-input
          v-model="searchWord"
          placeholder="输入英语单词、短语或句子，查询释义与例句..."
          size="large"
          class="search-input"
          @keyup.enter="doSearch"
          ref="searchInputRef"
        >
          <template #append>
            <el-button type="primary" :loading="searchLoading" @click="doSearch">
              <el-icon><Search /></el-icon>
            </el-button>
          </template>
        </el-input>
      </div>

      <div v-if="wordDetail" class="word-detail-content">
        <div class="phonetic-section">
          <div v-if="wordDetail.ukphone" class="phonetic-item">
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
          <div v-if="wordDetail.usphone" class="phonetic-item">
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

        <div v-if="wordDetail.translations?.length" class="detail-section">
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

        <div v-if="wordDetail.sentences?.length" class="detail-section">
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

        <div v-if="wordDetail.phrases?.length" class="detail-section">
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

        <div v-if="wordDetail.relWords?.length" class="detail-section">
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

        <div v-if="wordDetail.synonyms?.length" class="detail-section">
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
            >{{ w.word }}</span>
          </div>
        </div>
      </div>

      <div v-else-if="searched && !searchLoading" class="empty-state">
        <el-icon :size="48" color="#c0c4cc"><Search /></el-icon>
        <p>输入单词开始查询</p>
      </div>

      <template #footer>
        <el-button @click="dialogVisible = false">关闭</el-button>
        <el-button
          type="primary"
          :loading="addingWord"
          :disabled="!isLoggedIn || !wordDetail"
          @click="addToWordBook"
        >
          <el-icon><Plus /></el-icon>
          {{ isLoggedIn ? '加入单词本' : '请先登录' }}
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import {
  Search, VideoPlay, Collection, Document, Link, Connection, Share, Plus,
} from '@element-plus/icons-vue'
import { queryEnglishWord, addUserWordToCurrentBook, isValidEnglishQuery } from '../api/Word.js'
import { promptGoReviewAfterAdd } from '../utils/promptGoReview.js'
import {
  registerFeedbackGiftBubble,
  openFeedbackGiftDialog,
} from '../utils/promptFeedbackGift.js'

const router = useRouter()
const POS_KEY = 'desk_pet_float_pos'
const OLD_POS_KEY = 'word_search_float_pos'

const pos = ref({ x: 0, y: 200 })
const isDragging = ref(false)
const isRealDragging = ref(false)
const dragOffset = ref({ x: 0, y: 0 })
const dragStartPos = ref({ x: 0, y: 0 })
const petBounce = ref(false)
const menuOpen = ref(false)

/** @type {import('vue').Ref<null | { kind: 'gift' | 'feedback', source: string, context: string } | { kind: 'cheer', text: string }>} */
const bubble = ref(null)
let cheerTimer = null
let cheerHideTimer = null

const CHEERS = [
  '今天也在进步哦～',
  '听完再巩固一下，超棒！',
  '慢慢来，你已经很厉害了',
  '喵～休息一下眼睛吧',
  '不会的词点我查一查',
  '查不会的单词，点我就行',
  '生词卡住了？点我帮你查',
  '遇到生词别慌，点我查词',
  '坚持比速度更重要',
  '学累了就摸摸我（点点我）',
  '每听一分钟都算数！',
  '加油，下一题更顺手',
  '记住：听懂大意也很棒',
  '听不懂的词，马上查一查',
]

const dialogVisible = ref(false)
const searchWord = ref('')
const searchLoading = ref(false)
const wordDetail = ref(null)
const searched = ref(false)
const addingWord = ref(false)
const searchInputRef = ref(null)
const isLoggedIn = computed(() => !!localStorage.getItem('token'))

const loadPosition = () => {
  const raw = localStorage.getItem(POS_KEY) || localStorage.getItem(OLD_POS_KEY)
  if (raw) {
    try {
      pos.value = JSON.parse(raw)
      return
    } catch {
      /* fall through */
    }
  }
  pos.value = {
    x: Math.max(12, window.innerWidth - 72),
    y: Math.max(12, window.innerHeight - 150),
  }
}

const savePosition = () => {
  localStorage.setItem(POS_KEY, JSON.stringify(pos.value))
}

const floatStyle = computed(() => ({
  left: `${pos.value.x}px`,
  top: `${pos.value.y}px`,
  cursor: isDragging.value ? (isRealDragging.value ? 'grabbing' : 'default') : 'grab',
  zIndex: dialogVisible.value ? 3000 : 9999,
}))

const startDrag = (e) => {
  if (dialogVisible.value) return
  e.stopPropagation()
  isDragging.value = true
  const clientX = e.touches ? e.touches[0].clientX : e.clientX
  const clientY = e.touches ? e.touches[0].clientY : e.clientY
  dragOffset.value = { x: clientX - pos.value.x, y: clientY - pos.value.y }
  dragStartPos.value = { x: clientX, y: clientY }
}

const onDrag = (e) => {
  if (!isDragging.value || dialogVisible.value) return
  e.preventDefault()
  const clientX = e.touches ? e.touches[0].clientX : e.clientX
  const clientY = e.touches ? e.touches[0].clientY : e.clientY
  if (Math.abs(clientX - dragStartPos.value.x) < 5 && Math.abs(clientY - dragStartPos.value.y) < 5) {
    return
  }
  isRealDragging.value = true
  const size = 56
  pos.value = {
    x: Math.max(0, Math.min(clientX - dragOffset.value.x, window.innerWidth - size)),
    y: Math.max(0, Math.min(clientY - dragOffset.value.y, window.innerHeight - size)),
  }
}

const endDrag = () => {
  if (!isDragging.value) return
  isDragging.value = false
  setTimeout(() => { isRealDragging.value = false }, 10)
  savePosition()
}

const bounceOnce = () => {
  petBounce.value = true
  setTimeout(() => { petBounce.value = false }, 900)
}

const showGiftBubble = (payload = {}) => {
  clearTimeout(cheerHideTimer)
  menuOpen.value = false
  bubble.value = {
    kind: payload.claimed ? 'feedback' : 'gift',
    source: payload.source || '',
    context: payload.context || '',
  }
  bounceOnce()
}

const showCheerBubble = () => {
  // 练习后的反馈气泡优先，不打断
  if (
    bubble.value?.kind === 'gift'
    || bubble.value?.kind === 'feedback'
    || menuOpen.value
    || dialogVisible.value
  ) return
  const text = CHEERS[Math.floor(Math.random() * CHEERS.length)]
  bubble.value = { kind: 'cheer', text }
  bounceOnce()
  clearTimeout(cheerHideTimer)
  cheerHideTimer = setTimeout(() => {
    if (bubble.value?.kind === 'cheer') bubble.value = null
  }, 5200)
}

const scheduleCheer = () => {
  clearTimeout(cheerTimer)
  // 18s～40s 更勤快地冒泡：鼓励 + 提醒查词
  const wait = 18000 + Math.random() * 22000
  cheerTimer = setTimeout(() => {
    showCheerBubble()
    scheduleCheer()
  }, wait)
}

const dismissBubble = () => {
  bubble.value = null
  clearTimeout(cheerHideTimer)
}

const onCheerClick = () => {
  const text = bubble.value?.kind === 'cheer' ? bubble.value.text : ''
  dismissBubble()
  // 提到查词的鼓励语，点气泡直接打开查词
  if (/查/.test(text)) {
    openSearch()
    return
  }
  menuOpen.value = true
}

const openFeedbackFromBubble = () => {
  const ctx = (bubble.value?.kind === 'gift' || bubble.value?.kind === 'feedback')
    ? bubble.value
    : null
  bubble.value = null
  menuOpen.value = false
  openFeedbackGiftDialog({
    source: ctx?.source || 'desk-pet',
    context: ctx?.context || '',
  })
}

const openSearch = () => {
  menuOpen.value = false
  dialogVisible.value = true
  searchWord.value = ''
  wordDetail.value = null
  searched.value = false
  nextTick(() => searchInputRef.value?.focus?.())
}

const chooseSearch = () => openSearch()

const chooseFeedback = () => {
  menuOpen.value = false
  if (bubble.value?.kind === 'gift' || bubble.value?.kind === 'feedback') {
    openFeedbackFromBubble()
    return
  }
  bubble.value = null
  openFeedbackGiftDialog({ source: 'desk-pet-menu', context: '' })
}

const onPetClick = () => {
  if (isRealDragging.value) return
  // 点猫打开菜单：查词 / 反馈
  if (menuOpen.value) {
    menuOpen.value = false
    return
  }
  if (bubble.value?.kind === 'cheer') bubble.value = null
  menuOpen.value = true
}

const onDocPointer = (e) => {
  if (!menuOpen.value) return
  const root = e.target?.closest?.('.desk-pet')
  if (!root) menuOpen.value = false
}

const doSearch = async () => {
  const word = searchWord.value.trim()
  if (!word) return
  if (!isValidEnglishQuery(word)) {
    ElMessage.warning('请输入有效的英语单词、短语或句子')
    return
  }
  searchLoading.value = true
  searched.value = true
  try {
    const res = await queryEnglishWord(word)
    if (res.code === 200 && res.data) {
      wordDetail.value = res.data
    } else {
      wordDetail.value = null
      ElMessage.warning('未找到相关释义')
    }
  } catch {
    wordDetail.value = null
    ElMessage.error('查询失败，请稍后重试')
  } finally {
    searchLoading.value = false
  }
}

const playAudio = (url) => {
  new Audio(url).play().catch(() => ElMessage.warning('音频播放失败'))
}

const addToWordBook = async () => {
  if (!isLoggedIn.value) {
    ElMessage.warning('请先登录')
    dialogVisible.value = false
    router.push({ name: 'login' })
    return
  }
  const word = wordDetail.value.word
  const definition = wordDetail.value.translations
    ?.map((t) => `${t.pos}. ${t.tran_cn}`)
    .join('; ') || ''
  const example = wordDetail.value.sentences?.[0]
    ? `${wordDetail.value.sentences[0].s_content}\n${wordDetail.value.sentences[0].s_cn}`
    : ''
  addingWord.value = true
  try {
    await addUserWordToCurrentBook({ word, definition, example })
    dialogVisible.value = false
    await promptGoReviewAfterAdd(router, word, '/word-search')
  } catch (e) {
    if (e.response?.status === 409) ElMessage.warning('该单词已在单词本中')
    else ElMessage.error('添加失败，请稍后重试')
  } finally {
    addingWord.value = false
  }
}

const clampPos = () => {
  const size = 56
  pos.value.x = Math.max(0, Math.min(pos.value.x, window.innerWidth - size))
  pos.value.y = Math.max(0, Math.min(pos.value.y, window.innerHeight - size))
}

onMounted(() => {
  loadPosition()
  registerFeedbackGiftBubble(showGiftBubble)
  scheduleCheer()
  // 进站很快先打个招呼
  setTimeout(() => showCheerBubble(), 2500)
  window.addEventListener('mousemove', onDrag)
  window.addEventListener('mouseup', endDrag)
  window.addEventListener('touchmove', onDrag, { passive: false })
  window.addEventListener('touchend', endDrag)
  window.addEventListener('resize', clampPos)
  document.addEventListener('mousedown', onDocPointer)
  document.addEventListener('touchstart', onDocPointer)

  if (process.env.NODE_ENV === 'development') {
    try {
      const params = new URLSearchParams(window.location.search)
      if (params.has('previewFeedbackGift')) {
        localStorage.removeItem('le_feedback_gift_claimed')
        localStorage.removeItem('le_feedback_gift_prompt_date')
        showGiftBubble({ source: 'local-preview', context: '本地预览' })
      }
    } catch {
      /* ignore */
    }
  }
})

onUnmounted(() => {
  registerFeedbackGiftBubble(null)
  clearTimeout(cheerTimer)
  clearTimeout(cheerHideTimer)
  window.removeEventListener('mousemove', onDrag)
  window.removeEventListener('mouseup', endDrag)
  window.removeEventListener('touchmove', onDrag)
  window.removeEventListener('touchend', endDrag)
  window.removeEventListener('resize', clampPos)
  document.removeEventListener('mousedown', onDocPointer)
  document.removeEventListener('touchstart', onDocPointer)
})
</script>

<style scoped>
.desk-pet {
  position: fixed;
  z-index: 9999;
  user-select: none;
}

.pet-btn {
  position: relative;
  width: 60px;
  height: 60px;
  padding: 0;
  border: 2px solid #f0d4a8;
  border-radius: 18px;
  background:
    radial-gradient(circle at 30% 25%, #fff9ef 0%, #ffe8c4 55%, #ffd6a0 100%);
  box-shadow:
    0 2px 0 #e8c48a,
    var(--le-shadow);
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: inherit;
  transition: transform 0.2s ease, border-color 0.2s ease, box-shadow 0.2s ease;
}

.pet-btn:hover {
  transform: scale(1.07) translateY(-1px);
  border-color: #e8b86a;
}

.pet-btn:active {
  transform: scale(0.96);
}

.pet-btn.attention {
  border-color: rgba(37, 99, 235, 0.45);
  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.12), var(--le-shadow);
}

.pet-btn.bounce {
  animation: pet-bounce 0.85s ease;
}

@keyframes pet-bounce {
  0%, 100% { transform: translateY(0); }
  25% { transform: translateY(-10px); }
  50% { transform: translateY(0); }
  70% { transform: translateY(-5px); }
}

.pixel-cat {
  image-rendering: pixelated;
  image-rendering: crisp-edges;
  display: block;
  animation: pet-idle 2.8s ease-in-out infinite;
}

@keyframes pet-idle {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-2px); }
}

.pet-menu {
  position: absolute;
  right: 0;
  bottom: calc(100% + 10px);
  display: flex;
  flex-direction: column;
  gap: 6px;
  min-width: 148px;
  padding: 8px;
  border: 1px solid var(--le-border);
  border-radius: 14px;
  background: rgba(255, 255, 255, 0.97);
  box-shadow: var(--le-shadow);
  z-index: 2;
}

.pet-menu-item {
  display: block;
  width: 100%;
  padding: 9px 12px;
  border: 1px solid var(--le-border);
  border-radius: 10px;
  background: #fff;
  color: var(--le-text);
  font-size: 13px;
  font-weight: 600;
  text-align: left;
  cursor: pointer;
  transition: border-color 0.15s ease, background 0.15s ease, color 0.15s ease;
}

.pet-menu-item:hover {
  border-color: var(--le-border-strong);
  background: var(--le-bg-muted);
  color: var(--le-primary);
}

.pet-menu-item--accent {
  border-color: rgba(37, 99, 235, 0.22);
  background: var(--le-gradient-soft);
  color: var(--le-primary);
}

.pet-menu-enter-active,
.pet-menu-leave-active {
  transition: opacity 0.16s ease, transform 0.16s ease;
}

.pet-menu-enter-from,
.pet-menu-leave-to {
  opacity: 0;
  transform: translateY(6px) scale(0.96);
}

.pet-dot {
  position: absolute;
  top: 4px;
  right: 4px;
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background: #ef4444;
  border: 2px solid #fff;
  animation: pet-dot-pulse 1.4s ease infinite;
}

@keyframes pet-dot-pulse {
  0%, 100% { transform: scale(1); opacity: 1; }
  50% { transform: scale(1.15); opacity: 0.75; }
}

.pet-bubble {
  position: absolute;
  right: 0;
  bottom: calc(100% + 10px);
  width: min(220px, calc(100vw - 24px));
  padding: 12px 12px 10px;
  border: 1px solid var(--le-border);
  border-radius: 14px;
  background: rgba(255, 255, 255, 0.96);
  box-shadow: var(--le-shadow);
  backdrop-filter: blur(8px);
  -webkit-backdrop-filter: blur(8px);
  cursor: default;
}

.pet-bubble::after {
  content: '';
  position: absolute;
  right: 22px;
  bottom: -7px;
  width: 12px;
  height: 12px;
  border-right: 1px solid var(--le-border);
  border-bottom: 1px solid var(--le-border);
  background: #fff;
  transform: rotate(45deg);
}

.pet-bubble-close {
  position: absolute;
  top: 4px;
  right: 6px;
  width: 24px;
  height: 24px;
  border: none;
  background: transparent;
  color: var(--le-text-muted);
  font-size: 18px;
  line-height: 1;
  cursor: pointer;
}

.pet-bubble--cheer {
  padding: 10px 28px 10px 12px;
}

.pet-bubble-title {
  margin: 0 20px 4px 0;
  font-family: var(--le-font-display);
  font-size: 14px;
  font-weight: 700;
  color: var(--le-text);
}

.pet-bubble-text {
  margin: 0 0 10px;
  font-size: 12px;
  line-height: 1.5;
  color: var(--le-text-secondary);
}

.pet-bubble-cheer {
  display: block;
  width: 100%;
  margin: 0;
  padding: 0 4px 0 0;
  border: none;
  background: transparent;
  font-size: 13px;
  font-weight: 600;
  line-height: 1.45;
  color: var(--le-text);
  text-align: left;
  cursor: pointer;
}

.pet-bubble-cheer:hover {
  color: var(--le-primary);
}

.pet-bubble-cta {
  width: 100%;
  padding: 7px 10px;
  border: none;
  border-radius: 999px;
  background: var(--le-gradient);
  color: #fff;
  font-size: 12px;
  font-weight: 600;
  cursor: pointer;
}

.pet-bubble-cta:hover {
  filter: brightness(1.05);
}

.pet-bubble-enter-active,
.pet-bubble-leave-active {
  transition: opacity 0.2s ease, transform 0.2s ease;
}

.pet-bubble-enter-from,
.pet-bubble-leave-to {
  opacity: 0;
  transform: translateY(6px) scale(0.96);
}

.search-input-wrapper {
  margin-bottom: 20px;
}

.search-input :deep(.el-input__wrapper) {
  background: #fff !important;
  box-shadow: 0 0 0 1px var(--border-glass) inset, var(--le-shadow-sm) !important;
  border-radius: 28px;
  padding: 4px 8px 4px 20px;
}

.search-input :deep(.el-input__wrapper.is-focus) {
  box-shadow: 0 0 0 1px var(--le-primary) inset !important;
}

.search-input :deep(.el-input-group__append) {
  background: linear-gradient(135deg, var(--accent-blue) 0%, var(--accent-cyan) 100%);
  border: none;
  border-radius: 0 24px 24px 0;
}

.empty-state {
  text-align: center;
  padding: 40px 20px;
  color: var(--text-muted);
}

.empty-state p {
  margin-top: 12px;
  font-size: 14px;
}

.word-detail-content {
  max-height: calc(80vh - 200px);
  overflow-y: auto;
  padding-right: 8px;
}

.phonetic-section {
  display: flex;
  gap: 24px;
  margin-bottom: 20px;
  padding-bottom: 16px;
  border-bottom: 1px solid var(--le-border);
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
  border-bottom: 1px solid var(--le-border);
}

.translation-list,
.phrase-list {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.sentence-item {
  margin-bottom: 12px;
  padding: 12px;
  background: var(--le-bg-muted);
  border: 1px solid var(--le-border);
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

@media (max-width: 768px) {
  .pet-btn {
    width: 56px;
    height: 56px;
  }

  .pet-bubble {
    width: min(200px, calc(100vw - 20px));
  }

  .word-detail-content {
    max-height: calc(90vh - 180px);
  }

  .phonetic-section {
    flex-direction: column;
    gap: 8px;
  }
}
</style>

<style>
.word-search-dialog :deep(.el-dialog) {
  width: 90% !important;
  max-width: 700px !important;
  margin: 5vh auto !important;
  max-height: 90vh;
  display: flex;
  flex-direction: column;
}

.word-search-dialog :deep(.el-dialog__body) {
  flex: 1;
  overflow: hidden;
  padding: 16px 20px;
}

@media (max-width: 768px) {
  .word-search-dialog :deep(.el-dialog) {
    width: 95% !important;
    max-width: 95% !important;
    margin: 2vh auto !important;
    max-height: 96vh;
  }
}
</style>
