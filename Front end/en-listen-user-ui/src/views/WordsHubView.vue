<template>
  <PageShell
    title="单词"
    subtitle="以单词本为主，词根辅助扩展"
    :back="false"
    badge="WORDS"
  >
    <SectionSubNav :tabs="wordsTabs" aria-label="单词栏目" />

    <button
      type="button"
      class="hub-primary le-paper le-paper--ruled le-paper-flip"
      @click="goMyWords"
    >
      <div class="hub-primary-top">
        <span class="hub-badge">主要</span>
        <el-icon :size="22" color="var(--le-primary)"><Notebook /></el-icon>
      </div>
      <h2>我的单词本</h2>
      <p>收藏、复习与管理你的词汇，间隔重复记得更牢</p>
      <span class="hub-cta">{{ isLoggedIn ? '打开单词本' : '登录后使用' }} →</span>
    </button>

    <div class="hub-secondary">
      <p class="hub-secondary-label">更多</p>
      <button
        v-for="(item, i) in secondaryItems"
        :key="item.path"
        type="button"
        class="hub-card le-paper le-paper-flip"
        :class="`le-paper-flip-delay-${Math.min(i + 1, 5)}`"
        @click="router.push(item.to || item.path)"
      >
        <div class="hub-card-icon" :style="{ color: item.color }">
          <el-icon :size="22"><component :is="item.icon" /></el-icon>
        </div>
        <div class="hub-card-body">
          <h3>{{ item.title }}</h3>
          <p>{{ item.desc }}</p>
        </div>
        <el-icon class="hub-card-arrow"><ArrowRight /></el-icon>
      </button>
    </div>

    <div class="hub-download">
      <p class="hub-secondary-label">手机 App</p>
      <EaseWordDownload variant="compact" />
    </div>
  </PageShell>
</template>

<script setup>
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { Notebook, Collection, Timer, Files, ArrowRight } from '@element-plus/icons-vue'
import PageShell from '@/components/PageShell.vue'
import SectionSubNav from '@/components/SectionSubNav.vue'
import EaseWordDownload from '@/components/EaseWordDownload.vue'
import { wordsTabs } from '@/utils/sectionTabs'

const router = useRouter()

const isLoggedIn = computed(() =>
  !!(localStorage.getItem('token') && localStorage.getItem('userId'))
)

const goMyWords = () => {
  if (isLoggedIn.value) {
    router.push({ name: 'myWords' })
    return
  }
  router.push({ path: '/login', query: { redirect: '/my-words' } })
}

const secondaryItems = computed(() => [
  {
    title: '词根学习',
    desc: '系统扩展词汇量（辅助）',
    path: '/word-roots',
    icon: Collection,
    color: 'var(--le-success)',
  },
  {
    title: '官方词本',
    desc: '四六级 / 考研高频词',
    path: '/word-packs',
    icon: Files,
    color: 'var(--le-primary)',
  },
  {
    title: '单词复习',
    desc: '智能间隔复习到期词',
    path: '/word-review',
    to: isLoggedIn.value
      ? '/word-review'
      : { path: '/login', query: { redirect: '/word-review' } },
    icon: Timer,
    color: 'var(--le-warning)',
  },
])
</script>

<style scoped>
.hub-primary {
  display: block;
  width: 100%;
  padding: 22px 18px;
  border: 1px solid var(--le-border-strong);
  border-radius: var(--le-radius);
  background: var(--le-bg-elevated);
  text-align: left;
  cursor: pointer;
  margin-bottom: 20px;
  -webkit-tap-highlight-color: transparent;
}

.hub-primary-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 10px;
}

.hub-badge {
  font-size: 11px;
  font-weight: 650;
  letter-spacing: 0.04em;
  color: var(--le-primary);
  background: var(--le-gradient-soft);
  padding: 3px 8px;
  border-radius: 6px;
}

.hub-primary h2 {
  margin: 0 0 6px;
  font-size: 20px;
  font-weight: 700;
  color: var(--le-text);
  font-family: var(--le-font-display);
}

.hub-primary p {
  margin: 0 0 14px;
  font-size: 13px;
  color: var(--le-text-muted);
  line-height: 1.45;
}

.hub-cta {
  font-size: 14px;
  font-weight: 600;
  color: var(--le-primary);
}

.hub-secondary-label {
  margin: 0 0 8px 2px;
  font-size: 12px;
  font-weight: 600;
  color: var(--le-text-muted);
  letter-spacing: 0.04em;
}

.hub-secondary {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.hub-card {
  display: flex;
  align-items: center;
  gap: 12px;
  width: 100%;
  padding: 14px 14px;
  border: 1px solid var(--le-border);
  border-radius: var(--le-radius);
  background: var(--le-bg-elevated);
  text-align: left;
  cursor: pointer;
  -webkit-tap-highlight-color: transparent;
}

.hub-card-icon {
  flex-shrink: 0;
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 10px;
  background: var(--le-bg-muted);
}

.hub-card-body {
  flex: 1;
  min-width: 0;
}

.hub-card-body h3 {
  margin: 0 0 2px;
  font-size: 15px;
  font-weight: 600;
  color: var(--le-text);
}

.hub-card-body p {
  margin: 0;
  font-size: 12px;
  color: var(--le-text-muted);
}

.hub-card-arrow {
  flex-shrink: 0;
  color: var(--le-text-muted);
}

.hub-download {
  margin-top: 22px;
}
</style>
