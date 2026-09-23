<template>
  <Teleport to="body">
    <div
      v-if="visible"
      class="update-mask"
      @click.self="!remote?.forceUpdate && !downloading && $emit('later')"
    >
      <div class="update-dialog" role="dialog" aria-modal="true">
        <h3 class="title">发现新版本</h3>
        <p class="version">
          {{ localVersion }} → <strong>{{ remote?.versionName }}</strong>
        </p>
        <p v-if="remote?.changelog" class="changelog">{{ remote.changelog }}</p>
        <p v-else class="changelog muted">建议更新到最新版本以获得更好体验。</p>
        <p v-if="downloading" class="progress">下载中 {{ progress }}%</p>
        <div class="actions">
          <button
            v-if="!remote?.forceUpdate"
            type="button"
            class="btn-later"
            :disabled="downloading"
            @click="$emit('later')"
          >
            稍后
          </button>
          <button
            type="button"
            class="btn-update"
            :disabled="downloading"
            @click="$emit('update')"
          >
            {{ downloading ? '下载中...' : '立即更新' }}
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup>
defineProps({
  visible: { type: Boolean, default: false },
  remote: { type: Object, default: null },
  localVersion: { type: String, default: '' },
  downloading: { type: Boolean, default: false },
  progress: { type: Number, default: 0 },
})

defineEmits(['later', 'update'])
</script>

<style scoped>
.update-mask {
  position: fixed;
  inset: 0;
  z-index: 10000;
  background: rgba(0, 0, 0, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 24px;
}

.update-dialog {
  width: min(100%, 340px);
  background: var(--bg-card);
  border-radius: 16px;
  padding: 22px 20px 16px;
  box-shadow: 0 12px 40px rgba(0, 0, 0, 0.18);
}

.title {
  margin: 0 0 8px;
  font-size: 18px;
  font-weight: 600;
  text-align: center;
}

.version {
  margin: 0 0 12px;
  text-align: center;
  font-size: 14px;
  color: var(--text-secondary);
}

.version strong {
  color: var(--primary);
}

.changelog {
  margin: 0 0 16px;
  padding: 10px 12px;
  font-size: 13px;
  line-height: 1.55;
  color: var(--text-secondary);
  background: var(--bg);
  border-radius: 10px;
  white-space: pre-line;
  max-height: 160px;
  overflow-y: auto;
}

.changelog.muted {
  color: var(--text-muted);
}

.progress {
  margin: -8px 0 12px;
  text-align: center;
  font-size: 13px;
  color: var(--primary);
}

.actions {
  display: flex;
  gap: 10px;
}

.btn-later,
.btn-update {
  flex: 1;
  padding: 12px 0;
  border-radius: 10px;
  font-size: 15px;
  font-weight: 500;
  border: none;
}

.btn-later {
  background: var(--bg);
  color: var(--text-secondary);
  border: 1px solid var(--border);
}

.btn-update {
  background: var(--primary);
  color: #fff;
}

.btn-later:disabled,
.btn-update:disabled {
  opacity: 0.6;
}
</style>
