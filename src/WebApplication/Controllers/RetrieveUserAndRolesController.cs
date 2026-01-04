using APIService.RetrieveEntityMetadata;
using APIService.RetrieveUserAndRoles;
using CommonHelper.Model;
using System.Web.Http;

namespace WebApplication.Controllers
{
    /// <summary>
    /// 查询用户和安全角色
    /// </summary>
    public class RetrieveUserAndRolesController : BaseController
    {
        /// <summary>
        /// 查询用户和安全角色信息
        /// </summary>
        /// <param name="input">查询输入模型，包含环境标识和自定义字段列表</param>
        /// <returns>包含用户和安全角色信息的结果模型</returns>
        [HttpPost]
        public virtual ResultModel GetUserAndRoles([FromBody] GetUserAndRolesInput input)
        {
            return Command<RetrieveUserAndRolesCommand>().GetUserAndRoles(input.envir, input.customFields);
        }
    }
}
