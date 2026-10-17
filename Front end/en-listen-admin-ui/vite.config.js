import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import AutoImport from 'unplugin-auto-import/vite'
import Components from 'unplugin-vue-components/vite'
import { ElementPlusResolver } from 'unplugin-vue-components/resolvers'

export default defineConfig({
  base: './',
  plugins: [
    vue(),
    AutoImport({
      resolvers: [ElementPlusResolver({ importStyle: 'css' })],
      dts: false,
    }),
    Components({
      resolvers: [ElementPlusResolver({ importStyle: 'css' })],
      dts: false,
    }),
  ],
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
      },
      '/api/kaoyan': {
        target: 'http://localhost:5301',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/api\/kaoyan/, ''),
        timeout: 10000,
        proxyTimeout: 10000
      }
    }
  }
})
