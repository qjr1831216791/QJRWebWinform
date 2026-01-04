using System;
using System.Reflection;

namespace WebApplication.Areas.HelpPage.ModelDescriptions
{
    /// <summary>
    /// 模型文档提供程序接口，用于从XML文档中获取类型和成员的文档说明
    /// </summary>
    public interface IModelDocumentationProvider
    {
        /// <summary>
        /// 获取成员（属性或字段）的文档说明
        /// </summary>
        /// <param name="member">成员信息</param>
        /// <returns>成员的XML文档说明，如果未找到则返回null</returns>
        string GetDocumentation(MemberInfo member);

        /// <summary>
        /// 获取类型的文档说明
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>类型的XML文档说明，如果未找到则返回null</returns>
        string GetDocumentation(Type type);
    }
}