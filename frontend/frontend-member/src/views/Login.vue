<template>
  <div class="login-page">
    <div class="login-bg">
      <div class="bg-pattern"></div>
    </div>
    <div class="login-container">
      <div class="login-left">
        <div class="brand">
          <svg viewBox="0 0 100 100" fill="currentColor" class="brand-logo">
            <circle cx="50" cy="50" r="45" fill="none" stroke="currentColor" stroke-width="2" />
            <path d="M50 20 L54 36 L70 36 L57 46 L62 62 L50 52 L38 62 L43 46 L30 36 L46 36 Z" />
          </svg>
          <h1 class="brand-title">XX党建</h1>
          <p class="brand-subtitle">党员学习平台</p>
        </div>
        <div class="brand-slogan">
          <p>不忘初心 · 牢记使命</p>
          <p>学思想 · 强党性 · 重实践 · 建新功</p>
        </div>
      </div>

      <div class="login-right">
        <div class="login-card">
          <h2 class="login-title">用户登录</h2>
          <p class="login-desc">请使用手机号和密码登录系统</p>

          <el-form
            ref="loginFormRef"
            :model="loginForm"
            :rules="loginRules"
            class="login-form"
            @keyup.enter="handleLogin"
          >
            <el-form-item prop="phone">
              <el-input
                v-model="loginForm.phone"
                placeholder="请输入手机号"
                size="large"
                :prefix-icon="Phone"
                maxlength="11"
              />
            </el-form-item>
            <el-form-item prop="password">
              <el-input
                v-model="loginForm.password"
                type="password"
                placeholder="请输入密码"
                size="large"
                :prefix-icon="Lock"
                show-password
              />
            </el-form-item>
            <el-form-item>
              <el-button
                type="primary"
                size="large"
                class="login-btn"
                :loading="loading"
                @click="handleLogin"
              >
                登 录
              </el-button>
            </el-form-item>
          </el-form>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { Phone, Lock } from '@element-plus/icons-vue'
import { useUserStore } from '@/stores/user'

const router = useRouter()
const userStore = useUserStore()

const loginFormRef = ref(null)
const loading = ref(false)

const loginForm = reactive({
  phone: '',
  password: ''
})

const loginRules = {
  phone: [
    { required: true, message: '请输入手机号', trigger: 'blur' },
    { pattern: /^1[3-9]\d{9}$/, message: '请输入正确的手机号', trigger: 'blur' }
  ],
  password: [
    { required: true, message: '请输入密码', trigger: 'blur' },
    { min: 6, message: '密码长度不能少于6位', trigger: 'blur' }
  ]
}

async function handleLogin() {
  if (!loginFormRef.value) return
  try {
    await loginFormRef.value.validate()
    loading.value = true
    await userStore.login(loginForm.phone, loginForm.password)
    ElMessage.success('登录成功')
    router.push('/home')
  } catch (err) {
    if (err?.message) {
      ElMessage.error(err.message)
    }
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.login-page {
  height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #8B0000 0%, #C8161D 50%, #A01217 100%);
  position: relative;
  overflow: hidden;
}

.login-bg {
  position: absolute;
  inset: 0;
  overflow: hidden;
}

.bg-pattern {
  position: absolute;
  width: 600px;
  height: 600px;
  border-radius: 50%;
  background: radial-gradient(circle, rgba(255, 255, 255, 0.08) 0%, transparent 70%);
  top: -200px;
  right: -100px;
}

.bg-pattern::after {
  content: '';
  position: absolute;
  width: 400px;
  height: 400px;
  border-radius: 50%;
  background: radial-gradient(circle, rgba(212, 168, 67, 0.1) 0%, transparent 70%);
  bottom: -100px;
  left: 100px;
}

.login-container {
  display: flex;
  width: 900px;
  max-width: 90%;
  background: rgba(255, 255, 255, 0.98);
  border-radius: 20px;
  overflow: hidden;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
  position: relative;
  z-index: 1;
}

.login-left {
  flex: 1;
  background: linear-gradient(160deg, #C8161D 0%, #8B0000 100%);
  padding: 60px 40px;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  color: #fff;
  position: relative;
}

.login-left::before {
  content: '';
  position: absolute;
  top: -50px;
  right: -50px;
  width: 200px;
  height: 200px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.05);
}

.brand {
  text-align: center;
}

.brand-logo {
  width: 80px;
  height: 80px;
  color: var(--gold-l);
  margin-bottom: 16px;
}

.brand-title {
  font-size: 32px;
  font-weight: 700;
  margin-bottom: 8px;
  letter-spacing: 4px;
}

.brand-subtitle {
  font-size: 16px;
  opacity: 0.85;
  letter-spacing: 2px;
}

.brand-slogan {
  text-align: center;
  font-size: 14px;
  line-height: 2;
  opacity: 0.8;
}

.login-right {
  flex: 1;
  padding: 60px 50px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.login-card {
  width: 100%;
  max-width: 340px;
}

.login-title {
  font-size: 26px;
  font-weight: 700;
  color: var(--t1);
  margin-bottom: 8px;
}

.login-desc {
  font-size: 14px;
  color: var(--t3);
  margin-bottom: 32px;
}

.login-form {
  width: 100%;
}

.login-btn {
  width: 100%;
  height: 48px;
  font-size: 16px;
  font-weight: 600;
  letter-spacing: 4px;
  margin-top: 8px;
}

@media (max-width: 768px) {
  .login-left {
    display: none;
  }

  .login-container {
    width: 90%;
  }
}
</style>
