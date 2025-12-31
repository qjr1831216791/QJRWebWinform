using APIService.RetrieveEntityMetadata;
using APIService.RetrieveUserAndRoles;
using CommonHelper.Model;
using CefSharp;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace QJRWebWinform.WPF.Controllers
{
    /// <summary>
    /// 查询用户和安全角色
    /// </summary>
    public class RetrieveUserAndRolesController : DynamicControllerBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="mainWindow"></param>
        public RetrieveUserAndRolesController(Window mainWindow) : base(mainWindow)
        {
        }

        public override string Name => "RetrieveUserAndRoles";

        /// <summary>
        /// 查询用户和安全角色（核心业务逻辑）
        /// </summary>
        /// <param name="parameters">JSON参数：GetUserAndRolesInput 对象</param>
        /// <returns></returns>
        private ResultModel GetUserAndRolesCore(string parameters)
        {
            var input = DeserializeParameters<GetUserAndRolesInput>(parameters);
            SetParameters(parameters); // 设置参数以提取 crmEnv
            return Command<RetrieveUserAndRolesCommand>().GetUserAndRoles(input?.envir, input?.customFields);
        }

        /// <summary>
        /// 查询用户和安全角色（同步版本）
        /// </summary>
        /// <param name="parameters">JSON参数：GetUserAndRolesInput 对象</param>
        /// <returns></returns>
        public virtual object GetUserAndRoles(string parameters)
        {
            return GetUserAndRolesCore(parameters);
        }

        /// <summary>
        /// 查询用户和安全角色（异步版本）
        /// </summary>
        /// <param name="parameters">JSON参数：GetUserAndRolesInput 对象</param>
        /// <param name="callback">JavaScript 回调函数</param>
        public virtual void GetUserAndRoles(string parameters, IJavascriptCallback callback)
        {
            Task.Run(() =>
            {
                try
                {
                    var result = GetUserAndRolesCore(parameters);
                    var resultJson = JsonConvert.SerializeObject(result);
                    callback.ExecuteAsync(true, resultJson);
                }
                catch (Exception ex)
                {
                    callback.ExecuteAsync(false, ex.Message);
                }
            });
        }
    }
}

