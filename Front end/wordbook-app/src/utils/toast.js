import { ref } from 'vue'

const message = ref('')
const visible = ref(false)
let timer = null

export function useToast() {
  const show = (msg, duration = 2000) => {
    message.value = msg
    visible.value = true
    clearTimeout(timer)
    timer = setTimeout(() => {
      visible.value = false
    }, duration)
  }
  return { message, visible, show }
}

const toast = useToast()
export const showToast = toast.show
