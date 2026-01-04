namespace WebApplication.Areas.HelpPage.ModelDescriptions
{
    /// <summary>
    /// 枚举值描述类，用于描述枚举类型的单个枚举值信息
    /// </summary>
    public class EnumValueDescription
    {
        /// <summary>
        /// 获取或设置枚举值的文档说明
        /// </summary>
        public string Documentation { get; set; }

        /// <summary>
        /// 获取或设置枚举值的名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 获取或设置枚举值的数值（字符串形式）
        /// </summary>
        public string Value { get; set; }
    }
}