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
        /// <summary>
        /// 释放资源，实现IDisposable接口
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// 初始化HTTP模块，注册请求开始事件处理程序
        /// </summary>
        /// <param name="context">HTTP应用程序上下文</param>
        public void Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(Context_BeginRequest);
        }

        /// <summary>
        /// 处理请求开始事件，重写静态资源路径以支持Vue构建的静态文件
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        /// <remarks>
        /// 支持两种路径模式：
        /// 1. 旧版模式：static/js/..., static/css/..., static/fonts/..., static/img/... -> /dist/static/...
        /// 2. 新版模式（Vue CLI 5）：/js/..., /css/..., /fonts/..., /img/... -> /dist/...
        /// </remarks>
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