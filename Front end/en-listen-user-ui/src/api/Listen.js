import request from './Request'

// 获取所有分类
export const getCategories = () => request.get('/Listen/GetCategories')

// 获取分类下的所有试卷
export const getAlbumsByCategoryId = (categoryId) =>
  request.get('/Listen/GetAlbumsByCategoryId', { params: { categoryId } })

// 获取试卷下的所有题目（音频）
export const getEpisodesByAlbumId = (albumId) =>
  request.get('/Listen/GetEpisodesByAlbumId', { params: { albumId } })
