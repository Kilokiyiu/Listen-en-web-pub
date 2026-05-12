const { defineConfig } = require('@vue/cli-service')
module.exports = defineConfig({
  transpileDependencies: true,
  devServer: {
    proxy: {
      '/api/listen': {
        target: 'http://localhost:5047',
        changeOrigin: true,
        pathRewrite: { '^/api/listen': '' }
      },
      '/api/identity': {
        target: 'http://localhost:5263',
        changeOrigin: true
        // 不重写路径，保持 /api/identity 前缀
      },
      '/api/article': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        pathRewrite: { '^/api/article': '' }
      }
    }
  }
})
