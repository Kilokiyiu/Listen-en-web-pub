import { createRouter, createWebHashHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import { setPageMeta, DEFAULT_TITLE, DEFAULT_DESCRIPTION } from '../utils/seo'
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
    path: '/exams',
    name: 'exams',
    component: () => import('../views/PaperListView.vue'),
    meta: {
      title: '英语听力真题与学习资料 - ListenEase',
      description: '浏览四六级、雅思、托福英语听力历年真题与学习资料，在线练习并下载试卷、答案 PDF。'
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
    path: '/daily',
    name: 'dailyArticle',
    component: () => import('../views/DailyArticleView.vue'),
    meta: {
      title: '每日英语短文学习资料 - ListenEase',
      description: '每天一篇精选英语短文学习资料，英汉对照阅读，配合听力练习保持学习节奏。'
    }
  },
  {
    path: '/kaoyan',
    name: 'kaoyan',
    component: () => import('../views/kaoyan/KaoyanPaperListView.vue'),
    meta: {
      title: '考研英语模拟练习（开发中）- ListenEase',
      description: '考研英语模块开发中，提供英语一/英语二完形填空与阅读理解模拟在线练习。'
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
    path: '/word-roots',
    name: 'wordRoots',
    component: () => import('../views/WordRootsView.vue'),
    meta: {
      title: '词根词缀学习资料 - ListenEase',
      description: '系统学习英语词根词缀，积累词汇学习资料，扩大词汇量、提升记忆效率。'
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
      title: '官方词本 - 四六级考研高频词 - ListenEase',
      description: '领取四级、六级、考研官方高频词本，一键加入个人单词本，用间隔复习背单词。'
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
      title: 'BBC 英语新闻学习资料 - ListenEase',
      description: '精选 BBC 英语新闻阅读与听力学习资料，提升语感与阅读理解能力。'
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
  }
]

const scrollToTop = () => {
  window.scrollTo(0, 0)
  document.documentElement.scrollTop = 0
  document.body.scrollTop = 0
}

const router = createRouter({
  history: createWebHashHistory(process.env.BASE_URL),
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
    next('/login')
  } else {
    next()
  }
})

router.afterEach((to) => {
  scrollToTop()
  const hashPath = to.path || '/'
  setPageMeta({
    title: to.meta.title,
    description: to.meta.description,
    path: hashPath === '/' ? '/' : `/#${hashPath}`,
    noindex: !!to.meta.noindex
  })
  trackPageView(hashPath)
})

export default router
