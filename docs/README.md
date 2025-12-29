# QJR Web Winform 开发文档

## 📋 项目概述

**QJR Web Winform** 是一个基于 **CefSharp + WPF + Vue 2** 的 Windows 桌面应用程序解决方案。

### 技术栈

- **前端框架**: Vue 2.6.14
- **前端构建工具**: Vue CLI 5.x
- **后端框架**: .NET Framework 4.7.2 + WPF
- **浏览器引擎**: CefSharp (Chromium)
- **通信方式**: JavaScript Binding (CefSharp)

### 项目结构

```
QJRWebWinform/
├── QJRWebWinform.sln                    # 解决方案文件
├── src/
│   └── QJRWebWinform.WPF/               # WPF 主项目
│       ├── App.xaml / App.xaml.cs      # 应用程序入口
│       ├── MainWindow.xaml / .cs       # 主窗口
│       ├── CefSharpHost.cs             # CefSharp 封装类
│       ├── NativeHost.cs                # 暴露给 JS 的 C# 对象
│       └── wwwroot/                    # 前端构建输出目录
├── frontend/                            # Vue 2 前端项目
│   ├── src/
│   │   ├── main.js                     # Vue 入口文件
│   │   ├── App.vue                     # 根组件
│   │   └── components/                 # Vue 组件
│   ├── public/
│   │   └── index.html                  # HTML 模板
│   ├── package.json                    # 前端依赖配置
│   └── vue.config.js                   # Vue CLI 配置
├── scripts/                             # 构建脚本
│   ├── build-frontend.ps1              # 前端构建脚本
│   └── build-all.ps1                   # 完整构建脚本
└── docs/                                # 文档目录
    └── README.md                        # 本文档
```

---

## 🚀 环境要求

### 后端开发环境

- **操作系统**: Windows 10/11 或 Windows Server 2016+
- **.NET Framework**: 4.7.2 或更高版本
- **Visual Studio**: 2019 或 2022 (推荐 2022)
  - 工作负载：.NET 桌面开发
- **NuGet**: Visual Studio 自带

### 前端开发环境

- **Node.js**: 14.x 或更高版本 (推荐 16.x LTS)
- **npm**: 6.x 或更高版本 (随 Node.js 安装)
- **Vue CLI**: 通过 `npm install -g @vue/cli` 全局安装

### 验证环境

```powershell
# 检查 Node.js 版本
node --version

# 检查 npm 版本
npm --version

# 检查 Vue CLI 版本
vue --version

# 检查 .NET Framework 版本
# 在 Visual Studio 中打开项目，查看项目属性
```

---

## 📦 项目初始化

### 1. 克隆或下载项目

```powershell
# 如果使用 Git
git clone <repository-url>
cd QJRWebWinform
```

### 2. 安装前端依赖

```powershell
cd frontend
npm install
```

### 3. 还原后端 NuGet 包

在 Visual Studio 中：
1. 右键点击解决方案
2. 选择"还原 NuGet 包"

或使用命令行：

```powershell
# 在项目根目录执行
nuget restore QJRWebWinform.sln
```

### 4. 构建前端（首次运行前必须）

```powershell
# 方式1: 使用构建脚本
.\scripts\build-frontend.ps1

# 方式2: 手动构建
cd frontend
npm run build
```

---

## 🔧 开发指南

### 前后端分离开发

本项目采用前后端分离架构，便于独立开发和调试。

#### 前端开发模式

1. **启动前端开发服务器**:

```powershell
cd frontend
npm run serve
```

开发服务器将在 `http://localhost:8080` 启动，支持热重载。

2. **修改前端代码**:
   - 编辑 `frontend/src/` 目录下的 Vue 文件
   - 保存后自动重新编译
   - 浏览器自动刷新（如果使用浏览器调试）

3. **前端调试**:
   - 在浏览器中打开 `http://localhost:8080`
   - 使用浏览器开发者工具 (F12) 调试
   - 支持 Vue DevTools 扩展

#### 后端开发模式

1. **在 Visual Studio 中打开项目**:
   - 双击 `QJRWebWinform.sln` 打开解决方案

2. **配置调试模式**:
   - 在 `MainWindow.xaml.cs` 的 `LoadFrontend()` 方法中
   - Debug 模式下会自动连接到 `http://localhost:8080`
   - Release 模式下加载本地 `wwwroot` 目录

3. **后端调试**:
   - 设置断点
   - 按 F5 启动调试
   - 使用 Visual Studio 调试工具

#### 集成调试（前后端同时运行）

1. **启动前端开发服务器**:
```powershell
cd frontend
npm run serve
```

2. **在 Visual Studio 中启动后端**:
   - 确保项目配置为 Debug 模式
   - 按 F5 启动调试
   - 后端会自动连接到前端开发服务器

3. **调试流程**:
   - 前端代码修改 → 自动热重载
   - 后端代码修改 → 重新编译并重启应用
   - 可以在前后端同时设置断点

---

## 🐛 调试步骤详解

### 前端调试

#### 方法1: 浏览器调试（推荐用于前端开发）

1. 启动前端开发服务器:
```powershell
cd frontend
npm run serve
```

2. 在浏览器中打开 `http://localhost:8080`

3. 使用浏览器开发者工具:
   - 按 `F12` 打开开发者工具
   - 在 Console 标签查看日志
   - 在 Sources 标签设置断点
   - 在 Network 标签查看网络请求

4. 使用 Vue DevTools:
   - 安装 Vue DevTools 浏览器扩展
   - 在开发者工具的 Vue 标签中查看组件状态

#### 方法2: 在 CefSharp 中调试

1. 确保后端运行在 Debug 模式

2. 在 `App.xaml.cs` 中启用 CefSharp 日志:
```csharp
settings.LogSeverity = CefSharp.LogSeverity.Info;
```

3. 在 Visual Studio 输出窗口查看 CefSharp 日志

4. 使用 CefSharp 的远程调试功能:
   - 在代码中启用远程调试端口
   - 使用 Chrome DevTools 连接到 CefSharp

### 后端调试

1. **设置断点**:
   - 在 `MainWindow.xaml.cs`、`NativeHost.cs` 等文件中设置断点

2. **启动调试**:
   - 按 `F5` 或点击"开始调试"
   - 应用启动后，断点会在代码执行到该位置时暂停

3. **调试工具**:
   - **局部变量窗口**: 查看当前作用域的变量
   - **监视窗口**: 监视特定表达式的值
   - **调用堆栈**: 查看方法调用链
   - **即时窗口**: 执行代码和表达式

4. **调试技巧**:
   - 使用 `System.Diagnostics.Debug.WriteLine()` 输出调试信息
   - 在输出窗口查看调试日志

### 前后端通信调试

#### JavaScript 调用 C# 方法

1. **在浏览器控制台测试**:
```javascript
// 检查 nativeHost 是否可用
console.log(window.nativeHost);

// 调用 C# 方法
window.nativeHost.ShowMessage('测试消息');
```

2. **在 C# 中设置断点**:
   - 在 `NativeHost.cs` 的方法中设置断点
   - 从 JavaScript 调用时，断点会触发

#### C# 调用 JavaScript 方法

1. **在 JavaScript 中定义全局函数**:
```javascript
window.myFunction = function(data) {
    console.log('收到来自后端的数据:', data);
};
```

2. **在 C# 中调用**:
```csharp
webBrowser.ExecuteScriptAsync("window.myFunction('Hello from C#')");
```

3. **在浏览器控制台查看结果**

---

## 🔨 编译和生成

### 前端构建

#### 开发环境构建

```powershell
cd frontend
npm run serve
```

#### 生产环境构建

```powershell
# 方式1: 使用构建脚本
.\scripts\build-frontend.ps1

# 方式2: 手动构建
cd frontend
npm run build
```

构建输出目录: `src/QJRWebWinform.WPF/wwwroot/`

#### 构建配置

前端构建配置在 `frontend/vue.config.js` 中:

- **输出目录**: 自动输出到 WPF 项目的 `wwwroot` 目录
- **公共路径**: 生产环境使用相对路径 `./`
- **Source Map**: 生产环境默认关闭（可在配置中启用）

### 后端构建

#### 在 Visual Studio 中构建

1. **选择配置**:
   - Debug: 开发调试版本
   - Release: 生产发布版本

2. **构建项目**:
   - 菜单: `生成` → `生成解决方案` (Ctrl+Shift+B)
   - 或右键项目 → `生成`

3. **输出目录**:
   - Debug: `src/QJRWebWinform.WPF/bin/Debug/`
   - Release: `src/QJRWebWinform.WPF/bin/Release/`

#### 使用命令行构建

```powershell
# 使用 MSBuild
msbuild QJRWebWinform.sln /t:Build /p:Configuration=Release /p:Platform="Any CPU"

# 或使用构建脚本
.\scripts\build-all.ps1
```

### 完整构建流程

#### 方式1: 使用自动化脚本（推荐）

```powershell
# 构建前端和后端
.\scripts\build-all.ps1
```

脚本会自动执行:
1. 构建前端项目
2. 构建后端项目
3. 输出最终文件到 `bin/Release/`

#### 方式2: 手动构建

1. **构建前端**:
```powershell
cd frontend
npm run build
```

2. **构建后端**:
   - 在 Visual Studio 中选择 Release 配置
   - 生成解决方案

3. **验证输出**:
   - 检查 `src/QJRWebWinform.WPF/bin/Release/wwwroot/` 目录
   - 确保包含前端构建文件

---

## 📦 部署

### 发布准备

1. **构建生产版本**:
```powershell
# 构建前端
cd frontend
npm run build

# 构建后端（Release 配置）
# 在 Visual Studio 中生成 Release 版本
```

2. **检查输出文件**:
   - 可执行文件: `QJRWebWinform.WPF.exe`
   - 前端文件: `wwwroot/` 目录
   - 依赖 DLL: CefSharp 相关文件

### 部署清单

发布时需要包含以下文件:

```
发布目录/
├── QJRWebWinform.WPF.exe          # 主程序
├── QJRWebWinform.WPF.exe.config   # 配置文件
├── wwwroot/                       # 前端文件
│   ├── index.html
│   ├── js/
│   ├── css/
│   └── ...
├── CefSharp.*.dll                 # CefSharp 核心 DLL
├── CefSharp.BrowserSubprocess.exe # 浏览器子进程
├── locales/                       # CefSharp 语言文件
├── swiftshader/                   # CefSharp 渲染文件
└── ...                            # 其他依赖 DLL
```

### 部署方式

#### 方式1: 直接复制文件

1. 复制 `bin/Release/` 目录下的所有文件
2. 确保包含所有依赖 DLL
3. 在目标机器上运行 `QJRWebWinform.WPF.exe`

#### 方式2: 使用 Visual Studio 发布

1. 右键项目 → `发布`
2. 选择发布目标（文件夹、FTP、Web Deploy 等）
3. 配置发布设置
4. 点击"发布"

#### 方式3: 创建安装程序

使用以下工具创建安装程序:
- **WiX Toolset**: 创建 MSI 安装包
- **Inno Setup**: 创建 EXE 安装程序
- **NSIS**: Nullsoft Scriptable Install System

---

## 🔌 前后端通信机制

### JavaScript 调用 C# 方法

#### 1. 在 C# 中定义方法（NativeHost.cs）

```csharp
public class NativeHost
{
    public void ShowMessage(string message)
    {
        MessageBox.Show(message);
    }
    
    public string GetData()
    {
        return "Hello from C#";
    }
}
```

#### 2. 在 JavaScript 中调用

```javascript
// 同步调用
window.nativeHost.ShowMessage('Hello from Vue!');

// 获取返回值
const data = window.nativeHost.GetData();
console.log(data);
```

#### 3. 异步调用（带回调）

```csharp
public void SaveData(string data, IJavascriptCallback callback)
{
    try
    {
        // 执行保存操作
        // ...
        callback.ExecuteAsync(true, "保存成功");
    }
    catch (Exception ex)
    {
        callback.ExecuteAsync(false, ex.Message);
    }
}
```

```javascript
window.nativeHost.SaveData('test data', (success, message) => {
    if (success) {
        console.log('成功:', message);
    } else {
        console.error('失败:', message);
    }
});
```

### C# 调用 JavaScript 方法

#### 1. 在 JavaScript 中定义全局函数

```javascript
window.updateUI = function(data) {
    console.log('收到数据:', data);
    // 更新 Vue 组件
    app.$data.message = data;
};
```

#### 2. 在 C# 中调用

```csharp
// 执行 JavaScript 代码
webBrowser.ExecuteScriptAsync("window.updateUI('Hello from C#')");

// 或调用方法并传递数据
string jsonData = JsonConvert.SerializeObject(new { message = "Hello" });
webBrowser.ExecuteScriptAsync($"window.updateUI({jsonData})");
```

### 通信最佳实践

1. **错误处理**:
   - JavaScript 中检查 `window.nativeHost` 是否存在
   - C# 中使用 try-catch 捕获异常

2. **数据序列化**:
   - 复杂对象使用 JSON 序列化
   - 简单类型可以直接传递

3. **异步操作**:
   - 长时间操作使用回调函数
   - 避免阻塞 UI 线程

---

## ❓ 常见问题

### 1. CefSharp 初始化失败

**问题**: 应用启动时提示 CefSharp 初始化失败

**解决方案**:
- 确保已安装所有 CefSharp NuGet 包
- 检查目标平台（x86/x64/AnyCPU）是否匹配
- 确保 CefSharp 运行时文件存在于输出目录

### 2. 前端页面无法加载

**问题**: 应用启动后显示空白页面或错误

**解决方案**:
- Debug 模式: 确保前端开发服务器运行在 `http://localhost:8080`
- Release 模式: 确保已构建前端并输出到 `wwwroot` 目录
- 检查 `MainWindow.xaml.cs` 中的 `LoadFrontend()` 方法

### 3. JavaScript 无法调用 C# 方法

**问题**: `window.nativeHost` 未定义或调用失败

**解决方案**:
- 确保页面完全加载后再调用（使用 `FrameLoadEnd` 事件）
- 检查 `CefSharpHost.cs` 中的 `RegisterNativeHost()` 方法
- 在浏览器控制台检查 `window.nativeHost` 是否存在

### 4. 前端热重载不工作

**问题**: 修改前端代码后页面不自动刷新

**解决方案**:
- 确保使用 `npm run serve` 启动开发服务器
- 检查浏览器控制台是否有错误
- 尝试手动刷新页面

### 5. 构建失败

**问题**: 执行构建脚本时出错

**解决方案**:
- 检查 Node.js 和 npm 是否正确安装
- 确保已安装前端依赖 (`npm install`)
- 检查 `vue.config.js` 配置是否正确
- 查看错误日志获取详细信息

### 6. 发布后无法运行

**问题**: 在目标机器上运行应用失败

**解决方案**:
- 确保目标机器安装了 .NET Framework 4.7.2 或更高版本
- 检查是否包含所有 CefSharp 依赖文件
- 查看 Windows 事件查看器中的错误日志

---

## 📚 相关资源

### 官方文档

- [CefSharp 文档](https://github.com/cefsharp/CefSharp)
- [Vue 2 文档](https://v2.vuejs.org/)
- [Vue CLI 文档](https://cli.vuejs.org/)
- [WPF 文档](https://docs.microsoft.com/zh-cn/dotnet/desktop/wpf/)

### 学习资源

- [CefSharp 示例项目](https://github.com/cefsharp/CefSharp.MinimalExample)
- [Vue 2 教程](https://v2.vuejs.org/v2/guide/)
- [WPF 教程](https://docs.microsoft.com/zh-cn/dotnet/desktop/wpf/getting-started/)

---

## 📝 更新日志

### v1.0.0 (2024-01-XX)

- ✅ 初始版本发布
- ✅ 集成 CefSharp + WPF + Vue 2
- ✅ 实现前后端通信机制
- ✅ 提供完整的开发文档

---

## 👥 贡献

欢迎提交 Issue 和 Pull Request！

---

## 📄 许可证

[在此添加许可证信息]

---

## 📧 联系方式

如有问题或建议，请联系项目维护者。

---

**最后更新**: 2024-01-XX

