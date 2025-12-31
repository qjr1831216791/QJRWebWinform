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
        /// Gets a command object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
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