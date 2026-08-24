import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/views/Login.vue'),
    meta: { title: '登录' }
  },
  {
    path: '/',
    component: () => import('@/layouts/MainLayout.vue'),
    redirect: '/dashboard',
    children: [
      {
        path: 'dashboard',
        name: 'Dashboard',
        component: () => import('@/views/Dashboard.vue'),
        meta: { title: '工作台' }
      },
      {
        path: 'organization',
        name: 'Organization',
        component: () => import('@/views/Organization.vue'),
        meta: { title: '组织人员' }
      },
      {
        path: 'learning-content',
        name: 'LearningContent',
        component: () => import('@/views/LearningContent.vue'),
        meta: { title: '学习内容' }
      },
      {
        path: 'exam-management',
        name: 'ExamManagement',
        component: () => import('@/views/ExamManagement.vue'),
        meta: { title: '题库测验' }
      },
      {
        path: 'org-life',
        name: 'OrgLife',
        component: () => import('@/views/OrgLife.vue'),
        meta: { title: '组织生活' }
      },
      {
        path: 'data-analysis',
        name: 'DataAnalysis',
        component: () => import('@/views/DataAnalysis.vue'),
        meta: { title: '数据智能分析' }
      }
    ]
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

// 登录守卫
router.beforeEach((to, from, next) => {
  const token = localStorage.getItem('accessToken')
  if (to.path === '/login') {
    next()
  } else if (!token) {
    next('/login')
  } else {
    next()
  }
})

router.afterEach((to) => {
  document.title = to.meta.title ? `${to.meta.title} - 党校学习管理后台` : '党校学习管理后台'
})

export default router
