using BaiduMapService.Service;

namespace BaiduMapService.Model
{
    public class LocationIPOutput : BaiduMapApiResBase
    {
        /// <summary>
        /// 详细地址信息
        /// </summary>
        public string address { get; set; }

        /// <summary>
        /// 地址结构信息
        /// </summary>
        public AddressContent content { get; set; }
    }

    public class AddressContent
    {
        /// <summary>
        /// 简要地址信息
        /// </summary>
        public string address { get; set; }

        /// <summary>
        /// 地址结构信息
        /// </summary>
        public AddressDetail address_detail { get; set; }

        /// <summary>
        /// 城市中心经纬度
        /// </summary>
        public CityPoint point { get; set; }
    }

    public class AddressDetail
    {
        public string adcode { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public string city { get; set; }

        /// <summary>
        /// 百度城市代码
        /// </summary>
        public string city_code { get; set; }
        public string district { get; set; }

        /// <summary>
        /// 省份
        /// </summary>
        public string province { get; set; }
        public string street { get; set; }
        public string street_number { get; set; }
    }

    /// <summary>
    /// 城市中心经纬度
    /// </summary>
    public class CityPoint
    {
        /// <summary>
        /// 当前城市中心点经度
        /// </summary>
        public string x { get; set; }

        /// <summary>
        /// 当前城市中心点纬度
        /// </summary>
        public string y { get; set; }
    }
}
