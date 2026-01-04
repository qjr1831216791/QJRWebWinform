namespace WebApplication.Areas.HelpPage.ModelDescriptions
{
    /// <summary>
    /// 集合类型模型描述类，用于描述集合类型（如数组、列表等）的模型信息
    /// </summary>
    public class CollectionModelDescription : ModelDescription
    {
        /// <summary>
        /// 获取或设置集合元素的模型描述
        /// </summary>
        public ModelDescription ElementDescription { get; set; }
    }
}