using System;

namespace WebApplication.Areas.HelpPage
{
    /// <summary>
    /// 表示帮助页面上的无效示例，与此类关联的显示模板名为InvalidSample
    /// </summary>
    public class InvalidSample
    {
        /// <summary>
        /// 初始化无效示例实例
        /// </summary>
        /// <param name="errorMessage">错误消息</param>
        /// <exception cref="ArgumentNullException">当errorMessage为null时抛出</exception>
        public InvalidSample(string errorMessage)
        {
            if (errorMessage == null)
            {
                throw new ArgumentNullException("errorMessage");
            }
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// 获取错误消息
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// 确定指定的对象是否等于当前对象
        /// </summary>
        /// <param name="obj">要与当前对象进行比较的对象</param>
        /// <returns>如果指定的对象等于当前对象，则为true；否则为false</returns>
        public override bool Equals(object obj)
        {
            InvalidSample other = obj as InvalidSample;
            return other != null && ErrorMessage == other.ErrorMessage;
        }

        /// <summary>
        /// 作为默认哈希函数
        /// </summary>
        /// <returns>当前对象的哈希代码</returns>
        public override int GetHashCode()
        {
            return ErrorMessage.GetHashCode();
        }

        /// <summary>
        /// 返回表示当前对象的字符串
        /// </summary>
        /// <returns>错误消息</returns>
        public override string ToString()
        {
            return ErrorMessage;
        }
    }
}