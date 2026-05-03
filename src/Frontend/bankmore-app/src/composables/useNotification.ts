import { ref } from 'vue'
import { v4 as uuidv4 } from 'uuid'
import type { Notification, NotificationType } from '@/types'

const notifications = ref<Notification[]>([])

export function useNotification() {
  function add(type: NotificationType, message: string, durationMs = 4000) {
    const id = uuidv4()
    notifications.value.push({ id, type, message })
    setTimeout(() => remove(id), durationMs)
  }

  function remove(id: string) {
    notifications.value = notifications.value.filter((n) => n.id !== id)
  }

  const success = (msg: string) => add('success', msg)
  const error = (msg: string) => add('error', msg)
  const info = (msg: string) => add('info', msg)

  return { notifications, success, error, info, remove }
}
