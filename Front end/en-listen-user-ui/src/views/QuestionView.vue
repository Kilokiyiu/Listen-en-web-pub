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

    <div v-else-if="audioUrl" class="player-section">
      <div class="audio-card le-card">
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
        <p class="audio-tip"><el-icon><InfoFilled /></el-icon> 点击播放，开启听力训练</p>
        <p class="audio-tip audio-tip--sub">锁屏后可从锁屏界面继续播放；回到页面会自动恢复进度</p>
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
            {{ activePdf === 'paper' ? '隐藏试卷' : '查看试卷' }}
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
            {{ activePdf === 'answer' ? '隐藏答案' : '查看答案' }}
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

      <div class="toggle-wrap">
        <el-button :type="showText ? 'primary' : 'default'" round class="le-btn-gradient" @click="showText = !showText">
          {{ showText ? '隐藏原文' : '显示原文' }}
        </el-button>
      </div>

      <transition name="fade-slide">
        <div v-show="showText" class="subtitle-card le-card">
          <div class="subtitle-header"><el-icon><Document /></el-icon> 听力原文</div>
          <div class="subtitle-content">
            <p v-for="(line, i) in subtitleLines" :key="i" class="subtitle-line">{{ line }}</p>
            <el-empty v-if="subtitleLines.length === 0" description="暂无原文" :image-size="64" />
          </div>
        </div>
      </transition>
    </div>

    <el-empty v-else description="暂无音频数据" />
  </PageShell>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRoute } from 'vue-router'
import PageShell from '../components/PageShell.vue'
import PdfViewer from '../components/PdfViewer.vue'
import { getEpisodesByAlbumId, getAlbumById } from '../api/Listen.js'
import { useAudioPlayer } from '../composables/useAudioPlayer.js'

const route = useRoute()
const albumId = route.query.albumId
const audioRef = ref(null)

const albumTitle = ref('听力真题')
const audioUrl = ref('')
const paperFileUrl = ref('')
const answerFileUrl = ref('')
const loading = ref(true)
const showText = ref(false)
const subtitleText = ref('')
const activePdf = ref('')

const toFileUrl = (path) => (path ? `/api/listen${path}` : '')

const currentPdfUrl = computed(() => {
  if (activePdf.value === 'paper') return paperFileUrl.value
  if (activePdf.value === 'answer') return answerFileUrl.value
  return ''
})

const togglePdf = (type) => {
  activePdf.value = activePdf.value === type ? '' : type
}

const openDownload = (url) => {
  window.open(url, '_blank', 'noopener,noreferrer')
}

const subtitleLines = computed(() => {
  if (!subtitleText.value) return []
  try {
    const parsed = JSON.parse(subtitleText.value)
    if (Array.isArray(parsed)) return parsed.map(item => item.text || '').filter(Boolean)
  } catch {
    return subtitleText.value.split('\n').filter(t => t.trim())
  }
  return []
})

const loadEpisode = async () => {
  if (!albumId) { loading.value = false; return }
  try {
    const [episodes, album] = await Promise.all([
      getEpisodesByAlbumId(albumId),
      getAlbumById(albumId).catch(() => null)
    ])
    if (album?.name) {
      albumTitle.value = album.name.chinese || album.name.Chinese || album.name || '听力真题'
    }
    paperFileUrl.value = toFileUrl(album?.paperFileUrl || album?.PaperFileUrl)
    answerFileUrl.value = toFileUrl(album?.answerFileUrl || album?.AnswerFileUrl)

    const ep = (episodes || [])[0]
    if (ep) {
      if (!album?.name) {
        albumTitle.value = ep.name?.chinese || ep.name || '听力真题'
      }
      audioUrl.value = toFileUrl(ep.audioUrl || ep.AudioUrl)
      subtitleText.value = ep.Subtitle || ep.subtitle || ''
    }
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
}

.subtitle-card {
  overflow: hidden;
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
  margin: 0 0 10px;
  line-height: 1.75;
  font-size: 15px;
}

.fade-slide-enter-active, .fade-slide-leave-active { transition: all 0.25s ease; }
.fade-slide-enter-from, .fade-slide-leave-to { opacity: 0; transform: translateY(-8px); }
</style>
