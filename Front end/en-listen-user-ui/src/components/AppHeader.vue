<template>
  <header class="app-header">
    <div class="header-inner">
      <div class="logo" @click="router.push('/')">
        <div class="logo-icon">
          <el-icon :size="18"><Headset /></el-icon>
        </div>
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
          @click="router.push(link.path)"
        >
          {{ link.label }}
        </button>
      </nav>

      <div class="header-actions">
        <el-button circle size="small" class="icon-btn" @click="announcementVisible = true">
          <el-icon><BellFilled /></el-icon>
        </el-button>

        <template v-if="!isLoggedIn">
          <el-button class="hide-mobile" size="small" @click="router.push('/login')">登录</el-button>
          <el-button type="primary" size="small" class="le-btn-gradient" @click="router.push('/login?mode=register')">注册</el-button>
        </template>

        <el-dropdown v-else trigger="click" @command="handleCommand">
          <button type="button" class="user-btn">
            <el-avatar :size="32" :icon="UserFilled" />
            <span class="username hide-mobile">{{ username }}</span>
            <el-icon class="hide-mobile"><ArrowDown /></el-icon>
          </button>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item command="profile">个人中心 / 学习记录</el-dropdown-item>
              <el-dropdown-item command="wordRoots">词根学习</el-dropdown-item>
              <el-dropdown-item command="myWords">我的单词本</el-dropdown-item>
              <el-dropdown-item divided command="logout">退出登录</el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </div>
    </div>

    <el-dialog
      v-model="announcementVisible"
      title="平台公告"
      width="560px"
      append-to-body
      class="announcement-dialog"
      @close="handleCloseAnnouncement"
    >
<div class="admin-info-card">
          <div class="admin-info-title">管理员信息</div>
          <div class="admin-info-row"><span class="label">管理员</span><span>Admin</span></div>
          <div class="admin-info-row"><span class="label">邮箱</span><span>contact@your-domain.com</span></div>
          <div class="admin-info-row"><span class="label">GitHub</span><a href="https://github.com/your-github-org/your-repo" target="_blank" rel="noopener">github.com/your-github-org/your-repo</a></div>
          <div class="admin-info-row"><span class="label">状态</span><span class="status-badge">正常运行</span></div>
        </div>
      <h3 class="ann-section-title">最新公告</h3>
      <div class="announcement-list">
        <article v-for="(item, i) in announcements" :key="i" class="announcement-item">
          <time>{{ item.date }}</time>
          <h4>{{ item.title }}</h4>
          <p>{{ item.content }}</p>
        </article>
      </div>
      <div class="announcement-footer">
        <el-checkbox v-model="noShowToday">今日不再显示</el-checkbox>
      </div>
    </el-dialog>
  </header>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessage } from 'element-plus'
import { BellFilled, UserFilled, ArrowDown } from '@element-plus/icons-vue'

const router = useRouter()
const route = useRoute()

const navLinks = [
  { path: '/', label: '首页', names: ['home'] },
  { path: '/daily', label: '每日阅读', names: ['dailyArticle'] },
  { path: '/bbc-news', label: 'BBC', names: ['bbcNews'] },
  { path: '/word-roots', label: '词根', names: ['wordRoots', 'wordRootDetail'] },
  { path: '/my-words', label: '单词本', names: ['myWords', 'wordReview'] },
]

const isNavActive = (link) => {
  if (link.path === '/' && route.path === '/') return true
  return link.names.includes(route.name) || route.path.startsWith(link.path)
}

const announcementVisible = ref(false)
const noShowToday = ref(false)
const announcements = [
  { date: '2026-09-22', title: '学习记录上线', content: '听力标记完成与每日短文已读会记入学习记录；个人中心展示真实进度。' },
  { date: '2026-05-13', title: '平台上线', content: 'ListenEase 正式上线！提供四六级真题听力、BBC 外刊、单词复习与每日一句。' },
  { date: '2026-05-13', title: '更新计划', content: '更多学习功能开发中，欢迎通过邮箱(contact@your-domain.com)反馈建议。' },
]

const isLoggedIn = ref(false)
const username = ref('')

const checkLogin = () => {
  const token = localStorage.getItem('token')
  const savedUser = localStorage.getItem('username')
  isLoggedIn.value = !!(token && savedUser)
  username.value = savedUser || ''
}

onMounted(checkLogin)
watch(() => route.path, checkLogin)

const handleCloseAnnouncement = () => {
  if (noShowToday.value) {
    localStorage.setItem('announcement_hide_until', String(Date.now() + 86400000))
  }
}

const handleCommand = (cmd) => {
  if (cmd === 'logout') {
    localStorage.removeItem('token')
    localStorage.removeItem('username')
    localStorage.removeItem('userId')
    isLoggedIn.value = false
    ElMessage.success('已退出登录')
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
  background: rgba(12, 18, 34, 0.92);
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
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
}

.icon-btn {
  border: none !important;
  background: transparent !important;
  color: var(--le-text-secondary) !important;
}

.icon-btn:hover {
  background: var(--le-gradient-soft) !important;
  color: var(--le-primary) !important;
}

.header-actions :deep(.el-button:not(.le-btn-gradient):not(.el-button--primary)) {
  background: transparent;
  border: none;
  color: var(--le-text-secondary);
}

.header-actions :deep(.le-btn-gradient) {
  background: var(--le-gradient) !important;
  color: #fff !important;
  border: none !important;
  box-shadow: none !important;
}

.user-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  border: 1px solid var(--le-border);
  background: var(--le-bg-elevated);
  border-radius: 99px;
  padding: 4px 12px 4px 4px;
  cursor: pointer;
  transition: border-color 0.2s;
}

.user-btn:hover {
  border-color: var(--le-border-strong);
}

.username {
  font-size: 13px;
  color: var(--le-text-secondary);
  max-width: 100px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.admin-info-card {
  background: var(--le-gradient);
  border-radius: var(--le-radius);
  padding: 20px;
  color: #fff;
  margin-bottom: 20px;
}

.admin-info-title {
  font-weight: 700;
  margin-bottom: 12px;
}

.admin-info-row {
  display: flex;
  gap: 8px;
  font-size: 14px;
  margin-bottom: 8px;
}

.admin-info-row .label {
  opacity: 0.75;
  min-width: 72px;
}

.admin-info-row a {
  color: #bae6fd;
}

.status-badge {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  background: rgba(255,255,255,0.2);
  padding: 2px 10px;
  border-radius: 99px;
  font-size: 13px;
}

.status-badge::before {
  content: '';
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: #4ade80;
}

.ann-section-title {
  font-size: 16px;
  margin: 0 0 12px;
}

.announcement-item {
  padding: 14px 16px;
  background: var(--le-bg-muted);
  border-radius: var(--le-radius-sm);
  margin-bottom: 10px;
  border-left: 3px solid var(--le-primary);
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
}

.announcement-footer {
  margin-top: 16px;
  text-align: center;
}

@media (max-width: 768px) {
  .header-inner {
    padding: 0 12px;
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
}
</style>
