using CommonHelper.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using ThinkAPIService.Model;
using ThinkAPIService.Service;

namespace ThinkAPIService.API
{
    /// <summary>
    /// 查询节假日
    /// </summary>
    public class CalendarMonthCommand : ThinkAPIBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="apiName"></param>
        public CalendarMonthCommand(string apiName = "calendar/month") : base(apiName, HttpMethod.Get)
        {
            //接口地址：https://www.topthink.com/api/90
        }

        /// <summary>
        /// 执行请求
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public ResultModel Execute(DateTime? date = null)
        {
            if (date == null) date = DateTime.Today;

            string dateStr = date.Value.ToString("yyyy-MM");

            return base.Execute<HolidayModel>(new Dictionary<string, string>()
            {
                { "yearMonth", Regex.Replace(dateStr, "-0([1-9])", "-$1") }//接口要求不是两位数需要去0
            });
        }
    }
}
