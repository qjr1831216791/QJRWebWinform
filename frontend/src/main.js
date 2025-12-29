// The Vue build version to load with the `import` command
import Vue from 'vue'
import App from './App'
import router from './router'
import ElementUI from 'element-ui'
import 'element-ui/lib/theme-chalk/index.css'
import axios from 'axios'
import { rtcrm } from "../public/js/rtcrm.min.js"
import { jshelper } from "../public/js/JsCrmHelper.js"
import { WebConfig } from "../public/config/WebConfig.js"
import * as echarts from 'echarts'

// 设置全局属性（来自 VueWebProj）
Vue.prototype.$echarts = echarts
Vue.prototype.axios = axios
Vue.prototype.rtcrm = rtcrm // 公共脚本
Vue.prototype.jshelper = jshelper // 公共脚本
Vue.prototype.envconfig = WebConfig // 全局环境配置
Vue.prototype.$globalVar = {} // 全局变量

// 创建 context 对象，供 JsCrmHelper.js 等脚本使用
// 这个对象包含全局配置，可以在 Vue 实例创建前使用（支持独立调试）
const context = {
  envconfig: WebConfig,
  axios: axios,
  $globalVar: {},
  // Vue 实例创建后，这些方法会被更新为真实的 Vue 方法
  $confirm: function() {
    if (window.Vue && window.Vue.prototype.$confirm) {
      return window.Vue.prototype.$confirm.apply(this, arguments)
    }
    return Promise.resolve()
  },
  $loading: function(options) {
    if (window.Vue && window.Vue.prototype.$loading) {
      return window.Vue.prototype.$loading.call(this, options)
    }
    return { close: function() {} }
  }
}

// 设置全局变量，支持独立调试（当 Vue 实例未创建时）
if (typeof window !== 'undefined') {
  window.__WebConfig__ = WebConfig
  window.axios = axios
  window.__globalVar__ = context.$globalVar
  window.Vue = Vue
  window.__VUE_CONTEXT__ = context
}

// 导出 context，供 JsCrmHelper.js 使用
export default context

// 注册 Element UI
Vue.use(ElementUI)
Vue.config.productionTip = false

// 等待 CefSharp NativeHost 初始化完成（来自 QJRWebWinform）
function waitForNativeHost(callback, maxAttempts = 150) {
  let attempts = 0
  let isReady = false

  // 监听 nativeHostReady 事件
  window.nativeHostReady = function () {
    if (!isReady) {
      isReady = true
      callback()
    }
  }

  // 监听自定义事件
  window.addEventListener('nativeHostReady', function () {
    if (!isReady) {
      isReady = true
      callback()
    }
  })

  // 检查全局标志（如果后端已经设置）
  if (window.__nativeHostReady) {
    isReady = true
    callback()
    return
  }

  const checkNativeHost = () => {
    attempts++

    // 检查全局标志
    if (window.__nativeHostReady) {
      if (!isReady) {
        isReady = true
        callback()
      }
      return
    }

    // 检查 nativeHost 是否可用（检查是否有 executeCommand 方法，统一入口）
    if (typeof window.nativeHost !== 'undefined' &&
      window.nativeHost &&
      typeof window.nativeHost.executeCommand === 'function') {
      if (!isReady) {
        isReady = true
        callback()
      }
    } else if (attempts < maxAttempts) {
      setTimeout(checkNativeHost, 100)
    } else {
      // 超时后继续运行（可能 NativeHost 还未准备好，但不影响应用启动）
      // 注意：如果应用依赖 NativeHost，这里可能需要显示错误提示
      console.warn('NativeHost 初始化超时，某些功能可能不可用')
      callback()
    }
  }

  // 延迟一小段时间再开始轮询，给后端一些时间完成注册
  setTimeout(checkNativeHost, 200)
}

// 初始化 Vue 应用
waitForNativeHost(() => {
  const app = new Vue({
    router,
    render: h => h(App)
  }).$mount('#app')
  
  // 更新全局 context，使用真实的 Vue 实例方法
  if (typeof window !== 'undefined' && window.__VUE_CONTEXT__) {
    // 更新 context 的方法，使其使用真实的 Vue 实例
    const ctx = window.__VUE_CONTEXT__
    ctx.$confirm = app.$confirm.bind(app)
    ctx.$loading = app.$loading.bind(app)
    ctx.$globalVar = app.$globalVar || ctx.$globalVar
    // 更新全局变量引用
    window.__globalVar__ = ctx.$globalVar
  }
})

