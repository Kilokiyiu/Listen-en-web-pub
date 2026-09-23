<template>
  <header class="page-top-bar">
    <div class="bar-left">
      <button
        v-if="showBack"
        type="button"
        class="btn-back"
        aria-label="返回上一页"
        @click="handleBack"
      >
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M15 18l-6-6 6-6" />
        </svg>
        <span>返回</span>
      </button>
    </div>
    <h1 class="bar-title">{{ title }}</h1>
    <div class="bar-right">
      <slot name="right" />
    </div>
  </header>
</template>

<script setup>
import { computed } from 'vue'
import { useRouter } from 'vue-router'

const props = defineProps({
  title: { type: String, default: '' },
  showBack: { type: Boolean, default: false },
  backTo: { type: String, default: '' },
})

const router = useRouter()

const showBack = computed(() => props.showBack)

const handleBack = async () => {
  if (props.backTo) {
    router.replace(props.backTo)
    return
  }

  if (window.history.length > 1) {
    router.back()
    return
  }

  router.replace('/')
}
</script>

<style scoped>
.page-top-bar {
  position: sticky;
  top: 0;
  z-index: 150;
  display: grid;
  grid-template-columns: 72px 1fr 72px;
  align-items: center;
  min-height: 48px;
  padding: 8px 12px;
  padding-top: calc(8px + env(safe-area-inset-top, 0));
  background: rgba(255, 255, 255, 0.92);
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  border-bottom: 1px solid var(--border);
}

.bar-left,
.bar-right {
  display: flex;
  align-items: center;
}

.bar-right {
  justify-content: flex-end;
}

.btn-back {
  display: inline-flex;
  align-items: center;
  gap: 2px;
  padding: 6px 4px;
  background: none;
  border: none;
  color: var(--primary);
  font-size: 15px;
  font-weight: 500;
  -webkit-tap-highlight-color: transparent;
}

.btn-back svg {
  width: 20px;
  height: 20px;
  flex-shrink: 0;
}

.bar-title {
  margin: 0;
  font-size: 17px;
  font-weight: 600;
  text-align: center;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
</style>
