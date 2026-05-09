import { articleRequest } from './Request'

export const getDailyArticle = (date) =>
    articleRequest.get('/Article/GetArticleByDate', { params: { date } })

export const markArticleRead = (articleId) =>
    articleRequest.post('/Article/MarkIsRead', { articleId })

export const toggleFavorite = (articleId) =>
    articleRequest.post('/Article/ToggleFavorite', { articleId })

export const getReadHistory = (page = 1, pageSize = 20) =>
    articleRequest.get('/Article/GetReadHistory', { params: { page, pageSize } })