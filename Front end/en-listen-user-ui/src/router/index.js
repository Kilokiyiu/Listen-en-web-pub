import { ref } from 'vue'
import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import { setPageMeta, DEFAULT_TITLE, DEFAULT_DESCRIPTION, DEFAULT_KEYWORDS } from '../utils/seo'
import { SEO_HUBS, getSeoHubByPath } from '../utils/seoHubs'
import { trackPageView } from '../api/Analytics.js'

/** 顶栏/底栏点击后懒加载未完成时的全局进度 */
export const routeLoading = ref(false)

const CHUNK_RELOAD_KEY = 'le_chunk_reload_once'

const isChunkLoadError = (err) => {
  const msg = String(err?.message || err || '')
  return /Loading chunk [\d]+ failed|Failed to fetch dynamically imported module|Importing a module script failed|error loading dynamically imported module/i.test(msg)
}

const showRouteError = (msg) => {
  Promise.all([
    import('element-plus/es/components/message/index.mjs'),
    import('element-plus/es/components/message/style/css.mjs'),
  ]).then(([{ ElMessage }]) => ElMessage.error(msg)).catch(() => {
    // Element Plus 不可用时至少别静默失败
    console.error(msg)
  })
}

const routes = [
  {
    path: '/',
    name: 'home',
    component: HomeView,
    meta: {
      title: DEFAULT_TITLE,
      description: DEFAULT_DESCRIPTION,
      keywords: DEFAULT_KEYWORDS,
    }
  },
  {
    path: '/listening',
    name: 'listeningHub',
    component: () => import('../views/CategoryHubView.vue'),
    meta: {
      title: SEO_HUBS.listening.title,
      description: SEO_HUBS.listening.description,
      keywords: SEO_HUBS.listening.keywords,
    }
  },
  {
    path: '/si-liu-ji',
    name: 'siLiuJiHub',
    component: () => import('../views/CategoryHubView.vue'),
    meta: {
      title: SEO_HUBS.siLiuJi.title,
      description: SEO_HUBS.siLiuJi.description,
      keywords: SEO_HUBS.siLiuJi.keywords,
    }
  },
  {
    path: '/cet4',
    name: 'cet4Hub',
    component: () => import('../views/CategoryHubView.vue'),
    meta: {
      title: SEO_HUBS.cet4.title,
      description: SEO_HUBS.cet4.description,
      keywords: SEO_HUBS.cet4.keywords,
    }
  },
  {
    path: '/cet6',
    name: 'cet6Hub',
    component: () => import('../views/CategoryHubView.vue'),
    meta: {
      title: SEO_HUBS.cet6.title,
      description: SEO_HUBS.cet6.description,
      keywords: SEO_HUBS.cet6.keywords,
    }
  },
  {
    path: '/exams',
    name: 'exams',
    component: () => import('../views/PaperListView.vue'),
    meta: {
      title: SEO_HUBS.exams.title,
      description: SEO_HUBS.exams.description,
      keywords: SEO_HUBS.exams.keywords,
    }
  },
  {
    path: '/search',
    name: 'paperSearch',
    component: () => import('../views/PaperSearchView.vue'),
    meta: {
      title: '搜索四级六级与考研试卷 - ListenEase',
      description: '搜索英语四级、六级听力真题与考研英语试卷，快速找到往年试卷开始刷题。',
      keywords: '试卷搜索,四级真题,六级听力,考研英语,往年试卷,ListenEase',
    }
  },
  {
    path: '/exam',
    name: 'examDetail',
    component: () => import('../views/QuestionView.vue'),
    meta: {
      title: '免费四级六级听力刷题_往年试卷原文在线练 - ListenEase',
      description: '免费在线刷四级往年试卷与英语六级听力：播放音频、查看听力原文，下载试卷与答案 PDF。',
      keywords: '免费刷题,四级往年试卷,英语六级听力,听力原文,试卷下载,ListenEase',
    }
  },
  {
    path: '/login',
    name: 'login',
    component: () => import('../views/LoginView.vue'),
    meta: { noindex: true, title: '登录 - ListenEase' }
  },
  {
    path: '/profile',
    name: 'profile',
    component: () => import('../views/UserProfileView.vue'),
    meta: { noindex: true, title: '个人中心 - ListenEase' }
  },
  {
    path: '/history',
    name: 'history',
    component: () => import('../views/StudyRecordView.vue'),
    meta: { noindex: true, title: '学习记录 - ListenEase' }
  },
  {
    path: '/reading',
    name: 'readingHub',
    component: () => import('../views/ReadingHubView.vue'),
    meta: {
      title: SEO_HUBS.reading.title,
      description: SEO_HUBS.reading.description,
      keywords: SEO_HUBS.reading.keywords,
    }
  },
  {
    path: '/daily',
    name: 'dailyArticle',
    component: () => import('../views/DailyArticleView.vue'),
    meta: {
      title: SEO_HUBS.daily.title,
      description: SEO_HUBS.daily.description,
      keywords: SEO_HUBS.daily.keywords,
    }
  },
  {
    path: '/kaoyan',
    name: 'kaoyan',
    component: () => import('../views/kaoyan/KaoyanPaperListView.vue'),
    meta: {
      title: SEO_HUBS.kaoyan.title,
      description: SEO_HUBS.kaoyan.description,
      keywords: SEO_HUBS.kaoyan.keywords,
    }
  },
  {
    path: '/kaoyan/paper',
    name: 'kaoyanPaper',
    component: () => import('../views/kaoyan/KaoyanPaperView.vue'),
    meta: {
      title: '考研英语刷题_阅读原文免费在线作答 - ListenEase',
      description: '免费考研英语刷题：英语一/二完形与阅读原文在线作答，提交后看解析；整卷 PDF 可查阅作文题。',
      keywords: '考研英语刷题,考研英语二阅读原文,英语二历年真题,免费刷题,ListenEase',
    }
  },
  {
    path: '/words',
    name: 'wordsHub',
    component: () => import('../views/WordsHubView.vue'),
    meta: {
      title: '单词学习_免费辅助四六级刷题记单词 - ListenEase',
      description: '免费单词本、词根学习与间隔复习，配合大学四六级练习与英语刷题高效记单词。',
      keywords: '单词学习,四六级词汇,免费背单词,英语刷题,ListenEase',
    }
  },
  {
    path: '/word-roots',
    name: 'wordRoots',
    component: () => import('../views/WordRootsView.vue'),
    meta: {
      title: SEO_HUBS.wordRoots.title,
      description: SEO_HUBS.wordRoots.description,
      keywords: SEO_HUBS.wordRoots.keywords,
    }
  },
  {
    path: '/word-roots/:id',
    name: 'wordRootDetail',
    component: () => import('../views/WordRootDetailView.vue'),
    meta: {
      title: '词根详情 - ListenEase',
      description: '查看词根释义、例词与相关词汇，辅助英语词汇学习。'
    }
  },
  {
    path: '/my-words',
    name: 'myWords',
    component: () => import('../views/MyWordsView.vue'),
    meta: { noindex: true, title: '我的单词本 - ListenEase' }
  },
  {
    path: '/word-packs',
    name: 'wordPacks',
    component: () => import('../views/WordPacksView.vue'),
    meta: {
      title: SEO_HUBS.wordPacks.title,
      description: SEO_HUBS.wordPacks.description,
      keywords: SEO_HUBS.wordPacks.keywords,
    }
  },
  {
    path: '/word-review',
    name: 'wordReview',
    component: () => import('../views/WordReviewView.vue'),
    meta: { noindex: true, title: '单词复习 - ListenEase' }
  },
  {
    path: '/bbc-news',
    name: 'bbcNews',
    component: () => import('../views/BBCNewsView.vue'),
    meta: {
      title: SEO_HUBS.bbcNews.title,
      description: SEO_HUBS.bbcNews.description,
      keywords: SEO_HUBS.bbcNews.keywords,
    }
  },
  {
    path: '/feedback',
    name: 'feedback',
    component: () => import('../views/FeedbackView.vue'),
    meta: {
      title: '意见反馈 - ListenEase',
      description: '向 ListenEase 提交功能建议、问题反馈或改进意见。'
    }
  },
  {
    path: '/about',
    name: 'about',
    component: () => import('../views/AboutView.vue'),
    meta: {
      title: '关于 ListenEase_四六级备考网站与英语刷题工具',
      description: '了解 ListenEase：四六级备考网站，提供英语四级真题汇总、六级听力真题、大学英语六级考试卷与考研阅读练习。',
      keywords: 'ListenEase,四六级备考网站,英语四级真题汇总,六级听力真题,关于我们',
    }
  },
  {
    path: '/:pathMatch(.*)*',
    redirect: '/'
  }
]

const scrollToTop = () => {
  window.scrollTo(0, 0)
  document.documentElement.scrollTop = 0
  document.body.scrollTop = 0
}

const router = createRouter({
  history: createWebHistory(process.env.BASE_URL),
  routes,
  scrollBehavior (to, from, savedPosition) {
    if (savedPosition) return savedPosition
    return { top: 0, left: 0 }
  }
})

// 需要登录的路由
const authRoutes = ['/profile', '/history', '/my-words', '/word-review']

router.beforeEach((to, from, next) => {
  if (to.fullPath !== from.fullPath) {
    routeLoading.value = true
  }
  const isLoggedIn = localStorage.getItem('token') && localStorage.getItem('userId')
  if (authRoutes.includes(to.path) && !isLoggedIn) {
    next({ path: '/login', query: { redirect: to.fullPath } })
  } else {
    next()
  }
})

router.afterEach((to) => {
  routeLoading.value = false
  try {
    sessionStorage.removeItem(CHUNK_RELOAD_KEY)
  } catch {
    /* ignore */
  }
  scrollToTop()
  // 栏目页用干净 path 做 canonical；带查询的练习页保留 fullPath
  const pagePath = Object.keys(to.query || {}).length ? (to.fullPath || to.path) : (to.path || '/')
  const hub = getSeoHubByPath(to.path)
  setPageMeta({
    title: to.meta.title,
    description: to.meta.description,
    keywords: to.meta.keywords || hub?.keywords,
    path: pagePath,
    noindex: !!to.meta.noindex
  })
  trackPageView(to.path || '/')
  // GTM / GA：SPA 路由切换推送虚拟 page_view
  if (typeof window !== 'undefined') {
    window.dataLayer = window.dataLayer || []
    window.dataLayer.push({
      event: 'page_view',
      page_path: to.fullPath || to.path || '/',
      page_title: document.title,
      page_location: window.location.href,
    })
  }
})

// 发版后旧 HTML 指向已删除的 JS chunk 时：自动硬刷新一次；仍失败则提示用户
router.onError((err) => {
  routeLoading.value = false
  if (!isChunkLoadError(err)) {
    console.error('[router]', err)
    return
  }
  try {
    if (!sessionStorage.getItem(CHUNK_RELOAD_KEY)) {
      sessionStorage.setItem(CHUNK_RELOAD_KEY, '1')
      window.location.reload()
      return
    }
  } catch {
    /* private mode */
  }
  showRouteError('页面资源加载失败，请强制刷新（Ctrl+F5）后重试')
})

export default router

/** 空闲时预取常用栏目，减少「点了地址栏不变」的等待 */
export const prefetchCommonRoutes = () => {
  const jobs = [
    () => import('../views/DailyArticleView.vue'),
    () => import('../views/ReadingHubView.vue'),
    () => import('../views/PaperListView.vue'),
    () => import('../views/BBCNewsView.vue'),
    () => import('../views/kaoyan/KaoyanPaperListView.vue'),
    () => import('../views/WordsHubView.vue'),
    () => import('../views/AboutView.vue'),
    () => import('../views/LoginView.vue'),
  ]
  jobs.forEach((load, i) => {
    window.setTimeout(() => {
      load().catch(() => { /* 预取失败不影响使用，正式点击时会再走 onError */ })
    }, 400 + i * 200)
  })
}
