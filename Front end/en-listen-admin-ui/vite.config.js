import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  server: {
    port: 5173,
    proxy: {
      '/api/listen': {
        target: 'http://localhost:5047',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/api\/listen/, ''),
        timeout: 180000,
        proxyTimeout: 180000
      },
      '/api/identity': {
        target: 'http://localhost:5263',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/api\/identity/, '')
      },
      '/api/article': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/api\/article/, ''),
        timeout: 30000,
        proxyTimeout: 30000
      }
    }
  }
})
