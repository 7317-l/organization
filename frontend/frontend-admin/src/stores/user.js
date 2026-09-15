import { defineStore } from 'pinia'
import { login as apiLogin, getCurrentUser } from '@/api/auth'

export const useUserStore = defineStore('user', {
  state: () => ({
    accessToken: localStorage.getItem('accessToken') || '',
    refreshToken: localStorage.getItem('refreshToken') || '',
    userInfo: null
  }),
  getters: {
    isLoggedIn: (state) => !!state.accessToken
  },
  actions: {
    async login(phone, password) {
      const res = await apiLogin({ phone, password })
      this.accessToken = res.accessToken
      this.refreshToken = res.refreshToken
      localStorage.setItem('accessToken', res.accessToken)
      localStorage.setItem('refreshToken', res.refreshToken)
      return res
    },
    async fetchUserInfo() {
      const res = await getCurrentUser()
      this.userInfo = res
      return res
    },
    logout() {
      this.accessToken = ''
      this.refreshToken = ''
      this.userInfo = null
      localStorage.removeItem('accessToken')
      localStorage.removeItem('refreshToken')
    }
  }
})
