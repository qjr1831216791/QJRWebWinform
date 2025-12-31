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
    public class BaiduMapAPIBase
    {
        public BaiduMapConfig config { get; set; } = BaiduMapConfig.GetInstance();

        public readonly string APIUrl = "https://api.map.baidu.com";

        /// <summary>
        /// 接口名称
        /// </summary>
        public string apiName { get; set; }

        /// <summary>
        /// HttpMethod
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
        /// 执行接口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
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
        /// 执行接口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public virtual ResultModel Execute<T>(object data)
        {
            if (this.HttpMethod.Equals(HttpMethod.Get)) return Get<T>((Dictionary<string, string>)data);
            else throw new Exception("未知的HttpMethod！");
        }

        /// <summary>
        /// 获取QueryString
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
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

    public class BaiduMapApiResBase
    {
        public string status { get; set; }

        public string message { get; set; }
    }
}
