using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelper.Model
{
    public class LanguageInfo
    {
        /// <summary>
        /// Guid
        /// </summary>
        public Guid languagelocaleid { get; set; }

        /// <summary>
        /// 语言代码
        /// </summary>
        public int? localeid { get; set; }

        /// <summary>
        /// 语言编号
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// 语言名称
        /// </summary>
        public string language { get; set; }
    }
}
