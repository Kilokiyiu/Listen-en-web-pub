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
            :label="idx"
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
          <p v-if="resultMap[q.id].explanation" class="explain">
            {{ resultMap[q.id].explanation }}
          </p>
        </div>
      </div>
    </div>

    <div v-if="questions.length" class="quiz-actions">
      <el-button v-if="!submitted" type="primary" round :loading="submitting" @click="onSubmit">
        提交并查看答案
      </el-button>
      <el-button v-else round @click="onReset">再做一次</el-button>
    </div>
  </div>
</template>

<script setup>
import { computed, reactive, ref, watch } from 'vue'
import { ElMessage } from 'element-plus'

const props = defineProps({
  title: { type: String, default: '在线做题' },
  subtitle: { type: String, default: '' },
  passage: { type: String, default: '' },
  questions: { type: Array, default: () => [] },
  /** 是否显示小题题干；false 时仍显示题号与选项 */
  showStem: { type: Boolean, default: true },
  /** async (answers: {questionId, selectedIndex}[]) => { correctCount, scorePercent, results } */
  submitFn: { type: Function, required: true },
})

const emit = defineEmits(['submitted', 'reset'])

const answers = reactive({})
const submitted = ref(false)
const submitting = ref(false)
const correctCount = ref(0)
const scorePercent = ref(0)
const resultMap = reactive({})

watch(
  () => props.questions,
  () => {
    Object.keys(answers).forEach((k) => delete answers[k])
    Object.keys(resultMap).forEach((k) => delete resultMap[k])
    submitted.value = false
    correctCount.value = 0
    scorePercent.value = 0
  },
  { deep: true }
)

const answeredCount = computed(() =>
  props.questions.filter((q) => answers[q.id] !== undefined && answers[q.id] !== null).length
)

const setAnswer = (id, value) => {
  answers[id] = value
}

const resultClass = (id) => {
  if (!submitted.value || !resultMap[id]) return ''
  return resultMap[id].isCorrect ? 'is-correct' : 'is-wrong'
}

const onSubmit = async () => {
  const unanswered = props.questions.filter((q) => answers[q.id] === undefined || answers[q.id] === null)
  if (unanswered.length) {
    ElMessage.warning(`还有 ${unanswered.length} 题未作答，请全部选完再提交`)
    return
  }
  submitting.value = true
  try {
    const payload = props.questions.map((q) => ({
      questionId: q.id,
      selectedIndex: answers[q.id],
    }))

    const res = await props.submitFn(payload)
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
    emit('submitted', res)
  } catch (e) {
    // 错误已在拦截器提示
  } finally {
    submitting.value = false
  }
}

const onReset = () => {
  Object.keys(answers).forEach((k) => delete answers[k])
  Object.keys(resultMap).forEach((k) => delete resultMap[k])
  submitted.value = false
  correctCount.value = 0
  scorePercent.value = 0
  emit('reset')
}

defineExpose({ submitted, correctCount, scorePercent })
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
.explain { margin: 4px 0 0; color: var(--le-text-muted); line-height: 1.55; }

.quiz-actions {
  margin-top: 18px;
  display: flex;
  justify-content: center;
  gap: 12px;
}
</style>
