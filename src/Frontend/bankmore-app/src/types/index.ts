// ─── Auth / ContaCorrente ────────────────────────────────────────────────────

export interface CadastrarRequest {
  cpf: string
  senha: string
}

export interface CadastrarResponse {
  numeroConta: string
}

export interface LoginRequest {
  numeroContaOuCpf: string
  senha: string
}

export interface LoginResponse {
  token: string
  numeroConta: string
}

export interface InativarRequest {
  senha: string
}

// ─── Movimento ───────────────────────────────────────────────────────────────

export type TipoMovimento = 'C' | 'D'

export interface MovimentoRequest {
  chaveIdempotencia: string
  tipoMovimento: TipoMovimento
  valor: number
}

export interface MovimentoResponse {
  idMovimento: string
}

// ─── Saldo ───────────────────────────────────────────────────────────────────

export interface SaldoResponse {
  numeroConta: string
  nomeTitular: string
  dataHoraConsulta: string
  saldo: number
}

// ─── Transferência ───────────────────────────────────────────────────────────

export interface TransferenciaRequest {
  chaveIdempotencia: string
  numeroContaOrigem: string
  numeroContaDestino: string
  valor: number
}

export interface TransferenciaResponse {
  idTransferencia: string
}

// ─── Erros ───────────────────────────────────────────────────────────────────

export interface ErrorResponse {
  mensagem: string
  tipoErro: string
}

// ─── Notificação ─────────────────────────────────────────────────────────────

export type NotificationType = 'success' | 'error' | 'info'

export interface Notification {
  id: string
  type: NotificationType
  message: string
}
