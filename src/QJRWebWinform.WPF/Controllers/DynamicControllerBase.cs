using APIService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Windows;

namespace QJRWebWinform.WPF.Controllers
{
    /// <summary>
    /// WPF Controller 基类，提供与 WebApplication BaseController 类似的功能
    /// 支持从参数中提取 crmEnv，与 WebAPI 调用方式保持一致
    /// </summary>
    public abstract class DynamicControllerBase : ControllerBase
    {
        /// <summary>
        /// CRM 环境标识
        /// 可以从参数 JSON 中提取，也可以通过 SetCrmEnv 方法设置
        /// </summary>
        protected string crmEnv { get; set; } = string.Empty;

        /// <summary>
        /// 当前请求的参数（用于提取 crmEnv）
        /// </summary>
        private string _currentParameters;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="mainWindow"></param>
        protected DynamicControllerBase(Window mainWindow) : base(mainWindow)
        {
        }

        /// <summary>
        /// 设置当前请求的参数（用于提取 crmEnv）
        /// 在调用 Command 之前，应该先调用此方法设置参数
        /// </summary>
        /// <param name="parameters">JSON 参数字符串</param>
        protected void SetParameters(string parameters)
        {
            _currentParameters = parameters;
            ExtractCrmEnvFromParameters(parameters);
        }

        /// <summary>
        /// 手动设置 crmEnv（优先级高于从参数中提取）
        /// </summary>
        /// <param name="env">CRM 环境标识</param>
        protected void SetCrmEnv(string env)
        {
            crmEnv = env ?? string.Empty;
        }

        /// <summary>
        /// 从参数 JSON 中提取 crmEnv
        /// 支持以下字段名（不区分大小写，按优先级排序）：
        /// 1. crmEnv, CrmEnv, crmenv - 标准字段名
        /// </summary>
        /// <param name="parameters">JSON 参数字符串</param>
        private void ExtractCrmEnvFromParameters(string parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters))
            {
                return;
            }

            try
            {
                // 尝试解析 JSON
                var jsonObj = JObject.Parse(parameters);

                // 按优先级查找 crmEnv 字段（不区分大小写）
                string[] possibleKeys = { "CrmEnv", "crmenv" };

                foreach (var key in possibleKeys)
                {
                    var token = jsonObj.GetValue(key, StringComparison.OrdinalIgnoreCase);
                    if (token != null && token.Type == JTokenType.String)
                    {
                        var value = token.Value<string>();
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            crmEnv = value;
                            return;
                        }
                    }
                }
            }
            catch (JsonException)
            {
                // 如果不是有效的 JSON，忽略错误
            }
            catch (Exception)
            {
                // 忽略其他异常
            }
        }

        /// <summary>
        /// 获取 Command 对象（与 WebApplication BaseController 保持一致）
        /// </summary>
        /// <typeparam name="T">Command 类型</typeparam>
        /// <returns>初始化后的 Command 对象</returns>
        public virtual T Command<T>() where T : BaseCommand, new()
        {
            // 如果还没有从参数中提取 crmEnv，尝试从当前参数中提取
            if (string.IsNullOrWhiteSpace(crmEnv) && !string.IsNullOrWhiteSpace(_currentParameters))
            {
                ExtractCrmEnvFromParameters(_currentParameters);
            }

            T val = new T();
            val.Initialize(crmEnv);
            return val;
        }

        /// <summary>
        /// 获取 Command 对象（带参数，自动提取 crmEnv）
        /// 推荐使用此方法，可以自动从参数中提取 crmEnv
        /// </summary>
        /// <typeparam name="T">Command 类型</typeparam>
        /// <param name="parameters">JSON 参数字符串</param>
        /// <returns>初始化后的 Command 对象</returns>
        public virtual T Command<T>(string parameters) where T : BaseCommand, new()
        {
            SetParameters(parameters);
            return Command<T>();
        }
    }
}
