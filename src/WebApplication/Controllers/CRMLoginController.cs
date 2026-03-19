using APIService;
using System;
using System.Linq;
using System.Web.Http;

namespace WebApplication.Controllers
{
    /// <summary>
    /// CRM登录控制器
    /// </summary>
    public class CRMLoginController : BaseController
    {
        /// <summary>
        /// CRM登录
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public virtual bool Login()
        {
            try
            {
                BaseCommand cmd = new BaseCommand();
                var crmEnv = string.Empty;
                if (Request.Headers.Contains("crmenv"))
                {
                    crmEnv = Request.Headers.GetValues("crmenv").FirstOrDefault();
                }
                cmd.Initialize(crmEnv);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
