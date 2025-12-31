using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace CrmConnection
{
    public class CrmServiceHelper
    {
        private readonly string ConnectionString;
        public IOrganizationService OrganizationService;
        private CrmConnectionInfo _config;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString"></param>
        public CrmServiceHelper(string connectionString)
        {
            this.ConnectionString = connectionString;

            _config = CrmConnectionInfo.Parse(this.ConnectionString);

            OrganizationService = CreateCrmServiceClient();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString"></param>
        public CrmServiceHelper(string connectionString, Guid userId)
        {
            this.ConnectionString = connectionString;

            _config = CrmConnectionInfo.Parse(this.ConnectionString);

            var crmserviceclient = CreateCrmServiceClient();
            if (userId != Guid.Empty)
                crmserviceclient.CallerId = userId;
            this.OrganizationService = crmserviceclient;
        }

        /// <summary>
        /// 生成组织服务
        /// </summary>
        /// <returns></returns>
        private CrmServiceClient CreateCrmServiceClient()
        {
            //var tokenCacheStorePath = GetTokenCacheStorePath();

            switch (_config.AuthenticationType)
            {
                case CrmAuthenticationTypes.AD:
                    {
                        #region ActiveDirectory
                        bool useSsl = _config.ServiceUri.Scheme == "https";
                        var crmServiceClient = new CrmServiceClient(
                        new NetworkCredential { Domain = _config.DomainName, UserName = _config.UserId, Password = _config.Password }
                        , _config.ServiceUri.Host, _config.ServiceUri.Port.ToString(), _config.ServiceUri.Segments[1], useUniqueInstance: false, useSsl: useSsl
                        );
                        return crmServiceClient;
                        #endregion
                    }
                case CrmAuthenticationTypes.IFD:
                    {
                        #region IFD Federation
                        //crmServiceClient = new CrmServiceClient(
                        //new NetworkCredential { Domain = config.Domain, UserName = userName, Password = config.Password }
                        //, Microsoft.Xrm.Tooling.Connector.AuthenticationType.IFD, uri.Host, uri.Port.ToString(), uri.Host.Split('.')[0], useUniqueInstance: false, useSsl: useSsl
                        //);
                        #endregion
                    }
                    break;
                case CrmAuthenticationTypes.OAuth:
                    {
                        StringBuilder sb = new StringBuilder();
                        //RequireNewInstance = true，不然会生成当前环境的组织服务
                        if (string.IsNullOrWhiteSpace(_config.GrantType))
                        {
                            sb.Append($"AuthType=OAuth;Url={_config.ServiceUri};UserName={_config.UserId};Password={_config.Password};AppId={_config.AppId};RedirectUri=app://;LoginPrompt=Auto;RequireNewInstance=true;");
                        }
                        else
                        {
                            sb.Append($"AuthType=ClientSecret;Url={_config.ServiceUri};ClientId={_config.ClientId};ClientSecret={_config.ClientSecret};AppId={_config.AppId};RedirectUri=app://;LoginPrompt=Auto;RequireNewInstance=true;");
                        }
                        var crmServiceClient = new CrmServiceClient(sb.ToString());
                        return crmServiceClient;
                    }
                case CrmAuthenticationTypes.Office365:
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append($"AuthType=ClientSecret;Url={_config.ServiceUri};ClientId={_config.ClientId};ClientSecret={_config.ClientSecret};AppId={_config.AppId};RedirectUri=app://;LoginPrompt=Auto;RequireNewInstance=true;");
                        //sb.Append($"AuthType=Office365;Url={config.OrganizationUri};Username={config.UserName};Password={config.Password};RequireNewInstance=false;");
                        var crmServiceClient = new CrmServiceClient(sb.ToString());
                        return crmServiceClient;
                    }
            }
            return null;
        }

        private string GetTokenCacheStorePath()
        {
            var baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TokenCacheDir");
            if (!Directory.Exists(baseDir))
            {
                Directory.CreateDirectory(baseDir);
            }
            return System.IO.Path.Combine(baseDir, "MyTokenCache");
        }
    }
}
