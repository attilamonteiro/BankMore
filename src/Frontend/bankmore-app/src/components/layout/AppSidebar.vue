<script setup lang="ts">
import { RouterLink } from 'vue-router'

const links = [
  { to: '/dashboard', label: 'Dashboard', icon: '🏠' },
  { to: '/movimento', label: 'Depósito / Saque', icon: '💰' },
  { to: '/transferencia', label: 'Transferência', icon: '↔️' },
  { to: '/conta', label: 'Minha Conta', icon: '👤' },
]
</script>

<template>
  <nav class="w-56 shrink-0 border-r border-gray-200 bg-white px-3 py-6">
    <ul class="flex flex-col gap-1">
      <li v-for="link in links" :key="link.to">
        <RouterLink
          :to="link.to"
          class="flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium text-gray-600 transition hover:bg-blue-50 hover:text-blue-700"
          active-class="bg-blue-50 text-blue-700"
        >
          <span>{{ link.icon }}</span>
          {{ link.label }}
        </RouterLink>
      </li>
    </ul>
  </nav>
</template>
