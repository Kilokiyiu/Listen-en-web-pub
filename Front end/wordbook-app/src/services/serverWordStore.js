import * as wordApi from '../api/word'

export const getWords = (params) => wordApi.getUserWords(params)
export const addWord = (data) => wordApi.addUserWord(data)
export const deleteWord = (id) => wordApi.deleteUserWord(id)
export const getDueWords = (params) => wordApi.getDueWords(params)
export const getRandomWords = (params) => wordApi.getRandomWords(params)
export const reviewWord = (id, quality) => wordApi.reviewWord(id, quality)
export const getStats = (params) => wordApi.getWordStats(params)
