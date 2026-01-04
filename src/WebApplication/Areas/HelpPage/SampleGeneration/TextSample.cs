using System;

namespace WebApplication.Areas.HelpPage
{
    /// <summary>
    /// 表示帮助页面上的预格式化文本示例，与此类关联的显示模板名为TextSample
    /// </summary>
    public class TextSample
    {
        /// <summary>
        /// 初始化文本示例实例
        /// </summary>
        /// <param name="text">文本内容</param>
        /// <exception cref="ArgumentNullException">当text为null时抛出</exception>
        public TextSample(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            Text = text;
        }

        /// <summary>
        /// 获取文本内容
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// 确定指定的对象是否等于当前对象
        /// </summary>
        /// <param name="obj">要与当前对象进行比较的对象</param>
        /// <returns>如果指定的对象等于当前对象，则为true；否则为false</returns>
        public override bool Equals(object obj)
        {
            TextSample other = obj as TextSample;
            return other != null && Text == other.Text;
        }

        /// <summary>
        /// 作为默认哈希函数
        /// </summary>
        /// <returns>当前对象的哈希代码</returns>
        public override int GetHashCode()
        {
            return Text.GetHashCode();
        }

        /// <summary>
        /// 返回表示当前对象的字符串
        /// </summary>
        /// <returns>文本内容</returns>
        public override string ToString()
        {
            return Text;
        }
    }
}