<template>
  <div class="login-page">
    <el-card class="login-card" shadow="hover">
      <!-- Logo -->
      <div class="login-header">
        <el-icon :size="40" color="#409eff"><Headset /></el-icon>
        <h2>{{ isLogin ? '用户登录' : '用户注册' }}</h2>
        <p class="subtitle">英语四六级听力练习平台</p>
      </div>

      <!-- 登录表单 -->
      <el-form
        v-if="isLogin"
        ref="loginFormRef"
        :model="loginForm"
        :rules="loginRules"
        label-position="top"
      >
        <el-form-item label="用户名/邮箱" prop="username">
          <el-input
            v-model="loginForm.username"
            placeholder="请输入用户名或邮箱"
            :prefix-icon="User"
            size="large"
          />
        </el-form-item>

        <el-form-item label="密码" prop="password">
          <el-input
            v-model="loginForm.password"
            type="password"
            placeholder="请输入密码"
            :prefix-icon="Lock"
            size="large"
            show-password
            @keyup.enter="handleLogin"
          />
        </el-form-item>

        <el-form-item>
          <el-button
            type="primary"
            size="large"
            class="submit-btn"
            :loading="loading"
            @click="handleLogin"
          >
            登录
          </el-button>
        </el-form-item>
      </el-form>

      <!-- 注册表单 -->
      <el-form
        v-else
        ref="registerFormRef"
        :model="registerForm"
        :rules="registerRules"
        label-position="top"
      >
        <el-form-item label="用户名" prop="username">
          <el-input
            v-model="registerForm.username"
            placeholder="请输入用户名"
            :prefix-icon="User"
            size="large"
          />
        </el-form-item>

        <el-form-item label="邮箱" prop="email">
          <el-input
            v-model="registerForm.email"
            placeholder="请输入邮箱"
            :prefix-icon="Message"
            size="large"
          />
        </el-form-item>

        <el-form-item label="密码" prop="password">
          <el-input
            v-model="registerForm.password"
            type="password"
            placeholder="请输入密码（至少6位）"
            :prefix-icon="Lock"
            size="large"
            show-password
          />
        </el-form-item>

        <el-form-item label="确认密码" prop="confirmPassword">
          <el-input
            v-model="registerForm.confirmPassword"
            type="password"
            placeholder="请再次输入密码"
            :prefix-icon="Lock"
            size="large"
            show-password
            @keyup.enter="handleRegister"
          />
        </el-form-item>

        <el-form-item>
          <el-button
            type="primary"
            size="large"
            class="submit-btn"
            :loading="loading"
            @click="handleRegister"
          >
            注册
          </el-button>
        </el-form-item>
      </el-form>

      <!-- 切换 -->
      <div class="switch-mode">
        <span v-if="isLogin">
          还没有账号？
          <el-link type="primary" @click="switchMode">立即注册</el-link>
        </span>
        <span v-else>
          已有账号？
          <el-link type="primary" @click="switchMode">立即登录</el-link>
        </span>
      </div>
    </el-card>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessage } from 'element-plus'
import { loginByEmail, loginByUserName, getUserInfo } from "@/api/Auth"
import { register } from '@/api/Auth'
import { User, Lock, Message } from '@element-plus/icons-vue'

const router = useRouter()
const route = useRoute()
const loading = ref(false)
const isLogin = ref(true)

const updateMode = () => {
  isLogin.value = route.query.mode !== 'register'
}

onMounted(updateMode)
watch(() => route.query.mode, updateMode)

// 登录表单
const loginFormRef = ref(null)
const loginForm = reactive({
  username: '',
  password: ''
})

// 注册表单
const registerFormRef = ref(null)
const registerForm = reactive({
  username: '',
  email: '',
  password: '',
  confirmPassword: ''
})

// 登录校验规则
const loginRules = {
  username: [
    { required: true, message: '请输入用户名或邮箱', trigger: 'blur' }
  ],
  password: [
    { required: true, message: '请输入密码', trigger: 'blur' },
    { min: 6, message: '密码至少6位', trigger: 'blur' }
  ]
}

// 注册校验规则
const validateConfirmPassword = (rule, value, callback) => {
  if (value !== registerForm.password) {
    callback(new Error('两次输入的密码不一致'))
  } else {
    callback()
  }
}

const registerRules = {
  username: [
    { required: true, message: '请输入用户名', trigger: 'blur' },
    { min: 3, max: 20, message: '用户名长度3-20位', trigger: 'blur' }
  ],
  email: [
    { required: true, message: '请输入邮箱', trigger: 'blur' },
    { type: 'email', message: '请输入正确的邮箱格式', trigger: 'blur' }
  ],
  password: [
    { required: true, message: '请输入密码', trigger: 'blur' },
    { min: 6, message: '密码至少6位', trigger: 'blur' }
  ],
  confirmPassword: [
    { required: true, message: '请确认密码', trigger: 'blur' },
    { validator: validateConfirmPassword, trigger: 'blur' }
  ]
}

// 切换登录/注册
const switchMode = () => {
  isLogin.value = !isLogin.value
}

// 登录
const handleLogin = async () => {
  if (!loginFormRef.value) return

  const valid = await loginFormRef.value.validate().catch(() => false)
  if (!valid) return

  loading.value = true
  try {
    // 判断是邮箱还是用户名
    const isEmail = loginForm.username.includes('@')
    const api = isEmail ? loginByEmail : loginByUserName

    const res = await api({
      [isEmail ? 'email' : 'userName']: loginForm.username,
      password: loginForm.password
    })

    // 保存登录状态
    localStorage.setItem('token', res.token)
    localStorage.setItem('username', res.userName)

    // 获取用户信息（包含 userId）
    try {
      const userInfo = await getUserInfo()
      if (userInfo && userInfo.id) {
        localStorage.setItem('userId', userInfo.id)
      }
    } catch (e) {
      console.error('获取用户信息失败', e)
    }

    ElMessage.success('登录成功')
    router.push('/')
  } catch (err) {
    // 错误已在拦截器统一提示，这里不需要额外处理
  } finally {
    loading.value = false
  }
}

// 注册
const handleRegister = async () => {
  if (!registerFormRef.value) return

  const valid = await registerFormRef.value.validate().catch(() => false)
  if (!valid) return

  loading.value = true
  try {
    await register({
      userName: registerForm.username,
      email: registerForm.email,
      password: registerForm.password
    })
    ElMessage.success('注册成功，请登录')
    isLogin.value = true  // 切换到登录页
  } catch (err) {
    // 错误已在 request.js 拦截器统一提示
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.login-page {
  min-height: calc(100vh - 60px);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
  position: relative;
  overflow: hidden;
  /* 登录页使用局部暗色变量，保持科技感 */
  --text-primary: #ffffff;
  --text-secondary: #b0b3c7;
  --text-muted: #6b7089;
  --bg-card: rgba(255, 255, 255, 0.05);
  --border-glass: rgba(255, 255, 255, 0.1);
  --accent-cyan: #00d4ff;
  --shadow-glow: 0 0 20px rgba(0, 212, 255, 0.15);
}

/* 浮动光球装饰 */
.login-page::before {
  content: '';
  position: absolute;
  width: 400px;
  height: 400px;
  border-radius: 50%;
  background: radial-gradient(circle, rgba(0, 212, 255, 0.15) 0%, transparent 70%);
  top: -100px;
  left: -100px;
  animation: float 8s ease-in-out infinite;
}

.login-page::after {
  content: '';
  position: absolute;
  width: 300px;
  height: 300px;
  border-radius: 50%;
  background: radial-gradient(circle, rgba(139, 92, 246, 0.12) 0%, transparent 70%);
  bottom: -50px;
  right: -50px;
  animation: float 10s ease-in-out infinite reverse;
}

@keyframes float {
  0%, 100% { transform: translate(0, 0); }
  50% { transform: translate(20px, -20px); }
}

.login-card {
  width: 100%;
  max-width: 420px;
  padding: 32px;
  background: #1e2340 !important;
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  border: 1px solid rgba(255, 255, 255, 0.12) !important;
  border-radius: 16px !important;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3), inset 0 1px 0 rgba(255, 255, 255, 0.08);
  position: relative;
  z-index: 1;
}

.login-header {
  text-align: center;
  margin-bottom: 30px;
}

.login-header :deep(.el-icon) {
  filter: drop-shadow(0 0 10px rgba(0, 212, 255, 0.6));
}

.login-header h2 {
  margin-top: 16px;
  font-size: 26px;
  color: var(--text-primary);
  text-shadow: 0 0 20px rgba(0, 212, 255, 0.2);
}

.subtitle {
  margin-top: 8px;
  color: var(--text-muted);
  font-size: 14px;
}

/* 表单标签 */
.login-card :deep(.el-form-item__label) {
  color: var(--text-secondary);
}

/* 输入框 */
.login-card :deep(.el-input__wrapper) {
  background: rgba(255, 255, 255, 0.85) !important;
  box-shadow: none !important;
  border: 1px solid rgba(200, 210, 230, 0.6) !important;
  border-radius: 8px;
}

.login-card :deep(.el-input__wrapper:hover) {
  border-color: rgba(0, 180, 255, 0.5) !important;
}

.login-card :deep(.el-input__wrapper.is-focus) {
  border-color: #00b4ff !important;
  box-shadow: 0 0 8px rgba(0, 180, 255, 0.2) !important;
  background: rgba(255, 255, 255, 0.95) !important;
}

.login-card :deep(.el-input__inner) {
  color: #1a1a2e !important;
  font-weight: 500;
}

.login-card :deep(.el-input__inner::placeholder) {
  color: #9ca3af;
}

.login-card :deep(.el-input__icon) {
  color: #6b7280;
}

/* 提交按钮 */
.submit-btn {
  width: 100%;
  background: linear-gradient(135deg, var(--accent-blue) 0%, var(--accent-cyan) 100%) !important;
  border: none !important;
  box-shadow: 0 0 16px rgba(64, 158, 255, 0.3);
  transition: all 0.3s;
  border-radius: 8px;
  font-size: 16px;
  letter-spacing: 2px;
}

.submit-btn:hover {
  box-shadow: 0 0 24px rgba(0, 212, 255, 0.5);
  transform: translateY(-1px);
}

/* 切换链接 */
.switch-mode {
  text-align: center;
  margin-top: 20px;
  color: var(--text-muted);
  font-size: 14px;
}

.switch-mode :deep(.el-link) {
  color: var(--accent-cyan);
}
</style>
