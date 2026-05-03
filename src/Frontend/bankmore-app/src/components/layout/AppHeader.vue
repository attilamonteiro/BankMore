<script setup lang="ts">
import { useAuthStore } from '@/stores/auth'
import { useRouter } from 'vue-router'

const auth = useAuthStore()
const router = useRouter()

function logout() {
  auth.logout()
  router.push('/login')
}
</script>

<template>
  <header class="flex items-center justify-between border-b border-gray-200 bg-white px-6 py-4 shadow-sm">
    <div class="flex items-center gap-3">
      <span class="text-xl font-bold text-blue-700">BankMore</span>
      <span v-if="auth.numeroConta" class="hidden text-sm text-gray-500 sm:block">
        Conta {{ auth.numeroConta }}
      </span>
    </div>
    <button
      class="rounded-lg px-3 py-1.5 text-sm font-medium text-gray-600 hover:bg-gray-100 hover:text-gray-900 transition"
      @click="logout"
    >
      Sair
    </button>
  </header>
</template>
