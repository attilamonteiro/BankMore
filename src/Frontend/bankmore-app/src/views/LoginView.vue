<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useNotification } from '@/composables/useNotification'
import BaseInput from '@/components/ui/BaseInput.vue'
import BaseButton from '@/components/ui/BaseButton.vue'

const router = useRouter()
const auth = useAuthStore()
const { error: notify } = useNotification()

const contaOuCpf = ref('')
const senha = ref('')
const loading = ref(false)

async function submit() {
  if (!contaOuCpf.value || !senha.value) return
  loading.value = true
  try {
    await auth.login(contaOuCpf.value, senha.value)
    router.push('/dashboard')
  } catch (err) {
    notify(auth.extractErrorMessage(err, 'Credenciais inválidas.'))
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="flex min-h-screen items-center justify-center bg-gradient-to-br from-blue-50 to-blue-100 p-4">
    <div class="w-full max-w-md rounded-2xl bg-white p-8 shadow-lg">
      <div class="mb-8 text-center">
        <h1 class="text-3xl font-bold text-blue-700">BankMore</h1>
        <p class="mt-1 text-sm text-gray-500">Faça login na sua conta</p>
      </div>

      <form class="flex flex-col gap-5" @submit.prevent="submit">
        <BaseInput
          v-model="contaOuCpf"
          label="Número da conta ou CPF"
          placeholder="000.000.000-00 ou número da conta"
        />
        <BaseInput
          v-model="senha"
          label="Senha"
          type="password"
          placeholder="••••••••"
        />
        <BaseButton type="submit" :loading="loading" class="w-full mt-2">
          Entrar
        </BaseButton>
      </form>

      <p class="mt-6 text-center text-sm text-gray-500">
        Não tem conta?
        <RouterLink to="/cadastro" class="font-medium text-blue-600 hover:underline">
          Cadastre-se
        </RouterLink>
      </p>
    </div>
  </div>
</template>
