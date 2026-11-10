<template>
  <PageShell
    title="意见反馈"
    subtitle="遇到问题或有改进建议？告诉我们，我们会认真阅读每一条反馈"
    :back="true"
  >
    <div class="feedback-wrap">
      <div class="feedback-card le-card">
        <div class="feedback-perk">
          <span class="feedback-perk-badge">福利</span>
          <div class="feedback-perk-copy">
            <p class="feedback-perk-title">反馈获得完整学习资料</p>
            <p class="feedback-perk-desc">提交真实反馈后，可领取「英语学习资源」网盘资料（不定期补充）。</p>
          </div>
        </div>
        <el-form ref="formRef" :model="form" :rules="rules" label-position="top">
          <el-form-item label="反馈类型" prop="category">
            <el-select v-model="form.category" placeholder="请选择反馈类型" size="large" style="width: 100%">
              <el-option label="功能建议" value="功能建议" />
              <el-option label="问题反馈" value="问题反馈" />
              <el-option label="内容相关" value="内容相关" />
              <el-option label="其他" value="其他" />
            </el-select>
          </el-form-item>

          <el-row :gutter="16">
            <el-col :xs="24" :sm="12">
              <el-form-item label="称呼（选填）" prop="name">
                <el-input v-model="form.name" placeholder="如何称呼你" :prefix-icon="User" size="large" maxlength="50" show-word-limit />
              </el-form-item>
            </el-col>
            <el-col :xs="24" :sm="12">
              <el-form-item label="联系邮箱（选填）" prop="email">
                <el-input v-model="form.email" placeholder="方便我们回复你" :prefix-icon="Message" size="large" maxlength="100" />
              </el-form-item>
            </el-col>
          </el-row>

          <el-form-item label="反馈内容" prop="message">
            <el-input
              v-model="form.message"
              type="textarea"
              :rows="6"
              placeholder="请详细描述你的问题或建议，越具体越有助于我们改进"
              maxlength="2000"
              show-word-limit
            />
          </el-form-item>

          <el-button type="primary" size="large" class="submit-btn le-btn-gradient" :loading="loading" @click="handleSubmit">
            提交反馈
          </el-button>
        </el-form>

        <p class="feedback-tip">
          提交后我们会将反馈发送至管理员邮箱。若填写了联系邮箱，我们会在必要时与你取得联系。
        </p>
        <BlogCta class="feedback-blog" />
      </div>
    </div>
  </PageShell>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { ElMessage } from 'element-plus'
import { User, Message } from '@element-plus/icons-vue'
import PageShell from '@/components/PageShell.vue'
import BlogCta from '@/components/BlogCta.vue'
import { submitFeedback } from '@/api/Feedback'

const formRef = ref(null)
const loading = ref(false)

const form = reactive({
  category: '功能建议',
  name: localStorage.getItem('username') || '',
  email: '',
  message: ''
})

const rules = {
  category: [{ required: true, message: '请选择反馈类型', trigger: 'change' }],
  email: [{ type: 'email', message: '请输入正确的邮箱格式', trigger: 'blur' }],
  message: [
    { required: true, message: '请填写反馈内容', trigger: 'blur' },
    { min: 5, message: '反馈内容至少 5 个字符', trigger: 'blur' }
  ]
}

const handleSubmit = async () => {
  if (!formRef.value) return
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return

  loading.value = true
  try {
    await submitFeedback({
      category: form.category,
      name: form.name.trim() || undefined,
      email: form.email.trim() || undefined,
      message: form.message.trim()
    })
    ElMessage.success('反馈已提交，感谢你的建议！')
    form.message = ''
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.feedback-wrap {
  max-width: 720px;
  margin: 0 auto;
}

.feedback-card {
  padding: 28px 24px;
}

.feedback-perk {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  margin: 0 0 20px;
  padding: 14px 16px;
  border: 1px solid rgba(5, 150, 105, 0.18);
  border-radius: 12px;
  background: linear-gradient(135deg, rgba(5, 150, 105, 0.08) 0%, rgba(37, 99, 235, 0.04) 100%);
}

.feedback-perk-badge {
  flex-shrink: 0;
  display: inline-flex;
  align-items: center;
  padding: 4px 10px;
  border-radius: 999px;
  background: rgba(5, 150, 105, 0.12);
  color: var(--le-success);
  font-size: 12px;
  font-weight: 700;
  line-height: 1.2;
}

.feedback-perk-copy {
  min-width: 0;
}

.feedback-perk-title {
  margin: 0 0 4px;
  font-size: 15px;
  font-weight: 700;
  color: var(--le-text);
  line-height: 1.35;
}

.feedback-perk-desc {
  margin: 0;
  font-size: 13px;
  line-height: 1.55;
  color: var(--le-text-secondary);
}

.submit-btn {
  width: 100%;
  margin-top: 8px;
}

.feedback-tip {
  margin: 20px 0 0;
  font-size: 13px;
  color: var(--le-text-muted);
  line-height: 1.6;
  text-align: center;
}

.feedback-blog {
  display: flex;
  margin: 14px 0 0;
}

@media (max-width: 768px) {
  .feedback-card {
    padding: 20px 16px;
  }
}
</style>
