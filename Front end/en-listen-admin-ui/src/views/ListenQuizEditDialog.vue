<template>
  <el-dialog
    :model-value="modelValue"
    title="编辑听力在线做题"
    width="960px"
    top="3vh"
    destroy-on-close
    @update:model-value="emit('update:modelValue', $event)"
  >
    <p class="album-name">{{ albumName }}</p>
    <p class="tip">
      按四六级结构录入：先选 Section A / B / C，再在该 Section 下添加多段材料。
      若录错分区，可在材料里用「所属 Section」直接调整。
    </p>

    <el-tabs v-model="activeGroup" type="card" class="group-tabs">
      <el-tab-pane
        v-for="g in GROUP_NAMES"
        :key="g"
        :label="`${g}（${passagesOf(g).length}）`"
        :name="g"
      />
    </el-tabs>

    <div class="toolbar">
      <el-button type="primary" @click="addPassage">在 {{ activeGroup }} 添加材料</el-button>
    </div>

    <el-collapse v-if="passagesOf(activeGroup).length" v-model="activeNames">
      <el-collapse-item
        v-for="(sec, sIdx) in passagesOf(activeGroup)"
        :key="sec._key"
        :name="sec._key"
      >
        <template #title>
          <div class="collapse-title">
            <el-tag size="small" type="warning">{{ sec.groupName }}</el-tag>
            <span>{{ sec.title || `材料 ${sIdx + 1}` }}</span>
            <el-tag size="small" type="info">{{ sec.questions.length }} 题</el-tag>
            <el-button type="danger" link size="small" @click.stop="removePassage(sec)">删除</el-button>
          </div>
        </template>

        <el-form label-position="top" class="sec-form">
          <el-form-item label="所属 Section">
            <div class="group-move-row">
              <el-select
                :model-value="sec.groupName"
                style="width: 180px"
                @update:model-value="(v) => changePassageGroup(sec, v)"
              >
                <el-option v-for="g in GROUP_NAMES" :key="g" :label="g" :value="g" />
              </el-select>
              <span class="hint-sm">录错分区时可直接改到 A / B / C，改完记得点下方「保存全部」</span>
            </div>
          </el-form-item>
          <el-form-item label="材料标题">
            <el-input
              v-model="sec.title"
              :placeholder="passagePlaceholder(sec.groupName, sIdx)"
            />
          </el-form-item>
          <el-form-item label="本段原文">
            <el-input v-model="sec.transcript" type="textarea" :rows="5" placeholder="粘贴本段听力原文" />
          </el-form-item>
          <el-form-item label="本段音频（可选）">
            <div class="audio-row">
              <span class="audio-url">{{ sec.audioUrl || '尚未上传' }}</span>
              <el-upload
                v-if="sec.id"
                :show-file-list="false"
                accept=".mp3,.wav,.m4a,audio/*"
                :http-request="(opt) => uploadAudio(sec, opt)"
              >
                <el-button size="small">上传音频</el-button>
              </el-upload>
              <span v-else class="hint-sm">先保存后再上传本段音频</span>
            </div>
          </el-form-item>

          <div class="q-toolbar">
            <strong>题目</strong>
            <el-button size="small" @click="addQuestion(sec)">添加题目</el-button>
          </div>

          <div v-for="(q, qIdx) in sec.questions" :key="q._key" class="q-card">
            <div class="q-head">
              <span>第 {{ qIdx + 1 }} 题</span>
              <el-button type="danger" link size="small" @click="sec.questions.splice(qIdx, 1)">删除</el-button>
            </div>
            <el-form-item label="题号">
              <el-input-number v-model="q.number" :min="1" :max="200" />
            </el-form-item>
            <el-form-item label="题干（可空）">
              <el-input v-model="q.stem" placeholder="可选，如 What does the man mean?" />
            </el-form-item>
            <el-form-item
              v-for="(opt, oIdx) in q.options"
              :key="oIdx"
              :label="`选项 ${String.fromCharCode(65 + oIdx)}`"
            >
              <div class="opt-row">
                <el-radio v-model="q.correctAnswer" :label="oIdx">正确答案</el-radio>
                <el-input v-model="q.options[oIdx]" :placeholder="`${String.fromCharCode(65 + oIdx)}) ...`" />
              </div>
            </el-form-item>
            <el-form-item label="解析（可空）">
              <el-input v-model="q.explanation" type="textarea" :rows="2" />
            </el-form-item>
          </div>
        </el-form>
      </el-collapse-item>
    </el-collapse>

    <el-empty
      v-else
      :description="`${activeGroup} 还没有材料，点击上方添加（如 News Report / Long Conversation / Passage）`"
      :image-size="64"
    />

    <template #footer>
      <el-button @click="emit('update:modelValue', false)">取消</el-button>
      <el-button type="primary" :loading="saving" @click="save">保存全部</el-button>
    </template>
  </el-dialog>
</template>

<script setup>
import { ref, watch } from 'vue'
import { ElMessage } from 'element-plus'
import { getQuizSections, saveQuizSections, uploadSectionAudio, moveQuizSectionGroup } from '../api/Admin'

const GROUP_NAMES = ['Section A', 'Section B', 'Section C']

const props = defineProps({
  modelValue: Boolean,
  albumId: { type: String, default: '' },
  albumName: { type: String, default: '' },
})
const emit = defineEmits(['update:modelValue', 'saved'])

const sections = ref([])
const activeGroup = ref('Section A')
const activeNames = ref([])
const saving = ref(false)
let keySeq = 1
const nextKey = () => `k${keySeq++}`

const emptyQuestion = (n = 1) => ({
  _key: nextKey(),
  number: n,
  stem: '',
  options: ['', '', '', ''],
  correctAnswer: 0,
  explanation: '',
})

/** 显式 groupName 优先；否则按标题启发式推断（兼容旧数据） */
const inferGroup = (rawGroup, title) => {
  const fromField = normalizeGroup(rawGroup)
  if (fromField) return fromField

  const t = String(title || '')
  const m = t.match(/section\s*([abc])/i)
  if (m) return `Section ${m[1].toUpperCase()}`

  // 常见四六级材料名
  if (/news\s*report|short\s*news/i.test(t)) return 'Section A'
  if (/long\s*conversation|conversation|dialogue/i.test(t)) return 'Section B'
  if (/passage|lecture|talk/i.test(t)) return 'Section C'

  return 'Section A'
}

const normalizeGroup = (name) => {
  const raw = String(name || '').trim()
  if (!raw) return ''
  const hit = GROUP_NAMES.find((g) => g.toLowerCase() === raw.toLowerCase())
  if (hit) return hit
  const compact = raw.replace(/\s+/g, '').toUpperCase()
  if (compact === 'A' || compact === 'SECTIONA') return 'Section A'
  if (compact === 'B' || compact === 'SECTIONB') return 'Section B'
  if (compact === 'C' || compact === 'SECTIONC') return 'Section C'
  return ''
}

const defaultPassageTitle = (group, index) => {
  if (group === 'Section A') return `News Report ${index}`
  if (group === 'Section B') return `Long Conversation ${index}`
  return `Passage ${index}`
}

const passagePlaceholder = (group, sIdx) => defaultPassageTitle(group, sIdx + 1)

const emptyPassage = (group, n = 1) => ({
  _key: nextKey(),
  id: null,
  groupName: group,
  title: defaultPassageTitle(group, n),
  transcript: '',
  audioUrl: '',
  sequenceNumber: n,
  questions: [emptyQuestion(1)],
})

const passagesOf = (group) => sections.value.filter((s) => s.groupName === group)

const load = async () => {
  if (!props.albumId) return
  try {
    const list = await getQuizSections(props.albumId)
    if (!list?.length) {
      sections.value = []
      activeGroup.value = 'Section A'
      activeNames.value = []
      return
    }
    sections.value = list.map((s, i) => {
      const title = s.title || s.Title || `材料 ${i + 1}`
      const groupName = inferGroup(s.groupName || s.GroupName, title)
      return {
        _key: nextKey(),
        id: s.id || s.Id || null,
        groupName,
        title,
        transcript: s.transcript || s.Transcript || '',
        audioUrl: s.audioUrl || s.AudioUrl || '',
        sequenceNumber: s.sequenceNumber || s.SequenceNumber || i + 1,
        questions: (s.questions || s.Questions || []).length
          ? (s.questions || s.Questions || []).map((q, qi) => {
              const opts = [...(q.options || q.Options || [])]
              while (opts.length < 4) opts.push('')
              return {
                _key: nextKey(),
                number: q.number ?? q.Number ?? qi + 1,
                stem: q.stem || q.Stem || '',
                options: opts.slice(0, 4),
                correctAnswer: q.correctAnswer ?? q.CorrectAnswer ?? 0,
                explanation: q.explanation || q.Explanation || '',
              }
            })
          : [emptyQuestion(1)],
      }
    })
    activeGroup.value =
      GROUP_NAMES.find((g) => passagesOf(g).length) || 'Section A'
    activeNames.value = passagesOf(activeGroup.value).map((s) => s._key)
  } catch {
    sections.value = []
  }
}

watch(
  () => [props.modelValue, props.albumId],
  ([open]) => {
    if (open) load()
  }
)

watch(activeGroup, (g) => {
  activeNames.value = passagesOf(g).map((s) => s._key)
})

const addPassage = () => {
  const group = activeGroup.value
  const n = passagesOf(group).length + 1
  const sec = emptyPassage(group, n)
  sections.value.push(sec)
  activeNames.value = [...activeNames.value, sec._key]
}

const removePassage = (sec) => {
  const idx = sections.value.findIndex((s) => s._key === sec._key)
  if (idx >= 0) sections.value.splice(idx, 1)
}

const changePassageGroup = async (sec, nextGroup) => {
  const group = normalizeGroup(nextGroup) || nextGroup
  if (!group || group === sec.groupName) return

  const prev = sec.groupName
  sec.groupName = group
  activeGroup.value = group
  activeNames.value = [sec._key]

  // 已落库的材料可先即时改库，避免只改本地忘保存
  if (sec.id) {
    try {
      await moveQuizSectionGroup(sec.id, group)
      ElMessage.success(`「${sec.title}」已从 ${prev} 调整到 ${group}`)
      emit('saved')
    } catch {
      sec.groupName = prev
      activeGroup.value = prev
      ElMessage.error('调整 Section 失败，请稍后重试或点「保存全部」')
    }
  } else {
    ElMessage.success(`已改到 ${group}，请记得保存全部`)
  }
}

const addQuestion = (sec) => {
  sec.questions.push(emptyQuestion(sec.questions.length + 1))
}

const uploadAudio = async (sec, opt) => {
  try {
    const res = await uploadSectionAudio(sec.id, opt.file)
    sec.audioUrl = res.url || res.Url || sec.audioUrl
    ElMessage.success('音频上传成功')
  } catch {
    /* */
  }
}

const save = async () => {
  if (!sections.value.length) {
    ElMessage.warning('请至少在某一 Section 下添加一段材料')
    return
  }
  for (const sec of sections.value) {
    if (!sec.groupName || !GROUP_NAMES.includes(sec.groupName)) {
      ElMessage.warning(`「${sec.title}」所属 Section 无效`)
      return
    }
    if (!sec.title?.trim()) {
      ElMessage.warning(`${sec.groupName} 有材料标题为空`)
      return
    }
    for (const q of sec.questions) {
      const filled = (q.options || []).filter((o) => (o || '').trim())
      if (filled.length < 2) {
        ElMessage.warning(`「${sec.groupName} · ${sec.title}」有题目选项不足 2 个`)
        return
      }
    }
  }

  saving.value = true
  try {
    let seq = 1
    const ordered = []
    for (const g of GROUP_NAMES) {
      for (const sec of passagesOf(g)) {
        ordered.push({
          groupName: g,
          title: sec.title.trim(),
          transcript: sec.transcript || '',
          audioUrl: sec.audioUrl || null,
          sequenceNumber: seq++,
          questions: sec.questions.map((q, qi) => ({
            number: q.number || qi + 1,
            stem: q.stem || '',
            options: (q.options || []).map((o) => o.trim()).filter(Boolean),
            correctAnswer: q.correctAnswer,
            explanation: q.explanation || '',
            sequenceNumber: qi + 1,
          })),
        })
      }
    }
    await saveQuizSections(props.albumId, ordered)
    ElMessage.success('保存成功。可再为各段上传音频。')
    emit('saved')
    await load()
  } catch {
    /* */
  } finally {
    saving.value = false
  }
}
</script>

<style scoped>
.album-name { margin: 0 0 6px; font-weight: 600; }
.tip { margin: 0 0 12px; font-size: 13px; color: #606266; line-height: 1.5; }
.group-tabs { margin-bottom: 8px; }
.toolbar { margin-bottom: 12px; }
.collapse-title {
  display: flex;
  align-items: center;
  gap: 10px;
  width: 100%;
  padding-right: 12px;
}
.sec-form { padding-right: 4px; }
.group-move-row {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
}
.audio-row {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
}
.audio-url { font-size: 12px; color: #909399; word-break: break-all; }
.hint-sm { font-size: 12px; color: #909399; }
.q-toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin: 8px 0 12px;
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
