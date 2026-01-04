using System;

namespace WebApplication.Areas.HelpPage
{
    /// <summary>
    /// This represents an image sample on the help page. There's a display template named ImageSample associated with this class.
    /// </summary>
    public class ImageSample
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageSample"/> class.
        /// </summary>
        /// <param name="src">The URL of an image.</param>
        public ImageSample(string src)
        {
            if (src == null)
            {
                throw new ArgumentNullException("src");
            }
            Src = src;
        }

        /// <summary>
        /// 获取图像的URL地址
        /// </summary>
        public string Src { get; private set; }

        /// <summary>
        /// 确定指定的对象是否等于当前对象
        /// </summary>
        /// <param name="obj">要与当前对象进行比较的对象</param>
        /// <returns>如果指定的对象等于当前对象，则为true；否则为false</returns>
        public override bool Equals(object obj)
        {
            ImageSample other = obj as ImageSample;
            return other != null && Src == other.Src;
        }

        /// <summary>
        /// 作为默认哈希函数
        /// </summary>
        /// <returns>当前对象的哈希代码</returns>
        public override int GetHashCode()
        {
            return Src.GetHashCode();
        }

        /// <summary>
        /// 返回表示当前对象的字符串
        /// </summary>
        /// <returns>图像的URL地址</returns>
        public override string ToString()
        {
            return Src;
        }
    }
}