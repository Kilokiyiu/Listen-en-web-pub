const { defineConfig } = require('@vue/cli-service')
module.exports = defineConfig({
  transpileDependencies: true,
  chainWebpack: config => {
    config.plugin('html').tap(args => {
      args[0].title = 'ListenEase - 专业英语听力练习平台 | 四六级雅思托福真题'
      args[0].minify = {
        removeComments: false,
        collapseWhitespace: false,
        removeAttributeQuotes: false
      }
      return args
    })
  },
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
      },
      '/api/word': {
        target: 'http://localhost:5215',
        changeOrigin: true
        // 不重写路径，保持 /api/word 前缀
      }
    }
  }
})
