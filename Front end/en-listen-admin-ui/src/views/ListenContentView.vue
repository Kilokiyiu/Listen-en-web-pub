<template>
  <div class="admin-page">
    <PageHeader
      title="听力内容"
      description="统一管理 CET 听力试卷：上传音频、原文、PDF、分段在线做题与可见性"
    >
      <template #extra>
        <el-button type="primary" @click="uploadVisible = true">
          <el-icon><Plus /></el-icon>
          新增试卷
        </el-button>
      </template>
    </PageHeader>

    <div class="filter-bar">
      <el-radio-group v-model="categoryFilter" size="default">
        <el-radio-button label="all">全部</el-radio-button>
        <el-radio-button label="CET4">四级</el-radio-button>
        <el-radio-button label="CET6">六级</el-radio-button>
      </el-radio-group>
      <el-input
        v-model="keyword"
        clearable
        placeholder="搜索试卷名称"
        class="search-input"
      >
        <template #prefix>
          <el-icon><Search /></el-icon>
        </template>
      </el-input>
      <div class="filter-stats">
        <span>{{ filteredAlbums.length }} 套</span>
        <span>在线题 {{ stats.withQuiz }}</span>
        <span>缺 PDF {{ stats.missingPaper }}</span>
      </div>
      <el-button text type="primary" :loading="loading" @click="loadAlbums">
        <el-icon><Refresh /></el-icon>
        刷新
      </el-button>
    </div>

    <div class="admin-card admin-table-card">
      <div class="admin-card__body table-body">
        <el-table
          v-loading="loading"
          :data="filteredAlbums"
          stripe
          row-key="id"
          class="listen-table"
        >
          <el-table-column label="年份" width="100" align="center">
            <template #default="{ row }">
              <span class="year-cell">{{ parseAlbumMeta(row).year || '-' }}</span>
            </template>
          </el-table-column>
          <el-table-column label="试卷" width="280" show-overflow-tooltip>
            <template #default="{ row }">
              <span :title="row.nameChinese">{{ shortPaperName(row) }}</span>
            </template>
          </el-table-column>
          <el-table-column label="分类" width="100" align="center">
            <template #default="{ row }">
              <el-tag size="small" effect="plain">{{ categoryShort(row) }}</el-tag>
            </template>
          </el-table-column>
          <el-table-column label="就绪" width="220" align="center">
            <template #default="{ row }">
              <div class="ready-tags" :title="readyTooltip(row)">
                <span class="dot" :class="row.hasSubtitle ? 'on' : ''">原文</span>
                <span class="dot" :class="row.hasPaper ? 'on' : ''">试卷</span>
                <span class="dot" :class="row.hasAnswer ? 'on' : ''">答案</span>
                <span class="dot" :class="(row.quizQuestionCount || 0) > 0 ? 'on warn' : ''">
                  题{{ row.quizQuestionCount || 0 }}
                </span>
              </div>
            </template>
          </el-table-column>
          <el-table-column label="可见" width="100" align="center">
            <template #default="{ row }">
              <el-switch
                :model-value="row.isVisible"
                :loading="row.toggling"
                @change="handleToggle(row)"
              />
            </template>
          </el-table-column>
          <el-table-column label="操作" min-width="220" align="center">
            <template #default="{ row }">
              <div class="ops">
                <el-button type="warning" link size="small" @click="openQuizEditor(row)">
                  在线做题
                </el-button>
                <el-dropdown trigger="click" @command="(cmd) => onMore(cmd, row)">
                  <el-button type="primary" link size="small">
                    更多
                    <el-icon class="el-icon--right"><ArrowDown /></el-icon>
                  </el-button>
                  <template #dropdown>
                    <el-dropdown-menu>
                      <el-dropdown-item command="subtitle">
                        {{ row.hasSubtitle ? '编辑原文' : '添加原文' }}
                      </el-dropdown-item>
                      <el-dropdown-item command="paper">上传试卷 PDF</el-dropdown-item>
                      <el-dropdown-item command="answer">上传答案 PDF</el-dropdown-item>
                      <el-dropdown-item divided command="delete" class="danger-item">
                        删除试卷
                      </el-dropdown-item>
                    </el-dropdown-menu>
                  </template>
                </el-dropdown>
              </div>
            </template>
          </el-table-column>
        </el-table>
      </div>
    </div>

    <!-- 新增试卷 -->
    <el-dialog v-model="uploadVisible" title="新增听力试卷" width="640px" destroy-on-close>
      <el-form ref="formRef" :model="form" :rules="rules" label-position="top">
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="类别" prop="category">
              <el-select v-model="form.category" style="width: 100%">
                <el-option label="大学英语四级 (CET4)" value="CET4" />
                <el-option label="大学英语六级 (CET6)" value="CET6" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="年份" prop="year">
              <el-input-number v-model="form.year" :min="2000" :max="2030" style="width: 100%" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="月份" prop="month">
              <el-select v-model="form.month" style="width: 100%">
                <el-option label="6月" :value="6" />
                <el-option label="12月" :value="12" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="第几套" prop="setNumber">
              <el-select v-model="form.setNumber" style="width: 100%">
                <el-option label="第1套" :value="1" />
                <el-option label="第2套" :value="2" />
                <el-option label="第3套" :value="3" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="整卷音频" required>
          <el-upload
            ref="uploadRef"
            :auto-upload="false"
            :limit="1"
            accept=".mp3,.wav,.m4a"
            drag
            :on-change="handleFileChange"
            :on-remove="handleFileRemove"
          >
            <el-icon class="upload-icon"><UploadFilled /></el-icon>
            <div class="el-upload__text">拖拽或点击上传 mp3 / wav / m4a</div>
          </el-upload>
        </el-form-item>
        <el-form-item label="整卷原文（可选，字幕 JSON）">
          <el-input v-model="form.subtitle" type="textarea" :rows="4" placeholder='[{"start":0,"end":3,"text":"..."}]' />
          <p class="form-tip">分段做题原文请在保存后于「在线做题」中按 Section 录入。</p>
        </el-form-item>
        <el-alert
          :title="previewName"
          type="info"
          :closable="false"
          show-icon
        />
      </el-form>
      <template #footer>
        <el-button @click="uploadVisible = false">取消</el-button>
        <el-button type="primary" :loading="uploading" @click="handleUpload">上传并创建</el-button>
      </template>
    </el-dialog>

    <!-- 原文 -->
    <el-dialog
      v-model="subtitleVisible"
      :title="currentAlbum?.hasSubtitle ? '编辑原文' : '添加原文'"
      width="700px"
      destroy-on-close
    >
      <p class="dialog-info">{{ currentAlbum?.nameChinese }}</p>
      <el-input v-model="subtitleContent" type="textarea" :rows="14" />
      <template #footer>
        <el-button @click="subtitleVisible = false">取消</el-button>
        <el-button type="primary" :loading="saving" @click="saveSubtitle">保存</el-button>
      </template>
    </el-dialog>

    <ListenQuizEditDialog
      v-model="quizDialogVisible"
      :album-id="quizAlbumId"
      :album-name="quizAlbumName"
      @saved="loadAlbums"
    />

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
import { computed, onMounted, reactive, ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  getAllAlbums,
  updateEpisodeSubtitle,
  toggleAlbumVisibility,
  deleteEpisode,
  uploadAlbumDocument,
  uploadAudio,
} from '../api/Admin'
import PageHeader from '../components/PageHeader.vue'
import ListenQuizEditDialog from './ListenQuizEditDialog.vue'

const loading = ref(false)
const albums = ref([])
const categoryFilter = ref('all')
const keyword = ref('')

const uploadVisible = ref(false)
const uploading = ref(false)
const formRef = ref(null)
const uploadRef = ref(null)
const form = reactive({
  category: 'CET6',
  year: new Date().getFullYear(),
  month: 6,
  setNumber: 1,
  file: null,
  subtitle: '',
})
const rules = {
  category: [{ required: true, message: '请选择类别', trigger: 'change' }],
  year: [{ required: true, message: '请输入年份', trigger: 'blur' }],
  month: [{ required: true, message: '请选择月份', trigger: 'change' }],
  setNumber: [{ required: true, message: '请选择套号', trigger: 'change' }],
}

const subtitleVisible = ref(false)
const currentAlbum = ref(null)
const subtitleContent = ref('')
const saving = ref(false)

const quizDialogVisible = ref(false)
const quizAlbumId = ref('')
const quizAlbumName = ref('')

const pdfInputRef = ref(null)
const pendingUpload = ref({ albumId: null, documentType: null })

const categoryShort = (row) => {
  const name = (row.categoryNameChinese || row.categoryNameEnglish || '').toString()
  if (name.includes('四') || /CET-?4/i.test(name)) return 'CET4'
  if (name.includes('六') || /CET-?6/i.test(name)) return 'CET6'
  return name.slice(0, 6) || '-'
}

/** 从「2025年6月…第2套」解析排序字段 */
const parseAlbumMeta = (row) => {
  const name = row.nameChinese || ''
  const year = Number((name.match(/(\d{4})\s*年/) || [])[1]) || 0
  const month = Number((name.match(/(\d{1,2})\s*月/) || [])[1]) || 0
  const setNumber = Number((name.match(/第\s*(\d+)\s*套/) || [])[1]) || 0
  return { year, month, setNumber }
}

/** 列表展示用短名：去掉冗长前缀，保留月/套 */
const shortPaperName = (row) => {
  const name = row.nameChinese || ''
  const meta = parseAlbumMeta(row)
  if (!meta.year) return name
  const setPart = meta.setNumber ? `第${meta.setNumber}套` : ''
  const monthPart = meta.month ? `${meta.month}月` : ''
  const level = categoryShort(row) === 'CET4' ? '四级' : categoryShort(row) === 'CET6' ? '六级' : ''
  return [monthPart, level, setPart].filter(Boolean).join(' ') || name
}

const readyTooltip = (row) =>
  [
    `原文 ${row.hasSubtitle ? '有' : '无'}`,
    `试卷PDF ${row.hasPaper ? '有' : '无'}`,
    `答案PDF ${row.hasAnswer ? '有' : '无'}`,
    `在线题 ${row.quizQuestionCount || 0}`,
  ].join(' · ')

const filteredAlbums = computed(() => {
  let list = [...(albums.value || [])]
  if (categoryFilter.value !== 'all') {
    list = list.filter((a) => categoryShort(a) === categoryFilter.value)
  }
  const kw = keyword.value.trim().toLowerCase()
  if (kw) {
    list = list.filter((a) => (a.nameChinese || '').toLowerCase().includes(kw))
  }
  // 年份降序 → 月份降序 → 套号升序 → 分类
  list.sort((a, b) => {
    const ma = parseAlbumMeta(a)
    const mb = parseAlbumMeta(b)
    if (mb.year !== ma.year) return mb.year - ma.year
    if (mb.month !== ma.month) return mb.month - ma.month
    if (ma.setNumber !== mb.setNumber) return ma.setNumber - mb.setNumber
    return categoryShort(a).localeCompare(categoryShort(b))
  })
  return list
})

const stats = computed(() => {
  const list = filteredAlbums.value
  return {
    withQuiz: list.filter((a) => (a.quizQuestionCount || 0) > 0).length,
    missingPaper: list.filter((a) => !a.hasPaper).length,
  }
})

const previewName = computed(
  () =>
    `${form.year}年${form.month}月大学英语${form.category === 'CET4' ? '四级' : '六级'}听力真题（第${form.setNumber}套）`
)

const loadAlbums = async () => {
  loading.value = true
  try {
    albums.value = (await getAllAlbums()) || []
  } catch {
    albums.value = []
  } finally {
    loading.value = false
  }
}

const handleFileChange = (file) => {
  form.file = file.raw
}
const handleFileRemove = () => {
  form.file = null
}

const handleUpload = async () => {
  try {
    await formRef.value?.validate()
  } catch {
    return
  }
  if (!form.file) {
    ElMessage.warning('请选择音频文件')
    return
  }
  uploading.value = true
  try {
    const fd = new FormData()
    fd.append('categoryParam', form.category)
    fd.append('year', form.year)
    fd.append('month', form.month)
    fd.append('setNumber', form.setNumber)
    fd.append('file', form.file)
    if (form.subtitle.trim()) fd.append('subtitle', form.subtitle.trim())
    await uploadAudio(fd)
    ElMessage.success('上传成功')
    uploadVisible.value = false
    uploadRef.value?.clearFiles()
    form.file = null
    form.subtitle = ''
    await loadAlbums()
  } catch {
    /* */
  } finally {
    uploading.value = false
  }
}

const openQuizEditor = (row) => {
  quizAlbumId.value = row.id
  quizAlbumName.value = row.nameChinese || ''
  quizDialogVisible.value = true
}

const openSubtitle = (row) => {
  currentAlbum.value = row
  subtitleContent.value = row.subtitle || ''
  subtitleVisible.value = true
}

const saveSubtitle = async () => {
  if (!subtitleContent.value.trim()) {
    try {
      await ElMessageBox.confirm('原文内容为空，确定要保存吗？', '提示', { type: 'warning' })
    } catch {
      return
    }
  }
  saving.value = true
  try {
    await updateEpisodeSubtitle({
      episodeId: currentAlbum.value.firstEpisodeId,
      subtitle: subtitleContent.value.trim(),
      subtitleType: 'json',
    })
    ElMessage.success('保存成功')
    subtitleVisible.value = false
    await loadAlbums()
  } catch {
    /* */
  } finally {
    saving.value = false
  }
}

const triggerPdf = (row, documentType) => {
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
  } catch {
    /* */
  }
}

const handleToggle = async (row) => {
  row.toggling = true
  try {
    await toggleAlbumVisibility(row.id)
    row.isVisible = !row.isVisible
    ElMessage.success(row.isVisible ? '已显示' : '已隐藏')
  } catch {
    /* */
  } finally {
    row.toggling = false
  }
}

const handleDelete = async (row) => {
  try {
    await ElMessageBox.confirm(
      `确定删除「${row.nameChinese}」？将同时删除关联音频，不可恢复。`,
      '删除确认',
      { type: 'warning', confirmButtonText: '删除' }
    )
  } catch {
    return
  }
  try {
    await deleteEpisode(row.firstEpisodeId)
    ElMessage.success('删除成功')
    await loadAlbums()
  } catch {
    /* */
  }
}

const onMore = (cmd, row) => {
  if (cmd === 'subtitle') openSubtitle(row)
  else if (cmd === 'paper') triggerPdf(row, 'paper')
  else if (cmd === 'answer') triggerPdf(row, 'answer')
  else if (cmd === 'delete') handleDelete(row)
}

onMounted(loadAlbums)
</script>

<style scoped>
.filter-bar {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;
}

.search-input {
  width: 220px;
}

.filter-stats {
  display: flex;
  gap: 12px;
  margin-left: auto;
  font-size: 13px;
  color: var(--admin-text-secondary, #909399);
}

.table-body {
  padding: 0;
}

.listen-table :deep(.el-table__cell) {
  padding-left: 14px;
  padding-right: 14px;
}

.year-cell {
  display: inline-block;
  min-width: 3em;
  font-weight: 650;
  font-variant-numeric: tabular-nums;
  color: var(--admin-text, #303133);
}

.ready-tags {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  flex-wrap: nowrap;
}

.dot {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 36px;
  height: 24px;
  padding: 0 6px;
  border-radius: 6px;
  font-size: 12px;
  font-weight: 600;
  color: #909399;
  background: #f0f2f5;
  white-space: nowrap;
}

.dot.on {
  color: #067647;
  background: #ecfdf3;
}

.dot.on.warn {
  color: #b54708;
  background: #fffaeb;
}

.ops {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
}

.upload-icon {
  font-size: 40px;
  color: var(--admin-primary);
  margin-bottom: 6px;
}

.form-tip {
  margin: 6px 0 0;
  font-size: 12px;
  color: #909399;
}

.dialog-info {
  margin: 0 0 12px;
  padding: 10px 12px;
  background: #f5f7fa;
  border-radius: 8px;
  color: #606266;
  font-size: 14px;
}

.hidden-file-input {
  display: none;
}

:deep(.danger-item) {
  color: #f56c6c;
}
</style>
