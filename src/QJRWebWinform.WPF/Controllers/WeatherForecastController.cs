using APIService.WeatherForecast;
using CommonHelper.Model;
using CefSharp;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace QJRWebWinform.WPF.Controllers
{
    /// <summary>
    /// 天气预报
    /// </summary>
    public class WeatherForecastController : DynamicControllerBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="mainWindow"></param>
        public WeatherForecastController(Window mainWindow) : base(mainWindow)
        {
        }

        public override string Name => "WeatherForecast";

        /// <summary>
        /// 自动获取当前城市的天气预报（核心业务逻辑）
        /// </summary>
        /// <param name="parameters">JSON参数：{"citycode": "string"} 或 null</param>
        /// <returns></returns>
        private ResultModel GetWeatherForecastCore(string parameters)
        {
            var input = DeserializeParameters<GetWeatherForecastInput>(parameters);
            SetParameters(parameters);
            return Command<WeatherForecastCommand>().GetWeatherForecast(input?.citycode);
        }

        /// <summary>
        /// 自动获取当前城市的天气预报（同步版本）
        /// </summary>
        /// <param name="parameters">JSON参数：{"citycode": "string"} 或 null</param>
        /// <returns></returns>
        public virtual object GetWeatherForecast(string parameters)
        {
            return GetWeatherForecastCore(parameters);
        }

        /// <summary>
        /// 自动获取当前城市的天气预报（异步版本）
        /// </summary>
        /// <param name="parameters">JSON参数：{"citycode": "string"} 或 null</param>
        /// <param name="callback">JavaScript 回调函数</param>
        public virtual void GetWeatherForecast(string parameters, IJavascriptCallback callback)
        {
            Task.Run(() =>
            {
                try
                {
                    var result = GetWeatherForecastCore(parameters);
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
        /// 获取中国的省份和城市（核心业务逻辑）
        /// </summary>
        /// <param name="parameters">JSON参数：null 或 "{}"</param>
        /// <returns></returns>
        private ResultModel GetBaiduMapTownShipCore(string parameters)
        {
            SetParameters(parameters);
            return Command<WeatherForecastCommand>().GetBaiduMapTownShip();
        }

        /// <summary>
        /// 获取中国的省份和城市（同步版本）
        /// </summary>
        /// <param name="parameters">JSON参数：null 或 "{}"</param>
        /// <returns></returns>
        public virtual object GetBaiduMapTownShip(string parameters)
        {
            return GetBaiduMapTownShipCore(parameters);
        }

        /// <summary>
        /// 获取中国的省份和城市（异步版本）
        /// </summary>
        /// <param name="parameters">JSON参数：null 或 "{}"</param>
        /// <param name="callback">JavaScript 回调函数</param>
        public virtual void GetBaiduMapTownShip(string parameters, IJavascriptCallback callback)
        {
            Task.Run(() =>
            {
                try
                {
                    var result = GetBaiduMapTownShipCore(parameters);
                    var resultJson = JsonConvert.SerializeObject(result);
                    callback.ExecuteAsync(true, resultJson);
                }
                catch (Exception ex)
                {
                    callback.ExecuteAsync(false, ex.Message);
                }
            });
        }

        private class GetWeatherForecastInput
        {
            public string citycode { get; set; }
        }
    }
}

