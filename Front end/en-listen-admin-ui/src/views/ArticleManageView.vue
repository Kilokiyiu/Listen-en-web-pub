<template>
  <div class="admin-page">
    <PageHeader
      title="每日一篇"
      description="发布英语阅读文章，管理发布状态"
    />

    <div class="admin-card">
      <div class="admin-card__header">
        <span class="admin-card__title">添加新文章</span>
        <el-button type="primary" link @click="addEmptyArticle">
          <el-icon><Plus /></el-icon>
          再加一篇
        </el-button>
      </div>
      <div class="admin-card__body">
        <div class="article-list">
          <div v-for="(article, index) in form.articles" :key="index" class="article-item">
            <div class="article-item__header">
              <span class="article-number">第 {{ index + 1 }} 篇</span>
              <el-button
                v-if="form.articles.length > 1"
                type="danger"
                size="small"
                link
                @click="removeArticle(index)"
              >
                删除
              </el-button>
            </div>

            <el-form :model="article" :rules="rules" :ref="el => setFormRef(el, index)" label-width="100px" label-position="top">
              <el-row :gutter="16">
                <el-col :span="8">
                  <el-form-item label="公开日期" prop="publicDate">
                    <el-date-picker
                      v-model="article.publicDate"
                      type="date"
                      placeholder="选择日期"
                      format="YYYY-MM-DD"
                      value-format="YYYY-MM-DD"
                      style="width: 100%"
                    />
                  </el-form-item>
                </el-col>
                <el-col :span="8">
                  <el-form-item label="中文标题" prop="titleChinese">
                    <el-input v-model="article.titleChinese" placeholder="例如：科技改变生活" />
                  </el-form-item>
                </el-col>
                <el-col :span="8">
                  <el-form-item label="英文标题" prop="titleEnglish">
                    <el-input v-model="article.titleEnglish" placeholder="例如：Technology Changes Life" />
                  </el-form-item>
                </el-col>
              </el-row>

              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="英语原文" prop="englishText">
                    <el-input
                      v-model="article.englishText"
                      type="textarea"
                      :rows="5"
                      placeholder="请输入英语原文..."
                    />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="中文翻译" prop="chineseText">
                    <el-input
                      v-model="article.chineseText"
                      type="textarea"
                      :rows="5"
                      placeholder="请输入中文翻译..."
                    />
                  </el-form-item>
                </el-col>
              </el-row>
            </el-form>
          </div>
        </div>

        <div class="action-buttons">
          <el-button type="primary" size="large" @click="handleSubmit" :loading="submitting">
            {{ submitting ? '提交中...' : `提交 ${form.articles.length} 篇文章` }}
          </el-button>
        </div>
      </div>
    </div>

    <div class="admin-card admin-table-card">
      <div class="admin-card__header">
        <span class="admin-card__title">已发布的文章</span>
        <el-tag type="info" effect="plain">共 {{ articles.length }} 篇</el-tag>
      </div>
      <div class="admin-card__body table-body">
        <el-table :data="articles" stripe style="width: 100%" v-loading="loading">
          <el-table-column prop="publicDate" label="公开日期" width="120" />
          <el-table-column prop="titleChinese" label="中文标题" min-width="150" show-overflow-tooltip />
          <el-table-column prop="titleEnglish" label="英文标题" min-width="150" show-overflow-tooltip />
          <el-table-column prop="creationTime" label="录入日期" width="160">
            <template #default="{ row }">
              {{ formatDate(row.creationTime) }}
            </template>
          </el-table-column>
          <el-table-column label="状态" width="90">
            <template #default="{ row }">
              <el-tag :type="row.isPublished ? 'success' : 'info'" size="small" effect="light">
                {{ row.isPublished ? '已发布' : '草稿' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="操作" width="150" fixed="right">
            <template #default="{ row }">
              <el-button type="primary" link size="small" @click="toggleStatus(row)">
                {{ row.isPublished ? '取消发布' : '发布' }}
              </el-button>
              <el-button type="danger" link size="small" @click="handleDelete(row)">
                删除
              </el-button>
            </template>
          </el-table-column>
        </el-table>

        <el-empty v-if="articles.length === 0 && !loading" description="暂无文章，点击上方表单添加" />
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getAllArticles, batchAddArticles, deleteArticle, toggleArticlePublishStatus } from '../api/Admin'
import PageHeader from '../components/PageHeader.vue'

const loading = ref(false)
const submitting = ref(false)
const articles = ref([])
const formRefs = reactive({})

const getDefaultArticle = () => ({
  publicDate: '',
  titleChinese: '',
  titleEnglish: '',
  englishText: '',
  chineseText: '',
})

const form = reactive({
  articles: [getDefaultArticle()],
})

const rules = {
  publicDate: [{ required: true, message: '请选择公开日期', trigger: 'change' }],
  titleChinese: [{ required: true, message: '请输入中文标题', trigger: 'blur' }],
  titleEnglish: [{ required: true, message: '请输入英文标题', trigger: 'blur' }],
  englishText: [{ required: true, message: '请输入英语原文', trigger: 'blur' }],
  chineseText: [{ required: true, message: '请输入中文翻译', trigger: 'blur' }],
}

const setFormRef = (el, index) => {
  if (el) {
    formRefs[index] = el
  }
}

const addEmptyArticle = () => {
  form.articles.push(getDefaultArticle())
  const today = new Date()
  const pad = n => String(n).padStart(2, '0')
  form.articles[form.articles.length - 1].publicDate = `${today.getFullYear()}-${pad(today.getMonth() + 1)}-${pad(today.getDate())}`
}

const removeArticle = (index) => {
  form.articles.splice(index, 1)
}

const loadArticles = async () => {
  loading.value = true
  try {
    articles.value = await getAllArticles()
  } catch (e) {
    // 错误已在拦截器中处理
  } finally {
    loading.value = false
  }
}

const handleSubmit = async () => {
  const validPromises = Object.values(formRefs).map(ref => {
    return ref?.validate?.() || Promise.resolve(true)
  })

  const results = await Promise.all(validPromises)
  if (results.some(r => !r)) {
    ElMessage.warning('请填写完整表单')
    return
  }

  const articlesData = form.articles.map(a => ({
    publicDate: a.publicDate,
    titleChinese: a.titleChinese,
    titleEnglish: a.titleEnglish,
    englishText: a.englishText,
    chineseText: a.chineseText,
  }))

  submitting.value = true
  try {
    await batchAddArticles({ articles: articlesData })
    ElMessage.success(`成功添加 ${form.articles.length} 篇文章`)
    form.articles = [getDefaultArticle()]
    await loadArticles()
  } catch (e) {
    if (e?.response?.data?.errors) {
      const errors = e.response.data.errors
      const errorMessages = Object.values(errors).flat().join('；')
      ElMessage.error(`提交失败：${errorMessages}`)
    }
  } finally {
    submitting.value = false
  }
}

const toggleStatus = async (row) => {
  try {
    await toggleArticlePublishStatus(row.id)
    ElMessage.success(row.isPublished ? '已取消发布' : '已发布')
    await loadArticles()
  } catch (e) {
    // 错误已在拦截器中处理
  }
}

const handleDelete = async (row) => {
  try {
    await ElMessageBox.confirm(
      `确定删除文章「${row.titleChinese}」吗？`,
      '确认删除',
      { type: 'warning' }
    )
    await deleteArticle(row.id)
    ElMessage.success('删除成功')
    await loadArticles()
  } catch (e) {
    if (e !== 'cancel') {
      // 错误已在拦截器中处理
    }
  }
}

const formatDate = (dateStr) => {
  if (!dateStr) return '-'
  const d = new Date(dateStr)
  return d.toLocaleString('zh-CN')
}

onMounted(() => {
  loadArticles()
})
</script>

<style scoped>
.article-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.article-item {
  background: #fafafa;
  border: 1px solid var(--admin-border);
  border-radius: var(--admin-radius);
  padding: 20px;
}

.article-item__header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.article-number {
  font-weight: 600;
  color: var(--admin-text);
}

.action-buttons {
  margin-top: 24px;
  text-align: center;
}

.table-body {
  padding: 0;
}

.table-body :deep(.el-table) {
  border-radius: 0 0 var(--admin-radius) var(--admin-radius);
}

.table-body :deep(.el-empty) {
  padding: 32px 0;
}
</style>
