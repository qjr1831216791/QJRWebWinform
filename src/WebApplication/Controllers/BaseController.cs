using APIService;
using System.Linq;
using System.Web.Http;

namespace WebApplication.Controllers
{
    /// <summary>
    /// Base controller for all controllers in the application.
    /// </summary>
    public class BaseController : ApiController
    {
        /// <summary>
        /// 获取命令对象实例，根据请求头中的crmenv参数初始化相应的CRM环境
        /// </summary>
        /// <typeparam name="T">命令类型，必须继承自BaseCommand并具有无参构造函数</typeparam>
        /// <returns>初始化后的命令对象实例</returns>
        public virtual T Command<T>() where T : BaseCommand, new()
        {
            var crmEnv = string.Empty;
            if (Request.Headers.Contains("crmenv"))
            {
                crmEnv = Request.Headers.GetValues("crmenv").FirstOrDefault();
            }
            T val = new T();
            val.Initialize(crmEnv);
            return val;
        }
    }
}