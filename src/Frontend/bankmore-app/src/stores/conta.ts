import { defineStore } from 'pinia'
import { ref } from 'vue'
import {
  consultarSaldo,
  criarMovimento,
  inativar as apiInativar,
} from '@/services/contaCorrenteApi'
import { useIdempotency } from '@/composables/useIdempotency'
import type { SaldoResponse, ErrorResponse } from '@/types'
import { AxiosError } from 'axios'

export const useContaStore = defineStore('conta', () => {
  const { newKey } = useIdempotency()

  const saldo = ref<SaldoResponse | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchSaldo() {
    loading.value = true
    error.value = null
    try {
      const { data } = await consultarSaldo()
      saldo.value = data
    } catch (err) {
      error.value = extractMessage(err, 'Erro ao consultar saldo.')
    } finally {
      loading.value = false
    }
  }

  async function depositar(valor: number) {
    loading.value = true
    error.value = null
    try {
      await criarMovimento({ chaveIdempotencia: newKey(), tipoMovimento: 'C', valor })
      await fetchSaldo()
    } catch (err) {
      error.value = extractMessage(err, 'Erro ao realizar depósito.')
      throw err
    } finally {
      loading.value = false
    }
  }

  async function sacar(valor: number) {
    loading.value = true
    error.value = null
    try {
      await criarMovimento({ chaveIdempotencia: newKey(), tipoMovimento: 'D', valor })
      await fetchSaldo()
    } catch (err) {
      error.value = extractMessage(err, 'Erro ao realizar saque.')
      throw err
    } finally {
      loading.value = false
    }
  }

  async function inativarConta(senha: string) {
    loading.value = true
    error.value = null
    try {
      await apiInativar({ senha })
    } catch (err) {
      error.value = extractMessage(err, 'Erro ao inativar conta.')
      throw err
    } finally {
      loading.value = false
    }
  }

  function extractMessage(err: unknown, fallback: string): string {
    if (err instanceof AxiosError && err.response?.data) {
      const data = err.response.data as ErrorResponse
      return data.mensagem ?? fallback
    }
    return fallback
  }

  return { saldo, loading, error, fetchSaldo, depositar, sacar, inativarConta }
})
