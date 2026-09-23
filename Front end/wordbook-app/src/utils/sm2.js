/**
 * SM-2 间隔重复算法（与 WordService 后端逻辑一致）
 * @param {object} word - 单词对象
 * @param {number} quality - 评分 0-5: 0=完全忘记, 3=模糊记得, 5=轻松记住
 */
export function updateReview(word, quality) {
  if (quality < 0 || quality > 5) return word

  let { repetitionCount = 0, easeFactor = 2.5, interval = 0 } = word

  if (quality >= 3) {
    if (repetitionCount === 0) interval = 1
    else if (repetitionCount === 1) interval = 6
    else interval = Math.round(interval * easeFactor)
    repetitionCount++
  } else {
    repetitionCount = 0
    interval = 1
  }

  easeFactor = easeFactor + (0.1 - (5 - quality) * (0.08 + (5 - quality) * 0.02))
  if (easeFactor < 1.3) easeFactor = 1.3

  const nextReview = new Date()
  nextReview.setDate(nextReview.getDate() + interval)

  return {
    ...word,
    repetitionCount,
    easeFactor,
    interval,
    nextReview: nextReview.toISOString(),
  }
}

export function getReviewStatus(word) {
  if (!word.nextReview) return { type: 'new', text: '新词' }
  const now = new Date()
  const next = new Date(word.nextReview)
  if (next <= now) return { type: 'due', text: '待复习' }
  if (word.interval >= 21) return { type: 'mastered', text: '已掌握' }
  return { type: 'learning', text: '复习中' }
}
