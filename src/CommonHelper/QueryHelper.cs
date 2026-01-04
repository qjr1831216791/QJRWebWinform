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
        /// 根据唯一名称查询单个实体记录
        /// </summary>
        /// <param name="organizationService">组织服务</param>
        /// <param name="entityName">实体逻辑名</param>
        /// <param name="uniqueName">唯一名称值</param>
        /// <param name="queryField">查询字段的逻辑名，默认为"new_name"</param>
        /// <param name="columnSet">要查询的字段数组，如果为null则只查询主键</param>
        /// <param name="multiException">如果查询结果超过1条时抛出的异常信息，如果为null则不检查多条记录</param>
        /// <returns>查询到的实体记录，如果未找到则返回null</returns>
        public static Entity QueryEntity(IOrganizationService organizationService, string entityName, object uniqueName, string queryField = "new_name", string[] columnSet = null, string multiException = null)
        {
            if (uniqueName == null) return null;

            return QueryEntity(organizationService, entityName, new Dictionary<string, object>()
            {
                [queryField] = uniqueName,
            }, columnSet, multiException);
        }

        /// <summary>
        /// 根据多个条件查询单个实体记录
        /// </summary>
        /// <param name="organizationService">组织服务</param>
        /// <param name="entityName">实体逻辑名</param>
        /// <param name="conditions">查询条件字典，key为字段逻辑名，value为字段值，所有条件以AND连接</param>
        /// <param name="columnSet">要查询的字段数组，如果为null则只查询主键</param>
        /// <param name="multiException">如果查询结果超过1条时抛出的异常信息，如果为null则不检查多条记录</param>
        /// <returns>查询到的实体记录，如果未找到则返回null</returns>
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
        /// 获取实体的元数据信息，支持缓存以提高性能
        /// </summary>
        /// <param name="service">组织服务</param>
        /// <param name="entityLogicalName">实体逻辑名</param>
        /// <param name="entityFilters">实体过滤器，指定要获取的元数据类型，默认为EntityFilters.Entity</param>
        /// <returns>实体元数据对象</returns>
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
        /// 根据多个条件查询多个实体记录
        /// </summary>
        /// <param name="organizationService">组织服务</param>
        /// <param name="entityName">实体逻辑名</param>
        /// <param name="conditions">查询条件字典，key为字段逻辑名，value为字段值，所有条件以AND连接</param>
        /// <param name="columnSet">要查询的字段数组，如果为null则只查询主键</param>
        /// <returns>查询到的实体记录集合，如果条件为空或未找到则返回null</returns>
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
        /// 根据多个条件分页查询实体记录集合
        /// </summary>
        /// <param name="organizationService">组织服务</param>
        /// <param name="entityName">实体逻辑名</param>
        /// <param name="pageIndex">页码，从1开始</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="conditions">查询条件字典，key为字段逻辑名，value为字段值，所有条件以AND连接，如果value为null则查询该字段为空的记录</param>
        /// <param name="columnSet">要查询的字段数组，如果为null则只查询主键</param>
        /// <returns>查询到的实体记录集合，如果条件为空则返回null</returns>
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
        /// 获取语言信息，支持根据语言区域ID或语言代码查询
        /// </summary>
        /// <param name="OrganizationServiceAdmin">管理员组织服务</param>
        /// <param name="localeid">语言区域ID，如果提供则按此ID查询</param>
        /// <param name="code">语言代码，如果localeid为空则按此代码查询</param>
        /// <returns>语言信息对象，如果未找到则返回null</returns>
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
