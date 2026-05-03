import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { login as apiLogin, cadastrar as apiCadastrar } from '@/services/contaCorrenteApi'
import type { ErrorResponse } from '@/types'
import { AxiosError } from 'axios'

const TOKEN_KEY = 'bankmore_token'
const CONTA_KEY = 'bankmore_numeroConta'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem(TOKEN_KEY))
  const numeroConta = ref<string | null>(localStorage.getItem(CONTA_KEY))

  const isAuthenticated = computed(() => !!token.value)

  async function register(cpf: string, senha: string): Promise<string> {
    const { data } = await apiCadastrar({ cpf, senha })
    numeroConta.value = data.numeroConta
    localStorage.setItem(CONTA_KEY, data.numeroConta)
    return data.numeroConta
  }

  async function login(contaOuCpf: string, senha: string): Promise<void> {
    const { data } = await apiLogin({ numeroContaOuCpf: contaOuCpf, senha })
    token.value = data.token
    numeroConta.value = data.numeroConta
    localStorage.setItem(TOKEN_KEY, data.token)
    localStorage.setItem(CONTA_KEY, data.numeroConta)
  }

  function logout() {
    token.value = null
    numeroConta.value = null
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(CONTA_KEY)
  }

  function extractErrorMessage(err: unknown, fallback: string): string {
    if (err instanceof AxiosError && err.response?.data) {
      const data = err.response.data as ErrorResponse
      return data.mensagem ?? fallback
    }
    return fallback
  }

  return { token, numeroConta, isAuthenticated, register, login, logout, extractErrorMessage }
})
