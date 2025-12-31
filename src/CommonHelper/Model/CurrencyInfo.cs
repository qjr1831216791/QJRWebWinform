using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelper.Model
{
    public class CurrencyInfo
    {
        /// <summary>
        /// Guid
        /// </summary>
        public Guid transactioncurrencyid { get; set; }

        /// <summary>
        /// 货币名称
        /// </summary>
        public string currencyname { get; set; }

        /// <summary>
        /// 货币精度
        /// </summary>
        public int currencyprecision { get; set; }

        /// <summary>
        /// 货币符号
        /// </summary>
        public string currencysymbol { get; set; }

        /// <summary>
        /// 汇率
        /// </summary>
        public decimal exchangerate { get; set; }

        /// <summary>
        /// 货币代码
        /// </summary>
        public string isocurrencycode { get; set; }
    }
}
