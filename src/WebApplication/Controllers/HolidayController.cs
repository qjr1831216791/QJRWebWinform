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
        /// <param name="date">日期字符串，格式：yyyy-MM，如果为空则获取当前月份的节假日</param>
        /// <returns>包含节假日列表的结果模型</returns>
        [HttpGet]
        public virtual ResultModel GetHolidayList(string date = null)
        {
            return Command<HolidayCommand>().GetHolidayList(date);
        }

        /// <summary>
        /// 通过配置文件获取纪念日信息
        /// </summary>
        /// <param name="date">日期字符串，格式：yyyy-MM-dd，如果为空则获取所有纪念日</param>
        /// <returns>包含纪念日列表的结果模型</returns>
        [HttpGet]
        public virtual ResultModel GetAnniversaryList(string date = null)
        {
            return Command<HolidayCommand>().GetAnniversaryList(date);
        }
    }
}
