<template>
  <div class="login-container">
    <div class="login-card">
      <h2>听力管理后台</h2>
      <p class="subtitle">管理员登录</p>
      <el-form :model="form" @submit.prevent="handleLogin" label-position="top">
        <el-form-item label="用户名">
          <el-input v-model="form.userName" placeholder="请输入管理员用户名" />
        </el-form-item>
        <el-form-item label="密码">
          <el-input v-model="form.password" type="password" placeholder="请输入密码" show-password />
        </el-form-item>
        <el-button type="primary" @click="handleLogin" :loading="loading" style="width: 100%">
          登录
        </el-button>
      </el-form>
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
    // 触发自定义事件，通知 App.vue 更新登录状态
    window.dispatchEvent(new CustomEvent('auth-login'))
    // 使用 Vue Router 导航到首页
    router.replace('/')
  } catch (e) {
    // 错误已在拦截器中处理
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.login-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background: linear-gradient(135deg, #0f0c29 0%, #302b63 50%, #24243e 100%);
}
.login-card {
  background: #fff;
  border-radius: 16px;
  padding: 48px 40px;
  width: 400px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
}
.login-card h2 {
  text-align: center;
  margin-bottom: 4px;
  color: #1a1a2e;
  font-size: 24px;
}
.subtitle {
  text-align: center;
  color: #8a8aaa;
  margin-bottom: 32px;
  font-size: 14px;
}
</style>
