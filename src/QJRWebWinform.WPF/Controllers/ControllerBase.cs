using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using CefSharp;
using Newtonsoft.Json;

namespace QJRWebWinform.WPF.Controllers
{
    /// <summary>
    /// Controller 基类，提供通用功能
    /// </summary>
    public abstract class ControllerBase : IController
    {
        /// <summary>
        /// Controller 名称（子类必须实现）
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// 主窗口引用（子类可以使用）
        /// </summary>
        protected Window MainWindow { get; private set; }

        /// <summary>
        /// Action 方法缓存（方法名 -> MethodInfo）
        /// </summary>
        private Dictionary<string, MethodInfo> _actionMethods;
        private Dictionary<string, MethodInfo> _asyncActionMethods;
        private readonly object _lock = new object();

        protected ControllerBase(Window mainWindow)
        {
            MainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
            InitializeActionMethods();
        }

        /// <summary>
        /// 初始化 Action 方法缓存
        /// </summary>
        private void InitializeActionMethods()
        {
            lock (_lock)
            {
                _actionMethods = new Dictionary<string, MethodInfo>(StringComparer.OrdinalIgnoreCase);
                _asyncActionMethods = new Dictionary<string, MethodInfo>(StringComparer.OrdinalIgnoreCase);

                var type = this.GetType();
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                foreach (var method in methods)
                {
                    // 跳过基类方法
                    if (method.DeclaringType == typeof(ControllerBase) || 
                        method.DeclaringType == typeof(object))
                        continue;

                    var parameters = method.GetParameters();
                    
                    // 同步 Action：返回 object，参数为 (string parameters)
                    if (method.ReturnType == typeof(object) && 
                        parameters.Length == 1 && 
                        parameters[0].ParameterType == typeof(string))
                    {
                        _actionMethods[method.Name] = method;
                    }
                    // 异步 Action：返回 void，参数为 (string parameters, IJavascriptCallback callback)
                    else if (method.ReturnType == typeof(void) && 
                             parameters.Length == 2 && 
                             parameters[0].ParameterType == typeof(string) &&
                             parameters[1].ParameterType == typeof(IJavascriptCallback))
                    {
                        _asyncActionMethods[method.Name] = method;
                    }
                }
            }
        }

        /// <summary>
        /// 执行 Action（同步）
        /// </summary>
        public object ExecuteAction(string actionName, string parameters)
        {
            if (string.IsNullOrWhiteSpace(actionName))
                throw new ArgumentException("Action 名称不能为空", nameof(actionName));

            lock (_lock)
            {
                if (!_actionMethods.TryGetValue(actionName, out var method))
                {
                    throw new ArgumentException($"Action '{actionName}' 在 Controller '{Name}' 中不存在");
                }

                try
                {
                    return method.Invoke(this, new object[] { parameters });
                }
                catch (TargetInvocationException ex)
                {
                    throw ex.InnerException ?? ex;
                }
            }
        }

        /// <summary>
        /// 执行 Action（异步）
        /// </summary>
        public void ExecuteActionAsync(string actionName, string parameters, IJavascriptCallback callback)
        {
            if (string.IsNullOrWhiteSpace(actionName))
            {
                callback.ExecuteAsync(false, "Action 名称不能为空");
                return;
            }

            lock (_lock)
            {
                if (!_asyncActionMethods.TryGetValue(actionName, out var method))
                {
                    callback.ExecuteAsync(false, $"Action '{actionName}' 在 Controller '{Name}' 中不存在或不是异步方法");
                    return;
                }

                try
                {
                    method.Invoke(this, new object[] { parameters, callback });
                }
                catch (TargetInvocationException ex)
                {
                    callback.ExecuteAsync(false, (ex.InnerException ?? ex).Message);
                }
                catch (Exception ex)
                {
                    callback.ExecuteAsync(false, ex.Message);
                }
            }
        }

        /// <summary>
        /// 获取所有可用的 Action 名称
        /// </summary>
        public string[] GetAvailableActions()
        {
            lock (_lock)
            {
                var actions = new List<string>();
                actions.AddRange(_actionMethods.Keys);
                actions.AddRange(_asyncActionMethods.Keys);
                return actions.Distinct().ToArray();
            }
        }

        /// <summary>
        /// 反序列化参数（辅助方法）
        /// </summary>
        protected T DeserializeParameters<T>(string parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters))
            {
                return default(T);
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(parameters);
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// 在 UI 线程上执行操作（辅助方法）
        /// </summary>
        protected void InvokeOnUIThread(Action action)
        {
            Application.Current.Dispatcher.Invoke(action);
        }
    }
}

