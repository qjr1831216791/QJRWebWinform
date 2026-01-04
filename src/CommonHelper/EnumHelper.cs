using System;
using System.Collections.Generic;

namespace CommonHelper
{
    /// <summary>
    /// 枚举帮助类，提供枚举类型的转换和操作功能
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        /// 将枚举类型转换为字典，键为枚举的整数值，值为枚举的名称
        /// </summary>
        /// <typeparam name="TEnum">枚举类型</typeparam>
        /// <returns>包含枚举值和名称的字典</returns>
        public static Dictionary<int, string> ToDictionary<TEnum>()
        {
            Type typeFromHandle = typeof(TEnum);
            Array values = Enum.GetValues(typeFromHandle);
            Dictionary<int, string> dictionary = new Dictionary<int, string>();
            foreach (Enum item in values)
            {
                dictionary.Add(Convert.ToInt32(item), item.ToString());
            }
            return dictionary;
        }
    }
}
