using APIService.Holiday;
using CommonHelper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QJRWebWinform.WPF.Controllers
{
    public class HolidayController : DynamicControllerBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="mainWindow"></param>
        public HolidayController(Window mainWindow) : base(mainWindow)
        {
        }

        public override string Name => "Holiday";

        /// <summary>
        /// 通过ThinkAPI获取节假日信息
        /// </summary>
        /// <param name="parameters">JSON参数：{"date": "string"} 或 null</param>
        /// <returns></returns>
        public virtual object GetHolidayList(string parameters)
        {
            var input = DeserializeParameters<GetHolidayListInput>(parameters);
            SetParameters(parameters);
            return Command<HolidayCommand>().GetHolidayList(input?.date);
        }

        /// <summary>
        /// 通过配置文件获取纪念日信息
        /// </summary>
        /// <param name="parameters">JSON参数：{"date": "string"} 或 null</param>
        /// <returns></returns>
        public virtual object GetAnniversaryList(string parameters)
        {
            var input = DeserializeParameters<GetAnniversaryListInput>(parameters);
            SetParameters(parameters);
            return Command<HolidayCommand>().GetAnniversaryList(input?.date);
        }

        private class GetHolidayListInput
        {
            public string date { get; set; }
        }

        private class GetAnniversaryListInput
        {
            public string date { get; set; }
        }
    }
}
