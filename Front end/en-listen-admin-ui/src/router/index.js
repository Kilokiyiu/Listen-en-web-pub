import { createRouter, createWebHashHistory } from 'vue-router'

const routes = [
  {
    path: '/login',
    name: 'login',
    component: () => import('../views/LoginView.vue')
  },
  {
    path: '/',
    name: 'upload',
    component: () => import('../views/UploadView.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/manage',
    name: 'manage',
    component: () => import('../views/EpisodeManageView.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/article',
    name: 'article',
    component: () => import('../views/ArticleManageView.vue'),
    meta: { requiresAuth: true }
  }
]

const router = createRouter({
  history: createWebHashHistory(),
  routes
})

// 路由守卫：未登录跳转到登录页
router.beforeEach((to, from, next) => {
  const token = localStorage.getItem('admin_token')
  // 如果是登录页，直接通过
  if (to.path === '/login') {
    next()
    return
  }
  // 其他页面需要登录
  if (to.meta.requiresAuth && !token) {
    next('/login')
  } else {
    next()
  }
})

export default router
