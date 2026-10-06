<template>
  <div id="app">
    <AppHeader />
    <main class="le-app-main" :class="{ 'has-mobile-nav': showMobileNav }">
      <router-view v-slot="{ Component }">
        <transition name="le-route" mode="out-in">
          <component :is="Component" />
        </transition>
      </router-view>
    </main>
    <AppFooter />
    <AppMobileNav v-if="showMobileNav" />
    <WordSearchFloat v-if="showMobileNav" />
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import AppHeader from './components/AppHeader.vue'
import AppFooter from './components/AppFooter.vue'
import AppMobileNav from './components/AppMobileNav.vue'
import WordSearchFloat from './components/WordSearchFloat.vue'

const route = useRoute()
const showMobileNav = computed(() => route.name !== 'login')
</script>

<style>
.le-route-enter-active {
  transition: transform 0.55s cubic-bezier(0.22, 0.9, 0.3, 1), opacity 0.4s ease;
  transform-origin: left center;
  backface-visibility: hidden;
}
.le-route-leave-active {
  transition: transform 0.4s cubic-bezier(0.55, 0.05, 0.8, 0.4), opacity 0.32s ease;
  transform-origin: left center;
  backface-visibility: hidden;
}
.le-route-enter-from {
  opacity: 0;
  transform: rotateY(-68deg) translateX(-18px) scale(0.98);
}
.le-route-leave-to {
  opacity: 0;
  transform: rotateY(48deg) translateX(10px) scale(0.98);
}

@media (prefers-reduced-motion: reduce) {
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
