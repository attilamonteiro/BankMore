import { defineStore } from 'pinia'
import { ref } from 'vue'
import { transferir as apiTransferir } from '@/services/transferenciaApi'
import { useIdempotency } from '@/composables/useIdempotency'
import type { ErrorResponse } from '@/types'
import { AxiosError } from 'axios'

export const useTransferenciaStore = defineStore('transferencia', () => {
  const { newKey } = useIdempotency()

  const loading = ref(false)
  const error = ref<string | null>(null)
  const success = ref(false)

  async function transferir(
    numeroContaOrigem: string,
    numeroContaDestino: string,
    valor: number,
  ) {
    loading.value = true
    error.value = null
    success.value = false
    try {
      await apiTransferir({
        chaveIdempotencia: newKey(),
        numeroContaOrigem,
        numeroContaDestino,
        valor,
      })
      success.value = true
    } catch (err) {
      error.value = extractMessage(err, 'Erro ao realizar transferência.')
      throw err
    } finally {
      loading.value = false
    }
  }

  function reset() {
    loading.value = false
    error.value = null
    success.value = false
  }

  function extractMessage(err: unknown, fallback: string): string {
    if (err instanceof AxiosError && err.response?.data) {
      const data = err.response.data as ErrorResponse
      return data.mensagem ?? fallback
    }
    return fallback
  }

  return { loading, error, success, transferir, reset }
})
