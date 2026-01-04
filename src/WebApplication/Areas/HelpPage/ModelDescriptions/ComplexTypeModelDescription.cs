using System.Collections.ObjectModel;

namespace WebApplication.Areas.HelpPage.ModelDescriptions
{
    /// <summary>
    /// 复杂类型模型描述类，用于描述复杂对象类型的模型信息
    /// </summary>
    public class ComplexTypeModelDescription : ModelDescription
    {
        /// <summary>
        /// 初始化复杂类型模型描述实例
        /// </summary>
        public ComplexTypeModelDescription()
        {
            Properties = new Collection<ParameterDescription>();
        }

        /// <summary>
        /// 获取复杂类型的所有属性描述集合
        /// </summary>
        public Collection<ParameterDescription> Properties { get; private set; }
    }
}