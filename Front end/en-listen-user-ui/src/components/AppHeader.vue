<template>
  <header class="app-header">
    <div class="header-inner">
      <div class="logo" @click="router.push('/')">
        <div class="logo-icon" aria-hidden="true">听</div>
        <span class="logo-text">ListenEase</span>
      </div>

      <!-- Desktop nav -->
      <nav class="desktop-nav" aria-label="站点导航">
        <button
          v-for="link in navLinks"
          :key="link.path"
          type="button"
          class="nav-link"
          :class="{ active: isNavActive(link) }"
          @click="goNav(link.path)"
        >
          {{ link.label }}
        </button>
      </nav>

      <div class="header-actions">
        <button
          type="button"
          class="action-chip action-chip--search"
          :class="{ active: route.name === 'paperSearch' }"
          aria-label="搜索试卷"
          @click="goSearch"
        >
          <svg viewBox="0 0 24 24" width="16" height="16" aria-hidden="true"><path fill="currentColor" d="M15.5 14h-.79l-.28-.27A6.47 6.47 0 0 0 16 9.5 6.5 6.5 0 1 0 9.5 16c1.61 0 3.09-.59 4.23-1.57l.27.28v.79l5 4.99L20.49 19l-4.99-5zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 11.99 14 9.5 14z"/></svg>
          <span class="hide-mobile">搜索</span>
        </button>

        <button
          type="button"
          class="action-chip"
          :class="{ active: route.name === 'about' }"
          aria-label="关于"
          @click="router.push('/about')"
        >
          关于
        </button>

        <button
          type="button"
          class="action-chip action-chip--icon"
          aria-label="通知"
          @click="announcementVisible = true"
        >
          <svg viewBox="0 0 24 24" width="16" height="16" aria-hidden="true"><path fill="currentColor" d="M12 22a2 2 0 0 0 2-2h-4a2 2 0 0 0 2 2zm6-6V11a6 6 0 1 0-12 0v5l-2 2v1h16v-1l-2-2z"/></svg>
        </button>

        <template v-if="!isLoggedIn">
          <button type="button" class="action-chip action-chip--login" @click="router.push('/login')">
            登录
          </button>
          <button
            type="button"
            class="action-chip action-chip--register"
            @click="router.push('/login?mode=register')"
          >
            注册
          </button>
        </template>

        <div v-else class="user-menu">
          <button type="button" class="user-btn" @click="userMenuOpen = !userMenuOpen">
            <span class="user-avatar">{{ avatarLetter }}</span>
            <span class="username hide-mobile">{{ username }}</span>
          </button>
          <div v-if="userMenuOpen" class="user-menu__list">
            <button type="button" @click="handleCommand('profile')">个人中心 / 学习记录</button>
            <button type="button" @click="handleCommand('wordRoots')">词根学习</button>
            <button type="button" @click="handleCommand('myWords')">我的单词本</button>
            <button type="button" @click="handleCommand('logout')">退出登录</button>
          </div>
        </div>
      </div>
    </div>

    <Teleport to="body">
    <div v-if="announcementVisible" class="announce-modal" @click.self="closeAnnouncement">
      <div class="announce-modal__panel" role="dialog" aria-label="平台公告">
        <h3>平台公告</h3>
      <div v-if="siteStatus" class="site-status-card" :class="`site-status-card--${siteStatusClass}`">
        <div class="site-status-card__head">
          <span class="site-status-dot" />
          <strong>{{ siteStatus.statusText || '网站运行正常' }}</strong>
          <span class="status-pill">{{ siteStatusLabel }}</span>
        </div>
        <p v-if="siteStatus.extraInfo" class="site-status-card__extra">{{ siteStatus.extraInfo }}</p>
        <p class="site-status-card__meta">
          状态更新：{{ formatStatusTime(siteStatus.updatedAt) }}
          <span v-if="serverTimeText"> · 服务器时间 {{ serverTimeText }}</span>
        </p>
      </div>

      <div class="announcement-list">
        <article v-for="item in announcements" :key="item.id || item.title" class="announcement-item">
          <div class="announcement-item__meta">
            <time>{{ item.date }}</time>
            <span v-if="item.isPinned" class="status-pill">置顶</span>
          </div>
          <h4>{{ item.title }}</h4>
          <p>{{ item.content }}</p>
        </article>
        <p v-if="!announcementLoading && announcements.length === 0">暂无公告</p>
      </div>
      <div class="announcement-footer">
        <label><input v-model="noShowToday" type="checkbox" /> 今日不再显示</label>
        <button type="button" class="action-chip" @click="closeAnnouncement">关闭</button>
      </div>
      </div>
    </div>
    </Teleport>
  </header>
</template>

<script setup>
import { computed, ref, onMounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { getAnnouncementBoard } from '../api/Announcement'

const router = useRouter()
const route = useRoute()

const goNav = (path) => {
  if (route.path === path) return
  router.push(path).catch(() => {})
}

const goSearch = () => {
  if (route.name === 'paperSearch') return
  router.push({ name: 'paperSearch' }).catch(() => {})
}

const ANNOUNCE_DATE_KEY = 'announcement_last_shown_date'
const ANNOUNCE_VERSION_KEY = 'announcement_board_version'

const navLinks = [
  { path: '/', label: '首页', names: ['home'] },
  { path: '/daily', label: '每日阅读', names: ['dailyArticle'] },
  { path: '/kaoyan', label: '考研英语', names: ['kaoyan', 'kaoyanPaper'] },
  { path: '/bbc-news', label: 'BBC', names: ['bbcNews'] },
  { path: '/word-roots', label: '词根', names: ['wordRoots', 'wordRootDetail'] },
  { path: '/my-words', label: '单词本', names: ['myWords', 'wordReview'] },
]

const isNavActive = (link) => {
  if (link.path === '/' && route.path === '/') return true
  return link.names.includes(route.name) || route.path.startsWith(link.path)
}

const announcementVisible = ref(false)
const announcementLoading = ref(false)
const noShowToday = ref(false)
const announcements = ref([])
const siteStatus = ref(null)
const boardVersion = ref('')
const serverTime = ref('')

const isLoggedIn = ref(false)
const username = ref('')
const userMenuOpen = ref(false)
const avatarLetter = computed(() => (username.value || '用').slice(0, 1))

const siteStatusClass = computed(() => {
  const s = (siteStatus.value?.status || 'Normal').toLowerCase()
  if (s === 'maintenance') return 'maintenance'
  if (s === 'notice') return 'notice'
  return 'normal'
})

const siteStatusLabel = computed(() => {
  const s = siteStatus.value?.status || 'Normal'
  if (s === 'Maintenance') return '维护中'
  if (s === 'Notice') return '提示'
  return '正常'
})

const serverTimeText = computed(() => formatStatusTime(serverTime.value, true))

const closeAnnouncement = () => {
  announcementVisible.value = false
  markAnnouncementSeen()
}

const todayKey = () => {
  const d = new Date()
  const m = String(d.getMonth() + 1).padStart(2, '0')
  const day = String(d.getDate()).padStart(2, '0')
  return `${d.getFullYear()}-${m}-${day}`
}

const formatStatusTime = (value, timeOnly = false) => {
  if (!value) return ''
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return String(value)
  const pad = (n) => String(n).padStart(2, '0')
  if (timeOnly) return `${pad(d.getHours())}:${pad(d.getMinutes())}`
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`
}

const checkLogin = () => {
  const token = localStorage.getItem('token')
  const savedUser = localStorage.getItem('username')
  isLoggedIn.value = !!(token && savedUser)
  username.value = savedUser || ''
}

const markAnnouncementSeen = () => {
  localStorage.setItem(ANNOUNCE_DATE_KEY, todayKey())
  if (boardVersion.value) {
    localStorage.setItem(ANNOUNCE_VERSION_KEY, boardVersion.value)
  }
  if (noShowToday.value) {
    localStorage.setItem('announcement_hide_until', String(Date.now() + 86400000))
  }
}

const maybeShowDailyAnnouncement = () => {
  const lastDate = localStorage.getItem(ANNOUNCE_DATE_KEY)
  const lastVersion = localStorage.getItem(ANNOUNCE_VERSION_KEY)
  const hasNewContent = boardVersion.value && boardVersion.value !== lastVersion
  if (hasNewContent || lastDate !== todayKey()) {
    announcementVisible.value = true
  }
}

const loadAnnouncementBoard = async () => {
  announcementLoading.value = true
  try {
    const board = await getAnnouncementBoard()
    siteStatus.value = board?.site || null
    announcements.value = board?.announcements || []
    boardVersion.value = board?.version || ''
    serverTime.value = board?.serverTime || ''
    maybeShowDailyAnnouncement()
  } catch {
    // 请求失败时仍保留手动打开能力，不打断页面
  } finally {
    announcementLoading.value = false
  }
}

onMounted(() => {
  checkLogin()
  loadAnnouncementBoard()
})
watch(() => route.path, checkLogin)

const handleCommand = (cmd) => {
  userMenuOpen.value = false
  if (cmd === 'logout') {
    localStorage.removeItem('token')
    localStorage.removeItem('username')
    localStorage.removeItem('userId')
    isLoggedIn.value = false
    router.push('/')
    return
  }
  const map = { profile: '/profile', wordRoots: '/word-roots', myWords: '/my-words' }
  if (map[cmd]) router.push(map[cmd])
}
</script>

<style scoped>
.app-header {
  position: sticky;
  top: 0;
  z-index: 100;
  background: rgba(255, 255, 255, 0.72);
  backdrop-filter: blur(12px) saturate(1.1);
  -webkit-backdrop-filter: blur(12px) saturate(1.1);
  border-bottom: 1px solid var(--le-border);
}

.header-inner {
  max-width: var(--le-max-w);
  margin: 0 auto;
  padding: 0 20px;
  height: var(--le-header-h);
  display: flex;
  align-items: center;
  gap: 16px;
}

.logo {
  display: flex;
  align-items: center;
  gap: 10px;
  cursor: pointer;
  flex-shrink: 0;
}

.logo-icon {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  background: var(--le-gradient);
  display: flex;
  align-items: center;
  justify-content: center;
  color: #fff;
}

.logo-text {
  font-family: var(--le-font-display);
  font-size: 20px;
  font-weight: 700;
  color: var(--le-text);
  letter-spacing: -0.02em;
}

.desktop-nav {
  display: flex;
  gap: 4px;
  flex: 1;
  justify-content: center;
}

.nav-link {
  position: relative;
  border: none;
  background: transparent;
  padding: 8px 16px;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 500;
  color: var(--le-text-secondary);
  cursor: pointer;
  transition: all 0.2s;
  font-family: inherit;
}

.nav-link:hover {
  color: var(--le-primary);
  background: var(--le-gradient-soft);
}

.nav-link.active {
  color: var(--le-primary);
  background: var(--le-gradient-soft);
  font-weight: 600;
}

.nav-link.active::after {
  content: '';
  position: absolute;
  bottom: 2px;
  left: 50%;
  transform: translateX(-50%);
  width: 20px;
  height: 2px;
  background: var(--le-primary);
  border-radius: 2px;
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-shrink: 0;
  margin-left: auto;
}

.action-chip {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-height: 32px;
  padding: 0 12px;
  border-radius: 8px;
  border: 1px solid var(--le-border-strong, #cbd5e1);
  background: #fff;
  color: var(--le-text);
  font-size: 13px;
  font-weight: 600;
  font-family: inherit;
  cursor: pointer;
  transition: border-color 0.15s, background 0.15s, color 0.15s, box-shadow 0.15s;
  -webkit-tap-highlight-color: transparent;
  box-shadow: 0 1px 2px rgba(15, 23, 42, 0.04);
}

.action-chip:hover,
.action-chip.active {
  border-color: var(--le-primary);
  color: var(--le-primary);
  background: var(--le-gradient-soft);
}

.action-chip--icon {
  width: 32px;
  padding: 0;
  color: var(--le-primary);
  border-color: rgba(37, 99, 235, 0.35);
  background: var(--le-gradient-soft);
}

.action-chip--search {
  gap: 6px;
  color: var(--le-primary);
  border-color: rgba(37, 99, 235, 0.35);
  background: var(--le-gradient-soft);
}

.action-chip--login {
  color: var(--le-primary);
  border-color: rgba(37, 99, 235, 0.45);
  background: #fff;
}

.action-chip--register {
  color: #fff;
  border-color: transparent;
  background: var(--le-gradient);
  box-shadow: 0 2px 8px rgba(37, 99, 235, 0.25);
}

.action-chip--register:hover {
  color: #fff;
  opacity: 0.95;
}

.user-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  border: 1px solid var(--le-border-strong, #cbd5e1);
  background: #fff;
  border-radius: 99px;
  padding: 4px 12px 4px 4px;
  cursor: pointer;
  transition: border-color 0.2s;
  box-shadow: 0 1px 2px rgba(15, 23, 42, 0.04);
}

.user-btn:hover {
  border-color: var(--le-primary);
}

.username {
  font-size: 13px;
  color: var(--le-text-secondary);
  max-width: 100px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.site-status-card {
  padding: 14px 16px;
  border-radius: var(--le-radius-sm);
  margin-bottom: 14px;
  border: 1px solid var(--le-border);
  background: #f8fafc;
}

.site-status-card--normal {
  border-color: rgba(34, 197, 94, 0.35);
  background: rgba(34, 197, 94, 0.06);
}

.site-status-card--notice {
  border-color: rgba(245, 158, 11, 0.4);
  background: rgba(245, 158, 11, 0.08);
}

.site-status-card--maintenance {
  border-color: rgba(239, 68, 68, 0.35);
  background: rgba(239, 68, 68, 0.06);
}

.site-status-card__head {
  display: flex;
  align-items: center;
  gap: 8px;
}

.site-status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: #22c55e;
  flex-shrink: 0;
}

.site-status-card--notice .site-status-dot {
  background: #f59e0b;
}

.site-status-card--maintenance .site-status-dot {
  background: #ef4444;
}

.site-status-card__extra {
  margin: 8px 0 0;
  font-size: 13px;
  color: var(--le-text-secondary);
  line-height: 1.5;
}

.site-status-card__meta {
  margin: 8px 0 0;
  font-size: 12px;
  color: var(--le-text-muted);
}

.announcement-item {
  padding: 14px 16px;
  background: var(--le-bg-muted);
  border-radius: var(--le-radius-sm);
  margin-bottom: 10px;
  border-left: 3px solid var(--le-primary);
}

.announcement-item__meta {
  display: flex;
  align-items: center;
  gap: 8px;
}

.announcement-item time {
  font-size: 12px;
  color: var(--le-text-muted);
}

.announcement-item h4 {
  margin: 6px 0 4px;
  font-size: 15px;
}

.announcement-item p {
  margin: 0;
  font-size: 14px;
  color: var(--le-text-secondary);
  line-height: 1.6;
  white-space: pre-wrap;
}

.announcement-footer {
  margin-top: 16px;
  text-align: center;
}

@media (max-width: 768px) {
  .header-inner {
    padding: 0 10px;
    gap: 8px;
  }
  .desktop-nav {
    display: none;
  }
  .hide-mobile {
    display: none !important;
  }
  .logo-text {
    font-size: 16px;
  }
  .header-actions {
    gap: 6px;
  }
  .action-chip {
    min-height: 30px;
    padding: 0 10px;
    font-size: 12px;
  }
  .action-chip--icon {
    width: 30px;
  }
  .action-chip--register {
    padding: 0 12px;
  }
}

.user-menu {
  position: relative;
}

.user-avatar {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background: var(--le-primary);
  color: #fff;
  font-size: 14px;
}

.user-menu__list {
  position: absolute;
  right: 0;
  top: calc(100% + 6px);
  z-index: 30;
  min-width: 180px;
  padding: 6px;
  border-radius: 12px;
  background: #fff;
  box-shadow: 0 8px 24px rgba(15, 23, 42, 0.12);
}

.user-menu__list button {
  display: block;
  width: 100%;
  padding: 8px 10px;
  border: 0;
  border-radius: 8px;
  background: transparent;
  text-align: left;
  font: inherit;
  cursor: pointer;
}

.user-menu__list button:hover {
  background: var(--le-bg-muted);
}

.announce-modal {
  position: fixed;
  inset: 0;
  z-index: 2000;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 24px;
  background: rgba(30, 41, 59, 0.35);
}

.announce-modal__panel {
  width: min(520px, 100%);
  max-height: min(80vh, 720px);
  overflow: auto;
  padding: 20px;
  border-radius: 16px;
  background: #fff;
}

.status-pill {
  display: inline-block;
  padding: 0 8px;
  border-radius: 999px;
  background: #eef2f7;
  font-size: 12px;
  line-height: 22px;
}

.announcement-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}
</style>
