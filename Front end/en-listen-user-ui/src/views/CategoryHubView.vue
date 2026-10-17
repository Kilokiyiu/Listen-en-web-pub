<template>
  <PageShell
    :title="hub.h1"
    :subtitle="hub.subtitle"
    :badge="hub.badge"
    back-label="返回首页"
    back-to="/"
  >
    <section class="seo-intro le-paper" aria-label="栏目介绍">
      <p v-for="(p, i) in hub.intro" :key="i">{{ p }}</p>
      <nav v-if="hub.related?.length" class="related-links" aria-label="相关栏目">
        <router-link v-for="link in hub.related" :key="link.to" :to="link.to" class="related-chip">
          {{ link.label }}
        </router-link>
      </nav>
    </section>

    <!-- 听力总览：入口卡片 -->
    <section v-if="hub.kind === 'hub'" class="hub-grid">
      <router-link
        v-for="item in hub.hubLinks"
        :key="item.to"
        :to="item.to"
        class="hub-card le-card le-card-interactive"
      >
        <span class="exam-tag">{{ item.tag }}</span>
        <h2 class="exam-title">{{ item.title }}</h2>
        <p class="hub-desc">{{ item.desc }}</p>
        <span class="hub-cta">进入 →</span>
      </router-link>
    </section>

    <!-- 四六级专题：两级各列一套预览 -->
    <template v-else-if="hub.kind === 'combo'">
      <div v-if="loading" class="le-loading-wrap">
        <el-icon class="is-loading" :size="28"><Loading /></el-icon>
        <span>加载试卷中...</span>
      </div>
      <template v-else>
        <section v-for="block in comboBlocks" :key="block.code" class="combo-block">
          <div class="le-section-header">
            <h2>{{ block.title }}</h2>
            <router-link class="view-all" :to="block.path">查看全部 →</router-link>
          </div>
          <el-empty v-if="!block.albums.length" description="暂无试卷" />
          <el-row v-else :gutter="16">
            <el-col v-for="item in block.albums" :key="item.id" :xs="12" :sm="8" :md="6">
              <div class="exam-card le-card le-card-interactive" @click="goAlbum(item.id)">
                <span class="exam-tag" :class="block.code">{{ block.label }}</span>
                <h3 class="exam-title">{{ item.title }}</h3>
                <div class="exam-footer">
                  <span>开始练习</span>
                  <span>→</span>
                </div>
              </div>
            </el-col>
          </el-row>
        </section>
      </template>
    </template>

    <!-- 单级听力：完整列表 -->
    <template v-else-if="hub.kind === 'listening'">
      <div v-if="loading" class="le-loading-wrap">
        <el-icon class="is-loading" :size="28"><Loading /></el-icon>
        <span>加载试卷中...</span>
      </div>
      <el-empty v-else-if="!albums.length" description="暂无试卷" />
      <el-row v-else :gutter="16">
        <el-col v-for="item in pagedAlbums" :key="item.id" :xs="12" :sm="8" :md="6">
          <div class="exam-card le-card le-card-interactive" @click="goAlbum(item.id)">
            <span class="exam-tag" :class="hub.code">{{ hub.badge }}</span>
            <h3 class="exam-title">{{ item.title }}</h3>
            <div class="exam-footer">
              <span>完整听力</span>
              <span>→</span>
            </div>
          </div>
        </el-col>
      </el-row>
      <div v-if="albums.length > pageSize" class="pagination-wrap">
        <el-pagination
          :current-page="currentPage"
          :page-size="pageSize"
          :total="albums.length"
          layout="prev, pager, next, total"
          background
          @current-change="onPageChange"
        />
      </div>
    </template>
  </PageShell>
</template>

<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import PageShell from '../components/PageShell.vue'
import { getAlbumsByCategoryId, getCategories } from '../api/Listen.js'
import { getSeoHubByPath, SEO_HUBS } from '../utils/seoHubs.js'
import { listCategoryMeta } from '../utils/categoryMeta.js'

const route = useRoute()
const router = useRouter()

const hub = computed(() => getSeoHubByPath(route.path) || SEO_HUBS.listening)

const loading = ref(false)
const albums = ref([])
const comboBlocks = ref([])
const currentPage = ref(1)
const pageSize = 12

const pagedAlbums = computed(() => {
  const start = (currentPage.value - 1) * pageSize
  return albums.value.slice(start, start + pageSize)
})

const goAlbum = (albumId) => {
  router.push({ name: 'examDetail', query: { albumId } })
}

const onPageChange = (page) => {
  currentPage.value = page
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

const resolveCategory = (categories, code) =>
  (categories || []).find((c) => c.code === code)

const loadListening = async () => {
  loading.value = true
  currentPage.value = 1
  try {
    const categories = (await getCategories()) || []
    const cat = resolveCategory(categories, hub.value.code)
    if (!cat) {
      albums.value = []
      return
    }
    const data = await getAlbumsByCategoryId(cat.id)
    albums.value = (data || []).map((a) => ({
      id: a.id,
      title: a.name?.chinese || a.name,
    }))
  } catch (e) {
    console.error('栏目页加载失败', e)
    albums.value = []
  } finally {
    loading.value = false
  }
}

const loadCombo = async () => {
  loading.value = true
  try {
    const categories = (await getCategories()) || []
    const codes = hub.value.comboCodes || []
    const blocks = []
    for (const code of codes) {
      const cat = resolveCategory(categories, code)
      const meta = listCategoryMeta[code] || { title: code, label: code }
      const path = code === 'cet4' ? '/cet4' : code === 'cet6' ? '/cet6' : '/exams'
      let list = []
      if (cat) {
        const data = await getAlbumsByCategoryId(cat.id)
        list = (data || []).slice(0, 8).map((a) => ({
          id: a.id,
          title: a.name?.chinese || a.name,
        }))
      }
      blocks.push({
        code,
        title: meta.title,
        label: meta.label,
        path,
        albums: list,
      })
    }
    comboBlocks.value = blocks
  } catch (e) {
    console.error('专题页加载失败', e)
    comboBlocks.value = []
  } finally {
    loading.value = false
  }
}

const load = async () => {
  const kind = hub.value.kind
  if (kind === 'listening') await loadListening()
  else if (kind === 'combo') await loadCombo()
  else {
    albums.value = []
    comboBlocks.value = []
  }
}

watch(
  () => route.path,
  () => {
    load()
  }
)

onMounted(load)
</script>

<style scoped>
.seo-intro {
  padding: 18px 20px;
  margin-bottom: 20px;
}

.seo-intro p {
  margin: 0 0 10px;
  font-size: 14px;
  line-height: 1.7;
  color: var(--le-text-secondary, #4b5563);
}

.seo-intro p:last-of-type {
  margin-bottom: 0;
}

.related-links {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-top: 14px;
}

.related-chip {
  display: inline-flex;
  padding: 6px 12px;
  border-radius: 8px;
  font-size: 13px;
  color: var(--le-primary, #2563eb);
  background: rgba(37, 99, 235, 0.08);
  text-decoration: none;
}

.related-chip:hover {
  background: rgba(37, 99, 235, 0.14);
}

.hub-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 16px;
}

@media (min-width: 768px) {
  .hub-grid {
    grid-template-columns: repeat(4, minmax(0, 1fr));
  }
}

.hub-card {
  display: block;
  padding: 18px;
  text-decoration: none;
  color: inherit;
}

.hub-desc {
  margin: 0 0 12px;
  font-size: 13px;
  color: var(--le-text-muted, #6b7280);
}

.hub-cta {
  font-size: 13px;
  color: var(--le-primary, #2563eb);
  font-weight: 600;
}

.combo-block {
  margin-bottom: 28px;
}

.le-section-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 12px;
}

.le-section-header h2 {
  margin: 0;
  font-size: 18px;
}

.view-all {
  font-size: 13px;
  color: var(--le-primary, #2563eb);
  text-decoration: none;
}

.exam-card {
  padding: 18px;
  margin-bottom: 16px;
  height: calc(100% - 16px);
}

.exam-tag {
  display: inline-block;
  padding: 3px 10px;
  border-radius: 99px;
  font-size: 11px;
  font-weight: 600;
  margin-bottom: 10px;
  background: rgba(37, 99, 235, 0.1);
  color: var(--le-primary, #2563eb);
}

.exam-tag.cet4 {
  background: rgba(37, 99, 235, 0.1);
  color: var(--le-primary, #2563eb);
}

.exam-tag.cet6 {
  background: rgba(124, 58, 237, 0.1);
  color: var(--le-purple, #7c3aed);
}

.exam-title {
  font-size: 15px;
  font-weight: 600;
  margin: 0 0 16px;
  min-height: 44px;
  line-height: 1.5;
}

.exam-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 12px;
  border-top: 1px solid var(--le-border, #e5e7eb);
  font-size: 13px;
  color: var(--le-text-muted, #6b7280);
}

.pagination-wrap {
  display: flex;
  justify-content: center;
  margin-top: 24px;
}
</style>
