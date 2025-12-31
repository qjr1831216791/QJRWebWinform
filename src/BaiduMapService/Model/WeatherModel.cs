using BaiduMapService.Service;
using System.Collections.Generic;

namespace BaiduMapService.Model
{
    public static class WeatherApiDataType
    {
        /// <summary>
        /// 表示查询当前的天气信息，返回结果中通常会包含当前的天气状况、温度、体感温度、相对湿度、风力等级、风向以及数据更新时间等，可让用户快速获取当下的天气状态
        /// </summary>
        public static string now { get; set; } = "now";

        /// <summary>
        /// 代表查询未来几天的天气预报信息
        /// </summary>
        public static string fc { get; set; } = "fc";

        /// <summary>
        /// 一般是指天气指数信息，例如穿衣指数、紫外线指数、感冒指数等
        /// </summary>
        public static string index { get; set; } = "index";

        /// <summary>
        /// 主要用于查询气象灾害预警信息，当有暴雨、大风、雷电、高温等气象灾害可能发生或已经发生时，会通过此参数返回相应的预警级别、预警内容、发布时间
        /// </summary>
        public static string alert { get; set; } = "alert";

        /// <summary>
        /// 表示查询未来几小时的逐小时天气预报信息，相比 fc 提供的按天的预报，此参数能更精确地呈现短时间内天气的变化情况
        /// </summary>
        public static string fc_hour { get; set; } = "fc_hour";

        /// <summary>
        /// 代表查询所有可用的天气信息，是一种综合查询的方式，返回的数据比较全面，涵盖了 now、fc、index 等不同类型数据的组合
        /// </summary>
        public static string all { get; set; } = "all";
    }

    public class WeatherOutput : BaiduMapApiResBase
    {
        public WeatherResult result { get; set; }
    }

    // 对应 "result" 节点下的对象结构
    public class WeatherResult
    {
        /// <summary>
        /// 地址信息
        /// </summary>
        public AddressLocation location { get; set; }

        /// <summary>
        /// 当天天气
        /// </summary>
        public WeatherNow now { get; set; }

        /// <summary>
        /// 未来的天气预报
        /// </summary>
        public List<WeatherForecast> forecasts { get; set; }
    }

    /// <summary>
    /// 地址信息
    /// </summary>
    public class AddressLocation
    {
        /// <summary>
        /// 国家
        /// </summary>
        public string country { get; set; }

        /// <summary>
        /// 省份
        /// </summary>
        public string province { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public string city { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 百度城市编码
        /// </summary>
        public string id { get; set; }
    }

    /// <summary>
    /// 当天天气
    /// </summary>
    public class WeatherNow
    {
        /// <summary>
        /// 天气现象
        /// </summary>
        public string text { get; set; }

        /// <summary>
        /// 温度（℃）
        /// </summary>
        public int temp { get; set; }

        /// <summary>
        /// 体感温度(℃)
        /// </summary>
        public int feels_like { get; set; }

        /// <summary>
        /// 相对湿度(%)
        /// </summary>
        public int rh { get; set; }

        /// <summary>
        /// 风力等级
        /// </summary>
        public string wind_class { get; set; }

        /// <summary>
        /// 风向描述
        /// </summary>
        public string wind_dir { get; set; }

        /// <summary>
        /// 数据更新时间，北京时间
        /// </summary>
        public string uptime { get; set; }
    }

    /// <summary>
    /// 未来的天气预报
    /// </summary>
    public class WeatherForecast
    {
        /// <summary>
        /// 白天天气现象
        /// </summary>
        public string text_day { get; set; }

        /// <summary>
        /// 晚上天气现象
        /// </summary>
        public string text_night { get; set; }

        /// <summary>
        /// 最高温度(℃)
        /// </summary>
        public int high { get; set; }

        /// <summary>
        /// 最低温度(℃)
        /// </summary>
        public int low { get; set; }

        /// <summary>
        /// 白天风力
        /// </summary>
        public string wc_day { get; set; }

        /// <summary>
        /// 白天风向
        /// </summary>
        public string wd_day { get; set; }

        /// <summary>
        /// 晚上风力
        /// </summary>
        public string wc_night { get; set; }

        /// <summary>
        /// 晚上风向
        /// </summary>
        public string wd_night { get; set; }

        /// <summary>
        /// 日期，北京时区
        /// </summary>
        public string date { get; set; }

        /// <summary>
        /// 星期，北京时区
        /// </summary>
        public string week { get; set; }
    }
}
