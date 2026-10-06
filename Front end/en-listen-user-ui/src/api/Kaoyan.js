import { kaoyanRequest } from './Request'

export const getKaoyanModuleStatus = () =>
  kaoyanRequest.get('/Kaoyan/GetModuleStatus')

export const getKaoyanHealth = () =>
  kaoyanRequest.get('/Kaoyan/Health')

export const getKaoyanPapers = (series) =>
  kaoyanRequest.get('/Kaoyan/GetPapers', { params: series ? { series } : {} })

export const getKaoyanPaperDetail = (paperId) =>
  kaoyanRequest.get('/Kaoyan/GetPaperDetail', { params: { paperId } })

export const submitKaoyanAnswers = (paperId, answers, sectionId) =>
  kaoyanRequest.post('/Kaoyan/SubmitAnswers', {
    paperId,
    sectionId: sectionId || null,
    answers,
  })

export const checkKaoyanAnswer = (questionId, selectedIndex) =>
  kaoyanRequest.post('/Kaoyan/CheckAnswer', { questionId, selectedIndex })
