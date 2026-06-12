import { onMounted, onUnmounted, watch, unref } from 'vue'

/**
 * 移动端音频播放：保存/恢复进度，锁屏后回来不从头发；Media Session 锁屏控制。
 */
export function useAudioPlayer(audioRef, options = {}) {
  const {
    storageKey = '',
    title = 'ListenEase',
    artist = 'ListenEase',
    album = '英语听力练习',
    resumeOnVisible = true,
  } = options

  let saveTimer = null
  let lastSavedTime = -1

  const getAudio = () => audioRef.value

  const resolveKey = () => unref(storageKey)

  const saveProgress = () => {
    const audio = getAudio()
    const key = resolveKey()
    if (!audio || !key || !audio.src) return

    const payload = {
      currentTime: audio.currentTime,
      wasPlaying: !audio.paused,
      updatedAt: Date.now(),
    }
    try {
      sessionStorage.setItem(key, JSON.stringify(payload))
    } catch {
      /* quota or private mode */
    }
  }

  const restoreProgress = (tryResume = resumeOnVisible) => {
    const audio = getAudio()
    const key = resolveKey()
    if (!audio || !key) return

    let saved
    try {
      saved = JSON.parse(sessionStorage.getItem(key) || 'null')
    } catch {
      return
    }
    if (!saved || typeof saved.currentTime !== 'number') return

    const duration = audio.duration
    if (!Number.isFinite(duration) || duration <= 0) return

    const t = Math.min(Math.max(0, saved.currentTime), duration - 0.5)
    if (t > 0.5) {
      audio.currentTime = t
    }

    if (tryResume && saved.wasPlaying) {
      audio.play().catch(() => {
        /* 浏览器可能要求用户手势才能继续播放 */
      })
    }
  }

  const setupMediaSession = () => {
    if (!('mediaSession' in navigator)) return

    const mediaTitle = unref(title) || 'ListenEase'

    try {
      navigator.mediaSession.metadata = new MediaMetadata({
        title: mediaTitle,
        artist,
        album,
      })

      navigator.mediaSession.setActionHandler('play', () => {
        getAudio()?.play()
      })
      navigator.mediaSession.setActionHandler('pause', () => {
        getAudio()?.pause()
      })
      navigator.mediaSession.setActionHandler('seekbackward', () => {
        const audio = getAudio()
        if (audio) audio.currentTime = Math.max(0, audio.currentTime - 10)
      })
      navigator.mediaSession.setActionHandler('seekforward', () => {
        const audio = getAudio()
        if (audio) audio.currentTime = Math.min(audio.duration || 0, audio.currentTime + 10)
      })
    } catch {
      /* 部分浏览器不支持全部 action */
    }
  }

  const syncMediaSessionPosition = () => {
    const audio = getAudio()
    if (!('mediaSession' in navigator) || !audio || !Number.isFinite(audio.duration)) return

    try {
      navigator.mediaSession.setPositionState({
        duration: audio.duration,
        playbackRate: audio.playbackRate,
        position: audio.currentTime,
      })
    } catch {
      /* setPositionState 在部分环境不可用 */
    }
  }

  const onTimeUpdate = () => {
    const audio = getAudio()
    if (!audio) return

    if (Math.abs(audio.currentTime - lastSavedTime) >= 2) {
      lastSavedTime = audio.currentTime
      saveProgress()
    }

    syncMediaSessionPosition()
  }

  const onLoadedMetadata = () => {
    restoreProgress(false)
    syncMediaSessionPosition()
  }

  const onVisibilityChange = () => {
    if (document.visibilityState === 'hidden') {
      saveProgress()
      return
    }
    if (document.visibilityState === 'visible') {
      restoreProgress(true)
    }
  }

  const onPageHide = () => saveProgress()

  const onPageShow = (event) => {
    if (event.persisted) {
      restoreProgress(true)
    }
  }

  const bindAudio = (audio) => {
    if (!audio) return
    audio.setAttribute('playsinline', '')
    audio.setAttribute('webkit-playsinline', '')
    audio.preload = 'auto'

    audio.addEventListener('loadedmetadata', onLoadedMetadata)
    audio.addEventListener('timeupdate', onTimeUpdate)
    audio.addEventListener('pause', saveProgress)
    audio.addEventListener('ended', saveProgress)
  }

  const unbindAudio = (audio) => {
    if (!audio) return
    audio.removeEventListener('loadedmetadata', onLoadedMetadata)
    audio.removeEventListener('timeupdate', onTimeUpdate)
    audio.removeEventListener('pause', saveProgress)
    audio.removeEventListener('ended', saveProgress)
  }

  const attach = () => {
    const audio = getAudio()
    if (audio) {
      bindAudio(audio)
      setupMediaSession()
      if (audio.readyState >= 1) {
        restoreProgress(false)
      }
    }
  }

  const detach = () => {
    saveProgress()
    unbindAudio(getAudio())
    if (saveTimer) {
      clearInterval(saveTimer)
      saveTimer = null
    }
  }

  onMounted(() => {
    attach()
    document.addEventListener('visibilitychange', onVisibilityChange)
    window.addEventListener('pagehide', onPageHide)
    window.addEventListener('pageshow', onPageShow)
    saveTimer = setInterval(saveProgress, 5000)
  })

  onUnmounted(() => {
    document.removeEventListener('visibilitychange', onVisibilityChange)
    window.removeEventListener('pagehide', onPageHide)
    window.removeEventListener('pageshow', onPageShow)
    detach()
  })

  watch(audioRef, (el, prev) => {
    unbindAudio(prev)
    if (el) {
      bindAudio(el)
      setupMediaSession()
    }
  })

  if (typeof title === 'object' && title !== null && 'value' in title) {
    watch(title, () => setupMediaSession())
  }

  if (typeof storageKey === 'object' && storageKey !== null && 'value' in storageKey) {
    watch(storageKey, () => saveProgress())
  }

  return { saveProgress, restoreProgress }
}
