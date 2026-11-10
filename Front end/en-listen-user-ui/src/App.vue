<template>
  <div id="app">
    <div
      class="le-route-progress"
      :class="{ active: routeLoading }"
      aria-hidden="true"
    />
    <AppHeader />
    <main class="le-app-main" :class="{ 'has-mobile-nav': showMobileNav }">
      <router-view v-slot="{ Component }">
        <transition name="le-route" mode="out-in">
          <keep-alive :include="keepAlivePages">
            <component :is="Component" />
          </keep-alive>
        </transition>
      </router-view>
    </main>
    <AppFooter />
    <AppMobileNav v-if="showMobileNav" />
    <DeskPetFloat v-if="showMobileNav && extrasReady" />
    <EncourageFeedbackDialog />
  </div>
</template>

<script setup>
import { computed, defineAsyncComponent, onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import AppHeader from './components/AppHeader.vue'
import AppFooter from './components/AppFooter.vue'
import AppMobileNav from './components/AppMobileNav.vue'
import EncourageFeedbackDialog from './components/EncourageFeedbackDialog.vue'
import { prefetchCommonRoutes, routeLoading } from './router'
const DeskPetFloat = defineAsyncComponent(() => import('./components/DeskPetFloat.vue'))

const route = useRoute()
const showMobileNav = computed(() => route.name !== 'login')
const extrasReady = ref(false)
onMounted(() => {
  const startExtras = () => { extrasReady.value = true }
  const startPrefetch = () => { prefetchCommonRoutes() }
  if (typeof window.requestIdleCallback === 'function') {
    window.requestIdleCallback(startExtras, { timeout: 4000 })
    window.requestIdleCallback(startPrefetch, { timeout: 6000 })
  } else {
    window.addEventListener('load', () => {
      startExtras()
      window.setTimeout(startPrefetch, 800)
    }, { once: true })
  }
})
/** 单词本等：返回时保留页面状态，避免整页重载 */
const keepAlivePages = ['MyWordsView']
</script>

<style>
.le-route-progress {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  height: 2px;
  z-index: 4000;
  pointer-events: none;
  background: transparent;
  opacity: 0;
  transform: scaleX(0);
  transform-origin: left center;
  transition: opacity 0.15s ease;
}
.le-route-progress.active {
  opacity: 1;
  background: linear-gradient(90deg, #3b82f6, #60a5fa, #93c5fd);
  animation: le-route-progress 0.9s ease-in-out infinite;
}
@keyframes le-route-progress {
  0% { transform: scaleX(0.12); }
  50% { transform: scaleX(0.72); }
  100% { transform: scaleX(0.28); }
}

.le-route-enter-active,
.le-route-leave-active {
  transition: opacity 0.18s ease, transform 0.18s ease;
}
.le-route-enter-from {
  opacity: 0;
  transform: translateY(8px);
}
.le-route-leave-to {
  opacity: 0;
  transform: translateY(-6px);
}

@media (prefers-reduced-motion: reduce) {
  .le-route-progress.active {
    animation: none;
    transform: scaleX(1);
  }
  .le-route-enter-active,
  .le-route-leave-active {
    transition: opacity 0.2s ease;
  }
  .le-route-enter-from,
  .le-route-leave-to {
    transform: none;
  }
}
</style>
