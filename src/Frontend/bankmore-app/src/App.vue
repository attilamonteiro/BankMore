<script setup lang="ts">
import { RouterView } from 'vue-router'
import { useNotification } from '@/composables/useNotification'
import BaseAlert from '@/components/ui/BaseAlert.vue'

const { notifications, remove } = useNotification()
</script>

<template>
  <RouterView />

  <!-- Toast notifications -->
  <Teleport to="body">
    <div class="fixed bottom-6 right-6 z-50 flex flex-col gap-3 w-80">
      <TransitionGroup name="toast">
        <div
          v-for="n in notifications"
          :key="n.id"
          class="cursor-pointer"
          @click="remove(n.id)"
        >
          <BaseAlert :type="n.type" :message="n.message" />
        </div>
      </TransitionGroup>
    </div>
  </Teleport>
</template>

<style>
.toast-enter-active,
.toast-leave-active {
  transition: all 0.3s ease;
}
.toast-enter-from {
  opacity: 0;
  transform: translateX(100%);
}
.toast-leave-to {
  opacity: 0;
  transform: translateX(100%);
}
</style>
