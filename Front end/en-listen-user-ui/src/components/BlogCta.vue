<template>
  <a
    class="blog-cta"
    :class="{ compact }"
    href="https://www.kilokiyiu.com"
    target="_blank"
    rel="noopener noreferrer"
  >
    <span class="blog-cta-rail" aria-hidden="true" />
    <span class="blog-cta-body">
      <span class="blog-cta-label">作者博客</span>
      <span class="blog-cta-title">想要了解更多？</span>
      <span class="blog-cta-desc">进入我的博客获取更多学习资料吧！</span>
    </span>
    <span class="blog-cta-go" aria-hidden="true">
      <span class="blog-cta-go-text">去看看</span>
      <svg viewBox="0 0 20 20" width="16" height="16" fill="none">
        <path
          d="M7.5 4.5L13 10l-5.5 5.5"
          stroke="currentColor"
          stroke-width="1.8"
          stroke-linecap="round"
          stroke-linejoin="round"
        />
      </svg>
    </span>
  </a>
</template>

<script setup>
defineProps({
  /** 弹窗等窄布局用紧凑样式 */
  compact: { type: Boolean, default: false },
})
</script>

<style scoped>
.blog-cta {
  position: relative;
  display: flex;
  align-items: center;
  gap: 14px;
  margin: 0;
  padding: 14px 14px 14px 0;
  overflow: hidden;
  border: 1px solid var(--le-border);
  border-radius: 14px;
  background:
    linear-gradient(135deg, rgba(255, 255, 255, 0.92) 0%, rgba(247, 249, 252, 0.88) 100%),
    var(--le-gradient-soft);
  box-shadow: var(--le-shadow-sm);
  text-decoration: none;
  transition:
    border-color 0.18s ease,
    box-shadow 0.18s ease,
    transform 0.18s ease;
}

.blog-cta::after {
  content: '';
  position: absolute;
  inset: 0;
  background: var(--le-paper-grain);
  opacity: 0.55;
  pointer-events: none;
}

.blog-cta:hover {
  border-color: rgba(37, 99, 235, 0.28);
  box-shadow: var(--le-shadow);
  transform: translateY(-1px);
}

.blog-cta-rail {
  flex-shrink: 0;
  align-self: stretch;
  width: 4px;
  margin: 10px 0 10px 10px;
  border-radius: 999px;
  background: linear-gradient(180deg, var(--le-primary) 0%, var(--le-accent) 100%);
}

.blog-cta-body {
  position: relative;
  z-index: 1;
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.blog-cta-label {
  font-family: var(--le-font-mono);
  font-size: 10px;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--le-primary);
  line-height: 1.3;
}

.blog-cta-title {
  font-family: var(--le-font-display);
  font-size: 15px;
  font-weight: 700;
  color: var(--le-text);
  line-height: 1.35;
}

.blog-cta-desc {
  font-size: 13px;
  line-height: 1.5;
  color: var(--le-text-secondary);
}

.blog-cta-go {
  position: relative;
  z-index: 1;
  flex-shrink: 0;
  display: inline-flex;
  align-items: center;
  gap: 2px;
  padding: 8px 12px;
  border-radius: 999px;
  background: var(--le-gradient);
  color: #fff;
  font-size: 12px;
  font-weight: 600;
  line-height: 1;
  box-shadow: 0 4px 12px rgba(37, 99, 235, 0.22);
  transition: transform 0.18s ease;
}

.blog-cta:hover .blog-cta-go {
  transform: translateX(2px);
}

.blog-cta.compact {
  gap: 10px;
  padding: 12px 12px 12px 0;
  border-radius: 12px;
}

.blog-cta.compact .blog-cta-title {
  font-size: 14px;
}

.blog-cta.compact .blog-cta-desc {
  font-size: 12px;
}

.blog-cta.compact .blog-cta-go {
  padding: 7px 10px;
}

@media (max-width: 520px) {
  .blog-cta {
    align-items: flex-start;
  }

  .blog-cta-go {
    margin-top: 2px;
  }

  .blog-cta-go-text {
    display: none;
  }

  .blog-cta-go {
    padding: 8px;
  }
}
</style>
