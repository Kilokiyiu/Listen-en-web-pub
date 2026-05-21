import { articleRequest } from './Request'

export const getDailyArticle = (date) =>
    articleRequest.get('/Article/GetArticleByDate', { params: { date } })

export const markArticleRead = (articleId) =>
    articleRequest.post('/Article/MarkIsRead', { articleId })

export const toggleFavorite = (articleId) =>
    articleRequest.post('/Article/ToggleFavorite', { articleId })

export const getReadHistory = (page = 1, pageSize = 20) =>
    articleRequest.get('/Article/GetReadHistory', { params: { page, pageSize } })

// BBC News API
export const getBBCNews = (category = null) =>
    articleRequest.get('/BBC/GetTopNews', { params: { category } })

export const getBBCCategories = () =>
    articleRequest.get('/BBC/GetCategories')

export const getBBCArticleDetail = (url) =>
    articleRequest.get('/BBC/GetArticleDetail', { params: { url } })