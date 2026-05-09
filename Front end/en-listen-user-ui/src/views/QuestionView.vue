<template>
  <div class="exam-detail-page">
    <!-- 顶部导航 -->
    <div class="detail-header">
      <el-button text @click="router.back()" class="back-btn">
        <el-icon><ArrowLeft /></el-icon> 返回列表
      </el-button>
    </div>

    <!-- 标题区 -->
    <div class="title-section">
      <div class="title-icon">
        <el-icon :size="28" color="#409eff"><Headset /></el-icon>
      </div>
      <h1 class="page-title">{{ albumTitle }}</h1>
    </div>

    <!-- 加载中 -->
    <div v-if="loading" class="loading-wrapper">
      <el-icon class="is-loading" :size="32"><Loading /></el-icon>
      <span>加载中...</span>
    </div>

    <!-- 音频播放器 -->
    <div v-else-if="audioUrl" class="audio-card">
      <div class="audio-card-inner">
        <div class="audio-visual">
          <div class="audio-wave" v-for="i in 20" :key="i" :style="{ animationDelay: i * 0.1 + 's' }"></div>
        </div>
        <div class="audio-wrapper">
          <audio
            ref="audioRef"
            :src="audioUrl"
            controls
            class="audio-player"
          ></audio>
        </div>
        <div class="audio-tips">
          <el-icon><InfoFilled /></el-icon>
          <span>点击播放按钮，开启听力训练</span>
        </div>
      </div>

      <!-- 显示原文按钮 -->
      <div class="text-toggle-wrapper">
        <el-button
          :type="showText ? 'primary' : 'default'"
          :icon="showText ? Hide : View"
          @click="showText = !showText"
          class="text-toggle-btn"
          round
        >
          {{ showText ? '隐藏原文' : '显示原文' }}
        </el-button>
      </div>

      <!-- 原文内容 -->
      <transition name="fade-slide">
        <div v-show="showText" class="subtitle-card">
          <div class="subtitle-header">
            <el-icon><Document /></el-icon>
            <span>听力原文</span>
          </div>
          <div class="subtitle-content">
            <template v-if="subtitleLines.length > 0">
              <p v-for="(line, index) in subtitleLines" :key="index" class="subtitle-line">
                {{ line }}
              </p>
            </template>
            <template v-else>
              <el-empty description="暂无原文" :image-size="80" />
            </template>
          </div>
        </div>
      </transition>
    </div>

    <!-- 无数据 -->
    <div v-else class="empty-wrapper">
      <el-empty description="暂无音频数据" />
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getEpisodesByAlbumId } from '../api/Listen.js'
import { View, Hide, Document } from '@element-plus/icons-vue'

const route = useRoute()
const router = useRouter()
const albumId = route.query.albumId

const albumTitle = ref('听力真题')
const audioUrl = ref('')
const loading = ref(true)
const audioRef = ref(null)
const showText = ref(false)
const subtitleText = ref('')

// 解析字幕 JSON，提取每行文本
const subtitleLines = computed(() => {
  if (!subtitleText.value) return []
  try {
    const parsed = JSON.parse(subtitleText.value)
    if (Array.isArray(parsed)) {
      return parsed.map(item => item.text || '').filter(t => t)
    }
  } catch {
    // 如果不是 JSON，按换行分割
    return subtitleText.value.split('\n').filter(t => t.trim())
  }
  return []
})

// 加载音频数据
const loadEpisode = async () => {
  if (!albumId) {
    loading.value = false
    return
  }
  try {
    const data = await getEpisodesByAlbumId(albumId)
    const episodes = data || []
    if (episodes.length > 0) {
      const ep = episodes[0]
      albumTitle.value = ep.name?.chinese || ep.name || '听力真题'
      // 拼接完整音频URL
      audioUrl.value = ep.audioUrl ? `/api/listen${ep.audioUrl}` : ''
      // 保存原文（兼容 PascalCase 和 camelCase）
      subtitleText.value = ep.Subtitle || ep.subtitle || ''
    }
  } catch (e) {
    console.error('获取音频失败', e)
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadEpisode()
})
</script>

<style scoped>
.exam-detail-page {
  padding: 0;
}

/* 顶部导航 */
.detail-header {
  padding: 16px 24px;
  border-bottom: 1px solid #eef1f6;
}

.back-btn {
  color: var(--text-secondary) !important;
  font-size: 14px;
}

.back-btn:hover {
  color: var(--accent-blue) !important;
}

/* 标题区 */
.title-section {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 28px 28px 20px;
}

.title-icon {
  width: 48px;
  height: 48px;
  border-radius: 14px;
  background: linear-gradient(135deg, rgba(64, 158, 255, 0.1) 0%, rgba(0, 168, 232, 0.1) 100%);
  display: flex;
  align-items: center;
  justify-content: center;
}

.page-title {
  font-size: 24px;
  font-weight: 700;
  color: var(--text-primary);
}

/* 加载中 */
.loading-wrapper {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  padding: 80px 0;
  color: var(--text-muted);
  font-size: 14px;
}

/* 音频卡片 */
.audio-card {
  margin: 0 28px 20px;
  background: linear-gradient(135deg, #f8faff 0%, #f0f5ff 100%);
  border: 1px solid #e4ecf7;
  border-radius: 16px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.06);
  overflow: hidden;
}

.audio-card-inner {
  padding: 32px 28px 24px;
}

/* 音波动画装饰 */
.audio-visual {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 4px;
  height: 40px;
  margin-bottom: 20px;
}

.audio-wave {
  width: 4px;
  border-radius: 2px;
  background: linear-gradient(180deg, var(--accent-blue), var(--accent-cyan));
  animation: wave 1.2s ease-in-out infinite;
  opacity: 0.4;
}

@keyframes wave {
  0%, 100% { height: 8px; opacity: 0.3; }
  50% { height: 32px; opacity: 0.7; }
}

.audio-wrapper {
  display: flex;
  justify-content: center;
  padding: 8px 0;
}

.audio-player {
  width: 100%;
  max-width: 600px;
  border-radius: 12px;
  outline: none;
}

.audio-tips {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  color: var(--text-muted);
  font-size: 13px;
  margin-top: 16px;
}

.audio-tips :deep(.el-icon) {
  color: var(--accent-blue);
}

/* 显示原文按钮 */
.text-toggle-wrapper {
  display: flex;
  justify-content: center;
  margin: 20px 28px 0;
}

.text-toggle-btn {
  padding: 10px 28px;
  font-size: 14px;
  font-weight: 500;
}

/* 原文卡片 */
.subtitle-card {
  margin: 16px 28px 28px;
  background: #fff;
  border: 1px solid #e8ecf4;
  border-radius: 16px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.04);
  overflow: hidden;
}

.subtitle-header {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 14px 20px;
  background: linear-gradient(135deg, #f8faff 0%, #f0f5ff 100%);
  border-bottom: 1px solid #e8ecf4;
  font-size: 15px;
  font-weight: 600;
  color: var(--text-primary);
}

.subtitle-header :deep(.el-icon) {
  color: var(--accent-blue);
}

.subtitle-content {
  padding: 20px 24px;
  max-height: 500px;
  overflow-y: auto;
}

.subtitle-line {
  margin: 0 0 10px 0;
  font-size: 15px;
  line-height: 1.8;
  color: var(--text-primary);
  text-align: justify;
}

.subtitle-line:last-child {
  margin-bottom: 0;
}

/* 动画 */
.fade-slide-enter-active,
.fade-slide-leave-active {
  transition: all 0.3s ease;
}

.fade-slide-enter-from,
.fade-slide-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}

/* 无数据 */
.empty-wrapper {
  padding: 60px 0;
}
</style>
