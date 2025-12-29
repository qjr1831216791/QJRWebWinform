# CefSharp 方法名大小写转换说明

## 问题现象

在浏览器控制台中查看 `window.nativeHost` 时，可能会看到：
- JavaScript 属性名：`closeWindow`（camelCase）
- 实际调用显示：`closewindow()`（全小写）

这与 C# 中的方法名 `CloseWindow`（PascalCase）不一致。

## 原因分析

### CefSharp 的默认命名转换

CefSharp 默认使用 **`LegacyCamelCaseJavascriptNameConverter`** 来转换方法名：

1. **C# 方法名**（PascalCase）→ **JavaScript 属性名**（camelCase）
   - `ShowMessage` → `showMessage`
   - `GetSystemInfo` → `getSystemInfo`
   - `CloseWindow` → `closeWindow`
   - `SetWindowTitle` → `setWindowTitle`

2. **实际调用**
   - JavaScript 中使用 camelCase 属性名调用
   - CefSharp 内部会将其映射回 C# 的原始方法名（不区分大小写）
   - 所以 `closeWindow()` 实际上调用的是 C# 的 `CloseWindow()` 方法

### 为什么显示为 `closewindow()`？

在 Chrome DevTools 中显示 `closeWindow: f closewindow()` 可能是因为：
1. **属性名**：`closeWindow`（camelCase，这是 JavaScript 中使用的名称）
2. **函数显示名**：`closewindow()`（可能是 DevTools 的内部显示，或 CefSharp 的内部映射）

**重要**：虽然显示为 `closewindow()`，但实际调用时应该使用 `closeWindow`（camelCase）。

## 解决方案

### 方案 1：使用 camelCase（推荐，符合 JavaScript 规范）

保持 CefSharp 的默认行为，在 JavaScript 中使用 camelCase：

```javascript
// ✅ 正确
window.nativeHost.showMessage('Hello');
window.nativeHost.getSystemInfo();
window.nativeHost.closeWindow();
window.nativeHost.setWindowTitle('New Title');

// ❌ 错误（大小写不匹配）
window.nativeHost.ShowMessage('Hello');  // undefined
window.nativeHost.CloseWindow();         // undefined
```

### 方案 2：禁用命名转换（保持 PascalCase）

如果希望 JavaScript 中的方法名与 C# 保持一致，可以禁用命名转换：

```csharp
// 在 CefSharpHost.cs 的 RegisterNativeHost 方法中
_browser.JavascriptObjectRepository.NameConverter = null;
_browser.JavascriptObjectRepository.Register("nativeHost", _nativeHost, isAsync: false);
```

这样，JavaScript 中就可以使用 PascalCase：

```javascript
// ✅ 正确（禁用转换后）
window.nativeHost.ShowMessage('Hello');
window.nativeHost.CloseWindow();
window.nativeHost.GetSystemInfo();
```

### 方案 3：自定义命名转换器

如果需要自定义转换规则，可以实现 `IJavascriptNameConverter` 接口：

```csharp
public class CustomNameConverter : IJavascriptNameConverter
{
    public string ConvertToJavaScript(string name)
    {
        // 自定义转换逻辑
        return name.ToLower(); // 例如：全部转为小写
    }
}

// 使用
_browser.JavascriptObjectRepository.NameConverter = new CustomNameConverter();
```

## 当前代码中的问题

查看 `App.vue` 中的代码：

```javascript
// 第 158 行：检查 ShowMessage（PascalCase）
hasShowMessage: typeof window.nativeHost.ShowMessage === 'function'

// 第 166 行：检查 showMessage（camelCase）
if (typeof window.nativeHost.showMessage !== 'function') {
```

**问题**：代码中混用了 PascalCase 和 camelCase，这会导致检查失败。

## 修复建议

### 修复 App.vue 中的方法名

将所有方法名统一为 camelCase（CefSharp 的默认转换）：

```javascript
// 修复 checkNativeHost 方法
checkNativeHost() {
  console.log('检查 NativeHost:', {
    exists: typeof window.nativeHost !== 'undefined',
    value: window.nativeHost,
    // ✅ 使用 camelCase
    hasShowMessage: typeof window.nativeHost !== 'undefined' && 
                   typeof window.nativeHost.showMessage === 'function'
  })
  
  if (typeof window.nativeHost === 'undefined' || !window.nativeHost) {
    this.result = '错误: NativeHost 未初始化...'
    return false
  }
  
  // ✅ 使用 camelCase
  if (typeof window.nativeHost.showMessage !== 'function') {
    this.result = '错误: NativeHost 方法不可用...'
    return false
  }
  return true
}

// 修复所有方法调用
showMessage() {
  if (this.checkNativeHost()) {
    try {
      // ✅ 使用 camelCase
      window.nativeHost.showMessage('这是一条来自前端的消息！')
      this.result = '已调用 showMessage 方法'
    } catch (error) {
      this.result = `错误: ${error.message}`
    }
  }
},

getSystemInfo() {
  if (this.checkNativeHost()) {
    try {
      // ✅ 使用 camelCase
      const info = window.nativeHost.getSystemInfo()
      this.result = info
    } catch (error) {
      this.result = `错误: ${error.message}`
    }
  }
},

// ... 其他方法同样使用 camelCase
```

## 总结

1. **CefSharp 默认行为**：将 C# 的 PascalCase 转换为 JavaScript 的 camelCase
2. **JavaScript 中应使用**：camelCase（`showMessage`, `closeWindow` 等）
3. **C# 中保持**：PascalCase（`ShowMessage`, `CloseWindow` 等）
4. **当前问题**：前端代码混用了大小写，需要统一为 camelCase

