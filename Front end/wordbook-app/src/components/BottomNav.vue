<template>
  <nav class="mobile-nav" aria-label="主导航">
    <button
      v-for="item in navItems"
      :key="item.path"
      type="button"
      class="mobile-nav-item"
      :class="{ active: isActive(item) }"
      @click="go(item.path)"
    >
      <span class="nav-icon" v-html="item.icon" />
      <span class="nav-label">{{ item.label }}</span>
    </button>
  </nav>
</template>

<script setup>
import { useRoute, useRouter } from 'vue-router'

const router = useRouter()
const route = useRoute()

const iconBook =
  '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20"/><path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z"/><path d="M8 7h8M8 11h6"/></svg>'

const iconReview =
  '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="9"/><path d="M12 7v5l3 2"/></svg>'

const iconUser =
  '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="8" r="4"/><path d="M5 20c0-3.3 3.1-6 7-6s7 2.7 7 6"/></svg>'

const navItems = [
  { path: '/', label: '单词本', icon: iconBook, match: ['words', 'add'] },
  { path: '/review', label: '复习', icon: iconReview, match: ['review'] },
  { path: '/settings', label: '我的', icon: iconUser, match: ['settings'] },
]

const isActive = (item) => {
  if (item.match.includes(route.name)) return true
  if (item.path === '/' && route.path === '/') return true
  if (item.path !== '/' && route.path.startsWith(item.path)) return true
  return false
}

const go = (path) => {
  if (route.path !== path) router.push(path)
}
</script>

<style scoped>
.mobile-nav {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  height: var(--mobile-nav-h, 56px);
  display: flex;
  align-items: stretch;
  justify-content: space-around;
  background: rgba(255, 255, 255, 0.92);
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  border-top: 1px solid var(--border);
  z-index: 200;
  padding: 0 4px;
  padding-bottom: env(safe-area-inset-bottom, 0);
}

.mobile-nav-item {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 2px;
  border: none;
  background: transparent;
  color: var(--text-muted);
  font-size: 10px;
  cursor: pointer;
  padding: 6px 0;
  transition: color 0.2s;
  -webkit-tap-highlight-color: transparent;
}

.mobile-nav-item.active {
  color: var(--primary);
}

.mobile-nav-item.active :deep(svg) {
  transform: scale(1.05);
}

.nav-icon {
  display: flex;
  width: 22px;
  height: 22px;
}

.nav-icon :deep(svg) {
  width: 100%;
  height: 100%;
  transition: transform 0.2s;
}

.nav-label {
  line-height: 1.2;
}
</style>
