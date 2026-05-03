import axios from 'axios'

const TOKEN_KEY = 'bankmore_token'

function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY)
}

function createInstance(baseURL: string) {
  const instance = axios.create({ baseURL })

  instance.interceptors.request.use((config) => {
    const token = getToken()
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  })

  instance.interceptors.response.use(
    (response) => response,
    (error) => {
      if (error.response?.status === 401) {
        localStorage.removeItem(TOKEN_KEY)
        localStorage.removeItem('bankmore_numeroConta')
        window.location.href = '/login'
      }
      return Promise.reject(error)
    },
  )

  return instance
}

export const contaCorrenteApi = createInstance(
  import.meta.env.VITE_CONTA_CORRENTE_BASE_URL ?? '/api/contacorrente',
)

export const transferenciaApi = createInstance(
  import.meta.env.VITE_TRANSFERENCIA_BASE_URL ?? '/api/transferencia',
)
