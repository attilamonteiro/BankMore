<script setup lang="ts">
import type { NotificationType } from '@/types'

defineProps<{
  type: NotificationType
  message: string
}>()
</script>

<template>
  <div
    :class="[
      'flex items-start gap-3 rounded-lg border p-4 text-sm',
      type === 'success' && 'border-green-300 bg-green-50 text-green-800',
      type === 'error' && 'border-red-300 bg-red-50 text-red-800',
      type === 'info' && 'border-blue-300 bg-blue-50 text-blue-800',
    ]"
  >
    <span class="mt-0.5 shrink-0 text-base leading-none">
      {{ type === 'success' ? '✓' : type === 'error' ? '✕' : 'ℹ' }}
    </span>
    <span>{{ message }}</span>
  </div>
</template>
