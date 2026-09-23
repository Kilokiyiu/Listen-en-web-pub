import { createRouter, createWebHashHistory } from 'vue-router'
import {
  isLoggedIn,
  isOfflineOnly,
  getActiveWordbook,
  setActiveWordbook,
} from '../services/appSettings'
import { WORDBOOK_TYPES } from '../config'

const routes = [
  {
    path: '/login',
    name: 'login',
    component: () => import('../views/LoginView.vue'),
    meta: { hideNav: true, topBar: { title: '登录', showBack: false } },
  },
  {
    path: '/',
    name: 'words',
    component: () => import('../views/WordsView.vue'),
    meta: { topBar: { title: '我的单词本', showBack: false } },
  },
  {
    path: '/add',
    name: 'add',
    component: () => import('../views/AddWordView.vue'),
    meta: { topBar: { title: '添加单词', showBack: true, backTo: '/' } },
  },
  {
    path: '/review',
    name: 'review',
    component: () => import('../views/ReviewView.vue'),
    meta: { topBar: { title: '复习', showBack: true, backTo: '/' } },
  },
  {
    path: '/settings',
    name: 'settings',
    component: () => import('../views/SettingsView.vue'),
    meta: { topBar: { title: '我的', showBack: true, backTo: '/' } },
  },
  {
    path: '/feedback',
    name: 'feedback',
    component: () => import('../views/FeedbackView.vue'),
    meta: { topBar: { title: '意见反馈', showBack: true, backTo: '/settings' } },
  },
]

const router = createRouter({
  history: createWebHashHistory(),
  routes,
})

router.beforeEach(async (to) => {
  const loggedIn = await isLoggedIn()
  const offlineOnly = await isOfflineOnly()

  if (to.name === 'login') {
    if (loggedIn) return { name: 'words' }
    return true
  }

  if (!loggedIn && !offlineOnly) {
    return { name: 'login', query: { redirect: to.fullPath } }
  }

  if (offlineOnly && (await getActiveWordbook()) === WORDBOOK_TYPES.server) {
    await setActiveWordbook(WORDBOOK_TYPES.local)
  }

  return true
})

export default router
