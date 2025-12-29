using System;
using System.Reflection;

namespace QJRWebWinform.WPF.Controllers
{
    /// <summary>
    /// Controller 接口，所有 Controller 必须实现此接口
    /// </summary>
    public interface IController
    {
        /// <summary>
        /// Controller 名称（用于路由）
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 执行 Action（同步）
        /// </summary>
        /// <param name="actionName">Action 名称</param>
        /// <param name="parameters">参数（JSON 字符串）</param>
        /// <returns>执行结果</returns>
        object ExecuteAction(string actionName, string parameters);

        /// <summary>
        /// 执行 Action（异步）
        /// </summary>
        /// <param name="actionName">Action 名称</param>
        /// <param name="parameters">参数（JSON 字符串）</param>
        /// <param name="callback">JavaScript 回调函数</param>
        void ExecuteActionAsync(string actionName, string parameters, CefSharp.IJavascriptCallback callback);

        /// <summary>
        /// 获取所有可用的 Action 名称
        /// </summary>
        string[] GetAvailableActions();
    }
}

