<template>
  <footer class="app-footer">
    <div class="footer-inner">
      <div class="footer-top">
        <div class="footer-brand">
          <div class="footer-logo">
            <div class="footer-logo-icon">听</div>
            <span class="footer-name">ListenEase</span>
          </div>
          <p class="footer-desc">专业的英语听力练习平台，开源共建，持续更新。</p>
        </div>
        <div class="footer-links">
          <router-link to="/about" class="footer-link">关于我们</router-link>
          <router-link to="/feedback" class="footer-link">意见反馈</router-link>
          <a href="https://github.com/Kilokiyiu/Listen-en-web-pub" target="_blank" rel="noopener noreferrer" class="footer-link">GitHub 源码</a>
          <a href="mailto:" class="footer-link">联系邮箱</a>
        </div>
      </div>
      <BlogCta class="footer-blog" />
      <div class="footer-bottom">
        <p>&copy; {{ currentYear }} ListenEase · Vue 3 + .NET · SQL Server</p>
      </div>
    </div>
  </footer>
</template>

<script setup>
import { computed } from 'vue'
import BlogCta from './BlogCta.vue'

const currentYear = computed(() => new Date().getFullYear())
</script>

<style scoped>
.app-footer {
  margin-top: auto;
  background: rgba(247, 249, 252, 0.78);
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
  border-top: 1px solid var(--le-border);
  padding-bottom: env(safe-area-inset-bottom, 0);
  color: var(--le-text-secondary);
}

.footer-inner {
  max-width: var(--le-max-w);
  margin: 0 auto;
  padding: 48px 20px 32px;
}

.footer-top {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 24px;
  flex-wrap: wrap;
  margin-bottom: 20px;
}

.footer-logo {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 12px;
}

.footer-logo-icon {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  background: var(--le-gradient);
  display: flex;
  align-items: center;
  justify-content: center;
  color: #fff;
}

.footer-name {
  font-family: var(--le-font-display);
  font-size: 18px;
  font-weight: 700;
  color: var(--le-text);
}

.footer-desc {
  margin: 0;
  max-width: 360px;
  font-size: 13px;
  line-height: 1.6;
  color: var(--le-text-secondary);
}

.footer-links {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
  align-items: flex-start;
}

.footer-link {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 8px 16px;
  border: 1px solid var(--le-border);
  border-radius: 999px;
  color: var(--le-text-secondary);
  font-size: 13px;
  text-decoration: none;
  transition: all 0.2s ease;
}

.footer-link:hover {
  background: var(--le-bg-muted);
  border-color: var(--le-border-strong);
  color: var(--le-primary);
}

.footer-blog {
  display: flex;
  margin: 0 0 24px;
}

.footer-bottom {
  padding-top: 24px;
  border-top: 1px solid var(--le-border);
  text-align: center;
}

.footer-bottom p {
  margin: 0;
  font-family: var(--le-font-mono);
  font-size: 12px;
  color: var(--le-text-muted);
}

@media (max-width: 768px) {
  .footer-inner {
    padding: 36px 12px 80px;
  }

  .footer-top {
    flex-direction: column;
  }
}
</style>
