using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using RekTec.Crm.Common.Helper;
using CommonHelper.Model;
using Newtonsoft.Json;

namespace CommonHelper
{
    public static class QueryHelper
    {
        /// <summary>
        /// 获取系统参数
        /// </summary>
        /// <param name="OrganizationServiceAdmin">管理员组织服务</param>
        /// <param name="parametername">系统参数名称</param>
        /// <returns></returns>
        public static string GetSystemParameter(this IOrganizationService OrganizationServiceAdmin, string parametername)
        {
            try
            {
                QueryExpression qe = new QueryExpression("new_systemparameter");
                qe.ColumnSet = new ColumnSet("new_value");
                qe.TopCount = 1;
                qe.NoLock = true;
                qe.Criteria.AddCondition("new_name", ConditionOperator.Equal, parametername);
                qe.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
                EntityCollection ec = OrganizationServiceAdmin.RetrieveMultiple(qe);

                if (ec != null && ec.Entities != null && ec.Entities.Count > 0 && ec.Entities[0].Contains("new_value"))
                {
                    return ec.Entities[0].GetAttributeValue<string>("new_value");
                }
                else
                {
                    throw new Exception($"无法获取系统参数{parametername}");
                }
            }
            catch (Exception)
            {
                throw new Exception($"无法获取系统参数{parametername}");
            }
        }

        /// <summary>
        /// QueryEntity
        /// </summary>
        /// <param name="uniqueName">唯一名称</param>
        /// <param name="entityName">实体名</param>
        /// <param name="queryField">查询的字段</param>
        /// <returns></returns>
        public static Entity QueryEntity(IOrganizationService organizationService, string entityName, object uniqueName, string queryField = "new_name", string[] columnSet = null, string multiException = null)
        {
            if (uniqueName == null) return null;

            return QueryEntity(organizationService, entityName, new Dictionary<string, object>()
            {
                [queryField] = uniqueName,
            }, columnSet, multiException);
        }

        /// <summary>
        /// QueryEntity 多条件
        /// </summary>
        /// <param name="entityName">实体名称</param>
        /// <param name="conditions">条件 key字段 value值</param>
        /// <returns></returns>
        public static Entity QueryEntity(IOrganizationService organizationService, string entityName, Dictionary<string, object> conditions = null, string[] columnSet = null, string multiException = null)
        {
            QueryExpression query = new QueryExpression(entityName);
            query.NoLock = true;
            query.ColumnSet.AddColumn(entityName + "id");
            if (columnSet != null && columnSet.Length > 0)
            {
                query.ColumnSet.AddColumns(columnSet);
            }

            if (conditions != null)
            {
                foreach (var i in conditions)
                {
                    query.Criteria.AddCondition(i.Key, ConditionOperator.Equal, i.Value);
                }
            }

            query.TopCount = !string.IsNullOrWhiteSpace(multiException) ? 2 : 1;

            var queryCollection = organizationService.RetrieveMultiple(query);
            if (queryCollection?.Entities.Count > 0)
            {
                if (queryCollection?.Entities.Count > 1 && multiException != null)
                    throw new Exception(multiException);

                return queryCollection.Entities.First();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 获得实体对应的元数据信息
        /// </summary>
        /// <param name="service"></param>
        /// <param name="entityLogicalName"></param>
        /// <param name="entityFilters"></param>
        /// <returns></returns>
        public static EntityMetadata GetEntityMetadata(IOrganizationService service, string entityLogicalName, EntityFilters entityFilters = EntityFilters.Entity)
        {
            CacheHelper cacheHelper = CacheHelper.CreateInstance(service);
            EntityMetadata cacheEntityMetadata = cacheHelper.GetValue<EntityMetadata>(
                Cache_Model.CACHE_KEY_MODULENAME_EntityMetadata.CacheKeyFormat(entityLogicalName),
                Cache_Model.CACHE_KEY_MODULENAME_EntityMetadata.GetCacheKey());

            if (cacheEntityMetadata == default(EntityMetadata))
            {
                RetrieveEntityResponse retrieveEntityResponse = (RetrieveEntityResponse)service.Execute(new RetrieveEntityRequest()
                {
                    RetrieveAsIfPublished = true,
                    LogicalName = entityLogicalName,
                    EntityFilters = entityFilters
                });
                cacheEntityMetadata = retrieveEntityResponse?.EntityMetadata;

                if (cacheEntityMetadata != default(EntityMetadata))
                    cacheHelper.SetValue(
                        Cache_Model.CACHE_KEY_MODULENAME_EntityMetadata.CacheKeyFormat(entityLogicalName),
                        cacheEntityMetadata,
                        Cache_Model.CACHE_KEY_MODULENAME_EntityMetadata.GetCacheKey()
                        );
            }

            return cacheEntityMetadata;
        }

        /// <summary>
        /// QueryEntity 多条件
        /// </summary>
        /// <param name="entityName">实体名称</param>
        /// <param name="conditions">条件 key字段 value值</param>
        /// <returns></returns>
        public static DataCollection<Entity> QueryEntites(IOrganizationService organizationService, string entityName, Dictionary<string, object> conditions, string[] columnSet = null)
        {
            if (conditions == null || conditions.Count == 0) return null;
            QueryExpression query = new QueryExpression(entityName);
            query.ColumnSet.AddColumn(entityName + "id");
            if (columnSet != null && columnSet.Length > 0)
            {
                query.ColumnSet.AddColumns(columnSet);
            }

            foreach (var i in conditions)
            {
                query.Criteria.AddCondition(i.Key, ConditionOperator.Equal, i.Value);
            }
            return organizationService.RetrieveMultiple(query).Entities;
        }

        /// <summary>
        /// QueryEntity 多条件
        /// </summary>
        /// <param name="entityName">实体名称</param>
        /// <param name="conditions">条件 key字段 value值</param>
        /// <returns></returns>
        public static EntityCollection QueryEntites(IOrganizationService organizationService, string entityName, int pageIndex, int pageSize, Dictionary<string, object> conditions, string[] columnSet = null)
        {
            if (conditions == null || conditions.Count == 0) return null;
            QueryExpression query = new QueryExpression(entityName);
            query.NoLock = true;
            query.ColumnSet.AddColumn(entityName + "id");
            query.PageInfo = new PagingInfo() { Count = pageSize, PageNumber = pageIndex };
            if (columnSet != null && columnSet.Length > 0)
            {
                query.ColumnSet.AddColumns(columnSet);
            }

            foreach (var i in conditions)
            {
                if (i.Value != null)
                    query.Criteria.AddCondition(i.Key, ConditionOperator.Equal, i.Value);
                else
                    query.Criteria.AddCondition(i.Key, ConditionOperator.Null);
            }
            return organizationService.RetrieveMultiple(query);
        }

        /// <summary>
        /// 获取币种信息
        /// </summary>
        /// <param name="currency"></param>
        /// <param name="OrganizationServiceAdmin"></param>
        /// <returns></returns>
        public static LanguageInfo GetLanguageInfo(this IOrganizationService OrganizationServiceAdmin, int? localeid, string code)
        {
            if (!localeid.HasValue && string.IsNullOrWhiteSpace(code)) return null;

            string currencyInfoCache = CacheHelper.CreateInstance(OrganizationServiceAdmin).GetValue("LanguageInfo", "LanguageInfo");
            List<LanguageInfo> info = new List<LanguageInfo>();
            if (!string.IsNullOrWhiteSpace(currencyInfoCache))
            {
                info = JsonConvert.DeserializeObject<List<LanguageInfo>>(currencyInfoCache);
            }
            else
            {
                QueryExpression qe = new QueryExpression("languagelocale");
                qe.ColumnSet = new ColumnSet("localeid", "language", "code");
                EntityCollection ec = OrganizationServiceAdmin.RetrieveMultiple(qe);

                info = ec != null && ec.Entities.Any() ?
                   ec.Entities.Select(e => new LanguageInfo()
                   {
                       code = e.GetStringOrDefault("code"),
                       language = e.GetStringOrDefault("language"),
                       languagelocaleid = e.Id,
                       localeid = e.GetIntOrDefault("localeid"),
                   }).ToList() : new List<LanguageInfo>();

                CacheHelper.CreateInstance(OrganizationServiceAdmin).SetValue("LanguageInfo", JsonConvert.SerializeObject(info), "LanguageInfo");//创建实体数字精度缓存
            }

            if (localeid.HasValue)
                return info.FirstOrDefault(e => e.localeid.HasValue && e.localeid.Value.Equals(localeid.Value));
            else
                return info.FirstOrDefault(e => e.code.Equals(code, StringComparison.OrdinalIgnoreCase));
        }
    }
}
