using Newtonsoft.Json;
using System.Collections.Generic;

namespace BaiduMapService.Model
{
    public static class BaiduMapTownShip
    {
        [JsonIgnore]
        public static Dictionary<string, string> dict { get; set; }

        [JsonIgnore]
        public static List<BaiduMapTownShipRow> table { get; set; }

        public static List<BaiduMapTownShipItem> options { get; set; }
    }

    public class BaiduMapTownShipItem
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="value"></param>
        /// <param name="label"></param>
        public BaiduMapTownShipItem(string value, string label)
        {
            this.value = value;
            this.label = label;
        }

        public string value { get; set; }

        public string label { get; set; }

        [JsonIgnore]
        public Dictionary<string, string> dict { get; set; }

        public List<BaiduMapTownShipItem> children { get; set; }
    }

    public class BaiduMapTownShipRow
    {
        public string provinceName { get; set; }

        public string provinceCode { get; set; }

        public string cityName { get; set; }

        public string cityCode { get; set; }

        public string countyName { get; set; }

        public string countyCode { get; set; }

        public string streetName { get; set; }

        public string streetCode { get; set; }
    }
}
