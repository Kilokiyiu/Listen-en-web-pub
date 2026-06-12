<template>
  <div class="admin-layout">
    <aside class="admin-sidebar">
      <div class="sidebar-logo">
        <div class="logo-mark">
          <el-icon :size="20"><Headset /></el-icon>
        </div>
        <span class="logo-text">Listen Admin</span>
      </div>

      <nav class="sidebar-menu">
        <router-link
          v-for="item in menuItems"
          :key="item.path"
          :to="item.path"
          class="menu-item"
          :class="{ active: isActive(item.path) }"
        >
          <el-icon class="menu-icon"><component :is="item.icon" /></el-icon>
          <span class="menu-label">{{ item.title }}</span>
        </router-link>
      </nav>
    </aside>

    <div class="admin-main">
      <header class="admin-topbar">
        <div class="topbar-left">
          <el-breadcrumb separator="/">
            <el-breadcrumb-item>工作台</el-breadcrumb-item>
            <el-breadcrumb-item>{{ currentTitle }}</el-breadcrumb-item>
          </el-breadcrumb>
        </div>

        <div class="topbar-right">
          <el-dropdown trigger="click" @command="handleCommand">
            <div class="user-trigger">
              <el-avatar :size="28" class="user-avatar">
                {{ avatarLetter }}
              </el-avatar>
              <span class="user-name">{{ userName }}</span>
              <el-icon><ArrowDown /></el-icon>
            </div>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item command="logout">
                  <el-icon><SwitchButton /></el-icon>
                  退出登录
                </el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
        </div>
      </header>

      <main class="admin-content">
        <router-view />
      </main>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import {
  Headset,
  DataAnalysis,
  Upload,
  FolderOpened,
  Document,
  ArrowDown,
  SwitchButton,
} from '@element-plus/icons-vue'

const route = useRoute()
const router = useRouter()

const menuItems = [
  { path: '/', title: '数据概览', icon: DataAnalysis },
  { path: '/upload', title: '音频上传', icon: Upload },
  { path: '/manage', title: '音频管理', icon: FolderOpened },
  { path: '/article', title: '每日一篇', icon: Document },
]

const userName = computed(() => localStorage.getItem('admin_userName') || '管理员')
const avatarLetter = computed(() => (userName.value[0] || 'A').toUpperCase())
const currentTitle = computed(() => route.meta.title || '工作台')

const isActive = (path) => {
  if (path === '/') return route.path === '/'
  return route.path === path || route.path.startsWith(`${path}/`)
}

const handleCommand = (command) => {
  if (command === 'logout') {
    localStorage.removeItem('admin_token')
    localStorage.removeItem('admin_userName')
    ElMessage.success('已退出登录')
    router.replace('/login')
  }
}
</script>

<style scoped>
.admin-layout {
  display: flex;
  min-height: 100vh;
  background: var(--admin-content-bg);
}

.admin-sidebar {
  width: var(--admin-sidebar-width);
  flex-shrink: 0;
  background: var(--admin-sidebar-bg);
  display: flex;
  flex-direction: column;
  position: fixed;
  top: 0;
  left: 0;
  bottom: 0;
  z-index: 100;
}

.sidebar-logo {
  height: 48px;
  display: flex;
  align-items: center;
  padding: 0 16px;
  gap: 10px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.08);
}

.logo-mark {
  width: 32px;
  height: 32px;
  border-radius: 6px;
  background: var(--admin-primary);
  display: flex;
  align-items: center;
  justify-content: center;
  color: #fff;
}

.logo-text {
  color: #fff;
  font-size: 16px;
  font-weight: 600;
  white-space: nowrap;
}

.sidebar-menu {
  flex: 1;
  padding: 8px;
  overflow-y: auto;
}

.menu-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 0 16px;
  height: 40px;
  margin-bottom: 4px;
  border-radius: 6px;
  color: rgba(255, 255, 255, 0.65);
  text-decoration: none;
  font-size: 14px;
  transition: all 0.2s;
}

.menu-item:hover {
  color: #fff;
  background: rgba(255, 255, 255, 0.08);
}

.menu-item.active {
  color: #fff;
  background: var(--admin-primary);
}

.menu-icon {
  font-size: 16px;
}

.admin-main {
  flex: 1;
  margin-left: var(--admin-sidebar-width);
  display: flex;
  flex-direction: column;
  min-width: 0;
}

.admin-topbar {
  height: var(--admin-header-height);
  background: #fff;
  border-bottom: 1px solid var(--admin-border);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 20px;
  position: sticky;
  top: 0;
  z-index: 99;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.04);
}

.topbar-left :deep(.el-breadcrumb__inner) {
  color: var(--admin-text-secondary);
  font-weight: 400;
}

.topbar-left :deep(.el-breadcrumb__item:last-child .el-breadcrumb__inner) {
  color: var(--admin-text);
  font-weight: 500;
}

.user-trigger {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  padding: 4px 8px;
  border-radius: 6px;
  transition: background 0.2s;
}

.user-trigger:hover {
  background: #f5f5f5;
}

.user-avatar {
  background: var(--admin-primary);
  color: #fff;
  font-size: 13px;
}

.user-name {
  font-size: 14px;
  color: var(--admin-text);
}

.admin-content {
  flex: 1;
  padding: 16px;
  overflow: auto;
}
</style>
