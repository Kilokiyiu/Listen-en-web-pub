<template>
  <PageShell
    title="我的单词本"
    subtitle="书架上的每一本，都是你的词汇积累"
    :back="true"
    back-label="单词"
    back-to="/words"
    badge="WORDS"
  >
    <template #header-extra>
      <el-button class="header-btn header-btn--secondary" @click="$router.push({ name: 'wordPacks' })">
        <el-icon><Collection /></el-icon>
        <span class="btn-label">官方词本</span>
      </el-button>
      <el-button
        v-if="!isFavoritesBook"
        type="primary"
        class="le-btn-gradient add-btn header-btn"
        @click="showAddDialog = true"
        :disabled="!currentBookId"
      >
        <el-icon><Plus /></el-icon>
        <span class="btn-label">添加单词</span>
      </el-button>
    </template>

    <SectionSubNav :tabs="wordsTabs" aria-label="单词栏目" />

    <!-- 书架：实体书本选择 -->
    <section class="bookshelf le-paper-flip">
      <div class="shelf-header">
        <span class="sheet-label">BOOKSHELF · 我的书架</span>
        <div class="shelf-tools">
          <el-button size="small" @click="openCreateBook">
            <el-icon><FolderAdd /></el-icon>
            新建词本
          </el-button>
          <el-button
            size="small"
            :disabled="!currentBook || isFavoritesBook"
            @click="openRenameBook"
          >重命名</el-button>
          <el-button
            size="small"
            :disabled="!currentBookId || isFavoritesBook"
            @click="openImportDialog"
          >导入</el-button>
          <el-button
            size="small"
            :disabled="!currentBookId || isFavoritesBook"
            @click="openShareDialog"
          >分享</el-button>
          <el-button size="small" @click="openClaimDialog">用分享码导入</el-button>
          <el-button
            size="small"
            type="danger"
            plain
            :disabled="!currentBook || isFavoritesBook"
            @click="handleDeleteBook"
          >
            删除
          </el-button>
        </div>
      </div>

      <div class="shelf-rail">
        <button
          v-for="(book, index) in wordBooks"
          :key="book.id"
          type="button"
          class="vocab-book le-paper-flip"
          :class="[
            book.isFavorites ? 'tone-fav' : `tone-${index % 5}`,
            `le-paper-flip-delay-${Math.min((index % 4) + 1, 5)}`,
            { active: book.id === currentBookId },
          ]"
          @click="selectBook(book.id)"
        >
          <span class="vocab-book__spine" aria-hidden="true"></span>
          <span class="vocab-book__edge" aria-hidden="true"></span>
          <div class="vocab-book__cover">
            <span class="vocab-book__badge">{{
              book.isFavorites ? 'FAV' : (book.isDefault ? 'DEFAULT' : 'MINE')
            }}</span>
            <h3 class="vocab-book__title">{{ book.name }}</h3>
            <p class="vocab-book__desc">{{
              book.description || (book.isFavorites ? '复习时点星加入' : '个人词汇本')
            }}</p>
            <div class="vocab-book__foot">
              <strong>{{ book.wordCount ?? 0 }}</strong>
              <span>词</span>
            </div>
          </div>
        </button>

        <button type="button" class="vocab-book vocab-book--add" @click="openCreateBook">
          <span class="vocab-book__spine" aria-hidden="true"></span>
          <div class="vocab-book__cover vocab-book__cover--add">
            <el-icon :size="28"><Plus /></el-icon>
            <span>放一本新书</span>
          </div>
        </button>
      </div>
      <p class="shelf-hint mobile-only-inline">左右滑动切换词本</p>
      <div class="shelf-plank" aria-hidden="true"></div>

      <!-- 官方词本同一页书架 -->
      <div class="shelf-header shelf-header--official">
        <span class="sheet-label">OFFICIAL · 官方词本</span>
        <el-button text type="primary" size="small" @click="$router.push({ name: 'wordPacks' })">
          全部官方词本 →
        </el-button>
      </div>

      <div v-if="packsLoading" class="official-loading">加载官方词本…</div>
      <el-empty
        v-else-if="!officialPacks.length"
        description="暂无已发布的官方词本"
        :image-size="56"
      />
      <div v-else class="shelf-rail shelf-rail--official">
        <div
          v-for="(pack, index) in officialPacks"
          :key="pack.id"
          class="vocab-book vocab-book--official le-paper-flip"
          :class="[
            `tone-${packTone(pack, index)}`,
            `le-paper-flip-delay-${Math.min((index % 4) + 1, 5)}`,
            {
              active: pack.claimed && pack.userWordBookId === currentBookId,
              claimed: pack.claimed,
            },
          ]"
        >
          <span class="vocab-book__spine" aria-hidden="true"></span>
          <span class="vocab-book__edge" aria-hidden="true"></span>
          <div class="vocab-book__cover">
            <div class="vocab-book__top">
              <span class="vocab-book__badge">{{ packCategoryLabel(pack.category) }}</span>
              <span v-if="pack.claimed" class="claimed-pill">已领</span>
            </div>
            <h3 class="vocab-book__title">{{ pack.name }}</h3>
            <p class="vocab-book__desc">{{ pack.description || '官方高频词，领取后可复习' }}</p>
            <div class="vocab-book__foot">
              <strong>{{ pack.wordCount ?? 0 }}</strong>
              <span>词</span>
            </div>
            <div class="vocab-book__actions">
              <button
                v-if="pack.claimed && pack.userWordBookId"
                type="button"
                class="ghost-btn"
                @click="selectBook(pack.userWordBookId)"
              >
                打开
              </button>
              <button
                type="button"
                class="solid-btn"
                :disabled="claimingId === pack.id"
                @click="claimPack(pack)"
              >
                {{
                  claimingId === pack.id
                    ? '领取中…'
                    : pack.claimed
                      ? '同步'
                      : '领取'
                }}
              </button>
            </div>
          </div>
        </div>
      </div>
      <div class="shelf-plank" aria-hidden="true"></div>
    </section>

    <!-- 打开的书：当前词本内容 -->
    <section v-if="currentBook" class="open-book le-paper le-paper--ruled le-paper--margin le-paper-flip le-paper-flip-delay-1">
      <div class="open-book__head">
        <div>
          <p class="sheet-label">OPEN BOOK · 正在阅读</p>
          <h2 class="open-book__title">{{ currentBook.name }}</h2>
          <p v-if="currentBook.description" class="open-book__subtitle">{{ currentBook.description }}</p>
        </div>
        <div class="toolbar-actions">
          <el-button
            v-if="stats?.dueCount > 0"
            type="primary"
            class="le-btn-gradient"
            @click="goToReview"
          >
            <el-icon><Timer /></el-icon>
            开始复习 ({{ stats.dueCount }})
          </el-button>
          <el-button v-if="stats?.totalWords > 0" @click="goToFreeReview">
            <el-icon><Refresh /></el-icon>
            自由复习
          </el-button>
        </div>
      </div>

      <div v-if="stats" class="le-stats-row open-book__stats">
        <div class="stat-tile">
          <div class="stat-value">{{ stats.totalWords }}</div>
          <div class="stat-label">总单词数</div>
        </div>
        <div class="stat-tile">
          <div class="stat-value is-warning">{{ stats.dueCount }}</div>
          <div class="stat-label">待复习</div>
        </div>
        <div class="stat-tile">
          <div class="stat-value is-success">{{ stats.masteredCount }}</div>
          <div class="stat-label">已掌握</div>
        </div>
        <div class="stat-tile">
          <div class="stat-value is-primary">{{ stats.reviewLogsCount }}</div>
          <div class="stat-label">复习次数</div>
        </div>
      </div>

      <div class="search-bar">
        <el-input
          v-model="searchKeyword"
          placeholder="在这本书里搜索单词、释义…"
          clearable
          class="search-input"
          @keyup.enter="handleSearch"
          @clear="handleSearch"
        >
          <template #append>
            <el-button @click="handleSearch">
              <el-icon><Search /></el-icon>
            </el-button>
          </template>
        </el-input>
        <div class="mark-filter" role="group" aria-label="按复习标记筛选">
          <button
            v-for="opt in reviewMarkOptions"
            :key="opt.value || 'all'"
            type="button"
            class="mark-chip"
            :class="[{ active: reviewMark === opt.value }, opt.cls]"
            @click="setReviewMark(opt.value)"
          >{{ opt.label }}</button>
        </div>
        <div class="batch-bar">
          <el-button size="small" @click="toggleSelectMode">
            {{ selectMode ? '取消多选' : '批量删除' }}
          </el-button>
          <template v-if="selectMode">
            <el-button size="small" @click="toggleSelectPage">
              {{ isPageAllSelected ? '取消本页' : '全选本页' }}
            </el-button>
            <el-button
              size="small"
              type="danger"
              :disabled="!selectedIds.length"
              :loading="batchDeleting"
              @click="batchDeleteSelected"
            >
              删除所选 ({{ selectedIds.length }})
            </el-button>
          </template>
        </div>
      </div>

      <div class="table-sheet desktop-only" v-loading="loading">
        <el-table
          ref="wordTableRef"
          :data="wordList"
          class="words-table"
          style="width: 100%"
          @selection-change="onTableSelectionChange"
        >
          <el-table-column v-if="selectMode" type="selection" width="44" />
          <el-table-column prop="word" label="单词" width="150">
            <template #default="{ row }">
              <span class="word-text">{{ row.word }}</span>
              <span v-if="row.isFavorite" class="fav-tag" title="已收藏">★</span>
            </template>
          </el-table-column>
          <el-table-column prop="definition" label="释义" min-width="200">
            <template #default="{ row }">
              <span class="definition-text">{{ row.definition || '暂无释义' }}</span>
            </template>
          </el-table-column>
          <el-table-column prop="example" label="例句" min-width="250">
            <template #default="{ row }">
              <span class="example-text">{{ row.example || '暂无例句' }}</span>
            </template>
          </el-table-column>
          <el-table-column prop="nextReview" label="复习" width="110">
            <template #default="{ row }">
              <span class="status-pill" :class="getReviewStatus(row).cls">
                {{ getReviewStatus(row).text }}
              </span>
            </template>
          </el-table-column>
          <el-table-column label="操作" width="72" align="center">
            <template #default="{ row }">
              <button type="button" class="icon-delete" @click="deleteWord(row.id)" title="删除">
                <el-icon :size="16"><Delete /></el-icon>
              </button>
            </template>
          </el-table-column>
        </el-table>
        <el-empty v-if="!loading && !wordList.length" description="这本书还是空白的，点右上角加词，或去领取官方词本" :image-size="72" />
      </div>

      <div v-loading="loading" class="word-cards mobile-only">
        <el-empty v-if="!loading && !wordList.length" description="这本书还是空白的" :image-size="72" />
        <article
          v-for="(row, index) in wordList"
          :key="row.id"
          class="word-card le-paper le-paper-flip"
          :class="[
            `le-paper-flip-delay-${Math.min((index % 4) + 1, 5)}`,
            { 'is-selected': selectMode && selectedIdSet.has(row.id) },
          ]"
          @click="selectMode ? toggleWordSelected(row.id) : undefined"
        >
          <div class="word-card-top">
            <div class="word-card-title">
              <el-checkbox
                v-if="selectMode"
                :model-value="selectedIdSet.has(row.id)"
                @click.stop
                @change="toggleWordSelected(row.id)"
              />
              <span class="word-text">{{ row.word }}</span>
              <span v-if="row.isFavorite" class="fav-tag" title="已收藏">★</span>
              <span class="status-pill" :class="getReviewStatus(row).cls">
                {{ getReviewStatus(row).text }}
              </span>
            </div>
            <button
              v-if="!selectMode"
              type="button"
              class="icon-delete"
              @click="deleteWord(row.id)"
              title="删除"
            >
              <el-icon :size="18"><Delete /></el-icon>
            </button>
          </div>
          <p class="word-card-def">{{ row.definition || '暂无释义' }}</p>
          <p v-if="row.example" class="word-card-ex">{{ row.example }}</p>
        </article>
      </div>

      <div class="pagination">
        <el-pagination
          v-model:current-page="page"
          v-model:page-size="pageSize"
          :total="total"
          :page-sizes="[10, 20, 50]"
          layout="total, sizes, prev, pager, next"
          @size-change="handleSizeChange"
          @current-change="handlePageChange"
        />
      </div>
    </section>

    <!-- 添加单词弹窗 -->
    <el-dialog
      v-model="showAddDialog"
      title="添加单词"
      width="500px"
      class="add-word-dialog"
    >
      <el-form :model="newWord" label-width="80px">
        <el-form-item label="单词本">
          <el-select v-model="newWord.wordBookId" style="width: 100%">
            <el-option
              v-for="book in realWordBooks"
              :key="book.id"
              :label="book.name"
              :value="book.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="单词" required>
          <el-input v-model="newWord.word" placeholder="输入单词" />
        </el-form-item>
        <el-form-item label="释义">
          <el-input
            v-model="newWord.definition"
            type="textarea"
            rows="2"
            placeholder="输入释义"
          />
        </el-form-item>
        <el-form-item label="例句">
          <el-input
            v-model="newWord.example"
            type="textarea"
            rows="3"
            placeholder="输入例句"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showAddDialog = false">取消</el-button>
        <el-button type="primary" class="le-btn-gradient" @click="addWord">添加</el-button>
      </template>
    </el-dialog>

    <!-- 新建 / 重命名单词本 -->
    <el-dialog
      v-model="showBookDialog"
      :title="bookDialogMode === 'create' ? '新建单词本' : '重命名单词本'"
      width="420px"
    >
      <el-form label-width="80px">
        <el-form-item label="名称" required>
          <el-input
            v-model="bookForm.name"
            maxlength="50"
            show-word-limit
            placeholder="例如：四级词汇"
          />
        </el-form-item>
        <el-form-item label="备注">
          <el-input
            v-model="bookForm.description"
            type="textarea"
            rows="2"
            maxlength="200"
            placeholder="可选"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showBookDialog = false">取消</el-button>
        <el-button type="primary" class="le-btn-gradient" @click="submitBookForm">确定</el-button>
      </template>
    </el-dialog>

    <!-- 批量导入 -->
    <el-dialog v-model="showImportDialog" title="一键导入单词" width="620px">
      <div class="import-guide">
        <p class="import-guide__title">导入格式说明（每次导入请先对照）</p>
        <div
          v-for="(item, idx) in importFormatGuide"
          :key="idx"
          class="import-guide__item"
          :class="{ 'is-warn': idx === importFormatGuide.length - 1 }"
        >
          <strong>{{ item.title }}</strong>
          <pre class="import-guide__example">{{ item.example }}</pre>
          <p>{{ item.note }}</p>
        </div>
        <p class="io-hint">也支持 PDF 词表、JSON（words/entries 数组）。</p>
      </div>
      <el-form label-width="88px">
        <el-form-item label="导入到">
          <el-radio-group v-model="importTargetMode" style="width: 100%; margin-bottom: 8px">
            <el-radio-button value="existing">已有词本</el-radio-button>
            <el-radio-button value="new">新建词本</el-radio-button>
          </el-radio-group>
          <el-select
            v-if="importTargetMode === 'existing'"
            v-model="importBookId"
            style="width: 100%"
            placeholder="选择目标词本"
          >
            <el-option
              v-for="book in realWordBooks"
              :key="book.id"
              :label="book.name"
              :value="book.id"
            />
          </el-select>
          <el-input
            v-else
            v-model="importNewBookName"
            maxlength="40"
            placeholder="新词本名称，如：四级核心词"
          />
        </el-form-item>
        <el-form-item label="选择文件">
          <input
            ref="importFileInput"
            type="file"
            accept=".json,.txt,.pdf,.docx,application/json,text/plain,application/pdf,application/vnd.openxmlformats-officedocument.wordprocessingml.document"
            @change="onImportFileSelected"
          />
        </el-form-item>
        <el-form-item label="或粘贴">
          <el-input
            v-model="importPaste"
            type="textarea"
            :rows="8"
            placeholder="推荐：abandon | 放弃&#10;格式乱也没关系，可点「AI 整理」"
          />
        </el-form-item>
      </el-form>
      <p class="io-hint">
        格式搞不定时：先粘贴/选文件，再点「AI 整理」，确认预览后导入。需登录；每位用户每天限用 1 次；音标与词性会写入释义。
      </p>
      <p v-if="importPreviewCount != null" class="io-hint">
        已解析 {{ importPreviewCount }} 个词条
      </p>
      <template #footer>
        <el-button @click="showImportDialog = false">取消</el-button>
        <el-button :loading="aiOrganizeBusy" :disabled="importBusy" @click="runAiOrganize">
          AI 整理
        </el-button>
        <el-button
          type="primary"
          class="le-btn-gradient"
          :loading="importBusy"
          :disabled="aiOrganizeBusy"
          @click="submitImport"
        >
          确认导入
        </el-button>
      </template>
    </el-dialog>

    <!-- 分享 -->
    <el-dialog v-model="showShareDialog" title="分享单词本" width="480px">
      <p class="io-hint">生成分享码发给好友，对方点「用分享码导入」即可导入；也可导出文件自行发送。</p>
      <div v-if="shareResult" class="share-result">
        <div class="share-code">{{ shareResult.code }}</div>
        <p>
          《{{ shareResult.title }}》· {{ shareResult.wordCount }} 词 ·
          有效至 {{ formatDate(shareResult.expiresAt) }}
        </p>
        <el-button type="primary" class="le-btn-gradient" @click="copyShareCode">复制分享码</el-button>
      </div>
      <div class="share-actions">
        <el-button :loading="shareBusy" :disabled="!currentBookId" @click="generateShareCode">
          {{ shareResult ? '重新生成分享码' : '生成分享码' }}
        </el-button>
        <el-button :disabled="!currentBookId || exportBusy" @click="exportCurrentBook('json')">
          导出 JSON
        </el-button>
        <el-button :disabled="!currentBookId || exportBusy" @click="exportCurrentBook('txt')">
          导出 TXT
        </el-button>
      </div>
      <template #footer>
        <el-button @click="showShareDialog = false">关闭</el-button>
      </template>
    </el-dialog>

    <!-- 用分享码导入 -->
    <el-dialog v-model="showClaimDialog" title="用分享码导入词本" width="480px">
      <el-form label-width="88px">
        <el-form-item label="分享码" required>
          <el-input
            v-model="claimCode"
            maxlength="16"
            placeholder="输入 6 位分享码"
            @keyup.enter="previewClaim"
          />
        </el-form-item>
      </el-form>
      <div v-if="claimPreview" class="claim-preview">
        <p>
          <strong>{{ claimPreview.title }}</strong>
          · {{ claimPreview.wordCount }} 词
          · 有效至 {{ formatDate(claimPreview.expiresAt) }}
        </p>
        <ul v-if="claimPreview.preview?.length" class="claim-preview-list">
          <li v-for="(w, i) in claimPreview.preview.slice(0, 8)" :key="i">
            {{ w.word }}
            <span v-if="w.definition"> — {{ w.definition }}</span>
          </li>
        </ul>
      </div>
      <template #footer>
        <el-button @click="showClaimDialog = false">取消</el-button>
        <el-button @click="previewClaim" :loading="claimBusy">预览</el-button>
        <el-button
          type="primary"
          class="le-btn-gradient"
          :loading="claimBusy"
          :disabled="!claimPreview"
          @click="submitClaim"
        >
          领取到新词本
        </el-button>
      </template>
    </el-dialog>
  </PageShell>
</template>

<script>
export default { name: 'MyWordsView' }
</script>

<script setup>
import { ref, computed, onMounted, onActivated, watch, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import PageShell from '../components/PageShell.vue'
import SectionSubNav from '../components/SectionSubNav.vue'
import { wordsTabs } from '../utils/sectionTabs'
import { promptGoReviewAfterAdd } from '../utils/promptGoReview.js'
import {
  getUserWords,
  addUserWord,
  importUserWords,
  aiOrganizeUserWords,
  deleteUserWord,
  batchDeleteUserWords,
  getWordStats,
  getWordBooks,
  createWordBook,
  updateWordBook,
  deleteWordBook,
  getCurrentWordBookId,
  setCurrentWordBookId,
  isFavoritesBookId,
  makeFavoritesBook,
  getOfficialWordPacks,
  claimOfficialWordPack,
  shareWordBook,
  getSharedWordBook,
  claimSharedWordBook,
} from '../api/Word.js'
import {
  parseWordFile,
  parseImportFile,
  serializeToJson,
  serializeToTxt,
  IMPORT_FORMAT_GUIDE,
} from '../utils/wordImportParse.js'

const router = useRouter()

const wordList = ref([])
const wordBooks = ref([])
const officialPacks = ref([])
const packsLoading = ref(false)
const claimingId = ref(null)
const currentBookId = ref(null)
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)
const searchKeyword = ref('')
const reviewMark = ref('')
const reviewMarkOptions = [
  { value: '', label: '全部', cls: '' },
  { value: 'forget', label: '忘记', cls: 'forget' },
  { value: 'fuzzy', label: '模糊', cls: 'fuzzy' },
  { value: 'remember', label: '记住', cls: 'remember' },
  { value: 'unreviewed', label: '未复习', cls: 'unreviewed' },
  { value: 'favorite', label: '收藏', cls: 'favorite' },
]
const loading = ref(false)
const stats = ref(null)
const showAddDialog = ref(false)
const newWord = ref({ word: '', definition: '', example: '', wordBookId: null })

const showBookDialog = ref(false)
const bookDialogMode = ref('create')
const bookForm = ref({ name: '', description: '' })

const showImportDialog = ref(false)
const importBookId = ref(null)
const importTargetMode = ref('existing')
const importNewBookName = ref('')
const importPaste = ref('')
const importFileInput = ref(null)
const importBusy = ref(false)
const aiOrganizeBusy = ref(false)
const importPreviewCount = ref(null)
const importFormatGuide = IMPORT_FORMAT_GUIDE
const pendingImportWords = ref(null)

const selectMode = ref(false)
const selectedIds = ref([])
const batchDeleting = ref(false)
const wordTableRef = ref(null)
const selectedIdSet = computed(() => new Set(selectedIds.value))
const isPageAllSelected = computed(() => {
  if (!wordList.value.length) return false
  return wordList.value.every((w) => selectedIdSet.value.has(w.id))
})

const showShareDialog = ref(false)
const shareBusy = ref(false)
const exportBusy = ref(false)
const shareResult = ref(null)

const showClaimDialog = ref(false)
const claimCode = ref('')
const claimPreview = ref(null)
const claimBusy = ref(false)

const currentBook = computed(() =>
  wordBooks.value.find(b => b.id === currentBookId.value) || null
)
const isFavoritesBook = computed(() => isFavoritesBookId(currentBookId.value))
const realWordBooks = computed(() => wordBooks.value.filter((b) => !b.isFavorites))

const attachFavoritesBook = async (books) => {
  let favoriteCount = 0
  try {
    const stats = await getWordStats({ favoritesOnly: true })
    favoriteCount = stats?.totalWords ?? 0
  } catch {
    favoriteCount = 0
  }
  return [makeFavoritesBook(favoriteCount), ...(books || [])]
}

const loadBooks = async () => {
  const books = await getWordBooks()
  const list = await attachFavoritesBook(books)
  wordBooks.value = list
  if (!list.length) return

  const saved = getCurrentWordBookId()
  const match = list.find((b) => b.id === saved)
  currentBookId.value =
    match?.id || list.find((b) => b.isDefault)?.id || list.find((b) => !b.isFavorites)?.id || list[0].id
  setCurrentWordBookId(currentBookId.value)
  newWord.value.wordBookId = isFavoritesBookId(currentBookId.value)
    ? realWordBooks.value[0]?.id || null
    : currentBookId.value
}

const loadWords = async () => {
  if (!currentBookId.value) return
  loading.value = true
  try {
    const res = await getUserWords({
      page: page.value,
      pageSize: pageSize.value,
      search: searchKeyword.value,
      wordBookId: currentBookId.value,
      reviewMark: reviewMark.value || undefined,
    })
    wordList.value = res.items || []
    total.value = res.total || 0
  } catch (e) {
    console.error('获取单词失败', e)
  } finally {
    loading.value = false
  }
}

const loadStats = async () => {
  if (!currentBookId.value) return
  try {
    stats.value = await getWordStats({ wordBookId: currentBookId.value })
  } catch (e) {
    console.error('获取统计失败', e)
  }
}

const refreshBookWordCounts = async () => {
  try {
    const books = await getWordBooks()
    wordBooks.value = await attachFavoritesBook(books)
  } catch (e) {
    console.error(e)
  }
}

const onBookChange = (id) => {
  setCurrentWordBookId(id)
  newWord.value.wordBookId = isFavoritesBookId(id)
    ? realWordBooks.value[0]?.id || null
    : id
  page.value = 1
  reviewMark.value = ''
  clearSelection()
  selectMode.value = false
  loadWords()
  loadStats()
}

const selectBook = (id) => {
  if (currentBookId.value === id) return
  currentBookId.value = id
  onBookChange(id)
}

const packCategoryLabel = (c) =>
  ({ cet4: '四级', cet6: '六级', kaoyan: '考研', other: '官方' }[c] || '官方')

const packTone = (pack, index) => {
  if (pack.category === 'cet4') return 0
  if (pack.category === 'cet6') return 1
  if (pack.category === 'kaoyan') return 2
  return index % 5
}

const loadOfficialPacks = async () => {
  packsLoading.value = true
  try {
    officialPacks.value = (await getOfficialWordPacks()) || []
  } catch (e) {
    console.error('加载官方词本失败', e)
    officialPacks.value = []
  } finally {
    packsLoading.value = false
  }
}

const claimPack = async (pack) => {
  try {
    await ElMessageBox.confirm(
      `将「${pack.name}」复制到你的书架（已有单词会跳过）。词量较大时可能需要几秒。`,
      pack.claimed ? '同步官方词本' : '领取官方词本',
      { type: 'info', confirmButtonText: pack.claimed ? '开始同步' : '开始领取', cancelButtonText: '取消' }
    )
  } catch {
    return
  }

  claimingId.value = pack.id
  try {
    const res = await claimOfficialWordPack(pack.id)
    ElMessage.success(
      `完成：新增 ${res.added} 词，本中共 ${res.totalInBook} 词` +
        (res.remaining > 0 ? `（还可同步 ${res.remaining}）` : '')
    )
    await Promise.all([loadBooks(), loadOfficialPacks()])
    if (res?.userWordBookId) {
      selectBook(res.userWordBookId)
    }
  } catch (e) {
    console.error(e)
  } finally {
    claimingId.value = null
  }
}

const getReviewStatus = (word) => {
  const q = word?.lastQuality
  if (q === 0) return { type: 'danger', text: '忘记', cls: 'is-forget' }
  if (q === 3) return { type: 'warning', text: '模糊', cls: 'is-fuzzy' }
  if (q === 5) return { type: 'success', text: '记住', cls: 'is-remember' }
  if (!word.nextReview) {
    return { type: 'info', text: '新词', cls: 'is-new' }
  }
  const now = new Date()
  const next = new Date(word.nextReview)
  if (next <= now) {
    return { type: 'warning', text: '待复习', cls: 'is-due' }
  }
  if (word.interval >= 21) {
    return { type: 'success', text: '已掌握', cls: 'is-mastered' }
  }
  return { type: 'primary', text: '复习中', cls: 'is-learning' }
}

const addWord = async () => {
  if (!newWord.value.word.trim()) {
    ElMessage.warning('请输入单词')
    return
  }
  try {
    await addUserWord({
      ...newWord.value,
      wordBookId: newWord.value.wordBookId || currentBookId.value
    })
    const addedWord = newWord.value.word.trim()
    showAddDialog.value = false
    newWord.value = {
      word: '',
      definition: '',
      example: '',
      wordBookId: currentBookId.value
    }
    await refreshBookWordCounts()
    loadWords()
    loadStats()
    await promptGoReviewAfterAdd(router, addedWord, '/my-words')
  } catch (e) {
    console.error(e)
  }
}

const clearSelection = () => {
  selectedIds.value = []
  wordTableRef.value?.clearSelection?.()
}

const toggleSelectMode = () => {
  selectMode.value = !selectMode.value
  clearSelection()
}

const toggleWordSelected = (id) => {
  const set = new Set(selectedIds.value)
  if (set.has(id)) set.delete(id)
  else set.add(id)
  selectedIds.value = [...set]
}

const toggleSelectPage = () => {
  if (isPageAllSelected.value) {
    const pageIds = new Set(wordList.value.map((w) => w.id))
    selectedIds.value = selectedIds.value.filter((id) => !pageIds.has(id))
  } else {
    const set = new Set(selectedIds.value)
    wordList.value.forEach((w) => set.add(w.id))
    selectedIds.value = [...set]
  }
  syncTableSelection()
}

const onTableSelectionChange = (rows) => {
  if (!selectMode.value) return
  const pageIds = new Set(wordList.value.map((w) => w.id))
  const kept = selectedIds.value.filter((id) => !pageIds.has(id))
  selectedIds.value = [...kept, ...rows.map((r) => r.id)]
}

const syncTableSelection = async () => {
  await nextTick()
  const table = wordTableRef.value
  if (!table || !selectMode.value) return
  table.clearSelection?.()
  wordList.value.forEach((row) => {
    if (selectedIdSet.value.has(row.id)) table.toggleRowSelection?.(row, true)
  })
}

const deleteWord = async (id) => {
  try {
    await ElMessageBox.confirm('确定要删除这个单词吗？', '提示', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    })
    await deleteUserWord(id)
    selectedIds.value = selectedIds.value.filter((x) => x !== id)
    ElMessage.success('删除成功')
    await refreshBookWordCounts()
    loadWords()
    loadStats()
  } catch (e) {
    if (e !== 'cancel') {
      console.error(e)
    }
  }
}

const batchDeleteSelected = async () => {
  const ids = [...selectedIds.value]
  if (!ids.length) return
  try {
    await ElMessageBox.confirm(
      `确定删除选中的 ${ids.length} 个单词吗？此操作不可恢复。`,
      '批量删除',
      {
        confirmButtonText: '删除',
        cancelButtonText: '取消',
        type: 'warning',
      }
    )
  } catch {
    return
  }

  batchDeleting.value = true
  try {
    const res = await batchDeleteUserWords(ids)
    ElMessage.success(`已删除 ${res?.deleted ?? ids.length} 个单词`)
    clearSelection()
    selectMode.value = false
    await refreshBookWordCounts()
    loadWords()
    loadStats()
  } catch (e) {
    console.error(e)
  } finally {
    batchDeleting.value = false
  }
}

const openCreateBook = () => {
  bookDialogMode.value = 'create'
  bookForm.value = { name: '', description: '' }
  showBookDialog.value = true
}

const openRenameBook = () => {
  if (!currentBook.value || currentBook.value.isFavorites) return
  bookDialogMode.value = 'rename'
  bookForm.value = {
    name: currentBook.value.name,
    description: currentBook.value.description || ''
  }
  showBookDialog.value = true
}

const submitBookForm = async () => {
  const name = bookForm.value.name?.trim()
  if (!name) {
    ElMessage.warning('请输入单词本名称')
    return
  }
  try {
    if (bookDialogMode.value === 'create') {
      const book = await createWordBook({
        name,
        description: bookForm.value.description
      })
      ElMessage.success('单词本已创建')
      await loadBooks()
      currentBookId.value = book.id
      onBookChange(book.id)
    } else {
      await updateWordBook(currentBookId.value, {
        name,
        description: bookForm.value.description
      })
      ElMessage.success('已更新')
      await refreshBookWordCounts()
    }
    showBookDialog.value = false
  } catch (e) {
    console.error(e)
  }
}

const handleDeleteBook = async () => {
  if (!currentBook.value || currentBook.value.isFavorites) return

  if (currentBook.value.isDefault) {
    try {
      await ElMessageBox.confirm(
        `删除默认单词本「${currentBook.value.name}」？其中的单词将一并删除，并自动新建一本空白默认单词本。`,
        '删除默认单词本',
        { confirmButtonText: '删除并重建', cancelButtonText: '取消', type: 'warning' }
      )
      await deleteWordBook(currentBook.value.id, { moveWordsToDefault: false, deleteWords: true })
      ElMessage.success('已删除，并新建空白默认单词本')
      clearSelection()
      selectMode.value = false
      await loadBooks()
      onBookChange(currentBookId.value)
    } catch (e) {
      if (e !== 'cancel') console.error(e)
    }
    return
  }

  try {
    await ElMessageBox.confirm(
      `删除「${currentBook.value.name}」后，词本里的单词如何处理？`,
      '删除单词本',
      {
        distinguishCancelAndClose: true,
        confirmButtonText: '迁移到默认本',
        cancelButtonText: '连同单词删除',
        type: 'warning',
      }
    )
    await deleteWordBook(currentBook.value.id, { moveWordsToDefault: true, deleteWords: false })
    ElMessage.success('词本已删除，单词已移到默认本')
    clearSelection()
    selectMode.value = false
    await loadBooks()
    onBookChange(currentBookId.value)
  } catch (action) {
    if (action === 'cancel') {
      try {
        await ElMessageBox.confirm(
          `将永久删除「${currentBook.value.name}」及其全部单词，不可恢复。确定吗？`,
          '连同单词删除',
          { confirmButtonText: '永久删除', cancelButtonText: '返回', type: 'error' }
        )
        await deleteWordBook(currentBook.value.id, { moveWordsToDefault: false, deleteWords: true })
        ElMessage.success('词本及单词已删除')
        clearSelection()
        selectMode.value = false
        await loadBooks()
        onBookChange(currentBookId.value)
      } catch (e) {
        if (e !== 'cancel') console.error(e)
      }
    } else if (action !== 'close') {
      console.error(action)
    }
  }
}

const formatDate = (iso) => {
  if (!iso) return ''
  try {
    return new Date(iso).toLocaleDateString('zh-CN')
  } catch {
    return String(iso)
  }
}

const serializeWordsForAi = (words) =>
  (words || [])
    .map((w) => {
      const word = String(w?.word || '').trim()
      if (!word) return ''
      const def = String(w?.definition || '').trim()
      const ex = String(w?.example || '').trim()
      if (ex) return `${word} | ${def} | ${ex}`
      if (def) return `${word} | ${def}`
      return word
    })
    .filter(Boolean)
    .join('\n')

const openImportDialog = () => {
  importBookId.value = currentBookId.value
  importTargetMode.value = 'existing'
  importNewBookName.value = ''
  importPaste.value = ''
  importPreviewCount.value = null
  pendingImportWords.value = null
  showImportDialog.value = true
}

const fillImportPreview = (words, label = '解析') => {
  pendingImportWords.value = words
  importPreviewCount.value = words.length
  importPaste.value = words
    .slice(0, 100)
    .map((w) => {
      const def = w.definition || ''
      const ex = w.example || ''
      if (ex) return `${w.word} | ${def} | ${ex}`
      if (def) return `${w.word} | ${def}`
      return w.word
    })
    .join('\n')
  if (words.length > 100) {
    importPaste.value += `\n# …共 ${words.length} 词，点导入将导入全部${label}结果（AI 整理也会使用全部）`
  }
}

const onImportFileSelected = async (e) => {
  const file = e.target?.files?.[0]
  if (!file) return
  importBusy.value = true
  try {
    const words = await parseImportFile(file)
    const lower = (file.name || '').toLowerCase()
    if (lower.endsWith('.pdf') || lower.endsWith('.docx') || lower.endsWith('.json')) {
      fillImportPreview(words)
    } else {
      importPreviewCount.value = words.length
      const { readFileAsText } = await import('../utils/wordImportParse.js')
      importPaste.value = await readFileAsText(file)
      pendingImportWords.value = words
    }
    ElMessage.success(`已读取文件，解析到 ${words.length} 个词`)
  } catch (err) {
    pendingImportWords.value = null
    ElMessage.error(err?.message || '文件解析失败')
  } finally {
    importBusy.value = false
    if (importFileInput.value) importFileInput.value.value = ''
  }
}

const runAiOrganize = async () => {
  const expected = pendingImportWords.value?.length || 0
  const text = pendingImportWords.value?.length
    ? serializeWordsForAi(pendingImportWords.value)
    : (importPaste.value || '').trim()
  if (!text) {
    ElMessage.warning('请先粘贴内容或选择文件')
    return
  }
  aiOrganizeBusy.value = true
  try {
    const res = await aiOrganizeUserWords(text)
    const entries = res?.entries || []
    if (!entries.length) {
      ElMessage.warning('AI 未整理出有效词条，请检查原文或稍后重试')
      return
    }
    fillImportPreview(entries, 'AI 整理')
    if (expected > 0 && entries.length < expected) {
      ElMessage.success(`AI 已整理 ${entries.length}/${expected} 个词，请确认后导入`)
    } else {
      ElMessage.success(`AI 已整理 ${entries.length} 个词，请确认后导入`)
    }
  } catch (e) {
    console.error(e)
  } finally {
    aiOrganizeBusy.value = false
  }
}

const submitImport = async () => {
  let targetBookId = importBookId.value
  if (importTargetMode.value === 'new') {
    const name = (importNewBookName.value || '').trim()
    if (!name) {
      ElMessage.warning('请填写新建词本名称')
      return
    }
  } else if (!targetBookId) {
    ElMessage.warning('请选择目标词本')
    return
  }

  let words
  try {
    words = pendingImportWords.value?.length
      ? pendingImportWords.value
      : parseWordFile(importPaste.value)
  } catch (err) {
    ElMessage.error(err?.message || '解析失败')
    return
  }
  importBusy.value = true
  try {
    if (importTargetMode.value === 'new') {
      const book = await createWordBook({
        name: (importNewBookName.value || '').trim(),
      })
      targetBookId = book.id || book.Id
    }
    const r = await importUserWords({
      wordBookId: targetBookId,
      entries: words.map((w) => ({
        word: w.word,
        definition: w.definition || '',
        example: w.example || '',
      })),
    })
    ElMessage.success(`导入完成：新增 ${r.added}，跳过 ${r.skipped}${r.failed ? `，失败 ${r.failed}` : ''}`)
    showImportDialog.value = false
    pendingImportWords.value = null
    await loadBooks()
    if (targetBookId === currentBookId.value) {
      await Promise.all([loadWords(), loadStats(), refreshBookWordCounts()])
    } else {
      selectBook(targetBookId)
      await refreshBookWordCounts()
    }
  } catch (e) {
    console.error(e)
  } finally {
    importBusy.value = false
  }
}

const openShareDialog = () => {
  shareResult.value = null
  showShareDialog.value = true
}

const generateShareCode = async () => {
  if (!currentBookId.value) return
  shareBusy.value = true
  try {
    shareResult.value = await shareWordBook(currentBookId.value)
    ElMessage.success('分享码已生成')
  } catch (e) {
    console.error(e)
  } finally {
    shareBusy.value = false
  }
}

const copyShareCode = async () => {
  const code = shareResult.value?.code
  if (!code) return
  try {
    await navigator.clipboard.writeText(code)
    ElMessage.success('已复制分享码')
  } catch {
    ElMessage.info(`分享码：${code}`)
  }
}

const fetchAllWordsInBook = async (wordBookId) => {
  const all = []
  let p = 1
  const size = 200
  for (;;) {
    const res = await getUserWords({ page: p, pageSize: size, wordBookId })
    const items = res.items || []
    all.push(...items)
    if (all.length >= (res.total || 0) || items.length === 0) break
    p += 1
    if (p > 100) break
  }
  return all
}

const downloadText = (filename, content) => {
  const blob = new Blob([content], { type: 'text/plain;charset=utf-8' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  document.body.appendChild(a)
  a.click()
  a.remove()
  URL.revokeObjectURL(url)
}

const exportCurrentBook = async (format) => {
  if (!currentBookId.value) return
  exportBusy.value = true
  try {
    const words = await fetchAllWordsInBook(currentBookId.value)
    if (!words.length) {
      ElMessage.warning('当前词本没有单词可导出')
      return
    }
    const stamp = new Date().toISOString().slice(0, 10)
    const name = `wordbook_${stamp}.${format === 'txt' ? 'txt' : 'json'}`
    const content =
      format === 'txt'
        ? serializeToTxt(words)
        : serializeToJson(words, { source: currentBook.value?.name || 'web' })
    downloadText(name, content)
    ElMessage.success(`已导出 ${words.length} 个单词`)
  } catch (e) {
    console.error(e)
    ElMessage.error('导出失败')
  } finally {
    exportBusy.value = false
  }
}

const openClaimDialog = () => {
  claimCode.value = ''
  claimPreview.value = null
  showClaimDialog.value = true
}

const previewClaim = async () => {
  const code = claimCode.value.trim()
  if (!code) {
    ElMessage.warning('请输入分享码')
    return
  }
  claimBusy.value = true
  try {
    claimPreview.value = await getSharedWordBook(code)
  } catch (e) {
    claimPreview.value = null
    console.error(e)
  } finally {
    claimBusy.value = false
  }
}

const submitClaim = async () => {
  const code = claimCode.value.trim()
  if (!code || !claimPreview.value) return
  claimBusy.value = true
  try {
    const res = await claimSharedWordBook(code)
    ElMessage.success(
      `已领取到「${res.userWordBookName}」：新增 ${res.added}，跳过 ${res.skipped}`
    )
    showClaimDialog.value = false
    await loadBooks()
    if (res?.userWordBookId) selectBook(res.userWordBookId)
  } catch (e) {
    console.error(e)
  } finally {
    claimBusy.value = false
  }
}

const goToReview = () => {
  router.push({ name: 'wordReview', query: { wordBookId: currentBookId.value } })
}

const goToFreeReview = () => {
  router.push({
    name: 'wordReview',
    query: { mode: 'free', wordBookId: currentBookId.value }
  })
}

const handleSearch = () => {
  page.value = 1
  clearSelection()
  loadWords()
}

const setReviewMark = (value) => {
  reviewMark.value = reviewMark.value === value ? '' : value
  page.value = 1
  clearSelection()
  loadWords()
}

const handleSizeChange = (size) => {
  pageSize.value = size
  clearSelection()
  loadWords()
}

const handlePageChange = () => {
  loadWords().then(() => syncTableSelection())
}

watch(selectMode, () => {
  if (selectMode.value) syncTableSelection()
})

watch(wordList, () => {
  if (selectMode.value) syncTableSelection()
})

watch(showAddDialog, (open) => {
  if (open) newWord.value.wordBookId = currentBookId.value
})

const bootstrapped = ref(false)

const bootstrap = async ({ quiet = false } = {}) => {
  try {
    if (!quiet) {
      await loadBooks()
      await Promise.all([loadWords(), loadStats(), loadOfficialPacks()])
    } else if (currentBookId.value) {
      // 返回单词本：静默刷新统计/词数，不打断当前列表
      await Promise.all([loadStats(), refreshBookWordCounts()])
    } else {
      await loadBooks()
      await Promise.all([loadWords(), loadStats(), loadOfficialPacks()])
    }
    bootstrapped.value = true
  } catch (e) {
    console.error(e)
  }
}

onMounted(() => bootstrap({ quiet: false }))
onActivated(() => {
  if (bootstrapped.value) bootstrap({ quiet: true })
})
</script>


<style scoped>
.add-btn { flex-shrink: 0; }

:deep(.le-page-header) { width: 100%; }

.sheet-label {
  font-family: var(--le-font-mono);
  font-size: 11px;
  color: var(--le-primary);
  letter-spacing: 0.06em;
}

.bookshelf { margin-bottom: 22px; }

.shelf-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  flex-wrap: wrap;
  margin-bottom: 14px;
}

.shelf-tools { display: flex; flex-wrap: wrap; gap: 8px; }

.io-hint {
  margin: 0 0 12px;
  font-size: 13px;
  color: var(--le-text-muted, #6b7280);
  line-height: 1.5;
}

.import-guide {
  margin-bottom: 16px;
  padding: 12px 14px;
  border-radius: 10px;
  background: var(--le-surface-soft, #f8fafc);
  border: 1px solid var(--le-border, rgba(55, 75, 105, 0.12));
}
.import-guide__title {
  margin: 0 0 10px;
  font-size: 14px;
  font-weight: 600;
  color: var(--le-text, #1e293b);
}
.import-guide__item {
  margin-bottom: 12px;
  padding-bottom: 10px;
  border-bottom: 1px dashed rgba(55, 75, 105, 0.12);
}
.import-guide__item:last-of-type { border-bottom: none; margin-bottom: 4px; }
.import-guide__item strong {
  display: block;
  font-size: 13px;
  margin-bottom: 4px;
}
.import-guide__item p {
  margin: 4px 0 0;
  font-size: 12px;
  color: var(--le-text-muted, #6b7280);
  line-height: 1.5;
}
.import-guide__item.is-warn {
  background: rgba(220, 38, 38, 0.04);
  border: 1px solid rgba(220, 38, 38, 0.15);
  border-radius: 8px;
  padding: 10px;
}
.import-guide__example {
  margin: 6px 0 0;
  padding: 8px 10px;
  border-radius: 6px;
  background: #fff;
  border: 1px solid rgba(55, 75, 105, 0.1);
  font-family: ui-monospace, SFMono-Regular, Menlo, Consolas, monospace;
  font-size: 12px;
  line-height: 1.45;
  white-space: pre-wrap;
  color: #334155;
}

.batch-bar {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-top: 10px;
}
.word-card.is-selected {
  outline: 2px solid rgba(37, 99, 235, 0.35);
  outline-offset: 1px;
}
.share-result {
  text-align: center;
  margin-bottom: 16px;
  padding: 12px;
  background: var(--le-surface-soft, #f8fafc);
  border-radius: 8px;
}
.share-code {
  font-size: 28px;
  font-weight: 700;
  letter-spacing: 0.2em;
  font-family: ui-monospace, monospace;
  margin-bottom: 8px;
}
.share-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}
.claim-preview {
  margin-top: 8px;
  font-size: 14px;
}
.claim-preview-list {
  margin: 8px 0 0;
  padding-left: 18px;
  color: var(--le-text-muted, #6b7280);
  font-size: 13px;
}

.shelf-rail {
  display: flex;
  gap: 14px;
  overflow-x: auto;
  padding: 8px 4px 0;
  scroll-snap-type: x proximity;
  -webkit-overflow-scrolling: touch;
}

.shelf-rail::-webkit-scrollbar { height: 6px; }
.shelf-rail::-webkit-scrollbar-thumb {
  background: rgba(55, 75, 105, 0.2);
  border-radius: 999px;
}

.shelf-plank {
  height: 14px;
  margin-top: -2px;
  border-radius: 2px 2px 8px 8px;
  background:
    linear-gradient(180deg, rgba(120, 90, 60, 0.35), rgba(90, 65, 40, 0.55)),
    repeating-linear-gradient(90deg, rgba(255,255,255,0.04) 0 2px, transparent 2px 7px);
  box-shadow: 0 8px 16px rgba(30, 41, 59, 0.12), inset 0 1px 0 rgba(255,255,255,0.25);
}

.vocab-book {
  position: relative;
  flex: 0 0 auto;
  width: 148px;
  height: 196px;
  padding: 0;
  border: none;
  background: transparent;
  cursor: pointer;
  text-align: left;
  scroll-snap-align: start;
  transform-origin: left bottom;
  transition: transform 0.2s ease;
  filter: drop-shadow(0 6px 10px rgba(30, 41, 59, 0.12));
}

.vocab-book:hover {
  transform: translateY(-10px) rotate(-1.5deg);
  z-index: 2;
}

.vocab-book.active {
  transform: translateY(-16px) rotate(-2deg) scale(1.03);
  z-index: 3;
  filter: drop-shadow(0 12px 18px rgba(37, 99, 235, 0.22));
}

.vocab-book__spine {
  position: absolute;
  left: 0; top: 4px; bottom: 4px;
  width: 14px;
  border-radius: 3px 0 0 3px;
  background: linear-gradient(90deg, rgba(0,0,0,0.22), rgba(255,255,255,0.08));
  z-index: 2;
}

.vocab-book__edge {
  position: absolute;
  right: -3px; top: 8px; bottom: 8px;
  width: 6px;
  border-radius: 0 2px 2px 0;
  background: repeating-linear-gradient(#f4f1ea 0 2px, #e8e2d6 2px 3px);
  box-shadow: 1px 0 0 rgba(0,0,0,0.06);
  z-index: 1;
}

.vocab-book__cover {
  position: absolute;
  inset: 0;
  padding: 16px 14px 14px 22px;
  border-radius: 4px 10px 10px 4px;
  border: 1px solid rgba(55, 75, 105, 0.16);
  background-color: #f8fafc;
  background-image: var(--le-paper-grain), linear-gradient(145deg, rgba(255,255,255,0.55), transparent 55%);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.vocab-book.tone-0 .vocab-book__cover { background-color: #edf3fb; border-color: rgba(37,99,235,0.22); }
.vocab-book.tone-0 .vocab-book__spine { background: linear-gradient(180deg, #1d4ed8, #2563eb); }
.vocab-book.tone-1 .vocab-book__cover { background-color: #eef8f3; border-color: rgba(5,150,105,0.22); }
.vocab-book.tone-1 .vocab-book__spine { background: linear-gradient(180deg, #047857, #059669); }
.vocab-book.tone-2 .vocab-book__cover { background-color: #f8f3ec; border-color: rgba(180,120,60,0.25); }
.vocab-book.tone-2 .vocab-book__spine { background: linear-gradient(180deg, #9a6b3f, #c4894a); }
.vocab-book.tone-3 .vocab-book__cover { background-color: #f3f1f8; border-color: rgba(91,106,191,0.22); }
.vocab-book.tone-3 .vocab-book__spine { background: linear-gradient(180deg, #4a5aa8, #5b6abf); }
.vocab-book.tone-4 .vocab-book__cover { background-color: #f7f0ef; border-color: rgba(201,120,120,0.28); }
.vocab-book.tone-4 .vocab-book__spine { background: linear-gradient(180deg, #b45a5a, #c97878); }
.vocab-book.tone-fav .vocab-book__cover { background-color: #fff8eb; border-color: rgba(245,158,11,0.35); }
.vocab-book.tone-fav .vocab-book__spine { background: linear-gradient(180deg, #d97706, #f59e0b); }

.vocab-book__badge {
  font-family: var(--le-font-mono);
  font-size: 10px;
  letter-spacing: 0.08em;
  color: var(--le-text-muted);
  margin-bottom: 10px;
}

.vocab-book__title {
  margin: 0;
  font-family: var(--le-font-display);
  font-size: 16px;
  font-weight: 700;
  line-height: 1.3;
  color: var(--le-text);
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.vocab-book__desc {
  margin: 8px 0 0;
  font-size: 12px;
  line-height: 1.4;
  color: var(--le-text-muted);
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  flex: 1;
}

.vocab-book__foot {
  display: flex;
  align-items: baseline;
  gap: 4px;
  margin-top: auto;
  padding-top: 8px;
  border-top: 1px dashed rgba(55, 75, 105, 0.16);
}

.vocab-book__foot strong {
  font-family: var(--le-font-display);
  font-size: 20px;
  color: var(--le-text);
}

.vocab-book__foot span { font-size: 12px; color: var(--le-text-muted); }

.vocab-book--add .vocab-book__spine { background: linear-gradient(180deg, #94a3b8, #64748b); }

.vocab-book__cover--add {
  align-items: center;
  justify-content: center;
  gap: 8px;
  color: var(--le-text-secondary);
  background: repeating-linear-gradient(-45deg, rgba(255,255,255,0.65) 0 8px, rgba(238,242,247,0.9) 8px 16px);
  border-style: dashed;
}

.vocab-book__cover--add span { font-size: 13px; }

.shelf-header--official {
  margin-top: 22px;
}

.official-loading {
  padding: 16px 4px;
  font-size: 13px;
  color: var(--le-text-muted);
}

.vocab-book--official {
  height: 228px;
  width: 156px;
}

.vocab-book__top {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 6px;
  margin-bottom: 8px;
}

.claimed-pill {
  font-size: 11px;
  color: var(--le-success);
  background: rgba(5, 150, 105, 0.1);
  border: 1px solid rgba(5, 150, 105, 0.2);
  border-radius: 4px;
  padding: 1px 6px;
  flex-shrink: 0;
}

.vocab-book__actions {
  display: flex;
  gap: 6px;
  margin-top: 10px;
}

.ghost-btn,
.solid-btn {
  flex: 1;
  height: 28px;
  border-radius: 6px;
  font-size: 12px;
  cursor: pointer;
  border: 1px solid var(--le-border-strong);
  background: rgba(255, 255, 255, 0.8);
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

.open-book {
  position: relative;
  padding: 22px 22px 22px 48px;
  margin-bottom: 8px;
  overflow: hidden;
}

.open-book__head {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 16px;
  flex-wrap: wrap;
  margin-bottom: 16px;
}

.open-book__title {
  margin: 6px 0 0;
  font-family: var(--le-font-display);
  font-size: 26px;
  font-weight: 700;
  color: var(--le-text);
}

.open-book__subtitle {
  margin: 6px 0 0;
  font-size: 13px;
  color: var(--le-text-muted);
}

.open-book__stats { margin-bottom: 14px; }

.stat-tile {
  padding: 14px 12px;
  text-align: center;
  border-radius: 4px 10px 10px 4px;
  background: rgba(255,255,255,0.55);
  border: 1px solid var(--le-border);
}

.stat-value {
  font-family: var(--le-font-display);
  font-size: 26px;
  font-weight: 700;
  color: var(--le-text);
  line-height: 1.2;
}

.stat-value.is-warning { color: var(--le-warning); }
.stat-value.is-success { color: var(--le-success); }
.stat-value.is-primary { color: var(--le-primary); }
.stat-label { margin-top: 4px; font-size: 12px; color: var(--le-text-muted); }

.toolbar-actions { display: flex; flex-wrap: wrap; gap: 8px; justify-content: flex-end; }
.search-bar { margin-bottom: 14px; }
.search-input { width: 100%; }
.search-input { width: min(360px, 100%); }
.search-input :deep(.el-input__wrapper) {
  background: #fff;
  box-shadow: 0 0 0 1px var(--le-border) inset;
}
.search-input :deep(.el-input__wrapper.is-focus) {
  box-shadow: 0 0 0 1px var(--le-primary) inset;
}

.table-sheet {
  overflow: hidden;
  border-radius: 4px 10px 10px 4px;
  min-height: 120px;
  background: rgba(255,255,255,0.72);
  border: 1px solid var(--le-border);
}

.words-table {
  --el-table-bg-color: transparent;
  --el-table-tr-bg-color: transparent;
  --el-table-header-bg-color: rgba(238, 242, 247, 0.65);
  --el-table-row-hover-bg-color: rgba(37, 99, 235, 0.04);
  --el-table-border-color: var(--le-border);
}
.words-table :deep(.el-table__inner-wrapper::before) { display: none; }
.words-table :deep(th.el-table__cell) {
  font-family: var(--le-font-mono);
  font-size: 12px;
  font-weight: 600;
  letter-spacing: 0.04em;
  color: var(--le-text-muted);
}

.word-text {
  font-family: var(--le-font-display);
  font-weight: 700;
  font-size: 15px;
  color: var(--le-text);
  word-break: break-word;
}
.definition-text { color: var(--le-text-secondary); }
.example-text { color: var(--le-text-muted); font-size: 13px; }

.status-pill {
  display: inline-flex;
  align-items: center;
  font-family: var(--le-font-mono);
  font-size: 11px;
  font-weight: 500;
  letter-spacing: 0.03em;
  padding: 3px 8px;
  border-radius: 4px;
  border: 1px solid var(--le-border);
  background: var(--le-bg-muted);
  color: var(--le-text-secondary);
  white-space: nowrap;
}
.status-pill.is-new { color: var(--le-text-muted); }
.status-pill.is-due {
  color: var(--le-warning);
  background: rgba(217, 119, 6, 0.08);
  border-color: rgba(217, 119, 6, 0.22);
}
.status-pill.is-mastered {
  color: var(--le-success);
  background: rgba(5, 150, 105, 0.08);
  border-color: rgba(5, 150, 105, 0.22);
}
.status-pill.is-learning {
  color: var(--le-primary);
  background: rgba(37, 99, 235, 0.08);
  border-color: rgba(37, 99, 235, 0.18);
}
.status-pill.is-forget {
  color: var(--le-danger, #dc2626);
  background: rgba(220, 38, 38, 0.08);
  border-color: rgba(220, 38, 38, 0.22);
}
.status-pill.is-fuzzy {
  color: var(--le-warning);
  background: rgba(217, 119, 6, 0.08);
  border-color: rgba(217, 119, 6, 0.22);
}
.status-pill.is-remember {
  color: var(--le-success);
  background: rgba(5, 150, 105, 0.08);
  border-color: rgba(5, 150, 105, 0.22);
}

.mark-filter {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-top: 10px;
}
.mark-chip {
  padding: 6px 12px;
  border-radius: 999px;
  border: 1px solid var(--le-border);
  background: var(--le-bg, #fff);
  color: var(--le-text-secondary);
  font-size: 13px;
  cursor: pointer;
}
.mark-chip.active {
  border-color: var(--le-primary);
  color: var(--le-primary);
  background: rgba(37, 99, 235, 0.08);
}
.mark-chip.forget.active {
  border-color: var(--le-danger, #dc2626);
  color: var(--le-danger, #dc2626);
  background: rgba(220, 38, 38, 0.08);
}
.mark-chip.fuzzy.active {
  border-color: var(--le-warning);
  color: var(--le-warning);
  background: rgba(217, 119, 6, 0.1);
}
.mark-chip.remember.active {
  border-color: var(--le-success);
  color: var(--le-success);
  background: rgba(5, 150, 105, 0.1);
}
.mark-chip.favorite.active {
  border-color: #f59e0b;
  color: #b45309;
  background: rgba(245, 158, 11, 0.12);
}
.fav-tag {
  margin-left: 4px;
  color: #f59e0b;
  font-size: 13px;
}

.icon-delete {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: 4px;
  border: none;
  background: transparent;
  color: var(--le-danger);
  cursor: pointer;
  border-radius: 6px;
}
.icon-delete:hover { background: rgba(220, 38, 38, 0.08); }

.word-cards { display: none; flex-direction: column; gap: 10px; min-height: 80px; }
.word-card { padding: 14px 14px 12px; border-radius: 4px 10px 10px 4px; }
.word-card-top {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 10px;
  margin-bottom: 8px;
}
.word-card-title {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 8px;
  min-width: 0;
  flex: 1;
}
.word-card-def {
  margin: 0;
  font-size: 14px;
  line-height: 1.55;
  color: var(--le-text-secondary);
  word-break: break-word;
}
.word-card-ex {
  margin: 8px 0 0;
  font-size: 13px;
  line-height: 1.5;
  color: var(--le-text-muted);
  word-break: break-word;
}

.pagination { display: flex; justify-content: center; margin-top: 20px; }
.mobile-only { display: none; }

@media (max-width: 768px) {
  .desktop-only { display: none !important; }
  .mobile-only { display: flex; }
  .mobile-only-inline { display: block; }

  :deep(.le-page-header) {
    flex-wrap: wrap;
    gap: 10px;
  }

  .header-btn {
    padding: 8px 10px;
  }

  .btn-label {
    display: none;
  }

  .shelf-header {
    flex-direction: column;
    align-items: stretch;
    gap: 10px;
  }

  .shelf-tools {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: 6px;
  }

  .shelf-tools .el-button {
    margin: 0;
    width: 100%;
    padding: 8px 4px;
  }

  .shelf-rail {
    gap: 10px;
    padding: 10px 2px 4px;
    margin: 0 -4px;
    padding-left: 4px;
    padding-right: 20px;
    scroll-padding-left: 4px;
    scroll-snap-type: x mandatory;
  }

  .shelf-hint {
    margin: 6px 0 0;
    font-size: 11px;
    color: var(--le-text-muted);
    text-align: center;
  }

  .shelf-plank {
    height: 10px;
  }

  .vocab-book {
    width: 118px;
    height: 158px;
  }

  .vocab-book:hover {
    transform: none;
  }

  .vocab-book.active {
    transform: translateY(-8px);
  }

  .vocab-book__cover {
    padding: 12px 10px 10px 18px;
  }

  .vocab-book__badge {
    margin-bottom: 6px;
    font-size: 9px;
  }

  .vocab-book__title {
    font-size: 14px;
    -webkit-line-clamp: 2;
  }

  .vocab-book__desc {
    display: none;
  }

  .vocab-book__foot {
    padding-top: 6px;
  }

  .vocab-book__foot strong {
    font-size: 18px;
  }

  .vocab-book--official {
    width: calc(50% - 6px);
    height: auto;
    min-height: 200px;
    flex: 0 0 calc(50% - 6px);
  }

  .shelf-rail--official {
    display: flex;
    flex-wrap: wrap;
    overflow: visible;
    gap: 12px 12px;
    padding-right: 4px;
    scroll-snap-type: none;
  }

  .vocab-book--official .vocab-book__desc {
    display: -webkit-box;
    -webkit-line-clamp: 2;
    font-size: 11px;
  }

  .vocab-book--official .vocab-book__actions {
    margin-top: 8px;
  }

  .ghost-btn,
  .solid-btn {
    height: 34px;
    font-size: 13px;
  }

  .shelf-header--official {
    margin-top: 18px;
  }

  .open-book {
    padding: 16px 12px 14px 36px;
    margin-left: -2px;
    margin-right: -2px;
  }

  .open-book__title {
    font-size: 20px;
  }

  .open-book__stats {
    display: grid !important;
    grid-template-columns: 1fr 1fr;
    gap: 8px;
  }

  .open-book__stats .stat-tile {
    margin: 0;
  }

  .stat-value {
    font-size: 22px;
  }

  .toolbar-actions {
    width: 100%;
  }

  .toolbar-actions .el-button {
    flex: 1;
    min-height: 40px;
  }

  .search-input {
    width: 100%;
  }

  .word-card {
    padding: 12px;
  }

  .pagination {
    margin-top: 14px;
  }

  .pagination :deep(.el-pagination) {
    flex-wrap: wrap;
    justify-content: center;
    gap: 4px;
  }
  .pagination :deep(.el-pagination__sizes),
  .pagination :deep(.el-pagination__jump) { display: none; }

  :deep(.el-dialog) {
    width: 92% !important;
    max-height: 80vh;
    margin-top: 8vh !important;
  }
  :deep(.el-dialog__body) {
    padding: 12px 16px;
    max-height: calc(80vh - 120px);
    overflow-y: auto;
  }
}

.mobile-only-inline { display: none; }

@media (hover: none) {
  .vocab-book:hover {
    transform: none;
  }
}
</style>
