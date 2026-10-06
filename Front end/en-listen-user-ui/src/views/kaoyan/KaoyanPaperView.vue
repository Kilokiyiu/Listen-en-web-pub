<template>
  <PageShell :title="paperTitle" back-label="返回列表">
    <div v-if="loading" class="le-loading-wrap">
      <el-icon class="is-loading" :size="28"><Loading /></el-icon>
      <span>加载中...</span>
    </div>

    <template v-else-if="paper">
      <div class="section-tabs">
        <el-radio-group v-model="activeSectionId" size="default">
          <el-radio-button
            v-for="s in sections"
            :key="s.id"
            :label="s.id"
          >
            {{ s.title }}
          </el-radio-button>
        </el-radio-group>
      </div>

      <template v-if="activeSection">
        <ClozeQuizPanel
          v-if="activeSection.sectionType === 'cloze'"
          :title="activeSection.title"
          :passage="activeSection.passage"
          :questions="activeSection.questions"
          :submit-fn="(answers) => handleSubmit(answers, activeSection.id)"
          @submitted="onSubmitted"
        />
        <OnlineQuizPanel
          v-else
          :title="activeSection.title"
          subtitle="阅读文章后作答，全部答完提交后查看正确答案"
          :passage="activeSection.passage"
          :questions="activeSection.questions"
          :submit-fn="(answers) => handleSubmit(answers, activeSection.id)"
          @submitted="onSubmitted"
        />
      </template>

      <el-empty
        v-else-if="!sections.length"
        description="该卷暂无内容，请管理员在后台表单中录入"
      />
    </template>

    <el-empty v-else description="试卷不存在" />
  </PageShell>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import { ElMessage } from 'element-plus'
import PageShell from '../../components/PageShell.vue'
import OnlineQuizPanel from '../../components/OnlineQuizPanel.vue'
import ClozeQuizPanel from '../../components/ClozeQuizPanel.vue'
import { getKaoyanPaperDetail, submitKaoyanAnswers } from '../../api/Kaoyan.js'
import { recordStudyActivity } from '../../api/Study.js'
import { trackEvent } from '../../api/Analytics.js'

const route = useRoute()
const paperId = route.query.paperId
const loading = ref(true)
const paper = ref(null)
const sections = ref([])
const activeSectionId = ref('')
const sessionStartedAt = Date.now()

const paperTitle = computed(() => paper.value?.title || '考研英语')
const activeSection = computed(() =>
  sections.value.find((s) => s.id === activeSectionId.value) || null
)

const normalize = (detail) => {
  const secs = (detail.sections || detail.Sections || []).map((s) => ({
    id: s.id || s.Id,
    sectionType: s.sectionType || s.SectionType,
    title: s.title || s.Title,
    passage: s.passage || s.Passage || '',
    questions: (s.questions || s.Questions || []).map((q) => ({
      id: q.id || q.Id,
      number: q.number ?? q.Number,
      stem: q.stem || q.Stem || '',
      options: q.options || q.Options || [],
    })),
  }))
  return {
    paper: {
      id: detail.id || detail.Id,
      year: detail.year ?? detail.Year,
      series: detail.series || detail.Series,
      seriesName: detail.seriesName || detail.SeriesName,
      title: detail.title || detail.Title,
    },
    sections: secs,
  }
}

const handleSubmit = async (answers, sectionId) => {
  const res = await submitKaoyanAnswers(paperId, answers, sectionId)
  return {
    correctCount: res.correctCount ?? res.CorrectCount ?? 0,
    scorePercent: res.scorePercent ?? res.ScorePercent ?? 0,
    results: res.results || res.Results || [],
  }
}

const recordKaoyan = async (extraTitle) => {
  if (!localStorage.getItem('token')) return
  try {
    await recordStudyActivity({
      activityType: 'kaoyan',
      contentId: String(paperId),
      title: `${paperTitle.value}${extraTitle ? ` · ${extraTitle}` : ''}`,
      category: paper.value?.seriesName || '考研',
      durationSeconds: Math.round((Date.now() - sessionStartedAt) / 1000),
    })
  } catch {
    /* */
  }
}

const onSubmitted = async (res) => {
  trackEvent('kaoyan_quiz_submit', `/kaoyan/paper?paperId=${paperId}`)
  await recordKaoyan(activeSection.value?.title)
  ElMessage.success(`答题完成：${res.correctCount} 题正确`)
}

onMounted(async () => {
  if (!paperId) {
    loading.value = false
    return
  }
  try {
    const detail = await getKaoyanPaperDetail(paperId)
    const n = normalize(detail)
    paper.value = n.paper
    sections.value = n.sections
    activeSectionId.value = n.sections[0]?.id || ''
  } catch {
    paper.value = null
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.section-tabs {
  margin-bottom: 8px;
  max-width: 720px;
}
</style>
