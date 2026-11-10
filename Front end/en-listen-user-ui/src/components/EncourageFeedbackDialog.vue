<template>
  <el-dialog
    v-model="visible"
    :title="step === 'form' ? '用完啦，欢迎反馈' : '感谢反馈'"
    width="480px"
    :append-to-body="true"
    class="encourage-feedback-dialog"
    @closed="resetAll"
  >
    <!-- 步骤 1：填写反馈 -->
    <template v-if="step === 'form'">
      <div class="gift-banner">
        <p class="gift-title">留下反馈，领取学习资料</p>
        <p class="gift-desc">
          {{ contextLine }}
          写几句真实感受即可。提交后可领取「英语学习资源」网盘资料（不定期补充）。
        </p>
      </div>

      <el-form ref="formRef" :model="form" :rules="rules" label-position="top">
        <el-form-item label="反馈类型" prop="category">
          <el-select v-model="form.category" placeholder="请选择反馈类型" style="width: 100%">
            <el-option label="功能建议" value="功能建议" />
            <el-option label="问题反馈" value="问题反馈" />
            <el-option label="内容相关" value="内容相关" />
            <el-option label="使用体验" value="使用体验" />
            <el-option label="其他" value="其他" />
          </el-select>
        </el-form-item>
        <el-form-item label="反馈内容" prop="message">
          <el-input
            v-model="form.message"
            type="textarea"
            :rows="4"
            placeholder="例如：哪里好用、哪里卡住、还希望增加什么功能……"
            maxlength="2000"
            show-word-limit
          />
        </el-form-item>
        <el-form-item label="联系邮箱（选填）" prop="email">
          <el-input v-model="form.email" placeholder="方便我们必要时联系你" maxlength="100" />
        </el-form-item>
      </el-form>
    </template>

    <!-- 步骤 2：展示网盘链接（链接未配置时提示整理中） -->
    <div v-else class="gift-result">
      <template v-if="hasPan">
        <p class="gift-result-title">反馈已收到，这是学习资料网盘</p>
        <p class="gift-result-desc">英语学习资源（不定期补充）。</p>
        <a class="pan-link" :href="PAN.url" target="_blank" rel="noopener noreferrer">
          {{ PAN.url }}
        </a>
        <p v-if="PAN.code" class="pan-code">
          提取码：<strong>{{ PAN.code }}</strong>
        </p>
      </template>
      <template v-else>
        <p class="gift-result-title">反馈已收到，谢谢你！</p>
        <p class="gift-result-desc">
          学习资料（高频词清单、精听方法卡等）正在整理，网盘链接稍后上线。若填写了邮箱，我们也会尽快通知你。
        </p>
      </template>
    </div>

    <template #footer>
      <template v-if="step === 'form'">
        <el-button @click="visible = false">下次再说</el-button>
        <el-button type="primary" :loading="loading" @click="handleSubmit">提交并领取资料</el-button>
      </template>
      <template v-else-if="hasPan">
        <el-button @click="copyPan">复制链接</el-button>
        <el-button type="primary" @click="openPan">打开网盘</el-button>
        <el-button @click="visible = false">关闭</el-button>
      </template>
      <template v-else>
        <el-button type="primary" @click="visible = false">好的</el-button>
      </template>
    </template>
  </el-dialog>
</template>

<script setup>
import { computed, onMounted, onUnmounted, reactive, ref } from 'vue'
import { ElMessage } from 'element-plus'
import { submitFeedback } from '@/api/Feedback'
import {
  markFeedbackGiftClaimed,
  registerEncourageFeedbackOpener,
} from '@/utils/promptFeedbackGift'

/** 反馈礼品：百度网盘「英语学习资源(不定期补充)」 */
const PAN = {
  url: 'https://pan.baidu.com/s/15rBD8JaLGEi3lNPL0BfeuA?pwd=p6bh',
  code: 'p6bh',
}

const hasPan = computed(() => Boolean(PAN.url && PAN.url.startsWith('http')))

const visible = ref(false)
const step = ref('form') // 'form' | 'gift'
const loading = ref(false)
const formRef = ref(null)
const source = ref('')
const context = ref('')
const contextLine = ref('你刚完成一次学习。')

const form = reactive({
  category: '使用体验',
  email: '',
  message: '',
})

const rules = {
  category: [{ required: true, message: '请选择反馈类型', trigger: 'change' }],
  email: [
    {
      validator: (_rule, value, callback) => {
        const v = (value || '').trim()
        if (!v) {
          callback()
          return
        }
        if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v)) {
          callback(new Error('请输入正确的邮箱格式'))
          return
        }
        callback()
      },
      trigger: 'blur',
    },
  ],
  message: [
    { required: true, message: '请填写反馈内容', trigger: 'blur' },
    { min: 5, message: '反馈内容至少 5 个字符', trigger: 'blur' },
  ],
}

const resetForm = () => {
  form.message = ''
  form.email = ''
  form.category = '使用体验'
  formRef.value?.clearValidate?.()
}

const resetAll = () => {
  step.value = 'form'
  resetForm()
}

const open = (payload = {}) => {
  source.value = payload.source || ''
  context.value = payload.context || ''
  contextLine.value = context.value
    ? `你刚完成「${context.value}」。`
    : '你刚完成一次学习。'
  step.value = 'form'
  visible.value = true
}

const openPan = () => {
  window.open(PAN.url, '_blank', 'noopener,noreferrer')
}

const copyPan = async () => {
  const text = PAN.code ? `${PAN.url} 提取码：${PAN.code}` : PAN.url
  try {
    await navigator.clipboard.writeText(text)
    ElMessage.success('已复制网盘链接')
  } catch {
    ElMessage.info(text)
  }
}

const handleSubmit = async () => {
  if (!formRef.value) return
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return

  loading.value = true
  try {
    const email = form.email.trim()
    await submitFeedback({
      category: form.category,
      name: localStorage.getItem('username') || undefined,
      email: email || undefined,
      message: form.message.trim(),
      source: source.value
        ? `[完成学习领资料] ${source.value}`
        : '[完成学习领资料]',
    })
    markFeedbackGiftClaimed()
    step.value = 'gift'
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  // 仅注册弹窗：由桌宠气泡点击后调用 openFeedbackGiftDialog 打开
  registerEncourageFeedbackOpener(open)
})

onUnmounted(() => {
  registerEncourageFeedbackOpener(null)
})
</script>

<style scoped>
.gift-banner {
  margin: 0 0 16px;
  padding: 12px 14px;
  border-radius: var(--le-radius-sm, 8px);
  background: var(--le-gradient-soft, rgba(37, 99, 235, 0.08));
  border: 1px solid var(--le-border, rgba(55, 75, 105, 0.12));
}

.gift-title {
  margin: 0 0 6px;
  font-size: 15px;
  font-weight: 600;
  color: var(--le-text, #1a2433);
  line-height: 1.4;
}

.gift-desc {
  margin: 0;
  font-size: 13px;
  color: var(--le-text-secondary, #4a5568);
  line-height: 1.6;
}

.gift-result {
  padding: 4px 0 8px;
}

.gift-result-title {
  margin: 0 0 8px;
  font-size: 15px;
  font-weight: 600;
  color: var(--le-text, #1a2433);
  line-height: 1.4;
}

.gift-result-desc {
  margin: 0 0 14px;
  font-size: 13px;
  color: var(--le-text-secondary, #4a5568);
  line-height: 1.6;
}

.pan-link {
  display: block;
  word-break: break-all;
  font-size: 13px;
  line-height: 1.5;
  color: var(--el-color-primary, #2563eb);
  text-decoration: underline;
}

.pan-code {
  margin: 12px 0 0;
  font-size: 14px;
  color: var(--le-text, #1a2433);
}

.pan-code strong {
  letter-spacing: 0.04em;
}
</style>
