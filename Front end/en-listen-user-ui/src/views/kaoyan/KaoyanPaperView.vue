<template>
  <PageShell :title="paperTitle" back-label="返回列表">
    <div v-if="loading" class="le-loading-wrap">
      <el-icon class="is-loading" :size="28"><Loading /></el-icon>
      <span>加载中...</span>
    </div>

    <template v-else-if="paper">
      <div v-if="paperFileUrl || answerFileUrl" class="pdf-download-bar">
        <el-button
          v-if="paperFileUrl"
          type="primary"
          round
          class="pdf-action-btn"
          @click="downloadPdf('paper')"
        >
          <el-icon><Download /></el-icon>
          下载原题
        </el-button>
        <el-button
          v-if="answerFileUrl"
          type="success"
          round
          class="pdf-action-btn"
          @click="downloadPdf('answer')"
        >
          <el-icon><Download /></el-icon>
          下载解析
        </el-button>
      </div>

      <p v-if="sections.length" class="progress-tip">
        先做完全部分区题目（进度自动保存），再点下方「提交整卷并对答案」统一核对。
      </p>

      <QuizSectionTabs
        v-if="sectionTabs.length"
        v-model="activeSectionId"
        :items="sectionTabs"
        label="题目分区"
        aria-label="考研大题切换"
      />

      <template v-if="sections.length">
        <div
          v-for="sec in sections"
          :key="sec.id"
          v-show="sec.id === activeSectionId"
        >
          <ClozeQuizPanel
            v-if="sec.sectionType === 'cloze'"
            :ref="(el) => setPanelRef(sec.id, el)"
            :title="sec.title"
            :passage="sec.passage"
            :questions="sec.questions"
            :persist-key="persistKeyFor(sec.id)"
            defer-check
            @progress="(p) => onSectionProgress(sec.id, p)"
          />
          <OnlineQuizPanel
            v-else
            :ref="(el) => setPanelRef(sec.id, el)"
            :title="sec.title"
            subtitle="阅读后作答；可随时切换其它分区，做完全卷再统一对答案"
            :passage="sec.passage"
            :questions="sec.questions"
            :persist-key="persistKeyFor(sec.id)"
            defer-check
            @progress="(p) => onSectionProgress(sec.id, p)"
          />
        </div>
      </template>

      <el-empty
        v-else
        description="该卷暂无内容，请管理员在后台表单中录入"
      />

      <div v-if="sections.length" class="review-summary le-card">
        <div class="review-summary__head">
          <h3 class="review-summary__title">整卷对答案</h3>
          <p class="review-summary__sub">
            <template v-if="paperGraded">
              合计 {{ paperCorrect }}/{{ paperTotal }} 题正确（{{ paperScorePercent }}%）
            </template>
            <template v-else>
              已选 {{ answeredTotal }}/{{ paperTotal }} 题
              <template v-if="!allQuestionsAnswered"> · 全部选完后可提交</template>
            </template>
          </p>
        </div>
        <ul class="review-summary__list">
          <li
            v-for="item in reviewRows"
            :key="item.id"
            class="review-summary__row"
            :class="{
              'is-done': item.graded,
              'is-ready': !item.graded && item.complete,
            }"
          >
            <div class="review-summary__meta">
              <span class="review-summary__name">{{ item.label }}</span>
              <span v-if="item.graded" class="review-summary__score">
                {{ item.correctCount }}/{{ item.total }} · {{ item.scorePercent }}%
              </span>
              <span v-else-if="item.complete" class="review-summary__ready">已答完</span>
              <span v-else class="review-summary__pending">
                已选 {{ item.answered }}/{{ item.total }}
              </span>
            </div>
            <el-button type="primary" link size="small" @click="jumpToSection(item.id)">
              {{ item.graded ? '查看对错' : '去作答' }}
            </el-button>
          </li>
        </ul>
        <div class="review-summary__actions">
          <el-button
            v-if="!paperGraded"
            type="primary"
            round
            size="large"
            class="review-summary__submit"
            :loading="submittingAll"
            :disabled="!allQuestionsAnswered"
            @click="submitWholePaper"
          >
            提交整卷并对答案
          </el-button>
          <el-button
            v-else
            round
            size="large"
            class="review-summary__submit"
            @click="resetWholePaper"
          >
            整卷再做一次
          </el-button>
        </div>
      </div>

      <div v-if="paperFileUrl || answerFileUrl" class="pdf-section">
        <div class="pdf-actions">
          <el-button
            v-if="paperFileUrl"
            :type="activePdf === 'paper' ? 'primary' : 'default'"
            round
            plain
            class="pdf-action-btn"
            @click="togglePdf('paper')"
          >
            <el-icon><Document /></el-icon>
            {{ activePdf === 'paper' ? '隐藏原题' : '在线查看原题' }}
          </el-button>
          <el-button
            v-if="answerFileUrl"
            :type="activePdf === 'answer' ? 'success' : 'default'"
            round
            plain
            class="pdf-action-btn"
            @click="togglePdf('answer')"
          >
            <el-icon><DocumentChecked /></el-icon>
            {{ activePdf === 'answer' ? '隐藏解析' : '在线查看解析' }}
          </el-button>
        </div>

        <transition name="fade-slide">
          <div v-if="activePdf && currentPdfUrl" class="pdf-viewer le-card">
            <div class="pdf-header">
              <span class="pdf-title">{{ activePdf === 'paper' ? '原题 PDF' : '解析 PDF' }}</span>
              <el-button type="primary" link @click="downloadPdf(activePdf)">
                <el-icon><Download /></el-icon>
                下载
              </el-button>
            </div>
            <PdfViewer :key="currentPdfUrl" :src="currentPdfUrl" />
            <p class="pdf-fallback-tip">
              也可
              <a :href="currentPdfUrl" target="_blank" rel="noopener noreferrer">在新窗口打开</a>
              或点击上方下载。
            </p>
          </div>
        </transition>
      </div>
    </template>

    <el-empty v-else description="试卷不存在" />
  </PageShell>
</template>

<script setup>
import { computed, onMounted, reactive, ref } from 'vue'
import { useRoute } from 'vue-router'
import { ElMessage } from 'element-plus'
import { Document, DocumentChecked, Download, Loading } from '@element-plus/icons-vue'
import PageShell from '../../components/PageShell.vue'
import QuizSectionTabs from '../../components/QuizSectionTabs.vue'
import OnlineQuizPanel from '../../components/OnlineQuizPanel.vue'
import ClozeQuizPanel from '../../components/ClozeQuizPanel.vue'
import PdfViewer from '../../components/PdfViewer.vue'
import { getKaoyanPaperDetail, submitKaoyanAnswers } from '../../api/Kaoyan.js'
import { recordStudyActivity } from '../../api/Study.js'
import { trackEvent } from '../../api/Analytics.js'
import { promptFeedbackGift } from '../../utils/promptFeedbackGift.js'
import { getPageCache, setPageCache } from '../../utils/pageCache.js'
import { loadQuizProgress } from '../../utils/quizProgressStore.js'

const route = useRoute()
const paperId = route.query.paperId
const loading = ref(true)
const paper = ref(null)
const sections = ref([])
const activeSectionId = ref('')
const paperFileUrl = ref('')
const answerFileUrl = ref('')
const activePdf = ref('')
/** sectionId -> { answered, total, complete, graded, correctCount?, scorePercent? } */
const sectionProgress = reactive({})
const panelRefs = {}
const submittingAll = ref(false)
const sessionStartedAt = Date.now()

const paperTitle = computed(() => paper.value?.title || '考研英语')

const persistKeyFor = (sectionId) =>
  paperId && sectionId ? `kaoyan:quiz:${paperId}:${sectionId}` : ''

const setPanelRef = (sectionId, el) => {
  if (el) panelRefs[sectionId] = el
  else delete panelRefs[sectionId]
}

const onSectionProgress = (sectionId, p) => {
  const prev = sectionProgress[sectionId] || {}
  const graded = !!p.graded
  sectionProgress[sectionId] = {
    answered: p.answered ?? prev.answered ?? 0,
    total: p.total ?? prev.total ?? 0,
    complete: !!p.complete || graded,
    graded,
    correctCount: graded ? prev.correctCount : undefined,
    scorePercent: graded ? prev.scorePercent : undefined,
  }
}

const hydrateSectionProgress = () => {
  for (const s of sections.value) {
    const data = loadQuizProgress(persistKeyFor(s.id))
    const total = s.questions?.length || 0
    if (!data) {
      sectionProgress[s.id] = { answered: 0, total, complete: false, graded: false }
      continue
    }
    const answeredMap = data.answers || data.selectedMap || {}
    const answered = Object.keys(answeredMap).length
    sectionProgress[s.id] = {
      answered,
      total,
      complete: total > 0 && answered >= total,
      graded: !!data.submitted,
      correctCount: data.submitted ? data.correctCount ?? 0 : undefined,
      scorePercent: data.submitted ? data.scorePercent ?? 0 : undefined,
    }
  }
}

const toFileUrl = (path) => (path ? `/api/kaoyan${path}` : '')

const currentPdfUrl = computed(() => {
  if (activePdf.value === 'paper') return paperFileUrl.value
  if (activePdf.value === 'answer') return answerFileUrl.value
  return ''
})

const formatSectionLabel = (s) => {
  if (s.sectionType === 'cloze') return '完形填空'
  const m =
    /Text\s*(\d+)/i.exec(s.title || '') ||
    /阅读\s*Text\s*(\d+)/i.exec(s.title || '') ||
    /阅读.*?(\d+)/.exec(s.title || '')
  if (m) return `阅读 ${m[1]}`
  return s.title || '阅读'
}

const sectionTabs = computed(() =>
  sections.value.map((s) => {
    const p = sectionProgress[s.id]
    let badge = s.questions?.length || undefined
    let title = s.title
    if (p?.graded) {
      badge = `${p.correctCount}/${p.total}`
      title = `${s.title || formatSectionLabel(s)} · ${badge}`
    } else if (p && p.total) {
      badge = `${p.answered}/${p.total}`
      title = `${s.title || formatSectionLabel(s)} · 已选 ${badge}`
    }
    return {
      value: s.id,
      label: formatSectionLabel(s),
      title,
      badge,
    }
  })
)

const reviewRows = computed(() =>
  sections.value.map((s) => {
    const p = sectionProgress[s.id] || {}
    return {
      id: s.id,
      label: formatSectionLabel(s),
      answered: p.answered ?? 0,
      total: p.total ?? s.questions?.length ?? 0,
      complete: !!p.complete,
      graded: !!p.graded,
      correctCount: p.correctCount ?? 0,
      scorePercent: p.scorePercent ?? 0,
    }
  })
)

const paperTotal = computed(() =>
  sections.value.reduce((sum, s) => sum + (s.questions?.length || 0), 0)
)

const answeredTotal = computed(() =>
  sections.value.reduce((sum, s) => sum + (sectionProgress[s.id]?.answered || 0), 0)
)

const quizSections = computed(() =>
  sections.value.filter((s) => (s.questions?.length || 0) > 0)
)

const allQuestionsAnswered = computed(
  () =>
    quizSections.value.length > 0 &&
    quizSections.value.every((s) => {
      const p = sectionProgress[s.id]
      return p && (p.complete || p.graded)
    })
)

const paperGraded = computed(
  () =>
    quizSections.value.length > 0 &&
    quizSections.value.every((s) => sectionProgress[s.id]?.graded)
)

const paperCorrect = computed(() =>
  sections.value.reduce((sum, s) => sum + (sectionProgress[s.id]?.correctCount || 0), 0)
)

const paperScorePercent = computed(() => {
  const t = paperTotal.value
  if (!t) return 0
  return Math.round((paperCorrect.value * 1000) / t) / 10
})

const jumpToSection = (sectionId) => {
  activeSectionId.value = sectionId
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

const togglePdf = (type) => {
  activePdf.value = activePdf.value === type ? '' : type
}

const pdfDownloadName = (kind) => {
  const title = (paperTitle.value || '考研英语').replace(/[\\/:*?"<>|]/g, '_')
  const suffix = kind === 'answer' ? '解析' : '原题'
  return `${title}-${suffix}.pdf`
}

const downloadPdf = (kind) => {
  const url = kind === 'answer' ? answerFileUrl.value : paperFileUrl.value
  if (!url) return
  const a = document.createElement('a')
  a.href = url
  a.download = pdfDownloadName(kind)
  a.target = '_blank'
  a.rel = 'noopener noreferrer'
  document.body.appendChild(a)
  a.click()
  a.remove()
}

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
    paperFileUrl: toFileUrl(detail.paperFileUrl || detail.PaperFileUrl),
    answerFileUrl: toFileUrl(detail.answerFileUrl || detail.AnswerFileUrl),
    sections: secs,
  }
}

const recordKaoyan = async () => {
  if (!localStorage.getItem('token')) return
  try {
    await recordStudyActivity({
      activityType: 'kaoyan',
      contentId: String(paperId),
      title: `${paperTitle.value} · 整卷`,
      category: paper.value?.seriesName || '考研',
      durationSeconds: Math.round((Date.now() - sessionStartedAt) / 1000),
    })
  } catch {
    /* */
  }
}

const submitWholePaper = async () => {
  if (!allQuestionsAnswered.value || paperGraded.value) return
  const missing = quizSections.value.filter((s) => !panelRefs[s.id]?.isComplete?.())
  if (missing.length) {
    ElMessage.warning(`还有分区未答完：${missing.map(formatSectionLabel).join('、')}`)
    jumpToSection(missing[0].id)
    return
  }

  const allAnswers = []
  for (const s of quizSections.value) {
    const panel = panelRefs[s.id]
    if (!panel?.getAnswers) continue
    allAnswers.push(...panel.getAnswers())
  }

  submittingAll.value = true
  try {
    const res = await submitKaoyanAnswers(paperId, allAnswers)
    const results = res.results || res.Results || []
    const byId = new Map(
      results.map((r) => {
        const id = r.questionId || r.QuestionId
        return [id, r]
      })
    )

    for (const s of quizSections.value) {
      const panel = panelRefs[s.id]
      const sectionResultsList = (s.questions || [])
        .map((q) => byId.get(q.id))
        .filter(Boolean)
      const correctCount = sectionResultsList.filter((r) => !!(r.isCorrect ?? r.IsCorrect)).length
      const total = s.questions?.length || 0
      const scorePercent = total ? Math.round((correctCount * 1000) / total) / 10 : 0
      panel?.applyGrade?.({
        correctCount,
        scorePercent,
        results: sectionResultsList,
      })
      sectionProgress[s.id] = {
        answered: total,
        total,
        complete: true,
        graded: true,
        correctCount,
        scorePercent,
      }
    }

    const total = res.total ?? paperTotal.value
    const correctCount = res.correctCount ?? res.CorrectCount ?? paperCorrect.value
    trackEvent('kaoyan_quiz_submit', `/kaoyan/paper?paperId=${paperId}`)
    await recordKaoyan()
    ElMessage.success(`整卷完成：${correctCount}/${total} 题正确`)
    promptFeedbackGift({
      source: `考研 · ${paperTitle.value} · 整卷`,
      context: paperTitle.value || '考研练习',
      delayMs: 600,
    })
  } catch {
    /* */
  } finally {
    submittingAll.value = false
  }
}

const resetWholePaper = () => {
  for (const s of sections.value) {
    panelRefs[s.id]?.resetAll?.()
    sectionProgress[s.id] = {
      answered: 0,
      total: s.questions?.length || 0,
      complete: false,
      graded: false,
    }
  }
  ElMessage.success('已清空整卷作答，可重新练习')
  if (sections.value[0]) jumpToSection(sections.value[0].id)
}

const applyDetail = (detail, { preferKeepSection = false } = {}) => {
  const n = normalize(detail)
  paper.value = n.paper
  sections.value = n.sections
  paperFileUrl.value = n.paperFileUrl
  answerFileUrl.value = n.answerFileUrl
  hydrateSectionProgress()
  if (!preferKeepSection || !n.sections.some((s) => s.id === activeSectionId.value)) {
    activeSectionId.value = n.sections[0]?.id || ''
  }
}

onMounted(async () => {
  if (!paperId) {
    loading.value = false
    return
  }
  const cacheKey = `kaoyan:paper:${paperId}`
  const cached = getPageCache(cacheKey)
  if (cached) {
    applyDetail(cached)
    loading.value = false
  }
  try {
    const detail = await getKaoyanPaperDetail(paperId)
    setPageCache(cacheKey, detail, 5 * 60 * 1000)
    applyDetail(detail, { preferKeepSection: !!cached })
  } catch {
    if (!cached) paper.value = null
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.progress-tip {
  margin: 0 0 12px;
  font-size: 13px;
  color: var(--le-text-muted);
  line-height: 1.5;
}

.review-summary {
  margin-top: 24px;
  padding: 16px 18px 14px;
}

.review-summary__head {
  margin-bottom: 12px;
}

.review-summary__title {
  margin: 0;
  font-size: 16px;
  font-weight: 650;
  color: var(--le-text);
}

.review-summary__sub {
  margin: 6px 0 0;
  font-size: 13px;
  color: var(--le-text-muted);
}

.review-summary__list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.review-summary__row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 10px 12px;
  border-radius: 10px;
  border: 1px solid var(--le-border);
  background: var(--le-bg-muted);
}

.review-summary__row.is-done {
  border-color: color-mix(in srgb, var(--el-color-success) 35%, var(--le-border));
}

.review-summary__row.is-ready {
  border-color: color-mix(in srgb, var(--le-primary, #2563eb) 30%, var(--le-border));
}

.review-summary__meta {
  display: flex;
  flex-wrap: wrap;
  align-items: baseline;
  gap: 8px 12px;
  min-width: 0;
}

.review-summary__name {
  font-weight: 600;
  color: var(--le-text);
}

.review-summary__score {
  font-size: 13px;
  color: var(--el-color-success);
  font-weight: 600;
}

.review-summary__ready {
  font-size: 13px;
  color: var(--le-primary, #2563eb);
  font-weight: 600;
}

.review-summary__pending {
  font-size: 13px;
  color: var(--le-text-muted);
}

.review-summary__actions {
  margin-top: 16px;
  display: flex;
  justify-content: center;
}

.review-summary__submit {
  min-width: 200px;
}

.pdf-download-bar {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 12px;
  margin: 0 0 18px;
}

.pdf-section {
  margin-top: 28px;
  padding-top: 20px;
  border-top: 1px solid var(--le-border);
}

.pdf-actions {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 12px;
}

.pdf-action-btn {
  min-width: 132px;
}

.pdf-viewer {
  margin-top: 16px;
  overflow: hidden;
}

.pdf-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 12px 16px;
  background: var(--le-bg-muted);
  border-bottom: 1px solid var(--le-border);
}

.pdf-title {
  font-weight: 600;
  color: var(--le-text);
}

.pdf-fallback-tip {
  margin: 0;
  padding: 10px 16px 14px;
  font-size: 12px;
  color: var(--le-text-muted);
  text-align: center;
  border-top: 1px solid var(--le-border);
}

.pdf-fallback-tip a {
  color: var(--le-primary, #2563eb);
  text-decoration: none;
}

.pdf-fallback-tip a:hover {
  text-decoration: underline;
}

.fade-slide-enter-active,
.fade-slide-leave-active {
  transition: opacity 0.2s ease, transform 0.2s ease;
}

.fade-slide-enter-from,
.fade-slide-leave-to {
  opacity: 0;
  transform: translateY(6px);
}
</style>
