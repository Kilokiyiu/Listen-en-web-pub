<template>
  <div class="admin-page">
    <PageHeader
      title="官方词本"
      description="录入四级 / 六级 / 考研高频词，发布后用户可一键领取到个人单词本"
    >
      <template #extra>
        <el-button type="primary" @click="openCreatePack">
          <el-icon><Plus /></el-icon>
          新建词本
        </el-button>
      </template>
    </PageHeader>

    <div class="admin-card">
      <div class="admin-card__header">
        <span class="admin-card__title">词本列表</span>
        <el-button text type="primary" :loading="loading" @click="loadPacks">刷新</el-button>
      </div>
      <div class="admin-card__body">
        <el-table :data="packs" v-loading="loading" stripe>
          <el-table-column prop="name" label="名称" min-width="140" />
          <el-table-column prop="code" label="编码" width="120" />
          <el-table-column label="分类" width="100">
            <template #default="{ row }">
              <el-tag size="small">{{ categoryLabel(row.category) }}</el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="wordCount" label="词数" width="80" align="center" />
          <el-table-column prop="sortOrder" label="排序" width="70" align="center" />
          <el-table-column label="状态" width="90" align="center">
            <template #default="{ row }">
              <el-tag :type="row.isPublished ? 'success' : 'info'" size="small">
                {{ row.isPublished ? '已发布' : '草稿' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="操作" width="320" fixed="right">
            <template #default="{ row }">
              <el-button link type="primary" @click="openEntries(row)">词条</el-button>
              <el-button link type="primary" @click="openEditPack(row)">编辑</el-button>
              <el-button link type="warning" @click="togglePublish(row)">
                {{ row.isPublished ? '下架' : '发布' }}
              </el-button>
              <el-button link type="danger" @click="removePack(row)">删除</el-button>
            </template>
          </el-table-column>
        </el-table>
      </div>
    </div>

    <!-- 词条管理 -->
    <div v-if="activePack" class="admin-card" style="margin-top: 16px">
      <div class="admin-card__header">
        <span class="admin-card__title">
          词条 · {{ activePack.name }}
          <el-tag size="small" style="margin-left: 8px">{{ entryTotal }} 词</el-tag>
        </span>
        <div class="header-actions">
          <el-input
            v-model="entrySearch"
            clearable
            placeholder="搜索单词 / 释义"
            style="width: 200px"
            @keyup.enter="loadEntries(1)"
          />
          <el-button @click="loadEntries(1)">搜索</el-button>
          <el-button type="primary" @click="showAddEntry = true">添加单词</el-button>
          <el-button type="success" @click="showImport = true">批量导入</el-button>
        </div>
      </div>
      <div class="admin-card__body">
        <el-table :data="entries" v-loading="entriesLoading" stripe>
          <el-table-column prop="rank" label="#" width="70" align="center" />
          <el-table-column prop="word" label="单词" width="140" />
          <el-table-column prop="phonetic" label="音标" width="120" show-overflow-tooltip />
          <el-table-column prop="definition" label="释义" min-width="200" show-overflow-tooltip />
          <el-table-column prop="example" label="例句" min-width="160" show-overflow-tooltip />
          <el-table-column label="操作" width="140" fixed="right">
            <template #default="{ row }">
              <el-button link type="primary" @click="openEditEntry(row)">编辑</el-button>
              <el-button link type="danger" @click="removeEntry(row)">删除</el-button>
            </template>
          </el-table-column>
        </el-table>
        <div class="pager">
          <el-pagination
            background
            layout="prev, pager, next, total"
            :total="entryTotal"
            :page-size="entryPageSize"
            :current-page="entryPage"
            @current-change="loadEntries"
          />
        </div>
      </div>
    </div>

    <!-- 新建 / 编辑词本 -->
    <el-dialog v-model="packDialogVisible" :title="editingPack ? '编辑词本' : '新建词本'" width="520px">
      <el-form :model="packForm" label-width="90px">
        <el-form-item label="编码" required>
          <el-input
            v-model="packForm.code"
            :disabled="!!editingPack"
            placeholder="如 cet4-core"
          />
        </el-form-item>
        <el-form-item label="名称" required>
          <el-input v-model="packForm.name" placeholder="如 四级高频词" />
        </el-form-item>
        <el-form-item label="分类">
          <el-select v-model="packForm.category" style="width: 100%">
            <el-option label="四级" value="cet4" />
            <el-option label="六级" value="cet6" />
            <el-option label="考研" value="kaoyan" />
            <el-option label="其他" value="other" />
          </el-select>
        </el-form-item>
        <el-form-item label="排序">
          <el-input-number v-model="packForm.sortOrder" :min="0" :max="9999" />
        </el-form-item>
        <el-form-item label="描述">
          <el-input v-model="packForm.description" type="textarea" :rows="3" />
        </el-form-item>
        <el-form-item v-if="!editingPack" label="发布">
          <el-switch v-model="packForm.isPublished" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="packDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="saving" @click="savePack">保存</el-button>
      </template>
    </el-dialog>

    <!-- 添加 / 编辑词条 -->
    <el-dialog v-model="showAddEntry" :title="editingEntry ? '编辑词条' : '添加单词'" width="520px">
      <el-form :model="entryForm" label-width="90px">
        <el-form-item label="单词" required>
          <el-input v-model="entryForm.word" :disabled="!!editingEntry" />
        </el-form-item>
        <el-form-item label="音标">
          <el-input v-model="entryForm.phonetic" />
        </el-form-item>
        <el-form-item label="释义">
          <el-input v-model="entryForm.definition" type="textarea" :rows="2" />
        </el-form-item>
        <el-form-item label="例句">
          <el-input v-model="entryForm.example" type="textarea" :rows="2" />
        </el-form-item>
        <el-form-item label="排序">
          <el-input-number v-model="entryForm.rank" :min="0" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showAddEntry = false">取消</el-button>
        <el-button type="primary" :loading="saving" @click="saveEntry">保存</el-button>
      </template>
    </el-dialog>

    <!-- 批量导入 -->
    <el-dialog v-model="showImport" title="批量导入词条" width="640px">
      <p class="hint">
        粘贴 JSON 数组，或每行 <code>单词|释义|例句</code> / <code>单词\t释义</code>。
        也可使用 tools/word-pack-import 从 PDF 导出 JSON 后粘贴。
      </p>
      <el-input
        v-model="importText"
        type="textarea"
        :rows="12"
        placeholder='[{"word":"abandon","definition":"v. 放弃","rank":1}]'
      />
      <div style="margin-top: 12px">
        <el-checkbox v-model="replaceExisting">覆盖已存在单词的释义</el-checkbox>
      </div>
      <template #footer>
        <el-button @click="showImport = false">取消</el-button>
        <el-button type="primary" :loading="importing" @click="doImport">导入</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus } from '@element-plus/icons-vue'
import PageHeader from '../components/PageHeader.vue'
import {
  getWordPacks,
  createWordPack,
  updateWordPack,
  toggleWordPackPublish,
  deleteWordPack,
  getWordPackEntries,
  addWordPackEntry,
  updateWordPackEntry,
  deleteWordPackEntry,
  importWordPackEntries,
} from '../api/Admin'

const loading = ref(false)
const packs = ref([])
const activePack = ref(null)

const packDialogVisible = ref(false)
const editingPack = ref(null)
const saving = ref(false)
const packForm = reactive({
  code: '',
  name: '',
  category: 'cet4',
  description: '',
  sortOrder: 0,
  isPublished: false,
})

const entries = ref([])
const entriesLoading = ref(false)
const entryPage = ref(1)
const entryPageSize = 50
const entryTotal = ref(0)
const entrySearch = ref('')
const showAddEntry = ref(false)
const editingEntry = ref(null)
const entryForm = reactive({
  word: '',
  phonetic: '',
  definition: '',
  example: '',
  rank: 0,
})

const showImport = ref(false)
const importText = ref('')
const replaceExisting = ref(false)
const importing = ref(false)

const categoryLabel = (c) =>
  ({ cet4: '四级', cet6: '六级', kaoyan: '考研', other: '其他' }[c] || c)

const loadPacks = async () => {
  loading.value = true
  try {
    const res = await getWordPacks()
    packs.value = res.data || []
    if (activePack.value) {
      const fresh = packs.value.find((p) => p.id === activePack.value.id)
      if (fresh) activePack.value = fresh
    }
  } finally {
    loading.value = false
  }
}

const openCreatePack = () => {
  editingPack.value = null
  Object.assign(packForm, {
    code: '',
    name: '',
    category: 'cet4',
    description: '',
    sortOrder: packs.value.length,
    isPublished: false,
  })
  packDialogVisible.value = true
}

const openEditPack = (row) => {
  editingPack.value = row
  Object.assign(packForm, {
    code: row.code,
    name: row.name,
    category: row.category,
    description: row.description || '',
    sortOrder: row.sortOrder || 0,
    isPublished: row.isPublished,
  })
  packDialogVisible.value = true
}

const savePack = async () => {
  if (!packForm.name?.trim()) {
    ElMessage.warning('请填写名称')
    return
  }
  if (!editingPack.value && !packForm.code?.trim()) {
    ElMessage.warning('请填写编码')
    return
  }
  saving.value = true
  try {
    if (editingPack.value) {
      await updateWordPack(editingPack.value.id, {
        name: packForm.name,
        category: packForm.category,
        description: packForm.description,
        sortOrder: packForm.sortOrder,
      })
    } else {
      await createWordPack({
        code: packForm.code,
        name: packForm.name,
        category: packForm.category,
        description: packForm.description,
        sortOrder: packForm.sortOrder,
        isPublished: packForm.isPublished,
      })
    }
    ElMessage.success('已保存')
    packDialogVisible.value = false
    await loadPacks()
  } finally {
    saving.value = false
  }
}

const togglePublish = async (row) => {
  await toggleWordPackPublish(row.id)
  ElMessage.success(row.isPublished ? '已下架' : '已发布')
  await loadPacks()
}

const removePack = async (row) => {
  await ElMessageBox.confirm(`确定删除词本「${row.name}」及其全部词条？`, '确认', { type: 'warning' })
  await deleteWordPack(row.id)
  ElMessage.success('已删除')
  if (activePack.value?.id === row.id) activePack.value = null
  await loadPacks()
}

const openEntries = async (row) => {
  activePack.value = row
  entrySearch.value = ''
  await loadEntries(1)
}

const loadEntries = async (page = 1) => {
  if (!activePack.value) return
  entriesLoading.value = true
  entryPage.value = page
  try {
    const res = await getWordPackEntries(activePack.value.id, {
      page,
      pageSize: entryPageSize,
      search: entrySearch.value || undefined,
    })
    const data = res.data || {}
    entries.value = data.items || []
    entryTotal.value = data.total || 0
  } finally {
    entriesLoading.value = false
  }
}

const openEditEntry = (row) => {
  editingEntry.value = row
  Object.assign(entryForm, {
    word: row.word,
    phonetic: row.phonetic || '',
    definition: row.definition || '',
    example: row.example || '',
    rank: row.rank || 0,
  })
  showAddEntry.value = true
}

const saveEntry = async () => {
  if (!activePack.value) return
  if (!editingEntry.value && !entryForm.word?.trim()) {
    ElMessage.warning('请填写单词')
    return
  }
  saving.value = true
  try {
    if (editingEntry.value) {
      await updateWordPackEntry(activePack.value.id, editingEntry.value.id, {
        phonetic: entryForm.phonetic,
        definition: entryForm.definition,
        example: entryForm.example,
        rank: entryForm.rank,
      })
    } else {
      await addWordPackEntry(activePack.value.id, { ...entryForm })
    }
    ElMessage.success('已保存')
    showAddEntry.value = false
    editingEntry.value = null
    Object.assign(entryForm, { word: '', phonetic: '', definition: '', example: '', rank: 0 })
    await loadEntries(entryPage.value)
    await loadPacks()
  } finally {
    saving.value = false
  }
}

const removeEntry = async (row) => {
  await ElMessageBox.confirm(`删除词条「${row.word}」？`, '确认', { type: 'warning' })
  await deleteWordPackEntry(activePack.value.id, row.id)
  ElMessage.success('已删除')
  await loadEntries(entryPage.value)
  await loadPacks()
}

const parseImportText = (text) => {
  const raw = (text || '').trim()
  if (!raw) return []

  if (raw.startsWith('[')) {
    const arr = JSON.parse(raw)
    return arr
      .map((x, i) => ({
        word: x.word || x.Word || '',
        phonetic: x.phonetic || x.Phonetic || '',
        definition: x.definition || x.Definition || x.meaning || '',
        example: x.example || x.Example || '',
        rank: Number(x.rank || x.Rank || i + 1) || i + 1,
      }))
      .filter((x) => x.word)
  }

  return raw
    .split(/\r?\n/)
    .map((line, i) => line.trim())
    .filter(Boolean)
    .map((line, i) => {
      const parts = line.includes('|')
        ? line.split('|')
        : line.includes('\t')
          ? line.split('\t')
          : line.split(/\s{2,}/)
      return {
        word: (parts[0] || '').trim(),
        definition: (parts[1] || '').trim(),
        example: (parts[2] || '').trim(),
        phonetic: '',
        rank: i + 1,
      }
    })
    .filter((x) => x.word)
}

const doImport = async () => {
  if (!activePack.value) return
  let entriesPayload
  try {
    entriesPayload = parseImportText(importText.value)
  } catch (e) {
    ElMessage.error('解析失败：' + (e.message || e))
    return
  }
  if (!entriesPayload.length) {
    ElMessage.warning('没有可导入的词条')
    return
  }
  importing.value = true
  try {
    const res = await importWordPackEntries(activePack.value.id, {
      entries: entriesPayload,
      replaceExisting: replaceExisting.value,
    })
    const d = res.data || {}
    ElMessage.success(`新增 ${d.added || 0}，更新 ${d.updated || 0}，跳过 ${d.skipped || 0}`)
    showImport.value = false
    importText.value = ''
    await loadEntries(1)
    await loadPacks()
  } finally {
    importing.value = false
  }
}

onMounted(loadPacks)
</script>

<style scoped>
.header-actions {
  display: flex;
  gap: 8px;
  align-items: center;
  flex-wrap: wrap;
}
.pager {
  margin-top: 16px;
  display: flex;
  justify-content: flex-end;
}
.hint {
  color: var(--el-text-color-secondary);
  font-size: 13px;
  margin: 0 0 12px;
  line-height: 1.5;
}
.hint code {
  background: var(--el-fill-color);
  padding: 1px 6px;
  border-radius: 4px;
}
</style>
