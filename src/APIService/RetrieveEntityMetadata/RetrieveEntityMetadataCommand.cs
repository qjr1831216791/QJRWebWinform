using CommonHelper;
using CommonHelper.Model;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Xml;

namespace APIService.RetrieveEntityMetadata
{
    public class RetrieveEntityMetadataCommand : BaseCommand
    {
        /// <summary>
        /// 获取所有实体元数据
        /// </summary>
        /// <param name="OrganizationService"></param>
        /// <param name="isCustomEntity">是否只获取用户定制的实体</param>
        /// <returns></returns>
        public ResultModel GetAllEntityMetadata(string OrganizationService, bool isCustomEntity = false)
        {
            try
            {
                CreateCrmServic(OrganizationService, out IOrganizationService envirFromService);

                return GetAllEntityMetadata(envirFromService, isCustomEntity);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetAllEntityMetadata");
                Log.LogException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 获取所有实体元数据
        /// </summary>
        /// <param name="OrganizationService"></param>
        /// <param name="isCustomEntity">是否只获取用户定制的实体</param>
        /// <returns></returns>
        public ResultModel GetAllEntityMetadata(IOrganizationService OrganizationService, bool isCustomEntity = false)
        {
            ResultModel result = new ResultModel();
            try
            {
                List<EntityOption> entityOptions = GetAllEntityMetadataOptions(OrganizationService, isCustomEntity);

                result.Success(data: entityOptions.Select(e => new { label = e.displayName, key = e.entityName, objecttypecode = e.objecttypecode }));
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetAllEntityMetadata");
                Log.LogException(ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 获取所有实体元数据
        /// </summary>
        /// <param name="OrganizationService"></param>
        /// <param name="isCustomEntity"></param>
        /// <returns></returns>
        public List<EntityOption> GetAllEntityMetadataOptions(IOrganizationService OrganizationService, bool isCustomEntity = false)
        {
            List<EntityOption> entityOptions = new List<EntityOption>();
            try
            {
                RetrieveAllEntitiesRequest metadataRequest = new RetrieveAllEntitiesRequest
                {
                    EntityFilters = EntityFilters.Entity,
                    RetrieveAsIfPublished = true
                };

                RetrieveAllEntitiesResponse metadataResponse = (RetrieveAllEntitiesResponse)OrganizationService.Execute(metadataRequest);
                if (metadataResponse == null) throw new InvalidPluginExecutionException("获取所有实体元数据失败");

                List<string> ignoreEntity = new List<string>() { "msdyn_", "msdynmkt", "msdyncrm", "msfp", "retention", "organizationdata" };

                foreach (EntityMetadata em in metadataResponse.EntityMetadata)
                {
                    if (isCustomEntity && em.IsCustomEntity.HasValue && !em.IsCustomEntity.Value)
                    {
                        continue;
                    }
                    else if (!em.IsCustomizable.Value || em.IsReadOnlyInMobileClient.Value || !em.IsValidForAdvancedFind.Value)
                    {
                        continue;
                    }
                    else if (ignoreEntity.Any(e => em.LogicalName.Contains(e)))
                    {
                        continue;
                    }
                    else
                    {
                        if (em.DisplayName?.UserLocalizedLabel?.LanguageCode.Equals(2052) ?? false)
                        {
                            entityOptions.Add(new EntityOption()
                            {
                                entityName = em.LogicalName,
                                displayName = em.DisplayName?.UserLocalizedLabel?.Label,
                                objecttypecode = em.ObjectTypeCode ?? -1,
                            });
                        }
                        else if (em.DisplayName?.LocalizedLabels != null && em.DisplayName.LocalizedLabels.Any())
                        {
                            var label = em.DisplayName.LocalizedLabels.FirstOrDefault(l => l.LanguageCode.Equals(2052));
                            if (label != null)
                            {
                                entityOptions.Add(new EntityOption()
                                {
                                    entityName = em.LogicalName,
                                    displayName = label.Label,
                                    objecttypecode = em.ObjectTypeCode ?? -1,
                                });
                            }
                            else
                            {
                                entityOptions.Add(new EntityOption()
                                {
                                    entityName = em.LogicalName,
                                    displayName = em.DisplayName?.UserLocalizedLabel?.Label,
                                    objecttypecode = em.ObjectTypeCode ?? -1,
                                });
                            }
                        }
                        else
                        {
                            entityOptions.Add(new EntityOption()
                            {
                                entityName = em.LogicalName,
                                displayName = em.DisplayName?.UserLocalizedLabel?.Label,
                                objecttypecode = em.ObjectTypeCode ?? -1,
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetAllEntityMetadataOptions");
                Log.LogException(ex);
                throw ex;
            }
            return entityOptions;
        }

        /// <summary>
        /// 获取字段类型
        /// </summary>
        /// <returns></returns>
        public ResultModel GetAttributeTypeList()
        {
            ResultModel result = new ResultModel();
            try
            {
                var options = EnumHelper.ToDictionary<AttributeTypeCode>().Select(e => new
                {
                    label = e.Value,
                    key = e.Key
                });
                result.Success(data: options);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetAttributeTypeList");
                Log.LogException(ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 获取实体字段元数据
        /// </summary>
        /// <param name="OrganizationService"></param>
        /// <param name="entityName"></param>
        /// <param name="attributeName"></param>
        /// <param name="attributeType"></param>
        /// <returns></returns>
        public ResultModel GetAllAttributeMetadataFromEntity(string OrganizationService, string entityName, int? attributeType = null)
        {
            ResultModel result = new ResultModel();
            try
            {
                CreateCrmServic(OrganizationService, out IOrganizationService envirFromService);

                return GetAllAttributeMetadataFromEntity(envirFromService, entityName, attributeType);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetAllAttributeMetadataFromEntity");
                Log.LogException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 获取实体字段元数据
        /// </summary>
        /// <param name="OrganizationService"></param>
        /// <param name="entityName"></param>
        /// <param name="attributeName"></param>
        /// <param name="attributeType"></param>
        /// <returns></returns>
        public ResultModel GetAllAttributeMetadataFromEntity(IOrganizationService OrganizationService, string entityName, int? attributeType = null)
        {
            ResultModel result = new ResultModel();
            List<AttributeItem> attributeItems = new List<AttributeItem>();
            List<EntityOption> entityOptions = new List<EntityOption>();
            try
            {
                if (string.IsNullOrWhiteSpace(entityName)) throw new InvalidPluginExecutionException("实体名称不能为空");

                RetrieveEntityRequest req = new RetrieveEntityRequest()
                {
                    LogicalName = entityName,
                    RetrieveAsIfPublished = true,
                    EntityFilters = EntityFilters.Attributes,
                };
                RetrieveEntityResponse res = (RetrieveEntityResponse)OrganizationService.Execute(req);
                if (res == null) throw new InvalidPluginExecutionException("获取所有实体字段元数据失败");

                if (attributeType.HasValue && attributeType.Value.Equals((int)AttributeTypeCode.Lookup))
                {
                    entityOptions = GetAllEntityMetadataOptions(OrganizationService, false);
                }

                foreach (var attribute in res.EntityMetadata.Attributes)
                {
                    AttributeItem item = null;
                    if (attribute.DisplayName?.UserLocalizedLabel?.LanguageCode.Equals(2052) ?? false)
                    {
                        item = new AttributeItem()
                        {
                            logicalName = attribute.LogicalName,
                            displayName = attribute.DisplayName?.UserLocalizedLabel?.Label ?? attribute.LogicalName,
                            attributeType = attribute.AttributeType?.ToString() ?? "",
                            requiredLevel = attribute.RequiredLevel?.Value.ToString() ?? "",
                        };
                    }
                    else if (attribute.DisplayName?.LocalizedLabels != null && attribute.DisplayName.LocalizedLabels.Any())
                    {
                        var label = attribute.DisplayName.LocalizedLabels.FirstOrDefault(l => l.LanguageCode.Equals(2052));
                        if (label != null)
                        {
                            item = new AttributeItem()
                            {
                                logicalName = attribute.LogicalName,
                                displayName = label.Label,
                                attributeType = attribute.AttributeType?.ToString() ?? "",
                                requiredLevel = attribute.RequiredLevel?.Value.ToString() ?? "",
                            };
                        }
                        else
                        {
                            item = new AttributeItem()
                            {
                                logicalName = attribute.LogicalName,
                                displayName = attribute.DisplayName?.UserLocalizedLabel?.Label ?? attribute.LogicalName,
                                attributeType = attribute.AttributeType?.ToString() ?? "",
                                requiredLevel = attribute.RequiredLevel?.Value.ToString() ?? "",
                            };
                        }
                    }
                    else
                    {
                        item = new AttributeItem()
                        {
                            logicalName = attribute.LogicalName,
                            displayName = attribute.DisplayName?.UserLocalizedLabel?.Label ?? attribute.LogicalName,
                            attributeType = attribute.AttributeType?.ToString() ?? "",
                            requiredLevel = attribute.RequiredLevel?.Value.ToString() ?? "",
                        };
                    }

                    if (item != null)
                    {
                        var _item = FormatAttributeItem(attribute, item, entityOptions, attributeType);

                        attributeItems.Add(_item);
                    }
                }

                result.Success(data: attributeItems);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetAllAttributeMetadataFromEntity");
                Log.LogException(ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 格式化实体字段数据
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="item"></param>
        /// <param name="entityOptions"></param>
        /// <param name="attributeType"></param>
        private AttributeItem FormatAttributeItem(AttributeMetadata attribute, AttributeItem item, List<EntityOption> entityOptions, int? attributeType)
        {
            if ((attributeType.HasValue && attributeType.Value.Equals((int)AttributeTypeCode.String)) ||
                (!string.IsNullOrWhiteSpace(item.attributeType) && new List<string>() { "Memo", "String" }.Contains(item.attributeType)))
            {
                #region String Attribute
                StringAttributeItem _item = new StringAttributeItem(item);

                if (attribute is StringAttributeMetadata && string.IsNullOrWhiteSpace(((StringAttributeMetadata)attribute).AttributeOf))
                {
                    _item.strLength = ((StringAttributeMetadata)attribute).MaxLength ?? StringAttributeMetadata.MinSupportedLength;
                    _item.strFormat = (((StringAttributeMetadata)attribute).Format ?? StringFormat.Text).ToString();
                }
                else if (attribute is MemoAttributeMetadata && string.IsNullOrWhiteSpace(((MemoAttributeMetadata)attribute).AttributeOf))
                {
                    _item.strLength = ((MemoAttributeMetadata)attribute).MaxLength ?? StringAttributeMetadata.MinSupportedLength;
                    _item.strFormat = (((MemoAttributeMetadata)attribute).Format ?? StringFormat.Text).ToString();
                }
                #endregion

                return _item;
            }
            else if ((attributeType.HasValue && attributeType.Value.Equals((int)AttributeTypeCode.Boolean) && attribute is BooleanAttributeMetadata) ||
                (!string.IsNullOrWhiteSpace(item.attributeType) && new List<string>() { "Boolean" }.Contains(item.attributeType)))
            {
                #region Boolean Attribute
                BooleanAttributeItem _item = new BooleanAttributeItem(item);
                _item.defaultValue = ((BooleanAttributeMetadata)attribute).DefaultValue ?? false;
                #endregion

                return _item;
            }
            else if ((attributeType.HasValue && attributeType.Value.Equals((int)AttributeTypeCode.DateTime) && attribute is DateTimeAttributeMetadata) ||
                (!string.IsNullOrWhiteSpace(item.attributeType) && new List<string>() { "DateTime" }.Contains(item.attributeType)))
            {
                #region DateTime Attribute
                DateTimeAttributeItem _item = new DateTimeAttributeItem(item);
                _item.dateTimeFormat = ((DateTimeAttributeMetadata)attribute).Format?.ToString() ?? "";
                _item.dateTimeBehavior = ((DateTimeAttributeMetadata)attribute).DateTimeBehavior?.Value.ToString() ?? "";
                #endregion

                return _item;
            }
            else if ((attributeType.HasValue && attributeType.Value.Equals((int)AttributeTypeCode.Number)) ||
                (!string.IsNullOrWhiteSpace(item.attributeType) && new List<string>() { "Decimal", "Integer", "Double", "Money" }.Contains(item.attributeType)))
            {
                #region Number Attribute
                NumberAttributeItem _item = new NumberAttributeItem(item);

                if (attribute is DoubleAttributeMetadata)
                {
                    _item.precision = (((DoubleAttributeMetadata)attribute).Precision ?? DoubleAttributeMetadata.MinSupportedPrecision).ToString();
                    _item.minimum = (((DoubleAttributeMetadata)attribute).MinValue ?? DoubleAttributeMetadata.MinSupportedValue).ToString();
                    _item.maximum = (((DoubleAttributeMetadata)attribute).MaxValue ?? DoubleAttributeMetadata.MaxSupportedValue).ToString();
                }
                else if (attribute is IntegerAttributeMetadata)
                {
                    _item.precision = "0";
                    _item.minimum = (((IntegerAttributeMetadata)attribute).MinValue ?? IntegerAttributeMetadata.MinSupportedValue).ToString();
                    _item.maximum = (((IntegerAttributeMetadata)attribute).MaxValue ?? IntegerAttributeMetadata.MaxSupportedValue).ToString();
                }
                else if (attribute is DecimalAttributeMetadata)
                {
                    _item.precision = (((DecimalAttributeMetadata)attribute).Precision ?? DecimalAttributeMetadata.MinSupportedPrecision).ToString();
                    _item.minimum = (((DecimalAttributeMetadata)attribute).MinValue ?? Convert.ToDecimal(DecimalAttributeMetadata.MinSupportedValue)).ToString();
                    _item.maximum = (((DecimalAttributeMetadata)attribute).MaxValue ?? Convert.ToDecimal(DecimalAttributeMetadata.MaxSupportedValue)).ToString();
                }
                else if (attribute is MoneyAttributeMetadata)
                {
                    if (((MoneyAttributeMetadata)attribute).IsBaseCurrency ?? false) return item;

                    _item.precision = (((MoneyAttributeMetadata)attribute).Precision ?? MoneyAttributeMetadata.MinSupportedPrecision).ToString();
                    _item.minimum = (((MoneyAttributeMetadata)attribute).MinValue ?? MoneyAttributeMetadata.MinSupportedValue).ToString();
                    _item.maximum = (((MoneyAttributeMetadata)attribute).MaxValue ?? MoneyAttributeMetadata.MaxSupportedValue).ToString();
                    _item.isMoney = true;
                }
                #endregion

                return _item;
            }
            else if ((attributeType.HasValue && attributeType.Value.Equals((int)AttributeTypeCode.Picklist)) ||
                (!string.IsNullOrWhiteSpace(item.attributeType) && new List<string>() { "Picklist", "Status", "State", "MultiSelectPicklist" }.Contains(item.attributeType)))
            {
                #region Picklist Attribute
                PickListAttributeItem _item = new PickListAttributeItem(item);

                OptionMetadata[] array = null;
                if (attribute is PicklistAttributeMetadata)
                {
                    array = ((PicklistAttributeMetadata)attribute).OptionSet.Options.ToArray();
                }
                else if (attribute is StateAttributeMetadata)
                {
                    array = ((StateAttributeMetadata)attribute).OptionSet.Options.ToArray();
                }
                else if (attribute is StatusAttributeMetadata)
                {
                    array = ((StatusAttributeMetadata)attribute).OptionSet.Options.ToArray();
                }
                else if (attribute is MultiSelectPicklistAttributeMetadata)
                {
                    array = ((MultiSelectPicklistAttributeMetadata)attribute).OptionSet.Options.ToArray();
                }

                if (array != null && array.Any())
                {
                    foreach (OptionMetadata optionMetadata in array)
                    {
                        if (optionMetadata.Value.HasValue && optionMetadata.Value.HasValue)
                        {
                            if (optionMetadata.Label.UserLocalizedLabel.LanguageCode.Equals(2052))
                            {
                                _item.options.Add(optionMetadata.Value.Value, optionMetadata.Label.UserLocalizedLabel.Label);
                            }
                            else
                            {
                                var label = optionMetadata.Label.LocalizedLabels.FirstOrDefault(e => e.LanguageCode.Equals(2052))?.Label;
                                if (!string.IsNullOrWhiteSpace(label))
                                    _item.options.Add(optionMetadata.Value.Value, label);
                                else
                                    _item.options.Add(optionMetadata.Value.Value, optionMetadata.Label.UserLocalizedLabel.Label);
                            }
                        }
                    }
                    _item.optionsStr = string.Join("；", _item.options.Select(o => o.Value + "=" + o.Key).ToList());
                }
                #endregion

                return _item;
            }
            else if ((attributeType.HasValue && attributeType.Value.Equals((int)AttributeTypeCode.Lookup) && attribute is LookupAttributeMetadata) ||
                (!string.IsNullOrWhiteSpace(item.attributeType) && new List<string>() { "Lookup" }.Contains(item.attributeType)))
            {
                #region Lookup Attribute
                LookupAttributeItem _item = new LookupAttributeItem(item);
                var metadata = ((LookupAttributeMetadata)attribute);
                if (metadata.Targets != null && metadata.Targets.Length > 0)
                {
                    _item.linkedEntityName = metadata.Targets[0];
                    _item.linkedEntityDisplayName = entityOptions.FirstOrDefault(e => e.entityName == metadata.Targets[0])?.displayName;
                }
                #endregion

                return _item;
            }
            return item;
        }

        /// <summary>
        /// 查询指定实体的Ribbon信息
        /// </summary>
        /// <param name="OrganizationService"></param>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public ResultModel GetEntityRibbonMetadata(string OrganizationService, string entityName)
        {
            try
            {
                CreateCrmServic(OrganizationService, out IOrganizationService envirFromService);

                return GetEntityRibbonMetadata(envirFromService, entityName);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetAllEntityMetadata");
                Log.LogException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 查询指定实体的Ribbon信息
        /// </summary>
        /// <param name="OrganizationService"></param>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public ResultModel GetEntityRibbonMetadata(IOrganizationService OrganizationService, string entityName)
        {
            ResultModel result = new ResultModel();
            try
            {
                if (string.IsNullOrWhiteSpace(entityName)) throw new InvalidPluginExecutionException("实体名称不能为空");

                RetrieveEntityRibbonRequest req = new RetrieveEntityRibbonRequest()
                {
                    EntityName = entityName,
                    RibbonLocationFilter = RibbonLocationFilters.All,
                };
                RetrieveEntityRibbonResponse res = (RetrieveEntityRibbonResponse)OrganizationService.Execute(req);
                if (res == null) throw new InvalidPluginExecutionException("获取指定实体Ribbon元数据失败");

                string ribbonXml = Encoding.UTF8.GetString(unzipRibbon(res.CompressedEntityXml));
                result.Success(data: ribbonXml);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetEntityRibbonMetadata");
                Log.LogException(ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 查询指定实体的Ribbon差异信息
        /// </summary>
        /// <param name="OrganizationService"></param>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public ResultModel GetEntityRibbonDiffMetadata(string OrganizationService, string entityName)
        {
            try
            {
                CreateCrmServic(OrganizationService, out IOrganizationService envirFromService);

                return GetEntityRibbonDiffMetadata(envirFromService, entityName);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetAllEntityMetadata");
                Log.LogException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 查询指定实体的Ribbon差异信息
        /// </summary>
        /// <param name="OrganizationService"></param>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public ResultModel GetEntityRibbonDiffMetadata(IOrganizationService OrganizationService, string entityName)
        {
            ResultModel result = new ResultModel();
            try
            {
                if (string.IsNullOrWhiteSpace(entityName)) throw new InvalidPluginExecutionException("实体名称不能为空");

                QueryExpression qe = new QueryExpression("ribbondiff");
                qe.ColumnSet = new ColumnSet("ribbondiffuniqueid", "rdx", "ribbondiffid", "diffid", "tabid", "difftype");
                qe.Criteria.AddCondition("entity", ConditionOperator.Equal, entityName);
                qe.AddOrder("diffid", OrderType.Ascending);

                var ec = OrganizationServiceAdmin.RetrieveMultiple(qe);

                result.Success(data: RibbonDiffMetadataFormat(ec));
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetEntityRibbonMetadata");
                Log.LogException(ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 解压缩
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private byte[] unzipRibbon(byte[] data)
        {
            System.IO.Packaging.ZipPackage package = null;
            MemoryStream memStream = null;

            memStream = new MemoryStream();
            memStream.Write(data, 0, data.Length);
            package = (ZipPackage)ZipPackage.Open(memStream, FileMode.Open);

            ZipPackagePart part = (ZipPackagePart)package.GetPart(new Uri("/RibbonXml.xml", UriKind.Relative));
            using (Stream strm = part.GetStream())
            {
                long len = strm.Length;
                byte[] buff = new byte[len];
                strm.Read(buff, 0, (int)len);
                return buff;
            }
        }

        /// <summary>
        /// 格式化RibbonDiffXml
        /// </summary>
        /// <param name="ec"></param>
        /// <returns></returns>
        private List<RibbonDiff> RibbonDiffMetadataFormat(EntityCollection ec)
        {
            List<RibbonDiff> ribbonDiffList = new List<RibbonDiff>();
            if (ec == null || !ec.Entities.Any()) return ribbonDiffList;

            // 第一趟：建立 LocLabel 索引，用于后续根据 Button/@LabelText -> $LocLabels:{id} 匹配多语言词条
            Dictionary<string, List<RibbonLocLabel>> locLabelMap = new Dictionary<string, List<RibbonLocLabel>>();

            foreach (var e in ec.Entities)
            {
                var rdx = e.GetStringOrDefault("rdx");
                var trimmedRdx = rdx?.TrimStart();
                // 快速跳过：LocLabel 解析只在 rdx 以 <LocLabel 开头时进行，避免大量无意义的 LoadXml
                if (string.IsNullOrWhiteSpace(trimmedRdx) || !trimmedRdx.StartsWith("<LocLabel", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                var root = TryLoadRdxRoot(rdx);
                if (root == null) continue;
                if (!string.Equals(root.Name, "LocLabel", StringComparison.OrdinalIgnoreCase)) continue;

                var locLabelId = GetXmlAttributeValue(root, "Id");
                if (string.IsNullOrWhiteSpace(locLabelId)) continue;

                var titles = ParseRibbonLocLabels(root);
                if (titles == null || titles.Count == 0) continue;

                if (!locLabelMap.TryGetValue(locLabelId, out var list))
                {
                    list = new List<RibbonLocLabel>();
                    locLabelMap[locLabelId] = list;
                }
                // 合并词条；若存在重复项也保留，后续“优先2052/1033/首条”会按顺序选择
                list.AddRange(titles);
            }

            // 第二趟：构造 RibbonDiff，并在 CustomAction 中解析 Button 与对应 LocLabel
            foreach (var e in ec.Entities)
            {
                RibbonDiff diff = new RibbonDiff()
                {
                    Id = e.Id,
                    ribbondiffuniqueid = e.GetAttributeValue<Guid>("ribbondiffuniqueid"),
                    difftype = e.GetOptionValueOrDefault("difftype") ?? -1,
                    diffid = e.GetStringOrDefault("diffid"),
                    tabid = e.GetStringOrDefault("tabid"),
                    rdx = e.GetStringOrDefault("rdx"),
                    CustomAction = new RibbonCustomAction()
                };

                var rdx = e.GetStringOrDefault("rdx");
                var trimmedRdx = rdx?.TrimStart();
                // 快速跳过：只有 CustomAction/HideCustomAction 才需要解析 Button/Location
                if (string.IsNullOrWhiteSpace(trimmedRdx) ||
                    (!trimmedRdx.StartsWith("<CustomAction", StringComparison.OrdinalIgnoreCase) &&
                     !trimmedRdx.StartsWith("<HideCustomAction", StringComparison.OrdinalIgnoreCase)))
                {
                    ribbonDiffList.Add(diff);
                    continue;
                }

                var root = TryLoadRdxRoot(rdx);
                if (root == null)
                {
                    ribbonDiffList.Add(diff);
                    continue;
                }

                if (string.Equals(root.Name, "CustomAction", StringComparison.OrdinalIgnoreCase))
                {
                    var buttonNodes = root.SelectNodes(".//Button");
                    if (buttonNodes != null)
                    {
                        foreach (XmlNode buttonNode in buttonNodes)
                        {
                            var buttonElement = buttonNode as XmlElement;
                            if (buttonElement == null) continue;

                            var buttonId = GetXmlAttributeValue(buttonElement, "Id") ?? "";
                            var commandId = GetXmlAttributeValue(buttonElement, "Command") ?? "";

                            var labelText = GetXmlAttributeValue(buttonElement, "LabelText") ?? "";
                            string locLabelId = null;
                            if (!string.IsNullOrWhiteSpace(labelText))
                            {
                                const string prefix = "$LocLabels:";
                                var trimmed = labelText.Trim();
                                if (trimmed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                                {
                                    locLabelId = trimmed.Substring(prefix.Length);
                                }
                            }

                            // 找不到 loc label：按你的要求返回空串/空列表
                            List<RibbonLocLabel> ribbonLocLabels = new List<RibbonLocLabel>();
                            string ribbonLocLabel = "";
                            if (!string.IsNullOrWhiteSpace(locLabelId) && locLabelMap.TryGetValue(locLabelId, out var locLabels))
                            {
                                ribbonLocLabels = locLabels ?? new List<RibbonLocLabel>();
                                ribbonLocLabel = SelectRibbonLocLabel(ribbonLocLabels);
                            }

                            diff.CustomAction.Buttons.Add(new RibbonButton()
                            {
                                ButtonId = buttonId,
                                CommandId = commandId,
                                RibbonLocLabel = ribbonLocLabel,
                                RibbonLocLabels = ribbonLocLabels
                            });
                        }
                    }
                }
                else if (string.Equals(root.Name, "HideCustomAction", StringComparison.OrdinalIgnoreCase))
                {
                    // HideCustomAction 不提取 Button
                    diff.HideCustomAction = RibbonHideCustomAction.Parse(root);
                }

                ribbonDiffList.Add(diff);
            }

            return ribbonDiffList;
        }

        private XmlElement TryLoadRdxRoot(string rdx)
        {
            if (string.IsNullOrWhiteSpace(rdx)) return null;

            try
            {
                // rdx 通常是 XML 片段，不一定包含根节点：统一用 Root 包裹
                var xml = "<Root>" + rdx + "</Root>";
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);

                var root = doc.DocumentElement;
                if (root == null) return null;

                foreach (XmlNode child in root.ChildNodes)
                {
                    if (child.NodeType == XmlNodeType.Element)
                    {
                        return child as XmlElement;
                    }
                }
            }
            catch (Exception ex)
            {
                // 单条解析失败不影响整体结果
                Log.ErrorMsg("TryLoadRdxRoot");
                Log.LogException(ex);
            }

            return null;
        }

        private string GetXmlAttributeValue(XmlElement element, string attributeName)
        {
            if (element == null || element.Attributes == null || element.Attributes.Count == 0) return null;

            // 快速路径：优先用精确大小写读取（通常属性名是固定的）
            // 若返回空字符串，可能是大小写不一致或属性不存在，此时回退到忽略大小写的遍历。
            try
            {
                var v = element.GetAttribute(attributeName);
                if (!string.IsNullOrEmpty(v)) return v;
            }
            catch
            {
                // 忽略异常，回退到遍历
            }

            foreach (XmlAttribute attr in element.Attributes)
            {
                if (string.Equals(attr.Name, attributeName, StringComparison.OrdinalIgnoreCase))
                {
                    return attr.Value;
                }
            }
            return null;
        }

        private List<RibbonLocLabel> ParseRibbonLocLabels(XmlElement locLabelElement)
        {
            List<RibbonLocLabel> labels = new List<RibbonLocLabel>();
            if (locLabelElement == null) return labels;

            // Titles/Title 下每个 Title 一条 languagecode -> description
            var titleNodes = locLabelElement.SelectNodes(".//Titles/Title");
            if (titleNodes == null)
            {
                // 容错：有些 XML 片段可能直接写了 Title
                titleNodes = locLabelElement.SelectNodes(".//Title");
            }

            if (titleNodes == null) return labels;

            foreach (XmlNode titleNode in titleNodes)
            {
                var titleElement = titleNode as XmlElement;
                if (titleElement == null) continue;

                var languageCodeValue = GetXmlAttributeValue(titleElement, "languagecode");
                var descriptionValue = GetXmlAttributeValue(titleElement, "description");

                if (string.IsNullOrWhiteSpace(languageCodeValue)) continue;
                if (!int.TryParse(languageCodeValue, out int languageCode)) continue;

                labels.Add(new RibbonLocLabel()
                {
                    languagecode = languageCode,
                    description = descriptionValue ?? ""
                });
            }

            return labels;
        }

        private string SelectRibbonLocLabel(List<RibbonLocLabel> locLabels)
        {
            if (locLabels == null || locLabels.Count == 0) return "";
            if (locLabels.Count == 1) return locLabels[0]?.description ?? "";

            // 多语言：优先2052，其次1033，否则取第一条
            var cn = locLabels.FirstOrDefault(l => l != null && l.languagecode == 2052);
            if (cn != null) return cn.description ?? "";

            var en = locLabels.FirstOrDefault(l => l != null && l.languagecode == 1033);
            if (en != null) return en.description ?? "";

            return locLabels[0]?.description ?? "";
        }
    }

    #region GetEntityRibbonDiffMetadata
    public class RibbonDiff
    {
        public Guid Id { get; set; }

        public Guid ribbondiffuniqueid { get; set; }

        public string rdx { get; set; }

        public string diffid { get; set; }

        public string tabid { get; set; }

        public int difftype { get; set; }

        public RibbonHideCustomAction HideCustomAction { get; set; }

        public RibbonCustomAction CustomAction { get; set; }
    }

    public class RibbonLocLabel
    {
        public string description { get; set; }

        public int languagecode { get; set; }
    }

    public class RibbonCustomAction
    {
        public List<RibbonButton> Buttons { get; set; } = new List<RibbonButton>();
    }

    public class RibbonHideCustomAction
    {
        public string Location { get; set; }

        /// <summary>
        /// 解析 HideCustomAction 节点；rdx 是 XML 片段，HideCustomAction 通常作为根节点出现
        /// </summary>
        /// <param name="hideCustomActionElement"></param>
        /// <returns></returns>
        public static RibbonHideCustomAction Parse(XmlElement hideCustomActionElement)
        {
            if (hideCustomActionElement == null) return null;

            string location = null;
            if (hideCustomActionElement.Attributes != null && hideCustomActionElement.Attributes.Count > 0)
            {
                foreach (XmlAttribute attr in hideCustomActionElement.Attributes)
                {
                    if (string.Equals(attr.Name, "Location", StringComparison.OrdinalIgnoreCase))
                    {
                        location = attr.Value;
                        break;
                    }
                }
            }

            return new RibbonHideCustomAction()
            {
                Location = location
            };
        }
    }

    public class RibbonButton
    {
        public string ButtonId { get; set; }

        public string CommandId { get; set; }

        /// <summary>
        /// 按规则选择后的单值描述；找不到则为空串
        /// </summary>
        public string RibbonLocLabel { get; set; } = "";

        /// <summary>
        /// RibbonLocLabelId 对应的全部多语言词条；找不到则为空列表
        /// </summary>
        public List<RibbonLocLabel> RibbonLocLabels { get; set; } = new List<RibbonLocLabel>();
    }
    #endregion
}
