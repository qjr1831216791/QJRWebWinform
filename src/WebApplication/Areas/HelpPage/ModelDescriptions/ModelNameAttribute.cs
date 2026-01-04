using System;

namespace WebApplication.Areas.HelpPage.ModelDescriptions
{
    /// <summary>
    /// Use this attribute to change the name of the <see cref="ModelDescription"/> generated for a type.
    /// </summary>
    /// <summary>
    /// 模型名称特性，用于更改为类型生成的ModelDescription的名称
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, AllowMultiple = false, Inherited = false)]
    public sealed class ModelNameAttribute : Attribute
    {
        /// <summary>
        /// 初始化模型名称特性实例
        /// </summary>
        /// <param name="name">自定义的模型名称</param>
        public ModelNameAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// 获取自定义的模型名称
        /// </summary>
        public string Name { get; private set; }
    }
}