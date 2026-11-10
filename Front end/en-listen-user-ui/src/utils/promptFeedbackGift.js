/**
 * 每完成一次练习，桌宠弹出一次反馈气泡（不强制弹窗）。
 * 用户点气泡后再打开 EncourageFeedbackDialog。
 */

const CLAIMED_KEY = 'le_feedback_gift_claimed'

let bubbleHandler = null
let dialogHandler = null

/** 桌宠注册：显示小气泡 */
export function registerFeedbackGiftBubble(fn) {
  bubbleHandler = typeof fn === 'function' ? fn : null
}

/** 反馈弹窗注册：仅在用户主动点击后打开 */
export function registerEncourageFeedbackOpener(fn) {
  dialogHandler = typeof fn === 'function' ? fn : null
}

export function hasClaimedFeedbackGift() {
  try {
    return localStorage.getItem(CLAIMED_KEY) === '1'
  } catch {
    return false
  }
}

export function markFeedbackGiftClaimed() {
  try {
    localStorage.setItem(CLAIMED_KEY, '1')
  } catch {
    /* ignore */
  }
}

/**
 * 学完一次练习 → 弹出反馈气泡一次。
 * 未领过资料：引导「反馈领资料」；已领过：轻量邀请写反馈。
 * @param {{ source?: string, context?: string, delayMs?: number }} [options]
 * @returns {boolean}
 */
export function promptFeedbackGift(options = {}) {
  if (!bubbleHandler) return false

  const delayMs = Number.isFinite(options.delayMs) ? options.delayMs : 500
  const payload = {
    source: options.source || '',
    context: options.context || '',
    claimed: hasClaimedFeedbackGift(),
  }

  const show = () => {
    try {
      bubbleHandler(payload)
    } catch {
      /* ignore */
    }
  }

  if (delayMs > 0) setTimeout(show, delayMs)
  else show()
  return true
}

/** 用户点击气泡后再打开完整反馈弹窗 */
export function openFeedbackGiftDialog(payload = {}) {
  if (!dialogHandler) return false
  try {
    dialogHandler(payload)
    return true
  } catch {
    return false
  }
}
