using System;
using System.Text.RegularExpressions;
using System.Web;

namespace WebApplication.UrlRoutingModule
{
    /// <summary>
    /// Vue静态资源路由模块
    /// </summary>
    public class VueStaticRoutingModule : IHttpModule
    {
        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(Context_BeginRequest);
        }

        private void Context_BeginRequest(object sender, EventArgs e)
        {
            HttpApplication application = (HttpApplication)sender;
            HttpContext context = application.Context;

            string originalPath = context.Request.Path;

            // 支持两种路径模式：
            // 1. 旧版模式：static/js/..., static/css/..., static/fonts/..., static/img/...
            // 2. 新版模式（Vue CLI 5）：js/..., css/..., fonts/..., img/...
            if (Regex.IsMatch(originalPath, @"static/(?:js|css|fonts|img)"))
            {
                // 旧版模式：static/js/... -> /dist/static/js/...
                context.RewritePath($"/dist/{originalPath}");
            }
            else if (Regex.IsMatch(originalPath, @"^/(?:js|css|fonts|img)/"))
            {
                // 新版模式：/js/... -> /dist/js/...
                context.RewritePath($"/dist{originalPath}");
            }
        }
    }
}