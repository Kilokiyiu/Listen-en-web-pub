import { showToast } from './toast'
import { trackEvent } from '../api/analytics'
import { confirmDialog } from './dialog'

export async function promptGoReviewAfterAdd(router, word) {
  const label = word ? `「${word}」` : '单词'
  showToast(`${label}已加入单词本`)
  trackEvent('add_word', '/app/add')

  const go = await confirmDialog('现在去复习？', {
    title: '加入成功',
    confirmText: '去复习',
    cancelText: '稍后',
  })
  if (go) {
    router.push({ path: '/review', query: { mode: 'free' } })
    return true
  }
  return false
}
