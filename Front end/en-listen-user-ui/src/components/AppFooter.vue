<template>
  <footer class="app-footer">
    <div class="footer-inner">
      <div class="footer-top">
        <div class="footer-brand">
          <div class="footer-logo">
            <el-icon :size="20"><Headset /></el-icon>
            <span>ListenEase</span>
          </div>
          <p>专业的英语听力练习平台，开源共建，持续更新。</p>
        </div>
        <div class="footer-links">
          <a href="https://github.com/Kilokiyiu/Listen-en-web-pub" target="_blank" rel="noopener noreferrer">GitHub 源码</a>
          <a href="mailto:Kilokiyiu@outlook.com">联系邮箱</a>
          <a href="https://github.com/Kilokiyiu" target="_blank" rel="noopener noreferrer">作者主页</a>
        </div>
      </div>
      <div class="footer-bottom">
        <p>&copy; {{ currentYear }} ListenEase · Vue 3 + .NET · SQL Server</p>
      </div>
    </div>
  </footer>
</template>

<script setup>
import { computed } from 'vue'
import { Headset } from '@element-plus/icons-vue'
const currentYear = computed(() => new Date().getFullYear())
</script>

<style scoped>
.app-footer {
  background: var(--le-bg-dark);
  color: rgba(255,255,255,0.75);
  margin-top: auto;
  padding-bottom: env(safe-area-inset-bottom, 0);
}

.footer-inner {
  max-width: var(--le-max-w);
  margin: 0 auto;
  padding: 32px 20px 20px;
}

.footer-top {
  display: flex;
  justify-content: space-between;
  gap: 24px;
  flex-wrap: wrap;
  margin-bottom: 24px;
}

.footer-logo {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 700;
  color: #fff;
  margin-bottom: 8px;
}

.footer-brand p {
  font-size: 13px;
  line-height: 1.6;
  margin: 0;
  max-width: 360px;
  color: rgba(255,255,255,0.55);
}

.footer-links {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.footer-links a {
  color: rgba(255,255,255,0.65);
  text-decoration: none;
  font-size: 13px;
  transition: color 0.2s;
}

.footer-links a:hover {
  color: #93c5fd;
}

.footer-bottom {
  padding-top: 16px;
  border-top: 1px solid rgba(255,255,255,0.1);
  text-align: center;
}

.footer-bottom p {
  margin: 0;
  font-size: 12px;
  color: rgba(255,255,255,0.4);
}

@media (max-width: 768px) {
  .footer-inner {
    padding: 24px 12px 80px;
  }
  .footer-top {
    flex-direction: column;
  }
}
</style>
