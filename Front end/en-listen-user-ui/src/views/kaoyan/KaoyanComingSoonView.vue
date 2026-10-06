<template>
  <div class="kaoyan-page le-page">
    <div class="page-header">
      <el-button text class="back-btn" @click="router.back()">
        <el-icon><ArrowLeft /></el-icon> 返回
      </el-button>
      <div class="page-title">考研英语</div>
      <div class="placeholder"></div>
    </div>

    <section class="soon-panel le-paper">
      <span class="soon-badge">开发中</span>
      <h1 class="soon-title">{{ status?.name || '考研英语模块' }}</h1>
      <p class="soon-desc">
        {{ status?.description || fallbackDesc }}
      </p>
      <p class="soon-note">独立微服务</p>
      <p v-if="loadError" class="soon-error">模块状态暂不可用（服务未启动或未部署）</p>
    </section>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getKaoyanModuleStatus } from '../../api/Kaoyan.js'

const router = useRouter()
const status = ref(null)
const loadError = ref(false)
const fallbackDesc =
  '考研英语不含听力，将提供历年考研原题练习，双语精读等内容。当前功能开发中。'

onMounted(async () => {
  try {
    status.value = await getKaoyanModuleStatus()
    loadError.value = false
  } catch (e) {
    loadError.value = true
    console.error('获取考研模块状态失败', e)
  }
})
</script>

<style scoped>
.kaoyan-page {
  padding: 0 24px 32px;
  max-width: 720px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 0;
  border-bottom: 1px solid var(--le-border);
  margin-bottom: 24px;
}

.page-title {
  font-size: 18px;
  font-weight: 600;
  color: var(--le-text);
}

.placeholder {
  width: 60px;
}

.back-btn {
  color: var(--le-text-muted) !important;
}

.soon-panel {
  padding: 40px 28px;
  text-align: center;
}

.soon-badge {
  display: inline-block;
  font-size: 11px;
  font-weight: 600;
  padding: 4px 10px;
  border-radius: 4px;
  color: var(--le-warning);
  background: rgba(217, 119, 6, 0.1);
  border: 1px solid rgba(217, 119, 6, 0.22);
  margin-bottom: 16px;
}

.soon-title {
  margin: 0 0 12px;
  font-size: 22px;
  font-weight: 700;
  color: var(--le-text);
}

.soon-desc {
  margin: 0 auto 16px;
  max-width: 420px;
  font-size: 14px;
  line-height: 1.7;
  color: var(--le-text-muted);
}

.soon-note {
  margin: 0;
  font-size: 13px;
  color: var(--le-text-muted);
  opacity: 0.85;
}

.soon-error {
  margin: 12px 0 0;
  font-size: 12px;
  color: var(--le-warning);
}

@media (max-width: 768px) {
  .kaoyan-page {
    padding: 0 14px 24px;
  }
}
</style>
