import axios from 'axios'
import { ElMessage } from 'element-plus'
import router from '@/router'

const request = axios.create({
  baseURL: 'http://localhost:5091/api/v1',
  timeout: 15000
})

// 请求拦截器：自动添加 Bearer token
request.interceptors.request.use(
  config => {
    const token = localStorage.getItem('accessToken')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  error => {
    return Promise.reject(error)
  }
)

// 响应拦截器：统一处理错误和数据提取
request.interceptors.response.use(
  response => {
    // 文件下载等二进制流直接返回
    if (response.config.responseType === 'blob') {
      return response
    }
    const res = response.data
    // 假设后端统一返回格式：{ code, data, message }
    // code 为 200 或 0 表示成功
    if (res.code !== undefined && res.code !== 200 && res.code !== 0) {
      ElMessage.error(res.message || '请求失败')
      return Promise.reject(new Error(res.message || 'Error'))
    }
    // 成功则返回 data 字段；若没有 data 字段则返回整个响应体
    return res.data !== undefined ? res.data : res
  },
  error => {
    if (error.response) {
      const status = error.response.status
      const message = error.response.data?.message || error.response.data?.msg
      if (status === 401) {
        localStorage.removeItem('accessToken')
        ElMessage.error('登录已过期，请重新登录')
        router.push('/login')
      } else if (status === 403) {
        ElMessage.error(message || '没有权限访问')
      } else if (status === 404) {
        ElMessage.error(message || '请求的资源不存在')
      } else if (status >= 500) {
        ElMessage.error(message || '服务器内部错误')
      } else {
        ElMessage.error(message || '请求失败')
      }
    } else if (error.code === 'ECONNABORTED') {
      ElMessage.error('请求超时，请稍后重试')
    } else {
      ElMessage.error('网络错误，请检查网络连接')
    }
    return Promise.reject(error)
  }
)

export default request
