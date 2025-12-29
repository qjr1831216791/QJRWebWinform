import Vue from 'vue'
import App from './App.vue'

Vue.config.productionTip = false

// 等待 CefSharp 初始化完成
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

    // 检查 nativeHost 是否可用（检查是否有方法，注意使用 camelCase）
    if (typeof window.nativeHost !== 'undefined' &&
      window.nativeHost &&
      typeof window.nativeHost.showMessage === 'function') {
      if (!isReady) {
        isReady = true
        callback()
      }
    } else if (attempts < maxAttempts) {
      setTimeout(checkNativeHost, 100)
    } else {
      // 超时后继续运行（可能 NativeHost 还未准备好，但不影响应用启动）
      callback()
    }
  }

  // 延迟一小段时间再开始轮询，给后端一些时间完成注册
  setTimeout(checkNativeHost, 200)
}

// 初始化 Vue 应用
waitForNativeHost(() => {
  new Vue({
    render: h => h(App)
  }).$mount('#app')
})

