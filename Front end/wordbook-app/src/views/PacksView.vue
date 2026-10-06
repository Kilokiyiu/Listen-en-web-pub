<template>
  <div class="packs-page">
    <div class="filters">
      <button
        v-for="item in categories"
        :key="item.id"
        type="button"
        class="chip"
        :class="{ active: category === item.id }"
        @click="category = item.id"
      >
        {{ item.label }}
      </button>
    </div>

    <div v-if="loading" class="hint">加载中...</div>
    <div v-else-if="!filtered.length" class="hint">暂无词本</div>

    <div v-else class="shelf">
      <article
        v-for="(pack, index) in filtered"
        :key="pack.id"
        class="book"
        :class="[`tone-${toneOf(pack, index)}`, { claimed: pack.claimed }]"
      >
        <span class="spine" aria-hidden="true"></span>
        <div class="cover">
          <div class="top">
            <span class="badge">{{ categoryLabel(pack.category) }}</span>
            <span v-if="pack.claimed" class="pill">已领</span>
          </div>
          <h3>{{ pack.name }}</h3>
          <p class="count">
            <strong>{{ pack.wordCount }}</strong> 词
            <span v-if="pack.claimed">· {{ pack.claimedCount }}</span>
          </p>
          <div class="actions">
            <button type="button" class="ghost" @click="openDetail(pack)">预览</button>
            <button type="button" class="solid" :disabled="claimingId === pack.id" @click="claim(pack)">
              {{ claimingId === pack.id ? '领取中' : (pack.claimed ? '同步' : '领取') }}
            </button>
          </div>
        </div>
      </article>
    </div>

    <div v-if="detail" class="mask" @click.self="detail = null">
      <div class="sheet le-paper">
        <div class="sheet-head">
          <h3>{{ detail.name }}</h3>
          <button type="button" class="close" @click="detail = null">×</button>
        </div>
        <p class="meta">共 {{ detail.wordCount }} 词</p>
        <ul class="preview">
          <li v-for="(item, i) in detail.preview || []" :key="i">
            <strong>{{ item.word }}</strong>
            <span>{{ item.definition }}</span>
          </li>
        </ul>
        <button
          type="button"
          class="solid full"
          :disabled="claimingId === detail.id"
          @click="claim(detail)"
        >
          {{ detail.claimed ? '同步更新' : '领取' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import {
  claimOfficialWordPack,
  getOfficialWordPackDetail,
  getOfficialWordPacks,
} from '../api/word'
import { isLoggedIn, setActiveWordbook, setCurrentWordBookId } from '../services/appSettings'
import { WORDBOOK_TYPES } from '../config'
import { confirmDialog } from '../utils/dialog'
import { showToast } from '../utils/toast'

const router = useRouter()
const loading = ref(true)
const packs = ref([])
const category = ref('')
const claimingId = ref(null)
const detail = ref(null)

const categories = [
  { id: '', label: '全部' },
  { id: 'cet4', label: '四级' },
  { id: 'cet6', label: '六级' },
  { id: 'kaoyan', label: '考研' },
]

const categoryLabel = (c) =>
  ({ cet4: '四级', cet6: '六级', kaoyan: '考研', other: '其他' }[c] || c)

const toneOf = (pack, index) => {
  if (pack.category === 'cet4') return 0
  if (pack.category === 'cet6') return 1
  if (pack.category === 'kaoyan') return 2
  return index % 5
}

const filtered = computed(() => {
  if (!category.value) return packs.value
  return packs.value.filter((p) => p.category === category.value)
})

const load = async () => {
  loading.value = true
  try {
    packs.value = (await getOfficialWordPacks()) || []
  } catch (e) {
    showToast(typeof e === 'string' ? e : '加载失败')
  } finally {
    loading.value = false
  }
}

const openDetail = async (pack) => {
  try {
    detail.value = await getOfficialWordPackDetail(pack.id, 20)
  } catch (e) {
    showToast(typeof e === 'string' ? e : '预览失败')
  }
}

const claim = async (pack) => {
  if (!(await isLoggedIn())) {
    router.push({ name: 'login', query: { redirect: '/packs' } })
    return
  }
  const ok = await confirmDialog(`领取「${pack.name}」到云端单词本？`, {
    title: pack.claimed ? '同步词本' : '领取词本',
    confirmText: pack.claimed ? '同步' : '领取',
  })
  if (!ok) return

  claimingId.value = pack.id
  try {
    const res = await claimOfficialWordPack(pack.id)
    if (res?.userWordBookId) {
      await setCurrentWordBookId(res.userWordBookId)
    }
    await setActiveWordbook(WORDBOOK_TYPES.server)
    showToast(`新增 ${res.added} 词`)
    await load()
    if (detail.value?.id === pack.id) {
      detail.value = await getOfficialWordPackDetail(pack.id, 20)
    }
    const go = await confirmDialog('去单词本查看？', {
      title: '领取完成',
      confirmText: '去单词本',
      cancelText: '留下',
    })
    if (go) router.push('/')
  } catch (e) {
    showToast(typeof e === 'string' ? e : '领取失败')
  } finally {
    claimingId.value = null
  }
}

onMounted(load)
</script>

<style scoped>
.packs-page {
  padding: 16px;
}

.filters {
  display: flex;
  gap: 8px;
  margin-bottom: 14px;
}

.chip {
  flex: 1;
  padding: 8px 0;
  border: 1px solid var(--le-border);
  border-radius: 8px;
  background: var(--le-bg-elevated);
  color: var(--le-text-secondary);
  font-size: 13px;
}

.chip.active {
  background: var(--le-gradient);
  color: #fff;
  border-color: transparent;
}

.hint {
  text-align: center;
  padding: 48px 0;
  color: var(--le-text-muted);
}

.shelf {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px 10px;
}

.book {
  position: relative;
  min-height: 186px;
}

.spine {
  position: absolute;
  left: 0;
  top: 4px;
  bottom: 4px;
  width: 10px;
  border-radius: 3px 0 0 3px;
  z-index: 2;
}

.cover {
  height: 100%;
  padding: 12px 10px 10px 18px;
  border-radius: 4px 10px 10px 4px;
  border: 1px solid var(--le-border);
  background-image: var(--le-paper-grain);
  display: flex;
  flex-direction: column;
}

.book.tone-0 .cover { background-color: #edf3fb; }
.book.tone-0 .spine { background: linear-gradient(180deg, #1d4ed8, #2563eb); }
.book.tone-1 .cover { background-color: #eef8f3; }
.book.tone-1 .spine { background: linear-gradient(180deg, #047857, #059669); }
.book.tone-2 .cover { background-color: #f8f3ec; }
.book.tone-2 .spine { background: linear-gradient(180deg, #9a6b3f, #c4894a); }
.book.tone-3 .cover { background-color: #f3f1f8; }
.book.tone-3 .spine { background: linear-gradient(180deg, #4a5aa8, #5b6abf); }
.book.tone-4 .cover { background-color: #f7f0ef; }
.book.tone-4 .spine { background: linear-gradient(180deg, #b45a5a, #c97878); }

.top {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.badge {
  font-family: var(--le-font-mono);
  font-size: 10px;
  letter-spacing: 0.04em;
  color: var(--le-text-muted);
}

.pill {
  font-size: 11px;
  color: var(--le-success);
}

.cover h3 {
  margin: 0;
  font-size: 15px;
  line-height: 1.3;
}

.count {
  margin: 8px 0 0;
  font-size: 12px;
  color: var(--le-text-muted);
  flex: 1;
}

.count strong {
  font-size: 18px;
  color: var(--le-text);
}

.actions {
  display: flex;
  gap: 6px;
  margin-top: 10px;
}

.ghost,
.solid {
  flex: 1;
  height: 32px;
  border-radius: 6px;
  font-size: 13px;
}

.ghost {
  border: 1px solid var(--le-border-strong);
  background: rgba(255, 255, 255, 0.75);
  color: var(--le-text-secondary);
}

.solid {
  border: none;
  background: var(--le-gradient);
  color: #fff;
}

.solid:disabled {
  opacity: 0.65;
}

.solid.full {
  width: 100%;
  height: 40px;
  margin-top: 12px;
}

.mask {
  position: fixed;
  inset: 0;
  z-index: 400;
  background: rgba(30, 41, 59, 0.35);
  display: flex;
  align-items: flex-end;
  justify-content: center;
}

.sheet {
  width: 100%;
  max-height: 72vh;
  overflow: auto;
  padding: 16px;
  border-radius: 16px 16px 0 0;
}

.sheet-head {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.sheet-head h3 {
  margin: 0;
  font-size: 17px;
}

.close {
  border: none;
  background: none;
  font-size: 26px;
  color: var(--le-text-muted);
  line-height: 1;
}

.meta {
  margin: 6px 0 12px;
  font-size: 12px;
  color: var(--le-text-muted);
}

.preview {
  list-style: none;
  margin: 0;
  padding: 0;
}

.preview li {
  padding: 8px 0;
  border-bottom: 1px solid var(--le-border);
}

.preview strong {
  display: block;
  font-size: 14px;
}

.preview span {
  font-size: 12px;
  color: var(--le-text-secondary);
}
</style>
