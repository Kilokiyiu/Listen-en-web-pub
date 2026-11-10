import axios from "axios";

const showError = (msg) => {
    Promise.all([
        import("element-plus/es/components/message/index.mjs"),
        import("element-plus/es/components/message/style/css.mjs"),
    ]).then(([{ ElMessage }]) => ElMessage.error(msg))
}

// 单词服务请求实例
const wordRequest = axios.create({
    baseURL: "/api/word",
    timeout: 10000,
})

// 添加 token 拦截器
wordRequest.interceptors.request.use(
    config => {
        const token = localStorage.getItem('token');
        const userId = localStorage.getItem('userId');
        if (token) {
            config.headers.Authorization = `Bearer ${token}`
        }
        if (userId) {
            config.headers['X-User-Id'] = userId
        }
        return config;
    },
    error => Promise.reject(error)
)

wordRequest.interceptors.response.use(
    response => response.data,
    error => {
        let msg;
        if (error.response?.status === 401) {
            msg = "请先登录";
        } else if (error.response?.data) {
            msg = error.response.data;
        } else if (error.code === 'ECONNREFUSED') {
            msg = "服务未启动，请稍后重试";
        } else {
            msg = "网络错误，请稍后重试";
        }
        showError(msg)
        if(error.response?.status === 401){
            localStorage.removeItem('token');
            localStorage.removeItem('username');
            localStorage.removeItem('userId');
            window.location.href = '/login'
        }
        return Promise.reject(error)
    }
)

// 词根相关 API
export const getWordRoots = (params) => wordRequest.get('/word-roots', { params })
export const getWordRootDetail = (id) => wordRequest.get(`/word-roots/${id}`)
export const getWordRootQuiz = (id) => wordRequest.get(`/word-roots/${id}/quiz`)
export const getWordRootProgress = () => wordRequest.get('/word-root-progress')
export const markWordRootMastered = (id) => wordRequest.post(`/word-root-progress/${id}/master`)
export const getNextWordRoot = () => wordRequest.get('/word-root-progress/next')

// 单词本（多本）相关 API
export const getWordBooks = () => wordRequest.get('/word-books')
export const createWordBook = (data) => wordRequest.post('/word-books', data)
export const updateWordBook = (id, data) => wordRequest.put(`/word-books/${id}`, data)
export const deleteWordBook = (id, params) => wordRequest.delete(`/word-books/${id}`, { params })

const CURRENT_WORD_BOOK_KEY = 'currentWordBookId'

/** 虚拟「收藏」单词本 */
export const FAVORITES_BOOK_ID = '__favorites__'

export const isFavoritesBookId = (id) => id === FAVORITES_BOOK_ID

export const makeFavoritesBook = (wordCount = 0) => ({
  id: FAVORITES_BOOK_ID,
  name: '收藏',
  description: '复习时收藏的单词',
  isDefault: false,
  isFavorites: true,
  wordCount: Number(wordCount) || 0,
})

export const getCurrentWordBookId = () => localStorage.getItem(CURRENT_WORD_BOOK_KEY) || null

export const setCurrentWordBookId = (id) => {
    if (id) localStorage.setItem(CURRENT_WORD_BOOK_KEY, id)
    else localStorage.removeItem(CURRENT_WORD_BOOK_KEY)
}

export const wordListParams = (params = {}) => {
  const next = { ...(params || {}) }
  if (next.favoritesOnly || isFavoritesBookId(next.wordBookId)) {
    delete next.wordBookId
    next.favoritesOnly = true
    return next
  }
  return next
}

/** 添加单词时附带当前选中的单词本（无则走后端默认本） */
export const addUserWordToCurrentBook = (data) => {
    const wordBookId = getCurrentWordBookId()
    if (!wordBookId || isFavoritesBookId(wordBookId)) return addUserWord(data)
    return addUserWord({ ...data, wordBookId })
}

// 单词列表 / 复习相关 API
export const getUserWords = (params) =>
  wordRequest.get('/user-words', { params: wordListParams(params) })
export const addUserWord = (data) => wordRequest.post('/user-words', data)
export const importUserWords = (data) =>
  wordRequest.post('/user-words/import', data, { timeout: 120000 })
/** AI 整理杂乱文本为词条（需登录；不入库） */
export const aiOrganizeUserWords = (text) =>
  wordRequest.post('/user-words/ai-organize', { text }, { timeout: 120000 })
export const deleteUserWord = (id) => wordRequest.delete(`/user-words/${id}`)
export const batchDeleteUserWords = (ids) =>
  wordRequest.post('/user-words/batch-delete', { ids }, { timeout: 60000 })
export const moveUserWord = (id, wordBookId) => wordRequest.post(`/user-words/${id}/move`, { wordBookId })
export const getDueWords = (params) =>
  wordRequest.get('/user-words/due', { params: wordListParams(params) })
export const getRandomWords = (params) =>
  wordRequest.get('/user-words/random', { params: wordListParams(params) })
export const reviewWord = (id, quality) => wordRequest.post(`/user-words/${id}/review`, { quality })
export const toggleFavoriteWord = (id, isFavorite) =>
  wordRequest.post(`/user-words/${id}/favorite`, isFavorite == null ? {} : { isFavorite })
export const getWordStats = (params) =>
  wordRequest.get('/user-words/stats', { params: wordListParams(params) })

// 分享单词本
export const shareWordBook = (id) => wordRequest.post(`/word-books/${id}/share`)
export const getSharedWordBook = (code, preview = 20) =>
  wordRequest.get(`/shared-wordbooks/${encodeURIComponent(code)}`, { params: { preview } })
export const claimSharedWordBook = (code, data = {}) =>
  wordRequest.post(`/shared-wordbooks/${encodeURIComponent(code)}/claim`, data, { timeout: 120000 })
export const revokeSharedWordBook = (code) =>
  wordRequest.delete(`/shared-wordbooks/${encodeURIComponent(code)}`)

// 官方词本
export const getOfficialWordPacks = () => wordRequest.get('/word-packs')
export const getOfficialWordPackDetail = (id, preview = 20) =>
  wordRequest.get(`/word-packs/${id}`, { params: { preview } })
export const claimOfficialWordPack = (id, data = {}) =>
  wordRequest.post(`/word-packs/${id}/claim`, data, { timeout: 120000 })

// 查询单词、短语或句子（xxapi 词典，句子自动翻译）
export const queryEnglishWord = async (word) => {
    return wordRequest.get('/dictionary', { params: { word: word.trim() } })
}

/** 校验英语单词、短语或句子输入 */
export const isValidEnglishQuery = (text) => {
    const query = text?.trim()
    if (!query || query.length > 500) return false
    if (!/[a-zA-Z]/.test(query)) return false
    if (/[\u4e00-\u9fff]/.test(query)) return false
    return /^[a-zA-Z0-9\s\-'.,!?;:()"\/]+$/.test(query)
}
// 获取每日一句（Timeless API - 支持 CORS）
export const getDailyEnglish = async () => {
    try {
        const res = await axios.get('https://api.timelessq.com/english-sentence', {
            timeout: 10000
        })
        // 转换格式以兼容我们的组件
        return {
            code: 200,
            data: {
                content: res.data.data.content,
                note: res.data.data.note,
                date: res.data.data.date
            }
        }
    } catch (error) {
        console.error('获取每日一句失败', error)
        throw error
    }
}

export default wordRequest;
