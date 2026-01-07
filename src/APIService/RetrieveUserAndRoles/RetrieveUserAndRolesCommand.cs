using APIService.RetrieveUserAndRoles;
using CommonHelper;
using CommonHelper.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace APIService.RetrieveEntityMetadata
{
    public class RetrieveUserAndRolesCommand : BaseCommand
    {
        /// <summary>
        /// 获取所有用户和安全角色
        /// </summary>
        /// <param name="OrganizationService"></param>
        /// <param name="customFields"></param>
        /// <returns></returns>
        public ResultModel GetUserAndRoles(string OrganizationService, List<CustomUserField> customFields)
        {
            try
            {
                CreateCrmServic(OrganizationService, out IOrganizationService envirFromService);

                return GetUserAndRoles(envirFromService, customFields);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetUserAndRoles");
                Log.LogException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 获取所有用户和安全角色
        /// </summary>
        /// <param name="OrganizationService"></param>
        /// <param name="customFields"></param>
        /// <returns></returns>
        public ResultModel GetUserAndRoles(IOrganizationService OrganizationService, List<CustomUserField> customFields)
        {
            ResultModel result = new ResultModel();
            GetUserAndRolesResult data = new GetUserAndRolesResult();
            try
            {
                ColumnSet column = new ColumnSet("domainname", "internalemailaddress", "mobilephone", "businessunitid", "fullname");
                if (customFields != null && customFields.Any())
                    column.AddColumns(customFields
                        .Where(e => !string.IsNullOrWhiteSpace(e.logicalName))
                        .Select(e => e.logicalName)
                        .Distinct().ToArray());

                QueryExpression qe = new QueryExpression("systemuser");
                qe.Criteria.AddCondition("isdisabled", ConditionOperator.Equal, false);
                qe.Criteria.AddCondition("msdyn_usertype", ConditionOperator.Equal, 192350000);//类型为CRM 用户
                qe.Criteria.AddCondition("azurestate", ConditionOperator.Equal, 0);//Azure存在该用户
                qe.ColumnSet = column;

                LinkEntity leSRoles = new LinkEntity(qe.EntityName, "systemuserroles", $"{qe.EntityName}id", "systemuserid", JoinOperator.LeftOuter);
                leSRoles.EntityAlias = "sr";
                leSRoles.Columns = new ColumnSet("roleid");
                qe.LinkEntities.Add(leSRoles);

                LinkEntity leRole = new LinkEntity("systemuserroles", "role", $"roleid", "roleid", JoinOperator.LeftOuter);
                leRole.EntityAlias = "r";
                leRole.Columns = new ColumnSet("name");
                leSRoles.LinkEntities.Add(leRole);

                var ec = OrganizationService.RetrieveMultiple(qe);
                if (ec == null || !ec.Entities.Any())
                {
                    result.Success(data: data);
                    return result;
                }

                var groupById = ec.Entities.GroupBy(e => e.Id);
                data.TotalRecordCount = groupById.Count();

                foreach (var gb in groupById)
                {
                    var ent = gb.First();
                    List<EntityReference> securityRoles = gb
                        .Where(e => e.Contains("sr.roleid"))
                        .Select(e => new EntityReference()
                        {
                            LogicalName = "role",
                            Id = e.GetAliasAttributeValue<Guid>("sr.roleid"),
                            Name = e.GetStringOrDefault("r.name")
                        }).ToList();
                    var item = new Dictionary<string, object>
                    {
                        { "systemuserid", gb.Key },
                        { "fullname", gb.First().GetStringOrDefault("fullname") },
                        { "domainname", gb.First().GetStringOrDefault("domainname") },
                        { "businessunitid", gb.First().GetEFNameOrDefault("businessunitid") },
                        { "internalemailaddress", gb.First().GetStringOrDefault("internalemailaddress") },
                        { "mobilephone", gb.First().GetStringOrDefault("mobilephone") },
                        { "securityRoles", securityRoles },
                        { "securityRolesStr",  securityRoles == null || !securityRoles.Any() ? string.Empty : string.Join(";", securityRoles.Select(e => e.Name)) }
                    };

                    if (customFields != null && customFields.Any())
                    {
                        foreach (var cf in customFields)
                        {
                            if (ent.ContainsAndNotNull(cf.logicalName))
                            {
                                var value = ent[cf.logicalName];
                                if (value.GetType().Equals(typeof(DateTime)))
                                    item[cf.logicalName] = ent.GetDateTimeOrDefault(cf.logicalName);
                                else if (value.GetType().Equals(typeof(EntityReference)))
                                    item[cf.logicalName] = ent.GetEFNameOrDefault(cf.logicalName);
                                else if (value.GetType().Equals(typeof(OptionSetValue)))
                                    item[cf.logicalName] = ent.GetFormattedValueOrDefault(cf.logicalName);
                                else if (value.GetType().Equals(typeof(OptionSetValueCollection)))
                                    item[cf.logicalName] = ent.GetFormattedValueOrDefault(cf.logicalName);
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
                Log.ErrorMsg("GetUserAndRoles");
                Log.LogException(ex);
                throw ex;
            }
            return result;
        }
    }
}
