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
        /// 查询用户和安全角色
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual ResultModel GetUserAndRoles([FromBody] GetUserAndRolesInput input)
        {
            return Command<RetrieveUserAndRolesCommand>().GetUserAndRoles(input.envir, input.customFields);
        }
    }
}
