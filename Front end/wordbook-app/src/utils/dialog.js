import { reactive } from 'vue'

export const dialogState = reactive({
  visible: false,
  title: '',
  message: '',
  confirmText: '确定',
  cancelText: '取消',
  showCancel: true,
  input: false,
  value: '',
  danger: false,
  resolve: null,
})

function open(options) {
  return new Promise((resolve) => {
    Object.assign(dialogState, {
      visible: true,
      title: options.title || '',
      message: options.message || '',
      confirmText: options.confirmText || '确定',
      cancelText: options.cancelText || '取消',
      showCancel: options.showCancel !== false,
      input: !!options.input,
      value: options.value || '',
      danger: !!options.danger,
      resolve,
    })
  })
}

export function closeDialog(result) {
  const resolve = dialogState.resolve
  dialogState.visible = false
  dialogState.resolve = null
  resolve?.(result)
}

export function confirmDialog(message, options = {}) {
  return open({ message, ...options })
}

export function promptDialog(message, options = {}) {
  return open({ message, input: true, ...options })
}
