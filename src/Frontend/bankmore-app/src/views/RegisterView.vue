<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useNotification } from '@/composables/useNotification'
import BaseInput from '@/components/ui/BaseInput.vue'
import BaseButton from '@/components/ui/BaseButton.vue'

const router = useRouter()
const auth = useAuthStore()
const { success, error: notify } = useNotification()

const cpf = ref('')
const senha = ref('')
const confirmarSenha = ref('')
const loading = ref(false)
const numeroConta = ref('')

function maskCpf(e: Event) {
  const input = e.target as HTMLInputElement
  let v = input.value.replace(/\D/g, '').slice(0, 11)
  if (v.length > 9) v = `${v.slice(0, 3)}.${v.slice(3, 6)}.${v.slice(6, 9)}-${v.slice(9)}`
  else if (v.length > 6) v = `${v.slice(0, 3)}.${v.slice(3, 6)}.${v.slice(6)}`
  else if (v.length > 3) v = `${v.slice(0, 3)}.${v.slice(3)}`
  input.value = v
  cpf.value = v
}

async function submit() {
  if (!cpf.value || !senha.value) return
  if (senha.value !== confirmarSenha.value) {
    notify('As senhas não coincidem.')
    return
  }
  loading.value = true
  try {
    numeroConta.value = await auth.register(cpf.value, senha.value)
    success(`Conta criada! Número: ${numeroConta.value}`)
    setTimeout(() => router.push('/login'), 2000)
  } catch (err) {
    notify(auth.extractErrorMessage(err, 'Erro ao criar conta.'))
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
        <p class="mt-1 text-sm text-gray-500">Crie sua conta</p>
      </div>

      <div
        v-if="numeroConta"
        class="mb-6 rounded-xl border border-green-300 bg-green-50 p-4 text-center"
      >
        <p class="text-sm text-green-700">Conta criada com sucesso!</p>
        <p class="mt-1 text-xl font-bold text-green-800">{{ numeroConta }}</p>
        <p class="mt-1 text-xs text-green-600">Redirecionando para o login...</p>
      </div>

      <form v-else class="flex flex-col gap-5" @submit.prevent="submit">
        <div class="flex flex-col gap-1">
          <label class="text-sm font-medium text-gray-700">CPF</label>
          <input
            :value="cpf"
            type="text"
            inputmode="numeric"
            placeholder="000.000.000-00"
            maxlength="14"
            class="rounded-lg border border-gray-300 px-3 py-2.5 text-sm shadow-sm transition focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500"
            @input="maskCpf"
          />
        </div>
        <BaseInput
          v-model="senha"
          label="Senha"
          type="password"
          placeholder="••••••••"
        />
        <BaseInput
          v-model="confirmarSenha"
          label="Confirmar senha"
          type="password"
          placeholder="••••••••"
        />
        <BaseButton type="submit" :loading="loading" class="w-full mt-2">
          Criar conta
        </BaseButton>
      </form>

      <p class="mt-6 text-center text-sm text-gray-500">
        Já tem conta?
        <RouterLink to="/login" class="font-medium text-blue-600 hover:underline">
          Entrar
        </RouterLink>
      </p>
    </div>
  </div>
</template>
