using CommonHelper.Model;
using System.Collections.Generic;

namespace CommonHelper
{
    public static class CacheEnumExtension
    {
        public static string CACHE_KEY_FORMAT = "cache_{1}_{0}";
        public static Dictionary<Cache_Model, string> CacheKeys = new Dictionary<Cache_Model, string>()
        {
            [Cache_Model.CACHE_KEY_MODULENAME_EntityMetadata] = "HBXN_EntityMetadata",
        };


        public static string GetCacheKey(this Cache_Model module) => CacheKeys[module];

        public static string CacheKeyFormat(this Cache_Model module, string microKey)
            => string.Format(CACHE_KEY_FORMAT, module.GetCacheKey(), microKey);

    }
}
