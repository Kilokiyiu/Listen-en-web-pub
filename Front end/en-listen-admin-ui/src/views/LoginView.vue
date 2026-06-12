<template>
  <div class="login-page">
    <div class="login-panel login-panel--brand">
      <div class="brand-content">
        <div class="brand-logo">
          <el-icon :size="36"><Headset /></el-icon>
        </div>
        <h1>Listen Admin</h1>
        <p class="brand-desc">听力内容管理后台</p>
        <ul class="brand-features">
          <li><el-icon><Upload /></el-icon> 音频上传与管理</li>
          <li><el-icon><Document /></el-icon> 原文与 PDF 维护</li>
          <li><el-icon><Reading /></el-icon> 每日一篇发布</li>
        </ul>
      </div>
    </div>

    <div class="login-panel login-panel--form">
      <div class="login-form-wrap">
        <h2>欢迎回来</h2>
        <p class="form-subtitle">请输入管理员账号登录</p>

        <el-form :model="form" @submit.prevent="handleLogin" label-position="top" size="large">
          <el-form-item label="用户名">
            <el-input v-model="form.userName" placeholder="请输入管理员用户名">
              <template #prefix>
                <el-icon><User /></el-icon>
              </template>
            </el-input>
          </el-form-item>
          <el-form-item label="密码">
            <el-input
              v-model="form.password"
              type="password"
              placeholder="请输入密码"
              show-password
              @keyup.enter="handleLogin"
            >
              <template #prefix>
                <el-icon><Lock /></el-icon>
              </template>
            </el-input>
          </el-form-item>
          <el-button type="primary" @click="handleLogin" :loading="loading" class="login-btn">
            登 录
          </el-button>
        </el-form>
      </div>

      <p class="login-footer">ListenEase · 管理后台</p>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { loginByUserName } from '../api/Admin'
import { ElMessage } from 'element-plus'

const router = useRouter()
const loading = ref(false)
const form = ref({
  userName: '',
  password: ''
})

const handleLogin = async () => {
  if (!form.value.userName || !form.value.password) {
    ElMessage.warning('请输入用户名和密码')
    return
  }
  loading.value = true
  try {
    const res = await loginByUserName(form.value.userName, form.value.password)
    localStorage.setItem('admin_token', res.token)
    localStorage.setItem('admin_userName', res.userName)
    ElMessage.success('登录成功')
    router.replace('/')
  } catch (e) {
    // 错误已在拦截器中处理
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.login-page {
  display: flex;
  min-height: 100vh;
}

.login-panel {
  flex: 1;
  display: flex;
  flex-direction: column;
}

.login-panel--brand {
  background: linear-gradient(135deg, #001529 0%, #003a8c 100%);
  color: #fff;
  justify-content: center;
  align-items: center;
  padding: 48px;
}

.brand-content {
  max-width: 400px;
}

.brand-logo {
  width: 64px;
  height: 64px;
  border-radius: 12px;
  background: var(--admin-primary);
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 24px;
}

.brand-content h1 {
  font-size: 32px;
  font-weight: 700;
  margin-bottom: 8px;
}

.brand-desc {
  font-size: 16px;
  opacity: 0.75;
  margin-bottom: 32px;
}

.brand-features {
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.brand-features li {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 14px;
  opacity: 0.85;
}

.login-panel--form {
  background: #fff;
  justify-content: center;
  align-items: center;
  padding: 48px;
  position: relative;
}

.login-form-wrap {
  width: 100%;
  max-width: 380px;
}

.login-form-wrap h2 {
  font-size: 28px;
  font-weight: 600;
  color: var(--admin-text);
  margin-bottom: 8px;
}

.form-subtitle {
  color: var(--admin-text-secondary);
  margin-bottom: 32px;
  font-size: 14px;
}

.login-btn {
  width: 100%;
  margin-top: 8px;
  height: 44px;
  font-size: 16px;
}

.login-footer {
  position: absolute;
  bottom: 24px;
  color: var(--admin-text-secondary);
  font-size: 13px;
}

@media (max-width: 768px) {
  .login-page {
    flex-direction: column;
  }

  .login-panel--brand {
    padding: 32px 24px;
    min-height: auto;
  }

  .brand-features {
    display: none;
  }

  .login-panel--form {
    flex: 1;
    padding: 32px 24px;
  }
}
</style>
