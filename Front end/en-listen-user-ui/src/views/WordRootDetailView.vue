<template>
  <div class="word-root-detail-page">
    <el-page-header @back="goBack" title="词根详情" />

    <div v-if="wordRoot" class="detail-content">
      <!-- 词根基本信息 -->
      <el-card class="root-info-card">
        <div class="root-header">
          <h1>{{ wordRoot.root }}</h1>
          <el-tag :type="wordRoot.origin === 'Latin' ? 'primary' : 'warning'">
            {{ wordRoot.origin === 'Latin' ? '拉丁语' : '希腊语' }}
          </el-tag>
        </div>
        <div class="root-meaning">
          <p class="meaning-cn">{{ wordRoot.meaning }}</p>
          <p class="meaning-en" v-if="wordRoot.meaningEn">{{ wordRoot.meaningEn }}</p>
        </div>
        <div class="root-description">{{ wordRoot.description }}</div>
        <div class="root-actions">
          <el-button
            v-if="isLoggedIn"
            type="success"
            :disabled="isMastered"
            @click="markMastered"
          >
            <el-icon><CircleCheck /></el-icon>
            {{ isMastered ? '已掌握' : '标记为已掌握' }}
          </el-button>
          <el-button @click="showQuiz = true">
            <el-icon><QuestionFilled /></el-icon>
            小测验
          </el-button>
        </div>

        <!-- 上一个/下一个导航 -->
        <div class="root-nav">
          <el-button
            type="primary"
            plain
            :disabled="!hasPrev"
            @click="goToPrev"
          >
            <el-icon><ArrowLeft /></el-icon>
            上一个词根
          </el-button>
          <el-button
            type="primary"
            plain
            :disabled="!hasNext"
            @click="goToNext"
          >
            下一个词根
            <el-icon><ArrowRight /></el-icon>
          </el-button>
        </div>
      </el-card>

      <!-- 例词列表 -->
      <el-card class="examples-card">
        <template #header>
          <div class="card-header">
            <el-icon><Document /></el-icon>
            <span>例词 ({{ wordRoot.examples?.length || 0 }})</span>
          </div>
        </template>
        <div
          v-for="example in wordRoot.examples"
          :key="example.id"
          class="example-item"
        >
          <div class="example-word">
            <span v-if="example.prefix" class="prefix">{{ example.prefix }}</span>
            <span class="root-highlight">{{ example.root }}</span>
            <span v-if="example.suffix" class="suffix">{{ example.suffix }}</span>
          </div>
          <div class="example-meaning">{{ example.meaning }}</div>
          <div class="example-explanation" v-if="example.explanation">
            {{ example.explanation }}
          </div>
          <el-button
            v-if="isLoggedIn"
            type="primary"
            link
            size="small"
            @click="addToWordbook(example)"
          >
            <el-icon><Plus /></el-icon>
            加入单词本
          </el-button>
        </div>
      </el-card>

      <!-- 测验弹窗 -->
      <el-dialog
        v-model="showQuiz"
        title="词根测验"
        width="500px"
        :close-on-click-modal="false"
      >
        <div v-if="quiz" class="quiz-content">
          <p class="quiz-question">{{ quiz.question }}</p>
          <el-radio-group v-model="quizAnswer" class="quiz-options">
            <el-radio
              v-for="(option, index) in quiz.options"
              :key="index"
              :label="index"
            >
              {{ option }}
            </el-radio>
          </el-radio-group>
          <div class="quiz-result" v-if="quizSubmitted">
            <el-result
              :icon="quizCorrect ? 'success' : 'error'"
              :title="quizCorrect ? '回答正确！' : '回答错误'"
              :sub-title="quizCorrect ? '继续加油！' : `正确答案是：${quiz.options[quiz.correctAnswer]}`"
            />
          </div>
        </div>
        <template #footer>
          <el-button @click="showQuiz = false">关闭</el-button>
          <el-button
            type="primary"
            @click="submitQuiz"
            :disabled="quizAnswer === null || quizSubmitted"
          >
            提交答案
          </el-button>
        </template>
      </el-dialog>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { getWordRootDetail, getWordRootQuiz, markWordRootMastered, addUserWord, getWordRoots } from '../api/Word.js'

const route = useRoute()
const router = useRouter()

const wordRoot = ref(null)
const quiz = ref(null)
const showQuiz = ref(false)
const quizAnswer = ref(null)
const quizSubmitted = ref(false)
const quizCorrect = ref(false)
const isMastered = ref(false)
const isLoggedIn = ref(false)
const allRootIds = ref([])
const currentIndex = ref(-1)
const hasPrev = computed(() => currentIndex.value > 0)
const hasNext = computed(() => currentIndex.value >= 0 && currentIndex.value < allRootIds.value.length - 1)

// 检查登录状态
const checkLoginStatus = () => {
  isLoggedIn.value = !!(localStorage.getItem('token') && localStorage.getItem('userId'))
}

// 加载所有词根ID列表（用于导航）
const loadAllRootIds = async () => {
  try {
    const res = await getWordRoots({ page: 1, pageSize: 1000 })
    allRootIds.value = (res.items || []).map(item => item.id)
    const id = route.params.id
    currentIndex.value = allRootIds.value.findIndex(itemId => itemId === id)
  } catch (e) {
    console.error('加载词根列表失败', e)
  }
}

const goToPrev = () => {
  if (currentIndex.value > 0) {
    const prevId = allRootIds.value[currentIndex.value - 1]
    router.push({ name: 'wordRootDetail', params: { id: prevId } })
  }
}

const goToNext = () => {
  if (currentIndex.value < allRootIds.value.length - 1) {
    const nextId = allRootIds.value[currentIndex.value + 1]
    router.push({ name: 'wordRootDetail', params: { id: nextId } })
  }
}

const loadWordRoot = async () => {
  try {
    const id = route.params.id
    const res = await getWordRootDetail(id)
    wordRoot.value = res
  } catch (e) {
    console.error('获取词根详情失败', e)
  }
}

const loadQuiz = async () => {
  try {
    const id = route.params.id
    const res = await getWordRootQuiz(id)
    quiz.value = res
  } catch (e) {
    console.error('获取测验失败', e)
  }
}

const markMastered = async () => {
  if (!isLoggedIn.value) {
    ElMessage.warning('请先登录')
    return
  }
  try {
    const id = route.params.id
    await markWordRootMastered(id)
    isMastered.value = true
    ElMessage.success('已标记为掌握')
  } catch (e) {
    console.error(e)
  }
}

const submitQuiz = () => {
  quizSubmitted.value = true
  quizCorrect.value = quizAnswer.value === quiz.value.correctAnswer
}

const addToWordbook = async (example) => {
  if (!isLoggedIn.value) {
    ElMessage.warning('请先登录')
    return
  }
  try {
    await addUserWord({
      word: example.word,
      definition: example.meaning,
      example: example.explanation || ''
    })
    ElMessage.success('已添加到单词本')
  } catch (e) {
    console.error(e)
  }
}

const goBack = () => {
  router.push({ name: 'wordRoots' })
}

onMounted(() => {
  checkLoginStatus()
  loadAllRootIds()
  loadWordRoot()
  loadQuiz()
})

// 监听路由参数变化，切换词根时重新加载
watch(() => route.params.id, (newId, oldId) => {
  if (newId && newId !== oldId) {
    currentIndex.value = allRootIds.value.findIndex(itemId => itemId === newId)
    loadWordRoot()
    loadQuiz()
    isMastered.value = false
  }
})
</script>

<style scoped>
.word-root-detail-page {
  padding: 24px;
  max-width: 900px;
  margin: 0 auto;
}

.detail-content {
  margin-top: 24px;
}

.root-info-card {
  margin-bottom: 24px;
}

.root-header {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 16px;
}

.root-header h1 {
  font-size: 32px;
  color: var(--text-primary);
  margin: 0;
}

.root-meaning {
  margin-bottom: 16px;
}

.meaning-cn {
  font-size: 18px;
  color: var(--text-primary);
  font-weight: 500;
  margin-bottom: 4px;
}

.meaning-en {
  font-size: 14px;
  color: var(--text-secondary);
}

.root-description {
  color: var(--text-secondary);
  line-height: 1.6;
  margin-bottom: 20px;
}

.root-actions {
  display: flex;
  gap: 12px;
}

.root-nav {
  margin-top: 16px;
  display: flex;
  justify-content: space-between;
  gap: 12px;
}

.examples-card {
  margin-bottom: 24px;
}

.card-header {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 500;
}

.example-item {
  padding: 16px 0;
  border-bottom: 1px solid #eee;
}

.example-item:last-child {
  border-bottom: none;
}

.example-word {
  font-size: 18px;
  margin-bottom: 8px;
}

.prefix {
  color: #409eff;
}

.root-highlight {
  color: #f56c6c;
  font-weight: 600;
  text-decoration: underline;
}

.suffix {
  color: #67c23a;
}

.example-meaning {
  color: var(--text-secondary);
  margin-bottom: 4px;
}

.example-explanation {
  font-size: 13px;
  color: var(--text-muted);
  margin-bottom: 8px;
}

.quiz-content {
  padding: 16px 0;
}

.quiz-question {
  font-size: 16px;
  font-weight: 500;
  margin-bottom: 20px;
}

.quiz-options {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.quiz-result {
  margin-top: 20px;
}
</style>
