using CommonHelper;
using CommonHelper.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;

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
    }
}
