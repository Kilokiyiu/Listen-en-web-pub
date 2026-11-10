<template>
  <div class="admin-page">
    <PageHeader
      title="考研内容"
      description="英语一 / 英语二试卷管理 · 原题/解析 PDF · 题目编辑与可见性"
    >
      <template #extra>
        <el-button type="primary" @click="openCreate">
          <el-icon><Plus /></el-icon>
          新增试卷
        </el-button>
      </template>
    </PageHeader>

    <div class="admin-card admin-table-card">
      <div class="admin-card__header">
        <span class="admin-card__title">试卷列表（{{ sortedPapers.length }}）</span>
        <el-button text type="primary" :loading="loading" @click="load">刷新</el-button>
      </div>
      <div class="admin-card__body table-body">
        <el-table v-loading="loading" :data="sortedPapers" stripe row-key="id" class="ky-table">
          <el-table-column prop="year" label="年份" width="90" align="center" />
          <el-table-column prop="seriesName" label="科目" width="90" align="center" />
          <el-table-column prop="title" label="试卷名称" min-width="220" show-overflow-tooltip />
          <el-table-column prop="questionCount" label="题数" width="80" align="center" />
          <el-table-column label="PDF" width="120" align="center">
            <template #default="{ row }">
              <span class="pdf-flags">
                <el-tag size="small" :type="row.hasPaper ? 'success' : 'info'" effect="plain">原题</el-tag>
                <el-tag size="small" :type="row.hasAnswer ? 'success' : 'info'" effect="plain">解析</el-tag>
              </span>
            </template>
          </el-table-column>
          <el-table-column label="可见" width="90" align="center">
            <template #default="{ row }">
              <el-switch
                :model-value="row.isVisible"
                :loading="row.toggling"
                @change="toggle(row)"
              />
            </template>
          </el-table-column>
          <el-table-column label="操作" width="260" align="center" fixed="right">
            <template #default="{ row }">
              <el-button type="primary" link size="small" @click="openEdit(row)">编辑题目</el-button>
              <el-dropdown trigger="click" @command="(cmd) => onMore(cmd, row)">
                <el-button type="primary" link size="small">
                  更多
                  <el-icon class="el-icon--right"><ArrowDown /></el-icon>
                </el-button>
                <template #dropdown>
                  <el-dropdown-menu>
                    <el-dropdown-item command="paper">上传原题 PDF</el-dropdown-item>
                    <el-dropdown-item command="answer">上传解析 PDF</el-dropdown-item>
                    <el-dropdown-item divided command="delete" class="danger-item">
                      删除试卷
                    </el-dropdown-item>
                  </el-dropdown-menu>
                </template>
              </el-dropdown>
            </template>
          </el-table-column>
        </el-table>
      </div>
    </div>

    <el-dialog v-model="createVisible" title="新增试卷" width="480px" destroy-on-close>
      <el-form label-position="top">
        <el-form-item label="年份" required>
          <el-input-number v-model="createForm.year" :min="2000" :max="2100" style="width: 100%" />
        </el-form-item>
        <el-form-item label="科目" required>
          <el-radio-group v-model="createForm.series">
            <el-radio-button value="eng1">英语一</el-radio-button>
            <el-radio-button value="eng2">英语二</el-radio-button>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="标题（可选）">
          <el-input v-model="createForm.title" placeholder="默认：XXXX年考研英语一" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="createVisible = false">取消</el-button>
        <el-button type="primary" :loading="creating" @click="doCreate">创建</el-button>
      </template>
    </el-dialog>

    <input
      ref="pdfInputRef"
      type="file"
      accept=".pdf,application/pdf"
      class="hidden-input"
      @change="handlePdfSelected"
    />

    <KaoyanPaperEditDialog
      v-model="editVisible"
      :paper-id="editPaperId"
      :paper-title="editPaperTitle"
      @saved="load"
    />
  </div>
</template>

<script setup>
import { computed, onMounted, reactive, ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { ArrowDown, Plus } from '@element-plus/icons-vue'
import PageHeader from '../components/PageHeader.vue'
import KaoyanPaperEditDialog from './KaoyanPaperEditDialog.vue'
import {
  getKaoyanAllPapers,
  toggleKaoyanPaperVisibility,
  createKaoyanPaper,
  deleteKaoyanPaper,
  uploadKaoyanPaperDocument,
} from '../api/Admin'

const loading = ref(false)
const papers = ref([])
const editVisible = ref(false)
const editPaperId = ref('')
const editPaperTitle = ref('')
const createVisible = ref(false)
const creating = ref(false)
const pdfInputRef = ref(null)
const pendingUpload = ref({ paperId: null, documentType: null })
const createForm = reactive({
  year: new Date().getFullYear(),
  series: 'eng1',
  title: '',
})

const sortedPapers = computed(() =>
  [...(papers.value || [])].sort((a, b) => {
    const ya = a.year ?? 0
    const yb = b.year ?? 0
    if (yb !== ya) return yb - ya
    const sa = a.series === 'eng1' ? 0 : 1
    const sb = b.series === 'eng1' ? 0 : 1
    return sa - sb
  })
)

const load = async () => {
  loading.value = true
  try {
    papers.value = (await getKaoyanAllPapers()) || []
  } catch {
    papers.value = []
  } finally {
    loading.value = false
  }
}

const toggle = async (row) => {
  row.toggling = true
  try {
    await toggleKaoyanPaperVisibility(row.id)
    row.isVisible = !row.isVisible
    ElMessage.success(row.isVisible ? '已显示' : '已隐藏')
  } catch {
    /* */
  } finally {
    row.toggling = false
  }
}

const openEdit = (row) => {
  editPaperId.value = row.id
  editPaperTitle.value = row.title || `${row.year} ${row.seriesName}`
  editVisible.value = true
}

const openCreate = () => {
  createForm.year = new Date().getFullYear()
  createForm.series = 'eng1'
  createForm.title = ''
  createVisible.value = true
}

const doCreate = async () => {
  creating.value = true
  try {
    await createKaoyanPaper({
      year: createForm.year,
      series: createForm.series,
      title: createForm.title || undefined,
    })
    ElMessage.success('已创建，可继续编辑题目')
    createVisible.value = false
    await load()
  } catch {
    /* */
  } finally {
    creating.value = false
  }
}

const triggerPdf = (row, documentType) => {
  pendingUpload.value = { paperId: row.id, documentType }
  pdfInputRef.value?.click()
}

const handlePdfSelected = async (event) => {
  const file = event.target.files?.[0]
  const { paperId, documentType } = pendingUpload.value
  event.target.value = ''
  if (!file || !paperId || !documentType) return
  if (!file.name.toLowerCase().endsWith('.pdf')) {
    ElMessage.warning('请选择 PDF 文件')
    return
  }
  try {
    await uploadKaoyanPaperDocument(paperId, documentType, file)
    ElMessage.success(documentType === 'paper' ? '原题上传成功' : '解析上传成功')
    await load()
  } catch {
    /* */
  }
}

const onMore = (cmd, row) => {
  if (cmd === 'paper') triggerPdf(row, 'paper')
  else if (cmd === 'answer') triggerPdf(row, 'answer')
  else if (cmd === 'delete') remove(row)
}

const remove = async (row) => {
  try {
    await ElMessageBox.confirm(
      `确定删除「${row.title}」？分区与题目将一并删除，不可恢复。`,
      '删除确认',
      { type: 'warning', confirmButtonText: '删除' }
    )
  } catch {
    return
  }
  try {
    await deleteKaoyanPaper(row.id)
    ElMessage.success('已删除')
    await load()
  } catch {
    /* */
  }
}

onMounted(load)
</script>

<style scoped>
.table-body {
  padding: 0;
}

.ky-table :deep(.el-table__cell) {
  padding-left: 16px;
  padding-right: 16px;
}

.pdf-flags {
  display: inline-flex;
  gap: 4px;
  flex-wrap: wrap;
  justify-content: center;
}

.hidden-input {
  display: none;
}

.danger-item {
  color: var(--el-color-danger);
}
</style>
