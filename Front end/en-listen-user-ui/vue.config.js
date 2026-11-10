const { defineConfig } = require('@vue/cli-service')
const AutoImport = require('unplugin-auto-import/webpack')
const Components = require('unplugin-vue-components/webpack')
const { ElementPlusResolver } = require('unplugin-vue-components/resolvers')

const isProd = process.env.NODE_ENV === 'production'

module.exports = defineConfig({
  transpileDependencies: true,
  productionSourceMap: false,
  configureWebpack: {
    plugins: [
      AutoImport({
        resolvers: [ElementPlusResolver({ importStyle: 'css', resolveIcons: true })],
        dts: false,
      }),
      Components({
        resolvers: [ElementPlusResolver({ importStyle: 'css', resolveIcons: true })],
        dts: false,
      }),
    ],
  },
  css: {
    // 生产环境抽取并压缩 CSS（css-minimizer-webpack-plugin）
    extract: isProd,
  },
  chainWebpack: config => {
    config.plugin('html').tap(args => {
      args[0].title = 'ListenEase - 四六级备考网站_英语四级真题汇总与六级听力真题'
      if (isProd) {
        args[0].minify = {
          removeComments: true,
          collapseWhitespace: true,
          removeAttributeQuotes: true,
          collapseBooleanAttributes: true,
          removeScriptTypeAttributes: true,
          removeStyleLinkTypeAttributes: true,
          minifyCSS: true,
          minifyJS: true,
        }
      }
      return args
    })

    // JS：生产环境启用 Terser 深度压缩（摇树由 webpack optimization 负责）
    if (isProd) {
      config.optimization.minimizer('terser').tap(args => {
        const pluginOpts = args[0] || {}
        const terserOptions = pluginOpts.terserOptions || {}

        terserOptions.compress = {
          ...(terserOptions.compress || {}),
          drop_console: true,
          drop_debugger: true,
          pure_funcs: ['console.log'],
          passes: 2,
        }
        terserOptions.mangle = {
          ...(terserOptions.mangle || {}),
          toplevel: true,
        }
        terserOptions.format = {
          ...(terserOptions.format || {}),
          comments: false,
        }

        pluginOpts.terserOptions = terserOptions
        pluginOpts.extractComments = false
        args[0] = pluginOpts
        return args
      })
    }
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
        changeOrigin: true,
        pathRewrite: { '^/api/word': '' }
      },
      '/api/kaoyan': {
        target: 'http://localhost:5301',
        changeOrigin: true,
        pathRewrite: { '^/api/kaoyan': '' }
      }
    }
  }
})
