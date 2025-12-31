using APIService.RetrieveEntityMetadata;
using APIService.RetrieveUserAndRoles;
using CommonHelper.Model;
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
        /// 查询用户和安全角色
        /// </summary>
        /// <param name="parameters">JSON参数：GetUserAndRolesInput 对象</param>
        /// <returns></returns>
        public virtual object GetUserAndRoles(string parameters)
        {
            var input = DeserializeParameters<GetUserAndRolesInput>(parameters);
            SetParameters(parameters); // 设置参数以提取 crmEnv
            return Command<RetrieveUserAndRolesCommand>().GetUserAndRoles(input?.envir, input?.customFields);
        }
    }
}

