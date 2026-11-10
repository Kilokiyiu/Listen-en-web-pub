<template>
  <PageShell :title="albumTitle" :show-bar="false" back-label="返回列表">
    <template #header-extra>
      <div class="title-icon">
        <el-icon :size="24" color="#2563eb"><Headset /></el-icon>
      </div>
    </template>

    <div v-if="loading" class="le-loading-wrap">
      <el-icon class="is-loading" :size="32"><Loading /></el-icon>
      <span>加载中...</span>
    </div>

    <div v-else-if="audioUrl || quizSections.length" class="player-section">
      <!-- 上部：整卷音频 + 下载 + 在线答题 -->
      <div class="audio-card le-card">
        <template v-if="audioUrl">
          <div class="audio-label">听力音频</div>
          <div class="audio-visual">
            <div class="audio-wave" v-for="i in 16" :key="i" :style="{ animationDelay: i * 0.08 + 's' }" />
          </div>
          <audio
            ref="audioRef"
            :src="audioUrl"
            controls
            playsinline
            webkit-playsinline
            preload="auto"
            class="audio-player"
          />
          <p class="audio-tip"><el-icon><InfoFilled /></el-icon> 先听音频再作答；题干默认隐藏，可在下方答题设置中打开</p>
        </template>
        <template v-else>
          <p class="audio-tip"><el-icon><InfoFilled /></el-icon> 本套暂无整卷音频，请稍后补传后再练习</p>
        </template>
      </div>

      <div v-if="quizSections.length" class="section-quiz">
        <div class="quiz-settings le-card">
          <div class="quiz-settings__head">
            <h3>在线答题</h3>
            <p class="quiz-settings__tip">{{ practiceModeTip }}</p>
          </div>
          <div class="quiz-settings__row">
            <span class="quiz-settings__label">做题模式</span>
            <el-radio-group
              v-model="practiceMode"
              size="small"
              class="quiz-settings__modes"
            >
              <el-radio-button value="exam">模拟模式</el-radio-button>
              <el-radio-button value="practice">练习模式</el-radio-button>
            </el-radio-group>
          </div>
          <div class="quiz-settings__row quiz-settings__row--switch">
            <span class="quiz-settings__label">显示题干</span>
            <el-switch v-model="showStems" inline-prompt active-text="开" inactive-text="关" />
          </div>
          <transition name="fade-slide">
            <div v-if="modeSwitchHintVisible" class="mode-switch-hint">
              <p class="mode-switch-hint__text">
                已切换到{{ practiceMode === 'exam' ? '模拟' : '练习' }}模式；当前仍保留已看过的答案。
              </p>
              <div class="mode-switch-hint__actions">
                <button type="button" class="mode-switch-hint__btn" @click="dismissModeSwitchHint">
                  知道了
                </button>
                <button
                  type="button"
                  class="mode-switch-hint__btn mode-switch-hint__btn--primary"
                  @click="resetFromModeSwitchHint"
                >
                  重新作答
                </button>
              </div>
            </div>
          </transition>
        </div>

        <QuizSectionTabs
          v-model="activeGroupName"
          :items="groupTabs"
          :scroll="false"
          stretch
          label="听力大题"
          aria-label="听力 Section 切换"
        />

        <QuizSectionTabs
          v-if="passagesInActiveGroup.length > 1"
          v-model="activeQuizSectionId"
          :items="passageTabs"
          size="sm"
          label="本段材料"
          aria-label="听力材料切换"
        />
        <p v-else-if="activeQuizSection" class="passage-single-label">
          {{ formatPassageLabel(activeQuizSection.title, 0, activeGroupName, categoryCode) }}
          <span class="passage-single-en">（{{ activeQuizSection.title }}）</span>
        </p>

        <div
          v-for="sec in quizSections"
          :key="sec.id"
          v-show="sec.id === activeQuizSectionId"
          class="section-body le-card"
        >
          <div v-if="toFileUrl(sec.audioUrl)" class="section-audio-block">
            <div class="audio-label">本段音频（可选）</div>
            <audio
              :src="toFileUrl(sec.audioUrl)"
              controls
              playsinline
              class="section-audio"
            />
          </div>

          <OnlineQuizPanel
            v-if="sec.questions.length"
            :ref="(el) => setPanelRef(sec.id, el)"
            :title="`${formatGroupTabLabel(sec.groupName, categoryCode)} · ${sec.title}`"
            :subtitle="panelSubtitle"
            :questions="sec.questions"
            :show-stem="showStems"
            :persist-key="persistKeyFor(sec.id)"
            :defer-check="practiceMode === 'exam'"
            :submit-fn="practiceMode === 'practice' ? makeSectionSubmitFn(sec.id) : null"
            @progress="(p) => onSectionProgress(sec.id, p)"
            @submitted="(res) => onSectionSubmitted(sec.id, res)"
            @reset="() => onSectionReset(sec.id)"
          />

          <div
            v-if="isSectionUnlocked(sec.id) && sec.transcript?.trim()"
            class="section-transcript-after"
          >
            <div class="toggle-wrap toggle-wrap--inline">
              <el-button
                :type="showSectionText && sec.id === activeQuizSectionId ? 'primary' : 'default'"
                round
                size="small"
                @click="showSectionText = !showSectionText"
              >
                {{ showSectionText ? '隐藏本段原文' : '显示本段原文' }}
              </el-button>
            </div>
            <transition name="fade-slide">
              <div
                v-show="showSectionText && sec.id === activeQuizSectionId"
                class="subtitle-card le-card section-transcript-card"
              >
                <div class="subtitle-header">
                  <el-icon><Document /></el-icon>
                  本段原文 · {{ formatGroupTabLabel(sec.groupName, categoryCode) }} · {{ sec.title }}
                </div>
                <div class="subtitle-content">
                  <p
                    v-for="(para, i) in formatTranscriptParagraphs(sec.transcript)"
                    :key="`sec-${sec.id}-${i}`"
                    class="subtitle-line"
                    :class="{ 'subtitle-line--dialogue': isDialogueLine(para) }"
                  >{{ para }}</p>
                </div>
              </div>
            </transition>
          </div>
          <p v-else-if="sec.transcript?.trim()" class="transcript-locked-hint">
            {{ transcriptLockedHint }}
          </p>
        </div>

        <div class="review-summary le-card">
          <div class="review-summary__head">
            <h3 class="review-summary__title">
              {{ practiceMode === 'exam' ? '模拟交卷' : '练习进度' }}
            </h3>
            <p class="review-summary__sub">
              <template v-if="paperGraded">
                合计 {{ paperCorrect }}/{{ paperTotal }} 题正确（{{ paperScorePercent }}%）
              </template>
              <template v-else-if="anySectionGraded">
                已有 {{ gradedSectionCount }} 段查看过答案
                <template v-if="practiceMode === 'exam'">
                  · 可「重新作答」后再模拟交卷
                </template>
                <template v-else>
                  · 可继续练习其它段，或「重新作答」
                </template>
              </template>
              <template v-else>
                已选 {{ answeredTotal }}/{{ paperTotal }} 题
                <template v-if="practiceMode === 'exam' && !allQuestionsAnswered">
                  · 全部选完后可交卷对答案
                </template>
                <template v-else-if="practiceMode === 'practice'">
                  · 每段答完后可随时查看答案
                </template>
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
              v-if="practiceMode === 'exam' && !anySectionGraded"
              type="primary"
              round
              size="large"
              class="review-summary__submit"
              :loading="submittingAll"
              :disabled="!allQuestionsAnswered"
              @click="submitWholePaper"
            >
              交卷并对答案
            </el-button>
            <el-button
              v-if="anySectionGraded"
              round
              size="large"
              class="review-summary__submit"
              @click="confirmResetAnswers"
            >
              {{ resetAnswersLabel }}
            </el-button>
          </div>
        </div>
      </div>
      <div v-else-if="!loading" class="quiz-empty-hint">
        本套暂无在线题目。可先听上方音频；原题与解析见下方「资料与其它」。
      </div>

      <!-- 底部：资料 / 原文 / 完成 / 反馈 集中区 -->
      <div class="page-tools le-card">
        <div class="page-tools__head">
          <h3 class="page-tools__title">资料与其它</h3>
          <p class="page-tools__sub">原题、解析、原文与学习记录都放在这里</p>
        </div>

        <div v-if="paperFileUrl || answerFileUrl" class="page-tools__group">
          <div class="page-tools__group-label">试卷资料</div>
          <div class="page-tools__materials">
            <div v-if="paperFileUrl" class="page-tools__material">
              <span class="page-tools__material-name">原题</span>
              <el-button
                size="small"
                round
                :type="activePdf === 'paper' ? 'primary' : 'default'"
                @click="togglePdf('paper')"
              >
                <el-icon><Document /></el-icon>
                {{ activePdf === 'paper' ? '收起' : '在线查看' }}
              </el-button>
              <el-button size="small" round type="primary" @click="downloadPdf('paper')">
                <el-icon><Download /></el-icon>
                下载
              </el-button>
            </div>
            <div v-if="answerFileUrl" class="page-tools__material">
              <span class="page-tools__material-name">解析</span>
              <el-button
                size="small"
                round
                :type="activePdf === 'answer' ? 'success' : 'default'"
                @click="togglePdf('answer')"
              >
                <el-icon><DocumentChecked /></el-icon>
                {{ activePdf === 'answer' ? '收起' : '在线查看' }}
              </el-button>
              <el-button size="small" round type="success" @click="downloadPdf('answer')">
                <el-icon><Download /></el-icon>
                下载
              </el-button>
            </div>
          </div>
        </div>

        <div v-if="hasFullTranscript" class="page-tools__group">
          <div class="page-tools__group-label">整卷原文</div>
          <div class="page-tools__chips">
            <el-switch
              v-model="showText"
              inline-prompt
              active-text="显示"
              inactive-text="隐藏"
            />
          </div>
        </div>

        <div class="page-tools__group page-tools__group--actions">
          <div class="page-tools__group-label">本卷操作</div>
          <div class="page-tools__chips">
            <el-button
              type="success"
              size="small"
              round
              :loading="completing"
              :disabled="completed"
              @click="markComplete(false)"
            >
              {{ completed ? '已完成本卷' : '标记完成' }}
            </el-button>
            <FeedbackEntry
              :source="feedbackSource"
              button-type="default"
              size="small"
              :round="true"
              :plain="true"
            />
          </div>
        </div>

        <transition name="fade-slide">
          <div v-if="activePdf && currentPdfUrl" class="pdf-viewer">
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

        <transition name="fade-slide">
          <div v-show="showText && hasFullTranscript" class="subtitle-card full-transcript-card">
            <div class="subtitle-header"><el-icon><Document /></el-icon> 整卷听力原文</div>
            <div class="subtitle-content">
              <p
                v-for="(para, i) in fullTranscriptParagraphs"
                :key="`full-${i}`"
                class="subtitle-line"
                :class="{ 'subtitle-line--dialogue': isDialogueLine(para) }"
              >{{ para }}</p>
            </div>
          </div>
        </transition>
      </div>
    </div>

    <el-empty v-else description="暂无音频数据" />
  </PageShell>
</template>

<script setup>
import { ref, onMounted, computed, watch, reactive } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import PageShell from '../components/PageShell.vue'
import PdfViewer from '../components/PdfViewer.vue'
import FeedbackEntry from '../components/FeedbackEntry.vue'
import OnlineQuizPanel from '../components/OnlineQuizPanel.vue'
import QuizSectionTabs from '../components/QuizSectionTabs.vue'
import { getEpisodesByAlbumId, getAlbumById, getCategories, getQuizByAlbumId, submitQuiz } from '../api/Listen.js'
import { recordStudyActivity } from '../api/Study.js'
import { trackEvent } from '../api/Analytics.js'
import { promptFeedbackGift } from '../utils/promptFeedbackGift.js'
import { useAudioPlayer } from '../composables/useAudioPlayer.js'
import { loadQuizProgress } from '../utils/quizProgressStore.js'
import {
  GROUP_ORDER,
  formatGroupTabLabel,
  formatGroupBadge,
  formatPassageLabel,
  sectionTypeLabels,
  inferGroupName,
} from '../utils/cetListeningSections.js'

const route = useRoute()
const router = useRouter()
const albumId = route.query.albumId
const audioRef = ref(null)

const albumTitle = ref('听力真题')
const categoryLabel = ref('听力')
const categoryCode = ref('')
const audioUrl = ref('')
const paperFileUrl = ref('')
const answerFileUrl = ref('')
const loading = ref(true)
const showText = ref(false)
const showSectionText = ref(false)
const showStems = ref(false)
const subtitleText = ref('')
const activePdf = ref('')
const completing = ref(false)
const completed = ref(false)
const quizSections = ref([])
const activeGroupName = ref('Section A')
const activeQuizSectionId = ref('')
/** sectionId -> { answered, total, complete, graded, correctCount?, scorePercent? } */
const sectionProgress = reactive({})
const panelRefs = {}
const submittingAll = ref(false)
const sessionStartedAt = Date.now()

/** exam = 模拟模式（全部答完再看答案）；practice = 练习模式（可分段随时看答案） */
const PRACTICE_MODE_KEY = 'listen:quizPracticeMode'
const normalizePracticeMode = (raw) => {
  if (raw === 'practice' || raw === 'section') return 'practice'
  if (raw === 'exam' || raw === 'whole') return 'exam'
  return 'exam'
}
const loadPracticeMode = () => {
  try {
    return normalizePracticeMode(localStorage.getItem(PRACTICE_MODE_KEY))
  } catch {
    return 'exam'
  }
}
const practiceMode = ref(loadPracticeMode())
const modeSwitchHintVisible = ref(false)

const practiceModeTip = computed(() =>
  practiceMode.value === 'practice'
    ? '练习模式：适合平时训练，做完一段即可查看答案与解析，方便及时纠错。'
    : '模拟模式：接近真实考试，需全部答完后再交卷看答案，过程中不提前对答案。'
)

const dismissModeSwitchHint = () => {
  modeSwitchHintVisible.value = false
}

const resetFromModeSwitchHint = () => {
  modeSwitchHintVisible.value = false
  resetWholePaper({ notify: false })
  ElMessage.success(
    practiceMode.value === 'exam'
      ? '已清空作答，可开始模拟考试'
      : '已清空作答，可开始分段练习'
  )
}

const panelSubtitle = computed(() =>
  practiceMode.value === 'practice'
    ? '听音频后作答；本段答完即可查看答案'
    : '听音频后作答；请先做完全卷，再统一交卷对答案'
)

const transcriptLockedHint = computed(() =>
  practiceMode.value === 'practice'
    ? '本段查看答案后可阅读原文'
    : '交卷并对答案后可查看本段原文'
)

const categoryMeta = {
  cet4: 'CET-4',
  cet6: 'CET-6',
}

const toFileUrl = (path) => {
  if (!path) return ''
  const p = String(path)
  // 本地静态资源或绝对地址，不再拼 /api/listen
  if (/^https?:\/\//i.test(p) || p.startsWith('/demo/')) return p
  return `/api/listen${p}`
}

const persistKeyFor = (sectionId) =>
  albumId && sectionId ? `listen:quiz:${albumId}:${sectionId}` : ''

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

const normalizeSubmitResult = (res) => {
  const results = res?.results || res?.Results || []
  const correctCount = res?.correctCount ?? res?.CorrectCount ?? 0
  const total = res?.total ?? res?.Total ?? results.length
  const scorePercent =
    res?.scorePercent ??
    res?.ScorePercent ??
    (total ? Math.round((correctCount * 1000) / total) / 10 : 0)
  return { correctCount, scorePercent, results, total }
}

const makeSectionSubmitFn = (sectionId) => async (answers) => {
  const res = await submitQuiz(albumId, answers, sectionId)
  return normalizeSubmitResult(res)
}

const onSectionSubmitted = (sectionId, res) => {
  const normalized = normalizeSubmitResult(res)
  const total =
    quizSections.value.find((s) => s.id === sectionId)?.questions?.length ||
    normalized.total ||
    0
  sectionProgress[sectionId] = {
    answered: total,
    total,
    complete: true,
    graded: true,
    correctCount: normalized.correctCount,
    scorePercent: normalized.scorePercent,
  }
  if (sectionId === activeQuizSectionId.value) {
    showSectionText.value = true
  }
  trackEvent('listen_quiz_submit', `/exam?albumId=${albumId}&sectionId=${sectionId}`)
  if (localStorage.getItem('token')) {
    const sec = quizSections.value.find((s) => s.id === sectionId)
    recordStudyActivity({
      activityType: 'listen_quiz',
      contentId: String(albumId),
      title: `${albumTitle.value} · ${sec ? sectionLabel(sec) : '练习对答案'}`,
      category: categoryLabel.value,
      durationSeconds: getDurationSeconds(),
    }).catch(() => {})
  }
  ElMessage.success(
    `本段练习完成：${normalized.correctCount}/${total} 题正确`
  )
}

const onSectionReset = (sectionId) => {
  const total = quizSections.value.find((s) => s.id === sectionId)?.questions?.length || 0
  sectionProgress[sectionId] = {
    answered: 0,
    total,
    complete: false,
    graded: false,
  }
  if (sectionId === activeQuizSectionId.value) {
    showSectionText.value = false
  }
}

const hydrateSectionProgress = () => {
  for (const s of quizSections.value) {
    const data = loadQuizProgress(persistKeyFor(s.id))
    const total = s.questions?.length || 0
    if (!data) {
      sectionProgress[s.id] = { answered: 0, total, complete: false, graded: false }
      continue
    }
    const answeredMap = data.answers || {}
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

const availableGroups = computed(() =>
  GROUP_ORDER.filter((g) => quizSections.value.some((s) => s.groupName === g))
)

const groupProgressBadge = (groupName) => {
  const secs = quizSections.value.filter((s) => s.groupName === groupName)
  if (!secs.length) return formatGroupBadge(groupName)
  const answered = secs.reduce((n, s) => n + (sectionProgress[s.id]?.answered || 0), 0)
  const total = secs.reduce((n, s) => n + (s.questions?.length || 0), 0)
  const graded = secs.every((s) => sectionProgress[s.id]?.graded)
  if (graded) {
    const correct = secs.reduce((n, s) => n + (sectionProgress[s.id]?.correctCount || 0), 0)
    return `${correct}/${total}`
  }
  if (total) return `${answered}/${total}`
  return formatGroupBadge(groupName)
}

const groupTabs = computed(() =>
  availableGroups.value.map((g) => ({
    value: g,
    label: formatGroupTabLabel(g, categoryCode.value),
    badge: groupProgressBadge(g),
    title: formatGroupTabLabel(g, categoryCode.value),
  }))
)

const activeGroupTypeLabel = computed(
  () => sectionTypeLabels(categoryCode.value)[activeGroupName.value] || activeGroupName.value
)

const passagesInActiveGroup = computed(() =>
  quizSections.value.filter((s) => s.groupName === activeGroupName.value)
)

const passageTabs = computed(() =>
  passagesInActiveGroup.value.map((s, i) => {
    const p = sectionProgress[s.id]
    let badge = String(i + 1)
    if (p?.graded) badge = `${p.correctCount}/${p.total}`
    else if (p && p.total) badge = `${p.answered}/${p.total}`
    return {
      value: s.id,
      label: formatPassageLabel(s.title, i, s.groupName, categoryCode.value),
      title: s.title,
      badge,
    }
  })
)

const activeQuizSection = computed(() =>
  passagesInActiveGroup.value.find((s) => s.id === activeQuizSectionId.value)
  || passagesInActiveGroup.value[0]
  || null
)

const quizSectionsWithQuestions = computed(() =>
  quizSections.value.filter((s) => (s.questions?.length || 0) > 0)
)

const sectionLabel = (s) => {
  const idx = quizSections.value
    .filter((x) => x.groupName === s.groupName)
    .findIndex((x) => x.id === s.id)
  const passage = formatPassageLabel(s.title, Math.max(0, idx), s.groupName, categoryCode.value)
  return `${formatGroupTabLabel(s.groupName, categoryCode.value)} · ${passage}`
}

const reviewRows = computed(() =>
  quizSectionsWithQuestions.value.map((s) => {
    const p = sectionProgress[s.id] || {}
    return {
      id: s.id,
      label: sectionLabel(s),
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
  quizSectionsWithQuestions.value.reduce((sum, s) => sum + (s.questions?.length || 0), 0)
)

const answeredTotal = computed(() =>
  quizSectionsWithQuestions.value.reduce(
    (sum, s) => sum + (sectionProgress[s.id]?.answered || 0),
    0
  )
)

const allQuestionsAnswered = computed(
  () =>
    quizSectionsWithQuestions.value.length > 0 &&
    quizSectionsWithQuestions.value.every((s) => {
      const p = sectionProgress[s.id]
      return p && (p.complete || p.graded)
    })
)

const paperGraded = computed(
  () =>
    quizSectionsWithQuestions.value.length > 0 &&
    quizSectionsWithQuestions.value.every((s) => sectionProgress[s.id]?.graded)
)

const anySectionGraded = computed(() =>
  quizSectionsWithQuestions.value.some((s) => sectionProgress[s.id]?.graded)
)

const gradedSectionCount = computed(
  () => quizSectionsWithQuestions.value.filter((s) => sectionProgress[s.id]?.graded).length
)

const resetAnswersLabel = computed(() => {
  if (practiceMode.value === 'exam') {
    return paperGraded.value ? '再模拟一次' : '重新作答'
  }
  return paperGraded.value ? '全部再练一次' : '重新作答'
})

/** 该材料已对过答案后即可查看本段原文（模拟模式交卷后一次全部解锁） */
const isSectionUnlocked = (sectionId) => !!sectionProgress[sectionId]?.graded

watch(practiceMode, () => {
  try {
    localStorage.setItem(PRACTICE_MODE_KEY, practiceMode.value)
  } catch {
    /* ignore */
  }
  showSectionText.value = isSectionUnlocked(activeQuizSectionId.value)
  modeSwitchHintVisible.value = anySectionGraded.value
})

const paperCorrect = computed(() =>
  quizSectionsWithQuestions.value.reduce(
    (sum, s) => sum + (sectionProgress[s.id]?.correctCount || 0),
    0
  )
)

const paperScorePercent = computed(() => {
  const t = paperTotal.value
  if (!t) return 0
  return Math.round((paperCorrect.value * 1000) / t) / 10
})

const jumpToSection = (sectionId) => {
  const sec = quizSections.value.find((s) => s.id === sectionId)
  if (!sec) return
  activeGroupName.value = sec.groupName
  activeQuizSectionId.value = sectionId
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

const currentPdfUrl = computed(() => {
  if (activePdf.value === 'paper') return paperFileUrl.value
  if (activePdf.value === 'answer') return answerFileUrl.value
  return ''
})

const feedbackSource = computed(() => {
  const parts = [categoryLabel.value, albumTitle.value].filter(Boolean)
  return parts.length ? `听力页 · ${parts.join(' · ')}` : '听力页'
})

const togglePdf = (type) => {
  activePdf.value = activePdf.value === type ? '' : type
}

const pdfDownloadName = (kind) => {
  const title = (albumTitle.value || '听力真题').replace(/[\\/:*?"<>|]/g, '_')
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

/** 对话说话人行（M:/W: 等） */
const SPEAKER_LINE_RE =
  /^(?:M|W|Man|Woman|男|女|A|B)\s*[:：]\s*/i

const isDialogueTranscript = (lines) => {
  const nonEmpty = lines.map((l) => l.trim()).filter(Boolean)
  if (nonEmpty.length < 2) return false
  const speakerHits = nonEmpty.filter((l) => SPEAKER_LINE_RE.test(l)).length
  return speakerHits >= 2 && speakerHits / nonEmpty.length >= 0.4
}

/** 篇章/新闻：新段常见起首（无空行时用） */
const PROSE_PARA_START_RE =
  /^(?:So |But |Yet |Still |However[, ]|Meanwhile[, ]|Moreover[, ]|Furthermore[, ]|In fact[, ]|In reality[, ]|The reality is|Actually[, ]|Finally[, ]|First[, ]|Second[, ]|Third[, ]|Then |Next |Also[, ]|And yet |On the other hand|For example[, ]|For instance[, ]|As a result[, ]|In addition[, ]|In other words[, ]|Of course[, ]|Honestly[, ]|Unfortunately[, ]|Interestingly[, ]|[A-Z][a-z]+(?:\s+[A-Z][a-z]+){0,3}\s+(?:(?:has|have|once)\s+)?(?:also\s+)?(?:said|remarked|noted|added|explained|observed|wrote|argued|pointed out)\b)/

/**
 * 将无空行的英文篇章按句切成若干段（约 2–4 句/段，遇话语标记另起一段）
 */
const splitFlatProseIntoParagraphs = (flat) => {
  const text = String(flat || '').replace(/\s+/g, ' ').trim()
  if (!text) return []
  // 按句末切分，保留引号内句号尽量不误切：简化版按 .!? 后跟空格+大写
  const sentences = []
  const re = /[^.!?]+(?:[.!?]+["']?|(?=$))/g
  let m
  while ((m = re.exec(text)) !== null) {
    const s = m[0].trim()
    if (s) sentences.push(s)
  }
  if (sentences.length <= 2) return [text]

  const paras = []
  let buf = []
  const flush = () => {
    if (buf.length) {
      paras.push(buf.join(' ').replace(/\s+/g, ' ').trim())
      buf = []
    }
  }

  for (let i = 0; i < sentences.length; i++) {
    const s = sentences[i]
    const startNew =
      buf.length > 0 &&
      (PROSE_PARA_START_RE.test(s) ||
        buf.length >= 3 ||
        (buf.length >= 2 && s.length > 120))
    if (startNew) flush()
    buf.push(s)
  }
  flush()
  return paras.filter(Boolean)
}

/**
 * 原文排版：
 * - 男女对话：每句一行，保留换行
 * - 普通短文：有空行则按空行分段；整段无换行时按句/话语标记自动分段
 */
const formatTranscriptParagraphs = (raw) => {
  if (!raw || !String(raw).trim()) return []
  const text = String(raw).replace(/\r\n/g, '\n').trim()
  try {
    const parsed = JSON.parse(text)
    if (Array.isArray(parsed)) {
      return parsed
        .map((item) => (item.text || '').replace(/[ \t]+/g, ' ').trim())
        .filter(Boolean)
    }
  } catch {
    /* plain text */
  }

  const lines = text.split('\n').map((l) => l.replace(/[ \t]+/g, ' ').trimEnd())

  if (isDialogueTranscript(lines)) {
    // 说话人各占一行；若下一行不是说话人开头，视为上一句续行并合并
    const out = []
    for (const line of lines) {
      const trimmed = line.trim()
      if (!trimmed) {
        if (out.length && out[out.length - 1] !== '') out.push('')
        continue
      }
      if (SPEAKER_LINE_RE.test(trimmed) || out.length === 0 || out[out.length - 1] === '') {
        out.push(trimmed)
      } else {
        out[out.length - 1] = `${out[out.length - 1]} ${trimmed}`.trim()
      }
    }
    return out.filter((l, i, arr) => l !== '' || (i > 0 && arr[i - 1] !== ''))
  }

  // 已有空行分段
  if (/\n\s*\n/.test(text)) {
    return text
      .split(/\n\s*\n/)
      .map((para) => para.replace(/\s*\n\s*/g, ' ').replace(/[ \t]+/g, ' ').trim())
      .filter(Boolean)
  }

  // 多行但无空行：每行已是独立段（如导入保留的编号段）
  const nonEmptyLines = lines.map((l) => l.trim()).filter(Boolean)
  if (nonEmptyLines.length >= 2) {
    const avgLen =
      nonEmptyLines.reduce((n, l) => n + l.length, 0) / nonEmptyLines.length
    // 行均较长 → 当作已分段；较短可能是软换行，先合并再自动切
    if (avgLen >= 80) return nonEmptyLines
    return splitFlatProseIntoParagraphs(nonEmptyLines.join(' '))
  }

  // 整段无换行（库里新闻/篇章常见）→ 自动分段
  return splitFlatProseIntoParagraphs(text)
}

const isDialogueLine = (line) => SPEAKER_LINE_RE.test(String(line || '').trim())

const fullTranscriptSource = computed(() => {
  // 整卷原文仅用 Episode 字幕；各段题目原文须整卷对答案后才在题目下方可见
  return subtitleText.value?.trim() || ''
})

const hasFullTranscript = computed(() => Boolean(fullTranscriptSource.value.trim()))

const fullTranscriptParagraphs = computed(() =>
  formatTranscriptParagraphs(fullTranscriptSource.value)
)

watch(activeQuizSectionId, (id) => {
  showSectionText.value = isSectionUnlocked(id)
})

watch(activeGroupName, (g) => {
  const stillInGroup = quizSections.value.some(
    (s) => s.id === activeQuizSectionId.value && s.groupName === g
  )
  if (stillInGroup) return
  const first = quizSections.value.find((s) => s.groupName === g)
  activeQuizSectionId.value = first?.id || ''
})

watch(paperGraded, () => {
  showSectionText.value = isSectionUnlocked(activeQuizSectionId.value)
})

const resolveCategory = async (categoryId) => {
  if (!categoryId) return
  if (String(categoryId).startsWith('mock')) {
    categoryCode.value = 'cet4'
    categoryLabel.value = 'CET-4'
    return
  }
  try {
    const categories = await getCategories()
    const cat = (categories || []).find(c => c.id === categoryId)
    if (cat?.code) {
      categoryCode.value = cat.code
      categoryLabel.value = categoryMeta[cat.code] || cat.name?.chinese || cat.name || cat.code
    } else if (cat?.name?.chinese || cat?.name) {
      const name = cat.name?.chinese || cat.name
      categoryCode.value = String(name).includes('六') ? 'cet6' : String(name).includes('四') ? 'cet4' : ''
      categoryLabel.value = name
    }
  } catch {
    /* ignore */
  }
}

const getDurationSeconds = () => {
  const audio = audioRef.value
  if (audio && Number.isFinite(audio.duration) && audio.duration > 0) {
    return Math.round(Math.min(audio.currentTime || audio.duration, audio.duration))
  }
  return Math.round((Date.now() - sessionStartedAt) / 1000)
}

const nudgeListenFeedback = (fromAuto = false) => {
  promptFeedbackGift({
    source: feedbackSource.value || `听力 · ${albumTitle.value}`,
    context: albumTitle.value || '听力练习',
    delayMs: fromAuto ? 800 : 500,
  })
}

const markComplete = async (fromAuto = false) => {
  if (!albumId || completing.value || completed.value) return

  trackEvent('listen_complete', `/exam?albumId=${albumId}`)

  // 未登录：手动完成仍引导登录；音频听完则直接鼓励反馈（反馈接口无需登录）
  if (!localStorage.getItem('token')) {
    if (!fromAuto) {
      ElMessage.warning('登录后可记录学习进度')
      router.push({ name: 'login', query: { redirect: route.fullPath } })
      return
    }
    nudgeListenFeedback(true)
    return
  }

  completing.value = true
  try {
    await recordStudyActivity({
      activityType: 'listen',
      contentId: String(albumId),
      title: albumTitle.value,
      category: categoryLabel.value,
      durationSeconds: getDurationSeconds(),
    })
    completed.value = true
    if (!fromAuto) ElMessage.success('已记录学习进度')
    nudgeListenFeedback(fromAuto)
  } catch (e) {
    // 记进度失败仍视为本卷听完，继续鼓励反馈
    completed.value = true
    nudgeListenFeedback(fromAuto)
  } finally {
    completing.value = false
  }
}

const normalizeQuizSections = (raw) =>
  (raw || []).map((s) => ({
    id: s.id || s.Id,
    groupName: inferGroupName(s.groupName || s.GroupName, s.title || s.Title, categoryCode.value),
    title: s.title || s.Title || 'Passage',
    transcript: s.transcript || s.Transcript || '',
    audioUrl: s.audioUrl || s.AudioUrl || '',
    questions: (s.questions || s.Questions || []).map((q) => ({
      id: q.id || q.Id,
      number: q.number ?? q.Number,
      stem: q.stem || q.Stem || '',
      options: q.options || q.Options || [],
    })),
  }))

const submitWholePaper = async () => {
  if (!allQuestionsAnswered.value || paperGraded.value) return
  const missing = quizSectionsWithQuestions.value.filter(
    (s) => !panelRefs[s.id]?.isComplete?.()
  )
  if (missing.length) {
    ElMessage.warning(`还有材料未答完：${missing.map(sectionLabel).join('、')}`)
    jumpToSection(missing[0].id)
    return
  }

  const allAnswers = []
  for (const s of quizSectionsWithQuestions.value) {
    const panel = panelRefs[s.id]
    if (!panel?.getAnswers) continue
    allAnswers.push(...panel.getAnswers())
  }

  submittingAll.value = true
  try {
    const res = await submitQuiz(albumId, allAnswers)
    const results = res.results || res.Results || []
    const byId = new Map(
      results.map((r) => {
        const id = r.questionId || r.QuestionId
        return [id, r]
      })
    )

    for (const s of quizSectionsWithQuestions.value) {
      const panel = panelRefs[s.id]
      const sectionResultsList = (s.questions || [])
        .map((q) => byId.get(q.id))
        .filter(Boolean)
      const correctCount = sectionResultsList.filter(
        (r) => !!(r.isCorrect ?? r.IsCorrect)
      ).length
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

    const total = res.total ?? res.Total ?? paperTotal.value
    const correctCount = res.correctCount ?? res.CorrectCount ?? paperCorrect.value
    trackEvent('listen_quiz_submit', `/exam?albumId=${albumId}`)
    showSectionText.value = true

    if (localStorage.getItem('token')) {
      try {
        await recordStudyActivity({
          activityType: 'listen_quiz',
          contentId: String(albumId),
          title: `${albumTitle.value} · 模拟交卷`,
          category: categoryLabel.value,
          durationSeconds: getDurationSeconds(),
        })
      } catch {
        /* */
      }
    }
    ElMessage.success(`模拟交卷完成：${correctCount}/${total} 题正确`)
    nudgeListenFeedback(false)
  } catch {
    /* */
  } finally {
    submittingAll.value = false
  }
}

const resetWholePaper = ({ notify = true } = {}) => {
  for (const s of quizSectionsWithQuestions.value) {
    panelRefs[s.id]?.resetAll?.()
    sectionProgress[s.id] = {
      answered: 0,
      total: s.questions?.length || 0,
      complete: false,
      graded: false,
    }
  }
  showSectionText.value = false
  if (notify) ElMessage.success('已清空作答，可重新练习')
  if (quizSectionsWithQuestions.value[0]) {
    jumpToSection(quizSectionsWithQuestions.value[0].id)
  }
}

const confirmResetAnswers = async () => {
  try {
    await ElMessageBox.confirm(
      '将清空全部作答与对错结果，确定重新作答吗？',
      '重新作答',
      {
        confirmButtonText: '确定清空',
        cancelButtonText: '取消',
        type: 'warning',
      }
    )
    resetWholePaper()
  } catch {
    /* cancel */
  }
}

const loadEpisode = async () => {
  if (!albumId) { loading.value = false; return }
  try {
    const [episodes, album, quiz] = await Promise.all([
      getEpisodesByAlbumId(albumId),
      getAlbumById(albumId).catch(() => null),
      getQuizByAlbumId(albumId).catch(() => null),
    ])
    if (album?.name) {
      albumTitle.value = album.name.chinese || album.name.Chinese || album.name || '听力真题'
    }
    paperFileUrl.value = toFileUrl(album?.paperFileUrl || album?.PaperFileUrl)
    answerFileUrl.value = toFileUrl(album?.answerFileUrl || album?.AnswerFileUrl)
    await resolveCategory(album?.categoryId || album?.CategoryId)

    const ep = (episodes || [])[0]
    if (ep) {
      if (!album?.name) {
        albumTitle.value = ep.name?.chinese || ep.name || '听力真题'
      }
      audioUrl.value = toFileUrl(ep.audioUrl || ep.AudioUrl)
      subtitleText.value = ep.Subtitle || ep.subtitle || ''
    }
    quizSections.value = normalizeQuizSections(quiz?.sections || quiz?.Sections)
    hydrateSectionProgress()
    activeGroupName.value = availableGroups.value[0] || 'Section A'
    activeQuizSectionId.value =
      quizSections.value.find((s) => s.groupName === activeGroupName.value)?.id
      || quizSections.value[0]?.id
    if (isSectionUnlocked(activeQuizSectionId.value)) showSectionText.value = true
  } catch (e) {
    console.error('获取音频失败', e)
  } finally {
    loading.value = false
  }
}

onMounted(loadEpisode)

useAudioPlayer(audioRef, {
  storageKey: albumId ? `listen:audio:${albumId}` : '',
  title: albumTitle,
  album: '听力真题',
  onEnded: () => markComplete(true),
})
</script>

<style scoped>
.title-icon {
  width: 40px;
  height: 40px;
  border-radius: 12px;
  background: var(--le-gradient-soft);
  display: flex;
  align-items: center;
  justify-content: center;
}

.player-section {
  max-width: 720px;
  margin: 0 auto;
}

.audio-card {
  padding: 28px 20px;
  text-align: center;
  background: var(--le-gradient-soft);
}

.audio-label {
  font-size: 13px;
  font-weight: 650;
  color: var(--le-text);
  margin-bottom: 12px;
  letter-spacing: 0.02em;
}

.audio-visual {
  display: flex;
  justify-content: center;
  gap: 3px;
  height: 36px;
  margin-bottom: 16px;
}

.audio-wave {
  width: 4px;
  border-radius: 2px;
  background: var(--le-gradient);
  animation: wave 1.2s ease-in-out infinite;
}

@keyframes wave {
  0%, 100% { height: 8px; opacity: 0.35; }
  50% { height: 28px; opacity: 0.85; }
}

.audio-player {
  width: 100%;
  max-width: 100%;
}

.audio-tip {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  font-size: 13px;
  color: var(--le-text-muted);
  margin: 14px 0 0;
}

.audio-tip--sub {
  margin-top: 6px;
  font-size: 12px;
  opacity: 0.85;
}

.page-tools {
  margin-top: 28px;
  max-width: 720px;
  padding: 16px 18px 18px;
}

.page-tools__head {
  margin-bottom: 14px;
}

.page-tools__title {
  margin: 0;
  font-size: 16px;
  font-weight: 650;
  color: var(--le-text);
}

.page-tools__sub {
  margin: 4px 0 0;
  font-size: 13px;
  color: var(--le-text-muted);
  line-height: 1.45;
}

.page-tools__group {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 10px 14px;
  padding: 12px 0;
  border-top: 1px solid var(--le-border);
}

.page-tools__group--actions {
  padding-bottom: 2px;
}

.page-tools__group-label {
  flex: 0 0 auto;
  min-width: 4.5em;
  font-size: 13px;
  font-weight: 600;
  color: var(--le-text);
}

.page-tools__chips {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 8px;
  flex: 1;
  min-width: 0;
}

.page-tools__materials {
  display: flex;
  flex-direction: column;
  gap: 10px;
  flex: 1;
  min-width: 0;
}

.page-tools__material {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 8px;
}

.page-tools__material-name {
  min-width: 2.5em;
  font-size: 13px;
  font-weight: 600;
  color: var(--le-text-muted);
}

.pdf-viewer {
  margin-top: 12px;
  overflow: hidden;
  border: 1px solid var(--le-border);
  border-radius: 12px;
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

.toggle-wrap {
  text-align: center;
  margin: 20px 0;
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 10px;
}

.subtitle-card {
  overflow: hidden;
  margin-bottom: 14px;
}

.full-transcript-card {
  margin-top: 14px;
  border: 1px solid var(--le-border);
  border-radius: 12px;
}

.subtitle-header {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 14px 18px;
  font-weight: 600;
  background: var(--le-bg-muted);
  border-bottom: 1px solid var(--le-border);
}

.subtitle-content {
  padding: 18px;
  max-height: 50vh;
  overflow-y: auto;
}

.subtitle-line {
  margin: 0 0 14px;
  line-height: 1.8;
  font-size: 15px;
  text-align: justify;
  text-indent: 2em;
}

.subtitle-line--dialogue {
  margin-bottom: 8px;
  text-align: left;
  text-indent: 2em;
}

.subtitle-line:last-child {
  margin-bottom: 0;
}

.fade-slide-enter-active, .fade-slide-leave-active { transition: all 0.25s ease; }
.fade-slide-enter-from, .fade-slide-leave-to { opacity: 0; transform: translateY(-8px); }

.quiz-empty-hint {
  margin-top: 20px;
  text-align: center;
  font-size: 13px;
  color: var(--le-text-muted);
}

.section-quiz {
  margin-top: 28px;
  max-width: 720px;
}

.quiz-settings {
  padding: 16px 18px 14px;
  margin-bottom: 16px;
}

.quiz-settings__head h3 {
  margin: 0;
  font-size: 17px;
  font-weight: 650;
}

.quiz-settings__tip {
  margin: 6px 0 0;
  font-size: 13px;
  color: var(--le-text-muted);
  line-height: 1.5;
}

.quiz-settings__row {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 10px 14px;
  margin-top: 14px;
  padding-top: 12px;
  border-top: 1px solid var(--le-border);
}

.quiz-settings__row--switch {
  margin-top: 10px;
  padding-top: 10px;
}

.quiz-settings__label {
  flex: 0 0 auto;
  min-width: 4.5em;
  font-size: 13px;
  font-weight: 600;
  color: var(--le-text);
}

.quiz-settings__modes {
  flex: 1;
  flex-wrap: wrap;
}

.mode-switch-hint {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 10px 14px;
  margin-top: 12px;
  padding: 10px 12px;
  border-radius: 10px;
  background: color-mix(in srgb, var(--le-primary, #2563eb) 8%, var(--le-bg-muted));
  border: 1px solid color-mix(in srgb, var(--le-primary, #2563eb) 18%, var(--le-border));
}

.mode-switch-hint__text {
  margin: 0;
  flex: 1;
  min-width: 180px;
  font-size: 13px;
  line-height: 1.5;
  color: var(--le-text-secondary, var(--le-text));
}

.mode-switch-hint__actions {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 8px;
}

.mode-switch-hint__btn {
  appearance: none;
  border: 1px solid var(--le-border);
  background: var(--le-bg-elevated, #fff);
  color: var(--le-text-secondary, var(--le-text));
  border-radius: 999px;
  padding: 5px 12px;
  font-size: 12px;
  font-weight: 600;
  cursor: pointer;
  line-height: 1.4;
  transition: background 0.15s ease, border-color 0.15s ease, color 0.15s ease;
}

.mode-switch-hint__btn:hover {
  border-color: var(--le-border-strong);
  color: var(--le-text);
}

.mode-switch-hint__btn--primary {
  border-color: transparent;
  background: var(--le-gradient);
  color: #fff;
}

.mode-switch-hint__btn--primary:hover {
  filter: brightness(1.04);
  color: #fff;
  border-color: transparent;
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

.passage-single-label {
  margin: 0 0 14px;
  font-size: 13px;
  font-weight: 500;
  color: var(--le-text-muted);
}

.passage-single-en {
  font-weight: 400;
  opacity: 0.75;
}

.section-body {
  padding: 16px;
}

.section-audio-block {
  margin-bottom: 16px;
  padding-bottom: 14px;
  border-bottom: 1px solid var(--le-border);
}

.section-audio-block .audio-label {
  text-align: left;
  margin-bottom: 10px;
}

.section-audio {
  width: 100%;
  margin-bottom: 0;
}

.section-audio-empty {
  margin: 0;
  font-size: 13px;
  color: var(--le-text-muted);
}

.section-transcript-after {
  margin-top: 18px;
}

.toggle-wrap--inline {
  margin: 0 0 12px;
}

.questions-hidden-hint {
  margin: 0 0 8px;
  text-align: center;
  font-size: 13px;
  color: var(--le-text-muted);
  line-height: 1.6;
}

.section-transcript-card {
  margin-top: 0;
}

.transcript-locked-hint {
  margin: 16px 0 0;
  text-align: center;
  font-size: 13px;
  color: var(--le-text-muted);
}
</style>
