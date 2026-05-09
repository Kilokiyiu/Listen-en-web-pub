<template>
  <div class="profile-page">
    <!-- 顶部导航 -->
    <div class="page-header">
      <el-button text @click="$router.back()" class="back-btn">
        <el-icon><ArrowLeft /></el-icon> 返回
      </el-button>
    </div>

    <!-- 标题区 -->
    <div class="title-section">
      <div class="title-icon">
        <el-icon :size="28" color="#409eff"><UserFilled /></el-icon>
      </div>
      <div class="title-text">
        <h1 class="page-title">个人中心</h1>
        <p class="page-subtitle">管理你的账号信息与安全设置</p>
      </div>
    </div>

    <div class="profile-container">
      <!-- 左侧：用户信息卡片 -->
      <div class="profile-sidebar">
        <div class="user-card">
          <div class="user-card-bg"></div>
          <div class="user-card-content">
            <div class="avatar-wrapper">
              <el-avatar :size="72" :icon="UserFilled" />
            </div>
            <h3 class="user-name">{{ userInfo.userName || username }}</h3>
            <p class="user-email">{{ userInfo.email || '未绑定邮箱' }}</p>
            <div class="join-badge">
              <el-icon><Calendar /></el-icon>
              <span>{{ formatDate(userInfo.creationTime) }} 加入</span>
            </div>
          </div>
        </div>

        <div class="stat-card">
          <div class="stat-grid">
            <div class="stat-item">
              <div class="stat-icon-wrap icon-blue">
                <el-icon :size="18" color="#409eff"><Document /></el-icon>
              </div>
              <div class="stat-value">{{ stats.totalExams }}</div>
              <div class="stat-label">完成试卷</div>
            </div>
            <div class="stat-item">
              <div class="stat-icon-wrap icon-green">
                <el-icon :size="18" color="#67c23a"><Timer /></el-icon>
              </div>
              <div class="stat-value">{{ stats.totalMinutes }}</div>
              <div class="stat-label">学习时长(分)</div>
            </div>
            <div class="stat-item">
              <div class="stat-icon-wrap icon-orange">
                <el-icon :size="18" color="#e6a23c"><TrendCharts /></el-icon>
              </div>
              <div class="stat-value">{{ stats.avgScore }}</div>
              <div class="stat-label">平均分</div>
            </div>
          </div>
        </div>
      </div>

      <!-- 右侧：内容区 -->
      <div class="profile-main">
        <div class="section-card">
          <div class="section-header">
            <div class="section-title-wrap">
              <div class="title-bar"></div>
              <span class="section-title">账号安全</span>
            </div>
            <div class="section-badge">
              <el-icon><Lock /></el-icon>
              <span>密码管理</span>
            </div>
          </div>

          <el-form :model="pwdForm" :rules="pwdRules" ref="pwdFormRef" label-width="100px" class="pwd-form">
            <el-form-item label="原密码" prop="oldPassword">
              <el-input v-model="pwdForm.oldPassword" type="password" show-password placeholder="请输入原密码" />
            </el-form-item>
            <el-form-item label="新密码" prop="newPassword">
              <el-input v-model="pwdForm.newPassword" type="password" show-password placeholder="请输入新密码（至少6位）" />
            </el-form-item>
            <el-form-item label="确认密码" prop="confirmPassword">
              <el-input v-model="pwdForm.confirmPassword" type="password" show-password placeholder="请再次输入新密码" />
            </el-form-item>
            <el-form-item>
              <el-button type="primary" @click="handleChangePwd" :loading="pwdLoading">
                <el-icon><Check /></el-icon>
                确认修改
              </el-button>
            </el-form-item>
          </el-form>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { UserFilled, ArrowLeft, Lock, Check, Calendar, Document, Timer, TrendCharts } from '@element-plus/icons-vue'
import { getUserInfo } from '@/api/Auth'
import request from '@/api/Request'

const username = ref(localStorage.getItem('username') || '')
const userInfo = ref({})
const pwdLoading = ref(false)
const pwdFormRef = ref()

const pwdForm = reactive({
  oldPassword: '',
  newPassword: '',
  confirmPassword: ''
})

const validateConfirmPwd = (rule, value, callback) => {
  if (value !== pwdForm.newPassword) {
    callback(new Error('两次输入的密码不一致'))
  } else {
    callback()
  }
}

const pwdRules = {
  oldPassword: [{ required: true, message: '请输入原密码', trigger: 'blur' }],
  newPassword: [
    { required: true, message: '请输入新密码', trigger: 'blur' },
    { min: 6, message: '密码至少6位', trigger: 'blur' }
  ],
  confirmPassword: [
    { required: true, message: '请确认密码', trigger: 'blur' },
    { validator: validateConfirmPwd, trigger: 'blur' }
  ]
}

// Mock 统计数据
const stats = ref({
  totalExams: 12,
  totalMinutes: 356,
  avgScore: 78
})

const fetchUserInfo = async () => {
  try {
    const res = await getUserInfo()
    userInfo.value = res
  } catch (err) {
    // 接口失败时使用本地数据
    userInfo.value = {
      userName: username.value,
      email: '',
      creationTime: new Date().toISOString()
    }
  }
}

const handleChangePwd = async () => {
  const valid = await pwdFormRef.value.validate().catch(() => false)
  if (!valid) return

  pwdLoading.value = true
  try {
    await request.post('/Login/ChangePwd', {
      oldPassword: pwdForm.oldPassword,
      newPassword: pwdForm.newPassword
    })
    ElMessage.success('密码修改成功')
    pwdForm.oldPassword = ''
    pwdForm.newPassword = ''
    pwdForm.confirmPassword = ''
  } catch (err) {
    // 错误已在 request 拦截器提示
  } finally {
    pwdLoading.value = false
  }
}

const formatDate = (dateStr) => {
  if (!dateStr) return '未知'
  const date = new Date(dateStr)
  return date.toLocaleDateString('zh-CN')
}

onMounted(fetchUserInfo)
</script>

<style scoped>
.profile-page {
  padding: 0;
}

/* 顶部导航 */
.page-header {
  padding: 16px 24px;
  border-bottom: 1px solid #eef1f6;
}

.back-btn {
  color: var(--text-secondary) !important;
  font-size: 14px;
}

.back-btn:hover {
  color: var(--accent-blue) !important;
}

/* 标题区 */
.title-section {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 28px 28px 20px;
}

.title-icon {
  width: 48px;
  height: 48px;
  border-radius: 14px;
  background: linear-gradient(135deg, rgba(64, 158, 255, 0.1) 0%, rgba(0, 168, 232, 0.1) 100%);
  display: flex;
  align-items: center;
  justify-content: center;
}

.title-text {
  flex: 1;
}

.page-title {
  margin: 0;
  font-size: 24px;
  font-weight: 700;
  color: var(--text-primary);
}

.page-subtitle {
  margin: 4px 0 0;
  font-size: 13px;
  color: var(--text-muted);
}

/* 布局 */
.profile-container {
  max-width: 1000px;
  margin: 0 auto;
  display: flex;
  gap: 20px;
  padding: 0 28px 24px;
}

.profile-sidebar {
  width: 300px;
  flex-shrink: 0;
}

/* 用户卡片 */
.user-card {
  position: relative;
  border-radius: 14px;
  overflow: hidden;
  background: var(--bg-card);
  border: 1px solid var(--border-glass);
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.04);
}

.user-card-bg {
  height: 80px;
  background: linear-gradient(135deg, #1e2340 0%, #2d3a6e 50%, #409eff 100%);
  position: relative;
}

.user-card-bg::before {
  content: '';
  position: absolute;
  width: 120px;
  height: 120px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.06);
  top: -40px;
  right: -20px;
}

.user-card-bg::after {
  content: '';
  position: absolute;
  width: 80px;
  height: 80px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.04);
  bottom: -30px;
  left: 20px;
}

.user-card-content {
  text-align: center;
  padding: 0 20px 20px;
  margin-top: -36px;
  position: relative;
}

.avatar-wrapper {
  display: inline-block;
  border-radius: 50%;
  padding: 3px;
  background: #fff;
  box-shadow: 0 4px 16px rgba(64, 158, 255, 0.2);
}

.avatar-wrapper :deep(.el-avatar) {
  background: linear-gradient(135deg, #409eff, #00a8e8);
}

.user-name {
  margin: 12px 0 4px;
  font-size: 18px;
  font-weight: 600;
  color: var(--text-primary);
}

.user-email {
  margin: 0 0 10px;
  font-size: 13px;
  color: var(--text-muted);
}

.join-badge {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 4px 14px;
  border-radius: 20px;
  background: rgba(64, 158, 255, 0.08);
  color: var(--accent-blue);
  font-size: 12px;
}

/* 统计卡片 */
.stat-card {
  margin-top: 16px;
  border-radius: 14px;
  background: var(--bg-card);
  border: 1px solid var(--border-glass);
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.04);
  padding: 20px 16px;
}

.stat-grid {
  display: flex;
  justify-content: space-around;
}

.stat-item {
  text-align: center;
}

.stat-icon-wrap {
  width: 32px;
  height: 32px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 6px;
}

.stat-icon-wrap.icon-blue {
  background: linear-gradient(135deg, rgba(64, 158, 255, 0.12) 0%, rgba(64, 158, 255, 0.06) 100%);
}

.stat-icon-wrap.icon-green {
  background: linear-gradient(135deg, rgba(103, 194, 58, 0.12) 0%, rgba(103, 194, 58, 0.06) 100%);
}

.stat-icon-wrap.icon-orange {
  background: linear-gradient(135deg, rgba(230, 162, 60, 0.12) 0%, rgba(230, 162, 60, 0.06) 100%);
}

.stat-value {
  font-size: 22px;
  font-weight: 700;
  background: linear-gradient(135deg, var(--accent-blue) 0%, var(--accent-cyan) 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.stat-label {
  font-size: 11px;
  color: var(--text-muted);
  margin-top: 2px;
}

/* 右侧主区 */
.profile-main {
  flex: 1;
}

.section-card {
  border-radius: 14px;
  background: var(--bg-card);
  border: 1px solid var(--border-glass);
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.04);
  overflow: hidden;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px 24px;
  border-bottom: 1px solid #f0f3f8;
}

.section-title-wrap {
  display: flex;
  align-items: center;
  gap: 10px;
}

.title-bar {
  width: 4px;
  height: 18px;
  border-radius: 2px;
  background: linear-gradient(180deg, var(--accent-blue), var(--accent-cyan));
}

.section-title {
  font-size: 16px;
  font-weight: 600;
  color: var(--text-primary);
}

.section-badge {
  display: flex;
  align-items: center;
  gap: 5px;
  padding: 4px 14px;
  border-radius: 20px;
  background: rgba(64, 158, 255, 0.08);
  color: var(--accent-blue);
  font-size: 12px;
}

/* 表单 */
.pwd-form {
  padding: 24px;
}

.profile-main :deep(.el-form-item__label) {
  color: var(--text-secondary);
}

.profile-main :deep(.el-input__wrapper) {
  background: #f8f9fc !important;
  box-shadow: 0 0 0 1px var(--border-glass) inset !important;
  border-radius: 10px !important;
}

.profile-main :deep(.el-input__wrapper:hover) {
  box-shadow: 0 0 0 1px rgba(64, 158, 255, 0.3) inset !important;
}

.profile-main :deep(.el-input__inner) {
  color: var(--text-primary);
}

.profile-main :deep(.el-button--primary) {
  background: linear-gradient(135deg, var(--accent-blue) 0%, var(--accent-cyan) 100%) !important;
  border: none !important;
  border-radius: 10px !important;
  padding: 10px 28px !important;
  box-shadow: 0 4px 16px rgba(64, 158, 255, 0.3);
  transition: all 0.3s;
}

.profile-main :deep(.el-button--primary:hover) {
  transform: translateY(-2px);
  box-shadow: 0 6px 24px rgba(64, 158, 255, 0.4);
}

@media (max-width: 768px) {
  .profile-container {
    flex-direction: column;
  }
  .profile-sidebar {
    width: 100%;
  }
}
</style>
