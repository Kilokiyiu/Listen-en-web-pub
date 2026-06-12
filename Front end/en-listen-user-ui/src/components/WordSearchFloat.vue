<template>
  <!-- 浮动搜索按钮 -->
  <div
    class="word-search-float"
    :style="floatStyle"
    @mousedown="startDrag"
    @touchstart="startDrag"
  >
    <div class="float-btn" @click.stop="openSearch">
      <el-icon :size="22"><Search /></el-icon>
    </div>

    <!-- 搜索弹窗 -->
    <el-dialog
      v-model="dialogVisible"
      title="单词查询"
      width="90%"
      :max-width="700"
      class="word-search-dialog"
      :style="{ maxWidth: '700px' }"
      destroy-on-close
    >
      <!-- 搜索输入 -->
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
            <el-button type="primary" @click="doSearch" :loading="searchLoading">
              <el-icon><Search /></el-icon>
            </el-button>
          </template>
        </el-input>
      </div>

      <!-- 单词详情 -->
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

      <!-- 空状态 -->
      <div v-else-if="searched && !searchLoading" class="empty-state">
        <el-icon :size="48" color="#c0c4cc"><Search /></el-icon>
        <p>输入单词开始查询</p>
      </div>

      <template #footer>
        <el-button @click="dialogVisible = false">关闭</el-button>
        <el-button
          type="primary"
          @click="addToWordBook"
          :loading="addingWord"
          :disabled="!isLoggedIn || !wordDetail"
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
import { queryEnglishWord, addUserWord, isValidEnglishQuery } from '../api/Word.js'

const router = useRouter()

// ===== 拖拽逻辑 =====
const pos = ref({ x: 0, y: 200 })
const isDragging = ref(false)
const isRealDragging = ref(false)  // 标记是否真正在拖动（而非点击）
const dragOffset = ref({ x: 0, y: 0 })
const dragStartPos = ref({ x: 0, y: 0 })
const dragStartTime = ref(0)

// 从 localStorage 读取位置
const loadPosition = () => {
  const saved = localStorage.getItem('word_search_float_pos')
  if (saved) {
    try {
      const p = JSON.parse(saved)
      pos.value = p
    } catch (e) {}
  } else {
    // 默认位置：右下角
    pos.value = { x: window.innerWidth - 80, y: window.innerHeight - 160 }
  }
}

const savePosition = () => {
  localStorage.setItem('word_search_float_pos', JSON.stringify(pos.value))
}

const floatStyle = computed(() => ({
  left: `${pos.value.x}px`,
  top: `${pos.value.y}px`,
  cursor: isDragging.value ? (isRealDragging.value ? 'grabbing' : 'default') : 'grab',
  // 弹窗打开时提升 z-index，确保浮窗按钮在遮罩层之上
  zIndex: dialogVisible.value ? 3000 : 9999
}))

const startDrag = (e) => {
  // 弹窗打开时禁用拖动
  if (dialogVisible.value) return

  // 阻止事件冒泡，避免触发其他点击事件
  e.stopPropagation()
  
  isDragging.value = true
  const clientX = e.touches ? e.touches[0].clientX : e.clientX
  const clientY = e.touches ? e.touches[0].clientY : e.clientY
  dragOffset.value = {
    x: clientX - pos.value.x,
    y: clientY - pos.value.y
  }
  // 记录按下时的初始位置，用于区分点击和拖动
  dragStartPos.value = { x: clientX, y: clientY }
  dragStartTime.value = Date.now()
}

const onDrag = (e) => {
  // 弹窗打开时禁用拖动
  if (!isDragging.value || dialogVisible.value) return
  e.preventDefault()
  const clientX = e.touches ? e.touches[0].clientX : e.clientX
  const clientY = e.touches ? e.touches[0].clientY : e.clientY

  // 判断是否真正在拖动（移动超过5像素）
  const movedX = Math.abs(clientX - dragStartPos.value.x)
  const movedY = Math.abs(clientY - dragStartPos.value.y)
  
  if (movedX < 5 && movedY < 5) {
    // 移动距离太小，不移动浮窗，等待点击事件
    return
  }
  
  // 标记为真正的拖动，阻止点击事件
  isRealDragging.value = true

  let newX = clientX - dragOffset.value.x
  let newY = clientY - dragOffset.value.y

  // 边界限制
  const btnSize = 56
  newX = Math.max(0, Math.min(newX, window.innerWidth - btnSize))
  newY = Math.max(0, Math.min(newY, window.innerHeight - btnSize))

  pos.value = { x: newX, y: newY }
}

const endDrag = () => {
  if (isDragging.value) {
    isDragging.value = false
    // 延迟重置 isRealDragging，让点击事件能先检测到
    setTimeout(() => {
      isRealDragging.value = false
    }, 10)
    savePosition()
  }
}

// ===== 搜索逻辑 =====
const dialogVisible = ref(false)
const searchWord = ref('')
const searchLoading = ref(false)
const wordDetail = ref(null)
const searched = ref(false)
const addingWord = ref(false)
const searchInputRef = ref(null)
const isLoggedIn = computed(() => !!localStorage.getItem('token'))

const openSearch = () => {
  // 如果是拖动操作，不打开弹窗
  if (isRealDragging.value) return
  
  dialogVisible.value = true
  searchWord.value = ''
  wordDetail.value = null
  searched.value = false
  // 弹窗打开后自动聚焦输入框
  nextTick(() => {
    searchInputRef.value?.focus?.()
  })
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
  } catch (e) {
    wordDetail.value = null
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
    dialogVisible.value = false
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
    dialogVisible.value = false
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

// ===== 生命周期 =====
onMounted(() => {
  loadPosition()
  window.addEventListener('mousemove', onDrag)
  window.addEventListener('mouseup', endDrag)
  window.addEventListener('touchmove', onDrag, { passive: false })
  window.addEventListener('touchend', endDrag)
  window.addEventListener('resize', () => {
    // 窗口大小变化时，确保按钮在可视区域内
    const btnSize = 56
    pos.value.x = Math.max(0, Math.min(pos.value.x, window.innerWidth - btnSize))
    pos.value.y = Math.max(0, Math.min(pos.value.y, window.innerHeight - btnSize))
  })
})

onUnmounted(() => {
  window.removeEventListener('mousemove', onDrag)
  window.removeEventListener('mouseup', endDrag)
  window.removeEventListener('touchmove', onDrag)
  window.removeEventListener('touchend', endDrag)
})
</script>

<style scoped>
.word-search-float {
  position: fixed;
  z-index: 9999;
  user-select: none;
}

.float-btn {
  width: 56px;
  height: 56px;
  border-radius: 50%;
  background: linear-gradient(135deg, var(--accent-blue) 0%, var(--accent-cyan) 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  color: #fff;
  box-shadow: 0 4px 16px rgba(64, 158, 255, 0.4), 0 0 0 4px rgba(64, 158, 255, 0.1);
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
  position: relative;
}

.float-btn:hover {
  transform: scale(1.1);
  box-shadow: 0 6px 24px rgba(64, 158, 255, 0.5), 0 0 0 6px rgba(64, 158, 255, 0.15);
}

.float-btn:active {
  transform: scale(0.95);
}

/* 脉冲动画 */
.float-btn::after {
  content: '';
  position: absolute;
  inset: -4px;
  border-radius: 50%;
  border: 2px solid rgba(64, 158, 255, 0.3);
  animation: pulse 2s ease-in-out infinite;
}

@keyframes pulse {
  0%, 100% {
    transform: scale(1);
    opacity: 1;
  }
  50% {
    transform: scale(1.15);
    opacity: 0;
  }
}

/* 搜索输入框 */
.search-input-wrapper {
  margin-bottom: 20px;
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

/* 空状态 */
.empty-state {
  text-align: center;
  padding: 40px 20px;
  color: var(--text-muted);
}

.empty-state p {
  margin-top: 12px;
  font-size: 14px;
}

/* 单词详情样式 */
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
  .float-btn {
    width: 48px;
    height: 48px;
  }

  .word-detail-content {
    max-height: calc(90vh - 180px);
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

<style>
/* 弹窗全局样式 - unscoped */
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

  .word-search-dialog :deep(.el-dialog__body) {
    padding: 12px 16px;
  }
}
</style>
