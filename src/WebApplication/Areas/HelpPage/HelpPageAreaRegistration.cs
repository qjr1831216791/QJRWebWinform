using System.Web.Http;
using System.Web.Mvc;

namespace WebApplication.Areas.HelpPage
{
    /// <summary>
    /// 帮助页面区域注册类，用于注册HelpPage区域的路由
    /// </summary>
    public class HelpPageAreaRegistration : AreaRegistration
    {
        /// <summary>
        /// 获取区域名称
        /// </summary>
        public override string AreaName
        {
            get
            {
                return "HelpPage";
            }
        }

        /// <summary>
        /// 注册帮助页面区域的路由和配置
        /// </summary>
        /// <param name="context">区域注册上下文</param>
        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "HelpPage_Default",
                "Help/{action}/{apiId}",
                new { controller = "Help", action = "Index", apiId = UrlParameter.Optional });

            HelpPageConfig.Register(GlobalConfiguration.Configuration);
        }
    }
}