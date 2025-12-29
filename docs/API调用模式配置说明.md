# API 调用模式配置说明

## 概述

本项目支持两种 API 调用模式，可以根据不同的开发场景自动切换：

1. **WebAPI 模式**：通过 HTTP 请求调用后端 WebAPI（用于单独前端调试）
2. **NativeHost 模式**：通过 CefSharp 的 JavaScript Binding 直接调用 WPF 后端方法（用于 VS 调试和生产环境）

## 配置方式

在 `frontend/public/config/WebConfig.js` 中配置：

```javascript
var WebConfig = {
    VUE_APP_DEV_API_URL: "http://localhost:8098/", // WebAPI 基础 URL
    
    // API 调用模式配置
    // 'webapi': 通过 WebAPI 调用（用于单独前端调试）
    // 'nativehost': 通过 NativeHost 调用 WPF API（用于 VS 调试和生产环境）
    // 'auto': 自动检测，如果 NativeHost 可用则使用，否则使用 WebAPI
    API_MODE: 'auto', // 'webapi' | 'nativehost' | 'auto'
    
    // 是否强制使用 NativeHost（生产环境必须为 true）
    FORCE_NATIVE_HOST: false, // 生产环境应设置为 true
};
```

## 使用场景

### 1. 单独前端调试（浏览器模式）

**场景**：只运行前端开发服务器，通过浏览器访问 `http://localhost:8080`

**配置**：
```javascript
API_MODE: 'webapi',  // 或 'auto'
FORCE_NATIVE_HOST: false,
VUE_APP_DEV_API_URL: "http://localhost:8098/", // 确保后端 WebAPI 服务运行在此地址
```

**说明**：
- 使用 WebAPI 模式调用后端
- 需要确保后端 WebAPI 服务正在运行
- 适合前端独立开发和调试

### 2. VS 调试模式（WPF 应用）

**场景**：在 Visual Studio 中运行 WPF 应用（Debug 模式）

**配置**：
```javascript
API_MODE: 'auto',  // 或 'nativehost'
FORCE_NATIVE_HOST: false,
```

**说明**：
- 当 `API_MODE` 为 `'auto'` 时，系统会自动检测 NativeHost 是否可用
- 如果 NativeHost 可用（在 WPF 应用中），则使用 NativeHost 模式
- 如果 NativeHost 不可用（浏览器中），则自动降级到 WebAPI 模式
- 适合在 WPF 应用中进行集成调试

### 3. 生产环境

**场景**：发布后的生产版本

**配置**：
```javascript
API_MODE: 'nativehost',
FORCE_NATIVE_HOST: true,  // 必须设置为 true
```

**说明**：
- 强制使用 NativeHost 模式
- 如果 NativeHost 不可用，会显示警告但仍尝试使用 WebAPI（不推荐）
- 生产环境必须确保 NativeHost 可用

## 配置模式详解

### API_MODE: 'webapi'

- **行为**：始终使用 WebAPI 调用
- **适用场景**：单独前端调试
- **要求**：后端 WebAPI 服务必须运行

### API_MODE: 'nativehost'

- **行为**：尝试使用 NativeHost 调用
- **适用场景**：WPF 应用内运行
- **降级**：如果 NativeHost 不可用，会自动降级到 WebAPI（除非 `FORCE_NATIVE_HOST: true`）

### API_MODE: 'auto'（推荐）

- **行为**：自动检测并选择最佳模式
  - 如果 NativeHost 可用 → 使用 NativeHost
  - 如果 NativeHost 不可用 → 使用 WebAPI
- **适用场景**：开发阶段，需要在浏览器和 WPF 应用之间切换
- **优势**：无需手动切换配置

### FORCE_NATIVE_HOST: true

- **行为**：强制使用 NativeHost，不允许降级
- **适用场景**：生产环境
- **注意**：如果 NativeHost 不可用，会抛出错误

## 调用流程

### WebAPI 模式调用流程

```
前端代码
  ↓
invokeHiddenApi()
  ↓
ApiPost() → HTTP POST 请求
  ↓
后端 WebAPI Controller
  ↓
返回 JSON 响应
```

### NativeHost 模式调用流程

```
前端代码
  ↓
invokeHiddenApi()
  ↓
window.nativeHost.executeCommand(route, parameters)
  ↓
C# NativeHost.ExecuteCommand()
  ↓
ControllerRegistry → Controller → Action
  ↓
返回 JSON 字符串
```

## 代码示例

### 前端调用示例

```javascript
// 调用方式与之前完全相同，无需修改业务代码
jshelper.invokeHiddenApiAsync('new_hbxn_common', 'RetrieveEntityMetadata/GetAllAttributeMetadataFromEntity', input)
    .then(result => {
        if (result.isSuccess) {
            console.log('调用成功:', result.data);
        }
    })
    .catch(error => {
        console.error('调用失败:', error);
    });
```

### 后端 Controller 示例

```csharp
public class RetrieveEntityMetadataController : ControllerBase
{
    public override string Name => "RetrieveEntityMetadata";

    // 同步方法
    public object GetAllAttributeMetadataFromEntity(string parameters)
    {
        var param = DeserializeParameters<GetMetadataParams>(parameters);
        // ... 处理逻辑
        return new { /* 返回数据 */ };
    }

    // 异步方法
    public void GetAllAttributeMetadataFromEntityAsync(string parameters, IJavascriptCallback callback)
    {
        try
        {
            var param = DeserializeParameters<GetMetadataParams>(parameters);
            // ... 处理逻辑
            var result = new { /* 返回数据 */ };
            callback.ExecuteAsync(true, JsonConvert.SerializeObject(result));
        }
        catch (Exception ex)
        {
            callback.ExecuteAsync(false, ex.Message);
        }
    }
}
```

## 注意事项

1. **路由格式**：`apiName` 参数必须是 `ControllerName/ActionName` 格式
   - Controller 名称不需要包含 "Controller" 后缀
   - 例如：`RetrieveEntityMetadata/GetAllAttributeMetadataFromEntity`

2. **参数格式**：
   - 可以是对象：`{ key: 'value' }`
   - 可以是 JSON 字符串：`'{"key":"value"}'`
   - 系统会自动处理

3. **返回格式**：
   - WebAPI 和 NativeHost 模式都会返回统一格式：
   ```javascript
   {
       isSuccess: true,
       data: { /* 实际数据 */ },
       message: '调用成功'
   }
   ```

4. **错误处理**：
   - 两种模式都使用相同的错误处理机制
   - 错误会通过 Promise 的 `catch` 或 `throw` 抛出

5. **降级机制**：
   - 当 `API_MODE: 'auto'` 或 `'nativehost'` 时，如果 NativeHost 不可用，会自动降级到 WebAPI
   - 当 `FORCE_NATIVE_HOST: true` 时，不允许降级，会抛出错误

## 调试技巧

### 检查当前使用的模式

在浏览器控制台或 Vue DevTools 中：

```javascript
// 检查 NativeHost 是否可用
console.log('NativeHost 可用:', typeof window.nativeHost !== 'undefined' && typeof window.nativeHost.executeCommand === 'function');

// 检查当前配置
console.log('API 模式:', window.__WebConfig__?.API_MODE);
console.log('强制 NativeHost:', window.__WebConfig__?.FORCE_NATIVE_HOST);
```

### 手动切换模式（开发时）

在浏览器控制台中临时修改：

```javascript
// 临时切换到 WebAPI 模式
window.__VUE_CONTEXT__.envconfig.API_MODE = 'webapi';

// 临时切换到 NativeHost 模式
window.__VUE_CONTEXT__.envconfig.API_MODE = 'nativehost';
```

## 常见问题

### Q: 为什么在浏览器中调用失败？

A: 检查以下几点：
1. 如果使用 WebAPI 模式，确保后端 WebAPI 服务正在运行
2. 如果使用 NativeHost 模式，浏览器中 NativeHost 不可用，会自动降级到 WebAPI
3. 检查 `VUE_APP_DEV_API_URL` 配置是否正确

### Q: 为什么在 WPF 应用中调用失败？

A: 检查以下几点：
1. 确保 NativeHost 已正确注册（检查控制台是否有 NativeHost 相关错误）
2. 检查 Controller 和 Action 名称是否正确
3. 检查参数格式是否正确

### Q: 生产环境应该使用什么配置？

A: 生产环境必须：
```javascript
API_MODE: 'nativehost',
FORCE_NATIVE_HOST: true,
```

### Q: 如何添加新的 Controller？

A: 参考现有的 Controller 实现：
1. 创建继承自 `ControllerBase` 的类
2. 实现 `Name` 属性
3. 实现 Action 方法（同步或异步）
4. 在 `NativeHost.cs` 的 `RegisterControllers()` 方法中注册

## 相关文件

- `frontend/public/config/WebConfig.js` - 配置文件
- `frontend/public/js/JsCrmHelper.js` - 调用实现
- `src/QJRWebWinform.WPF/NativeHost.cs` - NativeHost 桥接
- `src/QJRWebWinform.WPF/Controllers/` - Controller 实现

