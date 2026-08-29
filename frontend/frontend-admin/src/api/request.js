import axios from 'axios'
import { ElMessage } from 'element-plus'
import router from '@/router'

const BASE_URL = 'http://localhost:5091/api/v1'

const request = axios.create({
  baseURL: BASE_URL,
  timeout: 30000
})

// 请求拦截器：附加 JWT
request.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('accessToken')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => Promise.reject(error)
)

// 响应拦截器：统一处理 code !== 200
request.interceptors.response.use(
  (response) => {
    const res = response.data
    // 兼容直接返回数据 / { code, data, message } 两种结构
    if (res && typeof res === 'object' && 'code' in res) {
      if (res.code === 200 || res.code === 0) {
        // 分页响应（PagedResponse：{ code, data:[...], page, size, total }）保留外层结构，
        // 供各视图取 res.data（数组）与 res.total（总数），避免解包成裸数组后列表为空
        if (res.total !== undefined && (res.page !== undefined || res.size !== undefined)) {
          return res
        }
        return res.data !== undefined ? res.data : res
      }
      ElMessage.error(res.message || res.msg || '请求失败')
      return Promise.reject(new Error(res.message || res.msg || '请求失败'))
    }
    return res
  },
  (error) => {
    const status = error.response?.status
    if (status === 401) {
      ElMessage.error('登录已过期，请重新登录')
      localStorage.removeItem('accessToken')
      localStorage.removeItem('refreshToken')
      router.push('/login')
    } else if (status === 403) {
      ElMessage.error('没有权限执行该操作')
    } else {
      ElMessage.error(error.response?.data?.message || error.message || '网络错误')
    }
    return Promise.reject(error)
  }
)

export default request
