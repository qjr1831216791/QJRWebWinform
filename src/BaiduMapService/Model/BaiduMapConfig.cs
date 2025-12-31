using BaiduMapService.Service;
using CommonHelper.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace BaiduMapService.Model
{
    public class BaiduMapConfig
    {
        private string SK { get; set; }

        private string _AK { get; set; }

        public string AK
        {
            get
            {
                return _AK;
            }
        }

        private BaiduMapConfig()
        {
            this.LoadConfig();
        }

        private static readonly object syncRoot = new object();

        private static BaiduMapConfig instance = new BaiduMapConfig();

        /// <summary>
        /// 获取实例
        /// </summary>
        /// <returns></returns>
        public static BaiduMapConfig GetInstance()
        {
            // 双重检查锁定
            if (instance == null)
            {
                lock (syncRoot)
                {
                    if (instance == null)
                    {
                        instance = new BaiduMapConfig();
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
            string path = $"{AppDomain.CurrentDomain.BaseDirectory}/Config/{ConfigFileNameEnum.BaiduMapConfig}";
            if (File.Exists(path))
            {
                doc.Load(path);
            }
            else
            {
                throw new FileNotFoundException($"配置文件缺失：{ConfigFileNameEnum.CrmConfig}");
            }

            XmlNodeList AK = doc.DocumentElement.SelectNodes("/Configuration/AK");
            if (AK is null || AK.Count == 0) throw new Exception("无法获取百度地图AK！");
            this._AK = AK[0].InnerText;

            XmlNodeList SK = doc.DocumentElement.SelectNodes("/Configuration/SK");
            if (SK is null || SK.Count == 0) throw new Exception("无法获取百度地图SK！");
            this.SK = SK[0].InnerText;
        }

        /// <summary>
        /// 计算SN
        /// </summary>
        /// <param name="url"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public string CaculateAKSN(string url, Dictionary<string, string> input)
        {
            return AKSNCaculater.CaculateAKSN(AK, SK, url, input);
        }
    }
}
