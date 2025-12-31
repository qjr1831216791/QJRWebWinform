using System.ComponentModel;

namespace CommonHelper.Model
{
    public enum StateCode
    {
        /// <summary>
        /// Active
        /// </summary>
        [Description("Active")]
        Active = 0,

        /// <summary>
        /// Deactive
        /// </summary>
        [Description("Deactive")]
        Deactive = 1,
    }

    public enum StatusCode
    {
        /// <summary>
        /// Active
        /// </summary>
        [Description("Active")]
        Active = 1,

        /// <summary>
        /// Deactive
        /// </summary>
        [Description("Deactive")]
        Deactive = 2,
    }

    public enum Cache_Model
    {
        CACHE_KEY_MODULENAME_EntityMetadata,
        CACHE_KEY_MODULENAME_AutoAssignedLead,
    }

    public static class ConfigFileNameEnum
    {
        /// <summary>
        /// RekTec.XStudio.CrmClient.Config.CrmConfig.xml.config
        /// </summary>
        public static string CrmConfig { get; } = "RekTec.XStudio.CrmClient.Config.CrmConfig.xml.config";

        /// <summary>
        /// Environments.CrmClient.Config.CrmConfig.json
        /// </summary>
        public static string CrmConfigJSON { get; } = "Environments.CrmClient.Config.CrmConfig.json";

        /// <summary>
        /// 百度地图的配置文件
        /// </summary>
        public static string BaiduMapConfig { get; } = "Baidu.Map.Config.xml.config";

        /// <summary>
        /// 百度地图的城乡编码配置文件
        /// </summary>
        public static string BaiduMapTownshipArea { get; } = "BaiduMap_Township_Area.xlsx";

        /// <summary>
        /// ThinkAPI配置文件
        /// </summary>
        public static string ThinkAPIConfig { get; } = "ThinkAPI.Config.xml.config";

        /// <summary>
        /// 纪念日配置文件
        /// </summary>
        public static string AnniversaryDate { get; } = "Anniversary.date.json";

        /// <summary>
        /// Log配置文件
        /// </summary>
        public static string LogConfig { get; } = "Log.Config.xml.config";
    }
}
