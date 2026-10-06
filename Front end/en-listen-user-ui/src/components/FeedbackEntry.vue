<template>
  <div class="feedback-entry">
    <el-button :type="buttonType" :round="round" :plain="plain" @click="visible = true">
      <el-icon v-if="showIcon"><ChatDotRound /></el-icon>
      {{ buttonText }}
    </el-button>

    <el-dialog
      v-model="visible"
      title="意见反馈"
      width="480px"
      :append-to-body="true"
      class="feedback-dialog"
      @closed="resetForm"
    >
      <p v-if="source" class="feedback-source">当前页面：{{ source }}</p>
      <el-form ref="formRef" :model="form" :rules="rules" label-position="top">
        <el-form-item label="反馈类型" prop="category">
          <el-select v-model="form.category" placeholder="请选择反馈类型" style="width: 100%">
            <el-option label="功能建议" value="功能建议" />
            <el-option label="问题反馈" value="问题反馈" />
            <el-option label="内容相关" value="内容相关" />
            <el-option label="其他" value="其他" />
          </el-select>
        </el-form-item>
        <el-form-item label="反馈内容" prop="message">
          <el-input
            v-model="form.message"
            type="textarea"
            :rows="5"
            placeholder="请描述你遇到的问题或建议，越具体越有助于我们改进"
            maxlength="2000"
            show-word-limit
          />
        </el-form-item>
        <el-form-item label="联系邮箱（选填）" prop="email">
          <el-input v-model="form.email" placeholder="方便我们回复你" maxlength="100" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="visible = false">取消</el-button>
        <el-button type="primary" :loading="loading" @click="handleSubmit">提交反馈</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { ElMessage } from 'element-plus'
import { ChatDotRound } from '@element-plus/icons-vue'
import { submitFeedback } from '@/api/Feedback'

const props = defineProps({
  source: { type: String, default: '' },
  buttonText: { type: String, default: '意见反馈' },
  buttonType: { type: String, default: 'default' },
  round: { type: Boolean, default: true },
  plain: { type: Boolean, default: true },
  showIcon: { type: Boolean, default: true },
})

const visible = ref(false)
const loading = ref(false)
const formRef = ref(null)

const form = reactive({
  category: '问题反馈',
  email: '',
  message: '',
})

const rules = {
  category: [{ required: true, message: '请选择反馈类型', trigger: 'change' }],
  email: [{ type: 'email', message: '请输入正确的邮箱格式', trigger: 'blur' }],
  message: [
    { required: true, message: '请填写反馈内容', trigger: 'blur' },
    { min: 5, message: '反馈内容至少 5 个字符', trigger: 'blur' },
  ],
}

const resetForm = () => {
  form.message = ''
  form.email = ''
  form.category = '问题反馈'
  formRef.value?.clearValidate?.()
}

const handleSubmit = async () => {
  if (!formRef.value) return
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return

  loading.value = true
  try {
    await submitFeedback({
      category: form.category,
      name: localStorage.getItem('username') || undefined,
      email: form.email.trim() || undefined,
      message: form.message.trim(),
      source: props.source || undefined,
    })
    ElMessage.success('反馈已提交，感谢你的建议！')
    visible.value = false
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.feedback-entry {
  display: inline-flex;
}

.feedback-source {
  margin: 0 0 12px;
  font-size: 13px;
  color: var(--le-text-muted);
  line-height: 1.5;
}
</style>
