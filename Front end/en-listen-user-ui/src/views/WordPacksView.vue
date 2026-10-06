<template>
  <PageShell
    title="官方词本"
    subtitle="从书架领取一本，开始系统背词"
    :back="true"
    badge="PACKS"
  >
    <div class="filter-bar le-paper-flip">
      <el-radio-group v-model="category">
        <el-radio-button label="">全部</el-radio-button>
        <el-radio-button label="cet4">四级</el-radio-button>
        <el-radio-button label="cet6">六级</el-radio-button>
        <el-radio-button label="kaoyan">考研</el-radio-button>
      </el-radio-group>
      <el-button text type="primary" @click="$router.push({ name: 'myWords' })">
        我的书架 →
      </el-button>
    </div>

    <el-skeleton v-if="loading" :rows="4" animated />

    <el-empty v-else-if="!filteredPacks.length" description="暂无已发布的官方词本" />

    <section v-else class="bookstore le-paper-flip le-paper-flip-delay-1">
      <div class="shelf-header">
        <span class="sheet-label">OFFICIAL · 官方书架</span>
        <span class="shelf-count">{{ filteredPacks.length }} 本可领</span>
      </div>

      <div class="shelf-rail">
        <article
          v-for="(pack, index) in filteredPacks"
          :key="pack.id"
          class="vocab-book"
          :class="[`tone-${toneOf(pack, index)}`, { claimed: pack.claimed }]"
        >
          <span class="vocab-book__spine" aria-hidden="true"></span>
          <span class="vocab-book__edge" aria-hidden="true"></span>
          <div class="vocab-book__cover">
            <div class="vocab-book__top">
              <span class="vocab-book__badge">{{ categoryLabel(pack.category) }}</span>
              <span v-if="pack.claimed" class="claimed-pill">已领</span>
            </div>
            <h3 class="vocab-book__title">{{ pack.name }}</h3>
            <p class="vocab-book__desc">{{ pack.description || '系统词表，领取后按间隔复习' }}</p>
            <div class="vocab-book__foot">
              <strong>{{ pack.wordCount }}</strong>
              <span>词</span>
              <span v-if="pack.claimed" class="sync">· 已同步 {{ pack.claimedCount }}</span>
            </div>
            <div class="vocab-book__actions">
              <button type="button" class="ghost-btn" @click="openDetail(pack)">预览</button>
              <button
                type="button"
                class="solid-btn"
                :disabled="claimingId === pack.id"
                @click="claim(pack)"
              >
                {{ claimingId === pack.id ? '领取中…' : (pack.claimed ? '同步' : '领取') }}
              </button>
            </div>
          </div>
        </article>
      </div>
      <div class="shelf-plank" aria-hidden="true"></div>
    </section>

    <el-drawer v-model="detailVisible" :title="detail?.name || '词本预览'" size="420px">
      <template v-if="detail">
        <p class="drawer-desc">{{ detail.description || '领取后写入你的个人单词本，可用现有复习功能。' }}</p>
        <p class="drawer-meta">共 {{ detail.wordCount }} 词 · 预览前 {{ detail.preview?.length || 0 }} 条</p>
        <el-table :data="detail.preview || []" size="small" stripe max-height="360">
          <el-table-column prop="word" label="单词" width="110" />
          <el-table-column prop="definition" label="释义" show-overflow-tooltip />
        </el-table>
        <div class="drawer-actions">
          <el-button
            type="primary"
            class="le-btn-gradient"
            :loading="claimingId === detail.id"
            @click="claim(detail)"
          >
            {{ detail.claimed ? '同步更新' : '领取并背诵' }}
          </el-button>
        </div>
      </template>
    </el-drawer>
  </PageShell>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import PageShell from '../components/PageShell.vue'
import {
  getOfficialWordPacks,
  getOfficialWordPackDetail,
  claimOfficialWordPack,
  setCurrentWordBookId,
} from '../api/Word.js'

const router = useRouter()
const loading = ref(true)
const packs = ref([])
const category = ref('')
const claimingId = ref(null)
const detailVisible = ref(false)
const detail = ref(null)

const categoryLabel = (c) =>
  ({ cet4: '四级', cet6: '六级', kaoyan: '考研', other: '其他' }[c] || c)

const toneOf = (pack, index) => {
  if (pack.category === 'cet4') return 0
  if (pack.category === 'cet6') return 1
  if (pack.category === 'kaoyan') return 2
  return index % 5
}

const filteredPacks = computed(() => {
  if (!category.value) return packs.value
  return packs.value.filter((p) => p.category === category.value)
})

const ensureLogin = () => {
  if (!localStorage.getItem('token') || !localStorage.getItem('userId')) {
    router.push({ name: 'login', query: { redirect: '/word-packs' } })
    return false
  }
  return true
}

const load = async () => {
  loading.value = true
  try {
    packs.value = (await getOfficialWordPacks()) || []
  } finally {
    loading.value = false
  }
}

const openDetail = async (pack) => {
  detail.value = await getOfficialWordPackDetail(pack.id, 30)
  detailVisible.value = true
}

const claim = async (pack) => {
  if (!ensureLogin()) return

  try {
    await ElMessageBox.confirm(
      `将「${pack.name}」中的单词复制到你的个人单词本（已有单词会跳过）。词量较大时可能需要几秒。`,
      '领取官方词本',
      { type: 'info', confirmButtonText: '开始领取', cancelButtonText: '取消' }
    )
  } catch {
    return
  }

  claimingId.value = pack.id
  try {
    const res = await claimOfficialWordPack(pack.id)
    if (res?.userWordBookId) {
      setCurrentWordBookId(res.userWordBookId)
    }
    ElMessage.success(
      `完成：新增 ${res.added} 词，本中共 ${res.totalInBook} 词` +
        (res.remaining > 0 ? `（还可同步 ${res.remaining}）` : '')
    )
    await load()
    if (detail.value?.id === pack.id) {
      detail.value = await getOfficialWordPackDetail(pack.id, 30)
    }
    await ElMessageBox.confirm('是否前往单词本开始复习？', '领取成功', {
      confirmButtonText: '去单词本',
      cancelButtonText: '留在这里',
      type: 'success',
    })
      .then(() => router.push({ name: 'myWords' }))
      .catch(() => {})
  } finally {
    claimingId.value = null
  }
}

onMounted(load)
</script>

<style scoped>
.sheet-label {
  font-family: var(--le-font-mono);
  font-size: 11px;
  color: var(--le-primary);
  letter-spacing: 0.06em;
}

.filter-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;
  flex-wrap: wrap;
}

.bookstore {
  margin-bottom: 12px;
}

.shelf-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 14px;
}

.shelf-count {
  font-size: 12px;
  color: var(--le-text-muted);
}

.shelf-rail {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(168px, 1fr));
  gap: 18px 16px;
  padding: 6px 2px 0;
}

.shelf-plank {
  height: 14px;
  margin-top: 10px;
  border-radius: 2px 2px 8px 8px;
  background:
    linear-gradient(180deg, rgba(120, 90, 60, 0.35), rgba(90, 65, 40, 0.55)),
    repeating-linear-gradient(90deg, rgba(255, 255, 255, 0.04) 0 2px, transparent 2px 7px);
  box-shadow: 0 8px 16px rgba(30, 41, 59, 0.12), inset 0 1px 0 rgba(255, 255, 255, 0.25);
}

.vocab-book {
  position: relative;
  height: 250px;
  transform-origin: left bottom;
  transition: transform 0.25s ease, filter 0.25s ease;
  filter: drop-shadow(0 8px 12px rgba(30, 41, 59, 0.12));
}

.vocab-book:hover {
  transform: translateY(-8px) rotate(-1.2deg);
  z-index: 2;
}

.vocab-book__spine {
  position: absolute;
  left: 0;
  top: 4px;
  bottom: 4px;
  width: 14px;
  border-radius: 3px 0 0 3px;
  z-index: 2;
}

.vocab-book__edge {
  position: absolute;
  right: -3px;
  top: 8px;
  bottom: 8px;
  width: 6px;
  border-radius: 0 2px 2px 0;
  background: repeating-linear-gradient(#f4f1ea 0 2px, #e8e2d6 2px 3px);
  z-index: 1;
}

.vocab-book__cover {
  position: absolute;
  inset: 0;
  padding: 16px 14px 14px 22px;
  border-radius: 4px 10px 10px 4px;
  border: 1px solid rgba(55, 75, 105, 0.16);
  background-image: var(--le-paper-grain), linear-gradient(145deg, rgba(255, 255, 255, 0.5), transparent 55%);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.vocab-book.tone-0 .vocab-book__cover { background-color: #edf3fb; border-color: rgba(37, 99, 235, 0.22); }
.vocab-book.tone-0 .vocab-book__spine { background: linear-gradient(180deg, #1d4ed8, #2563eb); }
.vocab-book.tone-1 .vocab-book__cover { background-color: #eef8f3; border-color: rgba(5, 150, 105, 0.22); }
.vocab-book.tone-1 .vocab-book__spine { background: linear-gradient(180deg, #047857, #059669); }
.vocab-book.tone-2 .vocab-book__cover { background-color: #f8f3ec; border-color: rgba(180, 120, 60, 0.25); }
.vocab-book.tone-2 .vocab-book__spine { background: linear-gradient(180deg, #9a6b3f, #c4894a); }
.vocab-book.tone-3 .vocab-book__cover { background-color: #f3f1f8; border-color: rgba(91, 106, 191, 0.22); }
.vocab-book.tone-3 .vocab-book__spine { background: linear-gradient(180deg, #4a5aa8, #5b6abf); }
.vocab-book.tone-4 .vocab-book__cover { background-color: #f7f0ef; border-color: rgba(201, 120, 120, 0.28); }
.vocab-book.tone-4 .vocab-book__spine { background: linear-gradient(180deg, #b45a5a, #c97878); }

.vocab-book__top {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 6px;
  margin-bottom: 10px;
}

.vocab-book__badge {
  font-family: var(--le-font-mono);
  font-size: 10px;
  letter-spacing: 0.06em;
  color: var(--le-text-muted);
}

.claimed-pill {
  font-size: 11px;
  color: var(--le-success);
  background: rgba(5, 150, 105, 0.1);
  border: 1px solid rgba(5, 150, 105, 0.2);
  border-radius: 4px;
  padding: 1px 6px;
}

.vocab-book__title {
  margin: 0;
  font-family: var(--le-font-display);
  font-size: 17px;
  font-weight: 700;
  line-height: 1.3;
  color: var(--le-text);
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.vocab-book__desc {
  margin: 8px 0 0;
  font-size: 12px;
  line-height: 1.45;
  color: var(--le-text-muted);
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
  flex: 1;
}

.vocab-book__foot {
  display: flex;
  align-items: baseline;
  gap: 4px;
  margin-top: 8px;
  padding-top: 8px;
  border-top: 1px dashed rgba(55, 75, 105, 0.16);
}

.vocab-book__foot strong {
  font-family: var(--le-font-display);
  font-size: 20px;
  color: var(--le-text);
}

.vocab-book__foot span {
  font-size: 12px;
  color: var(--le-text-muted);
}

.vocab-book__foot .sync {
  margin-left: 2px;
}

.vocab-book__actions {
  display: flex;
  gap: 8px;
  margin-top: 12px;
}

.ghost-btn,
.solid-btn {
  flex: 1;
  height: 32px;
  border-radius: 6px;
  font-size: 13px;
  cursor: pointer;
  border: 1px solid var(--le-border-strong);
  background: rgba(255, 255, 255, 0.75);
  color: var(--le-text-secondary);
}

.solid-btn {
  border: none;
  background: var(--le-gradient);
  color: #fff;
}

.solid-btn:disabled {
  opacity: 0.65;
  cursor: wait;
}

.drawer-desc {
  color: var(--le-text-secondary);
  line-height: 1.6;
  margin: 0 0 8px;
}

.drawer-meta {
  font-size: 12px;
  color: var(--le-text-muted);
  margin: 0 0 12px;
}

.drawer-actions {
  margin-top: 16px;
}

@media (max-width: 640px) {
  .filter-bar {
    flex-direction: column;
    align-items: stretch;
    gap: 10px;
  }

  .filter-bar :deep(.el-radio-group) {
    display: flex;
    width: 100%;
  }

  .filter-bar :deep(.el-radio-button) {
    flex: 1;
  }

  .filter-bar :deep(.el-radio-button__inner) {
    width: 100%;
    padding: 8px 0;
  }

  .shelf-header {
    margin-bottom: 10px;
  }

  .shelf-rail {
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 12px 10px;
  }

  .vocab-book {
    height: auto;
    min-height: 210px;
  }

  .vocab-book:hover {
    transform: none;
  }

  .vocab-book__cover {
    padding: 12px 10px 10px 18px;
  }

  .vocab-book__title {
    font-size: 15px;
  }

  .vocab-book__desc {
    -webkit-line-clamp: 2;
    font-size: 11px;
  }

  .vocab-book__actions {
    margin-top: 8px;
  }

  .ghost-btn,
  .solid-btn {
    height: 36px;
    font-size: 13px;
  }

  .shelf-plank {
    height: 10px;
    margin-top: 8px;
  }
}

@media (hover: none) {
  .vocab-book:hover {
    transform: none;
  }
}
</style>
