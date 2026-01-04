using System;

namespace WebApplication.Areas.HelpPage.ModelDescriptions
{
    /// <summary>
    /// 模型描述基类，用于描述类型的模型信息
    /// </summary>
    public abstract class ModelDescription
    {
        /// <summary>
        /// 获取或设置模型的文档说明
        /// </summary>
        public string Documentation { get; set; }

        /// <summary>
        /// 获取或设置模型的类型
        /// </summary>
        public Type ModelType { get; set; }

        /// <summary>
        /// 获取或设置模型的名称
        /// </summary>
        public string Name { get; set; }
    }
}