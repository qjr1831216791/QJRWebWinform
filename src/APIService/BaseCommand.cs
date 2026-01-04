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
        /// 管理员组织服务，具有管理员权限的CRM组织服务实例
        /// </summary>
        public IOrganizationService OrganizationServiceAdmin { get { return _OrganizationServiceAdmin; } }

        /// <summary>
        /// 静态管理员组织服务实例
        /// </summary>
        public static IOrganizationService _OrganizationServiceAdmin = null;

        /// <summary>
        /// 组织服务，普通权限的CRM组织服务实例
        /// </summary>
        public IOrganizationService OrganizationService { get { return _OrganizationService; } }

        /// <summary>
        /// 静态组织服务实例
        /// </summary>
        private static IOrganizationService _OrganizationService = null;

        /// <summary>
        /// Web配置模型实例，包含环境配置等信息
        /// </summary>
        public WebConfigModel webConfig { get { return _webConfig; } }

        /// <summary>
        /// 静态Web配置模型实例
        /// </summary>
        private static WebConfigModel _webConfig = WebConfigModel.Instance;

        /// <summary>
        /// 日志服务实例，用于记录日志
        /// </summary>
        public SerilogService Log { get { return _log; } }

        /// <summary>
        /// 静态日志服务实例
        /// </summary>
        private static SerilogService _log = SerilogService.GetInstance();

        /// <summary>
        /// 当前CRM环境标识
        /// </summary>
        public string crmEnv { get { return _crmEnv; } }

        /// <summary>
        /// 静态当前环境标识
        /// </summary>
        private static string _crmEnv = null;

        /// <summary>
        /// 上一次初始化组织服务的时间，用于判断是否需要重新初始化
        /// </summary>
        private static DateTime prvInitDate = DateTime.MinValue;

        /// <summary>
        /// 时区信息，用于时间转换
        /// </summary>
        public TimeZoneInfo TimeZoneInfo { get { return this._TimeZoneInfo; } }

        /// <summary>
        /// 时区信息实例，默认为本地时区
        /// </summary>
        private TimeZoneInfo _TimeZoneInfo = TimeZoneInfo.Local;

        /// <summary>
        /// 初始化组织服务，根据指定的CRM环境创建或重用组织服务实例
        /// </summary>
        /// <param name="crmEnv">CRM环境标识，如果为空则使用默认配置</param>
        /// <remarks>
        /// 如果距离上次初始化超过45分钟，或环境标识发生变化，将重新初始化组织服务
        /// </remarks>
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
        /// 创建两个环境的CRM组织服务，用于环境间的数据同步
        /// </summary>
        /// <param name="envirFrom">源环境标识</param>
        /// <param name="envirTo">目标环境标识</param>
        /// <param name="envirFromService">输出的源环境组织服务</param>
        /// <param name="envirToService">输出的目标环境组织服务</param>
        /// <exception cref="InvalidPluginExecutionException">当环境配置无效或组织服务创建失败时抛出</exception>
        /// <exception cref="CryptographicException">当连接字符串解密失败时抛出</exception>
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
        /// 创建指定环境的CRM组织服务
        /// </summary>
        /// <param name="envirFrom">环境标识</param>
        /// <param name="envirFromService">输出的组织服务</param>
        /// <exception cref="InvalidPluginExecutionException">当环境配置无效或组织服务创建失败时抛出</exception>
        /// <exception cref="CryptographicException">当连接字符串解密失败时抛出</exception>
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

        /// <summary>
        /// 释放资源，实现IDisposable接口
        /// </summary>
        public void Dispose()
        {

        }
    }
}
