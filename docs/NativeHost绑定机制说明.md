# CefSharp NativeHost 绑定机制详解

## 绑定流程概述

`window.nativeHost` 的绑定过程涉及以下几个关键步骤：

### 1. 初始化阶段（App.xaml.cs）

```csharp
// 启用同步 JavaScript 绑定（必须在创建 ChromiumWebBrowser 之前设置）
CefSharpSettings.WcfEnabled = true;
```

**作用**：
- 启用 WCF（Windows Communication Foundation）支持
- 这是同步绑定的前提条件
- 必须在创建任何 `ChromiumWebBrowser` 实例之前设置

### 2. 注册阶段（CefSharpHost.cs）

```csharp
_browser.JavascriptObjectRepository.Register("nativeHost", _nativeHost, isAsync: false);
```

**绑定机制**：

#### 同步绑定（isAsync: false）
- **工作原理**：
  1. CefSharp 在 V8 JavaScript 引擎的全局作用域中创建一个代理对象
  2. 这个代理对象直接映射到 C# 的 `NativeHost` 实例
  3. 对象名称就是注册时使用的字符串 `"nativeHost"`
  4. **关键**：同步绑定必须在页面加载**之前**完成，否则对象不会出现在 JavaScript 全局作用域中

- **对象位置**：
  - 如果成功，对象应该在 JavaScript 的**全局作用域**中，即 `window.nativeHost` 或直接 `nativeHost`
  - 但根据 CefSharp 的实现，可能需要通过 `window.nativeHost` 访问

#### 异步绑定（isAsync: true）
- **工作原理**：
  1. CefSharp 创建一个异步绑定对象
  2. 前端需要通过 `CefSharp.bindObjectAsync('nativeHost', 'nativeHost')` 来绑定
  3. 绑定后对象才会在 JavaScript 中可用

- **对象位置**：
  - 需要手动调用 `CefSharp.bindObjectAsync` 后才会出现在全局作用域
  - 或者通过 JavaScript 注入：`window.nativeHost = nativeHost`

### 3. 绑定时机

**关键问题**：同步绑定必须在页面加载前完成！

```
时间线：
1. Cef.Initialize() - 初始化 CefSharp
2. 创建 ChromiumWebBrowser 实例
3. 浏览器初始化完成 (IsBrowserInitialized = true)
4. ⚠️ 注册 NativeHost（必须在此步骤完成）
5. 设置 WebBrowser.Address（开始加载页面）
6. 页面开始加载 (FrameLoadStart)
7. 页面加载完成 (FrameLoadEnd)
```

如果注册发生在步骤 5 之后，同步绑定会失败！

## 当前代码的问题分析

### 问题 1：注册时机可能不对

虽然代码中尝试在页面加载前注册，但可能存在竞态条件：
- `IsBrowserInitializedChanged` 事件触发时，页面可能已经开始加载
- 需要确保在设置 `Address` 之前注册完成

### 问题 2：同步绑定可能未生效

即使 `IsBound("nativeHost")` 返回 `true`，也不意味着对象在 JavaScript 中可用：
- `IsBound` 只检查 C# 端的注册状态
- 不检查 JavaScript 端的可用性

### 问题 3：对象可能不在 window 作用域中

CefSharp 的同步绑定可能将对象放在：
- 全局作用域（`nativeHost`）而不是 `window.nativeHost`
- 或者需要特殊的访问方式

## 调试方法

### 1. 检查注册状态（C# 端）

```csharp
bool isBound = _browser.JavascriptObjectRepository.IsBound("nativeHost");
System.Diagnostics.Debug.WriteLine($"IsBound: {isBound}");
```

### 2. 检查 JavaScript 端可用性

在浏览器控制台（F12）中执行：

```javascript
// 检查全局作用域
console.log('nativeHost (全局):', typeof nativeHost);
console.log('window.nativeHost:', typeof window.nativeHost);

// 检查 CefSharp 对象
console.log('CefSharp:', typeof CefSharp);

// 如果使用异步绑定
if (typeof CefSharp !== 'undefined' && CefSharp.bindObjectAsync) {
    CefSharp.bindObjectAsync('nativeHost', 'nativeHost').then(function(bound) {
        console.log('绑定结果:', bound);
        console.log('window.nativeHost:', typeof window.nativeHost);
    });
}
```

### 3. 检查绑定时机

在代码中添加日志：

```csharp
System.Diagnostics.Debug.WriteLine($"注册时间: {DateTime.Now:HH:mm:ss.fff}");
System.Diagnostics.Debug.WriteLine($"浏览器初始化: {_browser.IsBrowserInitialized}");
System.Diagnostics.Debug.WriteLine($"页面地址: {_browser.Address}");
```

## 解决方案

### 方案 1：确保在页面加载前注册（推荐）

```csharp
// 在 MainWindow.xaml.cs 中
private void WebBrowser_Loaded(object sender, RoutedEventArgs e)
{
    _nativeHost = new NativeHost(this);
    _cefHost = new CefSharpHost(WebBrowser, _nativeHost);
    
    // 等待浏览器初始化
    WebBrowser.IsBrowserInitializedChanged += (s, args) =>
    {
        if (WebBrowser.IsBrowserInitialized)
        {
            // 等待注册完成
            bool registered = _cefHost.WaitForRegistration(5000);
            
            // 确保注册完成后再加载页面
            if (registered)
            {
                LoadFrontend();
            }
        }
    };
}
```

### 方案 2：使用 JavaScript 注入（备选）

如果同步绑定失败，在页面加载后通过 JavaScript 注入：

```csharp
string injectScript = @"
    if (typeof nativeHost !== 'undefined') {
        window.nativeHost = nativeHost;
    } else if (typeof CefSharp !== 'undefined' && CefSharp.bindObjectAsync) {
        CefSharp.bindObjectAsync('nativeHost', 'nativeHost').then(function() {
            window.nativeHost = nativeHost;
        });
    }
";
_browser.ExecuteScriptAsync(injectScript);
```

### 方案 3：完全使用异步绑定

如果同步绑定始终不工作，改用异步绑定：

```csharp
// 注册时使用异步绑定
_browser.JavascriptObjectRepository.Register("nativeHost", _nativeHost, isAsync: true);

// 在页面加载后注入绑定代码
string bindScript = @"
    if (typeof CefSharp !== 'undefined' && CefSharp.bindObjectAsync) {
        CefSharp.bindObjectAsync('nativeHost', 'nativeHost').then(function() {
            window.nativeHost = nativeHost;
            console.log('NativeHost 已绑定到 window');
        });
    }
";
_browser.ExecuteScriptAsync(bindScript);
```

## 总结

`window.nativeHost` 的绑定依赖于：

1. **CefSharpSettings.WcfEnabled = true**（同步绑定前提）
2. **在页面加载前完成注册**（同步绑定要求）
3. **正确的注册方法调用**（`Register` 方法）
4. **对象在 JavaScript 全局作用域中的可用性**（可能需要手动注入到 `window`）

当前问题很可能是**时机问题**或**CefSharp 120 版本的同步绑定机制变化**。建议：
1. 先尝试确保在页面加载前注册
2. 如果仍不行，使用异步绑定 + JavaScript 注入的方案

