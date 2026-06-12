<template>
  <div class="login-page le-page">
    <div class="login-wrap">
      <div class="login-brand">
        <div class="brand-icon">
          <el-icon :size="32"><Headset /></el-icon>
        </div>
        <h1>ListenEase</h1>
        <p>专业英语听力练习平台</p>
      </div>

      <div class="login-card le-card">
        <h2>{{ isLogin ? '欢迎回来' : '创建账号' }}</h2>
        <p class="card-sub">{{ isLogin ? '登录后继续你的学习进度' : '注册即可使用单词本与学习记录' }}</p>

        <el-form v-if="isLogin" ref="loginFormRef" :model="loginForm" :rules="loginRules" label-position="top">
          <el-form-item label="用户名 / 邮箱" prop="username">
            <el-input v-model="loginForm.username" placeholder="请输入用户名或邮箱" :prefix-icon="User" size="large" />
          </el-form-item>
          <el-form-item label="密码" prop="password">
            <el-input v-model="loginForm.password" type="password" placeholder="请输入密码" :prefix-icon="Lock" size="large" show-password @keyup.enter="handleLogin" />
          </el-form-item>
          <el-button type="primary" size="large" class="submit-btn le-btn-gradient" :loading="loading" @click="handleLogin">登录</el-button>
        </el-form>

        <el-form v-else ref="registerFormRef" :model="registerForm" :rules="registerRules" label-position="top">
          <el-form-item label="用户名" prop="username">
            <el-input v-model="registerForm.username" placeholder="3-20 位字符" :prefix-icon="User" size="large" />
          </el-form-item>
          <el-form-item label="邮箱" prop="email">
            <el-input v-model="registerForm.email" placeholder="your@email.com" :prefix-icon="Message" size="large" />
          </el-form-item>
          <el-form-item label="密码" prop="password">
            <el-input v-model="registerForm.password" type="password" placeholder="至少 6 位" :prefix-icon="Lock" size="large" show-password />
          </el-form-item>
          <el-form-item label="确认密码" prop="confirmPassword">
            <el-input v-model="registerForm.confirmPassword" type="password" placeholder="再次输入密码" :prefix-icon="Lock" size="large" show-password @keyup.enter="handleRegister" />
          </el-form-item>
          <el-button type="primary" size="large" class="submit-btn le-btn-gradient" :loading="loading" @click="handleRegister">注册</el-button>
        </el-form>

        <div class="switch-mode">
          <span v-if="isLogin">还没有账号？<el-link type="primary" @click="switchMode">立即注册</el-link></span>
          <span v-else>已有账号？<el-link type="primary" @click="switchMode">立即登录</el-link></span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessage } from 'element-plus'
import { loginByEmail, loginByUserName, getUserInfo, register } from '@/api/Auth'
import { User, Lock, Message } from '@element-plus/icons-vue'

const router = useRouter()
const route = useRoute()
const loading = ref(false)
const isLogin = ref(true)

const updateMode = () => { isLogin.value = route.query.mode !== 'register' }
onMounted(updateMode)
watch(() => route.query.mode, updateMode)

const loginFormRef = ref(null)
const loginForm = reactive({ username: '', password: '' })
const registerFormRef = ref(null)
const registerForm = reactive({ username: '', email: '', password: '', confirmPassword: '' })

const loginRules = {
  username: [{ required: true, message: '请输入用户名或邮箱', trigger: 'blur' }],
  password: [{ required: true, message: '请输入密码', trigger: 'blur' }, { min: 6, message: '密码至少6位', trigger: 'blur' }]
}

const validateConfirmPassword = (rule, value, callback) => {
  if (value !== registerForm.password) callback(new Error('两次输入的密码不一致'))
  else callback()
}

const registerRules = {
  username: [{ required: true, message: '请输入用户名', trigger: 'blur' }, { min: 3, max: 20, message: '用户名长度3-20位', trigger: 'blur' }],
  email: [{ required: true, message: '请输入邮箱', trigger: 'blur' }, { type: 'email', message: '请输入正确的邮箱格式', trigger: 'blur' }],
  password: [{ required: true, message: '请输入密码', trigger: 'blur' }, { min: 6, message: '密码至少6位', trigger: 'blur' }],
  confirmPassword: [{ required: true, message: '请确认密码', trigger: 'blur' }, { validator: validateConfirmPassword, trigger: 'blur' }]
}

const switchMode = () => { isLogin.value = !isLogin.value }

const handleLogin = async () => {
  if (!loginFormRef.value) return
  const valid = await loginFormRef.value.validate().catch(() => false)
  if (!valid) return
  loading.value = true
  try {
    const isEmail = loginForm.username.includes('@')
    const api = isEmail ? loginByEmail : loginByUserName
    const res = await api({ [isEmail ? 'email' : 'userName']: loginForm.username, password: loginForm.password })
    localStorage.setItem('token', res.token)
    localStorage.setItem('username', res.userName)
    try {
      const userInfo = await getUserInfo()
      if (userInfo?.id) localStorage.setItem('userId', userInfo.id)
    } catch (e) { console.error(e) }
    ElMessage.success('登录成功')
    const redirect = typeof route.query.redirect === 'string' ? route.query.redirect : ''
    router.push(redirect || '/')
  } finally {
    loading.value = false
  }
}

const handleRegister = async () => {
  if (!registerFormRef.value) return
  const valid = await registerFormRef.value.validate().catch(() => false)
  if (!valid) return
  loading.value = true
  try {
    await register({ userName: registerForm.username, email: registerForm.email, password: registerForm.password })
    ElMessage.success('注册成功，请登录')
    isLogin.value = true
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.login-page {
  min-height: calc(100vh - var(--le-header-h));
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 24px 0;
}

.login-wrap {
  width: 100%;
  max-width: 440px;
  padding: 0 4px;
}

.login-brand {
  text-align: center;
  margin-bottom: 24px;
}

.brand-icon {
  width: 64px;
  height: 64px;
  margin: 0 auto 12px;
  border-radius: 18px;
  background: var(--le-gradient);
  display: flex;
  align-items: center;
  justify-content: center;
  color: #fff;
  box-shadow: 0 8px 24px rgba(37, 99, 235, 0.35);
}

.login-brand h1 {
  font-size: 24px;
  margin: 0 0 4px;
}

.login-brand p {
  color: var(--le-text-muted);
  font-size: 14px;
  margin: 0;
}

.login-card {
  padding: 28px 24px;
}

.login-card h2 {
  font-size: 20px;
  margin: 0 0 4px;
}

.card-sub {
  font-size: 13px;
  color: var(--le-text-muted);
  margin: 0 0 24px;
}

.submit-btn {
  width: 100%;
  margin-top: 8px;
}

.switch-mode {
  text-align: center;
  margin-top: 20px;
  font-size: 14px;
  color: var(--le-text-muted);
}
</style>
