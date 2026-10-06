<template>
  <el-dialog
    :model-value="modelValue"
    :title="`编辑 · ${paperTitle}`"
    width="960px"
    top="3vh"
    destroy-on-close
    @update:model-value="emit('update:modelValue', $event)"
  >
    <el-tabs v-model="tab">
      <el-tab-pane
        v-for="sec in sections"
        :key="sec._key"
        :label="sec.sectionType === 'cloze' ? '完形填空' : '阅读理解'"
        :name="sec._key"
      >
        <el-form label-position="top">
          <el-form-item :label="sec.sectionType === 'cloze' ? '完形原文（空格用 __1__ __2__ 标记）' : '阅读原文'">
            <el-input v-model="sec.passage" type="textarea" :rows="8" />
          </el-form-item>

          <div class="q-toolbar">
            <strong>{{ sec.sectionType === 'cloze' ? '空格选项' : '阅读题目' }}</strong>
            <el-button size="small" @click="addQuestion(sec)">添加{{ sec.sectionType === 'cloze' ? '空格' : '题目' }}</el-button>
          </div>

          <div v-for="(q, qIdx) in sec.questions" :key="q._key" class="q-card">
            <div class="q-head">
              <span>{{ sec.sectionType === 'cloze' ? `空格 ${q.number}` : `第 ${qIdx + 1} 题` }}</span>
              <el-button type="danger" link size="small" @click="sec.questions.splice(qIdx, 1)">删除</el-button>
            </div>
            <el-form-item label="题号">
              <el-input-number v-model="q.number" :min="1" :max="100" />
            </el-form-item>
            <el-form-item v-if="sec.sectionType === 'reading'" label="题干">
              <el-input v-model="q.stem" type="textarea" :rows="2" placeholder="题目文字" />
            </el-form-item>
            <el-form-item
              v-for="(opt, oIdx) in q.options"
              :key="oIdx"
              :label="`选项 ${String.fromCharCode(65 + oIdx)}`"
            >
              <div class="opt-row">
                <el-radio v-model="q.correctAnswer" :label="oIdx">正确答案</el-radio>
                <el-input v-model="q.options[oIdx]" />
              </div>
            </el-form-item>
            <el-form-item label="解析（可空）">
              <el-input v-model="q.explanation" type="textarea" :rows="2" />
            </el-form-item>
          </div>
        </el-form>
      </el-tab-pane>
    </el-tabs>

    <template #footer>
      <el-button @click="emit('update:modelValue', false)">取消</el-button>
      <el-button type="primary" :loading="saving" @click="save">保存</el-button>
    </template>
  </el-dialog>
</template>

<script setup>
import { ref, watch } from 'vue'
import { ElMessage } from 'element-plus'
import { getKaoyanPaperFull, saveKaoyanPaperContent } from '../api/Admin'

const props = defineProps({
  modelValue: Boolean,
  paperId: { type: String, default: '' },
  paperTitle: { type: String, default: '' },
})
const emit = defineEmits(['update:modelValue', 'saved'])

const sections = ref([])
const tab = ref('')
const saving = ref(false)
let keySeq = 1
const nextKey = () => `k${keySeq++}`

const emptyQ = (n = 1) => ({
  _key: nextKey(),
  number: n,
  stem: '',
  options: ['', '', '', ''],
  correctAnswer: 0,
  explanation: '',
})

const ensureSections = (list) => {
  const byType = Object.fromEntries((list || []).map((s) => [s.sectionType, s]))
  const make = (type, title, existing) => {
    const qs = (existing?.questions || []).map((q, i) => {
      const opts = [...(q.options || [])]
      while (opts.length < 4) opts.push('')
      return {
        _key: nextKey(),
        number: q.number || i + 1,
        stem: q.stem || '',
        options: opts.slice(0, 4),
        correctAnswer: q.correctAnswer ?? 0,
        explanation: q.explanation || '',
      }
    })
    return {
      _key: nextKey(),
      sectionType: type,
      title,
      passage: existing?.passage || '',
      sequenceNumber: type === 'cloze' ? 1 : 2,
      questions: qs.length ? qs : [emptyQ(1)],
    }
  }
  return [
    make('cloze', '完形填空', byType.cloze),
    make('reading', '阅读理解', byType.reading),
  ]
}

const load = async () => {
  if (!props.paperId) return
  try {
    const full = await getKaoyanPaperFull(props.paperId)
    sections.value = ensureSections(full.sections || full.Sections || [])
    tab.value = sections.value[0]?._key || ''
  } catch {
    sections.value = ensureSections([])
    tab.value = sections.value[0]?._key || ''
  }
}

watch(
  () => [props.modelValue, props.paperId],
  ([open]) => {
    if (open) load()
  }
)

const addQuestion = (sec) => {
  const n = sec.questions.length + 1
  sec.questions.push(emptyQ(n))
}

const save = async () => {
  for (const sec of sections.value) {
    for (const q of sec.questions) {
      const filled = (q.options || []).filter((o) => (o || '').trim())
      if (filled.length < 2) {
        ElMessage.warning(`${sec.title || sec.sectionType} 有题目选项不足 2 个`)
        return
      }
      if (sec.sectionType === 'reading' && !(q.stem || '').trim()) {
        ElMessage.warning('阅读理解题干不能为空')
        return
      }
    }
  }
  saving.value = true
  try {
    await saveKaoyanPaperContent({
      paperId: props.paperId,
      sections: sections.value.map((sec, i) => ({
        sectionType: sec.sectionType,
        title: sec.title,
        passage: sec.passage || '',
        sequenceNumber: i + 1,
        questions: sec.questions.map((q, qi) => ({
          number: q.number || qi + 1,
          stem: q.stem || '',
          options: (q.options || []).map((o) => o.trim()).filter(Boolean),
          correctAnswer: q.correctAnswer,
          explanation: q.explanation || '',
          sequenceNumber: qi + 1,
        })),
      })),
    })
    ElMessage.success('保存成功')
    emit('saved')
    emit('update:modelValue', false)
  } catch {
    /* */
  } finally {
    saving.value = false
  }
}
</script>

<style scoped>
.q-toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin: 4px 0 12px;
}
.q-card {
  border: 1px solid #ebeef5;
  border-radius: 8px;
  padding: 12px;
  margin-bottom: 12px;
  background: #fafafa;
}
.q-head {
  display: flex;
  justify-content: space-between;
  margin-bottom: 8px;
  font-weight: 600;
}
.opt-row {
  display: flex;
  align-items: center;
  gap: 12px;
  width: 100%;
}
.opt-row .el-input { flex: 1; }
</style>
