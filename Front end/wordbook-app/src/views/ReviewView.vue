<template>
  <div class="review-page">
    <p class="subtitle">{{ isFreeReview ? '随机抽词，随时结束' : 'SM-2 间隔重复' }}</p>

    <div v-if="loading" class="loading">加载中...</div>

    <div v-else-if="dueWords.length === 0 && !isComplete" class="empty">
      <p>{{ isFreeReview ? '单词本还是空的' : '暂时没有到期要复习的词' }}</p>
      <button class="btn-primary" @click="router.push('/add')">去添加单词</button>
      <button v-if="!isFreeReview" class="btn-secondary" @click="goFreeReview">自由复习</button>
    </div>

    <div v-else-if="currentWord && !isComplete" class="review-body">
      <div class="progress">
        <span>{{ currentIndex + 1 }} / {{ dueWords.length }}</span>
        <div class="progress-bar">
          <div class="progress-fill" :style="{ width: progressPct + '%' }" />
        </div>
      </div>

      <div class="flash-card" :class="{ flipped: isFlipped }" @click="flipCard">
        <div class="card-face front">
          <div class="word">{{ currentWord.word }}</div>
          <p class="hint">点击翻转查看释义</p>
        </div>
        <div class="card-face back">
          <div class="word">{{ currentWord.word }}</div>
          <p class="definition">{{ currentWord.definition || '暂无释义' }}</p>
          <p v-if="currentWord.example" class="example">{{ currentWord.example }}</p>
        </div>
      </div>

      <div v-if="isFlipped" class="rating">
        <button class="rate forget" @click="rate(0)">忘记</button>
        <button class="rate fuzzy" @click="rate(3)">模糊</button>
        <button class="rate remember" @click="rate(5)">记住</button>
      </div>
      <button v-else class="btn-skip" @click="skip">跳过</button>
    </div>

    <div v-if="isComplete && dueWords.length > 0" class="complete">
      <div class="complete-icon">🎉</div>
      <h2>{{ isFreeReview ? '本轮复习完成！' : '今日复习完成！' }}</h2>
      <p>{{ isFreeReview ? `你复习了 ${dueWords.length} 个单词` : `完成了 ${dueWords.length} 个到期单词` }}</p>
      <button class="btn-primary" @click="router.push('/')">查看单词本</button>
      <button v-if="isFreeReview" class="btn-secondary" @click="restart">再来一轮</button>
      <button v-else class="btn-secondary" @click="goFreeReview">自由复习</button>
    </div>

    <div v-else-if="isComplete && dueWords.length === 0" class="complete">
      <h2>{{ isFreeReview ? '单词本还是空的' : '暂时没有到期单词' }}</h2>
      <p>{{ isFreeReview ? '先去加几个词，再回来完成一轮复习' : '可以自由复习，或继续添加单词' }}</p>
      <button class="btn-primary" @click="router.push('/add')">去添加单词</button>
      <button v-if="!isFreeReview" class="btn-secondary" @click="goFreeReview">自由复习</button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { getDueWords, getRandomWords, reviewWord } from '../services/wordService'
import { setCurrentWordBookId } from '../services/appSettings'
import { showToast } from '../utils/toast'
import { trackEvent } from '../api/analytics'

const router = useRouter()
const route = useRoute()

const isFreeReview = computed(() => route.query.mode === 'free')
const dueWords = ref([])
const currentIndex = ref(0)
const isFlipped = ref(false)
const loading = ref(false)
const isComplete = ref(false)
let enteredTracked = false
let finishTracked = false

const currentWord = computed(() => dueWords.value[currentIndex.value] || null)
const progressPct = computed(() =>
  dueWords.value.length ? Math.round((currentIndex.value / dueWords.value.length) * 100) : 0
)

const trackFinish = () => {
  if (finishTracked || dueWords.value.length === 0) return
  finishTracked = true
  trackEvent('finish_review', '/app/review')
}

const load = async () => {
  loading.value = true
  try {
    const bookId = route.query.wordBookId
    if (bookId) {
      await setCurrentWordBookId(String(bookId))
    }
    if (isFreeReview.value) {
      dueWords.value = (await getRandomWords({ limit: 50 })) || []
    } else {
      dueWords.value = (await getDueWords({ limit: 50 })) || []
    }
    if (dueWords.value.length === 0) {
      isComplete.value = true
    } else if (!enteredTracked) {
      enteredTracked = true
      trackEvent('enter_review', '/app/review')
    }
  } catch (e) {
    showToast(typeof e === 'string' ? e : '加载失败')
  } finally {
    loading.value = false
  }
}

const flipCard = () => { isFlipped.value = !isFlipped.value }

const messages = { 0: '没关系，下次会记住的！', 3: '继续加油！', 5: '太棒了！' }

const rate = async (quality) => {
  try {
    await reviewWord(currentWord.value.id, quality)
    showToast(messages[quality])
    isFlipped.value = false
    currentIndex.value++
    if (currentIndex.value >= dueWords.value.length) {
      isComplete.value = true
      trackFinish()
    }
  } catch (e) {
    showToast(typeof e === 'string' ? e : '保存失败')
  }
}

const skip = () => {
  currentIndex.value++
  if (currentIndex.value >= dueWords.value.length) {
    isComplete.value = true
    trackFinish()
  }
}

const restart = () => {
  isComplete.value = false
  currentIndex.value = 0
  isFlipped.value = false
  enteredTracked = false
  finishTracked = false
  load()
}

const goFreeReview = () => {
  router.push({ path: '/review', query: { mode: 'free' } })
}

watch(
  () => route.query.mode,
  () => {
    isComplete.value = false
    dueWords.value = []
    currentIndex.value = 0
    isFlipped.value = false
    enteredTracked = false
    finishTracked = false
    load()
  }
)

onMounted(load)
</script>

<style scoped>
.review-page {
  padding: 16px;
}

.subtitle {
  font-size: 14px;
  color: var(--text-secondary);
  margin: 0 0 20px;
  text-align: center;
}

.loading, .empty {
  text-align: center;
  padding: 60px 20px;
  color: var(--text-muted);
}

.review-body {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.progress {
  width: 100%;
  max-width: 320px;
  text-align: center;
  margin-bottom: 24px;
  color: var(--text-secondary);
  font-size: 14px;
}

.progress-bar {
  height: 6px;
  background: var(--border);
  border-radius: 3px;
  margin-top: 8px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  background: var(--primary);
  border-radius: 3px;
  transition: width 0.3s;
}

.flash-card {
  width: 100%;
  max-width: 340px;
  height: 260px;
  perspective: 1000px;
  cursor: pointer;
  position: relative;
  margin-bottom: 28px;
}

.card-face {
  position: absolute;
  inset: 0;
  backface-visibility: hidden;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  background: var(--bg-card);
  border: 1px solid var(--border);
  border-radius: 16px;
  padding: 24px;
  transition: transform 0.5s;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.06);
}

.card-face.back {
  transform: rotateY(180deg);
}

.flash-card.flipped .card-face.front {
  transform: rotateY(180deg);
}

.flash-card.flipped .card-face.back {
  transform: rotateY(0);
}

.word {
  font-size: 32px;
  font-weight: 600;
  margin-bottom: 12px;
}

.hint {
  font-size: 13px;
  color: var(--text-muted);
}

.definition {
  font-size: 17px;
  color: var(--text-secondary);
  text-align: center;
  margin: 0 0 10px;
}

.example {
  font-size: 14px;
  color: var(--text-muted);
  font-style: italic;
  text-align: center;
  margin: 0;
  white-space: pre-line;
}

.rating {
  display: flex;
  gap: 12px;
  width: 100%;
  max-width: 340px;
}

.rate {
  flex: 1;
  padding: 14px 8px;
  border: none;
  border-radius: 10px;
  font-size: 15px;
  font-weight: 600;
  color: #fff;
}

.rate.forget { background: #f56c6c; }
.rate.fuzzy { background: #e6a23c; }
.rate.remember { background: #67c23a; }

.btn-skip {
  background: none;
  border: none;
  color: var(--text-muted);
  font-size: 14px;
}

.complete {
  text-align: center;
  padding: 40px 20px;
}

.complete-icon {
  font-size: 56px;
  margin-bottom: 12px;
}

.complete h2 {
  margin: 0 0 8px;
}

.complete p {
  color: var(--text-secondary);
  margin-bottom: 24px;
}

.btn-primary, .btn-secondary {
  display: block;
  width: 100%;
  max-width: 280px;
  margin: 0 auto 12px;
  padding: 14px;
  border-radius: 10px;
  font-size: 16px;
  border: none;
}

.btn-primary {
  background: var(--primary);
  color: #fff;
}

.btn-secondary {
  background: var(--bg-card);
  border: 1px solid var(--border);
}
</style>
