using APIService.WeatherForecast;
using CommonHelper.Model;
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
        /// 自动获取当前城市的天气预报
        /// </summary>
        /// <param name="parameters">JSON参数：{"citycode": "string"} 或 null</param>
        /// <returns></returns>
        public virtual object GetWeatherForecast(string parameters)
        {
            var input = DeserializeParameters<GetWeatherForecastInput>(parameters);
            SetParameters(parameters);
            return Command<WeatherForecastCommand>().GetWeatherForecast(input?.citycode);
        }

        /// <summary>
        /// 获取中国的省份和城市
        /// </summary>
        /// <param name="parameters">JSON参数：null 或 "{}"</param>
        /// <returns></returns>
        public virtual object GetBaiduMapTownShip(string parameters)
        {
            SetParameters(parameters);
            return Command<WeatherForecastCommand>().GetBaiduMapTownShip();
        }

        private class GetWeatherForecastInput
        {
            public string citycode { get; set; }
        }
    }
}

