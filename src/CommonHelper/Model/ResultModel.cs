using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelper.Model
{
    public enum ResultCode
    {
        /// <summary>
        /// Success
        /// </summary>
        [Description("Success")]
        Success = 1,

        /// <summary>
        /// Failed
        /// </summary>
        [Description("Failed")]
        Failed = 0,
    }

    /// <summary>
    /// 结果模型类，用于封装API或方法的执行结果
    /// </summary>
    public class ResultModel
    {
        /// <summary>
        /// 是否执行成功
        /// </summary>
        public bool isSuccess { get; set; }

        /// <summary>
        /// 结果消息
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// 结果代码
        /// </summary>
        public int code { get; set; }

        /// <summary>
        /// 返回的数据对象
        /// </summary>
        public object data { get; set; }

        /// <summary>
        /// 设置成功结果
        /// </summary>
        /// <param name="message">成功消息，默认为"Success"</param>
        /// <param name="code">结果代码，默认为ResultCode.Success</param>
        /// <param name="data">返回的数据对象，如果为null则不设置</param>
        public void Success(string message = "Success", int code = (int)ResultCode.Success, object data = null)
        {
            this.isSuccess = true;
            this.message = message;
            this.code = code;
            if (data != null) this.data = data;
        }

        /// <summary>
        /// 设置失败结果
        /// </summary>
        /// <param name="message">失败消息，默认为"Failed"</param>
        /// <param name="code">结果代码，默认为ResultCode.Failed</param>
        /// <param name="data">返回的数据对象，如果为null则不设置</param>
        public void Failed(string message = "Failed", int code = (int)ResultCode.Failed, object data = null)
        {
            this.isSuccess = false;
            this.message = message;
            this.code = code;
            if (data != null) this.data = data;
        }
    }
}
