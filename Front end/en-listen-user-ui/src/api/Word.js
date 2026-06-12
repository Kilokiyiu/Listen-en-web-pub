import axios from "axios";
import { ElMessage } from "element-plus";

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
        ElMessage.error(msg)
        if(error.response?.status === 401){
            localStorage.removeItem('token');
            localStorage.removeItem('username');
            localStorage.removeItem('userId');
            window.location.href="/#/login"
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

// 单词本相关 API
export const getUserWords = (params) => wordRequest.get('/user-words', { params })
export const addUserWord = (data) => wordRequest.post('/user-words', data)
export const deleteUserWord = (id) => wordRequest.delete(`/user-words/${id}`)
export const getDueWords = (params) => wordRequest.get('/user-words/due', { params })
export const getRandomWords = (params) => wordRequest.get('/user-words/random', { params })
export const reviewWord = (id, quality) => wordRequest.post(`/user-words/${id}/review`, { quality })
export const getWordStats = () => wordRequest.get('/user-words/stats')

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
