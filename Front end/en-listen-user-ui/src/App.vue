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
.le-route-enter-active,
.le-route-leave-active {
  transition: opacity 0.2s ease, transform 0.2s ease;
}
.le-route-enter-from {
  opacity: 0;
  transform: translateY(6px);
}
.le-route-leave-to {
  opacity: 0;
  transform: translateY(-4px);
}
</style>
