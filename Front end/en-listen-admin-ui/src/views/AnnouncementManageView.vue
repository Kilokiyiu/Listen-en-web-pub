<template>
  <div class="admin-page">
    <PageHeader
      title="公告管理"
      description="发布平台公告，并维护用户端展示的网站运行状态"
    />

    <div class="admin-card">
      <div class="admin-card__header">
        <span class="admin-card__title">网站运行状态</span>
        <el-button type="primary" :loading="runtimeSaving" @click="saveRuntime">保存状态</el-button>
      </div>
      <div class="admin-card__body">
        <el-form :model="runtimeForm" label-position="top" class="runtime-form">
          <el-row :gutter="16">
            <el-col :xs="24" :sm="8">
              <el-form-item label="运行状态">
                <el-select v-model="runtimeForm.status" style="width: 100%">
                  <el-option label="正常" value="Normal" />
                  <el-option label="提示" value="Notice" />
                  <el-option label="维护中" value="Maintenance" />
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :xs="24" :sm="8">
              <el-form-item label="状态文案">
                <el-input v-model="runtimeForm.statusText" maxlength="200" placeholder="例如：网站运行正常" />
              </el-form-item>
            </el-col>
            <el-col :xs="24" :sm="8">
              <el-form-item label="补充说明">
                <el-input v-model="runtimeForm.extraInfo" maxlength="500" placeholder="可选，例如服务范围说明" />
              </el-form-item>
            </el-col>
          </el-row>
          <p v-if="runtimeUpdatedAt" class="runtime-meta">上次更新：{{ formatTime(runtimeUpdatedAt) }}</p>
        </el-form>
      </div>
    </div>

    <div class="admin-card admin-table-card">
      <div class="admin-card__header">
        <span class="admin-card__title">公告列表</span>
        <el-button type="primary" @click="openCreate">
          <el-icon><Plus /></el-icon>
          发布公告
        </el-button>
      </div>
      <div class="admin-card__body table-body">
        <el-table v-loading="loading" :data="announcements" stripe style="width: 100%">
          <el-table-column prop="publishDate" label="日期" width="120" />
          <el-table-column prop="title" label="标题" min-width="160" show-overflow-tooltip />
          <el-table-column prop="content" label="内容" min-width="240" show-overflow-tooltip />
          <el-table-column label="置顶" width="80" align="center">
            <template #default="{ row }">
              <el-tag v-if="row.isPinned" type="warning" size="small" effect="light">置顶</el-tag>
              <span v-else class="muted">—</span>
            </template>
          </el-table-column>
          <el-table-column label="状态" width="90" align="center">
            <template #default="{ row }">
              <el-tag :type="row.isPublished ? 'success' : 'info'" size="small" effect="light">
                {{ row.isPublished ? '已发布' : '草稿' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="更新时间" width="170">
            <template #default="{ row }">{{ formatTime(row.updatedAt) }}</template>
          </el-table-column>
          <el-table-column label="操作" width="220" fixed="right">
            <template #default="{ row }">
              <el-button type="primary" link size="small" @click="openEdit(row)">编辑</el-button>
              <el-button type="primary" link size="small" @click="togglePublish(row)">
                {{ row.isPublished ? '取消发布' : '发布' }}
              </el-button>
              <el-button type="danger" link size="small" @click="handleDelete(row)">删除</el-button>
            </template>
          </el-table-column>
        </el-table>
        <el-empty v-if="!loading && announcements.length === 0" description="暂无公告，点击右上角发布" />
      </div>
    </div>

    <el-dialog
      v-model="dialogVisible"
      :title="editingId ? '编辑公告' : '发布公告'"
      width="560px"
      destroy-on-close
      :close-on-click-modal="false"
    >
      <el-form ref="formRef" :model="form" :rules="rules" label-position="top">
        <el-form-item label="标题" prop="title">
          <el-input v-model="form.title" maxlength="200" show-word-limit placeholder="公告标题" />
        </el-form-item>
        <el-form-item label="内容" prop="content">
          <el-input
            v-model="form.content"
            type="textarea"
            :rows="5"
            maxlength="4000"
            show-word-limit
            placeholder="公告正文"
          />
        </el-form-item>
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="发布日期" prop="publishDate">
              <el-date-picker
                v-model="form.publishDate"
                type="date"
                value-format="YYYY-MM-DD"
                format="YYYY-MM-DD"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="排序（越小越靠前）">
              <el-input-number v-model="form.sortOrder" :min="0" :max="9999" style="width: 100%" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item>
          <el-checkbox v-model="form.isPinned">置顶</el-checkbox>
          <el-checkbox v-model="form.isPublished" style="margin-left: 16px">立即发布</el-checkbox>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="saving" @click="submitForm">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { onMounted, reactive, ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus } from '@element-plus/icons-vue'
import PageHeader from '../components/PageHeader.vue'
import {
  getAnnouncements,
  createAnnouncement,
  updateAnnouncement,
  toggleAnnouncementPublish,
  deleteAnnouncement,
  getSiteRuntime,
  updateSiteRuntime,
} from '../api/Admin'

const loading = ref(false)
const saving = ref(false)
const runtimeSaving = ref(false)
const announcements = ref([])
const dialogVisible = ref(false)
const editingId = ref(null)
const formRef = ref(null)
const runtimeUpdatedAt = ref(null)

const runtimeForm = reactive({
  status: 'Normal',
  statusText: '网站运行正常',
  extraInfo: '',
})

const today = () => {
  const d = new Date()
  const m = String(d.getMonth() + 1).padStart(2, '0')
  const day = String(d.getDate()).padStart(2, '0')
  return `${d.getFullYear()}-${m}-${day}`
}

const form = reactive({
  title: '',
  content: '',
  publishDate: today(),
  isPinned: false,
  isPublished: true,
  sortOrder: 0,
})

const rules = {
  title: [{ required: true, message: '请输入标题', trigger: 'blur' }],
  content: [{ required: true, message: '请输入内容', trigger: 'blur' }],
  publishDate: [{ required: true, message: '请选择日期', trigger: 'change' }],
}

const formatTime = (value) => {
  if (!value) return '—'
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return String(value)
  const pad = (n) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`
}

const loadRuntime = async () => {
  const res = await getSiteRuntime()
  runtimeForm.status = res.status || 'Normal'
  runtimeForm.statusText = res.statusText || ''
  runtimeForm.extraInfo = res.extraInfo || ''
  runtimeUpdatedAt.value = res.updatedAt
}

const loadAnnouncements = async () => {
  loading.value = true
  try {
    announcements.value = (await getAnnouncements()) || []
  } finally {
    loading.value = false
  }
}

const saveRuntime = async () => {
  runtimeSaving.value = true
  try {
    const res = await updateSiteRuntime({
      status: runtimeForm.status,
      statusText: runtimeForm.statusText,
      extraInfo: runtimeForm.extraInfo,
    })
    runtimeUpdatedAt.value = res.updatedAt
    ElMessage.success('运行状态已保存')
  } finally {
    runtimeSaving.value = false
  }
}

const resetForm = () => {
  editingId.value = null
  form.title = ''
  form.content = ''
  form.publishDate = today()
  form.isPinned = false
  form.isPublished = true
  form.sortOrder = 0
}

const openCreate = () => {
  resetForm()
  dialogVisible.value = true
}

const openEdit = (row) => {
  editingId.value = row.id
  form.title = row.title
  form.content = row.content
  form.publishDate = row.publishDate
  form.isPinned = !!row.isPinned
  form.isPublished = !!row.isPublished
  form.sortOrder = row.sortOrder ?? 0
  dialogVisible.value = true
}

const submitForm = async () => {
  await formRef.value?.validate()
  saving.value = true
  try {
    const payload = {
      title: form.title,
      content: form.content,
      publishDate: form.publishDate,
      isPinned: form.isPinned,
      isPublished: form.isPublished,
      sortOrder: form.sortOrder,
    }
    if (editingId.value) {
      await updateAnnouncement({ ...payload, id: editingId.value })
      ElMessage.success('公告已更新')
    } else {
      await createAnnouncement(payload)
      ElMessage.success('公告已发布')
    }
    dialogVisible.value = false
    await loadAnnouncements()
  } finally {
    saving.value = false
  }
}

const togglePublish = async (row) => {
  await toggleAnnouncementPublish(row.id)
  ElMessage.success(row.isPublished ? '已取消发布' : '已发布')
  await loadAnnouncements()
}

const handleDelete = async (row) => {
  await ElMessageBox.confirm(`确定删除公告「${row.title}」？`, '删除确认', { type: 'warning' })
  await deleteAnnouncement(row.id)
  ElMessage.success('已删除')
  await loadAnnouncements()
}

onMounted(async () => {
  await Promise.all([loadRuntime(), loadAnnouncements()])
})
</script>

<style scoped>
.runtime-form {
  max-width: 100%;
}

.runtime-meta {
  margin: 0;
  font-size: 12px;
  color: var(--el-text-color-secondary);
}

.muted {
  color: var(--el-text-color-placeholder);
}
</style>
