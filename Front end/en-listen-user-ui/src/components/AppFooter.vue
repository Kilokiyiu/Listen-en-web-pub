<template>
  <footer class="app-footer">
    <div class="footer-container">
      <div class="footer-main">
        <!-- 左侧：网站信息 -->
        <div class="footer-brand">
          <div class="footer-logo">
            <el-icon :size="22" color="#409eff"><Headset /></el-icon>
            <span>ListenEase</span>
          </div>
          <p class="footer-desc">
            专业的英语听力在线练习平台，帮助你提升英语听力能力<br>
            本项目完全开源，如果您感兴趣，可以在<a href="https://github.com/Kilokiyiu/Listen-en-web-pub" target="_blank" rel="noopener noreferrer">GitHub</a>上查看源代码，欢迎star和fork
          </p>
        </div>

        <!-- 中间：开源相关 -->
        <div class="footer-links">
          <div class="footer-section">
            <h4>开源项目</h4>
            <a href="https://github.com/Kilokiyiu/Listen-en-web-pub" target="_blank" rel="noopener noreferrer">
              <el-icon><Link /></el-icon>
              源代码
            </a>
            <a href="https://github.com/Kilokiyiu/Listen-en-web-pub" target="_blank" rel="noopener noreferrer">
              <el-icon><Link /></el-icon>
              部署脚本(完善中)
            </a>
          </div>

          <div class="footer-section">
            <h4>其他联系方式</h4>
            <a href="mailto:Kilokiyiu@outlook.com">
              <el-icon><Message /></el-icon>
              Kilokiyiu@outlook.com
            </a>
            <a href="https://github.com/Kilokiyiu" target="_blank" rel="noopener noreferrer">
              <el-icon><User /></el-icon>
              GitHub 主页
            </a>
              <a href="https://wx.mail.qq.com/" target="_blank" rel="noopener noreferrer">
              <el-icon><User /></el-icon>
              QQ：2752223406@qq.com
            </a>
          </div>
        </div>
      </div>

      <!-- 底部：版权 -->
      <div class="footer-bottom">
        <div class="footer-divider"></div>
        <div class="footer-copyright">
          <p>&copy; {{ currentYear }} ListenEase. 基于 Vue.js + .NET 10 构建</p>
          <p class="footer-tech">采用DDD架构，前后端分离，数据存储于 SQL Server</p>
        </div>
      </div>
    </div>
  </footer>
</template>

<script setup>
import { computed } from 'vue'
import { Headset, Link, Message, User } from '@element-plus/icons-vue'

const currentYear = computed(() => new Date().getFullYear())
</script>

<style scoped>
.app-footer {
  background: #1a1a2e;
  color: rgba(255, 255, 255, 0.85);
  margin-top: 32px;
}

.footer-container {
  max-width: 1400px;
  margin: 0 auto;
  padding: 40px 24px 24px;
}

.footer-main {
  display: flex;
  justify-content: space-between;
  gap: 48px;
  flex-wrap: wrap;
}

/* 品牌信息 */
.footer-brand {
  flex: 1;
  min-width: 260px;
  max-width: 400px;
}

.footer-logo {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 18px;
  font-weight: 600;
  color: #ffffff;
  margin-bottom: 12px;
}

.footer-logo :deep(.el-icon) {
  filter: drop-shadow(0 0 6px rgba(64, 158, 255, 0.5));
}

.footer-desc {
  font-size: 14px;
  color: rgba(255, 255, 255, 0.55);
  line-height: 1.7;
}

/* 链接区域 */
.footer-links {
  display: flex;
  gap: 48px;
  flex-wrap: wrap;
}

.footer-section {
  min-width: 160px;
}

.footer-section h4 {
  font-size: 14px;
  font-weight: 600;
  color: #ffffff;
  margin-bottom: 14px;
  position: relative;
  padding-left: 10px;
}

.footer-section h4::before {
  content: '';
  position: absolute;
  left: 0;
  top: 50%;
  transform: translateY(-50%);
  width: 3px;
  height: 14px;
  background: var(--accent-blue);
  border-radius: 0;
}

.footer-section a {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  color: rgba(255, 255, 255, 0.65);
  text-decoration: none;
  margin-bottom: 10px;
  transition: all 0.2s;
}

.footer-section a:hover {
  color: #409eff;
  transform: translateX(3px);
}

.footer-section a .el-icon {
  font-size: 12px;
  opacity: 0.7;
}

/* 底部版权 */
.footer-bottom {
  margin-top: 32px;
}

.footer-divider {
  height: 1px;
  background: rgba(255, 255, 255, 0.1);
  margin-bottom: 20px;
}

.footer-copyright {
  text-align: center;
}

.footer-copyright p {
  font-size: 13px;
  color: rgba(255, 255, 255, 0.45);
  margin: 0;
}

.footer-copyright p:first-child {
  margin-bottom: 4px;
}

.footer-tech {
  font-size: 12px !important;
  opacity: 0.7;
}

/* 响应式 */
@media (max-width: 768px) {
  .footer-container {
    padding: 28px 16px 20px;
  }

  .footer-main {
    flex-direction: column;
    gap: 28px;
  }

  .footer-brand {
    max-width: none;
  }

  .footer-links {
    gap: 32px;
  }

  .footer-section {
    min-width: 140px;
  }
}
</style>
