<template>
  <div class="admin-page">
    <PageHeader
      title="内容管理"
      description="管理听力试卷、原文、PDF 文档及可见性"
    >
      <template #extra>
        <el-button type="primary" @click="router.push('/upload')">
          <el-icon><Upload /></el-icon>
          去上传音频
        </el-button>
      </template>
    </PageHeader>

    <div class="admin-stat-grid">
      <div class="admin-stat-card">
        <div>
          <div class="admin-stat-card__label">试卷总数</div>
          <div class="admin-stat-card__value">{{ stats.total }}</div>
        </div>
        <div class="admin-stat-card__icon admin-stat-card__icon--blue">
          <el-icon><FolderOpened /></el-icon>
        </div>
      </div>
      <div class="admin-stat-card">
        <div>
          <div class="admin-stat-card__label">已有原文</div>
          <div class="admin-stat-card__value">{{ stats.withSubtitle }}</div>
        </div>
        <div class="admin-stat-card__icon admin-stat-card__icon--green">
          <el-icon><Document /></el-icon>
        </div>
      </div>
      <div class="admin-stat-card">
        <div>
          <div class="admin-stat-card__label">已上传 PDF</div>
          <div class="admin-stat-card__value">{{ stats.withPaper }}</div>
        </div>
        <div class="admin-stat-card__icon admin-stat-card__icon--orange">
          <el-icon><Files /></el-icon>
        </div>
      </div>
      <div class="admin-stat-card">
        <div>
          <div class="admin-stat-card__label">已隐藏</div>
          <div class="admin-stat-card__value">{{ stats.hidden }}</div>
        </div>
        <div class="admin-stat-card__icon admin-stat-card__icon--purple">
          <el-icon><Hide /></el-icon>
        </div>
      </div>
    </div>

    <div class="admin-card admin-table-card">
      <div class="admin-card__header">
        <span class="admin-card__title">试卷管理</span>
        <el-button text type="primary" @click="loadAlbums" :loading="loading">
          <el-icon><Refresh /></el-icon>
          刷新
        </el-button>
      </div>
      <div class="admin-card__body table-body">
        <el-table
          v-loading="loading"
          :data="albums"
          style="width: 100%"
          row-key="id"
          stripe
        >
          <el-table-column prop="nameChinese" label="试卷名称" min-width="280" show-overflow-tooltip />
          <el-table-column prop="categoryNameChinese" label="分类" width="120" />
          <el-table-column prop="episodeCount" label="题目数量" width="90" align="center" />
          <el-table-column label="是否有原文" width="100" align="center">
            <template #default="{ row }">
              <el-tag :type="row.hasSubtitle ? 'success' : 'info'" size="small" effect="light">
                {{ row.hasSubtitle ? '有' : '无' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="试卷 PDF" width="110" align="center">
            <template #default="{ row }">
              <el-tag :type="row.hasPaper ? 'success' : 'info'" size="small" effect="light">
                {{ row.hasPaper ? '已上传' : '未上传' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="答案 PDF" width="110" align="center">
            <template #default="{ row }">
              <el-tag :type="row.hasAnswer ? 'success' : 'info'" size="small" effect="light">
                {{ row.hasAnswer ? '已上传' : '未上传' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="隐藏" width="70" align="center">
            <template #default="{ row }">
              <el-switch
                :model-value="!row.isVisible"
                @change="handleToggle(row)"
                :loading="row.toggling"
              />
            </template>
          </el-table-column>
          <el-table-column label="创建时间" width="170">
            <template #default="{ row }">
              {{ formatDate(row.creationTime) }}
            </template>
          </el-table-column>
          <el-table-column label="操作" width="320" align="center" fixed="right">
            <template #default="{ row }">
              <el-button type="primary" link size="small" @click="openEdit(row)">
                {{ row.hasSubtitle ? '编辑原文' : '添加原文' }}
              </el-button>
              <el-button type="primary" link size="small" @click="triggerUpload(row, 'paper')">
                上传试卷
              </el-button>
              <el-button type="success" link size="small" @click="triggerUpload(row, 'answer')">
                上传答案
              </el-button>
              <el-button type="danger" link size="small" @click="handleDelete(row)">
                删除
              </el-button>
            </template>
          </el-table-column>
        </el-table>
      </div>
    </div>

    <el-dialog
      v-model="dialogVisible"
      :title="currentAlbum?.hasSubtitle ? '编辑原文' : '添加原文'"
      width="700px"
      destroy-on-close
    >
      <div class="dialog-subtitle">
        <p class="dialog-info">
          <el-icon><InfoFilled /></el-icon>
          <span>{{ currentAlbum?.nameChinese || '' }}</span>
        </p>
      </div>

      <el-input
        v-model="subtitleContent"
        type="textarea"
        :rows="15"
        placeholder="请粘贴字幕JSON内容&#10;格式示例：&#10;[&#10;  {&#10;    &quot;start&quot;: 0,&#10;    &quot;end&quot;: 3,&#10;    &quot;text&quot;: &quot;Hello, how are you?&quot;,&#10;    &quot;translation&quot;: &quot;&quot;&#10;  }&#10;]"
      />

      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="saveSubtitle" :loading="saving">
          保存
        </el-button>
      </template>
    </el-dialog>

    <input
      ref="pdfInputRef"
      type="file"
      accept=".pdf,application/pdf"
      class="hidden-file-input"
      @change="handlePdfSelected"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import {
  getAllAlbums,
  updateEpisodeSubtitle,
  toggleAlbumVisibility,
  deleteEpisode,
  uploadAlbumDocument
} from '../api/Admin'
import { ElMessage, ElMessageBox } from 'element-plus'
import PageHeader from '../components/PageHeader.vue'

const router = useRouter()

const loading = ref(false)
const albums = ref([])
const dialogVisible = ref(false)
const currentAlbum = ref(null)
const subtitleContent = ref('')
const saving = ref(false)
const pdfInputRef = ref(null)
const pendingUpload = ref({ albumId: null, documentType: null })

const stats = computed(() => ({
  total: albums.value.length,
  withSubtitle: albums.value.filter(a => a.hasSubtitle).length,
  withPaper: albums.value.filter(a => a.hasPaper).length,
  hidden: albums.value.filter(a => !a.isVisible).length,
}))

const triggerUpload = (row, documentType) => {
  pendingUpload.value = { albumId: row.id, documentType }
  pdfInputRef.value?.click()
}

const handlePdfSelected = async (event) => {
  const file = event.target.files?.[0]
  event.target.value = ''
  const { albumId, documentType } = pendingUpload.value
  if (!file || !albumId || !documentType) return

  if (!file.name.toLowerCase().endsWith('.pdf')) {
    ElMessage.warning('请选择 PDF 文件')
    return
  }

  try {
    await uploadAlbumDocument(albumId, documentType, file)
    ElMessage.success(documentType === 'paper' ? '试卷上传成功' : '答案上传成功')
    await loadAlbums()
  } catch (e) {
    // 错误已在拦截器中处理
  }
}

const loadAlbums = async () => {
  loading.value = true
  try {
    const data = await getAllAlbums()
    albums.value = data || []
  } catch (e) {
    console.error('获取试卷列表失败', e)
  } finally {
    loading.value = false
  }
}

const openEdit = async (row) => {
  currentAlbum.value = row
  subtitleContent.value = row.subtitle || ''
  dialogVisible.value = true
}

const saveSubtitle = async () => {
  if (!subtitleContent.value.trim()) {
    try {
      await ElMessageBox.confirm('原文内容为空，确定要保存吗？', '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      })
    } catch {
      return
    }
  }

  saving.value = true
  try {
    await updateEpisodeSubtitle({
      episodeId: currentAlbum.value.firstEpisodeId,
      subtitle: subtitleContent.value.trim(),
      subtitleType: 'json'
    })
    ElMessage.success('保存成功')
    dialogVisible.value = false
    await loadAlbums()
  } catch (e) {
    // 错误已在拦截器中处理
  } finally {
    saving.value = false
  }
}

const handleToggle = async (row) => {
  row.toggling = true
  try {
    await toggleAlbumVisibility(row.id)
    row.isVisible = !row.isVisible
    ElMessage.success(row.isVisible ? '已显示' : '已隐藏')
  } catch (e) {
    // 错误已在拦截器中处理
  } finally {
    row.toggling = false
  }
}

const handleDelete = async (row) => {
  try {
    await ElMessageBox.confirm(
      `确定要删除试卷「${row.nameChinese}」吗？该操作不可恢复，且会同时删除关联的音频文件。`,
      '删除确认',
      { confirmButtonText: '删除', cancelButtonText: '取消', type: 'warning' }
    )
  } catch {
    return
  }
  try {
    await deleteEpisode(row.firstEpisodeId)
    ElMessage.success('删除成功')
    await loadAlbums()
  } catch (e) {
    // 错误已在拦截器中处理
  }
}

const formatDate = (dateStr) => {
  if (!dateStr) return '-'
  const d = new Date(dateStr)
  return d.toLocaleString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

onMounted(() => {
  loadAlbums()
})
</script>

<style scoped>
.table-body {
  padding: 0;
}

.table-body :deep(.el-table) {
  border-radius: 0 0 var(--admin-radius) var(--admin-radius);
}

.dialog-subtitle {
  margin-bottom: 16px;
}

.dialog-info {
  display: flex;
  align-items: center;
  gap: 8px;
  margin: 0;
  padding: 10px 14px;
  background: #f5f7fa;
  border-radius: 8px;
  color: #606266;
  font-size: 14px;
}

.dialog-info .el-icon {
  color: var(--admin-primary);
}

.hidden-file-input {
  display: none;
}
</style>
