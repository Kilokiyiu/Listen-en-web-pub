<template>
  <PageShell
    title="阅读"
    subtitle="选择今天想读的内容"
    :back="false"
    badge="READ"
  >
    <SectionSubNav :tabs="readingTabs" aria-label="阅读栏目" />

    <div class="hub-grid">
      <button
        v-for="(item, i) in items"
        :key="item.path"
        type="button"
        class="hub-card le-paper le-paper-flip"
        :class="`le-paper-flip-delay-${Math.min(i + 1, 5)}`"
        @click="router.push(item.path)"
      >
        <div class="hub-card-icon" :style="{ color: item.color }">
          <el-icon :size="28"><component :is="item.icon" /></el-icon>
        </div>
        <div class="hub-card-body">
          <h2>{{ item.title }}</h2>
          <p>{{ item.desc }}</p>
        </div>
        <el-icon class="hub-card-arrow"><ArrowRight /></el-icon>
      </button>
    </div>
  </PageShell>
</template>

<script setup>
import { useRouter } from 'vue-router'
import { Document, Postcard, ArrowRight } from '@element-plus/icons-vue'
import PageShell from '@/components/PageShell.vue'
import SectionSubNav from '@/components/SectionSubNav.vue'
import { readingTabs } from '@/utils/sectionTabs'

const router = useRouter()

const items = [
  {
    title: '每日短文',
    desc: '英汉对照精读，配合听力保持语感',
    path: '/daily',
    icon: Document,
    color: 'var(--le-primary)',
  },
  {
    title: 'BBC 新闻',
    desc: '外刊资讯阅读，提升语感与理解',
    path: '/bbc-news',
    icon: Postcard,
    color: '#bb1919',
  },
]
</script>

<style scoped>
.hub-grid {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.hub-card {
  display: flex;
  align-items: center;
  gap: 14px;
  width: 100%;
  padding: 18px 16px;
  border: 1px solid var(--le-border);
  border-radius: var(--le-radius);
  background: var(--le-bg-elevated);
  text-align: left;
  cursor: pointer;
  transition: border-color 0.15s, transform 0.15s;
  -webkit-tap-highlight-color: transparent;
}

.hub-card:active {
  transform: scale(0.99);
}

.hub-card-icon {
  flex-shrink: 0;
  width: 48px;
  height: 48px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 12px;
  background: var(--le-bg-muted);
}

.hub-card-body {
  flex: 1;
  min-width: 0;
}

.hub-card-body h2 {
  margin: 0 0 4px;
  font-size: 16px;
  font-weight: 650;
  color: var(--le-text);
  font-family: var(--le-font-display);
}

.hub-card-body p {
  margin: 0;
  font-size: 13px;
  color: var(--le-text-muted);
  line-height: 1.4;
}

.hub-card-arrow {
  flex-shrink: 0;
  color: var(--le-text-muted);
}
</style>
