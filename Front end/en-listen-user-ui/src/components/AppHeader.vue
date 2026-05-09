<template>
  <header class="app-header">
    <div class="header-container">
      <div class="logo" @click="router.push('/')">
        <el-icon :size="28" color="#409eff"><Headset /></el-icon>
        <span class="logo-text">英语听力</span>
      </div>

      <!-- 未登录 -->
      <div v-if="!isLoggedIn" class="header-right">
        <el-button size="small" @click="announcementVisible = true">
          <el-icon style="margin-right:4px"><BellFilled /></el-icon>公告
        </el-button>
        <el-button type="primary" plain size="small" @click="router.push('/login')">登录</el-button>
        <el-button type="primary" size="small" @click="router.push('/login?mode=register')">注册</el-button>
      </div>

      <!-- 已登录 -->
      <div v-else class="header-right">
        <el-button size="small" @click="announcementVisible = true">
          <el-icon style="margin-right:4px"><BellFilled /></el-icon>公告
        </el-button>
        <el-dropdown @command="handleCommand">
          <span class="user-info">
            <el-avatar :size="28" :icon="UserFilled" />
            <span class="username">{{ username }}</span>
            <el-icon><ArrowDown /></el-icon>
          </span>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item command="profile">个人中心</el-dropdown-item>
              <el-dropdown-item command="history">学习记录</el-dropdown-item>
              <el-dropdown-item divided command="logout">退出登录</el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </div>
    </div>

    <!-- 公告对话框 -->
    <el-dialog v-model="announcementVisible" title="平台公告" width="560px" :close-on-click-modal="true" append-to-body @close="handleCloseAnnouncement" class="announcement-dialog">
      <!-- 常驻：管理员信息 -->
      <div class="admin-info-card">
        <div class="admin-info-title">&#x2699;&#xFE0F; 管理员信息</div>
        <div class="admin-info-row">
          <span class="admin-info-label">管理员：</span>
          <span>Kilo</span>
        </div>
        <div class="admin-info-row">
          <span class="admin-info-label">联系邮箱：</span>
          <span>Kilokiyiu@outlook.com</span>
        </div>
        <div class="admin-info-row">
          <span class="admin-info-label">我的github</span>
          <span><a href="https://github.com/Kilokiyiu">github</a></span>
        </div>
          <div class="admin-info-row">
          <span class="admin-info-label">关于我:</span>
          <span><a href="https://mywebpage-f2u.pages.dev/">我的主页</a></span>
        </div>
        <div class="admin-info-row">
          <span class="admin-info-label">平台状态：</span>
          <span class="status-badge">正常运行</span>
        </div>
        <div class="admin-info-tip">如有问题或建议，请通过邮箱联系管理员</div>
      </div>

      <!-- 动态公告 -->
      <div class="announcement-section-title">最新公告</div>
      <div class="announcement-list">
        <div class="announcement-item" v-for="(item, index) in announcements" :key="index">
          <div class="announcement-date">{{ item.date }}</div>
          <div class="announcement-title">{{ item.title }}</div>
          <div class="announcement-desc">{{ item.content }}</div>
        </div>
      </div>

      <!-- 底部：一天内不再弹出 -->
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
import { BellFilled } from '@element-plus/icons-vue'

const router = useRouter()
const route = useRoute()

// 公告
const announcementVisible = ref(false)
const noShowToday = ref(false)
const announcements = [
  { date: '2026-05-05', title: '平台上线公告', content: '欢迎访问英语听力练习平台！本站正在持续更新中，如有问题请联系管理员。' },
  { date: '2026-05-01', title: '功能更新', content: '新增音频上传功能，管理员可通过后台上传CET听力真题音频。' },
]

// 检查是否应该自动弹出公告
const checkAutoShowAnnouncement = () => {
  const hideUntil = localStorage.getItem('announcement_hide_until')
  if (hideUntil && Date.now() < Number(hideUntil)) {
    return // 还在免打扰期内
  }
  announcementVisible.value = true
}

// 监听弹窗关闭，处理“今日不再显示”
const handleCloseAnnouncement = () => {
  if (noShowToday.value) {
    const tomorrow = Date.now() + 24 * 60 * 60 * 1000
    localStorage.setItem('announcement_hide_until', tomorrow.toString())
  }
}

// 登录状态
const isLoggedIn = ref(false)
const username = ref('')

// 检查登录状态
const checkLogin = () => {
  const token = localStorage.getItem('token')
  const savedUser = localStorage.getItem('username')
  if (token && savedUser) {
    isLoggedIn.value = true
    username.value = savedUser
  } else {
    isLoggedIn.value = false
    username.value = ''
  }
}

onMounted(() => {
  checkLogin()
  checkAutoShowAnnouncement()
})

// 每次路由变化都重新检查
watch(() => route.path, checkLogin)

// 下拉菜单操作
const handleCommand = (command) => {
  if (command === 'logout') {
    localStorage.removeItem('token')
    localStorage.removeItem('username')
    isLoggedIn.value = false
    username.value = ''
    ElMessage.success('已退出登录')
    router.push('/')
  } else if (command === 'profile') {
    router.push('/profile')
  } else if (command === 'history') {
    router.push('/history')
  }
}
</script>

<style scoped>
.app-header {
  background: rgba(255, 255, 255, 0.85);
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  border-bottom: 1px solid var(--border-glass);
  position: sticky;
  top: 0;
  z-index: 100;
  border-radius: 0 0 16px 16px;
  box-shadow: 0 2px 12px rgba(0,0,0,0.06);
}

.header-container {
  max-width: 1400px;
  margin: 0 auto;
  padding: 0 24px;
  height: 60px;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.logo {
  display: flex;
  align-items: center;
  gap: 10px;
  cursor: pointer;
}

.logo :deep(.el-icon) {
  filter: drop-shadow(0 0 6px rgba(64, 158, 255, 0.4));
}

.logo-text {
  font-size: 20px;
  font-weight: 600;
  color: var(--text-primary);
}

.header-right {
  display: flex;
  gap: 12px;
  align-items: center;
}

.header-right :deep(.el-button--primary) {
  background: linear-gradient(135deg, var(--accent-blue) 0%, var(--accent-cyan) 100%);
  border: none;
  box-shadow: 0 4px 12px rgba(64, 158, 255, 0.3);
  transition: all 0.3s;
}

.header-right :deep(.el-button--primary:hover) {
  box-shadow: 0 6px 20px rgba(64, 158, 255, 0.4);
  transform: translateY(-1px);
}

.header-right :deep(.el-button--primary.is-plain) {
  background: transparent;
  border: 1px solid var(--accent-blue);
  color: var(--accent-blue);
  box-shadow: none;
}

.header-right :deep(.el-button--primary.is-plain:hover) {
  background: rgba(64, 158, 255, 0.08);
  border-color: var(--accent-blue);
  color: var(--accent-blue);
}

.user-info {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  padding: 4px 12px;
  border-radius: 20px;
  transition: all 0.3s;
  border: 1px solid transparent;
}

.user-info:hover {
  background: rgba(0, 0, 0, 0.04);
  border-color: var(--border-glass);
}

.username {
  font-size: 14px;
  color: var(--text-secondary);
}

/* 下拉菜单 */
:deep(.el-dropdown__popper) {
  background: rgba(255, 255, 255, 0.95) !important;
  backdrop-filter: blur(12px);
  border: 1px solid var(--border-glass) !important;
  box-shadow: 0 8px 32px rgba(0,0,0,0.1);
}

:deep(.el-dropdown-menu) {
  background: transparent !important;
}

:deep(.el-dropdown-menu__item) {
  color: var(--text-secondary);
}

:deep(.el-dropdown-menu__item:hover) {
  background: rgba(64, 158, 255, 0.08) !important;
  color: var(--accent-blue);
}

/* 管理员信息卡 */
.admin-info-card {
  background: linear-gradient(135deg, #1a2a4a 0%, #2a5298 50%, #1e3a6f 100%);
  border-radius: 14px;
  padding: 22px 24px;
  margin-bottom: 22px;
  color: #ffffff;
  box-shadow: 0 4px 16px rgba(26, 42, 74, 0.3);
  position: relative;
  overflow: hidden;
}

.admin-info-card::before {
  content: '';
  position: absolute;
  top: -30px;
  right: -30px;
  width: 120px;
  height: 120px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.05);
}

.admin-info-card::after {
  content: '';
  position: absolute;
  bottom: -20px;
  left: -20px;
  width: 80px;
  height: 80px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.03);
}

.admin-info-title {
  font-size: 17px;
  font-weight: 700;
  margin-bottom: 16px;
  letter-spacing: 0.5px;
}

.admin-info-row {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 10px;
  font-size: 14px;
  position: relative;
  z-index: 1;
}

.admin-info-label {
  color: rgba(255, 255, 255, 0.6);
  min-width: 90px;
}

.admin-info-row a {
  color: #7dd3fc;
  text-decoration: none;
  transition: all 0.2s;
  border-bottom: 1px solid transparent;
}

.admin-info-row a:hover {
  color: #bae6fd;
  border-bottom-color: #bae6fd;
}

.admin-info-tip {
  margin-top: 14px;
  padding-top: 12px;
  border-top: 1px solid rgba(255, 255, 255, 0.15);
  font-size: 12px;
  color: rgba(255, 255, 255, 0.5);
  position: relative;
  z-index: 1;
}

.status-badge {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  background: rgba(74, 222, 128, 0.15);
  padding: 2px 10px;
  border-radius: 20px;
  font-size: 13px;
}

.status-badge::before {
  content: '';
  width: 7px;
  height: 7px;
  border-radius: 50%;
  background: #4ade80;
  box-shadow: 0 0 6px rgba(74, 222, 128, 0.5);
}

/* 公告分区标题 */
.announcement-section-title {
  font-size: 17px;
  font-weight: 700;
  color: var(--text-primary);
  margin-bottom: 16px;
  padding-left: 4px;
  display: flex;
  align-items: center;
  gap: 8px;
}

.announcement-section-title::before {
  content: '';
  width: 4px;
  height: 18px;
  border-radius: 2px;
  background: linear-gradient(180deg, var(--accent-blue), var(--accent-cyan));
}

/* 公告列表 */
.announcement-list {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.announcement-item {
  padding: 18px 20px;
  background: #f6f8fc;
  border-radius: 12px;
  border-left: 4px solid transparent;
  border-image: linear-gradient(180deg, #409eff, #00d4ff) 1;
  transition: all 0.2s;
}

.announcement-item:hover {
  background: #eef2fa;
  transform: translateX(3px);
}

.announcement-date {
  font-size: 13px;
  color: var(--text-muted);
  margin-bottom: 8px;
}

.announcement-title {
  font-size: 17px;
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 8px;
}

.announcement-desc {
  font-size: 15px;
  color: var(--text-secondary);
  line-height: 1.7;
}

/* 底部不再显示 */
.announcement-footer {
  margin-top: 20px;
  padding-top: 16px;
  border-top: 1px dashed #dde3ee;
  display: flex;
  justify-content: center;
}

.announcement-footer :deep(.el-checkbox__label) {
  font-size: 13px;
  color: var(--text-muted);
}

.announcement-footer :deep(.el-checkbox__input.is-checked .el-checkbox__inner) {
  background-color: var(--accent-blue);
  border-color: var(--accent-blue);
}

@media (max-width: 768px) {
  .app-header {
    border-radius: 0;
  }
}
</style>

<style>
/* 公告弹窗手机端适配 - 需要 unscoped 因为 append-to-body */
@media (max-width: 768px) {
  .announcement-dialog {
    --el-dialog-width: 92% !important;
    width: 92% !important;
    margin: 0 auto !important;
  }
  .announcement-dialog .el-dialog {
    width: 92% !important;
    margin: 0 auto !important;
  }
  .announcement-dialog .el-dialog__body {
    padding: 16px !important;
    }
}
</style>
