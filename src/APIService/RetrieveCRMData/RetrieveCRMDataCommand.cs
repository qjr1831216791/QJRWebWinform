using CommonHelper.Model;
using CommonHelper;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;

namespace APIService.RetrieveCRMData
{
    public class RetrieveCRMDataCommand : BaseCommand
    {
        /// <summary>
        /// 获取CRM Data
        /// </summary>
        /// <param name="OrganizationService"></param>
        /// <param name="customFields"></param>
        /// <returns></returns>
        public ResultModel RetrieveCRMData(string OrganizationService, string entityName, List<CustomField> customFields)
        {
            try
            {
                CreateCrmServic(OrganizationService, out IOrganizationService envirFromService);

                return RetrieveCRMData(envirFromService, entityName, customFields);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取CRM Data
        /// </summary>
        /// <param name="OrganizationService"></param>
        /// <param name="customFields"></param>
        /// <returns></returns>
        public ResultModel RetrieveCRMData(IOrganizationService OrganizationService, string entityName, List<CustomField> customFields)
        {
            ResultModel result = new ResultModel();
            CRMDataResult data = new CRMDataResult();
            try
            {
                ColumnSet column = new ColumnSet($"{entityName}id");
                if (customFields != null && customFields.Any())
                    column.AddColumns(customFields
                        .Where(e => !string.IsNullOrWhiteSpace(e.logicalName))
                        .Select(e => e.logicalName)
                        .Distinct().ToArray());

                QueryExpression qe = new QueryExpression(entityName);
                switch (entityName)
                {
                    case "systemuser":
                        {
                            qe.Criteria.AddCondition("isdisabled", ConditionOperator.Equal, false);
                            qe.Criteria.AddCondition("msdyn_usertype", ConditionOperator.Equal, 192350000);//类型为CRM 用户
                            qe.Criteria.AddCondition("azurestate", ConditionOperator.Equal, 0);//Azure存在该用户
                            break;
                        }
                    case "businessunit":
                        {
                            qe.Criteria.AddCondition("isdisabled", ConditionOperator.Equal, false);
                            break;
                        }
                    default:
                        {
                            qe.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
                            break;
                        }
                }
                qe.ColumnSet = column;

                var ec = OrganizationService.RetrieveMultiple(qe);
                if (ec == null || !ec.Entities.Any())
                {
                    result.Success(data: data);
                    return result;
                }

                data.TotalRecordCount = ec.Entities.Count();

                foreach (var ent in ec.Entities)
                {
                    var item = new Dictionary<string, object>
                    {
                        { $"{entityName}id", ent.Id },
                    };

                    if (customFields != null && customFields.Any())
                    {
                        foreach (var cf in customFields)
                        {
                            if (ent.ContainsAndNotNull(cf.logicalName))
                            {
                                var value = ent[cf.logicalName];
                                if (value.GetType().Equals(typeof(DateTime)))
                                {
                                    item[cf.logicalName] = ent.GetDateTimeOrDefault(cf.logicalName);
                                    item[$"{cf.logicalName}_formatted"] = ent.GetDateTime(cf.logicalName);
                                }
                                else if (value.GetType().Equals(typeof(EntityReference)))
                                {
                                    item[cf.logicalName] = ent.GetEFOrDefault(cf.logicalName);
                                    item[$"{cf.logicalName}_formatted"] = ent.GetEFNameOrDefault(cf.logicalName);
                                }
                                else if (value.GetType().Equals(typeof(OptionSetValue)))
                                {
                                    item[cf.logicalName] = ent.GetOptionValueOrDefault(cf.logicalName);
                                    item[$"{cf.logicalName}_formatted"] = ent.GetFormattedValueOrDefault(cf.logicalName);
                                }
                                else if (value.GetType().Equals(typeof(OptionSetValueCollection)))
                                {
                                    item[cf.logicalName] = ent.GetAliasAttributeValue<OptionSetValueCollection>(cf.logicalName).Select(e => e.Value).ToList();
                                    item[$"{cf.logicalName}_formatted"] = ent.GetFormattedValueOrDefault(cf.logicalName);
                                }
                                else
                                    item[cf.logicalName] = ent[cf.logicalName].ToString();
                            }
                        }
                    }
                    data.data.Add(item);
                }

                result.Success(data: data);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 获取CRM Data
        /// </summary>
        /// <param name="OrganizationService"></param>
        /// <param name="customFields"></param>
        /// <returns></returns>
        public ResultModel RetrieveCRMDataByFetchXml(string OrganizationService, string fetchXml, int pageIndex = 1, int pageSize = 5000)
        {
            try
            {
                CreateCrmServic(OrganizationService, out IOrganizationService envirFromService);

                return RetrieveCRMDataByFetchXml(envirFromService, fetchXml, pageIndex, pageSize);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取CRM Data
        /// </summary>
        /// <param name="OrganizationService"></param>
        /// <param name="customFields"></param>
        /// <returns></returns>
        public ResultModel RetrieveCRMDataByFetchXml(IOrganizationService OrganizationService, string fetchXml, int pageIndex = 1, int pageSize = 5000)
        {
            ResultModel result = new ResultModel();
            try
            {
                string pagingFetchXml = EntityExtention.PaginationFetchXml(fetchXml, pageIndex, pageSize);
                var ec = OrganizationService.RetrieveMultiple(new FetchExpression(pagingFetchXml));

                result.Success(data: ec);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }
    }
}
