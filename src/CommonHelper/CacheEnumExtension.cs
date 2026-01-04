using CommonHelper.Model;
using System.Collections.Generic;

namespace CommonHelper
{
    /// <summary>
    /// 缓存枚举扩展类，提供缓存键的生成和格式化功能
    /// </summary>
    public static class CacheEnumExtension
    {
        /// <summary>
        /// 缓存键的格式化字符串模板，{0}为模块缓存键，{1}为微键
        /// </summary>
        public static string CACHE_KEY_FORMAT = "cache_{1}_{0}";
        
        /// <summary>
        /// 缓存模块与缓存键的映射字典
        /// </summary>
        public static Dictionary<Cache_Model, string> CacheKeys = new Dictionary<Cache_Model, string>()
        {
            [Cache_Model.CACHE_KEY_MODULENAME_EntityMetadata] = "HBXN_EntityMetadata",
        };

        /// <summary>
        /// 获取指定缓存模块的缓存键
        /// </summary>
        /// <param name="module">缓存模块枚举</param>
        /// <returns>缓存键字符串</returns>
        public static string GetCacheKey(this Cache_Model module) => CacheKeys[module];

        /// <summary>
        /// 根据缓存模块和微键生成格式化的缓存键
        /// </summary>
        /// <param name="module">缓存模块枚举</param>
        /// <param name="microKey">微键，用于区分同一模块下的不同缓存项</param>
        /// <returns>格式化后的完整缓存键</returns>
        public static string CacheKeyFormat(this Cache_Model module, string microKey)
            => string.Format(CACHE_KEY_FORMAT, module.GetCacheKey(), microKey);

    }
}
