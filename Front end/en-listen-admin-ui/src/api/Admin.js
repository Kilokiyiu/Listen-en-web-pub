import { identityRequest } from './Request'
import listenRequest from './Request'
import { articleRequest, wordRequest } from './Request'

// ========== IdentityService 登录相关 ==========
export const loginByUserName = (userName, password) =>
  identityRequest.post('/Login/LoginByUserNameAndPwd', { userName, password })

// ========== 数据概览 ==========
export const getStatsOverview = () => identityRequest.get('/Admin/Overview')
export const getRegistrationTrend = (days = 30) =>
  identityRequest.get('/Admin/Registrations', { params: { days } })
export const getTrafficTrend = (days = 7) =>
  identityRequest.get('/Admin/Traffic', { params: { days } })
export const getTopPages = (days = 7, limit = 10) =>
  identityRequest.get('/Admin/TopPages', { params: { days, limit } })
export const getArticleReadingStats = () => articleRequest.get('/Admin/GetReadingStats')
export const getWordLearningStats = () => wordRequest.get('/Admin/GetLearningStats')

// ========== ListenService 管理相关 ==========
export const uploadAudio = (formData) =>
  listenRequest.post('/Admin/UploadAudio', formData)

export const getCategories = () =>
  listenRequest.get('/Listen/GetCategories')

// ========== 试卷管理 ==========
// 获取所有试卷
export const getAllAlbums = () =>
  listenRequest.get('/Admin/GetAllAlbums')

// 切换试卷显示/隐藏状态
export const toggleAlbumVisibility = (albumId) =>
  listenRequest.post('/Admin/ToggleAlbumVisibility', { episodeId: albumId })

// 上传试卷 PDF 或答案 PDF（documentType: paper | answer）
export const uploadAlbumDocument = (albumId, documentType, file) => {
  const formData = new FormData()
  formData.append('albumId', albumId)
  formData.append('documentType', documentType)
  formData.append('file', file)
  return listenRequest.post('/Admin/UploadAlbumDocument', formData)
}

// ========== 题目管理 ==========
// 获取所有题目（管理列表）
export const getAllEpisodes = () =>
  listenRequest.get('/Admin/GetAllEpisodes')

// 更新题目字幕
export const updateEpisodeSubtitle = (data) =>
  listenRequest.post('/Admin/UpdateEpisodeSubtitle', data)

// 切换显示/隐藏状态
export const toggleEpisodeVisibility = (episodeId) =>
  listenRequest.post('/Admin/ToggleEpisodeVisibility', { episodeId })

// 删除题目
export const deleteEpisode = (episodeId) =>
  listenRequest.post('/Admin/DeleteEpisode', { episodeId })

// ========== ArticleService 文章管理 ==========
// 获取所有文章
export const getAllArticles = () =>
  articleRequest.get('/Admin/GetAllArticles')

// 添加单篇文章
export const addArticle = (data) =>
  articleRequest.post('/Admin/AddArticle', data)

// 批量添加文章
export const batchAddArticles = (data) =>
  articleRequest.post('/Admin/BatchAddArticles', data)

// 删除文章
export const deleteArticle = (id) =>
  articleRequest.post('/Admin/DeleteArticle', { id })

// 切换发布状态
export const toggleArticlePublishStatus = (id) =>
  articleRequest.post('/Admin/TogglePublishStatus', { id })
