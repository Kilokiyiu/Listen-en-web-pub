import { identityRequest } from './Request'

export function submitFeedback(data) {
  return identityRequest.post('/Feedback/Submit', data, { timeout: 30000 })
}
