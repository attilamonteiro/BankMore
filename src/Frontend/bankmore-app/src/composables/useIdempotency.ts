import { v4 as uuidv4 } from 'uuid'

export function useIdempotency() {
  function newKey(): string {
    return uuidv4()
  }

  return { newKey }
}
