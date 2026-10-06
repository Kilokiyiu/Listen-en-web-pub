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
        path: 'listen',
        name: 'listen',
        component: () => import('../views/ListenContentView.vue'),
        meta: { title: '听力内容' },
      },
      // 旧入口兼容
      { path: 'upload', redirect: '/listen' },
      { path: 'manage', redirect: '/listen' },
      {
        path: 'article',
        name: 'article',
        component: () => import('../views/ArticleManageView.vue'),
        meta: { title: '每日一篇' },
      },
      {
        path: 'word-packs',
        name: 'wordPacks',
        component: () => import('../views/WordPackManageView.vue'),
        meta: { title: '官方词本' },
      },
      {
        path: 'kaoyan',
        name: 'kaoyan',
        component: () => import('../views/KaoyanManageView.vue'),
        meta: { title: '考研内容' },
      },
      {
        path: 'users',
        name: 'users',
        component: () => import('../views/UserManageView.vue'),
        meta: { title: '用户管理' },
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
