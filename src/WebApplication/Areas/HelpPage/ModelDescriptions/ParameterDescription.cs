using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WebApplication.Areas.HelpPage.ModelDescriptions
{
    /// <summary>
    /// 参数描述类，用于描述API参数的信息
    /// </summary>
    public class ParameterDescription
    {
        /// <summary>
        /// 初始化参数描述实例
        /// </summary>
        public ParameterDescription()
        {
            Annotations = new Collection<ParameterAnnotation>();
        }

        /// <summary>
        /// 获取参数的注解集合
        /// </summary>
        public Collection<ParameterAnnotation> Annotations { get; private set; }

        /// <summary>
        /// 获取或设置参数的文档说明
        /// </summary>
        public string Documentation { get; set; }

        /// <summary>
        /// 获取或设置参数的名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 获取或设置参数类型的模型描述
        /// </summary>
        public ModelDescription TypeDescription { get; set; }
    }
}