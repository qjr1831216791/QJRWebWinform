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

// 创建共享的全局变量对象，确保 Vue 实例和 context 使用同一个对象
// 这样无论在 Vue 组件中设置，还是在 context 中读取，都是同一个对象
const sharedGlobalVar = {}

Vue.prototype.$globalVar = sharedGlobalVar // 全局变量

// 创建 context 对象，供 JsCrmHelper.js 等脚本使用
// 这个对象包含全局配置，可以在 Vue 实例创建前使用（支持独立调试）
const context = {
  envconfig: WebConfig,
  axios: axios,
  $globalVar: sharedGlobalVar, // 使用共享的全局变量对象
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

/**
 * 检测当前是否为桌面端环境（WPF CefSharp）
 * @returns {boolean} true 表示桌面端，false 表示 Web 端
 */
function isDesktopEnvironment() {
  if (typeof window === 'undefined' || !window.location) {
    return false
  }
  // 桌面端使用 file:// 协议，Web 端使用 http:// 或 https://
  return window.location.protocol === 'file:'
}

/**
 * 等待 CefSharp NativeHost 初始化完成（返回 Promise 版本）
 * 仅在桌面端环境需要等待，Web 端立即返回 resolved Promise
 * @param {number} maxAttempts - 最大尝试次数，默认 150（15秒）
 * @returns {Promise<void>}
 */
function waitForNativeHostPromise(maxAttempts = 150) {
  // Web 端不需要等待 NativeHost
  if (!isDesktopEnvironment()) {
    return Promise.resolve()
  }

  return new Promise((resolve) => {
    let attempts = 0
    let isReady = false

    const callback = () => {
      if (!isReady) {
        isReady = true
        resolve()
      }
    }

    // 监听 nativeHostReady 事件
    window.nativeHostReady = function () {
      callback()
    }

    // 监听自定义事件
    window.addEventListener('nativeHostReady', function () {
      callback()
    })

    // 检查全局标志（如果后端已经设置）
    if (window.__nativeHostReady) {
      callback()
      return
    }

    const checkNativeHost = () => {
      attempts++

      // 检查全局标志
      if (window.__nativeHostReady) {
        callback()
        return
      }

      // 检查 nativeHost 是否可用（检查是否有 executeCommand 方法，统一入口）
      if (typeof window.nativeHost !== 'undefined' &&
        window.nativeHost &&
        typeof window.nativeHost.executeCommand === 'function') {
        callback()
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
  })
}

// 导出 waitForNativeHostPromise 供其他模块使用
if (typeof window !== 'undefined') {
  window.waitForNativeHostPromise = waitForNativeHostPromise
}

// 初始化 Vue 应用
// Web 端：立即启动，不等待 NativeHost
// 桌面端：等待 NativeHost 就绪后再启动（保持原有行为）
if (isDesktopEnvironment()) {
  // 桌面端：等待 NativeHost 就绪
  waitForNativeHostPromise().then(() => {
    initializeVueApp()
  })
} else {
  // Web 端：立即启动
  initializeVueApp()
}

/**
 * 初始化 Vue 应用
 */
function initializeVueApp() {
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
    // $globalVar 已经在初始化时共享，无需更新
    // 但为了确保一致性，仍然同步引用（实际上它们已经是同一个对象）
    ctx.$globalVar = app.$globalVar || ctx.$globalVar
    // 更新全局变量引用
    window.__globalVar__ = ctx.$globalVar
  }
}

