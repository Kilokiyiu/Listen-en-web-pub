<template>
  <div class="admin-page-header">
    <div>
      <h1 class="admin-page-header__title">{{ title }}</h1>
      <p v-if="description" class="admin-page-header__desc">{{ description }}</p>
    </div>
    <div v-if="$slots.extra" class="admin-page-header__extra">
      <slot name="extra" />
    </div>
  </div>
</template>

<script setup>
defineProps({
  title: { type: String, required: true },
  description: { type: String, default: '' },
})
</script>
