import { showToast } from './toast'
import { trackEvent } from '../api/analytics'

/**
 * 加词成功后引导去复习（App 二级漏斗）。
 * @returns {boolean} 是否已跳转复习页
 */
export function promptGoReviewAfterAdd(router, word) {
  const label = word ? `「${word}」` : '单词'
  showToast(`${label}已加入单词本`)
  trackEvent('add_word', '/app/add')

  if (confirm('加入成功。现在去复习一轮，记得更牢？')) {
    router.push({ path: '/review', query: { mode: 'free' } })
    return true
  }
  return false
}
