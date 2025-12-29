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
    resolve: {
      alias: {
        '@': path.resolve(__dirname, 'src')
      }
    }
  },

  // 链式操作 webpack 配置
  chainWebpack: config => {
    // 生产环境优化
    if (process.env.NODE_ENV === 'production') {
      config.optimization.minimize(true)
    }
  }
}

