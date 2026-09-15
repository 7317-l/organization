<template>
  <div class="login-page">
    <div class="login-bg"></div>
    <div class="login-card">
      <div class="login-header">
        <el-icon :size="40" color="#C8161D"><Flag /></el-icon>
        <h1 class="login-title">党校学习双端系统</h1>
        <p class="login-subtitle">管理后台登录</p>
      </div>

      <el-form
        ref="formRef"
        :model="form"
        :rules="rules"
        label-width="0"
        @submit.prevent="handleLogin"
      >
        <el-form-item prop="phone">
          <el-input
            v-model="form.phone"
            placeholder="请输入手机号"
            size="large"
            :prefix-icon="User"
          />
        </el-form-item>
        <el-form-item prop="password">
          <el-input
            v-model="form.password"
            type="password"
            placeholder="请输入密码"
            size="large"
            show-password
            :prefix-icon="Lock"
            @keyup.enter="handleLogin"
          />
        </el-form-item>
        <el-button
          type="primary"
          size="large"
          class="login-btn"
          :loading="loading"
          @click="handleLogin"
        >
          登 录
        </el-button>
      </el-form>

      <div class="login-footer">
        <span>© 2026 党校学习双端系统</span>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { User, Lock } from '@element-plus/icons-vue'
import { useUserStore } from '@/stores/user'

const router = useRouter()
const userStore = useUserStore()

const formRef = ref(null)
const loading = ref(false)

const form = reactive({
  phone: '',
  password: ''
})

const rules = {
  phone: [
    { required: true, message: '请输入手机号', trigger: 'blur' },
    { pattern: /^1[3-9]\d{9}$/, message: '手机号格式不正确', trigger: 'blur' }
  ],
  password: [
    { required: true, message: '请输入密码', trigger: 'blur' },
    { min: 6, message: '密码至少6位', trigger: 'blur' }
  ]
}

async function handleLogin() {
  if (!formRef.value) return
  await formRef.value.validate(async (valid) => {
    if (!valid) return
    loading.value = true
    try {
      await userStore.login(form.phone, form.password)
      ElMessage.success('登录成功')
      router.push('/dashboard')
    } catch (e) {
      // 错误已在拦截器提示
    } finally {
      loading.value = false
    }
  })
}
</script>

<style scoped>
.login-page {
  position: relative;
  height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
}
.login-bg {
  position: absolute;
  inset: 0;
  background: linear-gradient(135deg, #C8161D 0%, #8B0F14 50%, #5C0A0E 100%);
  z-index: 0;
}
.login-bg::before {
  content: '';
  position: absolute;
  inset: 0;
  background-image: radial-gradient(circle at 20% 30%, rgba(255,255,255,0.08) 0%, transparent 40%),
                    radial-gradient(circle at 80% 70%, rgba(255,255,255,0.06) 0%, transparent 40%);
}
.login-card {
  position: relative;
  z-index: 1;
  width: 420px;
  background: #fff;
  border-radius: 12px;
  padding: 48px 40px 32px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
}
.login-header {
  text-align: center;
  margin-bottom: 32px;
}
.login-title {
  margin: 12px 0 4px;
  color: #C8161D;
  font-size: 22px;
  font-weight: 700;
}
.login-subtitle {
  margin: 0;
  color: #999;
  font-size: 14px;
}
.login-btn {
  width: 100%;
  background: #C8161D;
  border-color: #C8161D;
  font-size: 16px;
  letter-spacing: 4px;
  margin-top: 8px;
}
.login-btn:hover {
  background: #A01016;
  border-color: #A01016;
}
.login-footer {
  text-align: center;
  margin-top: 24px;
  color: #bbb;
  font-size: 12px;
}
</style>
