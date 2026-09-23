import axios from 'axios'
import { API_BASE } from '../config'
import { getAuth, clearAuth } from '../services/appSettings'

function createClient(baseURL) {
  const client = axios.create({ baseURL, timeout: 15000 })

  client.interceptors.request.use(async (config) => {
    const { token, userId } = await getAuth()
    if (token) config.headers.Authorization = `Bearer ${token}`
    if (userId) config.headers['X-User-Id'] = userId
    return config
  })

  client.interceptors.response.use(
    (res) => res.data,
    async (error) => {
      if (error.response?.status === 401) {
        await clearAuth()
      }
      const data = error.response?.data
      const msg =
        (typeof data === 'string' && data) ||
        data?.message ||
        (error.code === 'ECONNABORTED' ? '请求超时' : '网络错误，请检查连接')
      return Promise.reject(typeof msg === 'string' ? msg : '请求失败')
    }
  )

  return client
}

export const identityApi = createClient(`${API_BASE}/api/identity`)
export const wordApi = createClient(`${API_BASE}/api/word`)
