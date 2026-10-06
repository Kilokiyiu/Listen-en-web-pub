<template>
  <div class="feedback-page">
    <p class="intro">功能建议或问题都可以写在这里。</p>

    <form class="form" @submit.prevent="handleSubmit">
      <label>
        <span>反馈类型 <em>*</em></span>
        <select v-model="form.category" required>
          <option v-for="c in categories" :key="c" :value="c">{{ c }}</option>
        </select>
      </label>

      <label>
        <span>称呼（选填）</span>
        <input v-model="form.name" type="text" maxlength="50" placeholder="如何称呼你" />
      </label>

      <label>
        <span>联系邮箱（选填）</span>
        <input v-model="form.email" type="email" maxlength="100" placeholder="方便我们回复你" />
      </label>

      <label>
        <span>反馈内容 <em>*</em></span>
        <textarea
          v-model="form.message"
          rows="6"
          maxlength="2000"
          placeholder="请详细描述你的问题或建议，越具体越有助于我们改进"
          required
        />
        <small class="counter">{{ form.message.length }} / 2000</small>
      </label>

      <button type="submit" class="btn-primary" :disabled="loading">
        {{ loading ? '提交中...' : '提交反馈' }}
      </button>
    </form>
  </div>
</template>

<script setup>
import { reactive, ref, onMounted } from 'vue'
import { submitFeedback } from '../api/feedback'
import { getAuth } from '../services/appSettings'
import { showToast } from '../utils/toast'

const categories = ['功能建议', '问题反馈', '内容相关', '其他']
const loading = ref(false)
const form = reactive({
  category: '功能建议',
  name: '',
  email: '',
  message: '',
})

onMounted(async () => {
  const auth = await getAuth()
  if (auth.username) form.name = auth.username
})

const handleSubmit = async () => {
  const message = form.message.trim()
  if (!form.category) {
    showToast('请选择反馈类型')
    return
  }
  if (message.length < 5) {
    showToast('反馈内容至少 5 个字符')
    return
  }
  const email = form.email.trim()
  if (email && !email.includes('@')) {
    showToast('请输入正确的邮箱格式')
    return
  }

  loading.value = true
  try {
    await submitFeedback({
      category: form.category,
      name: form.name.trim() || undefined,
      email: email || undefined,
      message,
    })
    showToast('反馈已提交，感谢你的建议！')
    form.message = ''
  } catch (e) {
    showToast(typeof e === 'string' ? e : '提交失败，请稍后重试')
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.feedback-page {
  padding: 16px;
}

.intro {
  margin: 0 0 16px;
  font-size: 14px;
  color: var(--text-secondary);
  line-height: 1.55;
}

.form {
  display: flex;
  flex-direction: column;
  gap: 16px;
  background: var(--bg-card);
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 16px;
}

label {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

label span {
  font-size: 14px;
  color: var(--text-secondary);
}

label em {
  color: var(--le-danger);
  font-style: normal;
}

input,
select,
textarea {
  padding: 12px 14px;
  border: 1px solid var(--border);
  border-radius: 10px;
  font-size: 15px;
  background: var(--bg);
  font-family: inherit;
  color: var(--text);
}

textarea {
  resize: vertical;
  min-height: 120px;
}

.counter {
  align-self: flex-end;
  font-size: 12px;
  color: var(--text-muted);
}

.btn-primary {
  margin-top: 4px;
  padding: 14px;
  background: var(--le-gradient);
  color: #fff;
  border: none;
  border-radius: 10px;
  font-size: 16px;
  font-weight: 600;
}

.btn-primary:disabled {
  opacity: 0.6;
}
</style>
