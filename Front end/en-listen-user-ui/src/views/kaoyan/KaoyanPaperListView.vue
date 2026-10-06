<template>
  <PageShell title="考研英语" back-label="返回首页">
    <p class="intro">开发中 · 英语一 / 英语二完形与阅读模拟练习（非正式真题库）</p>
    <el-alert
      class="dev-tip"
      type="warning"
      show-icon
      :closable="false"
      title="模拟内容说明"
      description="本模块仍在开发阶段。列表中的试卷用于功能体验与联调，题目内容不代表正式考研真题。"
    />

    <div class="filters">
      <el-radio-group v-model="series" size="default" @change="loadPapers">
        <el-radio-button label="">全部</el-radio-button>
        <el-radio-button label="eng1">英语一</el-radio-button>
        <el-radio-button label="eng2">英语二</el-radio-button>
      </el-radio-group>
    </div>

    <div v-if="loading" class="le-loading-wrap">
      <el-icon class="is-loading" :size="28"><Loading /></el-icon>
      <span>加载试卷...</span>
    </div>

    <el-empty v-else-if="!papers.length" description="暂无试卷" />

    <div v-else class="paper-list">
      <button
        v-for="p in papers"
        :key="p.id"
        type="button"
        class="paper-item le-card"
        @click="goPaper(p)"
      >
        <div class="paper-year">{{ p.year }}</div>
        <div class="paper-meta">
          <div class="paper-title">{{ p.title }}</div>
          <div class="paper-sub">{{ p.seriesName || seriesLabel(p.series) }} · 完形 / 阅读</div>
        </div>
        <el-icon class="paper-arrow"><ArrowRight /></el-icon>
      </button>
    </div>
  </PageShell>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import PageShell from '../../components/PageShell.vue'
import { getKaoyanPapers } from '../../api/Kaoyan.js'

const router = useRouter()
const loading = ref(true)
const papers = ref([])
const series = ref('')

const seriesLabel = (s) => (s === 'eng2' ? '英语二' : '英语一')

const loadPapers = async () => {
  loading.value = true
  try {
    const res = await getKaoyanPapers(series.value || undefined)
    papers.value = (res || []).map((p) => ({
      id: p.id || p.Id,
      year: p.year ?? p.Year,
      series: p.series || p.Series,
      seriesName: p.seriesName || p.SeriesName,
      title: p.title || p.Title,
    }))
  } catch (e) {
    papers.value = []
  } finally {
    loading.value = false
  }
}

const goPaper = (p) => {
  router.push({ name: 'kaoyanPaper', query: { paperId: p.id } })
}

onMounted(loadPapers)
</script>

<style scoped>
.intro {
  margin: 0 0 12px;
  color: var(--le-text-muted);
  font-size: 14px;
}

.dev-tip {
  margin-bottom: 16px;
}

.filters {
  margin-bottom: 18px;
}

.paper-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
  max-width: 720px;
}

.paper-item {
  display: flex;
  align-items: center;
  gap: 14px;
  width: 100%;
  text-align: left;
  padding: 14px 16px;
  border: 1px solid var(--le-border);
  background: #fff;
  cursor: pointer;
  transition: border-color 0.15s, box-shadow 0.15s;
}

.paper-item:hover {
  border-color: #93c5fd;
  box-shadow: 0 4px 14px rgba(37, 99, 235, 0.08);
}

.paper-year {
  flex-shrink: 0;
  width: 56px;
  height: 56px;
  border-radius: 12px;
  background: var(--le-gradient-soft, #eff6ff);
  color: var(--le-primary, #2563eb);
  font-weight: 700;
  font-size: 15px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.paper-meta { flex: 1; min-width: 0; }
.paper-title {
  font-weight: 650;
  font-size: 15px;
  color: var(--le-text);
}
.paper-sub {
  margin-top: 4px;
  font-size: 12px;
  color: var(--le-text-muted);
}
.paper-arrow { color: var(--le-text-muted); }
</style>
