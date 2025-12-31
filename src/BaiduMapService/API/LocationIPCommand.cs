using BaiduMapService.Model;
using BaiduMapService.Service;
using CommonHelper.Model;
using System.Collections.Generic;
using System.Net.Http;

namespace BaiduMapService.API
{
    public class LocationIPCommand : BaiduMapAPIBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="apiName"></param>
        public LocationIPCommand(string apiName = "location/ip") : base(apiName, HttpMethod.Get) { }

        /// <summary>
        /// 根据IP地址或经纬度坐标获取地址信息
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="coor"></param>
        /// <returns></returns>
        public ResultModel Execute(string ip = null, string coor = null)
        {
            var input = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(ip))
            {
                input.Add("ip", ip);
            }
            if (!string.IsNullOrEmpty(coor))
            {
                input.Add("coor", coor);
            }
            return base.Execute<LocationIPOutput>(input);
        }
    }
}
