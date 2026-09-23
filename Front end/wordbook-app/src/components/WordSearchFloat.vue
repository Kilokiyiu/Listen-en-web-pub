<template>
  <div
    v-show="!dialogVisible"
    class="word-search-float"
    :style="floatStyle"
    @mousedown="startDrag"
    @touchstart.passive="startDrag"
  >
    <button type="button" class="float-btn" aria-label="单词查询" @click.stop="openSearch">
      <svg viewBox="0 0 24 24" width="22" height="22" fill="none" stroke="currentColor" stroke-width="2.2">
        <circle cx="11" cy="11" r="7" />
        <path d="M20 20l-3.5-3.5" stroke-linecap="round" />
      </svg>
    </button>
  </div>

  <Teleport to="body">
    <div v-if="dialogVisible" class="search-overlay" @click.self="closeSearch">
      <div class="search-panel" role="dialog" aria-label="单词查询">
        <div class="panel-header">
          <h3>单词查询</h3>
          <button type="button" class="icon-close" aria-label="关闭" @click="closeSearch">×</button>
        </div>

        <div class="search-row">
          <input
            ref="searchInputRef"
            v-model="searchWord"
            type="search"
            enterkeyhint="search"
            placeholder="输入英语单词、短语或句子..."
            @keyup.enter="doSearch"
          />
          <button type="button" class="btn-search" :disabled="searchLoading" @click="doSearch">
            {{ searchLoading ? '...' : '查询' }}
          </button>
        </div>

        <div class="panel-body">
          <div v-if="wordDetail" class="word-detail">
            <div class="word-title">{{ wordDetail.word }}</div>

            <div v-if="wordDetail.ukphone || wordDetail.usphone" class="phonetic-section">
              <div v-if="wordDetail.ukphone" class="phonetic-item">
                <span class="phonetic-label">英</span>
                <span class="phonetic-text">/{{ wordDetail.ukphone }}/</span>
                <button
                  v-if="wordDetail.ukspeech"
                  type="button"
                  class="btn-play"
                  @click="playAudio(wordDetail.ukspeech)"
                >
                  ▶
                </button>
              </div>
              <div v-if="wordDetail.usphone" class="phonetic-item">
                <span class="phonetic-label">美</span>
                <span class="phonetic-text">/{{ wordDetail.usphone }}/</span>
                <button
                  v-if="wordDetail.usspeech"
                  type="button"
                  class="btn-play"
                  @click="playAudio(wordDetail.usspeech)"
                >
                  ▶
                </button>
              </div>
            </div>

            <div v-if="wordDetail.translations?.length" class="detail-section">
              <h4>释义</h4>
              <div class="tag-list">
                <span v-for="(t, i) in wordDetail.translations" :key="i" class="tag">
                  {{ t.pos }}. {{ t.tran_cn }}
                </span>
              </div>
            </div>

            <div v-if="wordDetail.sentences?.length" class="detail-section">
              <h4>例句</h4>
              <div v-for="(s, i) in wordDetail.sentences" :key="i" class="sentence-item">
                <p class="sentence-en">{{ s.s_content }}</p>
                <p class="sentence-cn">{{ s.s_cn }}</p>
              </div>
            </div>

            <div v-if="wordDetail.phrases?.length" class="detail-section">
              <h4>短语</h4>
              <div class="tag-list">
                <span
                  v-for="(p, i) in wordDetail.phrases.slice(0, 10)"
                  :key="i"
                  class="tag tag-muted"
                >
                  {{ p.p_content }}
                </span>
              </div>
            </div>

            <div v-if="wordDetail.relWords?.length" class="detail-section">
              <h4>同根词</h4>
              <div v-for="(group, i) in wordDetail.relWords" :key="i" class="group-row">
                <span class="tag tag-warn">{{ group.Pos }}</span>
                <span v-for="(w, j) in group.Hwds" :key="j" class="group-item">
                  {{ w.hwd }}
                  <small>{{ w.tran }}</small>
                </span>
              </div>
            </div>

            <div v-if="wordDetail.synonyms?.length" class="detail-section">
              <h4>近义词</h4>
              <div v-for="(group, i) in wordDetail.synonyms" :key="i" class="group-row">
                <span class="tag tag-ok">{{ group.pos }}</span>
                <span v-for="(w, j) in group.Hwds" :key="j" class="group-item">{{ w.word }}</span>
              </div>
            </div>
          </div>

          <div v-else-if="searched && !searchLoading" class="empty-state">
            <p>未找到相关释义，换个词试试</p>
          </div>

          <div v-else-if="!searchLoading" class="empty-state">
            <p>输入单词开始查询</p>
          </div>
        </div>

        <div class="panel-footer">
          <button type="button" class="btn-ghost" @click="closeSearch">关闭</button>
          <button
            type="button"
            class="btn-primary"
            :disabled="!wordDetail || addingWord"
            @click="addToWordBook"
          >
            {{ addingWord ? '添加中...' : `加入${storageLabel}` }}
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, nextTick, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { Preferences } from '@capacitor/preferences'
import { queryEnglishWord, isValidEnglishQuery } from '../api/word'
import { addWord, getStorageLabel } from '../services/wordService'
import { showToast } from '../utils/toast'
import { promptGoReviewAfterAdd } from '../utils/promptGoReview'

const POS_KEY = 'wordbook_search_float_pos'
const BTN_SIZE = 52

const route = useRoute()
const router = useRouter()const pos = ref({ x: 0, y: 200 })
const isDragging = ref(false)
const isRealDragging = ref(false)
const dragOffset = ref({ x: 0, y: 0 })
const dragStartPos = ref({ x: 0, y: 0 })

const dialogVisible = ref(false)
const searchWord = ref('')
const searchLoading = ref(false)
const wordDetail = ref(null)
const searched = ref(false)
const addingWord = ref(false)
const searchInputRef = ref(null)
const storageLabel = ref('单词本')

const floatStyle = computed(() => ({
  left: `${pos.value.x}px`,
  top: `${pos.value.y}px`,
  cursor: isDragging.value ? (isRealDragging.value ? 'grabbing' : 'default') : 'grab',
}))

const clampPos = (x, y) => {
  const maxX = Math.max(0, window.innerWidth - BTN_SIZE)
  const maxY = Math.max(0, window.innerHeight - BTN_SIZE)
  return {
    x: Math.max(0, Math.min(x, maxX)),
    y: Math.max(0, Math.min(y, maxY)),
  }
}

const loadPosition = async () => {
  const { value } = await Preferences.get({ key: POS_KEY })
  if (value) {
    try {
      const p = JSON.parse(value)
      pos.value = clampPos(Number(p.x) || 0, Number(p.y) || 0)
      return
    } catch {
      /* fall through */
    }
  }
  pos.value = clampPos(window.innerWidth - 72, window.innerHeight - 180)
}

const savePosition = async () => {
  await Preferences.set({ key: POS_KEY, value: JSON.stringify(pos.value) })
}

const refreshLabel = async () => {
  storageLabel.value = await getStorageLabel()
}

const startDrag = (e) => {
  if (dialogVisible.value) return
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
  if (
    Math.abs(clientX - dragStartPos.value.x) < 5 &&
    Math.abs(clientY - dragStartPos.value.y) < 5
  ) {
    return
  }
  isRealDragging.value = true
  pos.value = clampPos(clientX - dragOffset.value.x, clientY - dragOffset.value.y)
}

const endDrag = () => {
  if (!isDragging.value) return
  isDragging.value = false
  setTimeout(() => {
    isRealDragging.value = false
  }, 10)
  savePosition()
}

const openSearch = async () => {
  if (isRealDragging.value) return
  dialogVisible.value = true
  searchWord.value = ''
  wordDetail.value = null
  searched.value = false
  await refreshLabel()
  nextTick(() => searchInputRef.value?.focus?.())
}

const closeSearch = () => {
  dialogVisible.value = false
}

const doSearch = async () => {
  const word = searchWord.value.trim()
  if (!word) return
  if (!isValidEnglishQuery(word)) {
    showToast('请输入有效的英语单词、短语或句子')
    return
  }

  searchLoading.value = true
  searched.value = true
  wordDetail.value = null
  try {
    const res = await queryEnglishWord(word)
    if (res?.code === 200 && res.data) {
      wordDetail.value = res.data
    } else {
      showToast(res?.message || '未找到相关释义')
    }
  } catch (e) {
    showToast(typeof e === 'string' ? e : e?.message || '查询失败，请稍后重试')
  } finally {
    searchLoading.value = false
  }
}

const playAudio = (url) => {
  const audio = new Audio(url)
  audio.play().catch(() => showToast('音频播放失败'))
}

const addToWordBook = async () => {
  if (!wordDetail.value) return
  const word = wordDetail.value.word
  const definition =
    wordDetail.value.translations?.map((t) => `${t.pos}. ${t.tran_cn}`).join('; ') || ''
  const first = wordDetail.value.sentences?.[0]
  const example = first ? `${first.s_content}\n${first.s_cn}` : ''

  addingWord.value = true
  try {
    await addWord({ word, definition, example })
    closeSearch()
    promptGoReviewAfterAdd(router, word)
  } catch (e) {
    const msg = typeof e === 'string' ? e : e?.message || '添加失败'
    showToast(msg.includes('已存在') ? '该单词已在单词本中' : msg)
  } finally {
    addingWord.value = false
  }
}

const onResize = () => {
  pos.value = clampPos(pos.value.x, pos.value.y)
}

watch(
  () => route.name,
  () => {
    if (route.name === 'login') closeSearch()
  }
)

onMounted(async () => {
  await loadPosition()
  await refreshLabel()
  window.addEventListener('mousemove', onDrag)
  window.addEventListener('mouseup', endDrag)
  window.addEventListener('touchmove', onDrag, { passive: false })
  window.addEventListener('touchend', endDrag)
  window.addEventListener('resize', onResize)
})

onUnmounted(() => {
  window.removeEventListener('mousemove', onDrag)
  window.removeEventListener('mouseup', endDrag)
  window.removeEventListener('touchmove', onDrag)
  window.removeEventListener('touchend', endDrag)
  window.removeEventListener('resize', onResize)
})
</script>

<style scoped>
.word-search-float {
  position: fixed;
  z-index: 9999;
  user-select: none;
  touch-action: none;
}

.float-btn {
  width: 52px;
  height: 52px;
  border: none;
  border-radius: 50%;
  background: linear-gradient(135deg, #409eff 0%, #36cfc9 100%);
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 16px rgba(64, 158, 255, 0.4), 0 0 0 4px rgba(64, 158, 255, 0.1);
  position: relative;
  padding: 0;
}

.float-btn::after {
  content: '';
  position: absolute;
  inset: -4px;
  border-radius: 50%;
  border: 2px solid rgba(64, 158, 255, 0.3);
  animation: pulse 2s ease-in-out infinite;
  pointer-events: none;
}

@keyframes pulse {
  0%,
  100% {
    transform: scale(1);
    opacity: 1;
  }
  50% {
    transform: scale(1.15);
    opacity: 0;
  }
}

.search-overlay {
  position: fixed;
  inset: 0;
  z-index: 10000;
  background: rgba(0, 0, 0, 0.45);
  display: flex;
  align-items: flex-end;
  justify-content: center;
  padding: 12px;
  padding-bottom: calc(12px + env(safe-area-inset-bottom, 0));
}

.search-panel {
  width: 100%;
  max-width: 560px;
  max-height: min(88vh, 720px);
  background: #fff;
  border-radius: 16px;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  box-shadow: 0 12px 40px rgba(0, 0, 0, 0.18);
}

.panel-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 14px 16px 8px;
}

.panel-header h3 {
  margin: 0;
  font-size: 17px;
}

.icon-close {
  border: none;
  background: transparent;
  font-size: 28px;
  line-height: 1;
  color: var(--text-muted);
  padding: 0 4px;
}

.search-row {
  display: flex;
  gap: 8px;
  padding: 0 16px 12px;
}

.search-row input {
  flex: 1;
  min-width: 0;
  padding: 11px 14px;
  border: 1px solid var(--border);
  border-radius: 24px;
  font-size: 15px;
  background: #f7f8fa;
}

.btn-search {
  padding: 0 16px;
  border: none;
  border-radius: 24px;
  background: linear-gradient(135deg, #409eff 0%, #36cfc9 100%);
  color: #fff;
  font-size: 14px;
  font-weight: 600;
  white-space: nowrap;
}

.btn-search:disabled {
  opacity: 0.65;
}

.panel-body {
  flex: 1;
  overflow-y: auto;
  padding: 0 16px 12px;
  -webkit-overflow-scrolling: touch;
}

.empty-state {
  text-align: center;
  padding: 40px 12px;
  color: var(--text-muted);
  font-size: 14px;
}

.word-title {
  font-size: 22px;
  font-weight: 700;
  margin-bottom: 10px;
}

.phonetic-section {
  display: flex;
  flex-wrap: wrap;
  gap: 12px 20px;
  margin-bottom: 14px;
  padding-bottom: 12px;
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
  font-size: 15px;
  font-family: 'Times New Roman', serif;
}

.btn-play {
  border: none;
  background: #ecf5ff;
  color: #409eff;
  width: 28px;
  height: 28px;
  border-radius: 50%;
  font-size: 11px;
  padding: 0;
}

.detail-section {
  margin-bottom: 16px;
}

.detail-section h4 {
  margin: 0 0 10px;
  font-size: 14px;
  color: var(--text-primary);
  padding-bottom: 6px;
  border-bottom: 1px solid #f0f0f0;
}

.tag-list {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.tag {
  display: inline-block;
  padding: 4px 10px;
  border-radius: 6px;
  background: #ecf5ff;
  color: #409eff;
  font-size: 13px;
}

.tag-muted {
  background: #f4f4f5;
  color: #606266;
}

.tag-warn {
  background: #fdf6ec;
  color: #e6a23c;
}

.tag-ok {
  background: #f0f9eb;
  color: #67c23a;
}

.sentence-item {
  margin-bottom: 10px;
  padding: 10px 12px;
  background: #f8f9fa;
  border-radius: 8px;
}

.sentence-en {
  margin: 0 0 4px;
  font-size: 14px;
}

.sentence-cn {
  margin: 0;
  font-size: 13px;
  color: var(--text-muted);
}

.group-row {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 8px;
  margin-bottom: 8px;
}

.group-item {
  font-size: 14px;
}

.group-item small {
  margin-left: 4px;
  color: var(--text-muted);
  font-size: 12px;
}

.panel-footer {
  display: flex;
  gap: 10px;
  padding: 12px 16px;
  border-top: 1px solid var(--border);
}

.btn-ghost,
.btn-primary {
  flex: 1;
  padding: 12px;
  border-radius: 10px;
  font-size: 15px;
  font-weight: 600;
}

.btn-ghost {
  background: #fff;
  border: 1px solid var(--border);
  color: var(--text-primary);
}

.btn-primary {
  background: var(--primary);
  border: none;
  color: #fff;
}

.btn-primary:disabled {
  opacity: 0.55;
}
</style>
