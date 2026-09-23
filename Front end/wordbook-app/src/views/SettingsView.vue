<template>
  <div class="settings-page">
    <section v-if="loggedIn" class="section">
      <h2>单词本切换</h2>
      <p class="desc">登录后可同时使用云端与本地单词本，可通过下方同步互相备份</p>
      <div class="mode-options">
        <button
          class="mode-btn"
          :class="{ active: activeWordbook === 'server' }"
          @click="switchWordbook('server')"
        >
          <span class="mode-icon">☁️</span>
          <div>
            <strong>云端单词本</strong>
            <small>同步到服务器，多设备共享</small>
          </div>
        </button>
        <button
          class="mode-btn"
          :class="{ active: activeWordbook === 'local' }"
          @click="switchWordbook('local')"
        >
          <span class="mode-icon">📱</span>
          <div>
            <strong>本地单词本</strong>
            <small>仅存手机，可离线使用</small>
          </div>
        </button>
      </div>
    </section>

    <section v-else class="section">
      <h2>离线模式</h2>
      <p class="desc">当前为离线模式，仅可使用本地单词本。登录后可同时使用云端单词本。</p>
      <button class="btn-primary" @click="router.push({ name: 'login', query: { from: 'settings' } })">
        登录账号
      </button>
    </section>

    <section v-if="loggedIn" class="section">
      <h2>数据同步</h2>
      <p class="desc">
        将「当前云端单词本」与本地互相备份。按单词文本合并：已存在的会跳过，不会覆盖复习进度。
      </p>
      <div class="sync-actions">
        <button class="btn-primary" :disabled="busy" @click="downloadFromCloud">
          {{ syncing === 'download' ? '下载中...' : '云端 → 本地' }}
        </button>
        <button class="btn-primary btn-secondary" :disabled="busy" @click="uploadToCloud">
          {{ syncing === 'upload' ? '上传中...' : '本地 → 云端' }}
        </button>
      </div>
      <p v-if="syncHint" class="sync-hint">{{ syncHint }}</p>
    </section>

    <section class="section">
      <h2>意见反馈</h2>
      <p class="desc">功能建议、问题反馈或内容相关意见，均可提交给我们。</p>
      <button class="btn-primary" @click="router.push({ name: 'feedback' })">去反馈</button>
    </section>

    <section class="section">
      <h2>导入 / 导出</h2>
      <p class="desc">
        导出当前单词本（{{ activeLabel }}）为 JSON 或 TXT；也可从文件导入并合并到当前单词本。
      </p>
      <div class="sync-actions">
        <button class="btn-primary" :disabled="busy" @click="exportWords('json')">
          {{ syncing === 'export-json' ? '导出中...' : '导出为 JSON' }}
        </button>
        <button class="btn-primary btn-secondary" :disabled="busy" @click="exportWords('txt')">
          {{ syncing === 'export-txt' ? '导出中...' : '导出为 TXT' }}
        </button>
        <button class="btn-primary btn-secondary" :disabled="busy" @click="pickImportFile">
          {{ syncing === 'import' ? '导入中...' : '从文件导入' }}
        </button>
      </div>
      <input
        ref="fileInput"
        type="file"
        class="file-input"
        accept=".json,.txt,application/json,text/plain"
        @change="onImportFile"
      />
      <p v-if="ioHint" class="sync-hint">{{ ioHint }}</p>
    </section>

    <section v-if="loggedIn" class="section">
      <h2>账号</h2>
      <div class="account-info">
        <span>已登录：{{ username }}</span>
        <button class="btn-outline" @click="logout">退出登录</button>
      </div>
    </section>

    <section v-if="activeWordbook === 'local'" class="section danger">
      <h2>本地数据</h2>
      <p class="desc">清除后无法恢复，请谨慎操作</p>
      <button class="btn-danger" @click="clearLocal">清除本地单词本</button>
    </section>

    <section class="section">
      <h2>关于</h2>
      <p class="desc">
        EaseWord 听易词 · 与
        <a href="https://your-domain.com" target="_blank" rel="noopener">ListenEase</a>
        网站共用云端账号
      </p>
      <p class="version">当前版本 {{ appVersionLabel }}</p>
      <p class="update-notice">
        由于 APK 签名变动，本次需先卸载旧版本再安装新包；请提前将本地单词上传到云端。之后更新可直接覆盖安装，无需再卸载。
      </p>
      <div class="sync-actions" style="margin-top: 12px">
        <button
          class="btn-primary"
          :disabled="busy"
          @click="handleCheckUpdate"
        >
          {{ syncing === 'checking' ? '检查中...' : '检查更新' }}
        </button>
        <button
          v-if="pendingUpdate"
          class="btn-primary btn-secondary"
          :disabled="busy"
          @click="handleInstallUpdate"
        >
          {{ syncing === 'updating' ? `下载中 ${updateProgress}%` : `下载并安装 ${pendingUpdate.versionName}` }}
        </button>
      </div>
      <p v-if="updateHint" class="sync-hint">{{ updateHint }}</p>
    </section>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import {
  getActiveWordbook,
  setActiveWordbook,
  getAuth,
  clearAuth,
  isLoggedIn,
  setOfflineOnly,
} from '../services/appSettings'
import { clearAllData } from '../services/localWordStore'
import { downloadCloudToLocal, uploadLocalToCloud } from '../services/wordSyncService'
import {
  exportActiveWordbook,
  importIntoActiveWordbook,
  parseWordFile,
  readFileAsText,
} from '../services/wordImportExport'
import {
  checkForUpdate,
  downloadAndInstallUpdate,
  getLocalAppInfo,
} from '../services/appUpdateService'
import { WORDBOOK_TYPES } from '../config'
import { showToast } from '../utils/toast'

const router = useRouter()
const activeWordbook = ref('local')
const username = ref('')
const loggedIn = ref(false)
const syncing = ref('')
const syncHint = ref('')
const ioHint = ref('')
const fileInput = ref(null)
const appVersionLabel = ref('...')
const pendingUpdate = ref(null)
const updateHint = ref('')
const updateProgress = ref(0)

const busy = computed(() => !!syncing.value)
const activeLabel = computed(() =>
  activeWordbook.value === 'local' ? '本地单词本' : '云端单词本'
)
const load = async () => {
  activeWordbook.value = await getActiveWordbook()
  loggedIn.value = await isLoggedIn()
  const auth = await getAuth()
  username.value = auth.username || ''
  try {
    const info = await getLocalAppInfo()
    appVersionLabel.value = `${info.version} (${info.build})`
  } catch {
    appVersionLabel.value = '未知'
  }
}

const switchWordbook = async (type) => {
  if (type === activeWordbook.value) return
  const label = type === 'local' ? '本地单词本' : '云端单词本'
  if (!confirm(`切换到「${label}」？\n\n本地与云端数据相互独立，可用「数据同步」互相备份。`)) return

  await setActiveWordbook(type === 'local' ? WORDBOOK_TYPES.local : WORDBOOK_TYPES.server)
  activeWordbook.value = type
  showToast(`已切换到${label}`)
  router.push('/')
}

const downloadFromCloud = async () => {
  if (
    !confirm(
      '将当前云端单词本合并到本地？\n\n本地已有的单词会跳过；仅补全本地空着的释义/例句。'
    )
  ) {
    return
  }

  syncing.value = 'download'
  syncHint.value = ''
  try {
    const r = await downloadCloudToLocal()
    if (r.total === 0) {
      syncHint.value = '云端暂无单词'
      showToast('云端暂无单词')
      return
    }
    syncHint.value = `云端共 ${r.total} 个：新增 ${r.added}，补全 ${r.updated}，跳过 ${r.skipped}`
    showToast(`已下载：新增 ${r.added} 个`)
  } catch (e) {
    showToast(typeof e === 'string' ? e : '下载失败')
  } finally {
    syncing.value = ''
  }
}

const uploadToCloud = async () => {
  if (
    !confirm(
      '将本地单词上传到当前云端单词本？\n\n云端已有的单词会跳过。\n新上传的单词在云端复习进度会重新开始。'
    )
  ) {
    return
  }

  syncing.value = 'upload'
  syncHint.value = ''
  try {
    const r = await uploadLocalToCloud()
    if (r.total === 0) {
      syncHint.value = '本地暂无单词'
      showToast('本地暂无单词')
      return
    }
    const failPart = r.failed ? `，失败 ${r.failed}` : ''
    syncHint.value = `本地共 ${r.total} 个：上传 ${r.added}，跳过 ${r.skipped}${failPart}`
    showToast(r.failed ? `上传完成，失败 ${r.failed} 个` : `已上传：新增 ${r.added} 个`)
  } catch (e) {
    showToast(typeof e === 'string' ? e : '上传失败')
  } finally {
    syncing.value = ''
  }
}

const logout = async () => {
  if (!confirm('确定退出登录？退出后需重新登录或选择离线模式。')) return
  await clearAuth()
  await setOfflineOnly(false)
  username.value = ''
  loggedIn.value = false
  showToast('已退出')
  router.replace('/login')
}

const exportWords = async (format) => {
  syncing.value = `export-${format}`
  ioHint.value = ''
  try {
    const r = await exportActiveWordbook(format)
    ioHint.value = `已导出 ${r.count} 个单词（${r.filename}）`
    showToast(`已导出 ${r.count} 个单词`)
  } catch (e) {
    showToast(typeof e === 'string' ? e : e?.message || '导出失败')
  } finally {
    syncing.value = ''
  }
}

const pickImportFile = () => {
  fileInput.value?.click()
}

const onImportFile = async (event) => {
  const file = event.target?.files?.[0]
  event.target.value = ''
  if (!file) return

  if (
    !confirm(
      `将「${file.name}」导入到当前「${activeLabel.value}」？\n\n已存在的单词会跳过，不会覆盖复习进度。`
    )
  ) {
    return
  }

  syncing.value = 'import'
  ioHint.value = ''
  try {
    const text = await readFileAsText(file)
    const words = parseWordFile(text, file.name)
    const r = await importIntoActiveWordbook(words)
    const failPart = r.failed ? `，失败 ${r.failed}` : ''
    const updatePart = r.updated ? `，补全 ${r.updated}` : ''
    ioHint.value = `文件 ${r.total} 个：新增 ${r.added}${updatePart}，跳过 ${r.skipped}${failPart}`
    showToast(`导入完成：新增 ${r.added} 个`)
  } catch (e) {
    showToast(typeof e === 'string' ? e : e?.message || '导入失败')
  } finally {
    syncing.value = ''
  }
}

const clearLocal = async () => {
  if (!confirm('确定清除所有本地单词？此操作不可恢复。')) return
  await clearAllData()
  showToast('本地数据已清除')
  router.push('/')
}

const handleCheckUpdate = async () => {
  syncing.value = 'checking'
  updateHint.value = ''
  pendingUpdate.value = null
  try {
    const result = await checkForUpdate()
    if (!result.hasUpdate) {
      updateHint.value = `已是最新版（${result.local.version}）`
      showToast('已是最新版本')
      return
    }
    pendingUpdate.value = result.remote
    updateHint.value = [
      `发现新版本 ${result.remote.versionName}`,
      result.remote.changelog ? `更新说明：${result.remote.changelog}` : '',
    ]
      .filter(Boolean)
      .join('\n')
    showToast(`发现新版本 ${result.remote.versionName}`)
  } catch (e) {
    showToast(typeof e === 'string' ? e : e?.message || '检查更新失败')
  } finally {
    syncing.value = ''
  }
}

const handleInstallUpdate = async () => {
  if (!pendingUpdate.value) return
  if (
    !confirm(
      `下载并安装 ${pendingUpdate.value.versionName}？\n\n下载完成后请在系统弹窗中确认安装。`
    )
  ) {
    return
  }

  syncing.value = 'updating'
  updateProgress.value = 0
  try {
    await downloadAndInstallUpdate(pendingUpdate.value, (p) => {
      updateProgress.value = p
    })
    updateHint.value = '已打开安装界面，请按系统提示完成安装'
    showToast('请确认安装更新')
  } catch (e) {
    showToast(typeof e === 'string' ? e : e?.message || '更新失败')
  } finally {
    syncing.value = ''
    updateProgress.value = 0
  }
}

onMounted(load)
</script>

<style scoped>
.settings-page {
  padding: 16px;
}

.section {
  background: var(--bg-card);
  border: 1px solid var(--border);
  border-radius: 14px;
  padding: 16px;
  margin-bottom: 16px;
}

.section h2 {
  font-size: 16px;
  margin: 0 0 6px;
}

.desc {
  font-size: 13px;
  color: var(--text-secondary);
  margin: 0 0 14px;
  line-height: 1.5;
}

.mode-options {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.mode-btn {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 14px;
  border: 2px solid var(--border);
  border-radius: 12px;
  background: var(--bg);
  text-align: left;
  cursor: pointer;
}

.mode-btn.active {
  border-color: var(--primary);
  background: #ecf5ff;
}

.mode-icon {
  font-size: 28px;
}

.mode-btn strong {
  display: block;
  font-size: 15px;
  margin-bottom: 2px;
}

.mode-btn small {
  font-size: 12px;
  color: var(--text-muted);
}

.sync-actions {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.btn-secondary {
  background: #fff;
  color: var(--primary);
  border: 1px solid var(--primary);
}

.sync-hint {
  margin: 12px 0 0;
  font-size: 13px;
  color: var(--text-secondary);
  line-height: 1.5;
}

.file-input {
  display: none;
}

.account-info {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 14px;
}

.btn-outline,
.btn-primary,
.btn-danger {
  padding: 10px 16px;
  border-radius: 8px;
  font-size: 14px;
}

.btn-outline {
  background: none;
  border: 1px solid var(--border);
}

.btn-primary {
  width: 100%;
  background: var(--primary);
  color: #fff;
  border: none;
}

.btn-primary:disabled {
  opacity: 0.6;
}

.btn-danger {
  width: 100%;
  background: #fef0f0;
  color: #f56c6c;
  border: 1px solid #fbc4c4;
}

.version {
  font-size: 12px;
  color: var(--text-muted);
  margin: 8px 0 0;
}

.update-notice {
  margin: 10px 0 0;
  padding: 10px 12px;
  font-size: 12px;
  line-height: 1.55;
  color: #8a6d3b;
  background: #fff8e8;
  border-radius: 8px;
}

.sync-hint {
  white-space: pre-line;
}

.section.danger h2 {
  color: #f56c6c;
}
</style>
