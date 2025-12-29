# NativeHost Controller/Action 架构使用说明

## 架构概述

NativeHost 采用类似 WebAPI 的 Controller/Action 架构，通过路由 `ControllerName/ActionName` 来调用功能。

### 架构优势

1. **分层清晰**：Controller 包含多个相关的 Action
2. **易于组织**：相关功能集中在同一个 Controller 中
3. **易于扩展**：新增功能只需在对应 Controller 中添加 Action 方法
4. **路由清晰**：`ControllerName/ActionName` 格式直观明了
5. **类型安全**：通过参数类定义参数结构

## 架构设计

```
NativeHost (统一入口)
    ↓
ControllerRegistry (Controller 注册表)
    ↓
IController (Controller 接口)
    ↓
ControllerBase (Controller 基类)
    ↓
具体 Controller 类 (SystemController, WindowController 等)
    ↓
Action 方法 (GetSystemInfo, ShowMessage 等)
```

## 路由格式

路由格式：`ControllerName/ActionName`

示例：
- `System/GetSystemInfo` - System Controller 的 GetSystemInfo Action
- `System/ShowMessage` - System Controller 的 ShowMessage Action
- `Window/SetTitle` - Window Controller 的 SetTitle Action
- `Window/Close` - Window Controller 的 Close Action
- `Data/Process` - Data Controller 的 Process Action
- `Data/Save` - Data Controller 的 Save Action（异步）

## 前端调用方式

### 同步 Action 调用

```javascript
// 基本调用（无参数）
const result = JSON.parse(window.nativeHost.executeCommand('System/GetSystemInfo'))
if (result.success) {
    console.log('系统信息:', result.data)
} else {
    console.error('错误:', result.error)
}

// 带参数调用
const params = JSON.stringify({ message: 'Hello', title: '测试' })
const result = JSON.parse(window.nativeHost.executeCommand('System/ShowMessage', params))
```

### 异步 Action 调用

```javascript
// 异步调用（带回调）
const params = JSON.stringify({ data: 'test data' })
window.nativeHost.executeCommandAsync('Data/Save', params, (success, message) => {
    if (success) {
        console.log('成功:', message)
    } else {
        console.error('失败:', message)
    }
})
```

### 返回结果格式

所有 Action 返回统一的 JSON 格式：

**成功时**：
```json
{
    "success": true,
    "data": { /* Action 返回的数据 */ }
}
```

**失败时**：
```json
{
    "success": false,
    "error": "错误信息"
}
```

## 后端开发新 Controller

### 步骤 1：创建 Controller 类

在 `Controllers` 目录下创建新的 Controller 类，继承 `ControllerBase`：

```csharp
using System;
using System.Windows;
using QJRWebWinform.WPF.Controllers;

namespace QJRWebWinform.WPF.Controllers
{
    /// <summary>
    /// 配置相关 Controller
    /// </summary>
    public class SyncConfigurationController : ControllerBase
    {
        public override string Name => "SyncConfiguration";

        public SyncConfigurationController(Window mainWindow) : base(mainWindow)
        {
        }

        /// <summary>
        /// 获取系统配置
        /// </summary>
        public object GetSystemConfigs(string parameters)
        {
            // 解析参数（如果需要）
            var param = DeserializeParameters<GetConfigsParams>(parameters);
            
            // 执行业务逻辑
            return new
            {
                config1 = "value1",
                config2 = "value2"
            };
        }

        /// <summary>
        /// 保存配置（异步）
        /// </summary>
        public void SaveConfig(string parameters, CefSharp.IJavascriptCallback callback)
        {
            try
            {
                var param = DeserializeParameters<SaveConfigParams>(parameters);
                // 异步保存逻辑
                System.Threading.Tasks.Task.Run(() =>
                {
                    // 保存操作
                    callback.ExecuteAsync(true, "配置保存成功");
                });
            }
            catch (Exception ex)
            {
                callback.ExecuteAsync(false, ex.Message);
            }
        }

        private class GetConfigsParams
        {
            public string Category { get; set; }
        }

        private class SaveConfigParams
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }
    }
}
```

### 步骤 2：注册 Controller

在 `NativeHost.cs` 的 `RegisterControllers()` 方法中注册新 Controller：

```csharp
private void RegisterControllers()
{
    ControllerRegistry.RegisterRange(new IController[]
    {
        // ... 现有 Controller
        new SyncConfigurationController(_mainWindow)  // 添加新 Controller
    });
}
```

### 步骤 3：前端调用

```javascript
// 前端调用
const params = JSON.stringify({ category: 'system' })
const result = JSON.parse(window.nativeHost.executeCommand('SyncConfiguration/GetSystemConfigs', params))
```

## Action 方法约定

### 同步 Action

- 方法签名：`public object ActionName(string parameters)`
- 返回类型：`object`（将被序列化为 JSON）
- 参数：`string parameters`（JSON 字符串）

示例：
```csharp
public object GetSystemInfo(string parameters)
{
    return new { osVersion = "..." };
}
```

### 异步 Action

- 方法签名：`public void ActionName(string parameters, IJavascriptCallback callback)`
- 返回类型：`void`
- 参数：`string parameters`（JSON 字符串）和 `IJavascriptCallback callback`

示例：
```csharp
public void Save(string parameters, IJavascriptCallback callback)
{
    System.Threading.Tasks.Task.Run(() =>
    {
        // 异步操作
        callback.ExecuteAsync(true, "保存成功");
    });
}
```

## 现有 Controller 和 Action

### System Controller

| Action | 说明 | 参数 | 返回 |
|--------|------|------|------|
| `GetSystemInfo` | 获取系统信息 | 无 | `{ osVersion, dotNetVersion, ... }` |
| `ShowMessage` | 显示消息框 | `{ message: string, title?: string }` | `{ success: true, message: "..." }` |

### Window Controller

| Action | 说明 | 参数 | 返回 |
|--------|------|------|------|
| `SetTitle` | 设置窗口标题 | `{ title: string }` | `{ success: true, title: "..." }` |
| `Close` | 关闭窗口 | 无 | `{ success: true, message: "..." }` |

### Data Controller

| Action | 说明 | 参数 | 返回 |
|--------|------|------|------|
| `Process` | 处理数据 | `{ input: string }` | `{ input, output, processed }` |
| `Save` | 保存数据（异步） | `{ data: string }` | 通过回调返回 |

## 最佳实践

### 1. Controller 命名

- 使用 PascalCase：`SyncConfigurationController`
- Controller 名称（Name 属性）通常与类名去掉 `Controller` 后缀一致：`SyncConfiguration`

### 2. Action 命名

- 使用 PascalCase：`GetSystemConfigs`
- 动词开头：`Get`, `Save`, `Delete`, `Update` 等

### 3. 参数验证

```csharp
public object GetSystemConfigs(string parameters)
{
    var param = DeserializeParameters<GetConfigsParams>(parameters);
    if (param == null || string.IsNullOrWhiteSpace(param.Category))
    {
        throw new ArgumentException("Category 参数不能为空");
    }
    // ...
}
```

### 4. UI 线程操作

```csharp
InvokeOnUIThread(() =>
{
    // UI 操作
    MainWindow.Title = "New Title";
});
```

### 5. 异步操作

```csharp
public void SaveConfig(string parameters, IJavascriptCallback callback)
{
    System.Threading.Tasks.Task.Run(() =>
    {
        try
        {
            // 长时间运行的操作
            // ...
            callback.ExecuteAsync(true, "操作成功");
        }
        catch (Exception ex)
        {
            callback.ExecuteAsync(false, ex.Message);
        }
    });
}
```

## 调试命令

获取所有可用的 Controller 和 Action 列表：

```javascript
const commands = JSON.parse(window.nativeHost.getAvailableCommands())
console.log('可用命令:', commands)
// 输出格式：
// [
//   { controller: "System", actions: ["System/GetSystemInfo", "System/ShowMessage"] },
//   { controller: "Window", actions: ["Window/SetTitle", "Window/Close"] },
//   ...
// ]
```

## 路由解析

NativeHost 会自动解析路由格式：
- `ControllerName/ActionName` - 正确格式
- `ControllerName/ActionName/Extra` - 错误（多余部分会被忽略）
- `ControllerName` - 错误（缺少 Action）
- `ControllerName/` - 错误（Action 为空）

## 总结

- ✅ Controller/Action 架构：类似 WebAPI
- ✅ 路由格式：`ControllerName/ActionName`
- ✅ 易于扩展：在 Controller 中添加 Action 方法即可
- ✅ 易于组织：相关功能集中在同一个 Controller
- ✅ 类型安全：通过参数类定义参数
- ✅ 统一格式：所有 Action 返回相同格式的 JSON

