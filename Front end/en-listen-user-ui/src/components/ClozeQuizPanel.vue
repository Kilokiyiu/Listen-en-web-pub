<template>
  <div class="cloze-panel le-card">
    <div class="cloze-header">
      <div>
        <h3 class="cloze-title">{{ title || '完形填空' }}</h3>
        <p class="cloze-sub">点击文中空格可直接选题，也可在下方列表作答；全部答完后提交再看对错</p>
      </div>
      <el-tag v-if="submitted" type="success" effect="light">
        {{ correctCount }}/{{ questions.length }} · {{ scorePercent }}%
      </el-tag>
      <el-tag v-else type="info" effect="light">
        已选 {{ answeredCount }}/{{ questions.length }}
      </el-tag>
    </div>

    <div class="passage" v-if="tokens.length">
      <div class="passage-label">原文</div>
      <div class="passage-body">
        <template v-for="(t, i) in tokens" :key="i">
          <span v-if="t.type === 'text'">{{ t.text }}</span>
          <button
            v-else
            type="button"
            class="blank"
            :class="blankClass(t.number)"
            @click="focusBlank(t.number)"
          >
            {{ blankLabel(t.number) }}
          </button>
        </template>
      </div>
    </div>
    <el-empty v-else description="暂无完形原文" :image-size="64" />

    <el-empty v-if="!questions.length" description="暂无题目选项" :image-size="64" />

    <div v-else class="quiz-list">
      <div
        v-for="q in questions"
        :id="`cloze-q-${q.number}`"
        :key="q.id"
        class="quiz-item"
        :class="[resultClass(q.id), { 'is-focus': focusNumber === q.number }]"
      >
        <div class="quiz-stem">
          <span class="quiz-num">{{ q.number }}.</span>
          <span class="quiz-stem-muted">空格 {{ q.number }}</span>
        </div>
        <el-radio-group
          :model-value="selectedMap[q.id]"
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
        <div v-if="submitted && feedbackMap[q.id]" class="quiz-feedback">
          <span :class="feedbackMap[q.id].isCorrect ? 'ok' : 'bad'">
            {{ feedbackMap[q.id].isCorrect ? '正确' : '错误' }}
          </span>
          <span v-if="!feedbackMap[q.id].isCorrect" class="answer-key">
            正确答案：{{ (q.options || [])[feedbackMap[q.id].correctAnswer] }}
          </span>
          <p v-if="feedbackMap[q.id].explanation" class="explain">
            {{ feedbackMap[q.id].explanation }}
          </p>
        </div>
      </div>
    </div>

    <div v-if="questions.length" class="submit-row">
      <el-button
        v-if="!submitted"
        type="primary"
        round
        :loading="submitting"
        @click="onSubmit"
      >
        提交并查看答案
      </el-button>
      <el-button v-else round @click="onReset">再做一次</el-button>
    </div>

    <el-drawer
      v-model="drawerOpen"
      :title="activeQ ? `空格 ${activeQ.number}` : '选择选项'"
      direction="btt"
      size="auto"
      append-to-body
    >
      <div v-if="activeQ" class="drawer-body">
        <el-radio-group
          v-model="pendingIndex"
          class="quiz-options drawer-opts"
          :disabled="submitted"
        >
          <el-radio
            v-for="(opt, idx) in activeQ.options || []"
            :key="idx"
            :label="idx"
            class="quiz-option"
          >
            {{ opt }}
          </el-radio>
        </el-radio-group>
        <div v-if="submitted && feedbackMap[activeQ.id]" class="quiz-feedback">
          <span :class="feedbackMap[activeQ.id].isCorrect ? 'ok' : 'bad'">
            {{ feedbackMap[activeQ.id].isCorrect ? '正确' : '错误' }}
          </span>
          <span v-if="!feedbackMap[activeQ.id].isCorrect" class="answer-key">
            正确答案：{{ (activeQ.options || [])[feedbackMap[activeQ.id].correctAnswer] }}
          </span>
          <p v-if="feedbackMap[activeQ.id].explanation" class="explain">
            {{ feedbackMap[activeQ.id].explanation }}
          </p>
        </div>
        <div class="drawer-actions">
          <el-button @click="drawerOpen = false">关闭</el-button>
          <el-button
            v-if="!submitted"
            type="primary"
            :disabled="pendingIndex === null || pendingIndex === undefined"
            @click="confirmDrawer"
          >
            确认选择
          </el-button>
        </div>
      </div>
    </el-drawer>
  </div>
</template>

<script setup>
import { computed, reactive, ref, watch } from 'vue'
import { ElMessage } from 'element-plus'

const props = defineProps({
  title: { type: String, default: '完形填空' },
  passage: { type: String, default: '' },
  questions: { type: Array, default: () => [] },
  submitFn: { type: Function, required: true },
})

const emit = defineEmits(['submitted', 'reset'])

const submitting = ref(false)
const submitted = ref(false)
const correctCount = ref(0)
const scorePercent = ref(0)
const focusNumber = ref(null)
const drawerOpen = ref(false)
const pendingIndex = ref(null)
const feedbackMap = reactive({})
const selectedMap = reactive({})

const clearState = () => {
  Object.keys(feedbackMap).forEach((k) => delete feedbackMap[k])
  Object.keys(selectedMap).forEach((k) => delete selectedMap[k])
  submitted.value = false
  correctCount.value = 0
  scorePercent.value = 0
  focusNumber.value = null
  drawerOpen.value = false
  pendingIndex.value = null
}

watch(() => props.questions, () => clearState(), { deep: true })

const qByNumber = computed(() => {
  const map = {}
  for (const q of props.questions || []) map[q.number] = q
  return map
})

const activeQ = computed(() =>
  focusNumber.value != null ? qByNumber.value[focusNumber.value] : null
)

const answeredCount = computed(() => Object.keys(selectedMap).length)

const tokens = computed(() => {
  const text = props.passage || ''
  const re = /__(\d+)__|\{\{(\d+)\}\}|\[(\d+)\]/g
  const out = []
  let last = 0
  let m
  while ((m = re.exec(text)) !== null) {
    if (m.index > last) out.push({ type: 'text', text: text.slice(last, m.index) })
    out.push({ type: 'blank', number: Number(m[1] || m[2] || m[3]) })
    last = m.index + m[0].length
  }
  if (last < text.length) out.push({ type: 'text', text: text.slice(last) })
  return out
})

const short = (s) => {
  const t = (s || '').replace(/^[A-D]\)\s*/i, '')
  return t.length > 10 ? `${t.slice(0, 10)}…` : t
}

const blankLabel = (num) => {
  const q = qByNumber.value[num]
  if (!q) return String(num)
  if (selectedMap[q.id] != null) {
    const opt = q.options[selectedMap[q.id]]
    return opt ? `${num}. ${short(opt)}` : String(num)
  }
  return String(num)
}

const blankClass = (num) => {
  const q = qByNumber.value[num]
  if (!q) return ''
  if (submitted.value && feedbackMap[q.id]) {
    return feedbackMap[q.id].isCorrect ? 'is-ok' : 'is-bad'
  }
  const classes = []
  if (selectedMap[q.id] != null) classes.push('is-picked')
  if (focusNumber.value === num) classes.push('is-active')
  return classes.join(' ')
}

const resultClass = (id) => {
  if (!submitted.value || !feedbackMap[id]) return ''
  return feedbackMap[id].isCorrect ? 'is-correct' : 'is-wrong'
}

const setAnswer = (id, value) => {
  selectedMap[id] = value
  const q = (props.questions || []).find((x) => x.id === id)
  if (q) focusNumber.value = q.number
}

/** 点击文中空格：直接弹出选项，不跳转下方列表 */
const focusBlank = (num) => {
  const q = qByNumber.value[num]
  if (!q) {
    ElMessage.warning(`未找到空格 ${num} 对应题目`)
    return
  }
  focusNumber.value = num
  pendingIndex.value = selectedMap[q.id] ?? null
  drawerOpen.value = true
}

const confirmDrawer = () => {
  const q = activeQ.value
  if (!q || pendingIndex.value === null || pendingIndex.value === undefined) return
  selectedMap[q.id] = pendingIndex.value
  drawerOpen.value = false
}

const onSubmit = async () => {
  const unanswered = (props.questions || []).filter((q) => selectedMap[q.id] === undefined || selectedMap[q.id] === null)
  if (unanswered.length) {
    ElMessage.warning(`还有 ${unanswered.length} 个空格未作答，请全部选完再提交`)
    return
  }
  submitting.value = true
  try {
    const answers = (props.questions || []).map((q) => ({
      questionId: q.id,
      selectedIndex: selectedMap[q.id],
    }))
    const res = await props.submitFn(answers)
    correctCount.value = res.correctCount ?? 0
    scorePercent.value = res.scorePercent ?? 0
    Object.keys(feedbackMap).forEach((k) => delete feedbackMap[k])
    for (const r of res.results || []) {
      const id = r.questionId || r.QuestionId
      feedbackMap[id] = {
        isCorrect: !!(r.isCorrect ?? r.IsCorrect),
        correctAnswer: r.correctAnswer ?? r.CorrectAnswer,
        explanation: r.explanation ?? r.Explanation,
      }
    }
    submitted.value = true
    emit('submitted', res)
  } catch {
    /* */
  } finally {
    submitting.value = false
  }
}

const onReset = () => {
  clearState()
  emit('reset')
}
</script>

<style scoped>
.cloze-panel { padding: 18px; margin-top: 16px; }
.cloze-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 12px;
  margin-bottom: 14px;
}
.cloze-title { margin: 0; font-size: 17px; font-weight: 650; }
.cloze-sub { margin: 6px 0 0; font-size: 13px; color: var(--le-text-muted); }

.passage {
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
  line-height: 2;
  font-size: 15.5px;
  color: var(--le-text);
  white-space: pre-wrap;
  word-break: break-word;
}

.blank {
  display: inline-flex;
  align-items: center;
  margin: 0 3px;
  padding: 0 10px;
  min-width: 36px;
  height: 28px;
  border-radius: 8px;
  border: 1.5px dashed var(--le-primary, #2563eb);
  background: #eff6ff;
  color: var(--le-primary, #2563eb);
  font-weight: 650;
  font-size: 13px;
  cursor: pointer;
  vertical-align: middle;
}
.blank.is-picked { border-style: solid; }
.blank.is-active { box-shadow: 0 0 0 2px rgba(37, 99, 235, 0.25); }
.blank.is-ok {
  border-style: solid;
  border-color: #16a34a;
  background: #f0fdf4;
  color: #15803d;
}
.blank.is-bad {
  border-style: solid;
  border-color: #dc2626;
  background: #fef2f2;
  color: #b91c1c;
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
.quiz-item.is-focus {
  border-color: #93c5fd;
  box-shadow: 0 0 0 2px rgba(37, 99, 235, 0.08);
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
}
.quiz-num { margin-right: 6px; color: var(--le-primary, #2563eb); }
.quiz-stem-muted { color: var(--le-text-muted); font-weight: 500; }
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
.ok { color: #16a34a; font-weight: 600; }
.bad { color: #dc2626; font-weight: 600; }
.answer-key { color: var(--le-text); }
.explain { margin: 4px 0 0; color: var(--le-text-muted); line-height: 1.55; }

.submit-row {
  margin-top: 18px;
  display: flex;
  justify-content: center;
}

.drawer-body { padding: 0 8px 16px; }
.drawer-opts { width: 100%; }
.drawer-actions {
  margin-top: 16px;
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}
</style>
