<template>
  <div
    class="quiz-tabs"
    :class="[
      `quiz-tabs--${size}`,
      { 'quiz-tabs--scroll': scroll, 'quiz-tabs--stretch': stretch },
    ]"
  >
    <div v-if="label" class="quiz-tabs__label">{{ label }}</div>
    <div class="quiz-tabs__track" role="tablist" :aria-label="ariaLabel">
      <button
        v-for="item in items"
        :key="item.value"
        type="button"
        role="tab"
        class="quiz-tabs__btn"
        :class="{ active: modelValue === item.value }"
        :aria-selected="modelValue === item.value"
        :title="item.title || item.label"
        @click="onSelect(item.value)"
      >
        <span v-if="item.badge" class="quiz-tabs__badge">{{ item.badge }}</span>
        <span class="quiz-tabs__text">{{ item.label }}</span>
      </button>
    </div>
  </div>
</template>

<script setup>
const props = defineProps({
  modelValue: { type: [String, Number], default: '' },
  /** [{ value, label, title?, badge? }] */
  items: { type: Array, default: () => [] },
  label: { type: String, default: '' },
  ariaLabel: { type: String, default: '大题切换' },
  /** md | sm */
  size: { type: String, default: 'md' },
  scroll: { type: Boolean, default: true },
  /** 少量选项时均分宽度 */
  stretch: { type: Boolean, default: false },
})

const emit = defineEmits(['update:modelValue', 'change'])

const onSelect = (value) => {
  if (value === props.modelValue) return
  emit('update:modelValue', value)
  emit('change', value)
}
</script>

<style scoped>
.quiz-tabs {
  margin-bottom: 14px;
  max-width: 720px;
}

.quiz-tabs__label {
  font-size: 12px;
  font-weight: 600;
  color: var(--le-text-muted);
  letter-spacing: 0.02em;
  margin: 0 0 8px;
}

.quiz-tabs__track {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  padding: 6px;
  background: var(--le-bg-muted);
  border: 1px solid var(--le-border);
  border-radius: 14px;
}

.quiz-tabs--scroll .quiz-tabs__track {
  flex-wrap: nowrap;
  overflow-x: auto;
  -webkit-overflow-scrolling: touch;
  scrollbar-width: none;
}

.quiz-tabs--scroll .quiz-tabs__track::-webkit-scrollbar {
  display: none;
}

.quiz-tabs__btn {
  position: relative;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  flex: 0 0 auto;
  border: 1px solid transparent;
  background: transparent;
  color: var(--le-text-secondary);
  font-family: inherit;
  font-weight: 550;
  cursor: pointer;
  border-radius: 10px;
  transition:
    background 0.18s ease,
    color 0.18s ease,
    border-color 0.18s ease,
    box-shadow 0.18s ease,
    transform 0.15s ease;
  -webkit-tap-highlight-color: transparent;
}

.quiz-tabs--md .quiz-tabs__btn {
  min-height: 38px;
  padding: 8px 16px;
  font-size: 13.5px;
}

.quiz-tabs--sm .quiz-tabs__btn {
  min-height: 32px;
  padding: 6px 12px;
  font-size: 12.5px;
  border-radius: 9px;
}

.quiz-tabs__btn:hover {
  color: var(--le-primary);
  background: rgba(255, 255, 255, 0.72);
}

.quiz-tabs__btn.active {
  color: var(--le-primary);
  background: #fff;
  border-color: rgba(37, 99, 235, 0.18);
  box-shadow: 0 1px 2px rgba(30, 41, 59, 0.06), 0 4px 12px rgba(37, 99, 235, 0.08);
  font-weight: 650;
}

.quiz-tabs__btn:active {
  transform: scale(0.98);
}

.quiz-tabs__badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 18px;
  height: 18px;
  padding: 0 5px;
  border-radius: 99px;
  font-size: 11px;
  font-weight: 700;
  line-height: 1;
  color: var(--le-text-muted);
  background: rgba(55, 75, 105, 0.08);
}

.quiz-tabs__btn.active .quiz-tabs__badge {
  color: #fff;
  background: var(--le-primary);
}

.quiz-tabs__text {
  white-space: nowrap;
}

.quiz-tabs--stretch .quiz-tabs__btn {
  flex: 1 1 0;
}

@media (max-width: 768px) {
  .quiz-tabs__track {
    gap: 6px;
    padding: 5px;
  }

  .quiz-tabs--md .quiz-tabs__btn {
    min-height: 36px;
    padding: 7px 13px;
    font-size: 13px;
  }

  .quiz-tabs--sm .quiz-tabs__btn {
    min-height: 30px;
    padding: 5px 10px;
  }
}
</style>
