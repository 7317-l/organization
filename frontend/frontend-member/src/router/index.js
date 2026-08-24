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
    redirect: '/home',
    children: [
      {
        path: 'home',
        name: 'Home',
        component: () => import('@/views/Home.vue'),
        meta: { title: '首页', icon: 'home' }
      },
      {
        path: 'learning',
        name: 'LearningCenter',
        component: () => import('@/views/LearningCenter.vue'),
        meta: { title: '学习中心', icon: 'learning' }
      },
      {
        path: 'content/:id',
        name: 'ContentDetail',
        component: () => import('@/views/ContentDetail.vue'),
        meta: { title: '内容详情', hidden: true }
      },
      {
        path: 'exam',
        name: 'ExamCenter',
        component: () => import('@/views/ExamCenter.vue'),
        meta: { title: '考试中心', icon: 'exam' }
      },
      {
        path: 'quiz/:testId',
        name: 'Quiz',
        component: () => import('@/views/Quiz.vue'),
        meta: { title: '答题', hidden: true }
      },
      {
        path: 'ai-chat',
        name: 'AiChat',
        component: () => import('@/views/AiChat.vue'),
        meta: { title: 'AI党建助手', hidden: true }
      },
      {
        path: 'report',
        name: 'Report',
        component: () => import('@/views/Report.vue'),
        meta: { title: 'AI学习报告', hidden: true }
      },
      {
        path: 'profile',
        name: 'Profile',
        component: () => import('@/views/Profile.vue'),
        meta: { title: '我的', icon: 'profile' }
      }
    ]
  },
  {
    path: '/:pathMatch(.*)*',
    redirect: '/home'
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

// 路由守卫：未登录跳转登录页
router.beforeEach((to, from, next) => {
  const token = localStorage.getItem('accessToken')
  if (to.path === '/login') {
    if (token) {
      next('/home')
    } else {
      next()
    }
  } else {
    if (!token) {
      next('/login')
    } else {
      next()
    }
  }
})

// 页面标题
router.afterEach(to => {
  document.title = to.meta.title ? `${to.meta.title} · 党员学习平台` : '党员学习平台'
})

export default router
