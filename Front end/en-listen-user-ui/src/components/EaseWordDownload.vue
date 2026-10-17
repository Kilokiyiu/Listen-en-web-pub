<template>
  <aside class="ease-word-card" :class="[`is-${variant}`]">
    <div class="card-top">
      <span class="badge">ANDROID</span>
      <span class="badge badge--version">v{{ EASE_WORD.version }}</span>
    </div>
    <div class="title-row">
      <h2 class="app-name">{{ EASE_WORD.name }}</h2>
    </div>
    <p class="app-cn">{{ EASE_WORD.nameCn }}</p>
    <p class="app-desc">{{ EASE_WORD.desc }}</p>
    <a
      class="app-download"
      :href="EASE_WORD.href"
      :download="EASE_WORD.filename"
    >
      下载 APK
    </a>
    <p
      class="app-note"
      title="旧签名版本（如 0.8.0）需先上传云端并卸载后再安装；之后可直接覆盖更新。"
    >
      {{ EASE_WORD.note }}
    </p>
  </aside>
</template>

<script setup>
import { EASE_WORD } from '@/config/easeWord'

defineProps({
  /** hero | section | compact */
  variant: { type: String, default: 'section' },
})
</script>

<style scoped>
.ease-word-card {
  background: rgba(255, 255, 255, 0.94);
  background-image: var(--le-paper-grain);
  border: 1px solid var(--le-border);
  border-radius: 4px 8px 8px 4px;
  padding: 18px;
  box-shadow: var(--le-shadow-sm);
}

.ease-word-card.is-hero {
  justify-self: stretch;
  width: 100%;
  max-width: none;
}

.ease-word-card.is-compact {
  padding: 14px 16px;
}

.card-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  margin-bottom: 12px;
}

.badge {
  font-family: var(--le-font-mono);
  font-size: 11px;
  font-weight: 500;
  color: var(--le-primary);
  background: rgba(37, 99, 235, 0.08);
  border: 1px solid rgba(37, 99, 235, 0.18);
  padding: 3px 8px;
  border-radius: 4px;
  letter-spacing: 0.04em;
}

.badge--version {
  color: var(--le-text-muted);
  background: var(--le-bg-muted);
  border-color: var(--le-border);
}

.app-name {
  margin: 0 0 4px;
  font-family: var(--le-font-display);
  font-size: 22px;
  font-weight: 700;
  color: var(--le-text);
}

.is-compact .app-name {
  font-size: 18px;
}

.app-cn {
  margin: 0 0 10px;
  color: var(--le-text-secondary);
  font-size: 13px;
  font-weight: 500;
}

.app-desc {
  margin: 0 0 16px;
  font-size: 13px;
  color: var(--le-text-secondary);
}

.is-compact .app-desc {
  margin-bottom: 12px;
}

.app-download {
  display: block;
  text-align: center;
  text-decoration: none;
  background: var(--le-primary);
  color: #fff;
  font-weight: 700;
  font-size: 14px;
  padding: 10px;
  border-radius: 8px;
  transition: opacity 0.15s ease, background 0.15s ease;
}

.app-download:hover {
  opacity: 0.92;
  background: var(--le-primary-light);
  color: #fff;
}

.app-note {
  margin: 10px 0 0;
  font-size: 11px;
  color: var(--le-text-muted);
  text-align: center;
  line-height: 1.35;
}
</style>
