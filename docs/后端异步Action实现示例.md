# 后端异步 Action 实现示例

## 概述

本文档说明如何在现有 Controller 中添加异步版本的 Action，以解决 UI 卡顿问题。

## 方案说明

- **方案A**：保持现有同步 Action 不变，按需添加异步版本
- **优点**：向后兼容，可以逐步迁移，不影响现有功能
- **缺点**：需要维护两套代码（但可以共享业务逻辑）

## 实现方式

### 1. 基本结构

Controller 可以同时提供同步和异步两个版本的 Action：

```csharp
// 同步版本（现有，保持不变）
public object ActionName(string parameters)
{
    // 业务逻辑
}

// 异步版本（新增）
public void ActionName(string parameters, IJavascriptCallback callback)
{
    // 异步执行业务逻辑
}
```

### 2. 完整示例：DefaultController.GetCRMEnvironments

#### 现有同步版本（保持不变）

```csharp
/// <summary>
/// 获取网站配置的CRM环境信息（同步版本）
/// </summary>
/// <param name="parameters">JSON参数：null 或 "{}"</param>
/// <returns></returns>
public virtual object GetCRMEnvironments(string parameters)
{
    SetParameters(parameters);
    return Command<DefaultCommand>().GetCRMEnvironments();
}
```

#### 新增异步版本

```csharp
/// <summary>
/// 获取网站配置的CRM环境信息（异步版本）
/// </summary>
/// <param name="parameters">JSON参数：null 或 "{}"</param>
/// <param name="callback">JavaScript 回调函数</param>
public virtual void GetCRMEnvironments(string parameters, IJavascriptCallback callback)
{
    // 使用 Task.Run 在后台线程执行，避免阻塞 UI 线程
    System.Threading.Tasks.Task.Run(() =>
    {
        try
        {
            SetParameters(parameters);
            var result = Command<DefaultCommand>().GetCRMEnvironments();
            
            // 将 ResultModel 序列化为 JSON 字符串
            // 注意：异步 API 需要返回 JSON 字符串，前端会解析
            var resultJson = JsonConvert.SerializeObject(result);
            
            // 成功时调用回调：callback.ExecuteAsync(true, resultJson)
            callback.ExecuteAsync(true, resultJson);
        }
        catch (Exception ex)
        {
            // 失败时调用回调：callback.ExecuteAsync(false, errorMessage)
            callback.ExecuteAsync(false, ex.Message);
        }
    });
}
```

### 3. 更复杂的示例：带参数解析的异步 Action

```csharp
/// <summary>
/// 测试API Post入参（异步版本）
/// </summary>
/// <param name="parameters">JSON参数：{"input": "string"}</param>
/// <param name="callback">JavaScript 回调函数</param>
public virtual void TestAPIPost(string parameters, IJavascriptCallback callback)
{
    Task.Run(() =>
    {
        try
        {
            // 参数解析（与同步版本相同）
            var input = DeserializeParameters<TestAPIPostInput>(parameters);
            SetParameters(parameters);
            
            // 业务逻辑（与同步版本相同）
            ResultModel result = new ResultModel();
            result.Success(message: $"Input is：{input?.input}");
            
            // 序列化并返回
            var resultJson = JsonConvert.SerializeObject(result);
            callback.ExecuteAsync(true, resultJson);
        }
        catch (Exception ex)
        {
            callback.ExecuteAsync(false, ex.Message);
        }
    });
}
```

### 4. 共享业务逻辑的最佳实践

如果业务逻辑复杂，可以提取为私有方法，同步和异步版本都调用它：

```csharp
/// <summary>
/// 核心业务逻辑（私有方法，供同步和异步版本共享）
/// </summary>
private ResultModel GetCRMEnvironmentsCore(string parameters)
{
    SetParameters(parameters);
    return Command<DefaultCommand>().GetCRMEnvironments();
}

/// <summary>
/// 同步版本
/// </summary>
public virtual object GetCRMEnvironments(string parameters)
{
    return GetCRMEnvironmentsCore(parameters);
}

/// <summary>
/// 异步版本
/// </summary>
public virtual void GetCRMEnvironments(string parameters, IJavascriptCallback callback)
{
    Task.Run(() =>
    {
        try
        {
            var result = GetCRMEnvironmentsCore(parameters);
            var resultJson = JsonConvert.SerializeObject(result);
            callback.ExecuteAsync(true, resultJson);
        }
        catch (Exception ex)
        {
            callback.ExecuteAsync(false, ex.Message);
        }
    });
}
```

## 注意事项

### 1. 返回格式

- **同步版本**：直接返回 `ResultModel` 对象，系统会自动序列化
- **异步版本**：需要手动序列化为 JSON 字符串，通过 `callback.ExecuteAsync(true, jsonString)` 返回

### 2. 错误处理

- **同步版本**：抛出异常，系统会捕获并返回错误格式
- **异步版本**：必须使用 try-catch 捕获异常，通过 `callback.ExecuteAsync(false, errorMessage)` 返回错误

### 3. UI 线程操作

如果需要在 WPF UI 线程上执行操作（如显示消息框），需要在异步方法中使用 `Dispatcher.Invoke`：

```csharp
public virtual void ShowMessage(string parameters, IJavascriptCallback callback)
{
    Task.Run(() =>
    {
        try
        {
            var input = DeserializeParameters<ShowMessageInput>(parameters);
            
            // 如果需要在 UI 线程执行
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(input.message, input.title ?? "提示");
            });
            
            var result = new ResultModel();
            result.Success("消息已显示");
            callback.ExecuteAsync(true, JsonConvert.SerializeObject(result));
        }
        catch (Exception ex)
        {
            callback.ExecuteAsync(false, ex.Message);
        }
    });
}
```

### 4. 性能考虑

- **快速操作**（< 50ms）：可以保持同步版本，使用前端 setTimeout 包装即可
- **耗时操作**（> 50ms）：建议添加异步版本，避免 UI 卡顿
- **数据库查询、网络请求**：强烈建议使用异步版本

## 迁移策略

### 阶段1：识别需要异步的 Action
- 分析现有 Action 的执行时间
- 识别耗时操作（数据库查询、文件操作、网络请求等）

### 阶段2：逐步添加异步版本
- 优先为耗时操作添加异步版本
- 保持同步版本不变，确保向后兼容

### 阶段3：前端自动选择
- 前端会自动优先使用异步版本（如果存在）
- 如果不存在异步版本，自动降级到同步版本

## 测试建议

1. **功能测试**：确保异步版本返回结果与同步版本一致
2. **性能测试**：验证异步版本不会阻塞 UI
3. **错误处理测试**：确保异常情况能正确返回错误信息
4. **兼容性测试**：确保同步版本仍然正常工作

## 总结

- ✅ 保持现有同步 Action 不变
- ✅ 按需添加异步版本
- ✅ 共享业务逻辑代码
- ✅ 前端自动选择最佳方案
- ✅ 向后兼容，逐步迁移

