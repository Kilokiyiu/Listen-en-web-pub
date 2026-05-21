<template>
  <div id="app">
    <AppHeader />
    <div class="main-content">
      <router-view />
    </div>
    <AppFooter />
    <!-- 全局浮动搜索按钮 -->
    <WordSearchFloat />
  </div>
</template>

<script setup>
import AppHeader from './components/AppHeader.vue'
import AppFooter from './components/AppFooter.vue'
import WordSearchFloat from './components/WordSearchFloat.vue'
</script>

<style>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

:root {
  --bg-primary: #0a0e27;
  --bg-secondary: #111936;
  --bg-content: #faf8f5;
  --bg-card: #ffffff;
  --border-glass: rgba(0, 0, 0, 0.08);
  --text-primary: #1a1a2e;
  --text-secondary: #4a4a6a;
  --text-muted: #8a8aaa;
  --accent-cyan: #00a8e8;
  --accent-blue: #409eff;
  --accent-purple: #8b5cf6;
  --shadow-glow: 0 8px 32px rgba(0, 0, 0, 0.08);
}

body {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
  background: linear-gradient(135deg, #0f0c29 0%, #302b63 50%, #24243e 100%);
  background-attachment: fixed;
  min-height: 100vh;
  color: var(--text-primary);
}

/* 滚动条 */
::-webkit-scrollbar {
  width: 6px;
  height: 6px;
}

::-webkit-scrollbar-track {
  background: rgba(0, 0, 0, 0.03);
}

::-webkit-scrollbar-thumb {
  background: rgba(0, 0, 0, 0.15);
  border-radius: 3px;
}

::-webkit-scrollbar-thumb:hover {
  background: rgba(0, 0, 0, 0.25);
}

#app {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}

.main-content {
  flex: 1;
  max-width: 1400px;
  width: 100%;
  margin: 0 auto;
  padding: 0 0 32px;
  background: var(--bg-content);
  border-radius: 0;
}

@media (max-width: 768px) {
  .main-content {
    border-radius: 0;
  }
}
</style>
