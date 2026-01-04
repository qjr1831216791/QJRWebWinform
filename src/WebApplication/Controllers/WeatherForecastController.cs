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
        /// <param name="citycode">城市代码，如果为空则自动获取当前IP所在城市</param>
        /// <returns>包含天气预报信息的结果模型</returns>
        [HttpGet]
        public virtual ResultModel GetWeatherForecast(string citycode = null)
        {
            return Command<WeatherForecastCommand>().GetWeatherForecast(citycode);
        }

        /// <summary>
        /// 获取中国的省份和城市列表
        /// </summary>
        /// <returns>包含省份和城市信息的结果模型</returns>
        [HttpGet]
        public virtual ResultModel GetBaiduMapTownShip()
        {
            return Command<WeatherForecastCommand>().GetBaiduMapTownShip();
        }
    }
}
