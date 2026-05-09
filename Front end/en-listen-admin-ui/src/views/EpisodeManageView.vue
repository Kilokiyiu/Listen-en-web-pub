<template>
  <div class="manage-container">
    <div class="manage-header">
      <h2>内容管理</h2>
      <el-button type="primary" @click="router.push('/')">
        去上传音频
      </el-button>
    </div>

    <!-- ========== 试卷管理 ========== -->
    <el-card class="manage-card" shadow="never">
      <template #header>
        <div class="card-header">
          <span class="card-title">试卷管理</span>
        </div>
      </template>
      <el-table
        v-loading="loading"
        :data="albums"
        style="width: 100%"
        row-key="id"
        border
      >
        <el-table-column prop="nameChinese" label="试卷名称" min-width="280" show-overflow-tooltip />
        <el-table-column prop="categoryNameChinese" label="分类" width="120" />
        <el-table-column prop="episodeCount" label="题目数量" width="90" align="center" />
        <el-table-column label="是否有原文" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="row.hasSubtitle ? 'success' : 'info'" size="small">
              {{ row.hasSubtitle ? '有' : '无' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="隐藏" width="70" align="center">
          <template #default="{ row }">
            <el-switch
              :model-value="!row.isVisible"
              active-text=""
              inactive-text=""
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
        <el-table-column label="操作" width="200" align="center" fixed="right">
          <template #default="{ row }">
            <el-button type="primary" link size="small" @click="openEdit(row)">
              {{ row.hasSubtitle ? '编辑原文' : '添加原文' }}
            </el-button>
            <el-button type="danger" link size="small" @click="handleDelete(row)">
              删除
            </el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 编辑原文弹窗 -->
    <el-dialog
      v-model="dialogVisible"
      :title="currentAlbum?.hasSubtitle ? '编辑原文' : '添加原文'"
      width="700px"
      destroy-on-close
    >
      <div class="dialog-subtitle">
        <p class="dialog-info">
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
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getAllAlbums, updateEpisodeSubtitle, toggleAlbumVisibility, deleteEpisode } from '../api/Admin'
import { ElMessage, ElMessageBox } from 'element-plus'

const router = useRouter()

// ========== 数据 ==========
const loading = ref(false)
const albums = ref([])
const dialogVisible = ref(false)
const currentAlbum = ref(null)
const subtitleContent = ref('')
const saving = ref(false)

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
    // 通过 albumId 找到第一个 episode 来更新原文
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
.manage-container {
  padding: 32px;
  max-width: 1200px;
  margin: 0 auto;
}

.manage-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.manage-header h2 {
  margin: 0;
  color: #1a1a2e;
  font-size: 22px;
}

.manage-card {
  border-radius: 12px;
  margin-bottom: 24px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.card-title {
  font-weight: 600;
  font-size: 15px;
  color: #303133;
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
  color: #409eff;
}
</style>
