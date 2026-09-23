import { ElMessage, ElMessageBox } from 'element-plus'
import { trackEvent } from '../api/Analytics.js'

/**
 * 加词成功后引导完成一轮复习（二级漏斗）。
 * @param {import('vue-router').Router} router
 * @param {string} word
 * @param {string} [fromPath]
 */
export async function promptGoReviewAfterAdd(router, word, fromPath = '/my-words') {
  const label = word ? `「${word}」` : '单词'
  ElMessage.success(`${label}已加入单词本`)
  trackEvent('add_word', fromPath)

  try {
    await ElMessageBox.confirm(
      '现在去复习一轮，记得更牢。也可以稍后再从「我的单词」进入。',
      '加入成功',
      {
        confirmButtonText: '去复习',
        cancelButtonText: '稍后',
        type: 'success',
        distinguishCancelAndClose: true,
      }
    )
    router.push({ name: 'wordReview', query: { mode: 'free' } })
  } catch {
    /* 用户选择稍后或关闭 */
  }
}
