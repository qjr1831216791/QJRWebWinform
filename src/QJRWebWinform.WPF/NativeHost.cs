using System;
using System.Linq;
using System.Windows;
using CefSharp;
using QJRWebWinform.WPF.Controllers;
using Newtonsoft.Json;

namespace QJRWebWinform.WPF
{
    /// <summary>
    /// 暴露给 JavaScript 的 C# 对象
    /// 前端可以通过 window.nativeHost 访问这些方法
    /// 使用 Controller/Action 路由模式，类似 WebAPI
    /// </summary>
    public class NativeHost
    {
        private Window _mainWindow;

        public NativeHost(Window mainWindow)
        {
            _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
            
            // 注册所有 Controller
            RegisterControllers();
        }

        /// <summary>
        /// 注册所有可用的 Controller
        /// </summary>
        private void RegisterControllers()
        {
            ControllerRegistry.RegisterRange(new IController[]
            {
                new SystemController(_mainWindow),
                new WindowController(_mainWindow),
            });
        }

        /// <summary>
        /// 统一命令执行入口（同步）
        /// 前端调用：window.nativeHost.executeCommand('ControllerName/ActionName', { param1: 'value1' })
        /// 示例：window.nativeHost.executeCommand('System/GetSystemInfo')
        /// </summary>
        /// <param name="route">路由，格式：ControllerName/ActionName</param>
        /// <param name="parameters">命令参数（JSON 字符串）</param>
        /// <returns>执行结果（JSON 字符串）</returns>
        public string ExecuteCommand(string route, string parameters = null)
        {
            try
            {
                var (controllerName, actionName) = ParseRoute(route);
                
                var controller = ControllerRegistry.GetController(controllerName);
                if (controller == null)
                {
                    throw new ArgumentException($"Controller '{controllerName}' 不存在");
                }

                var result = controller.ExecuteAction(actionName, parameters);
                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 统一命令执行入口（异步）
        /// 前端调用：window.nativeHost.executeCommandAsync('ControllerName/ActionName', { param1: 'value1' }, callback)
        /// 示例：window.nativeHost.executeCommandAsync('Data/Save', { data: '...' }, callback)
        /// </summary>
        /// <param name="route">路由，格式：ControllerName/ActionName</param>
        /// <param name="parameters">命令参数（JSON 字符串）</param>
        /// <param name="callback">JavaScript 回调函数</param>
        public void ExecuteCommandAsync(string route, string parameters, IJavascriptCallback callback)
        {
            try
            {
                var (controllerName, actionName) = ParseRoute(route);
                
                var controller = ControllerRegistry.GetController(controllerName);
                if (controller == null)
                {
                    callback.ExecuteAsync(false, $"Controller '{controllerName}' 不存在");
                    return;
                }

                controller.ExecuteActionAsync(actionName, parameters, callback);
            }
            catch (Exception ex)
            {
                callback.ExecuteAsync(false, ex.Message);
            }
        }

        /// <summary>
        /// 解析路由：ControllerName/ActionName
        /// </summary>
        private (string controllerName, string actionName) ParseRoute(string route)
        {
            if (string.IsNullOrWhiteSpace(route))
            {
                throw new ArgumentException("路由不能为空");
            }

            var parts = route.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                throw new ArgumentException($"路由格式错误，应为 'ControllerName/ActionName'，实际为: {route}");
            }

            return (parts[0].Trim(), parts[1].Trim());
        }

        /// <summary>
        /// 获取所有可用的 Controller 和 Action 列表（用于调试）
        /// </summary>
        public string GetAvailableCommands()
        {
            var controllers = ControllerRegistry.GetAllControllerNames();
            var result = controllers.Select(controllerName =>
            {
                var controller = ControllerRegistry.GetController(controllerName);
                var actions = controller?.GetAvailableActions() ?? new string[0];
                return new
                {
                    controller = controllerName,
                    actions = actions.Select(action => $"{controllerName}/{action}").ToArray()
                };
            });

            return JsonConvert.SerializeObject(result);
        }
    }
}

