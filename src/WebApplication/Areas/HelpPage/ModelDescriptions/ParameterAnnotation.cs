using System;

namespace WebApplication.Areas.HelpPage.ModelDescriptions
{
    /// <summary>
    /// 参数注解类，用于描述参数的注解信息
    /// </summary>
    public class ParameterAnnotation
    {
        /// <summary>
        /// 获取或设置注解属性对象
        /// </summary>
        public Attribute AnnotationAttribute { get; set; }

        /// <summary>
        /// 获取或设置注解的文档说明
        /// </summary>
        public string Documentation { get; set; }
    }
}