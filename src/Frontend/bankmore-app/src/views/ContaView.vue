<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useContaStore } from '@/stores/conta'
import { useAuthStore } from '@/stores/auth'
import { useNotification } from '@/composables/useNotification'
import BaseInput from '@/components/ui/BaseInput.vue'
import BaseButton from '@/components/ui/BaseButton.vue'
import LoadingSpinner from '@/components/ui/LoadingSpinner.vue'

const conta = useContaStore()
const auth = useAuthStore()
const router = useRouter()
const { success, error: notify } = useNotification()

const showConfirm = ref(false)
const senha = ref('')

onMounted(() => {
  if (!conta.saldo) conta.fetchSaldo()
})

async function inativar() {
  if (!senha.value) {
    notify('Informe sua senha para confirmar.')
    return
  }
  try {
    await conta.inativarConta(senha.value)
    success('Conta inativada com sucesso.')
    auth.logout()
    router.push('/login')
  } catch {
    notify(conta.error ?? 'Erro ao inativar conta.')
  }
}
</script>

<template>
  <div class="mx-auto max-w-md space-y-6">
    <h2 class="text-2xl font-bold text-gray-900">Minha Conta</h2>

    <LoadingSpinner v-if="conta.loading && !conta.saldo" />

    <div v-else class="rounded-2xl bg-white p-6 shadow-sm border border-gray-200 space-y-4">
      <div class="space-y-3">
        <div>
          <p class="text-xs font-medium text-gray-500 uppercase tracking-wide">Titular</p>
          <p class="text-base font-semibold text-gray-900">
            {{ conta.saldo?.nomeTitular ?? '—' }}
          </p>
        </div>
        <div>
          <p class="text-xs font-medium text-gray-500 uppercase tracking-wide">Número da conta</p>
          <p class="text-base font-semibold text-gray-900">{{ auth.numeroConta }}</p>
        </div>
        <div>
          <p class="text-xs font-medium text-gray-500 uppercase tracking-wide">Status</p>
          <span class="inline-flex items-center gap-1.5 rounded-full bg-green-100 px-2.5 py-0.5 text-xs font-medium text-green-800">
            <span class="h-1.5 w-1.5 rounded-full bg-green-500" />
            Ativa
          </span>
        </div>
      </div>

      <hr class="border-gray-100" />

      <!-- Inativar section -->
      <div>
        <p class="text-sm font-medium text-gray-700 mb-3">Inativar conta</p>
        <p class="text-xs text-gray-500 mb-4">
          Esta ação é irreversível. A conta será desativada permanentemente.
        </p>

        <BaseButton
          v-if="!showConfirm"
          variant="danger"
          @click="showConfirm = true"
        >
          Inativar minha conta
        </BaseButton>

        <div v-else class="space-y-3 rounded-xl border border-red-200 bg-red-50 p-4">
          <p class="text-sm font-semibold text-red-700">Confirmação necessária</p>
          <BaseInput
            v-model="senha"
            label="Digite sua senha"
            type="password"
            placeholder="••••••••"
          />
          <div class="flex gap-3">
            <BaseButton variant="secondary" @click="showConfirm = false; senha = ''">
              Cancelar
            </BaseButton>
            <BaseButton variant="danger" :loading="conta.loading" @click="inativar">
              Confirmar inativação
            </BaseButton>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
