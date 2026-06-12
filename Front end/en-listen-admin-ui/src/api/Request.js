import axios from 'axios'
import { ElMessage } from 'element-plus'

// 管理员前端需要连两个后端服务：
// IdentityService (5263) 用于登录认证
// ListenService (5047) 用于音频管理

// 创建 IdentityService 的 axios 实例
export const identityRequest = axios.create({
  baseURL: "/api/identity",
  timeout: 10000,
})

// 创建 ListenService 的 axios 实例
const listenRequest = axios.create({
  baseURL: "/api/listen",
  timeout: 180000, // 上传文件可能较慢，180秒
})

// 创建 ArticleService 的 axios 实例
const articleRequest = axios.create({
  baseURL: "/api/article",
  timeout: 30000,
})

// 创建 WordService 的 axios 实例
export const wordRequest = axios.create({
  baseURL: "/api/word",
  timeout: 10000,
})

// 三个实例都添加 token 拦截器
const addTokenInterceptor = (instance, serviceName) => {
  instance.interceptors.request.use(
    config => {
      const token = localStorage.getItem('admin_token')
      if (token) {
        config.headers.Authorization = `Bearer ${token}`
      }
      return config
    },
    error => Promise.reject(error)
  )

  instance.interceptors.response.use(
    response => response.data,
    error => {
      if (error.response) {
        const status = error.response.status
        const data = error.response.data
        const msg = typeof data === 'string' ? data
          : (data?.message || data?.title || JSON.stringify(data) || `请求失败 (${status})`)
        ElMessage.error(msg)
        console.error(`[API Error - ${serviceName}]`, status, data)
        if (status === 401) {
          localStorage.removeItem('admin_token')
          localStorage.removeItem('admin_userName')
          window.location.href = '/#/login'
        }
      } else if (error.request) {
        ElMessage.error(`网络错误：后端服务无响应，请检查 ${serviceName} 是否运行`)
        console.error(`[API Error - ${serviceName}] No response:`, error.message)
      } else {
        ElMessage.error('请求配置错误：' + error.message)
      }
      return Promise.reject(error)
    }
  )
}

addTokenInterceptor(identityRequest, 'IdentityService')
addTokenInterceptor(listenRequest, 'ListenService')
addTokenInterceptor(articleRequest, 'ArticleService')
addTokenInterceptor(wordRequest, 'WordService')

export default listenRequest
export { articleRequest }
