using System;
using System.Collections.Generic;

namespace CommonHelper
{
    public static class EnumHelper
    {
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
