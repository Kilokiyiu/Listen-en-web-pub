<template>
  <div class="add-page">
    <form class="form" @submit.prevent="submit">
      <label>
        <span>单词 <em>*</em></span>
        <input v-model="form.word" type="text" placeholder="输入英文单词" required />
      </label>
      <label>
        <span>释义</span>
        <textarea v-model="form.definition" rows="3" placeholder="输入中文释义" />
      </label>
      <label>
        <span>例句</span>
        <textarea v-model="form.example" rows="4" placeholder="输入例句（可选）" />
      </label>
      <button type="submit" class="btn-primary" :disabled="saving">
        {{ saving ? '保存中...' : '保存' }}
      </button>
    </form>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { addWord } from '../services/wordService'
import { showToast } from '../utils/toast'
import { promptGoReviewAfterAdd } from '../utils/promptGoReview'

const router = useRouter()
const saving = ref(false)
const form = ref({ word: '', definition: '', example: '' })

const submit = async () => {
  if (!form.value.word.trim()) {
    showToast('请输入单词')
    return
  }
  saving.value = true
  try {
    const word = form.value.word.trim()
    await addWord(form.value)
    if (!promptGoReviewAfterAdd(router, word)) {
      router.back()
    }
  } catch (e) {
    showToast(typeof e === 'string' ? e : '添加失败')
  } finally {
    saving.value = false
  }
}
</script>

<style scoped>
.add-page {
  padding: 16px;
}

.form {
  display: flex;
  flex-direction: column;
  gap: 18px;
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
  color: #f56c6c;
  font-style: normal;
}

input, textarea {
  padding: 12px 14px;
  border: 1px solid var(--border);
  border-radius: 10px;
  font-size: 16px;
  background: var(--bg-card);
  font-family: inherit;
  resize: vertical;
}

.btn-primary {
  margin-top: 8px;
  padding: 14px;
  background: var(--primary);
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
