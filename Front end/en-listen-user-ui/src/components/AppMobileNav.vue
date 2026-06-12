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
      <el-icon :size="22"><component :is="item.icon" /></el-icon>
      <span>{{ item.label }}</span>
    </button>
  </nav>
</template>

<script setup>
import { useRoute, useRouter } from 'vue-router'
import { HomeFilled, Headset, Reading, Collection, User } from '@element-plus/icons-vue'

const router = useRouter()
const route = useRoute()

const navItems = [
  { path: '/', label: '首页', icon: HomeFilled, match: ['home'] },
  { path: '/exams', label: '听力', icon: Headset, match: ['exams', 'examDetail'] },
  { path: '/daily', label: '阅读', icon: Reading, match: ['dailyArticle', 'bbcNews'] },
  { path: '/word-roots', label: '单词', icon: Collection, match: ['wordRoots', 'wordRootDetail', 'myWords', 'wordReview'] },
  { path: '/profile', label: '我的', icon: User, match: ['profile', 'history', 'login'] },
]

const isActive = (item) => {
  if (item.path === '/' && route.path === '/') return true
  if (item.path !== '/' && route.path.startsWith(item.path.split('?')[0])) return true
  return item.match.includes(route.name)
}

const go = (path) => {
  if (route.path !== path) router.push(path)
}
</script>

<style scoped>
.mobile-nav {
  display: none;
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  height: var(--le-mobile-nav-h);
  background: rgba(255, 255, 255, 0.92);
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  border-top: 1px solid var(--le-border);
  z-index: 200;
  padding: 0 4px;
  padding-bottom: env(safe-area-inset-bottom, 0);
}

@media (max-width: 768px) {
  .mobile-nav {
    display: flex;
    align-items: stretch;
    justify-content: space-around;
  }
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
  color: var(--le-text-muted);
  font-size: 10px;
  cursor: pointer;
  padding: 6px 0;
  transition: color 0.2s;
  -webkit-tap-highlight-color: transparent;
}

.mobile-nav-item.active {
  color: var(--le-primary);
}

.mobile-nav-item.active .el-icon {
  transform: scale(1.05);
}

.mobile-nav-item span {
  line-height: 1.2;
}
</style>
