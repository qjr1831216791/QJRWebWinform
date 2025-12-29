using System;
using System.Collections.Generic;
using System.Linq;

namespace QJRWebWinform.WPF.Controllers
{
    /// <summary>
    /// Controller 注册表，管理所有可用的 Controller
    /// </summary>
    public class ControllerRegistry
    {
        private static readonly Dictionary<string, IController> _controllers = new Dictionary<string, IController>(StringComparer.OrdinalIgnoreCase);
        private static readonly object _lock = new object();

        /// <summary>
        /// 注册 Controller
        /// </summary>
        public static void Register(IController controller)
        {
            if (controller == null)
                throw new ArgumentNullException(nameof(controller));

            if (string.IsNullOrWhiteSpace(controller.Name))
                throw new ArgumentException("Controller 名称不能为空", nameof(controller));

            lock (_lock)
            {
                _controllers[controller.Name] = controller;
            }
        }

        /// <summary>
        /// 注册多个 Controller
        /// </summary>
        public static void RegisterRange(IEnumerable<IController> controllers)
        {
            if (controllers == null)
                throw new ArgumentNullException(nameof(controllers));

            foreach (var controller in controllers)
            {
                Register(controller);
            }
        }

        /// <summary>
        /// 获取 Controller
        /// </summary>
        public static IController GetController(string controllerName)
        {
            if (string.IsNullOrWhiteSpace(controllerName))
                return null;

            lock (_lock)
            {
                _controllers.TryGetValue(controllerName, out var controller);
                return controller;
            }
        }

        /// <summary>
        /// 检查 Controller 是否存在
        /// </summary>
        public static bool HasController(string controllerName)
        {
            if (string.IsNullOrWhiteSpace(controllerName))
                return false;

            lock (_lock)
            {
                return _controllers.ContainsKey(controllerName);
            }
        }

        /// <summary>
        /// 获取所有已注册的 Controller 名称
        /// </summary>
        public static IEnumerable<string> GetAllControllerNames()
        {
            lock (_lock)
            {
                return _controllers.Keys.ToList();
            }
        }

        /// <summary>
        /// 移除 Controller
        /// </summary>
        public static bool Unregister(string controllerName)
        {
            if (string.IsNullOrWhiteSpace(controllerName))
                return false;

            lock (_lock)
            {
                return _controllers.Remove(controllerName);
            }
        }

        /// <summary>
        /// 清空所有 Controller
        /// </summary>
        public static void Clear()
        {
            lock (_lock)
            {
                _controllers.Clear();
            }
        }
    }
}

