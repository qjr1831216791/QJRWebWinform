using CommonHelper.Model;
using System;
using System.IO;
using System.Xml;

namespace LogService.Model
{
    public class LogConfig
    {
        private string _TracePath { get; set; }

        /// <summary>
        /// 日志文件路径
        /// </summary>
        public string TracePath
        {
            get
            {
                return _TracePath;
            }
        }

        private int _KeepLogDays { get; set; }

        /// <summary>
        /// 日志保留天数
        /// </summary>
        public int KeepLogDays
        {
            get
            {
                return _KeepLogDays;
            }
        }

        private LogConfig()
        {
            this.LoadConfig();
        }

        private static readonly object syncRoot = new object();

        private static LogConfig instance = new LogConfig();

        /// <summary>
        /// 获取实例
        /// </summary>
        /// <returns></returns>
        public static LogConfig GetInstance()
        {
            // 双重检查锁定
            if (instance == null)
            {
                lock (syncRoot)
                {
                    if (instance == null)
                    {
                        instance = new LogConfig();
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
            string path = $"{AppDomain.CurrentDomain.BaseDirectory}/Config/{ConfigFileNameEnum.LogConfig}";
            if (File.Exists(path))
            {
                doc.Load(path);
            }
            else
            {
                throw new FileNotFoundException($"配置文件缺失：{ConfigFileNameEnum.LogConfig}");
            }

            XmlNodeList TracePath = doc.DocumentElement.SelectNodes("/Configuration/TracePath");
            if (TracePath is null || TracePath.Count == 0) throw new Exception("无法获取Log文件路径！");
            this._TracePath = TracePath[0].InnerText;

            XmlNodeList KeepLogDays = doc.DocumentElement.SelectNodes("/Configuration/KeepLogDays");
            if (KeepLogDays is null || KeepLogDays.Count == 0) throw new Exception("无法获取Log文件保留天数！");
            if (int.TryParse(KeepLogDays[0].InnerText, out int t_KeepLogDays))
            {
                _KeepLogDays = t_KeepLogDays;
            }
            else
            {
                _KeepLogDays = 7;
            }
        }
    }
}
