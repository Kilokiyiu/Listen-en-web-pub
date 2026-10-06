<template>
  <PageShell
    title="关于 ListenEase"
    subtitle="认识这个学习站，以及它为什么存在"
    :back="true"
  >
    <div class="about-page">
      <!-- Intro -->
      <section class="about-sheet le-paper le-paper--ruled le-paper--margin le-paper-flip">
        <p class="sheet-label">ABOUT</p>
        <h2 class="sheet-title">这是什么？</h2>
        <p class="sheet-body">
          ListenEase是一个面向英语学习者的听力练习平台。你可以在这里做四六级、雅思、托福等真题听力(雅思、托福仍在整理开发中)，
          阅读每日短文与 BBC 外刊，通过词根与单词本系统积累词汇，并把学习进度记在账号里。
        </p>
        <p class="sheet-body">
          站点坚持开源共建、持续更新，目标很简单：让备考与日常听力练习更顺手，少一点分散注意力的干扰。
        </p>
        <ul class="feature-list">
          <li>历年真题听力在线练习，支持阅读原文与试卷 / 答案 PDF</li>
          <li>每日短文、BBC 新闻，保持语感</li>
          <li>词根学习与单词本复习</li>
          <li>学习记录与账号同步（含 Android 单词本APP EaseWord）</li>
        </ul>
      </section>

      <!-- Why -->
      <section class="about-sheet le-paper le-paper-flip le-paper-flip-delay-1">
        <p class="sheet-label">WHY</p>
        <h2 class="sheet-title">为什么做这个网站？</h2>
        <p class="sheet-body">
          备考听力时，资料往往散落在网盘、App 和网页各处：想听一套真题，要先找音频、再找原文、再找答案。
          很多工具又偏商业化，广告与付费墙打断练习节奏。
        </p>
        <p class="sheet-body">
          ListenEase 想把「听、看、记、复习」收拢到同一处：打开就能练，练完能回看，单词能带走。
          它首先是学习工具，其次才是一个网站——所以界面会尽量把注意力留给内容本身。
        </p>
        <p class="sheet-body">
          如果这个网站能帮你多听完一套题、多记住几个词，它就达到了最初的目的。
        </p>
      </section>

      <!-- Author -->
      <section class="about-sheet author-sheet le-paper le-paper-flip le-paper-flip-delay-2">
        <p class="sheet-label">AUTHOR</p>
        <h2 class="sheet-title">作者与维护</h2>
        <div class="author-card">
          <div class="author-avatar" aria-hidden="true">K</div>
          <div class="author-info">
            <h3 class="author-name">Kilo</h3>
            <p class="author-role">站点作者 · 开发与维护</p>
            <p class="author-bio">
              ListenEase 由 Kilo（Kilokiyiu）独立开发与维护，欢迎通过邮箱或 GitHub 反馈问题、建议，
              或一起改进开源项目。
            </p>
            <div class="author-links">
              <a
                class="author-link"
                href="https://github.com/Kilokiyiu"
                target="_blank"
                rel="noopener noreferrer"
              >
                <el-icon><Link /></el-icon>
                GitHub · Kilokiyiu
              </a>
              <a
                class="author-link"
                href="https://mywebpage-f2u.pages.dev/"
                target="_blank"
                rel="noopener noreferrer"
              >
                <el-icon><User /></el-icon>
                作者主页
              </a>
              <a
                class="author-link"
                href="https://github.com/Kilokiyiu/Listen-en-web-pub"
                target="_blank"
                rel="noopener noreferrer"
              >
                <el-icon><Link /></el-icon>
                项目源码
              </a>
            </div>
          </div>
        </div>
        <p class="sheet-note">
          练习与阅读材料来自公开备考资源与精选内容整理，仅供个人学习使用。若你发现版权或内容问题，请通过邮箱联系，我们会尽快处理。
        </p>
      </section>

      <!-- CTA -->
      <div class="about-actions le-paper-flip le-paper-flip-delay-3">
        <router-link class="action-btn primary" to="/">回到首页开始练习</router-link>
        <router-link class="action-btn" to="/feedback">意见反馈</router-link>
      </div>
    </div>
  </PageShell>
</template>

<script setup>
import { Message, Link, User } from '@element-plus/icons-vue'
import PageShell from '@/components/PageShell.vue'
</script>

<style scoped>
.about-page {
  display: flex;
  flex-direction: column;
  gap: 20px;
  padding-bottom: 12px;
}

.about-sheet {
  padding: 28px 28px 28px 52px;
}

.about-sheet:not(.le-paper--margin) {
  padding-left: 28px;
}

.sheet-label {
  margin: 0 0 8px;
  font-family: var(--le-font-mono);
  font-size: 11px;
  letter-spacing: 0.08em;
  color: var(--le-primary);
}

.sheet-title {
  margin: 0 0 14px;
  font-family: var(--le-font-display);
  font-size: 1.35rem;
  font-weight: 700;
  color: var(--le-text);
}

.sheet-body {
  margin: 0 0 12px;
  font-size: 15px;
  line-height: 1.75;
  color: var(--le-text-secondary);
}

.sheet-body:last-child {
  margin-bottom: 0;
}

.feature-list {
  margin: 16px 0 0;
  padding: 0 0 0 1.15em;
  color: var(--le-text-secondary);
  font-size: 14px;
  line-height: 1.8;
}

.feature-list li {
  margin-bottom: 4px;
}

.author-card {
  display: flex;
  gap: 18px;
  align-items: flex-start;
  margin-top: 4px;
}

.author-avatar {
  flex-shrink: 0;
  width: 56px;
  height: 56px;
  border-radius: 14px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-family: var(--le-font-display);
  font-size: 22px;
  font-weight: 700;
  color: #fff;
  background: var(--le-gradient);
  box-shadow: var(--le-shadow-sm);
}

.author-info {
  min-width: 0;
  flex: 1;
}

.author-name {
  margin: 0 0 2px;
  font-family: var(--le-font-display);
  font-size: 1.2rem;
  font-weight: 700;
  color: var(--le-text);
}

.author-role {
  margin: 0 0 10px;
  font-size: 13px;
  color: var(--le-text-muted);
}

.author-bio {
  margin: 0 0 14px;
  font-size: 14px;
  line-height: 1.7;
  color: var(--le-text-secondary);
}

.author-links {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.author-link {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 7px 12px;
  border: 1px solid var(--le-border);
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.7);
  color: var(--le-text-secondary);
  font-size: 13px;
  text-decoration: none;
  transition: border-color 0.15s ease, color 0.15s ease, background 0.15s ease;
}

.author-link:hover {
  border-color: var(--le-border-strong);
  color: var(--le-primary);
  background: #fff;
}

.sheet-note {
  margin: 18px 0 0;
  padding-top: 14px;
  border-top: 1px dashed var(--le-border);
  font-size: 12px;
  line-height: 1.65;
  color: var(--le-text-muted);
}

.about-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  margin-top: 4px;
}

.action-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: 10px 18px;
  border-radius: 8px;
  border: 1px solid var(--le-border);
  background: rgba(255, 255, 255, 0.85);
  color: var(--le-text-secondary);
  font-size: 14px;
  font-weight: 600;
  text-decoration: none;
  transition: background 0.15s ease, color 0.15s ease, border-color 0.15s ease;
}

.action-btn:hover {
  border-color: var(--le-border-strong);
  color: var(--le-primary);
}

.action-btn.primary {
  background: var(--le-primary);
  border-color: transparent;
  color: #fff;
}

.action-btn.primary:hover {
  background: var(--le-primary-light);
  color: #fff;
}

@media (max-width: 768px) {
  .about-sheet,
  .about-sheet:not(.le-paper--margin) {
    padding: 20px 16px 20px 44px;
  }

  .about-sheet:not(.le-paper--margin) {
    padding-left: 16px;
  }

  .author-card {
    flex-direction: column;
    gap: 12px;
  }

  .about-actions {
    flex-direction: column;
  }

  .action-btn {
    width: 100%;
  }
}
</style>
