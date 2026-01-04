using System;
using System.Web.Http;
using System.Web.Mvc;
using WebApplication.Areas.HelpPage.ModelDescriptions;
using WebApplication.Areas.HelpPage.Models;

namespace WebApplication.Areas.HelpPage.Controllers
{
    /// <summary>
    /// The controller that will handle requests for the help page.
    /// </summary>
    public class HelpController : Controller
    {
        /// <summary>
        /// 错误视图名称常量
        /// </summary>
        private const string ErrorViewName = "Error";

        /// <summary>
        /// 构造函数，使用全局配置初始化
        /// </summary>
        public HelpController()
            : this(GlobalConfiguration.Configuration)
        {
        }

        /// <summary>
        /// 构造函数，使用指定的HTTP配置初始化
        /// </summary>
        /// <param name="config">HTTP配置对象</param>
        public HelpController(HttpConfiguration config)
        {
            Configuration = config;
        }

        /// <summary>
        /// 获取或设置HTTP配置对象
        /// </summary>
        public HttpConfiguration Configuration { get; private set; }

        /// <summary>
        /// 显示帮助页面首页，列出所有API
        /// </summary>
        /// <returns>包含所有API描述的视图</returns>
        public ActionResult Index()
        {
            ViewBag.DocumentationProvider = Configuration.Services.GetDocumentationProvider();
            return View(Configuration.Services.GetApiExplorer().ApiDescriptions);
        }

        /// <summary>
        /// 显示指定API的详细帮助信息
        /// </summary>
        /// <param name="apiId">API的唯一标识符</param>
        /// <returns>包含API详细信息的视图，如果API不存在则返回错误视图</returns>
        public ActionResult Api(string apiId)
        {
            if (!String.IsNullOrEmpty(apiId))
            {
                HelpPageApiModel apiModel = Configuration.GetHelpPageApiModel(apiId);
                if (apiModel != null)
                {
                    return View(apiModel);
                }
            }

            return View(ErrorViewName);
        }

        /// <summary>
        /// 显示指定模型的资源描述信息
        /// </summary>
        /// <param name="modelName">模型名称</param>
        /// <returns>包含模型描述的视图，如果模型不存在则返回错误视图</returns>
        public ActionResult ResourceModel(string modelName)
        {
            if (!String.IsNullOrEmpty(modelName))
            {
                ModelDescriptionGenerator modelDescriptionGenerator = Configuration.GetModelDescriptionGenerator();
                ModelDescription modelDescription;
                if (modelDescriptionGenerator.GeneratedModels.TryGetValue(modelName, out modelDescription))
                {
                    return View(modelDescription);
                }
            }

            return View(ErrorViewName);
        }
    }
}