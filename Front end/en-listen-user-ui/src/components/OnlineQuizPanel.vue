<template>
  <div class="quiz-panel le-card">
    <div class="quiz-header">
      <div>
        <h3 class="quiz-title">{{ title }}</h3>
        <p v-if="subtitle" class="quiz-subtitle">{{ subtitle }}</p>
      </div>
      <el-tag v-if="submitted" type="success" effect="light">
        {{ correctCount }}/{{ questions.length }} · {{ scorePercent }}%
      </el-tag>
      <el-tag v-else type="info" effect="light">
        已选 {{ answeredCount }}/{{ questions.length }}
      </el-tag>
    </div>

    <div v-if="passage" class="quiz-passage">
      <div class="passage-label">原文</div>
      <div class="passage-body">{{ passage }}</div>
    </div>

    <el-empty v-if="!questions.length" description="暂无在线题目" :image-size="72" />

    <div v-else class="quiz-list">
      <div
        v-for="q in questions"
        :key="q.id"
        class="quiz-item"
        :class="resultClass(q.id)"
      >
        <div class="quiz-stem">
          <span class="quiz-num">{{ q.number }}.</span>
          <template v-if="showStem">
            <span v-if="q.stem">{{ q.stem }}</span>
            <span v-else class="quiz-stem-muted">第 {{ q.number }} 题</span>
          </template>
          <span v-else class="quiz-stem-muted">题干已隐藏</span>
        </div>
        <el-radio-group
          :model-value="answers[q.id]"
          class="quiz-options"
          :disabled="submitted"
          @update:model-value="(v) => setAnswer(q.id, v)"
        >
          <el-radio
            v-for="(opt, idx) in q.options || []"
            :key="idx"
            :value="idx"
            class="quiz-option"
          >
            {{ opt }}
          </el-radio>
        </el-radio-group>
        <div v-if="submitted && resultMap[q.id]" class="quiz-feedback">
          <span :class="resultMap[q.id].isCorrect ? 'ok' : 'bad'">
            {{ resultMap[q.id].isCorrect ? '正确' : '错误' }}
          </span>
          <span v-if="!resultMap[q.id].isCorrect" class="answer-key">
            正确答案：{{ (q.options || [])[resultMap[q.id].correctAnswer] }}
          </span>
          <div v-if="resultMap[q.id].explanation" class="explain">
            {{ resultMap[q.id].explanation }}
          </div>
        </div>
      </div>
    </div>

    <div v-if="questions.length && !deferCheck" class="quiz-actions">
      <el-button v-if="!submitted" type="primary" round :loading="submitting" @click="onSubmit">
        查看本段答案
      </el-button>
      <el-button v-else round @click="onReset">本段再练一次</el-button>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { ElMessage } from 'element-plus'
import {
  clearQuizProgress,
  loadQuizProgress,
  saveQuizProgress,
} from '../utils/quizProgressStore.js'

const props = defineProps({
  title: { type: String, default: '在线做题' },
  subtitle: { type: String, default: '' },
  passage: { type: String, default: '' },
  questions: { type: Array, default: () => [] },
  /** 是否显示小题题干；false 时仍显示题号与选项 */
  showStem: { type: Boolean, default: true },
  /** async (answers: {questionId, selectedIndex}[]) => { correctCount, scorePercent, results } */
  submitFn: { type: Function, default: null },
  /** 传入则本会话内保留作答与判分结果（切换分区 / 刷新不丢） */
  persistKey: { type: String, default: '' },
  /**
   * 延后对答案：本区只选题并保存进度，不显示「提交并查看答案」；
   * 由父级整卷提交后调用 applyGrade 统一揭示。
   */
  deferCheck: { type: Boolean, default: false },
})

const emit = defineEmits(['submitted', 'reset', 'progress'])

const answers = reactive({})
const submitted = ref(false)
const submitting = ref(false)
const correctCount = ref(0)
const scorePercent = ref(0)
const resultMap = reactive({})

const questionIdsKey = computed(() =>
  (props.questions || []).map((q) => q.id).join(',')
)

const clearLocal = () => {
  Object.keys(answers).forEach((k) => delete answers[k])
  Object.keys(resultMap).forEach((k) => delete resultMap[k])
  submitted.value = false
  correctCount.value = 0
  scorePercent.value = 0
}

const persistNow = () => {
  if (!props.persistKey) return
  saveQuizProgress(props.persistKey, {
    answers: { ...answers },
    resultMap: { ...resultMap },
    submitted: submitted.value,
    correctCount: correctCount.value,
    scorePercent: scorePercent.value,
    questionIds: questionIdsKey.value,
  })
}

const emitProgress = () => {
  const answered = props.questions.filter(
    (q) => answers[q.id] !== undefined && answers[q.id] !== null
  ).length
  emit('progress', {
    answered,
    total: props.questions.length,
    complete: answered === props.questions.length && props.questions.length > 0,
    graded: submitted.value,
  })
}

const restorePersisted = () => {
  if (!props.persistKey) return false
  const data = loadQuizProgress(props.persistKey)
  if (!data || data.questionIds !== questionIdsKey.value) return false
  clearLocal()
  Object.assign(answers, data.answers || {})
  Object.assign(resultMap, data.resultMap || {})
  submitted.value = !!data.submitted
  correctCount.value = data.correctCount ?? 0
  scorePercent.value = data.scorePercent ?? 0
  emitProgress()
  return true
}

watch(questionIdsKey, () => {
  clearLocal()
  restorePersisted()
  emitProgress()
})

onMounted(() => {
  restorePersisted()
  emitProgress()
})

const answeredCount = computed(() =>
  props.questions.filter((q) => answers[q.id] !== undefined && answers[q.id] !== null).length
)

const setAnswer = (id, value) => {
  if (submitted.value) return
  answers[id] = value
  persistNow()
  emitProgress()
}

const resultClass = (id) => {
  if (!submitted.value || !resultMap[id]) return ''
  return resultMap[id].isCorrect ? 'is-correct' : 'is-wrong'
}

const applyGrade = (res) => {
  correctCount.value = res.correctCount ?? 0
  scorePercent.value = res.scorePercent ?? 0
  Object.keys(resultMap).forEach((k) => delete resultMap[k])
  for (const r of res.results || []) {
    const id = r.questionId || r.QuestionId
    resultMap[id] = {
      isCorrect: !!(r.isCorrect ?? r.IsCorrect),
      correctAnswer: r.correctAnswer ?? r.CorrectAnswer,
      explanation: r.explanation ?? r.Explanation,
    }
  }
  submitted.value = true
  persistNow()
  emitProgress()
}

const getAnswers = () =>
  props.questions.map((q) => ({
    questionId: q.id,
    selectedIndex: answers[q.id],
  }))

const isComplete = () =>
  props.questions.length > 0 &&
  props.questions.every((q) => answers[q.id] !== undefined && answers[q.id] !== null)

const onSubmit = async () => {
  if (typeof props.submitFn !== 'function') return
  const unanswered = props.questions.filter((q) => answers[q.id] === undefined || answers[q.id] === null)
  if (unanswered.length) {
    ElMessage.warning(`还有 ${unanswered.length} 题未作答，请全部选完再提交`)
    return
  }
  submitting.value = true
  try {
    const res = await props.submitFn(getAnswers())
    applyGrade(res)
    emit('submitted', res)
  } catch (e) {
    // 错误已在拦截器提示
  } finally {
    submitting.value = false
  }
}

const onReset = () => {
  clearLocal()
  clearQuizProgress(props.persistKey)
  emitProgress()
  emit('reset')
}

defineExpose({
  submitted,
  correctCount,
  scorePercent,
  getAnswers,
  isComplete,
  applyGrade,
  resetAll: onReset,
  answeredCount,
})
</script>

<style scoped>
.quiz-panel {
  padding: 20px 18px 18px;
  margin-top: 20px;
}

.quiz-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 16px;
}

.quiz-title {
  margin: 0;
  font-size: 17px;
  font-weight: 650;
  color: var(--le-text);
}

.quiz-subtitle {
  margin: 6px 0 0;
  font-size: 13px;
  color: var(--le-text-muted);
}

.quiz-passage {
  margin-bottom: 18px;
  padding: 14px 16px;
  background: var(--le-bg-muted);
  border-radius: 12px;
  border: 1px solid var(--le-border);
}

.passage-label {
  font-size: 12px;
  font-weight: 600;
  color: var(--le-text-muted);
  margin-bottom: 8px;
}

.passage-body {
  white-space: pre-wrap;
  word-break: break-word;
  line-height: 1.8;
  font-size: 15px;
  color: var(--le-text);
}

.quiz-list {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.quiz-item {
  padding: 14px 14px 10px;
  border: 1px solid var(--le-border);
  border-radius: 12px;
  background: #fff;
}

.quiz-item.is-correct {
  border-color: #86efac;
  background: #f0fdf4;
}

.quiz-item.is-wrong {
  border-color: #fca5a5;
  background: #fef2f2;
}

.quiz-stem {
  font-weight: 600;
  margin-bottom: 10px;
  line-height: 1.55;
}

.quiz-num {
  margin-right: 6px;
  color: var(--le-primary, #2563eb);
}

.quiz-stem-muted {
  color: var(--le-text-muted);
  font-weight: 500;
}

.quiz-options {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 6px;
  width: 100%;
}

.quiz-option {
  margin: 0 !important;
  height: auto !important;
  white-space: normal;
  line-height: 1.5;
  align-items: flex-start;
}

.quiz-feedback {
  margin-top: 10px;
  font-size: 13px;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.quiz-feedback .ok { color: #16a34a; font-weight: 600; }
.quiz-feedback .bad { color: #dc2626; font-weight: 600; }
.answer-key { color: var(--le-text); }
.explain {
  margin: 6px 0 0;
  color: var(--le-text-muted);
  line-height: 1.6;
  white-space: pre-wrap;
  word-break: break-word;
  font-size: 13px;
}

.quiz-actions {
  margin-top: 18px;
  display: flex;
  justify-content: center;
  gap: 12px;
}
</style>
