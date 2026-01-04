using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WebApplication.Areas.HelpPage.ModelDescriptions
{
    /// <summary>
    /// 枚举类型模型描述类，用于描述枚举类型的模型信息
    /// </summary>
    public class EnumTypeModelDescription : ModelDescription
    {
        /// <summary>
        /// 初始化枚举类型模型描述实例
        /// </summary>
        public EnumTypeModelDescription()
        {
            Values = new Collection<EnumValueDescription>();
        }

        /// <summary>
        /// 获取枚举类型的所有枚举值描述集合
        /// </summary>
        public Collection<EnumValueDescription> Values { get; private set; }
    }
}