import { identityRequest } from './Request'

/** 本地预览：URL 带 ?previewFeedbackGift=1 时走模拟提交，不请求后端 */
function shouldMockFeedback() {
  if (process.env.NODE_ENV !== 'development') return false
  try {
    return new URLSearchParams(window.location.search).has('previewFeedbackGift')
  } catch {
    return false
  }
}

export function submitFeedback(data) {
  if (shouldMockFeedback()) {
    // eslint-disable-next-line no-console
    console.log('[mock Feedback/Submit]', data)
    return Promise.resolve({ data: { ok: true, mocked: true } })
  }
  return identityRequest.post('/Feedback/Submit', data, { timeout: 30000 })
}
