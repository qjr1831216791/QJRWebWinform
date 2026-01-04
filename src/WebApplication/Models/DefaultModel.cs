using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication.Models
{
    /// <summary>
    /// 测试API Post入参3的模型类，用于测试复杂参数传递
    /// </summary>
    public class TestAPIPost3Model
    {
        /// <summary>
        /// 输入的字符串参数
        /// </summary>
        public string input { get; set; }

        /// <summary>
        /// 输入的字符串列表参数
        /// </summary>
        public List<String> list { get; set; }
    }
}