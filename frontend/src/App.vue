<template>
  <div id="app">
    <div class="container">
      <header class="header">
        <h1>QJR Web Winform</h1>
        <p class="subtitle">基于 CefSharp + WPF + Vue 2 的桌面应用程序</p>
      </header>

      <main class="main-content">
        <section class="demo-section">
          <h2>前后端通信示例</h2>

          <div class="button-group">
            <button @click="showMessage" class="btn btn-primary">
              显示消息框
            </button>

            <button @click="getSystemInfo" class="btn btn-primary">
              获取系统信息
            </button>

            <button @click="setWindowTitle" class="btn btn-secondary">
              设置窗口标题
            </button>

            <button @click="closeWindow" class="btn btn-danger">
              关闭窗口
            </button>
          </div>

          <div class="result-area" v-if="result">
            <h3>执行结果：</h3>
            <pre>{{ result }}</pre>
          </div>
        </section>

        <section class="info-section">
          <h2>技术栈</h2>
          <ul>
            <li><strong>前端：</strong>Vue 2.6.14 + Vue CLI</li>
            <li><strong>后端：</strong>.NET Framework 4.7.2 + WPF</li>
            <li><strong>浏览器引擎：</strong>CefSharp (Chromium)</li>
            <li><strong>通信方式：</strong>JavaScript Binding</li>
          </ul>
        </section>
      </main>
    </div>
  </div>
</template>

<script>
export default {
  name: 'App',
  data() {
    return {
      result: ''
    }
  },
  methods: {
    // 显示消息框
    showMessage() {
      if (this.checkNativeHost()) {
        try {
          const params = JSON.stringify({ message: '这是一条来自前端的消息！', title: '测试消息' })
          const result = JSON.parse(window.nativeHost.executeCommand('System/ShowMessage', params))

          if (result.success) {
            this.result = '已调用 System/ShowMessage'
          } else {
            this.result = `错误: ${result.error}`
          }
        } catch (error) {
          this.result = `错误: ${error.message}`
        }
      }
    },

    // 获取系统信息
    getSystemInfo() {
      if (this.checkNativeHost()) {
        try {
          const result = JSON.parse(window.nativeHost.executeCommand('System/GetSystemInfo'))

          if (result.success) {
            const info = result.data
            this.result = `操作系统: ${info.osVersion}\n.NET 版本: ${info.dotNetVersion}\n计算机名: ${info.machineName}\n用户名: ${info.userName}\n处理器数: ${info.processorCount}`
          } else {
            this.result = `错误: ${result.error}`
          }
        } catch (error) {
          this.result = `错误: ${error.message}`
        }
      }
    },

    // 设置窗口标题
    setWindowTitle() {
      if (this.checkNativeHost()) {
        try {
          const newTitle = `QJR Web Winform - ${new Date().toLocaleTimeString()}`
          const params = JSON.stringify({ title: newTitle })
          const result = JSON.parse(window.nativeHost.executeCommand('Window/SetTitle', params))

          if (result.success) {
            this.result = `窗口标题已设置为: ${result.data.title}`
          } else {
            this.result = `错误: ${result.error}`
          }
        } catch (error) {
          this.result = `错误: ${error.message}`
        }
      }
    },

    // 关闭窗口
    closeWindow() {
      if (this.checkNativeHost()) {
        if (confirm('确定要关闭窗口吗？')) {
          try {
            const result = JSON.parse(window.nativeHost.executeCommand('Window/Close'))

            if (result.success) {
              this.result = '窗口关闭命令已执行'
            } else {
              this.result = `错误: ${result.error}`
            }
          } catch (error) {
            this.result = `错误: ${error.message}`
          }
        }
      }
    },

    // 检查 NativeHost 是否可用
    checkNativeHost() {
      if (typeof window.nativeHost === 'undefined' || !window.nativeHost) {
        this.result = '错误: NativeHost 未初始化，请等待页面加载完成'
        return false
      }
      // 检查是否有 executeCommand 方法（统一入口）
      if (typeof window.nativeHost.executeCommand !== 'function') {
        this.result = `错误: NativeHost 方法不可用\n可用方法: ${Object.keys(window.nativeHost || {}).join(', ')}`
        return false
      }
      return true
    }
  }
}
</script>

<style>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

#app {
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  color: #2c3e50;
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

.header {
  text-align: center;
  color: white;
  margin-bottom: 40px;
  padding: 30px 0;
}

.header h1 {
  font-size: 2.5em;
  margin-bottom: 10px;
  text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.2);
}

.subtitle {
  font-size: 1.2em;
  opacity: 0.9;
}

.main-content {
  background: white;
  border-radius: 10px;
  padding: 30px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
}

.demo-section {
  margin-bottom: 40px;
}

.demo-section h2 {
  color: #667eea;
  margin-bottom: 20px;
  font-size: 1.8em;
}

.button-group {
  display: flex;
  flex-wrap: wrap;
  gap: 15px;
  margin-bottom: 30px;
}

.btn {
  padding: 12px 24px;
  border: none;
  border-radius: 5px;
  font-size: 16px;
  cursor: pointer;
  transition: all 0.3s ease;
  font-weight: 500;
}

.btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.2);
}

.btn-primary {
  background: #667eea;
  color: white;
}

.btn-primary:hover {
  background: #5568d3;
}

.btn-secondary {
  background: #48bb78;
  color: white;
}

.btn-secondary:hover {
  background: #38a169;
}

.btn-danger {
  background: #f56565;
  color: white;
}

.btn-danger:hover {
  background: #e53e3e;
}

.result-area {
  background: #f7fafc;
  border: 1px solid #e2e8f0;
  border-radius: 5px;
  padding: 20px;
  margin-top: 20px;
}

.result-area h3 {
  color: #2d3748;
  margin-bottom: 10px;
}

.result-area pre {
  background: white;
  padding: 15px;
  border-radius: 5px;
  border: 1px solid #e2e8f0;
  white-space: pre-wrap;
  word-wrap: break-word;
  font-family: 'Consolas', 'Monaco', monospace;
  font-size: 14px;
  line-height: 1.6;
}

.info-section {
  border-top: 2px solid #e2e8f0;
  padding-top: 30px;
}

.info-section h2 {
  color: #667eea;
  margin-bottom: 15px;
  font-size: 1.5em;
}

.info-section ul {
  list-style: none;
  padding-left: 0;
}

.info-section li {
  padding: 10px 0;
  border-bottom: 1px solid #e2e8f0;
  font-size: 16px;
}

.info-section li:last-child {
  border-bottom: none;
}

.info-section strong {
  color: #667eea;
}
</style>
