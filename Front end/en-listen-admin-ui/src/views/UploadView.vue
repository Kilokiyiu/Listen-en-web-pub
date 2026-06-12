<template>
  <div class="admin-page">
    <PageHeader
      title="音频上传"
      description="上传 CET4/CET6 听力真题音频，支持同时录入字幕原文"
    />

    <div class="upload-grid">
      <div class="admin-card">
        <div class="admin-card__header">
          <span class="admin-card__title">上传表单</span>
        </div>
        <div class="admin-card__body">
          <el-form :model="form" label-width="100px" :rules="rules" ref="formRef" label-position="top">
            <el-row :gutter="16">
              <el-col :span="12">
                <el-form-item label="类别" prop="category">
                  <el-select v-model="form.category" placeholder="选择类别" style="width: 100%">
                    <el-option label="大学英语四级 (CET4)" value="CET4" />
                    <el-option label="大学英语六级 (CET6)" value="CET6" />
                  </el-select>
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="年份" prop="year">
                  <el-input-number v-model="form.year" :min="2000" :max="2030" :step="1" style="width: 100%" />
                </el-form-item>
              </el-col>
            </el-row>

            <el-row :gutter="16">
              <el-col :span="12">
                <el-form-item label="月份" prop="month">
                  <el-select v-model="form.month" placeholder="选择月份" style="width: 100%">
                    <el-option label="6月" :value="6" />
                    <el-option label="12月" :value="12" />
                  </el-select>
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="第几套" prop="setNumber">
                  <el-select v-model="form.setNumber" placeholder="选择套号" style="width: 100%">
                    <el-option label="第1套" :value="1" />
                    <el-option label="第2套" :value="2" />
                    <el-option label="第3套" :value="3" />
                  </el-select>
                </el-form-item>
              </el-col>
            </el-row>

            <el-form-item label="音频文件" prop="file">
              <el-upload
                ref="uploadRef"
                :auto-upload="false"
                :limit="1"
                :on-change="handleFileChange"
                :on-remove="handleFileRemove"
                accept=".mp3,.wav,.m4a"
                drag
                class="upload-dragger"
              >
                <el-icon class="upload-icon"><UploadFilled /></el-icon>
                <div class="el-upload__text">拖拽文件到此处，或<em>点击上传</em></div>
                <template #tip>
                  <div class="el-upload__tip">仅支持 mp3 / wav / m4a 格式</div>
                </template>
              </el-upload>
            </el-form-item>

            <el-form-item label="听力原文">
              <el-input
                v-model="form.subtitle"
                type="textarea"
                :rows="6"
                placeholder='请粘贴字幕JSON，格式如：[{"start":0,"end":3,"text":"Hello"}]'
              />
              <div class="input-tip">可选。支持粘贴字幕 JSON，或后续在「音频管理」中补录</div>
            </el-form-item>

            <el-form-item>
              <el-button type="primary" @click="handleUpload" :loading="uploading" size="large" style="width: 100%">
                {{ uploading ? '上传中...' : '上传音频' }}
              </el-button>
            </el-form-item>
          </el-form>
        </div>
      </div>

      <div class="admin-card preview-card">
        <div class="admin-card__header">
          <span class="admin-card__title">上传预览</span>
        </div>
        <div class="admin-card__body">
          <template v-if="form.file">
            <el-descriptions :column="1" border size="small">
              <el-descriptions-item label="试卷名称">
                {{ form.year }}年{{ form.month }}月大学英语{{ form.category === 'CET4' ? '四级' : '六级' }}听力真题（第{{ form.setNumber }}套）
              </el-descriptions-item>
              <el-descriptions-item label="保存路径">
                /audios/{{ form.category }}/{{ form.year }}/{{ form.year }}.{{ form.month }}.{{ form.setNumber }}.mp3
              </el-descriptions-item>
              <el-descriptions-item label="文件大小">{{ formatSize(form.file.size) }}</el-descriptions-item>
              <el-descriptions-item label="原文">
                {{ form.subtitle.trim() ? '已填写' : '未填写' }}
              </el-descriptions-item>
            </el-descriptions>
          </template>
          <el-empty v-else description="选择音频文件后显示预览信息" :image-size="80" />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { uploadAudio } from '../api/Admin'
import { ElMessage } from 'element-plus'
import PageHeader from '../components/PageHeader.vue'

const formRef = ref(null)
const uploadRef = ref(null)
const uploading = ref(false)
const form = ref({
  category: 'CET6',
  year: new Date().getFullYear(),
  month: 6,
  setNumber: 1,
  file: null,
  subtitle: ''
})

const rules = {
  category: [{ required: true, message: '请选择类别', trigger: 'change' }],
  year: [{ required: true, message: '请输入年份', trigger: 'blur' }],
  month: [{ required: true, message: '请选择月份', trigger: 'change' }],
  setNumber: [{ required: true, message: '请选择套号', trigger: 'change' }],
}

const handleFileChange = (file) => {
  form.value.file = file.raw
}

const handleFileRemove = () => {
  form.value.file = null
}

const formatSize = (bytes) => {
  if (!bytes) return '0 B'
  const k = 1024
  const sizes = ['B', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return (bytes / Math.pow(k, i)).toFixed(1) + ' ' + sizes[i]
}

const handleUpload = async () => {
  if (!form.value.file) {
    ElMessage.warning('请选择音频文件')
    return
  }

  uploading.value = true
  try {
    const formData = new FormData()
    formData.append('categoryParam', form.value.category)
    formData.append('year', form.value.year)
    formData.append('month', form.value.month)
    formData.append('setNumber', form.value.setNumber)
    formData.append('file', form.value.file)
    if (form.value.subtitle.trim()) {
      formData.append('subtitle', form.value.subtitle.trim())
    }

    const res = await uploadAudio(formData)
    ElMessage.success(`上传成功！音频路径：${res.audioUrl}`)
    uploadRef.value?.clearFiles()
    form.value.file = null
    form.value.subtitle = ''
  } catch (e) {
    // 错误已在拦截器中处理
  } finally {
    uploading.value = false
  }
}
</script>

<style scoped>
.upload-grid {
  display: grid;
  grid-template-columns: 1fr 360px;
  gap: 16px;
  align-items: start;
}

@media (max-width: 960px) {
  .upload-grid {
    grid-template-columns: 1fr;
  }
}

.upload-icon {
  font-size: 48px;
  color: var(--admin-primary);
  margin-bottom: 8px;
}

.upload-dragger :deep(.el-upload-dragger) {
  border-radius: var(--admin-radius);
  border-color: #d9d9d9;
  transition: border-color 0.2s;
}

.upload-dragger :deep(.el-upload-dragger:hover) {
  border-color: var(--admin-primary);
}

.input-tip {
  margin-top: 8px;
  color: var(--admin-text-secondary);
  font-size: 12px;
}

.preview-card {
  position: sticky;
  top: calc(var(--admin-header-height) + 16px);
}
</style>
