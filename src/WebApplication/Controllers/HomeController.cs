using System.Web.Mvc;

namespace WebApplication.Controllers
{
    /// <summary>
    /// 首页控制器，用于处理根路径请求并转发到Vue应用
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// 首页操作，将请求转发到Vue应用的index.html
        /// </summary>
        /// <returns>空结果，因为使用了Server.Transfer进行内部转发</returns>
        public ActionResult Index()
        {
            Server.Transfer(Url.Content("~/dist/index.html"));
            return new EmptyResult();
        }
    }
}