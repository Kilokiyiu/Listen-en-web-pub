<template>
  <PageShell
    title="搜索试卷"
    subtitle="支持四级 / 六级听力与考研英语试卷"
    badge="SEARCH"
    back-label="返回"
  >
    <form class="search-box le-paper" @submit.prevent="runSearch">
      <input
        ref="inputRef"
        v-model="keyword"
        class="search-input"
        type="search"
        enterkeyhint="search"
        placeholder="例如：四级 2023、六级听力、英语二、考研"
        autocomplete="off"
        @input="onInput"
      />
      <button type="submit" class="search-submit" :disabled="loading">
        {{ loading ? '…' : '搜索' }}
      </button>
    </form>

    <div class="filter-row" role="tablist" aria-label="结果筛选">
      <button
        v-for="tab in filterTabs"
        :key="tab.value"
        type="button"
        class="filter-chip"
        :class="{ active: filter === tab.value }"
        @click="filter = tab.value"
      >
        {{ tab.label }}
      </button>
    </div>

    <p v-if="hint" class="hint">{{ hint }}</p>

    <div v-if="loading" class="le-loading-wrap">
      <el-icon class="is-loading" :size="28"><Loading /></el-icon>
      <span>加载试卷目录...</span>
    </div>

    <template v-else>
      <el-empty
        v-if="searched && !results.length"
        :description="emptyText"
      />

      <div v-else-if="results.length" class="result-list">
        <button
          v-for="item in results"
          :key="`${item.kind}-${item.id}`"
          type="button"
          class="result-item le-card le-card-interactive"
          @click="goItem(item)"
        >
          <span class="result-tag" :class="item.code || item.kind">{{ item.label }}</span>
          <div class="result-body">
            <h2 class="result-title">{{ item.title }}</h2>
            <p class="result-sub">{{ item.subtitle }}</p>
          </div>
          <el-icon class="result-arrow"><ArrowRight /></el-icon>
        </button>
      </div>

      <section v-else class="suggest le-paper">
        <p class="suggest-title">试试搜索</p>
        <div class="suggest-chips">
          <button
            v-for="s in suggestions"
            :key="s"
            type="button"
            class="suggest-chip"
            @click="quickSearch(s)"
          >
            {{ s }}
          </button>
        </div>
      </section>
    </template>
  </PageShell>
</template>

<script setup>
import { computed, nextTick, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ArrowRight, Loading } from '@element-plus/icons-vue'
import PageShell from '../components/PageShell.vue'
import { loadPaperCatalog, searchPapers } from '../utils/paperCatalog.js'

const route = useRoute()
const router = useRouter()

const inputRef = ref(null)
const keyword = ref('')
const filter = ref('all')
const loading = ref(false)
const searched = ref(false)
const catalog = ref([])
const results = ref([])

const filterTabs = [
  { value: 'all', label: '全部' },
  { value: 'listening', label: '四六级听力' },
  { value: 'kaoyan', label: '考研英语' },
]

const suggestions = ['四级', '六级', '2023', '英语二', '考研', '听力真题']

const hint = computed(() => {
  if (loading.value) return ''
  if (!searched.value) return `已索引 ${catalog.value.length} 套试卷`
  return `找到 ${results.value.length} 套「${keyword.value.trim() || '全部'}」`
})

const emptyText = computed(() => {
  if (filter.value === 'listening') return '没有匹配的四六级听力试卷'
  if (filter.value === 'kaoyan') return '没有匹配的考研试卷'
  return '没有找到相关试卷，换个关键词试试'
})

const applyFilter = () => {
  results.value = searchPapers(catalog.value, keyword.value, { kind: filter.value })
}

const runSearch = () => {
  const q = keyword.value.trim()
  searched.value = true
  applyFilter()
  const nextQuery = { ...route.query }
  if (q) nextQuery.q = q
  else delete nextQuery.q
  if (filter.value !== 'all') nextQuery.type = filter.value
  else delete nextQuery.type
  router.replace({ name: 'paperSearch', query: nextQuery }).catch(() => {})
}

const onInput = () => {
  if (!keyword.value.trim()) {
    searched.value = false
    results.value = []
    return
  }
  searched.value = true
  applyFilter()
}

const quickSearch = (text) => {
  keyword.value = text
  runSearch()
}

const goItem = (item) => {
  router.push(item.route)
}

const ensureCatalog = async () => {
  loading.value = true
  try {
    catalog.value = await loadPaperCatalog()
  } catch (e) {
    console.error('加载试卷目录失败', e)
    catalog.value = []
  } finally {
    loading.value = false
  }
}

watch(filter, () => {
  if (!searched.value && !keyword.value.trim()) return
  searched.value = true
  applyFilter()
  const nextQuery = { ...route.query }
  if (keyword.value.trim()) nextQuery.q = keyword.value.trim()
  else delete nextQuery.q
  if (filter.value !== 'all') nextQuery.type = filter.value
  else delete nextQuery.type
  router.replace({ name: 'paperSearch', query: nextQuery }).catch(() => {})
})

onMounted(async () => {
  await ensureCatalog()
  const q = typeof route.query.q === 'string' ? route.query.q : ''
  const type = typeof route.query.type === 'string' ? route.query.type : 'all'
  if (['all', 'listening', 'kaoyan'].includes(type)) filter.value = type
  keyword.value = q
  if (q) {
    searched.value = true
    applyFilter()
  }
  await nextTick()
  inputRef.value?.focus?.()
})
</script>

<style scoped>
.search-box {
  display: flex;
  gap: 10px;
  padding: 12px;
  margin-bottom: 14px;
  align-items: center;
}

.search-input {
  flex: 1;
  min-width: 0;
  height: 42px;
  border: 1px solid var(--le-border);
  border-radius: 10px;
  padding: 0 14px;
  font-size: 15px;
  font-family: inherit;
  background: #fff;
  color: var(--le-text);
  outline: none;
}

.search-input:focus {
  border-color: rgba(37, 99, 235, 0.45);
  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
}

.search-submit {
  flex-shrink: 0;
  height: 42px;
  padding: 0 18px;
  border: none;
  border-radius: 10px;
  background: var(--le-gradient);
  color: #fff;
  font-weight: 600;
  cursor: pointer;
  font-family: inherit;
}

.search-submit:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.filter-row {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-bottom: 10px;
}

.filter-chip {
  border: 1px solid var(--le-border);
  background: #fff;
  color: var(--le-text-secondary);
  border-radius: 999px;
  padding: 6px 12px;
  font-size: 13px;
  cursor: pointer;
  font-family: inherit;
}

.filter-chip.active {
  border-color: rgba(37, 99, 235, 0.35);
  background: var(--le-gradient-soft);
  color: var(--le-primary);
  font-weight: 600;
}

.hint {
  margin: 0 0 12px;
  font-size: 13px;
  color: var(--le-text-muted);
}

.result-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.result-item {
  display: flex;
  align-items: center;
  gap: 12px;
  width: 100%;
  padding: 14px 16px;
  text-align: left;
  cursor: pointer;
  font-family: inherit;
}

.result-tag {
  flex-shrink: 0;
  font-size: 11px;
  font-weight: 700;
  padding: 4px 8px;
  border-radius: 6px;
  background: var(--le-bg-muted);
  color: var(--le-text-secondary);
}

.result-tag.cet4 {
  background: rgba(34, 211, 238, 0.15);
  color: #0e7490;
}

.result-tag.cet6 {
  background: rgba(167, 139, 250, 0.18);
  color: #6d28d9;
}

.result-tag.eng1,
.result-tag.eng2,
.result-tag.kaoyan {
  background: rgba(239, 68, 68, 0.12);
  color: #b91c1c;
}

.result-body {
  flex: 1;
  min-width: 0;
}

.result-title {
  margin: 0;
  font-size: 15px;
  font-weight: 600;
  color: var(--le-text);
  line-height: 1.35;
}

.result-sub {
  margin: 4px 0 0;
  font-size: 12px;
  color: var(--le-text-muted);
}

.result-arrow {
  color: var(--le-text-muted);
  flex-shrink: 0;
}

.suggest {
  padding: 16px;
}

.suggest-title {
  margin: 0 0 10px;
  font-size: 13px;
  color: var(--le-text-muted);
}

.suggest-chips {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.suggest-chip {
  border: 1px dashed var(--le-border-strong);
  background: transparent;
  border-radius: 999px;
  padding: 6px 12px;
  font-size: 13px;
  color: var(--le-primary);
  cursor: pointer;
  font-family: inherit;
}

.suggest-chip:hover {
  background: var(--le-gradient-soft);
}
</style>
