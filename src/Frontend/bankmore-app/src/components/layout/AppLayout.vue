<script setup lang="ts">
import AppHeader from './AppHeader.vue'
import AppSidebar from './AppSidebar.vue'
</script>

<template>
  <div class="flex min-h-screen flex-col bg-gray-50">
    <AppHeader />
    <div class="flex flex-1">
      <AppSidebar />
      <main class="flex-1 overflow-auto p-6">
        <RouterView />
      </main>
    </div>
  </div>
</template>
