import { createRouter, createWebHashHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import { setPageMeta, DEFAULT_TITLE } from '../utils/seo'

const routes = [
  {
    path: '/',
    name: 'home',
    component: HomeView,
    meta: {
      title: DEFAULT_TITLE,
      description: 'ListenEase 是专业的英语听力练习平台，提供四六级、雅思、托福历年真题、每日短文与智能学习功能。'
    }
  },
  {
    path: '/exams',
    name: 'exams',
    component: () => import('../views/PaperListView.vue'),
    meta: {
      title: '听力真题列表 - ListenEase',
      description: '浏览四六级、雅思、托福英语听力历年真题与模拟试卷，在线练习备考。'
    }
  },
  {
    path: '/exam',
    name: 'examDetail',
    component: () => import('../views/QuestionView.vue')
  },
  {
    path: '/login',
    name: 'login',
    component: () => import('../views/LoginView.vue')
  },
  {
    path: '/profile',
    name: 'profile',
    component: () => import('../views/UserProfileView.vue')
  },
  {
    path: '/history',
    name: 'history',
    component: () => import('../views/StudyRecordView.vue')
  },
  {
    path: '/daily',
    name: 'dailyArticle',
    component: () => import('../views/DailyArticleView.vue'),
    meta: {
      title: '每日一篇短文 - ListenEase',
      description: '每天一篇精选英语短文，配合听力与阅读练习，保持学习状态。'
    }
  },
  {
    path: '/word-roots',
    name: 'wordRoots',
    component: () => import('../views/WordRootsView.vue'),
    meta: {
      title: '词根学习 - ListenEase',
      description: '通过词根词缀系统学习英语词汇，扩大词汇量、提升记忆效率。'
    }
  },
  {
    path: '/word-roots/:id',
    name: 'wordRootDetail',
    component: () => import('../views/WordRootDetailView.vue')
  },
  {
    path: '/my-words',
    name: 'myWords',
    component: () => import('../views/MyWordsView.vue')
  },
  {
    path: '/word-review',
    name: 'wordReview',
    component: () => import('../views/WordReviewView.vue')
  },
  {
    path: '/bbc-news',
    name: 'bbcNews',
    component: () => import('../views/BBCNewsView.vue'),
    meta: {
      title: 'BBC 每日新闻 - ListenEase',
      description: '精选 BBC 英语新闻阅读与听力材料，提升语感与阅读理解能力。'
    }
  }
]

const router = createRouter({
  history: createWebHashHistory(process.env.BASE_URL),
  routes
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
  const hashPath = to.path || '/'
  setPageMeta({
    title: to.meta.title,
    description: to.meta.description,
    path: hashPath === '/' ? '/' : `/#${hashPath}`
  })
})

export default router
