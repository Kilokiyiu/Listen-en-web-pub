<template>
  <div class="word-review-page le-page">
    <div class="page-header">
      <h1>
        <el-icon :size="28" :color="isFreeReview ? '#409eff' : '#e6a23c'"><Timer /></el-icon>
        {{ isFreeReview ? '自由复习' : '单词复习' }}
      </h1>
      <p class="subtitle">{{ isFreeReview ? '随机抽单词，随时开始随时结束' : '基于 SM-2 间隔重复算法' }}</p>
      <el-button v-if="isFreeReview" type="info" @click="endReview" class="end-btn">
        <el-icon><SwitchButton /></el-icon>
        结束复习
      </el-button>
    </div>

    <!-- 没有单词时 -->
    <el-empty
      v-if="!loading && dueWords.length === 0"
      :description="isFreeReview ? '你的单词本还是空的，先去添加单词吧' : '暂时没有需要复习的单词'"
    >
      <el-button v-if="!isFreeReview" type="primary" @click="goToWordRoots">
        去学习词根
      </el-button>
      <el-button type="primary" @click="goToMyWords">
        去添加单词
      </el-button>
    </el-empty>

    <!-- 复习卡片 -->
    <div v-else-if="currentWord" class="review-container">
      <!-- 进度 -->
      <div class="review-progress">
        <span>{{ currentIndex + 1 }} / {{ dueWords.length }}</span>
        <el-progress
          :percentage="Math.round(((currentIndex) / dueWords.length) * 100)"
          :stroke-width="8"
          style="width: 200px"
        />
      </div>

      <!-- 卡片 -->
      <div
        class="flash-card"
        :class="{ flipped: isFlipped }"
        @click="flipCard"
      >
        <!-- 正面 -->
        <div class="card-face card-front">
          <div class="word-display">{{ currentWord.word }}</div>
          <div class="flip-hint">点击翻转查看释义</div>
        </div>

        <!-- 背面 -->
        <div class="card-face card-back">
          <div class="word-display">{{ currentWord.word }}</div>
          <div class="definition">{{ currentWord.definition || '暂无释义' }}</div>
          <div class="example" v-if="currentWord.example">
            {{ currentWord.example }}
          </div>
        </div>
      </div>

      <!-- 评分按钮 -->
      <div class="rating-buttons" v-if="isFlipped">
        <el-button
          type="danger"
          size="large"
          @click="rateWord(0)"
        >
          <el-icon><Close /></el-icon>
          忘记
        </el-button>
        <el-button
          type="warning"
          size="large"
          @click="rateWord(3)"
        >
          <el-icon><SemiSelect /></el-icon>
          模糊
        </el-button>
        <el-button
          type="success"
          size="large"
          @click="rateWord(5)"
        >
          <el-icon><Check /></el-icon>
          记住
        </el-button>
      </div>

      <!-- 跳过按钮 -->
      <div class="skip-button" v-if="!isFlipped">
        <el-button link @click="skipWord">
          跳过这个单词
        </el-button>
      </div>
    </div>

    <!-- 完成 -->
    <el-result
      v-if="isComplete"
      icon="success"
      :title="isFreeReview ? '复习结束！' : '复习完成！'"
      :sub-title="isFreeReview ? '你已经复习了这一轮的所有单词' : '你已经完成了今天的复习任务'"
    >
      <template #extra>
        <el-button type="primary" @click="goToMyWords">
          查看单词本
        </el-button>
        <el-button v-if="isFreeReview" @click="restartFreeReview">
          再来一轮
        </el-button>
        <el-button v-else @click="goToWordRoots">
          继续学习
        </el-button>
      </template>
    </el-result>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessage } from 'element-plus'
import { getDueWords, getRandomWords, reviewWord } from '../api/Word.js'

const router = useRouter()
const route = useRoute()

const isFreeReview = computed(() => route.query.mode === 'free')

const dueWords = ref([])
const currentIndex = ref(0)
const isFlipped = ref(false)
const loading = ref(false)
const isComplete = ref(false)

const currentWord = computed(() => {
  if (currentIndex.value < dueWords.value.length) {
    return dueWords.value[currentIndex.value]
  }
  return null
})

const loadDueWords = async () => {
  loading.value = true
  try {
    if (isFreeReview.value) {
      const res = await getRandomWords({ limit: 50 })
      dueWords.value = res || []
    } else {
      const res = await getDueWords({ limit: 50 })
      dueWords.value = res || []
    }
    if (dueWords.value.length === 0) {
      isComplete.value = true
    }
  } catch (e) {
    console.error('获取单词失败', e)
  } finally {
    loading.value = false
  }
}

const flipCard = () => {
  isFlipped.value = !isFlipped.value
}

const rateWord = async (quality) => {
  try {
    const word = currentWord.value
    await reviewWord(word.id, quality)

    // 显示反馈
    const messages = {
      0: '没关系，下次会记住的！',
      3: '继续加油，多复习几次就记住了！',
      5: '太棒了！继续保持！'
    }
    ElMessage.success(messages[quality])

    // 下一个
    isFlipped.value = false
    currentIndex.value++

    if (currentIndex.value >= dueWords.value.length) {
      isComplete.value = true
    }
  } catch (e) {
    console.error(e)
  }
}

const skipWord = () => {
  currentIndex.value++
  if (currentIndex.value >= dueWords.value.length) {
    isComplete.value = true
  }
}

const endReview = () => {
  router.push({ name: 'myWords' })
}

const restartFreeReview = () => {
  isComplete.value = false
  dueWords.value = []
  currentIndex.value = 0
  isFlipped.value = false
  loadDueWords()
}

const goToMyWords = () => {
  router.push({ name: 'myWords' })
}

const goToWordRoots = () => {
  router.push({ name: 'wordRoots' })
}

onMounted(() => {
  loadDueWords()
})
</script>

<style scoped>
.word-review-page {
  padding: 24px;
  max-width: 800px;
  margin: 0 auto;
  min-height: calc(100vh - 60px);
}

.page-header {
  text-align: center;
  margin-bottom: 32px;
}

.page-header h1 {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  font-size: 28px;
  color: var(--text-primary);
  margin-bottom: 8px;
}

.subtitle {
  color: var(--text-secondary);
  font-size: 14px;
}

.review-container {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.review-progress {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 24px;
  color: var(--text-secondary);
}

.flash-card {
  width: 100%;
  max-width: 500px;
  height: 300px;
  perspective: 1000px;
  cursor: pointer;
  margin-bottom: 32px;
}

.card-face {
  width: 100%;
  height: 100%;
  position: absolute;
  backface-visibility: hidden;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  background: var(--bg-card);
  border: 1px solid var(--border-glass);
  border-radius: 16px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
  transition: transform 0.6s ease;
  padding: 32px;
}

.card-front {
  z-index: 2;
}

.card-back {
  transform: rotateY(180deg);
}

.flash-card.flipped .card-front {
  transform: rotateY(180deg);
}

.flash-card.flipped .card-back {
  transform: rotateY(0deg);
}

.word-display {
  font-size: 36px;
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 16px;
}

.flip-hint {
  font-size: 13px;
  color: var(--text-muted);
}

.definition {
  font-size: 18px;
  color: var(--text-secondary);
  text-align: center;
  margin-bottom: 12px;
}

.example {
  font-size: 14px;
  color: var(--text-muted);
  text-align: center;
  font-style: italic;
}

.rating-buttons {
  display: flex;
  gap: 16px;
}

.rating-buttons .el-button {
  min-width: 120px;
}

.skip-button {
  margin-top: 16px;
}

.end-btn {
  margin-top: 12px;
}

/* 手机端适配 */
@media (max-width: 768px) {
  .word-review-page {
    padding: 12px;
  }

  .page-header h1 {
    font-size: 22px;
  }

  .review-progress {
    flex-direction: column;
    gap: 8px;
  }

  .flash-card {
    max-width: 100%;
    height: 250px;
  }

  .word-display {
    font-size: 28px;
  }

  .definition {
    font-size: 16px;
  }

  .rating-buttons {
    flex-wrap: wrap;
    justify-content: center;
    gap: 8px;
  }

  .rating-buttons .el-button {
    min-width: 100px;
    flex: 1;
  }
}
</style>
