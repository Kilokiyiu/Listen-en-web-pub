import { createRouter, createWebHashHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'

const routes = [
  {
    path: '/',
    name: 'home',
    component: HomeView
  },
  {
    path: '/exams',
    name: 'exams',
    component: () => import('../views/PaperListView.vue')
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
    component: () => import('../views/DailyArticleView.vue')
  }
]

const router = createRouter({
  history: createWebHashHistory(process.env.BASE_URL),
  routes
})

export default router
