namespace WebApplication.Areas.HelpPage.ModelDescriptions
{
    /// <summary>
    /// 键值对类型模型描述类，用于描述键值对类型（KeyValuePair&lt;TKey, TValue&gt;）的模型信息
    /// </summary>
    public class KeyValuePairModelDescription : ModelDescription
    {
        /// <summary>
        /// 获取或设置键的模型描述
        /// </summary>
        public ModelDescription KeyModelDescription { get; set; }

        /// <summary>
        /// 获取或设置值的模型描述
        /// </summary>
        public ModelDescription ValueModelDescription { get; set; }
    }
}