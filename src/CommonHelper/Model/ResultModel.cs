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

    public class ResultModel
    {
        public bool isSuccess { get; set; }

        public string message { get; set; }

        public int code { get; set; }

        public object data { get; set; }

        public void Success(string message = "Success", int code = (int)ResultCode.Success, object data = null)
        {
            this.isSuccess = true;
            this.message = message;
            this.code = code;
            if (data != null) this.data = data;
        }

        public void Failed(string message = "Failed", int code = (int)ResultCode.Failed, object data = null)
        {
            this.isSuccess = false;
            this.message = message;
            this.code = code;
            if (data != null) this.data = data;
        }
    }
}
