<template>
  <div class="login-page">
    <div class="brand">
      <div class="icon">📖</div>
      <h2>EaseWord</h2>
      <p class="brand-desc">
        {{ isLogin ? '听易词 · 登录后可使用云端与本地单词本' : '听易词 · 注册账号，云端单词本多设备同步' }}
      </p>
    </div>

    <!-- 登录 -->
    <form v-if="isLogin" class="form" @submit.prevent="handleLogin">
      <label>
        <span>用户名 / 邮箱</span>
        <input v-model="username" type="text" placeholder="请输入用户名或邮箱" required />
      </label>
      <label>
        <span>密码</span>
        <input v-model="password" type="password" placeholder="至少 6 位" required minlength="6" />
      </label>
      <button type="submit" class="btn-primary" :disabled="loading">
        {{ loading ? '登录中...' : '登录' }}
      </button>
    </form>

    <!-- 注册 -->
    <form v-else class="form" @submit.prevent="handleRegister">
      <label>
        <span>用户名</span>
        <input v-model="regForm.username" type="text" placeholder="3-20 位字符" required minlength="3" maxlength="20" />
      </label>
      <label>
        <span>邮箱</span>
        <input v-model="regForm.email" type="email" placeholder="your@email.com" required />
      </label>
      <label>
        <span>密码</span>
        <input v-model="regForm.password" type="password" placeholder="至少 6 位" required minlength="6" />
      </label>
      <label>
        <span>确认密码</span>
        <input v-model="regForm.confirmPassword" type="password" placeholder="再次输入密码" required minlength="6" />
      </label>
      <button type="submit" class="btn-primary" :disabled="loading">
        {{ loading ? '注册中...' : '注册' }}
      </button>
    </form>

    <p class="switch">
      <template v-if="isLogin">
        还没有账号？
        <button type="button" class="link-btn" @click="switchMode(false)">立即注册</button>
      </template>
      <template v-else>
        已有账号？
        <button type="button" class="link-btn" @click="switchMode(true)">立即登录</button>
      </template>
    </p>

    <template v-if="isLogin">
      <div class="divider"><span>或</span></div>
      <button type="button" class="btn-offline" :disabled="loading" @click="enterOffline">
        离线模式 · 仅使用本地单词本
      </button>
    </template>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { loginByEmail, loginByUserName, getUserInfo, register } from '../api/auth'
import { setAuth, onLoginSuccess, enterOfflineMode } from '../services/appSettings'
import { showToast } from '../utils/toast'

const router = useRouter()
const route = useRoute()
const isLogin = ref(true)
const username = ref('')
const password = ref('')
const loading = ref(false)
const regForm = reactive({
  username: '',
  email: '',
  password: '',
  confirmPassword: '',
})

onMounted(() => {
  isLogin.value = route.query.mode !== 'register'
})

const switchMode = (login) => {
  isLogin.value = login
  const query = { ...route.query }
  if (login) {
    delete query.mode
  } else {
    query.mode = 'register'
  }
  router.replace({ name: 'login', query })
}

const handleLogin = async () => {
  loading.value = true
  try {
    const isEmail = username.value.includes('@')
    const api = isEmail ? loginByEmail : loginByUserName
    const payload = isEmail
      ? { email: username.value, password: password.value }
      : { userName: username.value, password: password.value }
    const res = await api(payload)

    await setAuth({ token: res.token, userId: '', username: res.userName })

    try {
      const info = await getUserInfo()
      if (info?.id) {
        await setAuth({ token: res.token, userId: info.id, username: res.userName })
      }
    } catch (e) {
      console.error(e)
    }

    await onLoginSuccess()
    showToast('登录成功')
    const redirect = route.query.redirect || '/'
    router.replace(redirect)
  } catch (e) {
    showToast(typeof e === 'string' ? e : '登录失败')
  } finally {
    loading.value = false
  }
}

const handleRegister = async () => {
  if (regForm.password !== regForm.confirmPassword) {
    showToast('两次输入的密码不一致')
    return
  }
  loading.value = true
  try {
    await register({
      userName: regForm.username,
      email: regForm.email,
      password: regForm.password,
    })
    showToast('注册成功，请登录')
    username.value = regForm.username
    password.value = ''
    switchMode(true)
  } catch (e) {
    showToast(typeof e === 'string' ? e : '注册失败')
  } finally {
    loading.value = false
  }
}

const enterOffline = async () => {
  await enterOfflineMode()
  showToast('已进入离线模式')
  router.replace('/')
}
</script>

<style scoped>
.login-page {
  min-height: calc(100vh - 48px);
  padding: 24px 20px 32px;
}

.brand {
  text-align: center;
  margin-bottom: 28px;
}

.brand .icon {
  font-size: 52px;
  margin-bottom: 8px;
}

.brand h2 {
  font-size: 22px;
  margin: 0 0 8px;
}

.brand-desc {
  color: var(--text-secondary);
  font-size: 14px;
  margin: 0;
  line-height: 1.5;
}

.form {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

label {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

label span {
  font-size: 14px;
  color: var(--text-secondary);
}

input {
  padding: 12px 14px;
  border: 1px solid var(--border);
  border-radius: 10px;
  font-size: 16px;
  background: var(--bg-card);
}

.btn-primary {
  margin-top: 8px;
  padding: 14px;
  background: var(--primary);
  color: #fff;
  border: none;
  border-radius: 10px;
  font-size: 16px;
  font-weight: 600;
}

.btn-primary:disabled {
  opacity: 0.6;
}

.switch {
  text-align: center;
  margin-top: 20px;
  font-size: 14px;
  color: var(--text-secondary);
}

.link-btn {
  background: none;
  border: none;
  color: var(--primary);
  font-size: 14px;
  padding: 0;
  font-weight: 500;
}

.divider {
  display: flex;
  align-items: center;
  margin: 20px 0;
  color: var(--text-muted);
  font-size: 13px;
}

.divider::before,
.divider::after {
  content: '';
  flex: 1;
  height: 1px;
  background: var(--border);
}

.divider span {
  padding: 0 12px;
}

.btn-offline {
  width: 100%;
  padding: 14px;
  background: var(--bg-card);
  color: var(--text-primary);
  border: 1px solid var(--border);
  border-radius: 10px;
  font-size: 15px;
  font-weight: 500;
}

.btn-offline:disabled {
  opacity: 0.6;
}
</style>
