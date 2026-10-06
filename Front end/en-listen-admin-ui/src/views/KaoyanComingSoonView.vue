<template>
  <div class="admin-page">
    <PageHeader
      title="考研英语"
      description="独立微服务 KaoyanService · 当前开发中"
    />

    <div class="admin-card">
      <div class="admin-card__body soon-body">
        <el-tag type="warning" effect="light">开发中</el-tag>
        <h2 class="soon-title">{{ status?.name || '考研英语模块' }}</h2>
        <p class="soon-desc">
          {{ status?.description || '考研英语将作为独立微服务建设，不与每日短文共用文章服务。' }}
        </p>
        <p class="soon-meta">服务：KaoyanService · 路径：/api/kaoyan</p>
        <p v-if="loadError" class="soon-error">无法连接 KaoyanService，请确认本地 5301 端口或生产容器已启动。</p>
        <p v-else-if="status" class="soon-ok">模块状态接口正常</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { kaoyanRequest } from '../api/Request'
import PageHeader from '../components/PageHeader.vue'

const status = ref(null)
const loadError = ref(false)

onMounted(async () => {
  try {
    status.value = await kaoyanRequest.get('/Kaoyan/GetModuleStatus')
    loadError.value = false
  } catch (e) {
    loadError.value = true
  }
})
</script>

<style scoped>
.soon-body {
  text-align: center;
  padding: 48px 24px;
}

.soon-title {
  margin: 16px 0 10px;
  font-size: 20px;
  color: var(--admin-text);
}

.soon-desc {
  margin: 0 auto 12px;
  max-width: 480px;
  font-size: 14px;
  line-height: 1.7;
  color: var(--admin-text-muted, #64748b);
}

.soon-meta {
  margin: 0;
  font-size: 12px;
  color: var(--admin-text-muted, #64748b);
}

.soon-error {
  margin: 14px 0 0;
  font-size: 13px;
  color: #d97706;
}

.soon-ok {
  margin: 14px 0 0;
  font-size: 13px;
  color: #059669;
}
</style>
