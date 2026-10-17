import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import { setPageMeta, DEFAULT_TITLE, DEFAULT_DESCRIPTION } from '../utils/seo'
import { SEO_HUBS } from '../utils/seoHubs'
import { trackPageView } from '../api/Analytics.js'

const routes = [
  {
    path: '/',
    name: 'home',
    component: HomeView,
    meta: {
      title: DEFAULT_TITLE,
      description: DEFAULT_DESCRIPTION
    }
  },
  {
    path: '/listening',
    name: 'listeningHub',
    component: () => import('../views/CategoryHubView.vue'),
    meta: {
      title: SEO_HUBS.listening.title,
      description: SEO_HUBS.listening.description
    }
  },
  {
    path: '/si-liu-ji',
    name: 'siLiuJiHub',
    component: () => import('../views/CategoryHubView.vue'),
    meta: {
      title: SEO_HUBS.siLiuJi.title,
      description: SEO_HUBS.siLiuJi.description
    }
  },
  {
    path: '/cet4',
    name: 'cet4Hub',
    component: () => import('../views/CategoryHubView.vue'),
    meta: {
      title: SEO_HUBS.cet4.title,
      description: SEO_HUBS.cet4.description
    }
  },
  {
    path: '/cet6',
    name: 'cet6Hub',
    component: () => import('../views/CategoryHubView.vue'),
    meta: {
      title: SEO_HUBS.cet6.title,
      description: SEO_HUBS.cet6.description
    }
  },
  {
    path: '/exams',
    name: 'exams',
    component: () => import('../views/PaperListView.vue'),
    meta: {
      title: SEO_HUBS.exams.title,
      description: SEO_HUBS.exams.description
    }
  },
  {
    path: '/exam',
    name: 'examDetail',
    component: () => import('../views/QuestionView.vue'),
    meta: {
      title: '听力真题在线练习 - ListenEase',
      description: '在线播放历年真题听力音频，查看听力原文，下载试卷 PDF 与答案，配套英语学习资料。'
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
      description: SEO_HUBS.reading.description
    }
  },
  {
    path: '/daily',
    name: 'dailyArticle',
    component: () => import('../views/DailyArticleView.vue'),
    meta: {
      title: SEO_HUBS.daily.title,
      description: SEO_HUBS.daily.description
    }
  },
  {
    path: '/kaoyan',
    name: 'kaoyan',
    component: () => import('../views/kaoyan/KaoyanPaperListView.vue'),
    meta: {
      title: SEO_HUBS.kaoyan.title,
      description: SEO_HUBS.kaoyan.description
    }
  },
  {
    path: '/kaoyan/paper',
    name: 'kaoyanPaper',
    component: () => import('../views/kaoyan/KaoyanPaperView.vue'),
    meta: {
      title: '考研英语在线做题 - ListenEase',
      description: '考研英语完形填空与阅读理解在线答题，提交后查看正确答案与解析。'
    }
  },
  {
    path: '/words',
    name: 'wordsHub',
    component: () => import('../views/WordsHubView.vue'),
    meta: {
      title: '单词学习 - ListenEase',
      description: '个人单词本、词根学习与间隔复习，集中管理英语词汇学习。'
    }
  },
  {
    path: '/word-roots',
    name: 'wordRoots',
    component: () => import('../views/WordRootsView.vue'),
    meta: {
      title: SEO_HUBS.wordRoots.title,
      description: SEO_HUBS.wordRoots.description
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
      description: SEO_HUBS.wordPacks.description
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
      description: SEO_HUBS.bbcNews.description
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
      title: '关于我们 - ListenEase 英语听力与学习资料',
      description: '了解 ListenEase：专注英语听力练习与英语学习资料，服务四六级、雅思、托福备考。'
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
  const isLoggedIn = localStorage.getItem('token') && localStorage.getItem('userId')
  if (authRoutes.includes(to.path) && !isLoggedIn) {
    next({ path: '/login', query: { redirect: to.fullPath } })
  } else {
    next()
  }
})

router.afterEach((to) => {
  scrollToTop()
  // 栏目页用干净 path 做 canonical；带查询的练习页保留 fullPath
  const pagePath = Object.keys(to.query || {}).length ? (to.fullPath || to.path) : (to.path || '/')
  setPageMeta({
    title: to.meta.title,
    description: to.meta.description,
    path: pagePath,
    noindex: !!to.meta.noindex
  })
  trackPageView(to.path || '/')
})

export default router
