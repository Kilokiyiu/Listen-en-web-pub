<template>
  <PageShell :title="pageTitle" :badge="`共 ${totalCount} 套`" back-label="返回">
    <div v-if="loading" class="le-loading-wrap">
      <el-icon class="is-loading" :size="28"><Loading /></el-icon>
      <span>加载中...</span>
    </div>

    <el-row v-else :gutter="16">
      <el-col v-for="item in examList" :key="item.id" :xs="12" :sm="8" :md="6">
        <div class="exam-card le-card le-card-interactive" @click="goDetail(item.id)">
          <span class="exam-tag" :class="categoryTag">{{ categoryLabel }}</span>
          <h3 class="exam-title">{{ item.title }}</h3>
          <div class="exam-footer">
            <span><el-icon><Headset /></el-icon> 完整听力</span>
            <el-icon><ArrowRight /></el-icon>
          </div>
        </div>
      </el-col>
    </el-row>

    <el-empty v-if="!loading && examList.length === 0" description="暂无试卷" />

    <div v-if="totalCount > pageSize" class="pagination-wrap">
      <el-pagination
        :current-page="currentPage"
        :page-size="pageSize"
        :total="totalCount"
        layout="prev, pager, next, total"
        background
        @current-change="handlePageChange"
      />
    </div>
  </PageShell>
</template>

<script setup>
import { computed, ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import PageShell from '../components/PageShell.vue'
import { getAlbumsByCategoryId, getCategories } from '../api/Listen.js'

const route = useRoute()
const router = useRouter()
const categoryId = computed(() => route.query.categoryId)

const pageTitle = ref('听力真题')
const categoryTag = ref('cet6')
const categoryLabel = ref('CET-6')
const albumList = ref([])
const loading = ref(false)
const currentPage = ref(1)
const pageSize = ref(12)

const categoryMeta = {
  cet6: { title: '六级听力真题', label: 'CET-6' },
  cet4: { title: '四级听力真题', label: 'CET-4' },
  ielts: { title: '雅思听力真题', label: 'IELTS' },
  toefl: { title: '托福听力真题', label: 'TOEFL' }
}

const examList = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  return albumList.value.slice(start, start + pageSize.value)
})

const totalCount = computed(() => albumList.value.length)

const handlePageChange = (page) => {
  currentPage.value = page
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

const goDetail = (albumId) => router.push({ name: 'examDetail', query: { albumId } })

const loadAlbums = async () => {
  if (!categoryId.value) return
  loading.value = true
  try {
    const [albumData, categories] = await Promise.all([
      getAlbumsByCategoryId(categoryId.value),
      getCategories()
    ])
    albumList.value = (albumData || []).map(a => ({
      id: a.id,
      title: a.name?.chinese || a.name,
    }))
    const cat = (categories || []).find(c => c.id === categoryId.value)
    if (cat) {
      const meta = categoryMeta[cat.code] || { title: '听力真题', label: 'CET-6' }
      pageTitle.value = meta.title
      categoryTag.value = cat.code
      categoryLabel.value = meta.label
    }
  } catch (e) {
    console.error('获取试卷失败', e)
  } finally {
    loading.value = false
  }
}

onMounted(loadAlbums)
</script>

<style scoped>
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
}

.exam-tag.cet4 { background: rgba(37,99,235,0.1); color: var(--le-primary); }
.exam-tag.cet6 { background: rgba(124,58,237,0.1); color: var(--le-purple); }
.exam-tag.ielts { background: rgba(16,185,129,0.1); color: var(--le-success); }
.exam-tag.toefl { background: rgba(245,158,11,0.1); color: var(--le-warning); }

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
  border-top: 1px solid var(--le-border);
  font-size: 13px;
  color: var(--le-text-muted);
}

.exam-footer span {
  display: flex;
  align-items: center;
  gap: 4px;
}

.pagination-wrap {
  display: flex;
  justify-content: center;
  margin-top: 32px;
}
</style>
