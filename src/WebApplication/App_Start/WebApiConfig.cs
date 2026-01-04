using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Mvc;

namespace WebApplication
{
    /// <summary>
    /// Web API配置类，用于配置Web API的路由和跨域设置
    /// </summary>
    public static class WebApiConfig
    {
        /// <summary>
        /// 注册Web API配置，包括跨域支持和路由设置
        /// </summary>
        /// <param name="config">HTTP配置对象</param>
        public static void Register(HttpConfiguration config)
        {
            // Web API 配置和服务
            // 配置跨域请求
            config.EnableCors(new EnableCorsAttribute("*", "*", "*"));

            // Web API 路由
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
