import { createRouter, createWebHashHistory } from 'vue-router'

const routes = [
  {
    path: '/login',
    name: 'login',
    component: () => import('../views/LoginView.vue'),
    meta: { title: '登录' },
  },
  {
    path: '/',
    component: () => import('../layouts/AdminLayout.vue'),
    meta: { requiresAuth: true },
    children: [
      {
        path: '',
        name: 'dashboard',
        component: () => import('../views/DashboardView.vue'),
        meta: { title: '数据概览' },
      },
      {
        path: 'upload',
        name: 'upload',
        component: () => import('../views/UploadView.vue'),
        meta: { title: '音频上传' },
      },
      {
        path: 'manage',
        name: 'manage',
        component: () => import('../views/EpisodeManageView.vue'),
        meta: { title: '音频管理' },
      },
      {
        path: 'article',
        name: 'article',
        component: () => import('../views/ArticleManageView.vue'),
        meta: { title: '每日一篇' },
      },
    ],
  },
]

const router = createRouter({
  history: createWebHashHistory(),
  routes,
})

router.beforeEach((to, from, next) => {
  const token = localStorage.getItem('admin_token')
  if (to.path === '/login') {
    next()
    return
  }
  if (to.meta.requiresAuth && !token) {
    next('/login')
  } else {
    next()
  }
})

export default router
