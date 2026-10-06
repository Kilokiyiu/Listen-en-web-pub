<template>
  <div class="add-page">
    <form class="form" @submit.prevent="submit">
      <label>
        <span>单词 <em>*</em></span>
        <div class="word-row">
          <input v-model="form.word" type="text" placeholder="输入英文单词" required />
          <button type="button" class="btn-lookup" :disabled="lookingUp" @click="lookup">
            {{ lookingUp ? '...' : '查词' }}
          </button>
        </div>
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
import { queryEnglishWord, isValidEnglishQuery } from '../api/word'
import { showToast } from '../utils/toast'
import { promptGoReviewAfterAdd } from '../utils/promptGoReview'

const router = useRouter()
const saving = ref(false)
const lookingUp = ref(false)
const form = ref({ word: '', definition: '', example: '' })

const lookup = async () => {
  const word = form.value.word.trim()
  if (!word) {
    showToast('请输入单词')
    return
  }
  if (!isValidEnglishQuery(word)) {
    showToast('请输入有效英语单词')
    return
  }
  lookingUp.value = true
  try {
    const res = await queryEnglishWord(word)
    if (res?.code === 200 && res.data) {
      const data = res.data
      form.value.word = data.word || word
      form.value.definition =
        data.translations?.map((t) => `${t.pos}. ${t.tran_cn}`).join('; ') || form.value.definition
      const first = data.sentences?.[0]
      if (first) form.value.example = `${first.s_content}\n${first.s_cn}`
    } else {
      showToast(res?.message || '未找到释义')
    }
  } catch (e) {
    showToast(typeof e === 'string' ? e : '查询失败')
  } finally {
    lookingUp.value = false
  }
}

const submit = async () => {
  if (!form.value.word.trim()) {
    showToast('请输入单词')
    return
  }
  saving.value = true
  try {
    const word = form.value.word.trim()
    await addWord(form.value)
    if (!await promptGoReviewAfterAdd(router, word)) {
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
  color: var(--le-danger);
  font-style: normal;
}

.word-row {
  display: flex;
  gap: 8px;
}

.word-row input {
  flex: 1;
  min-width: 0;
}

.btn-lookup {
  flex-shrink: 0;
  padding: 0 14px;
  border: 1px solid var(--le-border-strong);
  border-radius: 10px;
  background: var(--le-bg-elevated);
  color: var(--le-primary);
  font-size: 14px;
  font-weight: 500;
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
