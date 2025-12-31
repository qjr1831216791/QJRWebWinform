using CrmConnection;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RekTec.Crm.Common.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Xml;

namespace CommonHelper.Model
{
    /// <summary>
    /// Web配置类-单例模式
    /// </summary>
    public class WebConfigModel
    {
        private static WebConfigModel _instance;

        private WebConfigModel()
        {
            string path = $"{AppDomain.CurrentDomain.BaseDirectory}/Config/{ConfigFileNameEnum.CrmConfigJSON}";

            #region 读取环境配置文件
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                this.environment = JsonConvert.DeserializeObject<List<EnvironmentConfigModel>>(json);
            }
            else
            {
                throw new FileNotFoundException($"配置文件缺失：{ConfigFileNameEnum.CrmConfigJSON}");
            }
            #endregion
        }

        public static WebConfigModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new WebConfigModel();
                }
                return _instance;
            }
        }

        public List<EnvironmentConfigModel> environment { get; } = new List<EnvironmentConfigModel>();

        /// <summary>
        /// 按照加密连接字符串生成组织服务
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="OrganizationService"></param>
        /// <param name="OrganizationServiceAdmin"></param>
        /// <exception cref="Exception"></exception>
        public static void CreateCrmOrganizationService(string connectionString, out IOrganizationService OrganizationService, out IOrganizationService OrganizationServiceAdmin)
        {
            OrganizationService = null;
            OrganizationServiceAdmin = null;

            if (string.IsNullOrWhiteSpace(connectionString)) throw new Exception("无法获取组织服务连接字符串！");
            connectionString = EncryptionHelper.DESDecryption(connectionString);

            if (!string.IsNullOrWhiteSpace(connectionString))
                OrganizationService = new CrmServiceHelper(connectionString).OrganizationService;

            if (OrganizationService == null) throw new Exception($"组织服务生成失败");

            string crmadmin = QueryHelper.GetSystemParameter(OrganizationService, "crmadmin");
            Entity crmadminUser = QueryHelper.QueryEntity(OrganizationService, "systemuser", crmadmin, "domainname");
            if (crmadminUser == null) throw new Exception("无法获取系统管理员信息！");
            OrganizationServiceAdmin = new CrmServiceHelper(connectionString, crmadminUser.Id).OrganizationService;

            if (OrganizationServiceAdmin == null) throw new Exception($"组织服务生成失败");
        }

        /// <summary>
        /// 读取RekTec.XStudio.CrmClient.Config.CrmConfig.xml.config并生成组织服务
        /// </summary>
        /// <param name="OrganizationService"></param>
        /// <param name="OrganizationServiceAdmin"></param>
        /// <exception cref="Exception"></exception>
        public static void LoadCrmClientConfigXmlConfig(out IOrganizationService OrganizationService, out IOrganizationService OrganizationServiceAdmin)
        {
            OrganizationService = null;
            OrganizationServiceAdmin = null;

            XmlDocument doc = new XmlDocument();
            string path = $"{AppDomain.CurrentDomain.BaseDirectory}/Config/{ConfigFileNameEnum.CrmConfig}";
            if (File.Exists(path))
            {
                doc.Load(path);
            }
            else
            {
                throw new FileNotFoundException($"配置文件缺失：{ConfigFileNameEnum.CrmConfig}");
            }

            XmlNodeList nodes = doc.DocumentElement.SelectNodes("/CrmConfig/ConnectionString");
            if (nodes is null || nodes.Count == 0) throw new Exception("无法获取组织服务连接字符串！");

            CreateCrmOrganizationService(nodes[0].InnerText, out OrganizationService, out OrganizationServiceAdmin);
        }

        /// <summary>
        /// 读取Environments.CrmClient.Config.CrmConfig.json
        /// </summary>
        /// <param name="OrganizationService"></param>
        /// <param name="OrganizationServiceAdmin"></param>
        public static void LoadCrmClientConfigJsonConfig(string key, out IOrganizationService OrganizationService, out IOrganizationService OrganizationServiceAdmin)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new Exception("CRM环境Key不能为空！");

            OrganizationService = null;
            OrganizationServiceAdmin = null;

            CreateCrmOrganizationService(Instance.environment.FirstOrDefault(x => x.key.Equals(key, StringComparison.OrdinalIgnoreCase))?.defaultEnvConnectionString ?? string.Empty, out OrganizationService, out OrganizationServiceAdmin);
        }
    }

    /// <summary>
    /// 多环境配置类-只读
    /// </summary>
    public class EnvironmentConfigModel
    {
        public string key { get; }

        public string name { get; }

        public string defaultEnv { get; }

        public string defaultEnvConnectionString
        {
            get
            {
                if (string.IsNullOrWhiteSpace(defaultEnv)) return string.Empty;
                else if (env == null || !env.Any() || !env.ContainsKey(defaultEnv)) return string.Empty;

                return env[defaultEnv];
            }
        }

        public Dictionary<string, string> env { get; } = new Dictionary<string, string>();

        public EnvironmentConfigModel(string key, string name, string defaultEnv, Dictionary<string, string> env)
        {
            this.key = key;
            this.name = name;
            this.defaultEnv = defaultEnv;

            if (env != null && env.Any())
                this.env = env;
        }
    }
}
