<template>
  <div class="le-page">
    <header v-if="showHeader" class="le-page-header">
      <button v-if="back" type="button" class="le-back-btn" @click="onBack">
        <el-icon><ArrowLeft /></el-icon>
        {{ backLabel }}
      </button>
      <div v-if="title" class="le-page-title-wrap">
        <div v-if="showBar" class="le-title-bar" />
        <h1 class="le-page-title">{{ title }}</h1>
        <p v-if="subtitle" class="le-page-subtitle">{{ subtitle }}</p>
      </div>
      <span v-if="badge" class="le-badge">{{ badge }}</span>
      <slot name="header-extra" />
    </header>
    <slot />
  </div>
</template>

<script setup>
import { useRouter } from 'vue-router'
import { ArrowLeft } from '@element-plus/icons-vue'

const props = defineProps({
  title: { type: String, default: '' },
  subtitle: { type: String, default: '' },
  badge: { type: String, default: '' },
  back: { type: Boolean, default: true },
  backLabel: { type: String, default: '返回' },
  showHeader: { type: Boolean, default: true },
  showBar: { type: Boolean, default: true },
})

const router = useRouter()

const onBack = () => {
  if (window.history.length > 1) router.back()
  else router.push('/')
}
</script>
