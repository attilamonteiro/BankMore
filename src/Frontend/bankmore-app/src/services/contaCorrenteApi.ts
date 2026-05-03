import { contaCorrenteApi } from './api'
import type {
  CadastrarRequest,
  CadastrarResponse,
  LoginRequest,
  LoginResponse,
  InativarRequest,
  MovimentoRequest,
  MovimentoResponse,
  SaldoResponse,
} from '@/types'

export function cadastrar(data: CadastrarRequest) {
  return contaCorrenteApi.post<CadastrarResponse>('', data)
}

export function login(data: LoginRequest) {
  return contaCorrenteApi.post<LoginResponse>('/login', data)
}

export function inativar(data: InativarRequest) {
  return contaCorrenteApi.patch<void>('/inativar', data)
}

export function criarMovimento(data: MovimentoRequest) {
  return contaCorrenteApi.post<MovimentoResponse>('/movimento', data)
}

export function consultarSaldo() {
  return contaCorrenteApi.get<SaldoResponse>('/saldo')
}
