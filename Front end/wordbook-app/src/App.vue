<script setup>
import { computed, onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import BottomNav from './components/BottomNav.vue'
import PageTopBar from './components/PageTopBar.vue'
import ToastHost from './components/ToastHost.vue'
import UpdatePromptDialog from './components/UpdatePromptDialog.vue'
import WordSearchFloat from './components/WordSearchFloat.vue'
import {
  checkUpdateOnLaunch,
  downloadAndInstallUpdate,
} from './services/appUpdateService'
import { showToast } from './utils/toast'

const route = useRoute()
const showNav = computed(() => !route.meta.hideNav)
const showWordSearch = computed(() => route.name !== 'login')
const topBar = computed(() => {
  const meta = route.meta.topBar
  if (!meta) return null
  if (route.name === 'login') {
    const title = route.query.mode === 'register' ? '注册' : '登录'
    if (route.query.from === 'settings') {
      return { ...meta, title, showBack: true, backTo: '/settings' }
    }
    return { ...meta, title }
  }
  return meta
})

const updateVisible = ref(false)
const updateRemote = ref(null)
const updateLocalVersion = ref('')
const updateDownloading = ref(false)
const updateProgress = ref(0)

const dismissUpdate = () => {
  if (updateDownloading.value) return
  if (updateRemote.value?.forceUpdate) return
  updateVisible.value = false
}

const handleUpdateNow = async () => {
  if (!updateRemote.value || updateDownloading.value) return
  updateDownloading.value = true
  updateProgress.value = 0
  try {
    await downloadAndInstallUpdate(updateRemote.value, (p) => {
      updateProgress.value = p
    })
    showToast('请确认安装更新')
    if (!updateRemote.value.forceUpdate) {
      updateVisible.value = false
    }
  } catch (e) {
    showToast(typeof e === 'string' ? e : e?.message || '更新失败')
  } finally {
    updateDownloading.value = false
    updateProgress.value = 0
  }
}

onMounted(async () => {
  const result = await checkUpdateOnLaunch()
  if (!result?.hasUpdate) return
  updateLocalVersion.value = result.local.version
  updateRemote.value = result.remote
  updateVisible.value = true
})
</script>

<template>
  <div class="app" :class="{ 'has-nav': showNav }">
    <PageTopBar
      v-if="topBar"
      :title="topBar.title"
      :show-back="topBar.showBack"
      :back-to="topBar.backTo || ''"
    />
    <router-view />
    <BottomNav v-if="showNav" />
    <WordSearchFloat v-if="showWordSearch" />
    <ToastHost />
    <UpdatePromptDialog
      :visible="updateVisible"
      :remote="updateRemote"
      :local-version="updateLocalVersion"
      :downloading="updateDownloading"
      :progress="updateProgress"
      @later="dismissUpdate"
      @update="handleUpdateNow"
    />
  </div>
</template>

<style scoped>
.app {
  min-height: 100vh;
  background: var(--bg);
}

.app.has-nav :deep(.words-page),
.app.has-nav :deep(.review-page),
.app.has-nav :deep(.settings-page),
.app.has-nav :deep(.add-page),
.app.has-nav :deep(.feedback-page) {
  padding-bottom: calc(var(--mobile-nav-h) + env(safe-area-inset-bottom, 0) + 16px);
}
</style>
