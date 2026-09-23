<template>
  <div class="admin-page">
    <PageHeader title="用户管理" description="查看用户信息、管理角色组别、封禁账号与重置密码" />

    <div class="admin-card admin-table-card">
      <div class="admin-card__header">
        <div class="toolbar">
          <el-input
            v-model="keyword"
            placeholder="搜索用户名 / 邮箱"
            clearable
            style="width: 220px"
            @keyup.enter="loadUsers(1)"
          />
          <el-select v-model="roleFilter" placeholder="角色筛选" clearable style="width: 140px" @change="loadUsers(1)">
            <el-option label="全部角色" value="" />
            <el-option label="普通用户" value="User" />
            <el-option label="管理员" value="Admin" />
          </el-select>
          <el-button type="primary" :loading="loading" @click="loadUsers(1)">查询</el-button>
        </div>
        <el-button type="primary" @click="openCreate">
          <el-icon><Plus /></el-icon>
          新建用户
        </el-button>
      </div>

      <div class="admin-card__body table-body">
        <el-table v-loading="loading" :data="users" stripe style="width: 100%">
          <el-table-column prop="userName" label="用户名" min-width="120" show-overflow-tooltip />
          <el-table-column prop="email" label="邮箱" min-width="180" show-overflow-tooltip>
            <template #default="{ row }">{{ row.email || '—' }}</template>
          </el-table-column>
          <el-table-column label="角色" width="160">
            <template #default="{ row }">
              <el-tag
                v-for="role in row.roles"
                :key="role"
                size="small"
                effect="light"
                :type="role === 'Admin' ? 'danger' : 'info'"
                style="margin-right: 4px"
              >
                {{ role === 'Admin' ? '管理员' : '用户' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="状态" width="100" align="center">
            <template #default="{ row }">
              <el-tag :type="row.isLocked ? 'danger' : 'success'" size="small" effect="light">
                {{ row.isLocked ? '已封禁' : '正常' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="注册时间" width="170">
            <template #default="{ row }">{{ formatTime(row.creationTime) }}</template>
          </el-table-column>
          <el-table-column label="操作" width="320" fixed="right">
            <template #default="{ row }">
              <el-button link type="primary" @click="openRoles(row)">组别</el-button>
              <el-button link type="primary" @click="openSetPassword(row)">改密</el-button>
              <el-button link type="warning" @click="handleResetPassword(row)">重置密码</el-button>
              <el-button link :type="row.isLocked ? 'success' : 'danger'" @click="handleToggleLock(row)">
                {{ row.isLocked ? '解封' : '封禁' }}
              </el-button>
              <el-button link type="danger" @click="handleDelete(row)">删除</el-button>
            </template>
          </el-table-column>
        </el-table>

        <div class="pager">
          <el-pagination
            background
            layout="total, prev, pager, next"
            :total="total"
            :page-size="pageSize"
            :current-page="page"
            @current-change="loadUsers"
          />
        </div>
      </div>
    </div>

    <!-- 新建用户 -->
    <el-dialog v-model="createVisible" title="新建用户" width="480px" destroy-on-close>
      <el-form ref="createFormRef" :model="createForm" :rules="createRules" label-width="80px">
        <el-form-item label="用户名" prop="userName">
          <el-input v-model="createForm.userName" maxlength="50" />
        </el-form-item>
        <el-form-item label="邮箱" prop="email">
          <el-input v-model="createForm.email" placeholder="选填" />
        </el-form-item>
        <el-form-item label="密码" prop="password">
          <el-input v-model="createForm.password" type="password" show-password />
        </el-form-item>
        <el-form-item label="角色" prop="roles">
          <el-checkbox-group v-model="createForm.roles">
            <el-checkbox label="User">普通用户</el-checkbox>
            <el-checkbox label="Admin">管理员</el-checkbox>
          </el-checkbox-group>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="createVisible = false">取消</el-button>
        <el-button type="primary" :loading="saving" @click="submitCreate">创建</el-button>
      </template>
    </el-dialog>

    <!-- 角色管理 -->
    <el-dialog v-model="rolesVisible" title="组别管理" width="420px" destroy-on-close>
      <p class="dialog-tip">用户：{{ currentUser?.userName }}</p>
      <el-checkbox-group v-model="editRoles">
        <el-checkbox label="User">普通用户 (User)</el-checkbox>
        <el-checkbox label="Admin">管理员 (Admin)</el-checkbox>
      </el-checkbox-group>
      <template #footer>
        <el-button @click="rolesVisible = false">取消</el-button>
        <el-button type="primary" :loading="saving" @click="submitRoles">保存</el-button>
      </template>
    </el-dialog>

    <!-- 设置密码 -->
    <el-dialog v-model="pwdVisible" title="修改密码" width="420px" destroy-on-close>
      <p class="dialog-tip">
        用户：{{ currentUser?.userName }}
        <el-tag v-if="currentUser?.roles?.includes('Admin')" type="danger" size="small" style="margin-left: 8px">管理员</el-tag>
      </p>
      <el-form ref="pwdFormRef" :model="pwdForm" :rules="pwdRules" label-width="90px">
        <el-form-item label="新密码" prop="newPassword">
          <el-input v-model="pwdForm.newPassword" type="password" show-password placeholder="至少 6 位" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="pwdVisible = false">取消</el-button>
        <el-button type="primary" :loading="saving" @click="submitPassword">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus } from '@element-plus/icons-vue'
import PageHeader from '../components/PageHeader.vue'
import {
  getUsers,
  createUser,
  setUserRoles,
  setUserLock,
  resetUserPassword,
  setUserPassword,
  deleteUser,
} from '../api/Admin'

const loading = ref(false)
const saving = ref(false)
const users = ref([])
const total = ref(0)
const page = ref(1)
const pageSize = 20
const keyword = ref('')
const roleFilter = ref('')

const createVisible = ref(false)
const rolesVisible = ref(false)
const pwdVisible = ref(false)
const currentUser = ref(null)
const editRoles = ref([])
const createFormRef = ref(null)
const pwdFormRef = ref(null)

const createForm = reactive({
  userName: '',
  email: '',
  password: '',
  roles: ['User'],
})

const pwdForm = reactive({ newPassword: '' })

const createRules = {
  userName: [{ required: true, message: '请输入用户名', trigger: 'blur' }],
  password: [
    { required: true, message: '请输入密码', trigger: 'blur' },
    { min: 6, message: '密码至少 6 位', trigger: 'blur' },
  ],
  roles: [{ type: 'array', min: 1, message: '至少选择一个角色', trigger: 'change' }],
}

const pwdRules = {
  newPassword: [
    { required: true, message: '请输入新密码', trigger: 'blur' },
    { min: 6, message: '密码至少 6 位', trigger: 'blur' },
  ],
}

const formatTime = (value) => {
  if (!value) return '—'
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return value
  const pad = (n) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`
}

const loadUsers = async (p = page.value) => {
  page.value = p
  loading.value = true
  try {
    const res = await getUsers({
      keyword: keyword.value || undefined,
      role: roleFilter.value || undefined,
      page: page.value,
      pageSize,
    })
    users.value = res?.data?.items || []
    total.value = res?.data?.total || 0
  } finally {
    loading.value = false
  }
}

const openCreate = () => {
  createForm.userName = ''
  createForm.email = ''
  createForm.password = ''
  createForm.roles = ['User']
  createVisible.value = true
}

const submitCreate = async () => {
  const valid = await createFormRef.value?.validate().catch(() => false)
  if (!valid) return
  saving.value = true
  try {
    await createUser({
      userName: createForm.userName.trim(),
      email: createForm.email.trim() || undefined,
      password: createForm.password,
      roles: createForm.roles,
    })
    ElMessage.success('用户已创建')
    createVisible.value = false
    await loadUsers(1)
  } finally {
    saving.value = false
  }
}

const openRoles = (row) => {
  currentUser.value = row
  editRoles.value = [...(row.roles || [])]
  if (editRoles.value.length === 0) editRoles.value = ['User']
  rolesVisible.value = true
}

const submitRoles = async () => {
  if (!currentUser.value) return
  if (editRoles.value.length === 0) {
    ElMessage.warning('至少选择一个角色')
    return
  }
  saving.value = true
  try {
    await setUserRoles({ userId: currentUser.value.id, roles: editRoles.value })
    ElMessage.success('角色已更新')
    rolesVisible.value = false
    await loadUsers()
  } finally {
    saving.value = false
  }
}

const openSetPassword = (row) => {
  currentUser.value = row
  pwdForm.newPassword = ''
  pwdVisible.value = true
}

const submitPassword = async () => {
  const valid = await pwdFormRef.value?.validate().catch(() => false)
  if (!valid || !currentUser.value) return
  saving.value = true
  try {
    await setUserPassword({ userId: currentUser.value.id, newPassword: pwdForm.newPassword })
    ElMessage.success('密码已更新')
    pwdVisible.value = false
  } finally {
    saving.value = false
  }
}

const handleResetPassword = async (row) => {
  try {
    await ElMessageBox.confirm(`确定重置用户「${row.userName}」的密码吗？将生成随机新密码。`, '重置密码', {
      type: 'warning',
    })
  } catch {
    return
  }
  const res = await resetUserPassword(row.id)
  const password = res?.data?.password
  if (password) {
    await ElMessageBox.alert(`新密码：${password}\n请妥善保存并告知用户。`, '重置成功', { confirmButtonText: '已复制/已知晓' })
  } else {
    ElMessage.success('密码已重置')
  }
}

const handleToggleLock = async (row) => {
  const locked = !row.isLocked
  const tip = locked ? `确定封禁用户「${row.userName}」吗？` : `确定解除「${row.userName}」的封禁吗？`
  try {
    await ElMessageBox.confirm(tip, locked ? '封禁用户' : '解除封禁', { type: 'warning' })
  } catch {
    return
  }
  await setUserLock({ userId: row.id, locked, days: locked ? null : undefined })
  ElMessage.success(locked ? '已封禁' : '已解封')
  await loadUsers()
}

const handleDelete = async (row) => {
  try {
    await ElMessageBox.confirm(`确定删除用户「${row.userName}」吗？此操作为软删除。`, '删除用户', {
      type: 'warning',
      confirmButtonText: '删除',
    })
  } catch {
    return
  }
  await deleteUser(row.id)
  ElMessage.success('用户已删除')
  await loadUsers()
}

onMounted(() => loadUsers(1))
</script>

<style scoped>
.toolbar {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  align-items: center;
}

.admin-card__header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
}

.table-body {
  padding-top: 0;
}

.pager {
  display: flex;
  justify-content: flex-end;
  margin-top: 16px;
}

.dialog-tip {
  margin: 0 0 16px;
  color: var(--admin-text-secondary, #666);
  font-size: 14px;
}
</style>
