<script setup lang="ts">
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useContaStore } from '@/stores/conta'
import { useAuthStore } from '@/stores/auth'
import LoadingSpinner from '@/components/ui/LoadingSpinner.vue'

const conta = useContaStore()
const auth = useAuthStore()
const router = useRouter()

onMounted(() => conta.fetchSaldo())

function fmt(value: number) {
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value)
}

const actions = [
  { label: 'Depósito / Saque', desc: 'Creditar ou debitar valores', to: '/movimento', icon: '💰' },
  { label: 'Transferência', desc: 'Transferir para outra conta', to: '/transferencia', icon: '↔️' },
  { label: 'Minha Conta', desc: 'Dados e configurações', to: '/conta', icon: '👤' },
]
</script>

<template>
  <div class="mx-auto max-w-2xl space-y-6">
    <h2 class="text-2xl font-bold text-gray-900">Dashboard</h2>

    <!-- Saldo card -->
    <div class="rounded-2xl bg-gradient-to-br from-blue-600 to-blue-800 p-6 text-white shadow-lg">
      <p class="text-sm font-medium opacity-80">Saldo disponível</p>
      <LoadingSpinner v-if="conta.loading" />
      <p v-else class="mt-2 text-4xl font-bold tracking-tight">
        {{ conta.saldo ? fmt(conta.saldo.saldo) : 'R$ —' }}
      </p>
      <div v-if="conta.saldo" class="mt-4 flex items-center justify-between text-xs opacity-70">
        <span>{{ conta.saldo.nomeTitular }}</span>
        <span>Conta {{ auth.numeroConta }}</span>
      </div>
    </div>

    <!-- Action cards -->
    <div class="grid grid-cols-1 gap-4 sm:grid-cols-3">
      <button
        v-for="action in actions"
        :key="action.to"
        class="flex flex-col gap-2 rounded-xl border border-gray-200 bg-white p-5 text-left shadow-sm transition hover:border-blue-400 hover:shadow-md"
        @click="router.push(action.to)"
      >
        <span class="text-2xl">{{ action.icon }}</span>
        <span class="font-semibold text-gray-800">{{ action.label }}</span>
        <span class="text-xs text-gray-500">{{ action.desc }}</span>
      </button>
    </div>

    <!-- Last update -->
    <p v-if="conta.saldo" class="text-right text-xs text-gray-400">
      Atualizado em
      {{ new Date(conta.saldo.dataHoraConsulta).toLocaleString('pt-BR') }}
    </p>

    <p v-if="conta.error" class="text-sm text-red-600">{{ conta.error }}</p>
  </div>
</template>
