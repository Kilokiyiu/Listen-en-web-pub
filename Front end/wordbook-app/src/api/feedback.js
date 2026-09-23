import { identityApi } from './http'

export const submitFeedback = (data) =>
  identityApi.post('/Feedback/Submit', data, { timeout: 30000 })
