using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace BaiduMapService.Service
{
    /// <summary>
    /// SN加密方法
    /// </summary>
    public class AKSNCaculater
    {
        private static string MD5(string password)
        {
            try
            {
                System.Security.Cryptography.HashAlgorithm hash = System.Security.Cryptography.MD5.Create();
                byte[] hash_out = hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

                var md5_str = BitConverter.ToString(hash_out).Replace("-", "");
                return md5_str.ToLower();

            }
            catch
            {

                throw;
            }
        }

        public static string UrlEncode(string str)
        {
            str = HttpUtility.UrlEncode(str);
            byte[] buf = Encoding.ASCII.GetBytes(str);//等同于Encoding.ASCII.GetBytes(str)
            for (int i = 0; i < buf.Length; i++)
                if (buf[i] == '%')
                {
                    if (buf[i + 1] >= 'a') buf[i + 1] -= 32;
                    if (buf[i + 2] >= 'a') buf[i + 2] -= 32;
                    i += 2;
                }
            return Encoding.ASCII.GetString(buf);//同上，等同于Encoding.ASCII.GetString(buf)
        }

        private static string HttpBuildQuery(IDictionary<string, string> querystring_arrays)
        {
            StringBuilder sb = new StringBuilder();
            if (querystring_arrays != null && querystring_arrays.Any())
            {
                foreach (var item in querystring_arrays)
                {
                    sb.Append(UrlEncode(item.Key));
                    sb.Append("=");
                    sb.Append(UrlEncode(item.Value));
                    sb.Append("&");
                }
                sb.Remove(sb.Length - 1, 1);
            }
            return sb.ToString();
        }

        public static string CaculateAKSN(string ak, string sk, string url, IDictionary<string, string> querystring_arrays)
        {
            var queryString = HttpBuildQuery(querystring_arrays);

            var str = UrlEncode(url + "?" + queryString + sk);

            return MD5(str);
        }
    }
}
