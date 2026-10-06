<template>
  <Teleport to="body">
    <div v-if="dialogState.visible" class="mask" @click.self="onCancel">
      <div class="dialog le-paper" role="dialog" aria-modal="true">
        <h3 v-if="dialogState.title" class="title">{{ dialogState.title }}</h3>
        <p v-if="dialogState.message" class="message">{{ dialogState.message }}</p>
        <input
          v-if="dialogState.input"
          ref="inputRef"
          v-model="dialogState.value"
          class="field"
          type="text"
          @keyup.enter="onConfirm"
        />
        <div class="actions">
          <button v-if="dialogState.showCancel" type="button" class="btn ghost" @click="onCancel">
            {{ dialogState.cancelText }}
          </button>
          <button
            type="button"
            class="btn solid"
            :class="{ danger: dialogState.danger }"
            @click="onConfirm"
          >
            {{ dialogState.confirmText }}
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup>
import { nextTick, ref, watch } from 'vue'
import { closeDialog, dialogState } from '../utils/dialog'

const inputRef = ref(null)

watch(
  () => dialogState.visible,
  async (v) => {
    if (v && dialogState.input) {
      await nextTick()
      inputRef.value?.focus()
    }
  }
)

const onCancel = () => {
  if (!dialogState.showCancel) return
  closeDialog(dialogState.input ? null : false)
}

const onConfirm = () => {
  if (dialogState.input) {
    closeDialog(String(dialogState.value || '').trim() || null)
    return
  }
  closeDialog(true)
}
</script>

<style scoped>
.mask {
  position: fixed;
  inset: 0;
  z-index: 10000;
  background: rgba(30, 41, 59, 0.35);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 24px;
}

.dialog {
  width: min(100%, 340px);
  padding: 20px 18px 16px;
}

.title {
  margin: 0 0 8px;
  font-size: 17px;
  font-weight: 650;
  text-align: center;
}

.message {
  margin: 0 0 14px;
  font-size: 14px;
  line-height: 1.55;
  color: var(--le-text-secondary);
  white-space: pre-line;
}

.field {
  width: 100%;
  margin: 0 0 14px;
  padding: 10px 12px;
  border: 1px solid var(--le-border-strong);
  border-radius: 8px;
  background: var(--le-bg-surface);
  font-size: 15px;
}

.actions {
  display: flex;
  gap: 10px;
}

.btn {
  flex: 1;
  padding: 11px 0;
  font-size: 15px;
  font-weight: 500;
}

.btn.ghost {
  background: var(--le-bg-muted);
  color: var(--le-text-secondary);
  border: 1px solid var(--le-border);
  border-radius: 8px;
}

.btn.solid {
  background: var(--le-gradient);
  color: #fff;
  border: none;
  border-radius: 8px;
}

.btn.solid.danger {
  background: var(--le-danger);
}
</style>
