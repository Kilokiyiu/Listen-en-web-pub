<template>
  <PageShell :title="pageTitle" :badge="badgeText" back-label="返回">
    <!-- 未选分类：先选听力类型（手机底栏「听力」会进这里） -->
    <div v-if="!categoryId" class="category-picker">
      <p class="picker-hint">选择要练习的听力类型</p>
      <div v-if="categoriesLoading" class="le-loading-wrap">
        <el-icon class="is-loading" :size="28"><Loading /></el-icon>
        <span>加载中...</span>
      </div>
      <el-empty v-else-if="categories.length === 0" description="暂无听力分类" />
      <el-row v-else :gutter="16">
        <el-col v-for="cat in categories" :key="cat.id" :xs="12" :sm="8" :md="6">
          <div
            class="category-card le-card le-card-interactive"
            @click="selectCategory(cat.id)"
          >
            <span class="exam-tag" :class="cat.code || 'cet4'">{{ categoryLabelOf(cat) }}</span>
            <h3 class="exam-title">{{ categoryTitleOf(cat) }}</h3>
            <div class="exam-footer">
              <span><el-icon><Headset /></el-icon> 查看试卷</span>
              <el-icon><ArrowRight /></el-icon>
            </div>
          </div>
        </el-col>
      </el-row>
    </div>

    <!-- 已选分类：试卷列表 -->
    <template v-else>
      <div class="category-switch">
        <el-button text type="primary" @click="clearCategory">
          <el-icon><ArrowLeft /></el-icon>
          切换类型
        </el-button>
      </div>

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
    </template>
  </PageShell>
</template>

<script setup>
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ArrowLeft } from '@element-plus/icons-vue'
import PageShell from '../components/PageShell.vue'
import { getAlbumsByCategoryId, getCategories } from '../api/Listen.js'

const route = useRoute()
const router = useRouter()
const categoryId = computed(() => route.query.categoryId)

const pageTitle = ref('听力真题')
const categoryTag = ref('cet4')
const categoryLabel = ref('CET-4')
const albumList = ref([])
const categories = ref([])
const loading = ref(false)
const categoriesLoading = ref(false)
const currentPage = ref(1)
const pageSize = ref(12)

const categoryMeta = {
  cet6: { title: '六级听力真题', label: 'CET-6' },
  cet4: { title: '四级听力真题', label: 'CET-4' },
  ielts: { title: '雅思听力真题', label: 'IELTS' },
  toefl: { title: '托福听力真题', label: 'TOEFL' },
}

const examList = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  return albumList.value.slice(start, start + pageSize.value)
})

const totalCount = computed(() => albumList.value.length)

const badgeText = computed(() => {
  if (!categoryId.value) return categories.value.length ? `共 ${categories.value.length} 类` : ''
  return `共 ${totalCount.value} 套`
})

const categoryLabelOf = (cat) => {
  const meta = categoryMeta[cat.code]
  return meta?.label || cat.name?.chinese || cat.name || cat.code || '听力'
}

const categoryTitleOf = (cat) => {
  const meta = categoryMeta[cat.code]
  return meta?.title || cat.name?.chinese || cat.name || '听力真题'
}

const handlePageChange = (page) => {
  currentPage.value = page
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

const goDetail = (albumId) => router.push({ name: 'examDetail', query: { albumId } })

const selectCategory = (id) => {
  router.push({ name: 'exams', query: { categoryId: id } })
}

const clearCategory = () => {
  router.push({ name: 'exams' })
}

const loadCategories = async () => {
  categoriesLoading.value = true
  try {
    categories.value = (await getCategories()) || []
    pageTitle.value = '听力真题'
  } catch (e) {
    console.error('获取分类失败', e)
    categories.value = []
  } finally {
    categoriesLoading.value = false
  }
}

const loadAlbums = async () => {
  if (!categoryId.value) return
  loading.value = true
  currentPage.value = 1
  try {
    const [albumData, cats] = await Promise.all([
      getAlbumsByCategoryId(categoryId.value),
      categories.value.length ? Promise.resolve(categories.value) : getCategories(),
    ])
    if (!categories.value.length) categories.value = cats || []

    albumList.value = (albumData || []).map(a => ({
      id: a.id,
      title: a.name?.chinese || a.name,
    }))
    const cat = (categories.value || []).find(c => c.id === categoryId.value)
    if (cat) {
      const meta = categoryMeta[cat.code] || { title: '听力真题', label: 'CET-4' }
      pageTitle.value = meta.title
      categoryTag.value = cat.code || 'cet4'
      categoryLabel.value = meta.label
    }
  } catch (e) {
    console.error('获取试卷失败', e)
    albumList.value = []
  } finally {
    loading.value = false
  }
}

watch(
  categoryId,
  (id) => {
    if (id) loadAlbums()
    else {
      albumList.value = []
      loadCategories()
    }
  },
  { immediate: true }
)
</script>

<style scoped>
.picker-hint {
  margin: 0 0 16px;
  font-size: 14px;
  color: var(--le-text-muted);
}

.category-switch {
  margin-bottom: 8px;
}

.category-card,
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
