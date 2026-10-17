<template>
  <PageShell
    title="考研英语完形填空与阅读在线练习"
    subtitle="英语一 / 英语二历年真题，提交后查看答案与解析"
    :back="false"
    badge="KY"
  >
    <p class="intro">
      考研英语备考可在此按英语一、英语二筛选试卷，在线完成完形填空与阅读理解，提交后查看正确答案与解析。
      也可配合
      <router-link to="/cet4">四级听力</router-link>、
      <router-link to="/cet6">六级听力</router-link>、
      <router-link to="/word-packs">官方词本</router-link>
      一起练习。
    </p>

    <QuizSectionTabs
      v-model="series"
      :items="seriesTabs"
      :scroll="false"
      stretch
      aria-label="科目筛选"
      @change="loadPapers"
    />

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
import QuizSectionTabs from '../../components/QuizSectionTabs.vue'
import { getKaoyanPapers } from '../../api/Kaoyan.js'
import { getPageCache, setPageCache } from '../../utils/pageCache.js'

const router = useRouter()
const loading = ref(true)
const papers = ref([])
const series = ref('')

const seriesTabs = [
  { value: '', label: '全部' },
  { value: 'eng1', label: '英语一' },
  { value: 'eng2', label: '英语二' },
]

const seriesLabel = (s) => (s === 'eng2' ? '英语二' : '英语一')

const mapPapers = (res) =>
  (res || []).map((p) => ({
    id: p.id || p.Id,
    year: p.year ?? p.Year,
    series: p.series || p.Series,
    seriesName: p.seriesName || p.SeriesName,
    title: p.title || p.Title,
  }))

const loadPapers = async () => {
  const cacheKey = `kaoyan:list:${series.value || 'all'}`
  const cached = getPageCache(cacheKey)
  if (cached) {
    papers.value = cached
    loading.value = false
  } else {
    loading.value = true
  }
  try {
    const res = await getKaoyanPapers(series.value || undefined)
    const mapped = mapPapers(res)
    papers.value = mapped
    setPageCache(cacheKey, mapped, 5 * 60 * 1000)
  } catch (e) {
    if (!cached) papers.value = []
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
  line-height: 1.7;
}

.intro a {
  color: var(--le-primary, #2563eb);
  text-decoration: none;
}

.intro a:hover {
  text-decoration: underline;
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
