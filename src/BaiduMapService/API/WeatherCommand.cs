using BaiduMapService.Model;
using BaiduMapService.Service;
using CommonHelper.Model;
using System.Collections.Generic;
using System.Net.Http;

namespace BaiduMapService.API
{
    /// <summary>
    /// 天气预报查询
    /// </summary>
    public class WeatherCommand : BaiduMapAPIBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="apiName"></param>
        public WeatherCommand(string apiName = "weather/v1/") : base(apiName, HttpMethod.Get)
        {
        }

        /// <summary>
        /// 执行请求
        /// </summary>
        /// <param name="district_id"></param>
        /// <param name="data_type"></param>
        /// <returns></returns>
        public ResultModel Execute(string district_id, string data_type = null)
        {
            var input = new Dictionary<string, string>()
            {
                { "output", "json" }
            };
            if (!string.IsNullOrEmpty(district_id))
            {
                input.Add("district_id", district_id);
            }
            input.Add("data_type", string.IsNullOrWhiteSpace(data_type) ? WeatherApiDataType.all : data_type);
            return base.Execute<WeatherOutput>(input);
        }
    }
}
