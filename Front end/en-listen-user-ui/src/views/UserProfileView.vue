<template>
  <div class="profile-page le-page">
    <div class="page-header">
      <el-button text @click="$router.back()" class="back-btn">
        <el-icon><ArrowLeft /></el-icon> 返回
      </el-button>
    </div>

    <div class="title-section">
      <div class="title-icon">
        <el-icon :size="28" color="#409eff"><UserFilled /></el-icon>
      </div>
      <div class="title-text">
        <h1 class="page-title">个人中心</h1>
        <p class="page-subtitle">学习记录与账号管理</p>
      </div>
    </div>

    <div class="profile-container">
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

        <div class="stat-card" role="button" tabindex="0" @click="goHistory" @keydown.enter="goHistory">
          <div class="stat-grid">
            <div class="stat-item">
              <div class="stat-icon-wrap icon-blue">
                <el-icon :size="18" color="#409eff"><Headset /></el-icon>
              </div>
              <div class="stat-value">{{ stats.totalListen }}</div>
              <div class="stat-label">完成听力</div>
            </div>
            <div class="stat-item">
              <div class="stat-icon-wrap icon-green">
                <el-icon :size="18" color="#67c23a"><Reading /></el-icon>
              </div>
              <div class="stat-value">{{ stats.totalArticle }}</div>
              <div class="stat-label">完成阅读</div>
            </div>
            <div class="stat-item">
              <div class="stat-icon-wrap icon-orange">
                <el-icon :size="18" color="#e6a23c"><TrendCharts /></el-icon>
              </div>
              <div class="stat-value">{{ stats.streakDays }}</div>
              <div class="stat-label">连续(天)</div>
            </div>
          </div>
          <p class="stat-hint">点击查看完整学习记录 · 共 {{ stats.totalMinutes }} 分钟</p>
        </div>
      </div>

      <div class="profile-main">
        <div class="section-card">
          <div class="section-header">
            <div class="section-title-wrap">
              <div class="title-bar"></div>
              <span class="section-title">学习记录</span>
            </div>
            <el-button type="primary" link @click="goHistory">
              查看全部
              <el-icon><ArrowRight /></el-icon>
            </el-button>
          </div>

          <div v-if="recordsLoading" class="records-loading">
            <el-icon class="is-loading" :size="24"><Loading /></el-icon>
          </div>
          <el-empty
            v-else-if="recentRecords.length === 0"
            description="还没有学习记录，去听一套真题或读一篇短文吧"
            :image-size="72"
          >
            <el-button type="primary" @click="$router.push('/')">去首页练习</el-button>
            <el-button @click="$router.push({ name: 'dailyArticle' })">每日一篇</el-button>
          </el-empty>
          <div v-else class="record-list">
            <div
              v-for="item in recentRecords"
              :key="item.id"
              class="record-item"
              @click="openRecord(item)"
            >
              <div class="record-left">
                <div class="record-name-row">
                  <span class="record-name">{{ item.title }}</span>
                  <span class="record-tag" :class="tagClass(item)">{{ tagLabel(item) }}</span>
                </div>
                <div class="record-meta">
                  <span>{{ formatDateTime(item.updatedAt) }}</span>
                  <span>{{ formatDuration(item.durationSeconds) }}</span>
                </div>
              </div>
              <span class="done-badge">已完成</span>
            </div>
          </div>
        </div>

        <div class="section-card section-card--spaced">
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
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import {
  UserFilled, ArrowLeft, ArrowRight, Lock, Check, Calendar,
  Headset, Reading, TrendCharts, Loading,
} from '@element-plus/icons-vue'
import { getUserInfo } from '@/api/Auth'
import request from '@/api/Request'
import { getStudySummary, getStudyList } from '@/api/Study'

const router = useRouter()
const username = ref(localStorage.getItem('username') || '')
const userInfo = ref({})
const pwdLoading = ref(false)
const pwdFormRef = ref()
const recordsLoading = ref(false)
const recentRecords = ref([])

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

const stats = ref({
  totalListen: 0,
  totalArticle: 0,
  totalMinutes: 0,
  streakDays: 0
})

const unwrap = (res) => res?.data ?? res

const fetchUserInfo = async () => {
  try {
    const res = await getUserInfo()
    userInfo.value = res
  } catch (err) {
    userInfo.value = {
      userName: username.value,
      email: '',
      creationTime: new Date().toISOString()
    }
  }
}

const fetchStudyStats = async () => {
  try {
    const data = unwrap(await getStudySummary()) || {}
    stats.value = {
      totalListen: data.totalListen ?? 0,
      totalArticle: data.totalArticle ?? 0,
      totalMinutes: data.totalMinutes ?? 0,
      streakDays: data.streakDays ?? 0
    }
  } catch {
    /* keep zeros */
  }
}

const fetchRecentRecords = async () => {
  recordsLoading.value = true
  try {
    const data = unwrap(await getStudyList({ page: 1, pageSize: 5 })) || {}
    recentRecords.value = data.items || []
  } catch {
    recentRecords.value = []
  } finally {
    recordsLoading.value = false
  }
}

const goHistory = () => router.push({ name: 'history' })

const tagLabel = (item) => {
  if (item.category && item.category !== 'other' && item.category !== 'daily') return item.category
  return item.activityType === 'article' ? '阅读' : '听力'
}

const tagClass = (item) => {
  const cat = (item.category || '').toUpperCase()
  if (cat.includes('4')) return 'tag-cet4'
  if (cat.includes('6')) return 'tag-cet6'
  if (item.activityType === 'article') return 'tag-article'
  return 'tag-cet4'
}

const formatDateTime = (dateStr) => {
  if (!dateStr) return '-'
  const d = new Date(dateStr)
  if (Number.isNaN(d.getTime())) return dateStr
  const pad = (n) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`
}

const formatDuration = (seconds) => {
  const s = Math.max(0, Number(seconds) || 0)
  const minutes = Math.round(s / 60)
  if (minutes < 1) return s > 0 ? `${s}秒` : '-'
  const h = Math.floor(minutes / 60)
  const m = minutes % 60
  if (h > 0) return `${h}时${m}分`
  return `${m}分钟`
}

const openRecord = (item) => {
  if (item.activityType === 'article') {
    router.push({ name: 'dailyArticle' })
    return
  }
  router.push({ name: 'examDetail', query: { albumId: item.contentId } })
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

onMounted(() => {
  fetchUserInfo()
  fetchStudyStats()
  fetchRecentRecords()
})
</script>

<style scoped>
.profile-page {
  padding: 0;
}

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
  background: var(--le-bg-elevated);
  border: 1px solid rgba(34, 211, 238, 0.35);
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.35);
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

.stat-card {
  margin-top: 16px;
  border-radius: 14px;
  background: var(--bg-card);
  border: 1px solid var(--border-glass);
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.04);
  padding: 20px 16px 14px;
  cursor: pointer;
  transition: box-shadow 0.2s, border-color 0.2s;
}

.stat-card:hover {
  border-color: rgba(64, 158, 255, 0.35);
  box-shadow: 0 4px 16px rgba(64, 158, 255, 0.12);
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

.stat-hint {
  margin: 12px 0 0;
  text-align: center;
  font-size: 12px;
  color: var(--text-muted);
}

.profile-main {
  flex: 1;
  min-width: 0;
}

.section-card {
  border-radius: 14px;
  background: var(--bg-card);
  border: 1px solid var(--border-glass);
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.04);
  overflow: hidden;
}

.section-card--spaced {
  margin-top: 16px;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px 24px;
  border-bottom: 1px solid var(--le-border);
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

.records-loading {
  display: flex;
  justify-content: center;
  padding: 40px 0;
  color: var(--text-muted);
}

.record-list {
  padding: 4px 0;
}

.record-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 14px 24px;
  cursor: pointer;
  border-bottom: 1px solid var(--le-border);
  transition: background 0.2s;
}

.record-item:last-child {
  border-bottom: none;
}

.record-item:hover {
  background: rgba(64, 158, 255, 0.04);
}

.record-left {
  flex: 1;
  min-width: 0;
}

.record-name-row {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 6px;
}

.record-name {
  font-size: 14px;
  font-weight: 500;
  color: var(--text-primary);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.record-tag {
  flex-shrink: 0;
  padding: 1px 8px;
  border-radius: 10px;
  font-size: 11px;
  font-weight: 600;
}

.record-tag.tag-cet4 {
  background: rgba(64, 158, 255, 0.1);
  color: var(--accent-blue);
}

.record-tag.tag-cet6 {
  background: rgba(139, 92, 246, 0.1);
  color: #8b5cf6;
}

.record-tag.tag-article {
  background: rgba(103, 194, 58, 0.12);
  color: #67c23a;
}

.record-meta {
  display: flex;
  gap: 12px;
  font-size: 12px;
  color: var(--text-muted);
}

.done-badge {
  flex-shrink: 0;
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
  color: #67c23a;
  background: rgba(103, 194, 58, 0.1);
}

.pwd-form {
  padding: 24px;
}

.profile-main :deep(.el-form-item__label) {
  color: var(--text-secondary);
}

.profile-main :deep(.el-input__wrapper) {
  background: rgba(15, 23, 42, 0.65) !important;
  box-shadow: 0 0 0 1px var(--border-glass) inset !important;
  border-radius: 10px !important;
}

.profile-main :deep(.el-input__wrapper:hover) {
  box-shadow: 0 0 0 1px var(--le-border-strong) inset !important;
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
