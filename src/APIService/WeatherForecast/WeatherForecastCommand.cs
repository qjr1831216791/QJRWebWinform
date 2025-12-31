using BaiduMapService.API;
using BaiduMapService.Model;
using BaiduMapService.Service;
using CommonHelper;
using CommonHelper.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace APIService.WeatherForecast
{
    public class WeatherForecastCommand : BaseCommand
    {
        /// <summary>
        /// 自动获取当前城市的天气预报
        /// </summary>
        /// <returns></returns>
        public ResultModel GetWeatherForecast(string citycode)
        {
            ResultModel result = new ResultModel();
            try
            {
                if (string.IsNullOrWhiteSpace(citycode))
                {
                    LocationIPCommand cmd = new LocationIPCommand();
                    var res = cmd.Execute();
                    if (res == null || !res.isSuccess) throw new Exception(res.message);
                    var city = BaiduMapTownShipArea.GetCityCode(((LocationIPOutput)res.data).content.address_detail.adcode);
                    citycode = city != null ? city.cityCode : ((LocationIPOutput)res.data).content.address_detail.adcode;
                }

                WeatherCommand cmd2 = new WeatherCommand();
                result = cmd2.Execute(citycode);

                if (!result.isSuccess && result.code.Equals(40))
                {
                    result = cmd2.Execute("440100");//如果城市编码不合法，则查询广州-440100
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 获取中国的省份和城市
        /// </summary>
        /// <returns></returns>
        public ResultModel GetBaiduMapTownShip()
        {
            ResultModel result = new ResultModel();
            try
            {
                BaiduMapTownShipArea.BaiduMapTownShipInitialization();
                result.Success(data: BaiduMapTownShip.options);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }
    }
}
