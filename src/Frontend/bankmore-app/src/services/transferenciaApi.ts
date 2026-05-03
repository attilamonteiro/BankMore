import { transferenciaApi } from './api'
import type { TransferenciaRequest, TransferenciaResponse } from '@/types'

export function transferir(data: TransferenciaRequest) {
  return transferenciaApi.post<TransferenciaResponse>('', data)
}
