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
      <!-- 上部：整卷音频 + 在线答题 -->
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
          <p class="audio-tip"><el-icon><InfoFilled /></el-icon> 先听音频；默认隐藏题干，需要时可点「显示题干」</p>
        </template>
        <template v-else>
          <p class="audio-tip"><el-icon><InfoFilled /></el-icon> 本套暂无整卷音频，请稍后补传后再练习</p>
        </template>
      </div>

      <div v-if="quizSections.length" class="section-quiz">
        <div class="section-quiz-head">
          <h3>在线答题</h3>
<!--          <p>默认隐藏题干，选项可直接作答；听完后再显示题干核对</p>-->
        </div>

        <div class="toggle-wrap toggle-wrap--questions">
          <el-button
            :type="showStems ? 'primary' : 'warning'"
            round
            class="le-btn-gradient"
            @click="showStems = !showStems"
          >
            {{ showStems ? '隐藏题干' : '显示题干' }}
          </el-button>
        </div>

<!--        <p v-if="!showStems" class="questions-hidden-hint">-->
<!--          题干已隐藏，选项仍可作答。需要时可点「显示题干」。-->
<!--        </p>-->

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

        <div v-if="activeQuizSection" class="section-body le-card">
          <div v-if="sectionAudioUrl" class="section-audio-block">
            <div class="audio-label">本段音频（可选）</div>
            <audio
              :key="activeQuizSection.id"
              :src="sectionAudioUrl"
              controls
              playsinline
              class="section-audio"
            />
          </div>

          <OnlineQuizPanel
            v-if="activeQuizSection.questions.length"
            :key="activeQuizSection.id"
            :title="`${activeGroupTypeLabel} · ${activeQuizSection.title}`"
            subtitle="请听上方听力音频作答；全部选完后提交，即可查看答案、解析与本段原文"
            :questions="activeQuizSection.questions"
            :show-stem="showStems"
            :submit-fn="(answers) => handleSubmitQuiz(answers, activeQuizSection.id)"
            @submitted="onQuizSubmitted"
            @reset="onQuizReset"
          />

          <div v-if="canShowSectionTranscript && hasSectionTranscript" class="section-transcript-after">
            <div class="toggle-wrap toggle-wrap--inline">
              <el-button
                :type="showSectionText ? 'primary' : 'default'"
                round
                size="small"
                @click="showSectionText = !showSectionText"
              >
                {{ showSectionText ? '隐藏本段原文' : '显示本段原文' }}
              </el-button>
            </div>
            <transition name="fade-slide">
              <div v-show="showSectionText" class="subtitle-card le-card section-transcript-card">
                <div class="subtitle-header">
                  <el-icon><Document /></el-icon>
                  本段原文 · {{ activeGroupTypeLabel }} · {{ activeQuizSection?.title }}
                </div>
                <div class="subtitle-content">
                  <p
                    v-for="(para, i) in sectionTranscriptParagraphs"
                    :key="`sec-${i}`"
                    class="subtitle-line"
                    :class="{ 'subtitle-line--dialogue': isDialogueLine(para) }"
                  >{{ para }}</p>
                </div>
              </div>
            </transition>
          </div>
          <p v-else-if="hasSectionTranscript" class="transcript-locked-hint">
            提交本段答案后可查看对应原文
          </p>
        </div>
      </div>
      <div v-else-if="!loading" class="quiz-empty-hint">
        本套暂无在线题目。可先听上方音频；PDF 与资料在页面底部。
      </div>

      <!-- 底部：完成 / 反馈 / PDF / 整卷原文 -->
      <div class="page-footer-tools">
        <div class="complete-actions complete-actions--footer">
          <el-button
            type="success"
            round
            :loading="completing"
            :disabled="completed"
            @click="markComplete(false)"
          >
            {{ completed ? '已完成本卷' : '标记完成' }}
          </el-button>
          <FeedbackEntry :source="feedbackSource" />
        </div>

        <div v-if="paperFileUrl || answerFileUrl" class="pdf-section">
          <div class="pdf-actions">
            <el-button
              v-if="paperFileUrl"
              :type="activePdf === 'paper' ? 'primary' : 'default'"
              round
              class="pdf-action-btn"
              @click="togglePdf('paper')"
            >
              <el-icon><Document /></el-icon>
              {{ activePdf === 'paper' ? '隐藏试卷' : '查看试卷 PDF' }}
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
              {{ activePdf === 'answer' ? '隐藏答案' : '查看答案 PDF' }}
            </el-button>
          </div>

          <transition name="fade-slide">
            <div v-if="activePdf && currentPdfUrl" class="pdf-viewer le-card">
              <div class="pdf-header">
                <span class="pdf-title">{{ activePdf === 'paper' ? '试卷 PDF' : '答案 PDF' }}</span>
                <el-button type="primary" link @click="openDownload(currentPdfUrl)">
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

        <div v-if="hasFullTranscript" class="transcript-footer">
          <div class="toggle-wrap">
            <el-button
              :type="showText ? 'primary' : 'default'"
              round
              class="le-btn-gradient"
              @click="showText = !showText"
            >
              {{ showText ? '隐藏整卷原文' : '显示整卷原文' }}
            </el-button>
          </div>

          <transition name="fade-slide">
            <div v-show="showText" class="subtitle-card le-card full-transcript-card">
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
    </div>

    <el-empty v-else description="暂无音频数据" />
  </PageShell>
</template>

<script setup>
import { ref, onMounted, computed, watch, reactive } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import PageShell from '../components/PageShell.vue'
import PdfViewer from '../components/PdfViewer.vue'
import FeedbackEntry from '../components/FeedbackEntry.vue'
import OnlineQuizPanel from '../components/OnlineQuizPanel.vue'
import QuizSectionTabs from '../components/QuizSectionTabs.vue'
import { getEpisodesByAlbumId, getAlbumById, getCategories, getQuizByAlbumId, submitQuiz } from '../api/Listen.js'
import { recordStudyActivity } from '../api/Study.js'
import { trackEvent } from '../api/Analytics.js'
import { useAudioPlayer } from '../composables/useAudioPlayer.js'
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
/** 各材料是否已提交过（提交后才解锁本段原文） */
const submittedBySection = reactive({})
const sessionStartedAt = Date.now()

const categoryMeta = {
  cet4: 'CET-4',
  cet6: 'CET-6',
}

const toFileUrl = (path) => (path ? `/api/listen${path}` : '')

const availableGroups = computed(() =>
  GROUP_ORDER.filter((g) => quizSections.value.some((s) => s.groupName === g))
)

const groupTabs = computed(() =>
  availableGroups.value.map((g) => ({
    value: g,
    label: formatGroupTabLabel(g, categoryCode.value),
    badge: formatGroupBadge(g),
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
  passagesInActiveGroup.value.map((s, i) => ({
    value: s.id,
    label: formatPassageLabel(s.title, i, s.groupName, categoryCode.value),
    title: s.title,
    badge: String(i + 1),
  }))
)

const activeQuizSection = computed(() =>
  passagesInActiveGroup.value.find((s) => s.id === activeQuizSectionId.value)
  || passagesInActiveGroup.value[0]
  || null
)
const sectionAudioUrl = computed(() => toFileUrl(activeQuizSection.value?.audioUrl))

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

const openDownload = (url) => {
  window.open(url, '_blank', 'noopener,noreferrer')
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

const hasSectionTranscript = computed(() =>
  Boolean(activeQuizSection.value?.transcript?.trim())
)

const canShowSectionTranscript = computed(() => {
  const id = activeQuizSection.value?.id
  return Boolean(id && submittedBySection[id])
})

const sectionTranscriptParagraphs = computed(() =>
  formatTranscriptParagraphs(activeQuizSection.value?.transcript || '')
)

const fullTranscriptSource = computed(() => {
  // 整卷原文仅用 Episode 字幕；各段题目原文须提交后才在题目下方可见
  return subtitleText.value?.trim() || ''
})

const hasFullTranscript = computed(() => Boolean(fullTranscriptSource.value.trim()))

const fullTranscriptParagraphs = computed(() =>
  formatTranscriptParagraphs(fullTranscriptSource.value)
)

watch(activeQuizSectionId, () => {
  // 已提交过的段落自动展开原文；未提交则收起
  showSectionText.value = canShowSectionTranscript.value
})

watch(activeGroupName, (g) => {
  const first = quizSections.value.find((s) => s.groupName === g)
  activeQuizSectionId.value = first?.id || ''
})

watch(canShowSectionTranscript, (ok) => {
  if (ok) showSectionText.value = true
  else showSectionText.value = false
})

const resolveCategory = async (categoryId) => {
  if (!categoryId) return
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

const markComplete = async (fromAuto = false) => {
  if (!albumId || completing.value || completed.value) return

  trackEvent('listen_complete', `/exam?albumId=${albumId}`)

  if (!localStorage.getItem('token')) {
    if (!fromAuto) {
      ElMessage.warning('登录后可记录学习进度')
      router.push({ name: 'login', query: { redirect: route.fullPath } })
    }
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
  } catch (e) {
    // 错误已在拦截器提示
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

const handleSubmitQuiz = async (answers, sectionId) => {
  const res = await submitQuiz(albumId, answers, sectionId)
  return {
    correctCount: res.correctCount ?? res.CorrectCount ?? 0,
    scorePercent: res.scorePercent ?? res.ScorePercent ?? 0,
    results: res.results || res.Results || [],
  }
}

const onQuizSubmitted = async (res) => {
  trackEvent('listen_quiz_submit', `/exam?albumId=${albumId}`)
  const sec = activeQuizSection.value
  if (sec?.id) {
    submittedBySection[sec.id] = true
    showSectionText.value = true
  }
  const total = sec?.questions?.length || res.total || 0
  if (!localStorage.getItem('token')) {
    ElMessage.success(`答题完成：${res.correctCount}/${total}`)
    return
  }
  try {
    await recordStudyActivity({
      activityType: 'listen_quiz',
      contentId: String(albumId),
      title: `${albumTitle.value}${sec ? ` · ${sec.title}` : ''} · 在线答题`,
      category: categoryLabel.value,
      durationSeconds: getDurationSeconds(),
    })
    ElMessage.success(`答题完成：${res.correctCount}/${total}`)
  } catch {
    /* ignore */
  }
}

const onQuizReset = () => {
  const id = activeQuizSection.value?.id
  if (id) {
    submittedBySection[id] = false
    showSectionText.value = false
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
    activeGroupName.value = availableGroups.value[0] || 'Section A'
    activeQuizSectionId.value =
      quizSections.value.find((s) => s.groupName === activeGroupName.value)?.id
      || quizSections.value[0]?.id
      || ''
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

.complete-actions {
  margin-top: 16px;
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: center;
  gap: 12px;
}

.complete-actions--footer {
  margin-top: 0;
  margin-bottom: 8px;
}

.page-footer-tools {
  margin-top: 36px;
  padding-top: 24px;
  border-top: 1px solid var(--le-border);
  max-width: 720px;
}

.pdf-section {
  margin-top: 20px;
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

.toggle-wrap {
  text-align: center;
  margin: 20px 0;
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 10px;
}

.transcript-footer {
  margin-top: 28px;
  max-width: 720px;
}

.subtitle-card {
  overflow: hidden;
  margin-bottom: 14px;
}

.full-transcript-card {
  margin-top: 0;
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

.section-quiz-head h3 {
  margin: 0;
  font-size: 17px;
  font-weight: 650;
}

.section-quiz-head p {
  margin: 6px 0 14px;
  font-size: 13px;
  color: var(--le-text-muted);
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

.toggle-wrap--questions {
  margin: 8px 0 16px;
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
