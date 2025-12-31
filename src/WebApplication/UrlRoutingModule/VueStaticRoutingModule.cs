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

            // 这里可以添加自定义的路由处理逻辑
            // 例如，可以根据URL来决定是否重写URL或者转发请求到其他的处理程序

            string originalPath = context.Request.Path;
            if (Regex.IsMatch(originalPath, @"static/(?:js|css|fonts|img)"))
            {
                context.RewritePath($"/dist/{originalPath}");
            }
        }
    }
}