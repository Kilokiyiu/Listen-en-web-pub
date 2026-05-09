<template>
  <div class="upload-container">
    <div class="upload-card">
      <div class="card-header">
        <h2>音频上传</h2>
        <span class="user-info">管理员：{{ userName }}</span>
      </div>

      <el-form :model="form" label-width="100px" :rules="rules" ref="formRef">
        <el-form-item label="类别" prop="category">
          <el-select v-model="form.category" placeholder="选择类别" style="width: 100%">
            <el-option label="大学英语四级 (CET4)" value="CET4" />
            <el-option label="大学英语六级 (CET6)" value="CET6" />
          </el-select>
        </el-form-item>

        <el-form-item label="年份" prop="year">
          <el-input-number v-model="form.year" :min="2000" :max="2030" :step="1" style="width: 100%" />
        </el-form-item>

        <el-form-item label="月份" prop="month">
          <el-select v-model="form.month" placeholder="选择月份" style="width: 100%">
            <el-option label="6月" :value="6" />
            <el-option label="12月" :value="12" />
          </el-select>
        </el-form-item>

        <el-form-item label="第几套" prop="setNumber">
          <el-select v-model="form.setNumber" placeholder="选择套号" style="width: 100%">
            <el-option label="第1套" :value="1" />
            <el-option label="第2套" :value="2" />
            <el-option label="第3套" :value="3" />
          </el-select>
        </el-form-item>

        <el-form-item label="音频文件" prop="file">
          <el-upload
            ref="uploadRef"
            :auto-upload="false"
            :limit="1"
            :on-change="handleFileChange"
            :on-remove="handleFileRemove"
            accept=".mp3,.wav,.m4a"
            drag
          >
            <el-icon style="font-size: 48px; color: #c0c4cc"><i class="el-icon-upload" /></el-icon>
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
            placeholder="请粘贴字幕JSON内容，格式如：[{&quot;start&quot;:0,&quot;end&quot;:3,&quot;text&quot;:&quot;Hello&quot;}]"
          />
          <div class="input-tip">
            <span>可选。支持粘贴字幕JSON，或后续在"原文管理"中补录</span>
          </div>
        </el-form-item>

        <el-form-item>
          <el-button type="primary" @click="handleUpload" :loading="uploading" size="large" style="width: 100%">
            {{ uploading ? '上传中...' : '上传音频' }}
          </el-button>
        </el-form-item>
      </el-form>

      <!-- 上传预览 -->
      <div v-if="form.file" class="preview-info">
        <el-descriptions :column="1" border size="small" title="上传预览">
          <el-descriptions-item label="保存路径">
            /audios/{{ form.category }}/{{ form.year }}/{{ form.year }}.{{ form.month }}.{{ form.setNumber }}.mp3
          </el-descriptions-item>
          <el-descriptions-item label="文件大小">{{ formatSize(form.file.size) }}</el-descriptions-item>
          <el-descriptions-item label="试卷名称">
            {{ form.year }}年{{ form.month }}月大学英语{{ form.category === 'CET4' ? '四级' : '六级' }}听力真题（第{{ form.setNumber }}套）
          </el-descriptions-item>
        </el-descriptions>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { uploadAudio } from '../api/Admin'
import { ElMessage } from 'element-plus'

const router = useRouter()
const userName = computed(() => localStorage.getItem('admin_userName') || '')
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
    // 重置表单
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
.upload-container {
  padding: 32px;
  max-width: 700px;
  margin: 0 auto;
}
.upload-card {
  background: #fff;
  border-radius: 16px;
  padding: 32px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.08);
}
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}
.card-header h2 {
  margin: 0;
  color: #1a1a2e;
}
.user-info {
  color: #8a8aaa;
  font-size: 14px;
}
.preview-info {
  margin-top: 24px;
}

.input-tip {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-top: 8px;
  color: #8a8aaa;
  font-size: 12px;
}

.input-tip .el-icon {
  color: #409eff;
}
</style>
