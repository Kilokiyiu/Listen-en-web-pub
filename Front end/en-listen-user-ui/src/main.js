import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import './styles/global.css'
import { registerIcons } from './icons'

// 兼容旧 Hash 链接：/#/daily → /daily（收藏夹 / 搜索引擎旧收录）
if (typeof window !== 'undefined' && window.location.hash.startsWith('#/')) {
  const legacy = window.location.hash.slice(1)
  const nextUrl = legacy.startsWith('/') ? legacy : `/${legacy}`
  window.history.replaceState(null, '', nextUrl)
}

// 背景图在首屏脚本和样式之后再加载，避免和入口资源抢带宽。
const applyBackground = () => {
  document.documentElement.style.setProperty(
    '--le-bg-image',
    `url('${process.env.BASE_URL || '/'}background.jpg')`
  )
}
if (document.readyState === 'complete') applyBackground()
else window.addEventListener('load', applyBackground, { once: true })

const app = createApp(App)

registerIcons(app)

app.use(router)
app.mount('#app')
