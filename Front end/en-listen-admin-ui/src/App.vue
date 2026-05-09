<template>
  <div id="admin-app">
    <div class="admin-header" v-if="isLoggedIn">
      <div class="header-left">
        <span class="logo">听力管理后台</span>
        <nav class="nav-menu">
          <router-link to="/" :class="['nav-link', { active: $route.path === '/' }]">音频上传</router-link>
          <router-link to="/manage" :class="['nav-link', { active: $route.path === '/manage' }]">音频管理</router-link>
          <router-link to="/article" :class="['nav-link', { active: $route.path === '/article' }]">每日一篇</router-link>
        </nav>
      </div>
      <div class="header-right">
        <span class="admin-name">{{ userName }}</span>
        <el-button type="danger" size="small" @click="handleLogout">退出登录</el-button>
      </div>
    </div>
    <router-view />
  </div>
</template>

<script setup>
import { computed, ref, onMounted, onUnmounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessage } from 'element-plus'

const router = useRouter()
const route = useRoute()

// 使用响应式变量跟踪登录状态（localStorage 本身不是响应式的）
const isAuthenticated = ref(!!localStorage.getItem('admin_token'))

// 只有登录后才显示导航栏（且不在登录页）
const isLoggedIn = computed(() => {
  return isAuthenticated.value && route.path !== '/login'
})

const userName = computed(() => localStorage.getItem('admin_userName') || '')

// 监听 auth-login 事件（登录成功后触发）
const handleAuthLogin = () => {
  isAuthenticated.value = true
}

onMounted(() => {
  window.addEventListener('auth-login', handleAuthLogin)
})

onUnmounted(() => {
  window.removeEventListener('auth-login', handleAuthLogin)
})

const handleLogout = () => {
  localStorage.removeItem('admin_token')
  localStorage.removeItem('admin_userName')
  isAuthenticated.value = false
  ElMessage.success('已退出登录')
  router.replace('/login')
}
</script>

<style>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

body {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
  background: #f0f2f5;
  min-height: 100vh;
  color: #1a1a2e;
}

.admin-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0 24px;
  height: 56px;
  background: linear-gradient(135deg, #1a1a2e 0%, #302b63 100%);
  color: #fff;
}

.logo {
  font-size: 18px;
  font-weight: 600;
  letter-spacing: 1px;
}

.header-right {
  display: flex;
  align-items: center;
  gap: 16px;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 32px;
}

.nav-menu {
  display: flex;
  gap: 4px;
}

.nav-link {
  padding: 6px 16px;
  border-radius: 6px;
  color: rgba(255, 255, 255, 0.7);
  font-size: 14px;
  text-decoration: none;
  transition: all 0.2s;
}

.nav-link:hover {
  color: #fff;
  background: rgba(255, 255, 255, 0.1);
}

.nav-link.active {
  color: #fff;
  background: rgba(255, 255, 255, 0.15);
  font-weight: 500;
}

.admin-name {
  font-size: 14px;
  opacity: 0.85;
}
</style>
