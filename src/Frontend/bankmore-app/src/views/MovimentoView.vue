<script setup lang="ts">
import { ref } from 'vue'
import { useContaStore } from '@/stores/conta'
import { useNotification } from '@/composables/useNotification'
import BaseInput from '@/components/ui/BaseInput.vue'
import BaseButton from '@/components/ui/BaseButton.vue'

const conta = useContaStore()
const { success, error: notify } = useNotification()

const tipo = ref<'C' | 'D'>('C')
const valor = ref('')

function fmt(value: number) {
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value)
}

async function submit() {
  const v = parseFloat(valor.value.replace(',', '.'))
  if (!v || v <= 0) {
    notify('Informe um valor válido maior que zero.')
    return
  }
  try {
    if (tipo.value === 'C') {
      await conta.depositar(v)
      success(`Depósito de ${fmt(v)} realizado com sucesso!`)
    } else {
      await conta.sacar(v)
      success(`Saque de ${fmt(v)} realizado com sucesso!`)
    }
    valor.value = ''
  } catch {
    notify(conta.error ?? 'Erro ao processar operação.')
  }
}
</script>

<template>
  <div class="mx-auto max-w-md space-y-6">
    <h2 class="text-2xl font-bold text-gray-900">Depósito / Saque</h2>

    <!-- Saldo -->
    <div v-if="conta.saldo" class="rounded-xl bg-blue-50 p-4 text-center">
      <p class="text-xs text-blue-600">Saldo atual</p>
      <p class="text-2xl font-bold text-blue-800">{{ fmt(conta.saldo.saldo) }}</p>
    </div>

    <div class="rounded-2xl bg-white p-6 shadow-sm border border-gray-200 space-y-5">
      <!-- Tipo toggle -->
      <div class="flex rounded-lg overflow-hidden border border-gray-300">
        <button
          :class="[
            'flex-1 py-2.5 text-sm font-semibold transition',
            tipo === 'C' ? 'bg-green-600 text-white' : 'bg-white text-gray-600 hover:bg-gray-50',
          ]"
          @click="tipo = 'C'"
        >
          Depósito
        </button>
        <button
          :class="[
            'flex-1 py-2.5 text-sm font-semibold transition',
            tipo === 'D' ? 'bg-red-600 text-white' : 'bg-white text-gray-600 hover:bg-gray-50',
          ]"
          @click="tipo = 'D'"
        >
          Saque
        </button>
      </div>

      <BaseInput
        v-model="valor"
        label="Valor (R$)"
        type="number"
        placeholder="0,00"
      />

      <BaseButton
        type="button"
        :loading="conta.loading"
        :variant="tipo === 'C' ? 'primary' : 'danger'"
        class="w-full"
        @click="submit"
      >
        {{ tipo === 'C' ? 'Confirmar depósito' : 'Confirmar saque' }}
      </BaseButton>
    </div>
  </div>
</template>
