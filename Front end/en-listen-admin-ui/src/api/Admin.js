import { identityRequest } from './Request'
import listenRequest from './Request'
import { articleRequest, wordRequest, kaoyanRequest } from './Request'

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
export const getFunnelStats = (days = 7) =>
  identityRequest.get('/Admin/Funnel', { params: { days } })
export const getArticleReadingStats = () => articleRequest.get('/Admin/GetReadingStats')
export const getWordLearningStats = () => wordRequest.get('/Admin/GetLearningStats')

// ========== 用户管理 ==========
export const getUsers = (params = {}) =>
  identityRequest.get('/Admin/GetUsers', { params })

export const createUser = (data) =>
  identityRequest.post('/Admin/CreateUser', data)

export const setUserRoles = (data) =>
  identityRequest.post('/Admin/SetRoles', data)

export const setUserLock = (data) =>
  identityRequest.post('/Admin/SetLock', data)

export const resetUserPassword = (userId) =>
  identityRequest.post('/Admin/ResetPassword', { userId })

export const setUserPassword = (data) =>
  identityRequest.post('/Admin/SetPassword', data)

export const deleteUser = (userId) =>
  identityRequest.post('/Admin/DeleteUser', { userId })

// ========== ListenService 管理相关 ==========
export const uploadAudio = (formData) =>
  listenRequest.post('/Admin/UploadAudio', formData)

export const getCategories = () =>
  listenRequest.get('/Listen/GetCategories')

// ========== 试卷管理 ==========
export const getAllAlbums = () =>
  listenRequest.get('/Admin/GetAllAlbums')

export const toggleAlbumVisibility = (albumId) =>
  listenRequest.post('/Admin/ToggleAlbumVisibility', { episodeId: albumId })

export const uploadAlbumDocument = (albumId, documentType, file) => {
  const formData = new FormData()
  formData.append('albumId', albumId)
  formData.append('documentType', documentType)
  formData.append('file', file)
  return listenRequest.post('/Admin/UploadAlbumDocument', formData)
}

// ========== 题目管理 ==========
export const getAllEpisodes = () =>
  listenRequest.get('/Admin/GetAllEpisodes')

export const updateEpisodeSubtitle = (data) =>
  listenRequest.post('/Admin/UpdateEpisodeSubtitle', data)

export const toggleEpisodeVisibility = (episodeId) =>
  listenRequest.post('/Admin/ToggleEpisodeVisibility', { episodeId })

export const deleteEpisode = (episodeId) =>
  listenRequest.post('/Admin/DeleteEpisode', { episodeId })

export const getQuizSections = (albumId) =>
  listenRequest.get('/Admin/GetQuizSections', { params: { albumId } })

export const saveQuizSections = (albumId, sections) =>
  listenRequest.post('/Admin/SaveQuizSections', { albumId, sections })

export const moveQuizSectionGroup = (sectionId, groupName) =>
  listenRequest.post('/Admin/MoveQuizSectionGroup', { sectionId, groupName })

export const uploadSectionAudio = (sectionId, file) => {
  const formData = new FormData()
  formData.append('sectionId', sectionId)
  formData.append('file', file)
  return listenRequest.post('/Admin/UploadSectionAudio', formData)
}

// ========== KaoyanService ==========
export const getKaoyanModuleStatus = () =>
  kaoyanRequest.get('/Kaoyan/GetModuleStatus')

export const getKaoyanAllPapers = () =>
  kaoyanRequest.get('/Admin/GetAllPapers')

export const getKaoyanPaperFull = (paperId) =>
  kaoyanRequest.get('/Admin/GetPaperFull', { params: { paperId } })

export const toggleKaoyanPaperVisibility = (id) =>
  kaoyanRequest.post('/Admin/TogglePaperVisibility', { id })

export const createKaoyanPaper = (data) =>
  kaoyanRequest.post('/Admin/CreatePaper', data)

export const deleteKaoyanPaper = (id) =>
  kaoyanRequest.post('/Admin/DeletePaper', { id })

export const importKaoyanPaper = (data) =>
  kaoyanRequest.post('/Admin/ImportPaper', data)

export const saveKaoyanPaperContent = (data) =>
  kaoyanRequest.post('/Admin/SavePaperContent', data)

// ========== ArticleService 文章管理 ==========
export const getAllArticles = () =>
  articleRequest.get('/Admin/GetAllArticles')

export const addArticle = (data) =>
  articleRequest.post('/Admin/AddArticle', data)

export const batchAddArticles = (data) =>
  articleRequest.post('/Admin/BatchAddArticles', data)

export const deleteArticle = (id) =>
  articleRequest.post('/Admin/DeleteArticle', { id })

export const toggleArticlePublishStatus = (id) =>
  articleRequest.post('/Admin/TogglePublishStatus', { id })

export const updateArticlePublicDate = (id, publicDate) =>
  articleRequest.post('/Admin/UpdatePublicDate', { id, publicDate })

// ========== WordService 官方词本 ==========
export const getWordPacks = () => wordRequest.get('/Admin/WordPacks')

export const createWordPack = (data) => wordRequest.post('/Admin/WordPacks', data)

export const updateWordPack = (id, data) => wordRequest.put(`/Admin/WordPacks/${id}`, data)

export const toggleWordPackPublish = (id) => wordRequest.post(`/Admin/WordPacks/${id}/publish`)

export const deleteWordPack = (id) => wordRequest.delete(`/Admin/WordPacks/${id}`)

export const getWordPackEntries = (id, params = {}) =>
  wordRequest.get(`/Admin/WordPacks/${id}/entries`, { params })

export const addWordPackEntry = (id, data) =>
  wordRequest.post(`/Admin/WordPacks/${id}/entries`, data)

export const updateWordPackEntry = (packId, entryId, data) =>
  wordRequest.put(`/Admin/WordPacks/${packId}/entries/${entryId}`, data)

export const deleteWordPackEntry = (packId, entryId) =>
  wordRequest.delete(`/Admin/WordPacks/${packId}/entries/${entryId}`)

export const importWordPackEntries = (id, data) =>
  wordRequest.post(`/Admin/WordPacks/${id}/entries/import`, data, { timeout: 180000 })
