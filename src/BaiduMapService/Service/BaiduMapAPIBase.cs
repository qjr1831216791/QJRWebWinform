using BaiduMapService.Model;
using CommonHelper;
using CommonHelper.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace BaiduMapService.Service
{
    /// <summary>
    /// 百度地图API基类，提供调用百度地图API的基础功能
    /// </summary>
    public class BaiduMapAPIBase
    {
        /// <summary>
        /// 百度地图配置实例，包含AK、SK等配置信息
        /// </summary>
        public BaiduMapConfig config { get; set; } = BaiduMapConfig.GetInstance();

        /// <summary>
        /// 百度地图API的基础URL
        /// </summary>
        public readonly string APIUrl = "https://api.map.baidu.com";

        /// <summary>
        /// API接口名称，例如：weather/v1/
        /// </summary>
        public string apiName { get; set; }

        /// <summary>
        /// HTTP请求方法，默认为GET
        /// </summary>
        public HttpMethod HttpMethod { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="apiName"></param>
        /// <param name="httpMethod"></param>
        public BaiduMapAPIBase(string apiName, HttpMethod httpMethod = null)
        {
            this.apiName = apiName;
            this.HttpMethod = HttpMethod is null ? HttpMethod.Get : HttpMethod;//默认为Get
        }

        /// <summary>
        /// 执行GET请求调用百度地图API
        /// </summary>
        /// <typeparam name="T">返回数据的类型</typeparam>
        /// <param name="input">请求参数字典，key为参数名，value为参数值</param>
        /// <returns>包含API调用结果的ResultModel对象</returns>
        public virtual ResultModel Get<T>(Dictionary<string, string> input)
        {
            ResultModel result = new ResultModel();
            input = input == null ? new Dictionary<string, string>() { } : input;
            if (!input.ContainsKey("ak")) input.Add("ak", config.AK);
            try
            {
                string url = $"{APIUrl}/{apiName}";
                string inputQuery = GetQueryString(input);
                string sn = config.CaculateAKSN($"/{apiName}", input);
                object res = null;

                StringBuilder URL = new StringBuilder(url);
                URL.Append($"?{inputQuery}");
                URL.Append($"&sn={sn}");

                AsyncUtil.RunSync(async () =>
                {
                    using (var req = new HttpClient())
                    {
                        HttpResponseMessage response = await req.GetAsync(URL.ToString());
                        string responseContent = await response.Content.ReadAsStringAsync();

                        res = JsonConvert.DeserializeObject<T>(responseContent);

                        if (res != null && ((BaiduMapApiResBase)res).status.Equals("0"))
                        {
                            result.Success(data: res);
                        }
                        else
                        {
                            result.Failed(
                                code: int.TryParse(((BaiduMapApiResBase)res).status, out int status) ? status : (int)ResultCode.Failed,
                                message: ((BaiduMapApiResBase)res).message);
                        }
                    }
                });
            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }

        /// <summary>
        /// 执行API接口调用，根据HttpMethod选择相应的请求方法
        /// </summary>
        /// <typeparam name="T">返回数据的类型</typeparam>
        /// <param name="data">请求数据，对于GET请求应为Dictionary&lt;string, string&gt;类型</param>
        /// <returns>包含API调用结果的ResultModel对象</returns>
        /// <exception cref="Exception">当HttpMethod不支持时抛出异常</exception>
        public virtual ResultModel Execute<T>(object data)
        {
            if (this.HttpMethod.Equals(HttpMethod.Get)) return Get<T>((Dictionary<string, string>)data);
            else throw new Exception("未知的HttpMethod！");
        }

        /// <summary>
        /// 将参数字典转换为URL查询字符串
        /// </summary>
        /// <param name="input">参数字典，key为参数名，value为参数值</param>
        /// <returns>URL编码后的查询字符串，例如：key1=value1&key2=value2</returns>
        protected string GetQueryString(Dictionary<string, string> input)
        {
            StringBuilder queryStr = new StringBuilder();
            if (input != null && input.Any())
            {
                foreach (var item in input)
                {
                    queryStr.Append(AKSNCaculater.UrlEncode(item.Key));
                    queryStr.Append("=");
                    queryStr.Append(AKSNCaculater.UrlEncode(item.Value));
                    queryStr.Append("&");
                }
                queryStr.Remove(queryStr.Length - 1, 1);
            }
            return queryStr.ToString();
        }
    }

    /// <summary>
    /// 百度地图API响应基类，包含所有API响应的公共字段
    /// </summary>
    public class BaiduMapApiResBase
    {
        /// <summary>
        /// 响应状态码，"0"表示成功，其他值表示失败
        /// </summary>
        public string status { get; set; }

        /// <summary>
        /// 响应消息，包含错误信息或成功提示
        /// </summary>
        public string message { get; set; }
    }
}
