using CommonHelper.Model;
using System;
using System.IO;
using System.Xml;

namespace ThinkAPIService.Model
{
    public class ThinkAPIConfig
    {
        private string _AppCode { get; set; }

        public string AppCode
        {
            get
            {
                return _AppCode;
            }
        }

        private string _AuthToken { get; set; }

        public string AuthToken
        {
            get
            {
                return _AuthToken;
            }
        }

        private ThinkAPIConfig()
        {
            this.LoadConfig();
        }

        private static readonly object syncRoot = new object();

        private static ThinkAPIConfig instance = new ThinkAPIConfig();

        /// <summary>
        /// 获取实例
        /// </summary>
        /// <returns></returns>
        public static ThinkAPIConfig GetInstance()
        {
            // 双重检查锁定
            if (instance == null)
            {
                lock (syncRoot)
                {
                    if (instance == null)
                    {
                        instance = new ThinkAPIConfig();
                    }
                }
            }

            return instance;
        }

        /// <summary>
        /// 读取配置文件
        /// </summary>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="Exception"></exception>
        private void LoadConfig()
        {
            XmlDocument doc = new XmlDocument();
            string path = $"{AppDomain.CurrentDomain.BaseDirectory}/Config/{ConfigFileNameEnum.ThinkAPIConfig}";
            if (File.Exists(path))
            {
                doc.Load(path);
            }
            else
            {
                throw new FileNotFoundException($"配置文件缺失：{ConfigFileNameEnum.ThinkAPIConfig}");
            }

            XmlNodeList AppCode = doc.DocumentElement.SelectNodes("/Configuration/AppCode");
            if (AppCode is null || AppCode.Count == 0) throw new Exception("无法获取ThinkAPI授权码！");
            this._AppCode = AppCode[0].InnerText;

            XmlNodeList AuthToken = doc.DocumentElement.SelectNodes("/Configuration/AuthToken");
            if (AuthToken is null || AuthToken.Count == 0) throw new Exception("无法获取ThinkAPI授权码！");
            this._AuthToken = AuthToken[0].InnerText;
        }
    }
}
