import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  server: {
    proxy: {
      '/api/identity': {
        target: 'http://localhost:5263',
        changeOrigin: true,
      },
      '/api/word': {
        target: 'http://localhost:5215',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/api\/word/, ''),
      },
    },
  },
})
