using APIService.Holiday;
using CommonHelper.Model;
using CefSharp;
using Newtonsoft.Json;
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
        /// 通过ThinkAPI获取节假日信息（核心业务逻辑）
        /// </summary>
        /// <param name="parameters">JSON参数：{"date": "string"} 或 null</param>
        /// <returns></returns>
        private ResultModel GetHolidayListCore(string parameters)
        {
            var input = DeserializeParameters<GetHolidayListInput>(parameters);
            SetParameters(parameters);
            return Command<HolidayCommand>().GetHolidayList(input?.date);
        }

        /// <summary>
        /// 通过ThinkAPI获取节假日信息（同步版本）
        /// </summary>
        /// <param name="parameters">JSON参数：{"date": "string"} 或 null</param>
        /// <returns></returns>
        public virtual object GetHolidayList(string parameters)
        {
            return GetHolidayListCore(parameters);
        }

        /// <summary>
        /// 通过ThinkAPI获取节假日信息（异步版本）
        /// </summary>
        /// <param name="parameters">JSON参数：{"date": "string"} 或 null</param>
        /// <param name="callback">JavaScript 回调函数</param>
        public virtual void GetHolidayList(string parameters, IJavascriptCallback callback)
        {
            Task.Run(() =>
            {
                try
                {
                    var result = GetHolidayListCore(parameters);
                    var resultJson = JsonConvert.SerializeObject(result);
                    callback.ExecuteAsync(true, resultJson);
                }
                catch (Exception ex)
                {
                    callback.ExecuteAsync(false, ex.Message);
                }
            });
        }

        /// <summary>
        /// 通过配置文件获取纪念日信息（核心业务逻辑）
        /// </summary>
        /// <param name="parameters">JSON参数：{"date": "string"} 或 null</param>
        /// <returns></returns>
        private ResultModel GetAnniversaryListCore(string parameters)
        {
            var input = DeserializeParameters<GetAnniversaryListInput>(parameters);
            SetParameters(parameters);
            return Command<HolidayCommand>().GetAnniversaryList(input?.date);
        }

        /// <summary>
        /// 通过配置文件获取纪念日信息（同步版本）
        /// </summary>
        /// <param name="parameters">JSON参数：{"date": "string"} 或 null</param>
        /// <returns></returns>
        public virtual object GetAnniversaryList(string parameters)
        {
            return GetAnniversaryListCore(parameters);
        }

        /// <summary>
        /// 通过配置文件获取纪念日信息（异步版本）
        /// </summary>
        /// <param name="parameters">JSON参数：{"date": "string"} 或 null</param>
        /// <param name="callback">JavaScript 回调函数</param>
        public virtual void GetAnniversaryList(string parameters, IJavascriptCallback callback)
        {
            Task.Run(() =>
            {
                try
                {
                    var result = GetAnniversaryListCore(parameters);
                    var resultJson = JsonConvert.SerializeObject(result);
                    callback.ExecuteAsync(true, resultJson);
                }
                catch (Exception ex)
                {
                    callback.ExecuteAsync(false, ex.Message);
                }
            });
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
