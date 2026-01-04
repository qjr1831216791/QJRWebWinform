using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

namespace CrmConnection
{
    public class CrmConnectionInfo
    {
        /// <summary>
        /// 原始的连接字符串
        /// </summary>
        public string OriginalConnectionString { get; }

        /// <summary>
        /// URL of the Service being connected too.
        /// </summary>
        public Uri ServiceUri
        {
            get;
            internal set;
        }

        /// <summary>
        /// Authentication Type being used for this connection
        /// </summary>
        public CrmAuthenticationTypes AuthenticationType { get; private set; }

        /// <summary>
        /// OAuth Prompt behavior.
        /// </summary>
        public PromptBehavior PromptBehavior
        {
            get;
            internal set;
        }

        /// <summary>
        /// Claims based Delegated Authentication Url.
        /// </summary>
        public Uri HomeRealmUri
        {
            get;
            internal set;
        }

        /// <summary>
        /// OAuth User Identifier
        /// </summary>
        public UserIdentifier UserIdentifier
        {
            get;
            internal set;
        }

        /// <summary>
        /// Domain of User
        /// Active Directory Auth only.
        /// </summary>
        public string DomainName
        {
            get;
            internal set;
        }

        /// <summary>
        /// User ID of the User connection to CRM
        /// </summary>
        public string UserId
        {
            get;
            internal set;
        }

        /// <summary>
        /// Password of user, parsed from connection string
        /// </summary>
        public string Password
        {
            get;
            set;
        }

        /// <summary>
        /// Client ID used in the connection string
        /// </summary>
        public string ClientId
        {
            get;
            internal set;
        }

        /// <summary>
        /// Client对应的密钥
        /// </summary>
        public string ClientSecret { get; private set; }

        /// <summary>
        /// Organization Name parsed from the connection string.
        /// </summary>
        public string Organization
        {
            get;
            internal set;
        }

        /// <summary>
        /// Set if the connection string is for an onPremise connection
        /// </summary>
        public bool IsOnPremOauth
        {
            get;
            internal set;
        }

        /// <summary>
        /// CRM on-line region determined by the connection string
        /// </summary>
        public string CrmOnlineRegion
        {
            get;
            internal set;
        }

        /// <summary>
        /// OAuth Redirect URI
        /// </summary>
        public Uri RedirectUri
        {
            get;
            internal set;
        }

        /// <summary>
        /// OAuth Token Store Path
        /// </summary>
        public string TokenCacheStorePath
        {
            get;
            internal set;
        }

        /// <summary>
        /// 采用OAuth认证时，认证服务器的Token结点。例： https://dev.recloud.cc:9982/adfs/oauth2/token
        /// </summary>
        public string TokenEndPoint { get; private set; }

        /// <summary>
        /// 认证服务器的OAuth2/Resource
        /// </summary>
        public string Resource { get; private set; }

        /// <summary>
        /// When true, specifies a unique instance of the connection should be created. 
        /// </summary>
        public bool UseUniqueConnectionInstance
        {
            get;
            internal set;
        }

        /// <summary>
        /// 认证服务器上的LoginPrompt.
        /// </summary>
        public readonly string LoginPrompt;

        /// <summary>
        /// Oauth2.0 授权协议
        /// </summary>
        public readonly string GrantType;

        /// <summary>
        /// 认证服务器上的AppId.
        /// </summary>
        public readonly string AppId;

        private static bool IsTrueString(string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                return s.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                    s.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                    s.Equals("1", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        private AuthenticationProviderType GetEndpointType()
        {
            switch (this.AuthenticationType)
            {
                case CrmAuthenticationTypes.AD:
                    return AuthenticationProviderType.ActiveDirectory;
                case CrmAuthenticationTypes.IFD:
                    return AuthenticationProviderType.Federation;
                case CrmAuthenticationTypes.Claims:
                    return AuthenticationProviderType.Federation;
                case CrmAuthenticationTypes.Live:
                    return AuthenticationProviderType.LiveId;
                case CrmAuthenticationTypes.Office365:
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    return AuthenticationProviderType.OnlineFederation;
                case CrmAuthenticationTypes.OAuth:
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    return AuthenticationProviderType.OnlineFederation;
                default:
                    throw new Exception("暂不支持认证类型:" + this.AuthenticationType.ToString());
            }
        }

        public CrmConnectionInfo()
        {
        }

        private CrmConnectionInfo(string originalConnectionString, IDictionary<string, string> connection) :
            this(connection.FirstNotNullOrEmpty(DynamicCrmConstants.ServiceUri),
                connection.FirstNotNullOrEmpty(DynamicCrmConstants.UserName),
                connection.FirstNotNullOrEmpty(DynamicCrmConstants.Password),
                connection.FirstNotNullOrEmpty(DynamicCrmConstants.Domain),
                connection.FirstNotNullOrEmpty(DynamicCrmConstants.HomeRealmUri),
                connection.FirstNotNullOrEmpty(DynamicCrmConstants.AuthType),
                connection.FirstNotNullOrEmpty(DynamicCrmConstants.RequireNewInstance),
                connection.FirstNotNullOrEmpty(DynamicCrmConstants.ClientId),
                connection.FirstNotNullOrEmpty(DynamicCrmConstants.RedirectUri),
                connection.FirstNotNullOrEmpty(DynamicCrmConstants.TokenCacheStorePath),
                connection.FirstNotNullOrEmpty(DynamicCrmConstants.LoginPrompt),
                connection.FirstNotNullOrEmpty(DynamicCrmConstants.TokenEndPoint),
                connection.FirstNotNullOrEmpty(DynamicCrmConstants.ClientSecret),
                connection.FirstNotNullOrEmpty(DynamicCrmConstants.Resource),
                originalConnectionString,
                connection.FirstNotNullOrEmpty(DynamicCrmConstants.AppId),
                connection.FirstNotNullOrEmpty(DynamicCrmConstants.GrantType))
        {
        }

        private CrmConnectionInfo(string serviceUri, string userName, string password, string domain, string homeRealmUri, string authType, string requireNewInstance, string clientId, string redirectUri, string tokenCacheStorePath, string loginPrompt, string tokenEndpoint,
            string clientSecret, string resource, string originalConnectionString, string appId, string grantType)
        {
            this.OriginalConnectionString = originalConnectionString;

            Uri uri;
            this.ServiceUri = (this.GetValidUri(serviceUri, out uri) ? uri : null);
            Uri uri2;
            this.HomeRealmUri = (this.GetValidUri(homeRealmUri, out uri2) ? uri2 : null);
            this.DomainName = ((!string.IsNullOrWhiteSpace(domain)) ? domain : string.Empty);
            this.UserId = ((!string.IsNullOrWhiteSpace(userName)) ? userName : string.Empty);
            this.Password = ((!string.IsNullOrWhiteSpace(password)) ? password : string.Empty);
            this.ClientId = ((!string.IsNullOrWhiteSpace(clientId)) ? clientId : string.Empty);
            this.TokenCacheStorePath = ((!string.IsNullOrWhiteSpace(tokenCacheStorePath)) ? tokenCacheStorePath : string.Empty);
            this.RedirectUri = (Uri.IsWellFormedUriString(redirectUri, UriKind.RelativeOrAbsolute) ? new Uri(redirectUri) : null);
            bool useUniqueConnectionInstance = true;
            bool.TryParse(requireNewInstance, out useUniqueConnectionInstance);
            this.UseUniqueConnectionInstance = useUniqueConnectionInstance;
            this.UserIdentifier = ((!string.IsNullOrWhiteSpace(this.UserId)) ? new UserIdentifier(this.UserId, UserIdentifierType.OptionalDisplayableId) : null);
            this.TokenEndPoint = tokenEndpoint;
            this.ClientSecret = clientSecret;

            this.Resource = resource;

            this.AppId = appId;
            this.LoginPrompt = loginPrompt;
            this.GrantType = grantType;

            CrmAuthenticationTypes authenticationType;
            if (Enum.TryParse<CrmAuthenticationTypes>(authType, out authenticationType))
            {
                this.AuthenticationType = authenticationType;
            }
            else
            {
                this.AuthenticationType = CrmAuthenticationTypes.AD;
            }
            PromptBehavior promptBehavior;
            if (Enum.TryParse<PromptBehavior>(loginPrompt, out promptBehavior))
            {
                this.PromptBehavior = promptBehavior;
            }
            else
            {
                this.PromptBehavior = PromptBehavior.Auto;
            }
            if (this.ServiceUri != null)
            {
                this.SetOrgnameAndOnlineRegion(this.ServiceUri);
            }
            if (!string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(password))
            {
                
            }

            if (userName.Contains(@"\"))
            {
                var pos = userName.IndexOf('\\');
                this.UserId = userName.Substring(pos + 1);
                if (!string.IsNullOrEmpty(domain))
                {
                    this.DomainName = userName.Substring(0, pos);
                }
            }


            if (this.ServiceUri != null && this.AuthenticationType == CrmAuthenticationTypes.IFD)
            {
                if (this.ServiceUri.Segments.Length > 0)
                {
                    var sb = new StringBuilder(256);
                    sb.Append(this.ServiceUri.Scheme).Append("://");
                    sb.Append(this.ServiceUri.Host);
                    sb.Append(":").Append(this.ServiceUri.Port);
                    this.ServiceUri = new Uri(sb.ToString());
                }
            }
        }

        /// <summary>
        /// 获取Web API的前缀URL
        /// </summary>
        /// <returns>格式化的Web API URL前缀，例如：https://org.crm.dynamics.com/api/data/v9.0</returns>
        public string GetApiPrefix()
        {
            var prefix = this.ServiceUri.ToString();
            if (!prefix.EndsWith("/"))
            {
                prefix += "/";
            }
            return prefix + "api/data/v9.0";
        }

        /// <summary>
        /// 获取网络凭据对象，用于Active Directory认证
        /// </summary>
        /// <returns>包含用户名、密码和域名的NetworkCredential对象</returns>
        public System.Net.NetworkCredential GetNetworkCredential()
        {
            return new System.Net.NetworkCredential(this.UserId, this.Password, this.DomainName);
        }

        /// <summary>
        /// 验证URI字符串是否有效（必须是HTTP或HTTPS协议的绝对URI）
        /// </summary>
        /// <param name="uriSource">要验证的URI字符串</param>
        /// <param name="validUriResult">验证成功后的Uri对象</param>
        /// <returns>如果URI有效则返回true，否则返回false</returns>
        private bool GetValidUri(string uriSource, out Uri validUriResult)
        {
            return Uri.TryCreate(uriSource, UriKind.Absolute, out validUriResult) && (validUriResult.Scheme == Uri.UriSchemeHttp || validUriResult.Scheme == Uri.UriSchemeHttps);
        }

        /// <summary>
        /// Get the organization name and online region from the org
        /// </summary>
        /// <param name="serviceUri"></param>
        private void SetOrgnameAndOnlineRegion(Uri serviceUri)
        {
            string empty = string.Empty;
            string empty2 = string.Empty;
            bool isOnPremOauth = false;
            Utilities.GetOrgnameAndOnlineRegionFromServiceUri(serviceUri, out empty, out empty2, out isOnPremOauth);
            this.CrmOnlineRegion = empty;
            this.Organization = empty2;
            this.IsOnPremOauth = isOnPremOauth;
        }

        /// <summary>
        /// 解析连接字符串并创建CrmConnectionInfo对象
        /// </summary>
        /// <param name="connectionString">CRM连接字符串</param>
        /// <returns>解析后的CrmConnectionInfo对象</returns>
        public static CrmConnectionInfo Parse(string connectionString)
        {
            return new CrmConnectionInfo(connectionString, ToDictionary(connectionString));
        }

        /// <summary>
        /// 将连接字符串转换为字典格式
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <returns>包含连接字符串键值对的字典，如果解析失败则返回空字典</returns>
        private static IDictionary<string, string> ToDictionary(string connectionString)
        {
            try
            {
                DbConnectionStringBuilder source = new DbConnectionStringBuilder
                {
                    ConnectionString = connectionString
                };
                Dictionary<string, string> dictionary = source.Cast<KeyValuePair<string, object>>().ToDictionary((KeyValuePair<string, object> pair) => pair.Key, delegate (KeyValuePair<string, object> pair)
                {
                    if (pair.Value == null)
                    {
                        return string.Empty;
                    }
                    return pair.Value.ToString();
                });
                return new Dictionary<string, string>(dictionary, StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
            }
            return new Dictionary<string, string>();
        }

        #region DynamicCrmConstants
        private static class DynamicCrmConstants
        {
            public static readonly string[] ServiceUri = new string[]
            {
            "ServiceUri",
            "Service Uri",
            "Url",
            "Server"
            };

            public static readonly string[] UserName = new string[]
            {
            "UserName",
            "User Name",
            "UserId",
            "User Id"
            };

            public static readonly string[] Password = new string[]
            {
            "Password"
            };

            public static readonly string[] Domain = new string[]
            {
            "Domain"
            };

            public static readonly string[] HomeRealmUri = new string[]
            {
            "HomeRealmUri",
            "Home Realm Uri"
            };

            public static readonly string[] AuthType = new string[]
            {
            "AuthType",
            "AuthenticationType"
            };

            public static readonly string[] RequireNewInstance = new string[]
            {
            "RequireNewInstance"
            };

            public static readonly string[] ClientId = new string[]
            {
            "ClientId",
            "AppId",
            "ApplicationId"
            };

            public static readonly string[] RedirectUri = new string[]
            {
            "RedirectUri",
            "ReplyUrl"
            };

            public static readonly string[] TokenCacheStorePath = new string[]
            {
            "TokenCacheStorePath"
            };

            public static readonly string[] LoginPrompt = new string[]
            {
            "LoginPrompt"
            };

            public static readonly string[] TokenEndPoint = new string[]
            {
                "TokenEndPoint"
            };

            public static readonly string[] Resource = new string[]
            {
                "Resource"
            };

            public static readonly string[] ClientSecret = new string[]
            {
                "ClientSecret"
            };

            public static readonly string[] AppId = new string[]
            {
                "AppId"
            };

            public static readonly string[] GrantType = new string[]
            {
                "GrantType"
            };
        }
        #endregion

        private static class Utilities
        {
            /// <summary>
            /// Get the organization name and on-line region from the Uri
            /// </summary>
            /// <param name="serviceUri">Service Uri to parse</param>
            /// <param name="isOnPrem">if OnPrem, will be set to true, else false.</param>
            /// <param name="onlineRegion">Name of the CRM on line Region serving this request</param>
            /// <param name="organizationName">Name of the Organization extracted from the Service URI</param>
            public static void GetOrgnameAndOnlineRegionFromServiceUri(Uri serviceUri, out string onlineRegion, out string organizationName, out bool isOnPrem)
            {
                isOnPrem = false;
                onlineRegion = string.Empty;
                organizationName = string.Empty;
                if (serviceUri.Host.ToUpperInvariant().Contains("DYNAMICS.COM") || serviceUri.Host.ToUpperInvariant().Contains("MICROSOFTDYNAMICS.DE") || serviceUri.Host.ToUpperInvariant().Contains("DYNAMICS-INT.COM"))
                {
                    //throw new InvalidOperationException("不支持在线模式");
                    List<string> list = new List<string>(serviceUri.Host.Split(new string[]
                    {
                    "."
                    }, StringSplitOptions.RemoveEmptyEntries));
                    organizationName = list[0];
                    list.RemoveAt(0);
                    //CrmOnlineDiscoveryServers crmOnlineDiscoveryServers = new CrmOnlineDiscoveryServers();
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (string current in list)
                    {
                        if (!current.Equals("api"))
                        {
                            stringBuilder.AppendFormat("{0}.", current);
                        }
                    }
                    string crmKey = stringBuilder.ToString().TrimEnd(new char[]
                    {
                    '.'
                    }).TrimEnd(new char[]
                    {
                    '/'
                    });
                    stringBuilder.Clear();
                    if (!string.IsNullOrEmpty(crmKey))
                    {
                        //CrmOnlineDiscoveryServer crmOnlineDiscoveryServer = (from w in crmOnlineDiscoveryServers.OSDPServers
                        //                                                     where w.DiscoveryServer != null && w.DiscoveryServer.Host.Contains(crmKey)
                        //                                                     select w).FirstOrDefault<CrmOnlineDiscoveryServer>();
                        //if (crmOnlineDiscoveryServer != null && !string.IsNullOrEmpty(crmOnlineDiscoveryServer.ShortName))
                        //{
                        //    onlineRegion = crmOnlineDiscoveryServer.ShortName;
                        //}
                    }
                    isOnPrem = false;
                    return;
                }
                isOnPrem = true;
                if (serviceUri.Segments.Count<string>() >= 2)
                {
                    organizationName = serviceUri.Segments[1].TrimEnd(new char[]
                    {
                    '/'
                    });
                }
            }
        }
    }

    /// <summary>
    /// 扩展方法类，提供字符串和枚举的扩展功能
    /// </summary>
    internal static class Extension
    {
        /// <summary>
        /// 将字符串转换为指定类型的枚举值
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="enumName">枚举名称字符串</param>
        /// <returns>转换后的枚举值</returns>
        public static T ToEnum<T>(this string enumName)
        {
            return (T)((object)Enum.Parse(typeof(T), enumName));
        }

        /// <summary>
        /// 将整数值转换为指定类型的枚举值
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="enumValue">枚举的整数值</param>
        /// <returns>转换后的枚举值</returns>
        public static T ToEnum<T>(this int enumValue)
        {
            return enumValue.ToString().ToEnum<T>();
        }

        /// <summary>
        /// 将连接字符串转换为字典格式
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <returns>包含连接字符串键值对的字典，如果解析失败则返回空字典</returns>
        public static IDictionary<string, string> ToDictionary(this string connectionString)
        {
            try
            {
                DbConnectionStringBuilder source = new DbConnectionStringBuilder
                {
                    ConnectionString = connectionString
                };
                Dictionary<string, string> dictionary = source.Cast<KeyValuePair<string, object>>().ToDictionary((KeyValuePair<string, object> pair) => pair.Key, delegate (KeyValuePair<string, object> pair)
                {
                    if (pair.Value == null)
                    {
                        return string.Empty;
                    }
                    return pair.Value.ToString();
                });
                return new Dictionary<string, string>(dictionary, StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
            }
            return new Dictionary<string, string>();
        }

        /// <summary>
        /// 使用指定的参数格式化字符串
        /// </summary>
        /// <param name="format">格式化字符串</param>
        /// <param name="args">格式化参数</param>
        /// <returns>格式化后的字符串</returns>
        public static string FormatWith(this string format, params object[] args)
        {
            return format.FormatWith(new object[]
            {
                CultureInfo.InvariantCulture,
                args
            });
        }

        /// <summary>
        /// 从字典中获取第一个非空且非空字符串的值
        /// </summary>
        /// <typeparam name="TKey">字典键的类型</typeparam>
        /// <param name="dictionary">字典对象</param>
        /// <param name="keys">要查找的键数组，按顺序查找</param>
        /// <returns>找到的第一个非空值，如果所有键都不存在或值为空则返回null</returns>
        public static string FirstNotNullOrEmpty<TKey>(this IDictionary<TKey, string> dictionary, params TKey[] keys)
        {
            return (from key in keys
                    where dictionary.ContainsKey(key) && !string.IsNullOrEmpty(dictionary[key])
                    select dictionary[key]).FirstOrDefault<string>();
        }
    }

    /// <summary>
    /// CRM认证类型枚举
    /// </summary>
    public enum CrmAuthenticationTypes
    {
        /// <summary>
        /// Active Directory认证
        /// </summary>
        AD = 0,
        /// <summary>
        /// Live认证
        /// </summary>
        Live = 1,
        /// <summary>
        /// SPLA认证（IFD）
        /// </summary>
        IFD = 2,
        /// <summary>
        /// 基于声明的认证
        /// </summary>
        Claims = 3,
        /// <summary>
        /// Office365登录流程
        /// </summary>
        Office365 = 4,
        /// <summary>
        /// OAuth认证
        /// </summary>
        OAuth = 5,
        /// <summary>
        /// 无效连接
        /// </summary>
        InvalidConnection = -1
    }
}
