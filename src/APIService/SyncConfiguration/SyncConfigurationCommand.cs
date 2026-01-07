using CommonHelper;
using CommonHelper.Model;
using CrmConnection;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace APIService.SyncConfiguration
{
    public class SyncConfigurationCommand : BaseCommand
    {
        /// <summary>
        /// 获取环境参数
        /// </summary>
        /// <returns></returns>
        public virtual ResultModel GetEnvironments()
        {
            ResultModel result = new ResultModel();
            try
            {
                if (!string.IsNullOrWhiteSpace(crmEnv) && webConfig.environment.Any(e => e.key.Equals(crmEnv, StringComparison.OrdinalIgnoreCase)))
                {
                    var envs = webConfig.environment.FirstOrDefault(e => e.key.Equals(crmEnv, StringComparison.OrdinalIgnoreCase));

                    if (envs == null) throw new InvalidPluginExecutionException($"无效的CRM环境");

                    var output = envs.env.Select(e => new { key = e.Key, label = e.Key.ToUpper() }).ToList();
                    result.Success(data: output);
                }
                else
                {
                    QueryExpression qe = new QueryExpression("new_systemparameter");
                    qe.NoLock = true;
                    qe.ColumnSet = new ColumnSet("new_name", "new_type");
                    qe.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)StateCode.Active);
                    qe.Criteria.AddCondition("new_name", ConditionOperator.Like, $"SyncConfig.connetionString.%");
                    EntityCollection ec = OrganizationServiceAdmin.RetrieveMultiple(qe);

                    var output = ec != null && ec.Entities.Any() ?
                        ec.Entities
                            .Where(e => e.ContainsAndNotNull("new_name") && e.ContainsAndNotNull("new_type"))
                            .Select(e => new
                            {
                                key = e.GetStringOrDefault("new_name").Substring(e.GetStringOrDefault("new_name").LastIndexOf(".") + 1),
                                label = e.GetStringOrDefault("new_type")
                            }).ToList() :
                        null;
                    result.Success(data: output);
                }
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetEnvironments");
                Log.LogException(ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 获取系统参数
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <returns></returns>
        public ResultModel GetSystemParameters(string envirFrom, string envirTo)
        {
            ResultModel result = new ResultModel();
            try
            {
                CreateCrmServic(envirFrom, envirTo, out IOrganizationService envirFromService, out IOrganizationService envirToService);

                return GetSystemParameters(envirFromService, envirToService);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetSystemParameters");
                Log.LogException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 获取系统参数
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <returns></returns>
        public ResultModel GetSystemParameters(IOrganizationService envirFromService, IOrganizationService envirToService)
        {
            ResultModel result = new ResultModel();
            try
            {
                QueryExpression qe = new QueryExpression("new_systemparameter");
                qe.NoLock = true;
                qe.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)StateCode.Active);
                qe.ColumnSet = new ColumnSet("new_name", "new_value", "new_desc");
                EntityCollection ecFrom = envirFromService.RetrieveMultiple(qe);

                EntityCollection ecTo = envirToService.RetrieveMultiple(qe);

                SyncConfigurationModel model = new SyncConfigurationModel()
                {
                    ecFrom = ecFrom != null && ecFrom.Entities.Any() ? ecFrom.Entities.Select(e => new SystemParameterModel()
                    {
                        Id = e.Id,
                        new_name = e.GetStringOrDefault("new_name"),
                        new_value = e.GetStringOrDefault("new_value"),
                        new_desc = e.GetStringOrDefault("new_desc"),
                    }).ToList()
                    : new List<SystemParameterModel>(),

                    ecTo = ecTo != null && ecTo.Entities.Any() ? ecTo.Entities.Select(e => new SystemParameterModel()
                    {
                        Id = e.Id,
                        new_name = e.GetStringOrDefault("new_name"),
                        new_value = e.GetStringOrDefault("new_value"),
                        new_desc = e.GetStringOrDefault("new_desc"),
                    }).ToList()
                    : new List<SystemParameterModel>(),
                };

                result.Success(data: model);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetSystemParameters");
                Log.LogException(ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 同步系统参数
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <param name="configList"></param>
        /// <returns></returns>
        public ResultModel SyncSystemParameters(string envirFrom, string envirTo, List<string> configList)
        {
            ResultModel result = new ResultModel();
            StringBuilder msg = new StringBuilder();
            try
            {
                if (configList == null || !configList.Any()) throw new Exception("请选择至少一条数据！");

                CreateCrmServic(envirFrom, envirTo, out IOrganizationService envirFromService, out IOrganizationService envirToService);

                //字段映射表
                Dictionary<string, string> fieldMapping = new Dictionary<string, string>() {
                    {"new_name","new_name" },{"new_value","new_value" },{"new_type","new_type" },{"new_desc","new_desc" },
                };
                QueryExpression qe = new QueryExpression("new_systemparameter");
                qe.NoLock = true;
                qe.Criteria.AddCondition(new ConditionExpression($"{qe.EntityName}id", ConditionOperator.In, configList.ToArray()));
                qe.ColumnSet = new ColumnSet(fieldMapping.Keys.ToArray());
                EntityCollection ecFrom = envirFromService.RetrieveMultiple(qe);

                if (ecFrom == null || !ecFrom.Entities.Any())
                {
                    result.Success(message: "无法从系统获取符合条件的数据！");
                    return result;
                }

                foreach (var e in ecFrom.Entities)
                {
                    if (!e.Contains("new_name"))
                    {
                        continue;
                    }

                    string key = e.GetStringOrDefault("new_name");

                    try
                    {
                        Entity eTo = QueryHelper.QueryEntity(envirToService, qe.EntityName, new Dictionary<string, object>() { { "new_name", key } });

                        #region 实体赋值
                        Entity entity = new Entity(qe.EntityName);
                        foreach (var kv in fieldMapping)
                        {
                            if (e.Contains(kv.Key))
                                entity[kv.Value] = e[kv.Key];
                            else
                                entity[kv.Value] = null;
                        }

                        if (eTo != null)
                        {
                            entity.Id = eTo.Id;
                            envirToService.Update(entity);
                        }
                        else
                        {
                            Guid newId = envirToService.Create(entity);
                            entity.Id = newId;
                        }
                        #endregion

                        SyncAnnotations(eTo, e.Id, qe.EntityName, entity.Id, envirFromService, envirToService);
                    }
                    catch (Exception ex)
                    {
                        string _msg = $"系统参数：{key} 同步失败：{ex.Message}";
                        Log.ErrorMsg("SyncSystemParameters");
                        Log.ErrorMsg(_msg);
                        msg.AppendLine(_msg);
                    }
                }

                result.Success(message: msg.ToString());
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("SyncSystemParameters");
                Log.LogException(ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 获取自动编号
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <returns></returns>
        public ResultModel GetAutoNumbers(string envirFrom, string envirTo)
        {
            ResultModel result = new ResultModel();
            try
            {
                CreateCrmServic(envirFrom, envirTo, out IOrganizationService envirFromService, out IOrganizationService envirToService);

                return GetAutoNumbers(envirFromService, envirToService);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetAutoNumbers");
                Log.LogException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 获取自动编号
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <returns></returns>
        public ResultModel GetAutoNumbers(IOrganizationService envirFromService, IOrganizationService envirToService)
        {
            ResultModel result = new ResultModel();
            try
            {
                QueryExpression qe = new QueryExpression("new_autonumber");
                qe.NoLock = true;
                qe.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)StateCode.Active);
                qe.ColumnSet = new ColumnSet("new_name", "new_nofieldname", "new_useexpression", "new_prefix", "new_isyear", "new_length", "new_ismonth", "new_istwo", "new_isday");
                EntityCollection ecFrom = envirFromService.RetrieveMultiple(qe);

                EntityCollection ecTo = envirToService.RetrieveMultiple(qe);

                SyncConfigurationModel model = new SyncConfigurationModel()
                {
                    ecFrom = ecFrom != null && ecFrom.Entities.Any() ? ecFrom.Entities.Select(e => new AutoNumberModel()
                    {
                        Id = e.Id,
                        new_name = e.GetStringOrDefault("new_name"),
                        new_nofieldname = e.GetStringOrDefault("new_nofieldname"),
                        new_useexpression = e.GetFormattedValueOrDefault("new_useexpression"),
                        new_prefix = e.GetStringOrDefault("new_prefix"),
                        new_isyear = e.GetFormattedValueOrDefault("new_isyear"),
                        new_ismonth = e.GetFormattedValueOrDefault("new_ismonth"),
                        new_istwo = e.GetFormattedValueOrDefault("new_istwo"),
                        new_isday = e.GetFormattedValueOrDefault("new_isday"),
                        new_length = e.GetIntOrDefault("new_length"),
                    }).ToList()
                    : new List<AutoNumberModel>(),

                    ecTo = ecTo != null && ecTo.Entities.Any() ? ecTo.Entities.Select(e => new AutoNumberModel()
                    {
                        Id = e.Id,
                        new_name = e.GetStringOrDefault("new_name"),
                        new_nofieldname = e.GetStringOrDefault("new_nofieldname"),
                        new_useexpression = e.GetFormattedValueOrDefault("new_useexpression"),
                        new_prefix = e.GetStringOrDefault("new_prefix"),
                        new_isyear = e.GetFormattedValueOrDefault("new_isyear"),
                        new_ismonth = e.GetFormattedValueOrDefault("new_ismonth"),
                        new_istwo = e.GetFormattedValueOrDefault("new_istwo"),
                        new_isday = e.GetFormattedValueOrDefault("new_isday"),
                        new_length = e.GetIntOrDefault("new_length"),
                    }).ToList()
                    : new List<AutoNumberModel>(),
                };

                result.Success(data: model);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetAutoNumbers");
                Log.LogException(ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 同步自动编号
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <param name="configList"></param>
        /// <returns></returns>
        public ResultModel SyncAutoNumbers(string envirFrom, string envirTo, List<string> configList)
        {
            ResultModel result = new ResultModel();
            StringBuilder msg = new StringBuilder();
            try
            {
                if (configList == null || !configList.Any()) throw new Exception("请选择至少一条数据！");

                CreateCrmServic(envirFrom, envirTo, out IOrganizationService envirFromService, out IOrganizationService envirToService);

                return SyncAutoNumbers(envirFromService, envirToService, configList);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("SyncAutoNumbers");
                Log.LogException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 同步自动编号
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <param name="configList"></param>
        /// <returns></returns>
        public ResultModel SyncAutoNumbers(IOrganizationService envirFromService, IOrganizationService envirToService, List<string> configList)
        {
            ResultModel result = new ResultModel();
            StringBuilder msg = new StringBuilder();
            try
            {
                if (configList == null || !configList.Any()) throw new Exception("请选择至少一条数据！");

                Dictionary<string, string> fieldMapping = new Dictionary<string, string>()
                {
                    {"new_name","new_name" }, {"new_nofieldname","new_nofieldname" }, {"new_useexpression","new_useexpression" }, {"new_usesequence","new_usesequence" }, {"new_prefix","new_prefix" },
                    {"new_length","new_length" }, {"new_isyear","new_isyear" }, {"new_istwo","new_istwo" }, {"new_ismonth","new_ismonth" },{"new_isday","new_isday" },
                    {"new_serialkeyexpression","new_serialkeyexpression" },{"new_autonumberexpression","new_autonumberexpression" }
                };
                QueryExpression qe = new QueryExpression("new_autonumber");
                qe.NoLock = true;
                qe.Criteria.AddCondition(new ConditionExpression($"{qe.EntityName}id", ConditionOperator.In, configList.ToArray()));
                qe.ColumnSet = new ColumnSet(fieldMapping.Keys.ToArray());
                EntityCollection ecFrom = envirFromService.RetrieveMultiple(qe);

                if (ecFrom == null || !ecFrom.Entities.Any())
                {
                    result.Success(message: "无法从系统获取符合条件的数据！");
                    return result;
                }

                foreach (var e in ecFrom.Entities)
                {
                    if (!e.Contains("new_name"))
                    {
                        continue;
                    }

                    string new_name = e.GetStringOrDefault("new_name");
                    string new_nofieldname = e.GetStringOrDefault("new_nofieldname");

                    try
                    {
                        Entity eTo = QueryHelper.QueryEntity(envirToService, qe.EntityName, new Dictionary<string, object>() { { "new_name", new_name }, { "new_nofieldname", new_nofieldname } });

                        #region 实体赋值
                        Entity entity = new Entity(qe.EntityName);
                        foreach (var kv in fieldMapping)
                        {
                            if (e.Contains(kv.Key))
                                entity[kv.Value] = e[kv.Key];
                            else
                                entity[kv.Value] = null;
                        }

                        if (eTo != null)
                        {
                            entity.Id = eTo.Id;
                            envirToService.Update(entity);
                        }
                        else
                        {
                            Guid newId = envirToService.Create(entity);
                            entity.Id = newId;
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        string _msg = $"自动编号：{new_name}-{new_nofieldname} 同步失败：{ex.Message}";
                        Log.ErrorMsg("SyncAutoNumbers");
                        Log.ErrorMsg(_msg);
                        msg.AppendLine(_msg);
                    }
                }

                result.Success(message: msg.ToString());
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("SyncAutoNumbers");
                Log.LogException(ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 获取重复性检测
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <returns></returns>
        public ResultModel GetDuplicateDetects(string envirFrom, string envirTo)
        {
            ResultModel result = new ResultModel();
            try
            {
                CreateCrmServic(envirFrom, envirTo, out IOrganizationService envirFromService, out IOrganizationService envirToService);
                return GetDuplicateDetects(envirFromService, envirToService);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetDuplicateDetects");
                Log.LogException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 获取重复性检测
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <returns></returns>
        public ResultModel GetDuplicateDetects(IOrganizationService envirFromService, IOrganizationService envirToService)
        {
            ResultModel result = new ResultModel();
            try
            {
                QueryExpression qe = new QueryExpression("new_duplicatedetect");
                qe.NoLock = true;
                qe.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)StateCode.Active);
                qe.ColumnSet = new ColumnSet("new_name", "new_message");

                LinkEntity leDetail = new LinkEntity(qe.EntityName, "new_duplicatedetect_key", $"{qe.EntityName}id", "new_duplicatedetectid", JoinOperator.LeftOuter);
                leDetail.EntityAlias = "detail";
                leDetail.Columns = new ColumnSet("new_name", "new_null_ne_null", "new_duplicatedetect_keyid");
                leDetail.LinkCriteria.AddCondition("statecode", ConditionOperator.Equal, (int)StateCode.Active);
                qe.LinkEntities.Add(leDetail);

                EntityCollection ecFrom = envirFromService.RetrieveMultiple(qe);

                EntityCollection ecTo = envirToService.RetrieveMultiple(qe);

                var gb_ecFrom = ecFrom != null && ecFrom.Entities.Any() ? ecFrom.Entities.GroupBy(e => e.Id).ToList() : new List<IGrouping<Guid, Entity>>();
                var gb_ecTo = ecTo != null && ecTo.Entities.Any() ? ecTo.Entities.GroupBy(e => e.Id).ToList() : new List<IGrouping<Guid, Entity>>();

                SyncConfigurationModel model = new SyncConfigurationModel()
                {
                    ecFrom = gb_ecFrom.Any() ? gb_ecFrom.Select(gb => new DuplicateDetectModel()
                    {
                        Id = gb.FirstOrDefault().Id,
                        new_name = gb.FirstOrDefault().GetStringOrDefault("new_name"),
                        new_message = gb.FirstOrDefault().GetStringOrDefault("new_message"),
                        detail = gb.Where(e => e.Contains("detail.new_duplicatedetect_keyid")).Select(e => new DuplicateDetectDetail()
                        {
                            Id = e.GetAliasAttributeValue<Guid>("detail.new_duplicatedetect_keyid"),
                            new_name = e.GetStringOrDefault("detail.new_name"),
                            new_null_ne_null = e.GetFormattedValueOrDefault("detail.new_null_ne_null"),
                        }).ToList(),
                    }).ToList()
                    : new List<DuplicateDetectModel>(),

                    ecTo = gb_ecTo.Any() ? gb_ecTo.Select(gb => new DuplicateDetectModel()
                    {
                        Id = gb.FirstOrDefault().Id,
                        new_name = gb.FirstOrDefault().GetStringOrDefault("new_name"),
                        new_message = gb.FirstOrDefault().GetStringOrDefault("new_message"),
                        detail = gb.Where(e => e.Contains("detail.new_duplicatedetect_keyid")).Select(e => new DuplicateDetectDetail()
                        {
                            Id = e.GetAliasAttributeValue<Guid>("detail.new_duplicatedetect_keyid"),
                            new_name = e.GetStringOrDefault("detail.new_name"),
                            new_null_ne_null = e.GetFormattedValueOrDefault("detail.new_null_ne_null"),
                        }).ToList(),
                    }).ToList()
                    : new List<DuplicateDetectModel>(),
                };

                result.Success(data: model);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetDuplicateDetects");
                Log.LogException(ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 同步重复性检测
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <param name="configList"></param>
        /// <returns></returns>
        public ResultModel SyncDuplicateDetects(string envirFrom, string envirTo, List<string> configList)
        {
            ResultModel result = new ResultModel();
            StringBuilder msg = new StringBuilder();
            try
            {
                if (configList == null || !configList.Any()) throw new Exception("请选择至少一条数据！");

                CreateCrmServic(envirFrom, envirTo, out IOrganizationService envirFromService, out IOrganizationService envirToService);

                return SyncDuplicateDetects(envirFromService, envirToService, configList);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("SyncDuplicateDetects");
                Log.LogException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 同步重复性检测
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <param name="configList"></param>
        /// <returns></returns>
        public ResultModel SyncDuplicateDetects(IOrganizationService envirFromService, IOrganizationService envirToService, List<string> configList)
        {
            ResultModel result = new ResultModel();
            StringBuilder msg = new StringBuilder();
            try
            {
                if (configList == null || !configList.Any()) throw new Exception("请选择至少一条数据！");

                Dictionary<string, string> fieldMapping = new Dictionary<string, string>()
                {
                    {"new_name","new_name" },{"new_message","new_message" },
                };
                QueryExpression qe = new QueryExpression("new_duplicatedetect");
                qe.NoLock = true;
                qe.Criteria.AddCondition(new ConditionExpression($"{qe.EntityName}id", ConditionOperator.In, configList.ToArray()));
                qe.ColumnSet = new ColumnSet(fieldMapping.Keys.ToArray());

                //获取明细
                LinkEntity leDetail = new LinkEntity(qe.EntityName, "new_duplicatedetect_key", $"{qe.EntityName}id", "new_duplicatedetectid", JoinOperator.LeftOuter);
                leDetail.EntityAlias = "detail";
                leDetail.Columns = new ColumnSet("new_null_ne_null", "new_name", "new_duplicatedetectid");
                qe.LinkEntities.Add(leDetail);

                EntityCollection ecFrom = envirFromService.RetrieveMultiple(qe);

                if (ecFrom == null || !ecFrom.Entities.Any())
                {
                    result.Success(message: "无法从系统获取符合条件的数据！");
                    return result;
                }

                //按照主档的new_name分组
                var gbRibbon = ecFrom.Entities.Where(e => e.Contains("new_name")).GroupBy(e => e.GetStringOrDefault("new_name"));

                foreach (var gb in gbRibbon)
                {
                    string new_name = gb.Key;
                    Entity main = gb.First();//主档

                    try
                    {
                        #region 实体赋值
                        Entity eTo = QueryHelper.QueryEntity(envirToService, qe.EntityName, new Dictionary<string, object>() {
                            { "new_name", new_name },
                        });

                        Entity entity = new Entity(qe.EntityName);
                        foreach (var kv in fieldMapping)
                        {
                            if (main.Contains(kv.Key))
                                entity[kv.Value] = main[kv.Key];
                            else
                                entity[kv.Value] = null;
                        }

                        if (eTo != null)
                        {
                            entity.Id = eTo.Id;
                            envirToService.Update(entity);
                        }
                        else
                        {
                            Guid newId = envirToService.Create(entity);
                            entity.Id = newId;
                        }
                        #endregion

                        #region 明细实体赋值
                        foreach (var detail in gb)
                        {
                            if (!detail.Contains("detail.new_name")) continue;

                            string detail_new_name = detail.GetStringOrDefault("detail.new_name");//字段名称
                            try
                            {
                                Entity eToDetail = QueryHelper.QueryEntity(envirToService, "new_duplicatedetect_key", new Dictionary<string, object>() {
                                    { "new_name", detail_new_name },{ "new_duplicatedetectid", entity.Id },
                                });

                                Entity dentity = new Entity("new_duplicatedetect_key");
                                dentity["new_duplicatedetectid"] = entity.ToEntityReference();//主档
                                dentity["new_name"] = detail_new_name;//字段名称
                                if (detail.Contains("detail.new_null_ne_null"))
                                    dentity["new_null_ne_null"] = detail.GetBoolOrDefault("detail.new_null_ne_null").Value;//NULL值不相等

                                if (eToDetail != null)
                                {
                                    dentity.Id = eToDetail.Id;
                                    envirToService.Update(dentity);
                                }
                                else
                                {
                                    Guid newId = envirToService.Create(dentity);
                                    dentity.Id = newId;
                                }
                            }
                            catch (Exception ex)
                            {
                                string _msg = $"重复性检测：{new_name}，明细：{detail_new_name} 同步失败：{ex.Message}";
                                Log.ErrorMsg("SyncDuplicateDetects");
                                Log.ErrorMsg(_msg);
                                msg.AppendLine(_msg);
                            }
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        string _msg = $"重复性检测：{new_name} 同步失败：{ex.Message}";
                        Log.ErrorMsg("SyncDuplicateDetects");
                        Log.ErrorMsg(_msg);
                        msg.AppendLine(_msg);
                    }
                }

                result.Success(message: msg.ToString());
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("SyncDuplicateDetects");
                Log.LogException(ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 获取明细汇总
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <returns></returns>
        public ResultModel GetSumRelationshipDetails(string envirFrom, string envirTo)
        {
            ResultModel result = new ResultModel();
            try
            {
                CreateCrmServic(envirFrom, envirTo, out IOrganizationService envirFromService, out IOrganizationService envirToService);

                return GetSumRelationshipDetails(envirFromService, envirToService);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetSumRelationshipDetails");
                Log.LogException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 获取明细汇总
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <returns></returns>
        public ResultModel GetSumRelationshipDetails(IOrganizationService envirFromService, IOrganizationService envirToService)
        {
            ResultModel result = new ResultModel();
            try
            {
                QueryExpression qe = new QueryExpression("new_sumrelationshipdetail");
                qe.NoLock = true;
                qe.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)StateCode.Active);
                qe.ColumnSet = new ColumnSet("new_name", "new_listentity", "new_reffield", "new_total", "new_list", "new_type");
                EntityCollection ecFrom = envirFromService.RetrieveMultiple(qe);

                EntityCollection ecTo = envirToService.RetrieveMultiple(qe);

                SyncConfigurationModel model = new SyncConfigurationModel()
                {
                    ecFrom = ecFrom != null && ecFrom.Entities.Any() ? ecFrom.Entities.Select(e => new SumRelationshipDetailModel()
                    {
                        Id = e.Id,
                        new_name = e.GetStringOrDefault("new_name"),
                        new_list = e.GetStringOrDefault("new_list"),
                        new_listentity = e.GetStringOrDefault("new_listentity"),
                        new_reffield = e.GetStringOrDefault("new_reffield"),
                        new_total = e.GetStringOrDefault("new_total"),
                        new_type = e.GetFormattedValueOrDefault("new_type"),
                    }).ToList()
                    : new List<SumRelationshipDetailModel>(),

                    ecTo = ecTo != null && ecTo.Entities.Any() ? ecTo.Entities.Select(e => new SumRelationshipDetailModel()
                    {
                        Id = e.Id,
                        new_name = e.GetStringOrDefault("new_name"),
                        new_list = e.GetStringOrDefault("new_list"),
                        new_listentity = e.GetStringOrDefault("new_listentity"),
                        new_reffield = e.GetStringOrDefault("new_reffield"),
                        new_total = e.GetStringOrDefault("new_total"),
                        new_type = e.GetFormattedValueOrDefault("new_type"),
                    }).ToList()
                    : new List<SumRelationshipDetailModel>(),
                };

                result.Success(data: model);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetSumRelationshipDetails");
                Log.LogException(ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 同步明细汇总
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <param name="configList"></param>
        /// <returns></returns>
        public ResultModel SyncSumRelationshipDetails(string envirFrom, string envirTo, List<string> configList)
        {
            ResultModel result = new ResultModel();
            StringBuilder msg = new StringBuilder();
            try
            {
                if (configList == null || !configList.Any()) throw new Exception("请选择至少一条数据！");

                CreateCrmServic(envirFrom, envirTo, out IOrganizationService envirFromService, out IOrganizationService envirToService);

                return SyncSumRelationshipDetails(envirFromService, envirToService, configList);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("SyncSumRelationshipDetails");
                Log.LogException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 同步明细汇总
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <param name="configList"></param>
        /// <returns></returns>
        public ResultModel SyncSumRelationshipDetails(IOrganizationService envirFromService, IOrganizationService envirToService, List<string> configList)
        {
            ResultModel result = new ResultModel();
            StringBuilder msg = new StringBuilder();
            try
            {
                if (configList == null || !configList.Any()) throw new Exception("请选择至少一条数据！");

                Dictionary<string, string> fieldMapping = new Dictionary<string, string>()
                {
                    {"new_name","new_name" }, {"new_listentity","new_listentity" }, {"new_total","new_total" }, {"new_list","new_list" }, {"new_reffield","new_reffield" },{"new_type","new_type" },
                };
                QueryExpression qe = new QueryExpression("new_sumrelationshipdetail");
                qe.NoLock = true;
                qe.Criteria.AddCondition(new ConditionExpression($"{qe.EntityName}id", ConditionOperator.In, configList.ToArray()));
                qe.ColumnSet = new ColumnSet(fieldMapping.Keys.ToArray());
                EntityCollection ecFrom = envirFromService.RetrieveMultiple(qe);

                if (ecFrom == null || !ecFrom.Entities.Any())
                {
                    result.Success(message: "无法从系统获取符合条件的数据！");
                    return result;
                }

                foreach (var e in ecFrom.Entities)
                {
                    if (!e.Contains("new_name"))
                    {
                        continue;
                    }

                    string new_name = e.GetStringOrDefault("new_name");
                    string new_listentity = e.GetStringOrDefault("new_listentity");
                    string new_total = e.GetStringOrDefault("new_total");
                    string new_list = e.GetStringOrDefault("new_list");
                    string new_reffield = e.GetStringOrDefault("new_reffield");

                    try
                    {
                        Entity eTo = QueryHelper.QueryEntity(envirToService, qe.EntityName, new Dictionary<string, object>() {
                            { "new_name", new_name }, { "new_listentity", new_listentity },{ "new_total", new_total }, { "new_list", new_list },{ "new_reffield", new_reffield },
                        });

                        #region 实体赋值
                        Entity entity = new Entity(qe.EntityName);
                        foreach (var kv in fieldMapping)
                        {
                            if (e.Contains(kv.Key))
                                entity[kv.Value] = e[kv.Key];
                            else
                                entity[kv.Value] = null;
                        }

                        if (eTo != null)
                        {
                            entity.Id = eTo.Id;
                            envirToService.Update(entity);
                        }
                        else
                        {
                            Guid newId = envirToService.Create(entity);
                            entity.Id = newId;
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        string _msg = $"明细汇总：主实体：{new_name}({new_total})汇总子实体：{new_listentity}({new_list}) 同步失败：{ex.Message}";
                        Log.ErrorMsg("SyncSumRelationshipDetails");
                        Log.ErrorMsg(_msg);
                        msg.AppendLine(_msg);
                    }
                }

                result.Success(message: msg.ToString());
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("SyncSumRelationshipDetails");
                Log.LogException(ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 获取自定义按钮
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <returns></returns>
        public ResultModel GetRibbonRules(string envirFrom, string envirTo)
        {
            ResultModel result = new ResultModel();
            try
            {
                CreateCrmServic(envirFrom, envirTo, out IOrganizationService envirFromService, out IOrganizationService envirToService);

                return GetRibbonRules(envirFromService, envirToService);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetRibbonRules");
                Log.LogException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 获取自定义按钮
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <returns></returns>
        public ResultModel GetRibbonRules(IOrganizationService envirFromService, IOrganizationService envirToService)
        {
            ResultModel result = new ResultModel();
            try
            {
                QueryExpression qe = new QueryExpression("new_ribbon");
                qe.NoLock = true;
                qe.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)StateCode.Active);
                qe.ColumnSet = new ColumnSet("new_name", "new_desc");

                LinkEntity leDetail = new LinkEntity(qe.EntityName, "new_ribbonrule", $"{qe.EntityName}id", "new_ribbon", JoinOperator.LeftOuter);
                leDetail.EntityAlias = "detail";
                leDetail.Columns = new ColumnSet("new_roleid", "new_ribbonruleid");
                leDetail.LinkCriteria.AddCondition("statecode", ConditionOperator.Equal, (int)StateCode.Active);
                qe.LinkEntities.Add(leDetail);

                LinkEntity leRole = new LinkEntity("new_ribbonrule", "role", "new_roleid", "roleid", JoinOperator.LeftOuter);
                leRole.Columns = new ColumnSet("name");
                leRole.EntityAlias = "role";
                leDetail.LinkEntities.Add(leRole);

                EntityCollection ecFrom = envirFromService.RetrieveMultiple(qe);

                EntityCollection ecTo = envirToService.RetrieveMultiple(qe);

                var gb_ecFrom = ecFrom != null && ecFrom.Entities.Any() ? ecFrom.Entities.GroupBy(e => e.Id).ToList() : new List<IGrouping<Guid, Entity>>();
                var gb_ecTo = ecTo != null && ecTo.Entities.Any() ? ecTo.Entities.GroupBy(e => e.Id).ToList() : new List<IGrouping<Guid, Entity>>();

                SyncConfigurationModel model = new SyncConfigurationModel()
                {
                    ecFrom = gb_ecFrom.Any() ? gb_ecFrom.Select(gb => new RibbonRuleModel()
                    {
                        Id = gb.FirstOrDefault().Id,
                        new_name = gb.FirstOrDefault().GetStringOrDefault("new_name"),
                        new_desc = gb.FirstOrDefault().GetStringOrDefault("new_desc"),
                        detail = gb.Where(e => e.Contains("detail.new_ribbonruleid")).Select(e => new RibbonRuleDetailModel()
                        {
                            Id = e.GetAliasAttributeValue<Guid>("detail.new_ribbonruleid"),
                            new_roleid = e.GetStringOrDefault("role.name"),
                        }).ToList(),
                    }).ToList()
                    : new List<RibbonRuleModel>(),

                    ecTo = gb_ecTo.Any() ? gb_ecTo.Select(gb => new RibbonRuleModel()
                    {
                        Id = gb.FirstOrDefault().Id,
                        new_name = gb.FirstOrDefault().GetStringOrDefault("new_name"),
                        new_desc = gb.FirstOrDefault().GetStringOrDefault("new_desc"),
                        detail = gb.Where(e => e.Contains("detail.new_ribbonruleid")).Select(e => new RibbonRuleDetailModel()
                        {
                            Id = e.GetAliasAttributeValue<Guid>("detail.new_ribbonruleid"),
                            new_roleid = e.GetStringOrDefault("role.name"),
                        }).ToList(),
                    }).ToList()
                    : new List<RibbonRuleModel>(),
                };

                result.Success(data: model);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetRibbonRules");
                Log.LogException(ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 同步自定义按钮
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <param name="configList"></param>
        /// <returns></returns>
        public ResultModel SyncRibbonRules(string envirFrom, string envirTo, List<string> configList)
        {
            ResultModel result = new ResultModel();
            StringBuilder msg = new StringBuilder();
            try
            {
                if (configList == null || !configList.Any()) throw new Exception("请选择至少一条数据！");

                CreateCrmServic(envirFrom, envirTo, out IOrganizationService envirFromService, out IOrganizationService envirToService);

                return SyncRibbonRules(envirFromService, envirToService, configList);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("SyncRibbonRules");
                Log.LogException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 同步自定义按钮
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <param name="configList"></param>
        /// <returns></returns>
        public ResultModel SyncRibbonRules(IOrganizationService envirFromService, IOrganizationService envirToService, List<string> configList)
        {
            ResultModel result = new ResultModel();
            StringBuilder msg = new StringBuilder();
            try
            {
                if (configList == null || !configList.Any()) throw new Exception("请选择至少一条数据！");

                Dictionary<string, string> fieldMapping = new Dictionary<string, string>()
                {
                    {"new_name","new_name" },{"new_desc","new_desc" },
                };
                QueryExpression qe = new QueryExpression("new_ribbon");
                qe.NoLock = true;
                qe.Criteria.AddCondition(new ConditionExpression($"{qe.EntityName}id", ConditionOperator.In, configList.ToArray()));
                qe.ColumnSet = new ColumnSet(fieldMapping.Keys.ToArray());

                //获取明细
                LinkEntity leDetail = new LinkEntity(qe.EntityName, "new_ribbonrule", $"{qe.EntityName}id", "new_ribbon", JoinOperator.LeftOuter);
                leDetail.EntityAlias = "detail";
                leDetail.Columns = new ColumnSet("new_roleid", "new_ribbon");
                qe.LinkEntities.Add(leDetail);

                EntityCollection ecFrom = envirFromService.RetrieveMultiple(qe);

                if (ecFrom == null || !ecFrom.Entities.Any())
                {
                    result.Success(message: "无法从系统获取符合条件的数据！");
                    return result;
                }

                //按照主档的new_name分组
                var gbRibbon = ecFrom.Entities.Where(e => e.Contains("new_name")).GroupBy(e => e.GetStringOrDefault("new_name"));

                foreach (var gb in gbRibbon)
                {
                    string new_name = gb.Key;
                    Entity main = gb.First();//主档

                    try
                    {
                        #region 实体赋值
                        Entity eTo = QueryHelper.QueryEntity(envirToService, qe.EntityName, new Dictionary<string, object>() {
                            { "new_name", new_name },
                        });

                        Entity entity = new Entity(qe.EntityName);
                        foreach (var kv in fieldMapping)
                        {
                            if (main.Contains(kv.Key))
                                entity[kv.Value] = main[kv.Key];
                            else
                                entity[kv.Value] = null;
                        }

                        if (eTo != null)
                        {
                            entity.Id = eTo.Id;
                            envirToService.Update(entity);
                        }
                        else
                        {
                            Guid newId = envirToService.Create(entity);
                            entity.Id = newId;
                        }
                        #endregion

                        #region 明细实体赋值
                        foreach (var detail in gb)
                        {
                            if (!detail.Contains("detail.new_roleid")) continue;

                            EntityReference new_roleid = detail.GetEFOrDefault("detail.new_roleid");
                            try
                            {
                                Entity eToDetail = QueryHelper.QueryEntity(envirToService, "new_ribbonrule", new Dictionary<string, object>() {
                                    { "new_roleidname", new_roleid.Name },{ "new_ribbon", entity.Id },
                                });

                                //通过名称获取对应安全角色
                                Entity role = QueryHelper.QueryEntity(envirToService, "role", new Dictionary<string, object>() { { "name", new_roleid.Name } });
                                if (role is null)
                                {
                                    msg.AppendLine($"自定义按钮：{new_name}，明细：{new_roleid?.Name} 同步失败：无法获得目标环境的同名安全角色！");
                                    continue;//如果目标环境缺少同名的安全角色，则跳过该明细的同步
                                }

                                Entity dentity = new Entity("new_ribbonrule");
                                dentity["new_ribbon"] = entity.ToEntityReference();//自定义按钮主档
                                dentity["new_roleid"] = role.ToEntityReference();//安全角色

                                if (eToDetail != null)
                                {
                                    dentity.Id = eToDetail.Id;
                                    envirToService.Update(dentity);
                                }
                                else
                                {
                                    Guid newId = envirToService.Create(dentity);
                                    dentity.Id = newId;
                                }
                            }
                            catch (Exception ex)
                            {
                                string _msg = $"自定义按钮：{new_name}，明细：{new_roleid?.Name} 同步失败：{ex.Message}";
                                Log.ErrorMsg("SyncRibbonRules");
                                Log.ErrorMsg(_msg);
                                msg.AppendLine(_msg);
                            }
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        string _msg = $"自定义按钮：{new_name} 同步失败：{ex.Message}";
                        Log.ErrorMsg("SyncRibbonRules");
                        Log.ErrorMsg(_msg);
                        msg.AppendLine(_msg);
                    }
                }

                result.Success(message: msg.ToString());
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("SyncRibbonRules");
                Log.LogException(ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 获取CommonDeleteCheck步骤
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <returns></returns>
        public ResultModel GetCommonDeleteCheckPluginSteps(string envirFrom, string envirTo)
        {
            ResultModel result = new ResultModel();
            try
            {
                CreateCrmServic(envirFrom, envirTo, out IOrganizationService envirFromService, out IOrganizationService envirToService);

                return GetCommonDeleteCheckPluginSteps(envirFromService, envirToService);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetCommonDeleteCheckPluginSteps");
                Log.LogException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 获取CommonDeleteCheck步骤
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <returns></returns>
        public ResultModel GetCommonDeleteCheckPluginSteps(IOrganizationService envirFromService, IOrganizationService envirToService)
        {
            ResultModel result = new ResultModel();
            try
            {
                QueryExpression qe = new QueryExpression("sdkmessageprocessingstep");
                qe.NoLock = true;
                qe.Criteria.AddCondition("eventhandlername", ConditionOperator.Equal, "RekTec.Crm.Plugin.CommonDeleteCheck");
                qe.ColumnSet = new ColumnSet("name", "configuration", "stage", "sdkmessageid");

                // 添加连接查询实体名
                LinkEntity leFilter = new LinkEntity(qe.EntityName, "sdkmessagefilter", "sdkmessagefilterid", "sdkmessagefilterid", JoinOperator.Inner);
                leFilter.Columns = new ColumnSet("primaryobjecttypecode");
                leFilter.EntityAlias = "entity";
                qe.LinkEntities.Add(leFilter);

                // 添加连接查询SecureConfig信息
                LinkEntity leSC = new LinkEntity(qe.EntityName, "sdkmessageprocessingstepsecureconfig", "sdkmessageprocessingstepsecureconfigid", "sdkmessageprocessingstepsecureconfigid", JoinOperator.LeftOuter);
                leSC.EntityAlias = "sconfig";
                leSC.Columns = new ColumnSet("secureconfig");
                qe.LinkEntities.Add(leSC);

                // 执行查询
                EntityCollection ecFrom = envirFromService.RetrieveMultiple(qe);

                EntityCollection ecTo = envirToService.RetrieveMultiple(qe);

                SyncConfigurationModel model = new SyncConfigurationModel()
                {
                    ecFrom = ecFrom != null && ecFrom.Entities.Any() ? ecFrom.Entities.Select(e => new CommonDeleteCheckPluginStepModel()
                    {
                        Id = e.Id,
                        name = e.GetStringOrDefault("name"),
                        configuration = e.GetStringOrDefault("configuration"),
                        primaryobjecttypecode = e.GetFormattedValueOrDefault("entity.primaryobjecttypecode"),
                        sdkmessageid = e.GetFormattedValueOrDefault("sdkmessageid"),
                        secureconfig = e.GetStringOrDefault("sconfig.secureconfig"),
                        stage = e.GetFormattedValueOrDefault("stage"),
                    }).ToList()
                    : new List<CommonDeleteCheckPluginStepModel>(),

                    ecTo = ecTo != null && ecTo.Entities.Any() ? ecTo.Entities.Select(e => new CommonDeleteCheckPluginStepModel()
                    {
                        Id = e.Id,
                        name = e.GetStringOrDefault("name"),
                        configuration = e.GetStringOrDefault("configuration"),
                        primaryobjecttypecode = e.GetFormattedValueOrDefault("entity.primaryobjecttypecode"),
                        sdkmessageid = e.GetFormattedValueOrDefault("sdkmessageid"),
                        secureconfig = e.GetStringOrDefault("sconfig.secureconfig"),
                        stage = e.GetFormattedValueOrDefault("stage"),
                    }).ToList()
                    : new List<CommonDeleteCheckPluginStepModel>(),
                };

                result.Success(data: model);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetCommonDeleteCheckPluginSteps");
                Log.LogException(ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 同步CommonDeleteCheck步骤
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <param name="configList"></param>
        /// <returns></returns>
        public ResultModel SyncCommonDeleteCheckPluginSteps(string envirFrom, string envirTo, List<string> configList)
        {
            ResultModel result = new ResultModel();
            StringBuilder msg = new StringBuilder();
            try
            {
                if (configList == null || !configList.Any()) throw new Exception("请选择至少一条数据！");

                CreateCrmServic(envirFrom, envirTo, out IOrganizationService envirFromService, out IOrganizationService envirToService);

                return SyncCommonDeleteCheckPluginSteps(envirFromService, envirToService, configList);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("SyncCommonDeleteCheckPluginSteps");
                Log.LogException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 同步CommonDeleteCheck步骤
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <param name="configList"></param>
        /// <returns></returns>
        public ResultModel SyncCommonDeleteCheckPluginSteps(IOrganizationService envirFromService, IOrganizationService envirToService, List<string> configList)
        {
            ResultModel result = new ResultModel();
            StringBuilder msg = new StringBuilder();
            try
            {
                if (configList == null || !configList.Any()) throw new Exception("请选择至少一条数据！");

                QueryExpression qe = new QueryExpression("sdkmessageprocessingstep");
                qe.NoLock = true;
                qe.Criteria.AddCondition(new ConditionExpression($"{qe.EntityName}id", ConditionOperator.In, configList.ToArray()));
                qe.ColumnSet = new ColumnSet("name", "configuration", "sdkmessageprocessingstepsecureconfigid");

                // 添加连接查询SecureConfig信息
                LinkEntity leSC = new LinkEntity(qe.EntityName, "sdkmessageprocessingstepsecureconfig", "sdkmessageprocessingstepsecureconfigid", "sdkmessageprocessingstepsecureconfigid", JoinOperator.LeftOuter);
                leSC.EntityAlias = "sconfig";
                leSC.Columns = new ColumnSet("secureconfig", "customizationlevel");
                qe.LinkEntities.Add(leSC);

                EntityCollection ecFrom = envirFromService.RetrieveMultiple(qe);

                if (ecFrom == null || !ecFrom.Entities.Any())
                {
                    result.Success(message: "无法从系统获取符合条件的数据！");
                    return result;
                }

                foreach (var e in ecFrom.Entities)
                {
                    string name = e.GetStringOrDefault("name");
                    string configuration = e.GetStringOrDefault("configuration");//Config
                    string secureconfig = e.GetStringOrDefault("sconfig.secureconfig");//Secure Config
                    int customizationlevel = e.GetAliasAttributeValue<int>("sconfig.customizationlevel");

                    try
                    {
                        #region 实体赋值
                        Entity eTo = QueryHelper.QueryEntity(envirToService, qe.EntityName, new Dictionary<string, object>() {
                            { "name", name },
                        }, new string[] { "sdkmessageprocessingstepsecureconfigid", "plugintypeid", "organizationid" });

                        Entity entity = new Entity(qe.EntityName);
                        entity["configuration"] = configuration;

                        if (eTo != null && eTo.Contains("plugintypeid"))
                        {
                            entity.Id = eTo.Id;
                            envirToService.Update(entity);

                            if (eTo.Contains("sdkmessageprocessingstepsecureconfigid"))
                            {
                                Entity secureconfigEF = eTo.GetEFOrDefault("sdkmessageprocessingstepsecureconfigid").ToNewEntity();
                                secureconfigEF["secureconfig"] = secureconfig;
                                envirToService.Update(secureconfigEF);

                                PublishSecureConfig(envirToService, secureconfigEF.Id);//发布SecureConfig
                            }
                            else
                            {
                                Entity secureconfigEF = new Entity("sdkmessageprocessingstepsecureconfig")
                                {
                                    ["secureconfig"] = secureconfig,
                                    ["customizationlevel"] = new OptionSetValue(customizationlevel),
                                };

                                Guid newId = envirToService.Create(secureconfigEF);//创建StepSecureConfig

                                entity["sdkmessageprocessingstepsecureconfigid"] = new EntityReference(secureconfigEF.LogicalName, newId);
                                envirToService.Update(entity);//更新PluginMessage的StepSecureConfig

                                PublishSecureConfig(envirToService, newId);//发布SecureConfig
                            }

                            //发布plugin步骤
                            string plugintypeid = eTo.GetEFNameOrDefault("plugintypeid");
                            PublishPluginStep(envirToService, plugintypeid);
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        string _msg = $"CommonDeleteCheck步骤：{name} 同步失败：{ex.Message}";
                        Log.ErrorMsg("SyncCommonDeleteCheckPluginSteps");
                        Log.ErrorMsg(_msg);
                        msg.AppendLine(_msg);
                    }
                }

                result.Success(message: msg.ToString());
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("SyncCommonDeleteCheckPluginSteps");
                Log.LogException(ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 获取数据导入配置
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <returns></returns>
        public ResultModel GetImportConfigs(string envirFrom, string envirTo)
        {
            ResultModel result = new ResultModel();
            try
            {
                CreateCrmServic(envirFrom, envirTo, out IOrganizationService envirFromService, out IOrganizationService envirToService);

                return GetImportConfigs(envirFromService, envirToService);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetImportConfigs");
                Log.LogException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 获取数据导入配置
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <returns></returns>
        public ResultModel GetImportConfigs(IOrganizationService envirFromService, IOrganizationService envirToService)
        {
            ResultModel result = new ResultModel();
            try
            {
                QueryExpression qe = new QueryExpression("new_import");
                qe.NoLock = true;
                qe.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)StateCode.Active);
                qe.ColumnSet = new ColumnSet("new_name", "new_etn", "new_importtype");

                // 执行查询
                EntityCollection ecFrom = envirFromService.RetrieveMultiple(qe);

                EntityCollection ecTo = envirToService.RetrieveMultiple(qe);

                SyncConfigurationModel model = new SyncConfigurationModel()
                {
                    ecFrom = ecFrom != null && ecFrom.Entities.Any() ? ecFrom.Entities.Select(e => new GetImportConfigModel()
                    {
                        Id = e.Id,
                        new_name = e.GetStringOrDefault("new_name"),
                        new_etn = e.GetStringOrDefault("new_etn"),
                        new_importtype = e.GetFormattedValueOrDefault("new_importtype"),
                    }).ToList()
                    : new List<GetImportConfigModel>(),

                    ecTo = ecTo != null && ecTo.Entities.Any() ? ecTo.Entities.Select(e => new GetImportConfigModel()
                    {
                        Id = e.Id,
                        new_name = e.GetStringOrDefault("new_name"),
                        new_etn = e.GetStringOrDefault("new_etn"),
                        new_importtype = e.GetFormattedValueOrDefault("new_importtype"),
                    }).ToList()
                    : new List<GetImportConfigModel>(),
                };

                result.Success(data: model);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetImportConfigs");
                Log.LogException(ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 同步数据导入
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <param name="configList"></param>
        /// <returns></returns>
        public ResultModel SyncImportConfigs(string envirFrom, string envirTo, List<string> configList)
        {
            ResultModel result = new ResultModel();
            StringBuilder msg = new StringBuilder();
            try
            {
                if (configList == null || !configList.Any()) throw new Exception("请选择至少一条数据！");

                CreateCrmServic(envirFrom, envirTo, out IOrganizationService envirFromService, out IOrganizationService envirToService);

                return SyncImportConfigs(envirFromService, envirToService, configList);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("SyncImportConfigs");
                Log.LogException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 同步数据导入
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <param name="configList"></param>
        /// <returns></returns>
        public ResultModel SyncImportConfigs(IOrganizationService envirFromService, IOrganizationService envirToService, List<string> configList)
        {
            ResultModel result = new ResultModel();
            StringBuilder msg = new StringBuilder();
            try
            {
                if (configList == null || !configList.Any()) throw new Exception("请选择至少一条数据！");

                //字段映射表
                Dictionary<string, string> fieldMapping = new Dictionary<string, string>() {
                    {"new_name","new_name" },{"new_allowwarning","new_allowwarning" },{"new_batchcount","new_batchcount" },{"new_etn","new_etn" },{"new_importaction_id","new_importaction_id" },
                    {"new_importfieldconfig","new_importfieldconfig" },{"new_importtype","new_importtype" },{"new_language_id","new_language_id" },{"new_mainattribute","new_mainattribute" },
                    {"new_queueconfig","new_queueconfig" },{"new_uniquekey","new_uniquekey" },{"new_workflow_actionname","new_workflow_actionname" },{"new_workflow_id","new_workflow_id" },
                    {"new_workflow_afteractionname","new_workflow_afteractionname" },{"new_workflow_afterid","new_workflow_afterid" },{"new_workflow_beforeactionname","new_workflow_beforeactionname" },
                    {"new_workflow_beforeid","new_workflow_beforeid" },
                };
                QueryExpression qe = new QueryExpression("new_import");
                qe.NoLock = true;
                qe.Criteria.AddCondition(new ConditionExpression($"{qe.EntityName}id", ConditionOperator.In, configList.ToArray()));
                qe.ColumnSet = new ColumnSet(fieldMapping.Keys.ToArray());
                EntityCollection ecFrom = envirFromService.RetrieveMultiple(qe);

                if (ecFrom == null || !ecFrom.Entities.Any())
                {
                    result.Success(message: "无法从系统获取符合条件的数据！");
                    return result;
                }

                foreach (var e in ecFrom.Entities)
                {
                    if (!e.Contains("new_name"))
                    {
                        continue;
                    }

                    string new_name = e.GetStringOrDefault("new_name");//名称
                    string new_etn = e.GetStringOrDefault("new_etn");//实体名称

                    try
                    {
                        Entity eTo = QueryHelper.QueryEntity(envirToService, qe.EntityName, new Dictionary<string, object>() { { "new_name", new_name }, { "new_etn", new_etn } });

                        #region 实体赋值
                        Entity entity = new Entity(qe.EntityName);
                        foreach (var kv in fieldMapping)
                        {
                            if (e.Contains(kv.Key))
                                entity[kv.Value] = e[kv.Key];
                            else
                                entity[kv.Value] = null;
                        }

                        if (eTo != null)
                        {
                            entity.Id = eTo.Id;
                            envirToService.Update(entity);
                        }
                        else
                        {
                            Guid newId = envirToService.Create(entity);
                            entity.Id = newId;
                        }
                        #endregion

                        SyncAnnotations(eTo, e.Id, qe.EntityName, entity.Id, envirFromService, envirToService);
                    }
                    catch (Exception ex)
                    {
                        string _msg = $"数据导入：{new_name} 同步失败：{ex.Message}";
                        Log.ErrorMsg("SyncImportConfigs");
                        Log.ErrorMsg(_msg);
                        msg.AppendLine(_msg);
                    }
                }

                result.Success(message: msg.ToString());
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("SyncImportConfigs");
                Log.LogException(ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 获取语言配置
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <returns></returns>
        public ResultModel GetLanguageConfigs(string envirFrom, string envirTo,
            int? pageIndexEnvirFrom, int? pageSizeEnvirFrom, int? pageIndexEnvirTo, int? pageSizeEnvirTo, string createdonRange)
        {
            ResultModel result = new ResultModel();
            try
            {
                CreateCrmServic(envirFrom, envirTo, out IOrganizationService envirFromService, out IOrganizationService envirToService);

                return GetLanguageConfigs(envirFromService, envirToService, pageIndexEnvirFrom, pageSizeEnvirFrom, pageIndexEnvirTo, pageSizeEnvirTo, createdonRange);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetLanguageConfigs");
                Log.LogException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 获取语言配置
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <returns></returns>
        public ResultModel GetLanguageConfigs(IOrganizationService envirFromService, IOrganizationService envirToService,
            int? pageIndexEnvirFrom, int? pageSizeEnvirFrom, int? pageIndexEnvirTo, int? pageSizeEnvirTo, string createdonRange)
        {
            ResultModel result = new ResultModel();
            try
            {
                QueryExpression qe = new QueryExpression("new_languageconfig");
                qe.NoLock = true;
                qe.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)StateCode.Active);
                qe.ColumnSet = new ColumnSet("new_name", "new_language_id", "new_content", "new_note", "createdon", "modifiedon");

                // 修改时间和创建时间在指定日期至今期间的数据
                if (!string.IsNullOrWhiteSpace(createdonRange))
                {
                    var range = createdonRange.Split(';');
                    DateTime beginDT = DateTime.Parse(range[0]).ToLocalTime(TimeZoneInfo);
                    DateTime endDT = DateTime.Parse(range[1]).AddDays(1).ToLocalTime(TimeZoneInfo);//加一天包含当天

                    FilterExpression fe = new FilterExpression(LogicalOperator.Or);
                    fe.AddCondition(new ConditionExpression("createdon", ConditionOperator.Between, beginDT, endDT));
                    fe.AddCondition(new ConditionExpression("modifiedon", ConditionOperator.Between, beginDT, endDT));
                    qe.Criteria.Filters.Add(fe);
                }

                qe.AddOrder("createdon", OrderType.Descending);
                qe.AddOrder("modifiedon", OrderType.Descending);

                // 执行查询
                EntityCollection ecFrom = null;
                if (pageSizeEnvirFrom.HasValue && pageIndexEnvirFrom.HasValue)
                {
                    qe.PageInfo = new PagingInfo() { Count = pageSizeEnvirFrom.Value, PageNumber = pageIndexEnvirFrom.Value, ReturnTotalRecordCount = true };
                    ecFrom = envirFromService.RetrieveMultiple(qe);
                }

                EntityCollection ecTo = null;
                if (pageIndexEnvirTo.HasValue && pageSizeEnvirTo.HasValue)
                {
                    qe.PageInfo = new PagingInfo() { Count = pageSizeEnvirTo.Value, PageNumber = pageIndexEnvirTo.Value, ReturnTotalRecordCount = true };
                    ecTo = envirToService.RetrieveMultiple(qe);
                }

                SyncConfigurationModel model = new SyncConfigurationModel()
                {
                    ecFrom = new GetLanguageConfigResult()
                    {
                        TotalRecordCount = ecFrom != null ? ecFrom.TotalRecordCount : 0,
                        data = ecFrom != null && ecFrom.Entities.Any() ? ecFrom.Entities.Select(e => new GetLanguageConfigModel()
                        {
                            Id = e.Id,
                            new_name = e.GetStringOrDefault("new_name"),
                            new_language_id = e.GetEFNameOrDefault("new_language_id"),
                            new_content = e.GetStringOrDefault("new_content"),
                            new_note = e.GetStringOrDefault("new_note"),
                        }).ToList() : new List<GetLanguageConfigModel>(),
                    },

                    ecTo = new GetLanguageConfigResult()
                    {
                        TotalRecordCount = ecTo != null ? ecTo.TotalRecordCount : 0,
                        data = ecTo != null && ecTo.Entities.Any() ? ecTo.Entities.Select(e => new GetLanguageConfigModel()
                        {
                            Id = e.Id,
                            new_name = e.GetStringOrDefault("new_name"),
                            new_language_id = e.GetEFNameOrDefault("new_language_id"),
                            new_content = e.GetStringOrDefault("new_content"),
                            new_note = e.GetStringOrDefault("new_note"),
                        }).ToList() : new List<GetLanguageConfigModel>(),
                    },
                };

                result.Success(data: model);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetLanguageConfigs");
                Log.LogException(ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 同步语言配置
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <param name="configList"></param>
        /// <returns></returns>
        public ResultModel SyncLanguageConfigs(string envirFrom, string envirTo, List<string> configList)
        {
            ResultModel result = new ResultModel();
            StringBuilder msg = new StringBuilder();
            try
            {
                if (configList == null || !configList.Any()) throw new Exception("请选择至少一条数据！");

                CreateCrmServic(envirFrom, envirTo, out IOrganizationService envirFromService, out IOrganizationService envirToService);

                return SyncLanguageConfigs(envirFromService, envirToService, configList);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("SyncLanguageConfigs");
                Log.LogException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 同步语言配置
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <param name="configList"></param>
        /// <returns></returns>
        public ResultModel SyncLanguageConfigs(IOrganizationService envirFromService, IOrganizationService envirToService, List<string> configList)
        {
            ResultModel result = new ResultModel();
            StringBuilder msg = new StringBuilder();
            try
            {
                if (configList == null || !configList.Any()) throw new Exception("请选择至少一条数据！");

                //字段映射表
                Dictionary<string, string> fieldMapping = new Dictionary<string, string>() {
                    {"new_name","new_name" },{"new_language_id","new_language_id" },{"new_content","new_content" },{"new_note","new_note" },{"new_outerapp_id","new_outerapp_id" },
                };
                QueryExpression qe = new QueryExpression("new_languageconfig");
                qe.NoLock = true;
                qe.Criteria.AddCondition(new ConditionExpression($"{qe.EntityName}id", ConditionOperator.In, configList.ToArray()));
                qe.ColumnSet = new ColumnSet(fieldMapping.Keys.ToArray());

                //关联语言
                LinkEntity leLan = new LinkEntity(qe.EntityName, "new_language", "new_language_id", "new_languageid", JoinOperator.LeftOuter);
                leLan.EntityAlias = "l";
                leLan.Columns = new ColumnSet("new_name", "new_code", "new_langid");
                qe.LinkEntities.Add(leLan);

                //关联互联应用
                LinkEntity leApp = new LinkEntity(qe.EntityName, "new_outerapp", "new_outerapp_id", "new_outerappid", JoinOperator.LeftOuter);
                leApp.EntityAlias = "app";
                leApp.Columns = new ColumnSet("new_name", "new_appkey");
                qe.LinkEntities.Add(leApp);

                EntityCollection ecFrom = envirFromService.RetrieveMultiple(qe);

                if (ecFrom == null || !ecFrom.Entities.Any())
                {
                    result.Success(message: "无法从系统获取符合条件的数据！");
                    return result;
                }

                foreach (var e in ecFrom.Entities)
                {
                    if (!e.Contains("new_name") || !e.Contains("new_language_id"))
                    {
                        continue;
                    }

                    string new_name = e.GetStringOrDefault("new_name");//名称
                    EntityReference new_language_id = e.GetEFOrDefault("new_language_id");//语言

                    try
                    {
                        Entity eTo = QueryHelper.QueryEntity(envirToService, qe.EntityName, new Dictionary<string, object>() { { "new_name", new_name }, { "new_language_idname", new_language_id.Name } });

                        #region 实体赋值
                        Entity entity = new Entity(qe.EntityName);
                        foreach (var kv in fieldMapping)
                        {
                            if (e.Contains(kv.Key))
                            {
                                switch (kv.Key)
                                {
                                    case "new_language_id":
                                        Entity language = QueryHelper.QueryEntity(envirToService, "new_language", new Dictionary<string, object>() {
                                            { "new_langid", e.GetStringOrDefault("l.new_langid") }
                                        });
                                        entity["new_language_id"] = language.ToEntityReference();
                                        break;
                                    case "new_outerapp_id":
                                        Entity app = QueryHelper.QueryEntity(envirToService, "new_outerapp", new Dictionary<string, object>() {
                                            { "new_appkey", e.GetStringOrDefault("app.new_appkey") }
                                        });
                                        entity["new_outerapp_id"] = app.ToEntityReference();
                                        break;
                                    default:
                                        entity[kv.Value] = e[kv.Key];
                                        break;
                                }
                            }
                            else
                                entity[kv.Value] = null;
                        }

                        if (eTo != null)
                        {
                            entity.Id = eTo.Id;
                            envirToService.Update(entity);
                        }
                        else
                        {
                            Guid newId = envirToService.Create(entity);
                            entity.Id = newId;
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        string _msg = $"语言配置：{new_name} 同步失败：{ex.Message}";
                        Log.ErrorMsg("SyncLanguageConfigs");
                        Log.ErrorMsg(_msg);
                        msg.AppendLine(_msg);
                    }
                }

                result.Success(message: msg.ToString());
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("SyncLanguageConfigs");
                Log.LogException(ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 获取系统模板配置
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <returns></returns>
        public ResultModel GetDocumenttemplates(string envirFrom, string envirTo)
        {
            ResultModel result = new ResultModel();
            try
            {
                CreateCrmServic(envirFrom, envirTo, out IOrganizationService envirFromService, out IOrganizationService envirToService);

                return GetDocumenttemplates(envirFromService, envirToService);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetDocumenttemplates");
                Log.LogException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 获取系统模板配置
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <returns></returns>
        public ResultModel GetDocumenttemplates(IOrganizationService envirFromService, IOrganizationService envirToService)
        {
            ResultModel result = new ResultModel();
            try
            {
                QueryExpression qe = new QueryExpression("documenttemplate");
                qe.NoLock = true;
                qe.ColumnSet = new ColumnSet("name", "documenttype", "associatedentitytypecode", "languagecode");
                qe.ColumnSet.AddColumn("documenttemplateid");
                qe.Criteria.AddCondition("status", ConditionOperator.Equal, false);

                // 执行查询
                EntityCollection ecFrom = envirFromService.RetrieveMultiple(qe);

                EntityCollection ecTo = envirToService.RetrieveMultiple(qe);

                SyncConfigurationModel model = new SyncConfigurationModel()
                {
                    ecFrom = ecFrom != null && ecFrom.Entities.Any() ? ecFrom.Entities.Select(e => new GetDocumenttemplatesModel()
                    {
                        Id = e.Id,
                        name = e.GetStringOrDefault("name"),
                        languagecode = e.GetIntOrDefault("languagecode"),
                        languageName = envirFromService.GetLanguageInfo(e.GetIntOrDefault("languagecode"), null)?.language,
                        associatedentityLogicalName = e.GetStringOrDefault("associatedentitytypecode"),
                        associatedentityName = QueryHelper.GetEntityMetadata(envirFromService, e.GetStringOrDefault("associatedentitytypecode"))?.DisplayName?.UserLocalizedLabel?.Label,
                        documenttype = e.GetFormattedValueOrDefault("documenttype"),
                    }).ToList()
                    : new List<GetDocumenttemplatesModel>(),

                    ecTo = ecTo != null && ecTo.Entities.Any() ? ecTo.Entities.Select(e => new GetDocumenttemplatesModel()
                    {
                        Id = e.Id,
                        name = e.GetStringOrDefault("name"),
                        languagecode = e.GetIntOrDefault("languagecode"),
                        languageName = envirToService.GetLanguageInfo(e.GetIntOrDefault("languagecode"), null)?.language,
                        associatedentityLogicalName = e.GetStringOrDefault("associatedentitytypecode"),
                        associatedentityName = QueryHelper.GetEntityMetadata(envirToService, e.GetStringOrDefault("associatedentitytypecode"))?.DisplayName?.UserLocalizedLabel?.Label,
                        documenttype = e.GetFormattedValueOrDefault("documenttype"),
                    }).ToList()
                    : new List<GetDocumenttemplatesModel>(),
                };

                result.Success(data: model);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetDocumenttemplates");
                Log.LogException(ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 同步系统模板
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <param name="configList"></param>
        /// <returns></returns>
        public ResultModel SyncDocumenttemplates(string envirFrom, string envirTo, List<string> configList)
        {
            ResultModel result = new ResultModel();
            StringBuilder msg = new StringBuilder();
            try
            {
                if (configList == null || !configList.Any()) throw new Exception("请选择至少一条数据！");

                CreateCrmServic(envirFrom, envirTo, out IOrganizationService envirFromService, out IOrganizationService envirToService);

                return SyncDocumenttemplates(envirFromService, envirToService, configList);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("SyncDocumenttemplates");
                Log.LogException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 同步系统模板
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <param name="configList"></param>
        /// <returns></returns>
        public ResultModel SyncDocumenttemplates(IOrganizationService envirFromService, IOrganizationService envirToService, List<string> configList)
        {
            ResultModel result = new ResultModel();
            StringBuilder msg = new StringBuilder();
            try
            {
                if (configList == null || !configList.Any()) throw new Exception("请选择至少一条数据！");

                Dictionary<string, string> fieldMapping = new Dictionary<string, string>()
                {
                    {"name","name" }, {"content","content" }, {"documenttype","documenttype" }, {"associatedentitytypecode","associatedentitytypecode" }, {"languagecode","languagecode" },
                    {"description","description" },
                };
                QueryExpression qe = new QueryExpression("documenttemplate");
                qe.NoLock = true;
                qe.Criteria.AddCondition(new ConditionExpression($"{qe.EntityName}id", ConditionOperator.In, configList.ToArray()));
                qe.ColumnSet = new ColumnSet(fieldMapping.Keys.ToArray());
                EntityCollection ecFrom = envirFromService.RetrieveMultiple(qe);

                if (ecFrom == null || !ecFrom.Entities.Any())
                {
                    result.Success(message: "无法从系统获取符合条件的数据！");
                    return result;
                }

                foreach (var e in ecFrom.Entities)
                {
                    if (!e.Contains("name"))
                    {
                        continue;
                    }

                    string new_name = e.GetStringOrDefault("name");

                    try
                    {
                        Entity eTo = QueryHelper.QueryEntity(envirToService, qe.EntityName, new Dictionary<string, object>() {
                            { "name", new_name }
                        });

                        #region 实体赋值
                        Entity entity = new Entity(qe.EntityName);
                        foreach (var kv in fieldMapping)
                        {
                            if (e.Contains(kv.Key))
                                entity[kv.Value] = e[kv.Key];
                            else
                                entity[kv.Value] = null;
                        }

                        if (eTo != null)
                        {
                            entity.Id = eTo.Id;
                            envirToService.Update(entity);
                        }
                        else
                        {
                            Guid newId = envirToService.Create(entity);
                            entity.Id = newId;
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        string _msg = $"系统模板：{new_name} 同步失败：{ex.Message}";
                        Log.ErrorMsg("SyncDocumenttemplates");
                        Log.ErrorMsg(_msg);
                        msg.AppendLine(_msg);
                    }
                }

                result.Success(message: msg.ToString());
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("SyncDocumenttemplates");
                Log.LogException(ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 下载系统模板
        /// </summary>
        /// <param name="envir"></param>
        /// <param name="templateId"></param>
        /// <returns></returns>
        public virtual ResultModel DownloadTemplate(string envir, string templateId)
        {
            ResultModel result = new ResultModel();
            try
            {
                if (string.IsNullOrWhiteSpace(envir) || string.IsNullOrWhiteSpace(templateId) || !Guid.TryParse(templateId, out Guid TemplateId)) throw new Exception("无效的入参！");

                CreateCrmServic(envir, out IOrganizationService envirFromService);

                return DownloadTemplate(envirFromService, templateId);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("DownloadTemplate");
                Log.LogException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 下载系统模板
        /// </summary>
        /// <param name="envir"></param>
        /// <param name="templateId"></param>
        /// <returns></returns>
        public virtual ResultModel DownloadTemplate(IOrganizationService envirFromService, string templateId)
        {
            ResultModel result = new ResultModel();
            try
            {
                if (string.IsNullOrWhiteSpace(templateId) || !Guid.TryParse(templateId, out Guid TemplateId)) throw new Exception("无效的入参！");

                Entity template = envirFromService.Retrieve("documenttemplate", TemplateId, new ColumnSet("name", "content", "documenttype"));

                string fileName = $"{template.GetStringOrDefault("name")}_{Guid.NewGuid()}";
                var documenttype = template.GetOptionValueOrDefault("documenttype");
                if (documenttype.HasValue && documenttype.Value.Equals(1))
                {
                    //Excel
                    fileName += ".xlsx";
                }
                else if (documenttype.HasValue && documenttype.Value.Equals(2))
                {
                    //Word
                    fileName += ".docx";
                }

                result.Success(data: template.GetStringOrDefault("content"), message: fileName);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("DownloadTemplate");
                Log.LogException(ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 附件同步
        /// </summary>
        /// <param name="eTo"></param>
        /// <param name="entityName"></param>
        /// <param name="entityId"></param>
        /// <param name="envirFromService"></param>
        /// <param name="envirToService"></param>
        private void SyncAnnotations(Entity eTo, Guid eFromId, string entityName, Guid entityId, IOrganizationService envirFromService, IOrganizationService envirToService)
        {
            try
            {
                if (eTo != null)
                {
                    //删除目标环境的附件
                    var annotations = QueryHelper.QueryEntites(envirToService, "annotation", new Dictionary<string, object>()
                    {
                        ["objecttypecode"] = entityName,
                        ["isdocument"] = true,
                        ["objectid"] = eTo.Id,
                    });

                    if (annotations != null && annotations.Any())
                    {
                        foreach (var a in annotations)
                        {
                            envirToService.Delete(a.LogicalName, a.Id);
                        }
                    }
                }

                //把源环境的附件同步到目标环境
                var envirFromAnnotations = QueryHelper.QueryEntites(envirFromService, "annotation", new Dictionary<string, object>()
                {
                    ["objecttypecode"] = entityName,
                    ["isdocument"] = true,
                    ["objectid"] = eFromId,
                }, new string[] { "documentbody", "filename" });
                if (envirFromAnnotations != null && envirFromAnnotations.Any())
                {
                    foreach (var a in envirFromAnnotations)
                    {
                        Entity annotations = new Entity("annotation");
                        annotations["objectid"] = new EntityReference(entityName, entityId);
                        annotations["isdocument"] = true;
                        annotations["documentbody"] = a.GetStringOrDefault("documentbody");
                        annotations["filename"] = a.GetStringOrDefault("filename");
                        annotations["objecttypecode"] = entityName;
                        envirToService.Create(annotations);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("SyncAnnotations");
                Log.LogException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 发布Plugin-Step-SecureConfig
        /// </summary>
        /// <param name="sdkmessageprocessingstepsecureconfigid"></param>
        /// <exception cref="InvalidPluginExecutionException"></exception>
        private void PublishSecureConfig(IOrganizationService OrganizationService, Guid sdkmessageprocessingstepsecureconfigid)
        {
            PublishXmlRequest publishSCRequest = new PublishXmlRequest
            {
                ParameterXml = $"<importexportxml><solution manifest=\"true\"><entitymanifests><entity name=\"sdkmessageprocessingstepsecureconfig\"><sdkmessageprocessingstepsecureconfigs><sdkmessageprocessingstepsecureconfig>{sdkmessageprocessingstepsecureconfigid}</sdkmessageprocessingstepsecureconfig></sdkmessageprocessingstepsecureconfigs></entity></entitymanifests></solution></importexportxml>"
            };
            PublishXmlResponse publishSCResponse = (PublishXmlResponse)OrganizationService.Execute(publishSCRequest);

            // Check if the plugin step is published successfully
            if (publishSCResponse.Results.ContainsKey("ErrorMessage") && publishSCResponse.Results["ErrorMessage"] != null)
            {
                throw new InvalidPluginExecutionException(publishSCResponse.Results["ErrorMessage"].ToString());
            }
        }

        /// <summary>
        /// 发布plugin步骤
        /// </summary>
        /// <param name="OrganizationService"></param>
        /// <param name="stepId"></param>
        /// <exception cref="InvalidPluginExecutionException"></exception>
        private void PublishPluginStep(IOrganizationService OrganizationService, string plugintypeid)
        {
            PublishXmlRequest publishRequest = new PublishXmlRequest
            {
                ParameterXml = $"<importexportxml><solution manifest=\"true\"><entitymanifests><entity name=\"plugintype\"><plugintypes><plugintype>{plugintypeid}</plugintype></plugintypes></entity></entitymanifests></solution></importexportxml>"
            };
            PublishXmlResponse publishResponse = (PublishXmlResponse)OrganizationService.Execute(publishRequest);

            // Check if the plugin step is published successfully
            if (publishResponse.Results.ContainsKey("ErrorMessage") && publishResponse.Results["ErrorMessage"] != null)
            {
                throw new InvalidPluginExecutionException(publishResponse.Results["ErrorMessage"].ToString());
            }
        }

        /// <summary>
        /// 获取多语言数据字段
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <returns></returns>
        public ResultModel GetMultipleLanguageContrasts(string envirFrom, string envirTo)
        {
            ResultModel result = new ResultModel();
            try
            {
                CreateCrmServic(envirFrom, envirTo, out IOrganizationService envirFromService, out IOrganizationService envirToService);

                return GetMultipleLanguageContrasts(envirFromService, envirToService);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetMultipleLanguageContrasts");
                Log.LogException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 获取多语言数据字段
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <returns></returns>
        public ResultModel GetMultipleLanguageContrasts(IOrganizationService envirFromService, IOrganizationService envirToService)
        {
            ResultModel result = new ResultModel();
            try
            {
                QueryExpression qe = new QueryExpression("new_multiple_language_contrast");
                qe.NoLock = true;
                qe.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)StateCode.Active);
                qe.ColumnSet = new ColumnSet("new_entity_name", "new_multi_attribute_name", "new_code", "createdon");
                EntityCollection ecFrom = envirFromService.RetrieveMultiple(qe);

                EntityCollection ecTo = envirToService.RetrieveMultiple(qe);

                SyncConfigurationModel model = new SyncConfigurationModel()
                {
                    ecFrom = ecFrom != null && ecFrom.Entities.Any() ? ecFrom.Entities.Select(e => new MultipleLanguageContrastsModel()
                    {
                        Id = e.Id,
                        new_entity_name = e.GetStringOrDefault("new_entity_name"),
                        new_multi_attribute_name = e.GetStringOrDefault("new_multi_attribute_name"),
                        new_code = e.GetStringOrDefault("new_code"),
                        createdon = e.GetDateTimeOrDefault("createdon"),
                    }).ToList()
                    : new List<MultipleLanguageContrastsModel>(),

                    ecTo = ecTo != null && ecTo.Entities.Any() ? ecTo.Entities.Select(e => new MultipleLanguageContrastsModel()
                    {
                        Id = e.Id,
                        new_entity_name = e.GetStringOrDefault("new_entity_name"),
                        new_multi_attribute_name = e.GetStringOrDefault("new_multi_attribute_name"),
                        new_code = e.GetStringOrDefault("new_code"),
                        createdon = e.GetDateTimeOrDefault("createdon"),
                    }).ToList()
                    : new List<MultipleLanguageContrastsModel>(),
                };

                result.Success(data: model);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetMultipleLanguageContrasts");
                Log.LogException(ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 同步多语言数据字段
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <param name="configList"></param>
        /// <returns></returns>
        public ResultModel SyncMultipleLanguageContrasts(string envirFrom, string envirTo, List<string> configList)
        {
            ResultModel result = new ResultModel();
            StringBuilder msg = new StringBuilder();
            try
            {
                if (configList == null || !configList.Any()) throw new Exception("请选择至少一条数据！");

                CreateCrmServic(envirFrom, envirTo, out IOrganizationService envirFromService, out IOrganizationService envirToService);

                //字段映射表
                Dictionary<string, string> fieldMapping = new Dictionary<string, string>() {
                    {"new_entity_name","new_entity_name" },{"new_multi_attribute_name","new_multi_attribute_name" },{"new_code","new_code" },
                };
                QueryExpression qe = new QueryExpression("new_multiple_language_contrast");
                qe.NoLock = true;
                qe.Criteria.AddCondition(new ConditionExpression($"{qe.EntityName}id", ConditionOperator.In, configList.ToArray()));
                qe.ColumnSet = new ColumnSet(fieldMapping.Keys.ToArray());
                EntityCollection ecFrom = envirFromService.RetrieveMultiple(qe);

                if (ecFrom == null || !ecFrom.Entities.Any())
                {
                    result.Success(message: "无法从系统获取符合条件的数据！");
                    return result;
                }

                foreach (var e in ecFrom.Entities)
                {
                    if (!e.ContainsFieldsAnd("new_entity_name", "new_code"))
                    {
                        continue;
                    }

                    string new_entity_name = e.GetStringOrDefault("new_entity_name");
                    string new_code = e.GetStringOrDefault("new_code");
                    string key = $"{new_entity_name}-{new_code}";

                    try
                    {
                        Entity eTo = QueryHelper.QueryEntity(envirToService, qe.EntityName, new Dictionary<string, object>() {
                            { "new_entity_name", new_entity_name }, { "new_code", new_code }
                        });

                        #region 实体赋值
                        Entity entity = new Entity(qe.EntityName);
                        foreach (var kv in fieldMapping)
                        {
                            if (e.Contains(kv.Key))
                                entity[kv.Value] = e[kv.Key];
                            else
                                entity[kv.Value] = null;
                        }

                        if (eTo != null)
                        {
                            entity.Id = eTo.Id;
                            envirToService.Update(entity);
                        }
                        else
                        {
                            Guid newId = envirToService.Create(entity);
                            entity.Id = newId;
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        string _msg = $"系统参数：{key} 同步失败：{ex.Message}";
                        Log.ErrorMsg("SyncMultipleLanguageContrasts");
                        Log.ErrorMsg(_msg);
                        msg.AppendLine(_msg);
                    }
                }

                result.Success(message: msg.ToString());
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("SyncMultipleLanguageContrasts");
                Log.LogException(ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 获取数据多语言配置
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <returns></returns>
        public ResultModel GetDataLanguageConfigs(string envirFrom, string envirTo,
            int? pageIndexEnvirFrom, int? pageSizeEnvirFrom, int? pageIndexEnvirTo, int? pageSizeEnvirTo, string createdonRange)
        {
            ResultModel result = new ResultModel();
            try
            {
                CreateCrmServic(envirFrom, envirTo, out IOrganizationService envirFromService, out IOrganizationService envirToService);

                return GetDataLanguageConfigs(envirFromService, envirToService, pageIndexEnvirFrom, pageSizeEnvirFrom, pageIndexEnvirTo, pageSizeEnvirTo, createdonRange);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetDataLanguageConfigs");
                Log.LogException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 获取数据多语言配置
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <returns></returns>
        public ResultModel GetDataLanguageConfigs(IOrganizationService envirFromService, IOrganizationService envirToService,
            int? pageIndexEnvirFrom, int? pageSizeEnvirFrom, int? pageIndexEnvirTo, int? pageSizeEnvirTo, string createdonRange)
        {
            ResultModel result = new ResultModel();
            try
            {
                QueryExpression qe = new QueryExpression("new_data_languageconfig");
                qe.NoLock = true;
                qe.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)StateCode.Active);
                qe.ColumnSet = new ColumnSet("new_entity_name", "new_attribute_name", "new_language_id", "new_value", "new_code", "new_data_id", "new_language_code", "createdon");

                // 修改时间和创建时间在指定日期至今期间的数据
                if (!string.IsNullOrWhiteSpace(createdonRange))
                {
                    var range = createdonRange.Split(';');
                    DateTime beginDT = DateTime.Parse(range[0]).ToLocalTime(TimeZoneInfo);
                    DateTime endDT = DateTime.Parse(range[1]).AddDays(1).ToLocalTime(TimeZoneInfo);//加一天包含当天

                    FilterExpression fe = new FilterExpression(LogicalOperator.Or);
                    fe.AddCondition(new ConditionExpression("createdon", ConditionOperator.Between, beginDT, endDT));
                    fe.AddCondition(new ConditionExpression("modifiedon", ConditionOperator.Between, beginDT, endDT));
                    qe.Criteria.Filters.Add(fe);
                }

                qe.AddOrder("createdon", OrderType.Descending);
                qe.AddOrder("modifiedon", OrderType.Descending);

                // 执行查询
                EntityCollection ecFrom = null;
                if (pageSizeEnvirFrom.HasValue && pageIndexEnvirFrom.HasValue)
                {
                    qe.PageInfo = new PagingInfo() { Count = pageSizeEnvirFrom.Value, PageNumber = pageIndexEnvirFrom.Value, ReturnTotalRecordCount = true };
                    ecFrom = envirFromService.RetrieveMultiple(qe);
                }

                EntityCollection ecTo = null;
                if (pageIndexEnvirTo.HasValue && pageSizeEnvirTo.HasValue)
                {
                    qe.PageInfo = new PagingInfo() { Count = pageSizeEnvirTo.Value, PageNumber = pageIndexEnvirTo.Value, ReturnTotalRecordCount = true };
                    ecTo = envirToService.RetrieveMultiple(qe);
                }

                SyncConfigurationModel model = new SyncConfigurationModel()
                {
                    ecFrom = new DataLanguageConfigsResult()
                    {
                        TotalRecordCount = ecFrom != null ? ecFrom.TotalRecordCount : 0,
                        data = ecFrom != null && ecFrom.Entities.Any() ? ecFrom.Entities.Select(e => new DataLanguageConfigsModel()
                        {
                            Id = e.Id,
                            new_entity_name = e.GetStringOrDefault("new_entity_name"),
                            new_attribute_name = e.GetStringOrDefault("new_attribute_name"),
                            new_code = e.GetStringOrDefault("new_code"),
                            new_data_id = e.GetStringOrDefault("new_data_id"),
                            new_language_code = e.GetStringOrDefault("new_language_code"),
                            new_language_id = e.GetEFNameOrDefault("new_language_id"),
                            new_value = e.GetStringOrDefault("new_value"),
                            createdon = e.GetDateTimeOrDefault("createdon"),
                        }).ToList() : new List<DataLanguageConfigsModel>(),
                    },

                    ecTo = new DataLanguageConfigsResult()
                    {
                        TotalRecordCount = ecTo != null ? ecTo.TotalRecordCount : 0,
                        data = ecTo != null && ecTo.Entities.Any() ? ecTo.Entities.Select(e => new DataLanguageConfigsModel()
                        {
                            Id = e.Id,
                            new_entity_name = e.GetStringOrDefault("new_entity_name"),
                            new_attribute_name = e.GetStringOrDefault("new_attribute_name"),
                            new_code = e.GetStringOrDefault("new_code"),
                            new_data_id = e.GetStringOrDefault("new_data_id"),
                            new_language_code = e.GetStringOrDefault("new_language_code"),
                            new_language_id = e.GetEFNameOrDefault("new_language_id"),
                            new_value = e.GetStringOrDefault("new_value"),
                            createdon = e.GetDateTimeOrDefault("createdon"),
                        }).ToList() : new List<DataLanguageConfigsModel>(),
                    },
                };

                result.Success(data: model);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetDataLanguageConfigs");
                Log.LogException(ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 同步数据多语言配置
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <param name="configList"></param>
        /// <returns></returns>
        public ResultModel SyncDataLanguageConfigs(string envirFrom, string envirTo, List<string> configList)
        {
            ResultModel result = new ResultModel();
            StringBuilder msg = new StringBuilder();
            try
            {
                if (configList == null || !configList.Any()) throw new Exception("请选择至少一条数据！");

                CreateCrmServic(envirFrom, envirTo, out IOrganizationService envirFromService, out IOrganizationService envirToService);

                return SyncDataLanguageConfigs(envirFromService, envirToService, configList);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("SyncDataLanguageConfigs");
                Log.LogException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 同步数据多语言配置
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <param name="configList"></param>
        /// <returns></returns>
        public ResultModel SyncDataLanguageConfigs(IOrganizationService envirFromService, IOrganizationService envirToService, List<string> configList)
        {
            ResultModel result = new ResultModel();
            StringBuilder msg = new StringBuilder();
            try
            {
                if (configList == null || !configList.Any()) throw new Exception("请选择至少一条数据！");

                //字段映射表
                Dictionary<string, string> fieldMapping = new Dictionary<string, string>() {
                    {"new_entity_name","new_entity_name" },{"new_attribute_name","new_attribute_name" },{"new_language_id","new_language_id" },{"new_value","new_value" },
                    {"new_code","new_code" }
                };

                #region 查询待迁移数据
                QueryExpression qe = new QueryExpression("new_data_languageconfig");
                qe.NoLock = true;
                qe.Criteria.AddCondition(new ConditionExpression($"{qe.EntityName}id", ConditionOperator.In, configList.ToArray()));
                qe.ColumnSet = new ColumnSet(fieldMapping.Keys.ToArray());

                //关联语言
                LinkEntity leLan = new LinkEntity(qe.EntityName, "new_language", "new_language_id", "new_languageid", JoinOperator.LeftOuter);
                leLan.EntityAlias = "l";
                leLan.Columns = new ColumnSet("new_name", "new_code", "new_langid");
                qe.LinkEntities.Add(leLan);

                EntityCollection ecFrom = envirFromService.RetrieveMultiple(qe);

                if (ecFrom == null || !ecFrom.Entities.Any())
                {
                    result.Success(message: "无法从系统获取符合条件的数据！");
                    return result;
                }
                #endregion

                #region 缓存语言配置
                var ecLan = envirToService.QueryEntites("new_language", new Dictionary<string, object>()
                {
                    { "statecode", 0 }
                }, new string[] { "new_name", "new_code", "new_langid" });
                if (ecLan == null || !ecLan.Entities.Any())
                {
                    result.Success(message: "无法从系统获取语言配置！");
                    return result;
                }
                Dictionary<string, Entity> lanDict = ecLan.Entities
                    .Where(e => e.Contains("new_langid"))
                    .ToDictionary(e => e.GetStringOrDefault("new_langid"));
                #endregion

                #region 缓存多语言数据字段配置
                var ecLanContrast = envirToService.QueryEntites("new_multiple_language_contrast", new Dictionary<string, object>()
                {
                    { "statecode", 0 }
                }, new string[] { "new_entity_name", "new_code" });
                if (ecLanContrast == null || !ecLanContrast.Entities.Any())
                {
                    result.Success(message: "无法从系统获取多语言数据字段对应表！");
                    return result;
                }
                var lanContrastDict = ecLanContrast.Entities
                    .Select(e => new
                    {
                        new_entity_name = e.GetStringOrDefault("new_entity_name"),
                        new_code = e.GetStringOrDefault("new_code"),
                    })
                    .GroupBy(e => e.new_entity_name, e => e.new_code)
                    .ToDictionary(e => e.Key, e => e.FirstOrDefault());
                #endregion

                #region 缓存目标环境的基础数据
                Dictionary<string, EntityCollection> baseDataDict = new Dictionary<string, EntityCollection>();
                var entityNameList = ecFrom.Entities
                    .Where(e => e.Contains("new_entity_name"))
                    .Select(e => e.GetStringOrDefault("new_entity_name"))
                    .Distinct().ToList();

                foreach (var entityName in entityNameList)
                {
                    if (!lanContrastDict.ContainsKey(entityName))
                    {
                        msg.AppendLine($"实体：{entityName}无法从目标环境获取多语言数据字段对应表！");
                        continue;
                    }
                    string codeLogicalName = lanContrastDict[entityName];

                    var codeValues = ecFrom.Entities
                        .Where(e => e.Contains("new_code") && e.GetStringOrDefault("new_entity_name").Equals(entityName))
                        .Select(e => e.GetStringOrDefault("new_code"))
                        .Distinct().ToArray();
                    if (codeValues == null || !codeValues.Any())
                    {
                        msg.AppendLine($"实体：{entityName}，字段{codeLogicalName}没有找到有效的值");
                        continue;
                    }

                    QueryExpression qeBaseData = new QueryExpression(entityName);
                    qeBaseData.NoLock = true;
                    qeBaseData.ColumnSet = new ColumnSet(codeLogicalName);
                    qeBaseData.Criteria.AddCondition(new ConditionExpression(codeLogicalName, ConditionOperator.In, codeValues));
                    var ecBaseData = envirToService.RetrieveMultiple(qeBaseData);

                    baseDataDict[entityName] = ecBaseData;
                }
                #endregion

                foreach (var e in ecFrom.Entities)
                {
                    if (!e.ContainsFieldsAnd("new_entity_name", "new_language_id", "new_attribute_name", "new_code", "new_value")) continue;

                    string new_entity_name = e.GetStringOrDefault("new_entity_name");//实体名
                    string new_attribute_name = e.GetStringOrDefault("new_attribute_name");//字段名
                    string new_code = e.GetStringOrDefault("new_code");//编码
                    string new_value = e.GetStringOrDefault("new_value");//标签值
                    EntityReference new_language_id = e.GetEFOrDefault("new_language_id");//语言

                    string key = $"{new_entity_name}-{new_attribute_name}-{new_code}-{new_language_id?.Name}";

                    try
                    {
                        Entity eTo = QueryHelper.QueryEntity(envirToService, qe.EntityName, new Dictionary<string, object>() {
                            { "new_entity_name", new_entity_name }, { "new_language_idname", new_language_id.Name }, { "new_attribute_name", new_attribute_name },
                            { "new_code", new_code }, { "new_value", new_value },
                        });

                        #region 实体赋值
                        Entity entity = new Entity(qe.EntityName);
                        foreach (var kv in fieldMapping)
                        {
                            if (e.Contains(kv.Key))
                            {
                                switch (kv.Key)
                                {
                                    case "new_language_id":
                                        {
                                            string new_langid = e.GetStringOrDefault("l.new_langid");
                                            if (!string.IsNullOrWhiteSpace(new_langid) && lanDict.ContainsKey(new_langid))
                                            {
                                                entity["new_language_id"] = lanDict[new_langid].ToEntityReference();
                                                entity["new_language_code"] = new_langid;
                                            }
                                            break;
                                        }
                                    default:
                                        entity[kv.Value] = e[kv.Key];
                                        break;
                                }
                            }
                            else
                                entity[kv.Value] = null;
                        }

                        if (baseDataDict.ContainsKey(new_entity_name) && entity.ContainsAndNotNull("new_code"))
                        {
                            var ecData = baseDataDict[new_entity_name];
                            string new_codeValue = entity.GetStringOrDefault("new_code");
                            var envirToEnt = ecData != null && ecData.Entities.Any() ? ecData.Entities
                                .FirstOrDefault(ent => ent.Contains(lanContrastDict[new_entity_name]) &&
                                    ent.GetStringOrDefault(lanContrastDict[new_entity_name]).Equals(new_codeValue))
                                : null;
                            if (envirToEnt != null)
                                entity["new_data_id"] = envirToEnt.Id.ToString();
                        }

                        if (eTo != null)
                        {
                            entity.Id = eTo.Id;
                            envirToService.Update(entity);
                        }
                        else
                        {
                            Guid newId = envirToService.Create(entity);
                            entity.Id = newId;
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        string _msg = $"数据多语言配置：{key} 同步失败：{ex.Message}";
                        Log.ErrorMsg("SyncDataLanguageConfigs");
                        Log.ErrorMsg(_msg);
                        msg.AppendLine(_msg);
                    }
                }

                result.Success(message: msg.ToString());
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("SyncDataLanguageConfigs");
                Log.LogException(ex);
                throw ex;
            }
            return result;
        }
    }
}
