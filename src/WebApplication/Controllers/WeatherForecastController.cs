using APIService.WeatherForecast;
using CommonHelper.Model;
using System.Web.Http;

namespace WebApplication.Controllers
{
    /// <summary>
    /// 天气预报
    /// </summary>
    public class WeatherForecastController : BaseController
    {
        /// <summary>
        /// 自动获取当前城市的天气预报
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public virtual ResultModel GetWeatherForecast(string citycode = null)
        {
            return Command<WeatherForecastCommand>().GetWeatherForecast(citycode);
        }

        /// <summary>
        /// 获取中国的省份和城市
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public virtual ResultModel GetBaiduMapTownShip()
        {
            return Command<WeatherForecastCommand>().GetBaiduMapTownShip();
        }
    }
}
