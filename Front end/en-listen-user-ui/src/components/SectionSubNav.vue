<template>
  <nav class="section-sub-nav" :aria-label="ariaLabel">
    <button
      v-for="tab in tabs"
      :key="tab.path"
      type="button"
      class="sub-nav-item"
      :class="{ active: isActive(tab), secondary: tab.secondary }"
      @click="go(tab)"
    >
      {{ tab.label }}
    </button>
  </nav>
</template>

<script setup>
import { useRoute, useRouter } from 'vue-router'

const props = defineProps({
  tabs: { type: Array, required: true },
  ariaLabel: { type: String, default: '栏目切换' },
})

const route = useRoute()
const router = useRouter()

const isActive = (tab) => {
  if (tab.names?.includes(route.name)) return true
  if (tab.path && route.path === tab.path) return true
  if (tab.matchPrefix && route.path.startsWith(tab.matchPrefix)) return true
  return false
}

const go = (tab) => {
  if (isActive(tab)) return
  router.push(tab.to || tab.path)
}
</script>

<style scoped>
.section-sub-nav {
  display: none;
  gap: 6px;
  padding: 4px;
  margin: 0 0 14px;
  background: var(--le-bg-muted);
  border: 1px solid var(--le-border);
  border-radius: 10px;
  overflow-x: auto;
  -webkit-overflow-scrolling: touch;
  scrollbar-width: none;
}

.section-sub-nav::-webkit-scrollbar {
  display: none;
}

@media (max-width: 768px) {
  .section-sub-nav {
    display: flex;
  }
}

.sub-nav-item {
  flex: 1 0 auto;
  min-width: 0;
  border: none;
  background: transparent;
  color: var(--le-text-secondary);
  font-size: 13px;
  font-weight: 500;
  padding: 8px 12px;
  border-radius: 8px;
  cursor: pointer;
  white-space: nowrap;
  transition: background 0.15s, color 0.15s;
  -webkit-tap-highlight-color: transparent;
}

.sub-nav-item.secondary {
  color: var(--le-text-muted);
  font-weight: 400;
}

.sub-nav-item.active {
  background: var(--le-bg-elevated);
  color: var(--le-primary);
  box-shadow: var(--le-shadow-sm);
  font-weight: 600;
}

.sub-nav-item.secondary.active {
  color: var(--le-primary);
}
</style>
