const path = require('path')

module.exports = {
  // 开发服务器配置
  devServer: {
    port: 8080,
    open: false,
    hot: true,
    client: {
      overlay: {
        warnings: false,
        errors: true
      }
    }
  },

  // 构建输出配置
  outputDir: path.resolve(__dirname, '../src/QJRWebWinform.WPF/wwwroot'),

  // 静态资源路径
  publicPath: process.env.NODE_ENV === 'production'
    ? './'  // 生产环境使用相对路径
    : '/',  // 开发环境使用绝对路径

  // 生产环境是否生成 sourceMap
  productionSourceMap: false,

  // 配置 webpack
  configureWebpack: {
    cache: false,
    resolve: {
      alias: {
        '@': path.resolve(__dirname, 'src')
      }
    },
    // 明确指定上下文目录，确保从正确的源目录读取文件
    context: __dirname,
    // 禁用所有可能的缓存
    snapshot: {
      managedPaths: [],
      immutablePaths: []
    }
  },

  // 链式操作 webpack 配置
  chainWebpack: config => {
    // 禁用所有缓存机制，确保每次都重新构建
    config.cache(false)
    
    // 禁用 cache-loader（如果存在）
    if (config.module.rules.has('cache-loader')) {
      config.module.rules.delete('cache-loader')
    }
    
    // 禁用 ESLint loader
    config.module.rules.delete('eslint')

    // 配置 CopyPlugin，排除已被 webpack 打包的文件
    // JsCrmHelper.js 通过 main.js 的 import 被打包，不需要直接复制到输出目录
    config.plugin('copy').tap(args => {
      if (args && args[0] && Array.isArray(args[0])) {
        args[0] = args[0].map(pattern => {
          // 如果是复制 public 目录的配置
          if (pattern.from && typeof pattern.from === 'string' && pattern.from.includes('public')) {
            return {
              ...pattern,
              globOptions: {
                ...(pattern.globOptions || {}),
                ignore: [
                  ...(pattern.globOptions?.ignore || []),
                  '**/js/JsCrmHelper.js' // 排除 JsCrmHelper.js，因为它已经被 webpack 打包到 chunk 文件中了
                ]
              }
            }
          }
          return pattern
        })
      }
      return args
    })

    // 生产环境优化
    if (process.env.NODE_ENV === 'production') {
      config.optimization.minimize(true)

      // 配置 Terser 插件，保留 console 语句（用于调试）
      // 如果需要移除 console，可以设置环境变量 REMOVE_CONSOLE=true
      const shouldRemoveConsole = process.env.REMOVE_CONSOLE === 'true'

      // 尝试配置 Terser 插件（Vue CLI 5.0+）
      if (config.optimization.minimizers.has('terser')) {
        config.optimization.minimizer('terser').tap(args => {
          if (args[0]) {
            args[0].terserOptions = args[0].terserOptions || {}
            args[0].terserOptions.compress = args[0].terserOptions.compress || {}
            // 保留 console 语句，除非明确指定移除
            args[0].terserOptions.compress.drop_console = shouldRemoveConsole
            args[0].terserOptions.compress.pure_funcs = shouldRemoveConsole ? ['console.log', 'console.info', 'console.debug'] : []
          }
          return args
        })
      }
    }
  },

  // 禁用 ESLint
  lintOnSave: false
}

