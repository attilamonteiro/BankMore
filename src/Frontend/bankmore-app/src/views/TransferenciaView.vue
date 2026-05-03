<script setup lang="ts">
import { ref } from 'vue'
import { useTransferenciaStore } from '@/stores/transferencia'
import { useAuthStore } from '@/stores/auth'
import { useNotification } from '@/composables/useNotification'
import BaseInput from '@/components/ui/BaseInput.vue'
import BaseButton from '@/components/ui/BaseButton.vue'

const transferencia = useTransferenciaStore()
const auth = useAuthStore()
const { success, error: notify } = useNotification()

const contaDestino = ref('')
const valor = ref('')

function fmt(value: number) {
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value)
}

async function submit() {
  if (!contaDestino.value) {
    notify('Informe a conta de destino.')
    return
  }
  const v = parseFloat(valor.value.replace(',', '.'))
  if (!v || v <= 0) {
    notify('Informe um valor válido maior que zero.')
    return
  }
  if (!auth.numeroConta) {
    notify('Conta de origem não encontrada. Faça login novamente.')
    return
  }
  try {
    await transferencia.transferir(auth.numeroConta, contaDestino.value, v)
    success(`Transferência de ${fmt(v)} realizada com sucesso!`)
    contaDestino.value = ''
    valor.value = ''
  } catch {
    notify(transferencia.error ?? 'Erro ao realizar transferência.')
  }
}
</script>

<template>
  <div class="mx-auto max-w-md space-y-6">
    <h2 class="text-2xl font-bold text-gray-900">Transferência</h2>

    <div class="rounded-2xl bg-white p-6 shadow-sm border border-gray-200 space-y-5">
      <BaseInput
        :model-value="auth.numeroConta ?? ''"
        label="Conta de origem"
        disabled
        @update:model-value="() => {}"
      />
      <BaseInput
        v-model="contaDestino"
        label="Conta de destino"
        placeholder="Número da conta"
      />
      <BaseInput
        v-model="valor"
        label="Valor (R$)"
        type="number"
        placeholder="0,00"
      />

      <BaseButton
        type="button"
        :loading="transferencia.loading"
        class="w-full"
        @click="submit"
      >
        Transferir
      </BaseButton>
    </div>
  </div>
</template>
