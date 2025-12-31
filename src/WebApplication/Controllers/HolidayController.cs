using APIService.Holiday;
using CommonHelper.Model;
using System.Web.Http;

namespace WebApplication.Controllers
{
    /// <summary>
    /// 节假日与纪念日
    /// </summary>
    public class HolidayController : BaseController
    {
        /// <summary>
        /// 通过ThinkAPI获取节假日信息
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        [HttpGet]
        public virtual ResultModel GetHolidayList(string date = null)
        {
            return Command<HolidayCommand>().GetHolidayList(date);
        }

        /// <summary>
        /// 通过配置文件获取纪念日信息
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        [HttpGet]
        public virtual ResultModel GetAnniversaryList(string date = null)
        {
            return Command<HolidayCommand>().GetAnniversaryList(date);
        }
    }
}
