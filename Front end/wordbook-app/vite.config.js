import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  server: {
    proxy: {
      // 本地无 Word/Identity 服务时，开发期代理到线上便于预览
      '/api/identity': {
        target: 'https://your-domain.com',
        changeOrigin: true,
        secure: true,
      },
      '/api/word': {
        target: 'https://your-domain.com',
        changeOrigin: true,
        secure: true,
      },
    },
  },
})
