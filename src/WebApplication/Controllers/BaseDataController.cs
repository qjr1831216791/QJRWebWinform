using APIService.BaseData;
using CommonHelper.Model;
using System.Web.Http;

namespace WebApplication.Controllers
{
    /// <summary>
    /// 基础数据控制器
    /// </summary>
    public class BaseDataController : ApiController
    {
        /// <summary>
        /// 获取网站配置的CRM环境信息
        /// </summary>
        /// <returns>包含所有配置的CRM环境信息的结果模型</returns>
        [HttpGet]
        public virtual ResultModel GetCRMEnvironments()
        {
            return new BaseDataCommand().GetCRMEnvironments();
        }
    }
}
