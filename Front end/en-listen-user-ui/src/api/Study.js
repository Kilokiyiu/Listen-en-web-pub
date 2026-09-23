import { identityRequest } from './Request'

export const recordStudyActivity = (data) =>
  identityRequest.post('/Study/Record', data)

export const getStudySummary = () =>
  identityRequest.get('/Study/MySummary')

export const getStudyList = (params = {}) =>
  identityRequest.get('/Study/MyList', { params })
