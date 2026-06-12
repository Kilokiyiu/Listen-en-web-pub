<template>
  <div ref="rootRef" class="pdf-viewer-root">
    <div ref="pagesRef" class="pdf-scroll" />

    <div v-if="loading" class="pdf-state pdf-overlay">
      <el-icon class="is-loading" :size="28"><Loading /></el-icon>
      <span>{{ loadingText }}</span>
    </div>

    <div v-if="error" class="pdf-state pdf-state--error">
      <p>{{ error }}</p>
      <el-button type="primary" round @click="openExternal">在新窗口打开 PDF</el-button>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, watch } from 'vue'

const props = defineProps({
  src: {
    type: String,
    required: true,
  },
})

const rootRef = ref(null)
const pagesRef = ref(null)
const loading = ref(true)
const loadingText = ref('正在加载 PDF…')
const error = ref('')
const renderWidth = ref(320)

let resizeObserver = null
let renderToken = 0

const loadPdfJs = () => new Promise((resolve, reject) => {
  if (window.pdfjsLib) {
    resolve(window.pdfjsLib)
    return
  }

  const existing = document.querySelector('script[data-listen-pdfjs="true"]')
  if (existing) {
    existing.addEventListener('load', () => {
      window.pdfjsLib ? resolve(window.pdfjsLib) : reject(new Error('PDF.js 加载失败'))
    }, { once: true })
    existing.addEventListener('error', () => reject(new Error('PDF.js 脚本加载失败')), { once: true })
    return
  }

  const base = `${process.env.BASE_URL}pdfjs/`
  const script = document.createElement('script')
  script.src = `${base}pdf.min.js`
  script.async = true
  script.dataset.listenPdfjs = 'true'
  script.onload = () => {
    if (!window.pdfjsLib) {
      reject(new Error('PDF.js 加载失败'))
      return
    }
    if (!window.__listenPdfWorkerConfigured) {
      window.pdfjsLib.GlobalWorkerOptions.workerSrc = `${base}pdf.worker.min.js`
      window.__listenPdfWorkerConfigured = true
    }
    resolve(window.pdfjsLib)
  }
  script.onerror = () => reject(new Error('PDF.js 脚本加载失败'))
  document.head.appendChild(script)
})

const updateWidth = () => {
  const width = rootRef.value?.clientWidth
  if (width && width > 0) {
    renderWidth.value = Math.floor(width - 2)
  }
}

/** 手机 Retina 屏需按 DPR 渲染，否则 canvas 会被拉伸变糊 */
const getOutputScale = () => Math.min(window.devicePixelRatio || 1, 2.5)

const renderPageToCanvas = async (page, container) => {
  const baseViewport = page.getViewport({ scale: 1 })
  const cssScale = renderWidth.value / baseViewport.width
  const viewport = page.getViewport({ scale: cssScale })
  const outputScale = getOutputScale()

  const canvas = document.createElement('canvas')
  canvas.className = 'pdf-page-canvas'
  const ctx = canvas.getContext('2d')

  canvas.width = Math.floor(viewport.width * outputScale)
  canvas.height = Math.floor(viewport.height * outputScale)
  canvas.style.width = `${Math.floor(viewport.width)}px`
  canvas.style.height = `${Math.floor(viewport.height)}px`

  const transform = outputScale !== 1 ? [outputScale, 0, 0, outputScale, 0, 0] : null

  container.appendChild(canvas)
  await page.render({
    canvasContext: ctx,
    transform,
    viewport,
  }).promise
}

const clearPages = () => {
  if (pagesRef.value) {
    pagesRef.value.innerHTML = ''
  }
}

const renderPdf = async () => {
  const token = ++renderToken
  loading.value = true
  loadingText.value = '正在加载 PDF…'
  error.value = ''
  clearPages()

  const container = pagesRef.value
  if (!container) {
    loading.value = false
    error.value = 'PDF 容器初始化失败，请刷新页面重试。'
    return
  }

  try {
    const pdfjsLib = await loadPdfJs()
    if (token !== renderToken) return

    updateWidth()
    loadingText.value = '正在下载 PDF 文件…'

    const doc = await pdfjsLib.getDocument({
      url: props.src,
      withCredentials: false,
      disableAutoFetch: false,
      disableStream: false,
    }).promise

    if (token !== renderToken) return

    const totalPages = doc.numPages
    for (let pageNum = 1; pageNum <= totalPages; pageNum += 1) {
      if (token !== renderToken) return

      if (pageNum > 1) {
        loadingText.value = `正在渲染第 ${pageNum}/${totalPages} 页…`
      }

      const page = await doc.getPage(pageNum)
      await renderPageToCanvas(page, container)

      if (pageNum === 1 && token === renderToken) {
        loading.value = false
      }
    }

    if (token === renderToken) {
      loading.value = false
    }
  } catch (e) {
    if (token !== renderToken) return
    console.error('PDF render failed', e)
    loading.value = false
    error.value = 'PDF 加载失败，请尝试在新窗口打开。'
  }
}

const openExternal = () => {
  window.open(props.src, '_blank', 'noopener,noreferrer')
}

watch(
  () => props.src,
  () => {
    renderPdf()
  }
)

onMounted(() => {
  updateWidth()
  renderPdf()

  if (typeof ResizeObserver !== 'undefined' && rootRef.value) {
    resizeObserver = new ResizeObserver(() => {
      const prev = renderWidth.value
      updateWidth()
      if (Math.abs(prev - renderWidth.value) > 24) {
        renderPdf()
      }
    })
    resizeObserver.observe(rootRef.value)
  } else {
    window.addEventListener('resize', updateWidth)
  }
})

onUnmounted(() => {
  renderToken += 1
  resizeObserver?.disconnect()
  window.removeEventListener('resize', updateWidth)
})
</script>

<style scoped>
.pdf-viewer-root {
  position: relative;
  width: 100%;
  min-height: 200px;
  background: #eef2f7;
}

.pdf-scroll {
  max-height: 72vh;
  overflow: auto;
  -webkit-overflow-scrolling: touch;
  padding: 8px 0;
}

.pdf-scroll :deep(.pdf-page-canvas) {
  display: block;
  max-width: 100%;
  height: auto;
  margin: 0 auto 8px;
  background: #fff;
  box-shadow: 0 1px 4px rgba(15, 23, 42, 0.08);
}

.pdf-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 10px;
  min-height: 200px;
  padding: 24px 16px;
  color: var(--le-text-muted);
  font-size: 14px;
}

.pdf-overlay {
  position: absolute;
  inset: 0;
  z-index: 2;
  background: rgba(238, 242, 247, 0.92);
}

.pdf-state--error {
  position: relative;
  z-index: 3;
}

.pdf-state--error p {
  margin: 0;
  text-align: center;
  line-height: 1.6;
}
</style>
