using System.Web.Mvc;

namespace WebApplication.Controllers
{
    /// <summary>
    /// HomeController
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Index
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            Server.Transfer(Url.Content("~/dist/index.html"));
            return new EmptyResult();
        }
    }
}