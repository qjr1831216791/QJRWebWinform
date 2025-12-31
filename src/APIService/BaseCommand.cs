using CommonHelper;
using CommonHelper.Model;
using CrmConnection;
using LogService.Service;
using Microsoft.Xrm.Sdk;
using RekTec.Crm.Common.Helper;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace APIService
{
    public class BaseCommand : IDisposable
    {
        /// <summary>
        /// 管理员组织服务
        /// </summary>
        public IOrganizationService OrganizationServiceAdmin { get { return _OrganizationServiceAdmin; } }

        public static IOrganizationService _OrganizationServiceAdmin = null;

        /// <summary>
        /// 组织服务
        /// </summary>
        public IOrganizationService OrganizationService { get { return _OrganizationService; } }

        private static IOrganizationService _OrganizationService = null;

        /// <summary>
        /// Web配置
        /// </summary>
        public WebConfigModel webConfig { get { return _webConfig; } }

        private static WebConfigModel _webConfig = WebConfigModel.Instance;

        /// <summary>
        /// 日志实例
        /// </summary>
        public SerilogService Log { get { return _log; } }

        private static SerilogService _log = SerilogService.GetInstance();

        /// <summary>
        /// 当前环境
        /// </summary>
        public string crmEnv { get { return _crmEnv; } }

        private static string _crmEnv = null;

        /// <summary>
        /// 上一次初始化组织服务的时间
        /// </summary>
        private static DateTime prvInitDate = DateTime.MinValue;

        /// <summary>
        /// 时区
        /// </summary>
        public TimeZoneInfo TimeZoneInfo { get { return this._TimeZoneInfo; } }

        private TimeZoneInfo _TimeZoneInfo = TimeZoneInfo.Local;

        /// <summary>
        /// 初始化组织服务
        /// </summary>
        public void Initialize(string crmEnv)
        {
            try
            {
                if (prvInitDate.Equals(DateTime.MinValue) || DateTime.Now.Subtract(prvInitDate).TotalMinutes > 45)
                {
                    //如果上一次初始化组织服务时间超过45分钟，则重新初始化组织服务
                    _OrganizationServiceAdmin = null;
                    _OrganizationService = null;
                }
                if (!string.IsNullOrWhiteSpace(crmEnv) && !crmEnv.Equals(_crmEnv, StringComparison.OrdinalIgnoreCase))
                {
                    //如果传入的环境与当前环境不同，则重新初始化组织服务
                    _OrganizationServiceAdmin = null;
                    _OrganizationService = null;
                }
                if (_OrganizationServiceAdmin == null || _OrganizationService == null)
                {
                    if (string.IsNullOrWhiteSpace(crmEnv))
                    {
                        //不指定目标环境，则按照RekTec.XStudio.CrmClient.Config.CrmConfig.xml初始化组织服务
                        WebConfigModel.LoadCrmClientConfigXmlConfig(out IOrganizationService _IOrganizationService, out IOrganizationService _IOrganizationServiceAdmin);
                        _OrganizationService = _IOrganizationService;
                        _OrganizationServiceAdmin = _IOrganizationServiceAdmin;
                    }
                    else
                    {
                        //指定目标环境，则按照RekTec.XStudio.CrmClient.Config.CrmConfig.json初始化组织服务
                        WebConfigModel.LoadCrmClientConfigJsonConfig(crmEnv, out IOrganizationService _IOrganizationService, out IOrganizationService _IOrganizationServiceAdmin);
                        _OrganizationService = _IOrganizationService;
                        _OrganizationServiceAdmin = _IOrganizationServiceAdmin;
                        _crmEnv = crmEnv;
                    }

                    //记录生成组织服务的时间
                    prvInitDate = DateTime.Now;
                }
            }
            catch (CryptographicException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 获取组织服务
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <param name="envirFromService"></param>
        /// <param name="envirToService"></param>
        protected void CreateCrmServic(string envirFrom, string envirTo, out IOrganizationService envirFromService, out IOrganizationService envirToService)
        {
            try
            {
                envirFromService = null;
                envirToService = null;
                string envirFromSP = null;
                string envirToSP = null;

                if (!string.IsNullOrWhiteSpace(crmEnv) && webConfig.environment.Any(e => e.key.Equals(crmEnv, StringComparison.OrdinalIgnoreCase)))
                {
                    var envs = webConfig.environment.FirstOrDefault(e => e.key.Equals(crmEnv, StringComparison.OrdinalIgnoreCase));

                    if (envs == null) throw new InvalidPluginExecutionException($"无效的CRM环境");

                    if (envs.env.ContainsKey(envirFrom))
                        envirFromSP = envs.env[envirFrom];
                    else
                        throw new InvalidPluginExecutionException($"组织服务envirFromService生成失败");

                    if (envs.env.ContainsKey(envirTo))
                        envirToSP = envs.env[envirTo];
                    else
                        throw new InvalidPluginExecutionException($"组织服务envirToService生成失败");
                }
                else
                {
                    envirFromSP = QueryHelper.GetSystemParameter(OrganizationServiceAdmin, $"SyncConfig.connetionString.{envirFrom}");
                    envirToSP = QueryHelper.GetSystemParameter(OrganizationServiceAdmin, $"SyncConfig.connetionString.{envirTo}");
                }

                envirFromSP = EncryptionHelper.DESDecryption(envirFromSP);
                envirToSP = EncryptionHelper.DESDecryption(envirToSP);

                if (!string.IsNullOrWhiteSpace(envirToSP))
                    envirToService = new CrmServiceHelper(envirToSP).OrganizationService;

                if (!string.IsNullOrWhiteSpace(envirFromSP))
                    envirFromService = new CrmServiceHelper(envirFromSP).OrganizationService;

                if (envirFromService == null) throw new InvalidPluginExecutionException($"组织服务envirFromService生成失败");
                else if (envirToService == null) throw new InvalidPluginExecutionException($"组织服务envirToService生成失败");
            }
            catch (CryptographicException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取组织服务
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <param name="envirFromService"></param>
        /// <param name="envirToService"></param>
        protected void CreateCrmServic(string envirFrom, out IOrganizationService envirFromService)
        {
            try
            {
                envirFromService = null;
                string envirFromSP = null;

                if (!string.IsNullOrWhiteSpace(crmEnv) && webConfig.environment.Any(e => e.key.Equals(crmEnv, StringComparison.OrdinalIgnoreCase)))
                {
                    var envs = webConfig.environment.FirstOrDefault(e => e.key.Equals(crmEnv, StringComparison.OrdinalIgnoreCase));

                    if (envs == null) throw new InvalidPluginExecutionException($"组织服务envirFromService生成失败");

                    if (envs.env.ContainsKey(envirFrom))
                        envirFromSP = envs.env[envirFrom];
                }
                else
                {
                    envirFromSP = QueryHelper.GetSystemParameter(OrganizationServiceAdmin, $"SyncConfig.connetionString.{envirFrom}");
                }

                envirFromSP = EncryptionHelper.DESDecryption(envirFromSP);

                if (!string.IsNullOrWhiteSpace(envirFromSP))
                    envirFromService = new CrmServiceHelper(envirFromSP).OrganizationService;

                if (envirFromService == null) throw new InvalidPluginExecutionException($"组织服务envirFromService生成失败");
            }
            catch (CryptographicException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Dispose()
        {

        }
    }
}
