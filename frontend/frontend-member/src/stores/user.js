import { defineStore } from 'pinia'
import { login as apiLogin, getMe } from '@/api/auth'
import { roleText } from '@/utils/format'

export const useUserStore = defineStore('user', {
  state: () => ({
    token: localStorage.getItem('accessToken') || '',
    userInfo: null
  }),

  getters: {
    isLoggedIn: state => !!state.token,
    userName: state => state.userInfo?.name || state.userInfo?.nickname || '',
    userBranch: state => state.userInfo?.branchName || state.userInfo?.organizationName || state.userInfo?.organization || '',
    userRole: state => {
      const raw = state.userInfo?.roleName || state.userInfo?.role
      return roleText(raw)
    }
  },

  actions: {
    /** 登录 */
    async login(phone, password) {
      const data = await apiLogin(phone, password)
      this.token = data.accessToken
      localStorage.setItem('accessToken', data.accessToken)
      await this.fetchUserInfo()
      return data
    },

    /** 获取当前用户信息 */
    async fetchUserInfo() {
      const data = await getMe()
      this.userInfo = data
      return data
    },

    /** 退出登录 */
    logout() {
      this.token = ''
      this.userInfo = null
      localStorage.removeItem('accessToken')
    }
  }
})
