using CommonHelper.Model;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Organization;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using RekTec.Crm.Common.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CommonHelper
{
    public delegate object GetEntityAttributeData(Entity entity, string attr);

    /// <summary>
    /// **描述:** `EntityExtention是一个针对Microsoft.Xrm.Sdk的扩展帮助类`
    /// </summary>
    public static class EntityExtention
    {
        #region Datetime
        /// <summary>
        /// **描述:** `获取实体或preImage的指定字段的日期时间值，通常用于Plugin。`
        /// **适用场景:** `在Plugin中获取目标实体或其PreImage中的日期时间字段值，当不确定字段在哪个实体中时使用。`
        /// **常见用法:** `var modifedOn = targetEntity.GetDateTime("modifiedon", preImageEntity);`
        /// **性能表现:** `快且稳定。`
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="attr">字段逻辑名</param>
        /// <param name="entPre">preImage</param>
        /// <param name="timezone">时区</param>
        /// <returns>日期</returns>
        public static DateTime? GetDateTime(this Entity entity, string attr, Entity entPre, TimeZoneInfo timezone = null)
        {
            if (entity.Contains(attr))
            {
                return GetDateTime(entity, attr, timezone);
            }

            return GetDateTime(entPre, attr, timezone);
        }

        /// <summary>
        /// **描述:** `获取实体的指定字段的日期时间值，可选择转换为特定时区的时间。`
        /// **适用场景:** `基础操作，从单个实体记录中获取日期时间字段，并可进行时区转换。`
        /// **常见用法:** `var createdon = Entity.GetDateTime("createdon");`
        /// **性能表现:** `快且稳定。`
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="attr">字段逻辑名</param>
        /// <param name="timezone">时区</param>
        /// <returns>日期</returns>
        public static DateTime? GetDateTime(this Entity entity, string attr, TimeZoneInfo timezone = null)
        {
            if (entity.Contains(attr) && entity[attr] != null)
            {
                var a = entity[attr];
                if (a.GetType() == typeof(AliasedValue))
                {
                    return timezone == null ? (DateTime)((AliasedValue)a).Value : ((DateTime)((AliasedValue)a).Value).ToLocalTime(timezone);
                }
                return timezone == null ? (DateTime)a : ((DateTime)a).ToLocalTime(timezone);
            }
            return null;
        }

        /// <summary>
        /// **描述:** `获取实体的指定字段的日期时间值并格式化为字符串。`
        /// **适用场景:** `需要将日期时间字段以特定格式的字符串形式展示或使用时。`
        /// **常见用法:** `var createdon_str = Entity.GetDateTimeOrDefault("createdon");`
        /// **性能表现:** `快且稳定。`
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="attr">字段逻辑名</param>
        /// <param name="format">日期格式</param>
        /// <returns>日期的格式字符串</returns>
        public static string GetDateTimeOrDefault(this Entity entity, string attr, string format = "yyyy-MM-dd HH:mm:ss")
        {
            if (entity.Contains(attr) && entity[attr] != null)
            {
                var a = entity[attr];
                if (a.GetType() == typeof(AliasedValue))
                {
                    return ((DateTime)((AliasedValue)a).Value).ToString(format);
                }
                return ((DateTime)a).ToString(format);
            }
            return null;
        }

        /// <summary>
        /// **描述:** `获取实体的指定字段的日期时间值，根据指定时区转换为本地时间后，格式化为字符串。`
        /// **适用场景:** `处理涉及多时区的日期时间，并需要以特定格式的本地时间字符串展示时。`
        /// **常见用法:** `string localCreatedOn = Entity.GetDateTimeTimeZoneOrDefault("createdon", "yyyy/MM/dd HH:mm", userTimeZoneInfo);`
        /// **性能表现:** `快且稳定。`
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="attr">字段逻辑名</param>
        /// <param name="format">日期格式</param>
        /// <param name="timeZone">时区</param>
        /// <returns>日期的格式字符串</returns>
        public static string GetDateTimeTimeZoneOrDefault(this Entity entity, string attr, string format = "yyyy-MM-dd HH:mm:ss", TimeZoneInfo timeZone = null)
        {
            timeZone = timeZone == null ? TimeZoneInfo.Utc : timeZone;
            if (entity.Contains(attr) && entity[attr] != null)
            {
                var a = entity[attr];
                if (a.GetType() == typeof(AliasedValue))
                {
                    return ((DateTime)((AliasedValue)a).Value).ToLocalTime(timeZone).ToString(format);
                }
                return ((DateTime)a).ToLocalTime(timeZone).ToString(format);
            }
            return null;
        }

        /// <summary>
        /// **描述:** `将给定的UTC日期时间转换为指定CRM用户的本地时间。`
        /// **适用场景:** `需要将标准的UTC时间显示为特定用户所在时区的本地时间时，基于用户的CRM时区设置。`
        /// **常见用法:** `DateTime userLocalTime = utcDateTime.ToLocalTime(service, userId);`
        /// **性能表现:** `依赖一次Retrieve操作查询用户设置，性能适中。`
        /// </summary>
        /// <param name="dt">日期值 (通常应为UTC)</param>
        /// <param name="organizationService">组织服务，通常是OrganizationServiceAdmin</param>
        /// <param name="userId">SystemuserId</param>
        /// <returns>日期</returns>
        public static DateTime ToLocalTime(this DateTime dt, IOrganizationService organizationService, Guid userId)
        {
            Entity user = organizationService.Retrieve("usersettings", userId, new ColumnSet("timezonebias"));
            int? timezonebias = user.GetIntOrDefault("timezonebias");
            return dt.AddMinutes(timezonebias.HasValue ? -timezonebias.Value : 0);
        }
        #endregion

        #region String
        /// <summary>
        /// **描述:** `获取实体的指定字段的字符串值。`
        /// **适用场景:** `获取文本、单行文本、多行文本等字段的字符串表示。`
        /// **常见用法:** `var new_name = Entity.GetStringOrDefault("new_name");`
        /// **性能表现:** `快且稳定。`
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="attr">字段逻辑名</param>
        /// <returns>字符串</returns>
        public static string GetStringOrDefault(this Entity entity, string attr)
        {
            if (entity.Contains(attr) && entity[attr] != null)
            {
                var a = entity[attr];
                if (a == null) return null;
                if (a.GetType() == typeof(AliasedValue))
                {
                    return ((AliasedValue)a).Value.ToString();
                }
                return a.ToString();
            }
            return null;
        }
        #endregion

        #region PickList
        /// <summary>
        /// **描述:** `获取实体的指定字段的选项集 (OptionSetValue)对象。`
        /// **适用场景:** `需要获取选项集字段的完整对象，例如用于更新或比较。`
        /// **常见用法:** `var new_approvalstatus = Entity.GetOptionSetOrDefault("new_approvalstatus");`
        /// **性能表现:** `快且稳定。`
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="attr">字段逻辑名</param>
        /// <returns>选项集</returns>
        public static OptionSetValue GetOptionSetOrDefault(this Entity entity, string attr)
        {
            if (entity.Contains(attr) && entity[attr] != null)
            {
                var a = entity[attr];
                if (a.GetType() == typeof(AliasedValue))
                {
                    return (OptionSetValue)((AliasedValue)a).Value;
                }
                return (OptionSetValue)a;
            }
            return null;
        }

        /// <summary>
        /// **描述:** `获取实体或preImage中的指定选项集字段的整数值。`
        /// **适用场景:** `在Plugin中，当需要从目标实体或其PreImage中获取选项集字段的整数值，且不确定该字段存在于哪个映像时使用。`
        /// **常见用法:** `int? statusValue = targetEntity.GetOptionValueOrDefault("statecode", preImageEntity);`
        /// **性能表现:** `快且稳定。`
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="attr">字段逻辑名</param>
        /// <param name="entPr">preImage</param>
        /// <returns>选项集的值</returns>
        public static int? GetOptionValueOrDefault(this Entity entity, string attr, Entity entPr)
        {
            if (entity.Contains(attr))
            {
                return entity.GetOptionValueOrDefault(attr);
            }

            return entPr.GetOptionValueOrDefault(attr);
        }

        /// <summary>
        /// **描述:** `获取实体的指定选项集字段的整数值。`
        /// **适用场景:** `当需要获取选项集字段的原始整数值以进行逻辑比较或数据处理时。`
        /// **常见用法:** `var new_status = Entity.GetOptionValueOrDefault("new_status");`
        /// **性能表现:** `快且稳定。`
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="attr">字段逻辑名</param>
        /// <returns>选项集的值</returns>
        public static int? GetOptionValueOrDefault(this Entity entity, string attr)
        {
            if (entity.Contains(attr) && entity[attr] != null)
            {
                var a = entity[attr];
                if (a.GetType() == typeof(AliasedValue))
                {
                    return (int)(((OptionSetValue)((AliasedValue)a).Value).Value);
                }
                return (int)(((OptionSetValue)a).Value);
            }
            return null;
        }

        /// <summary>
        /// **描述:** `获取实体的指定字段的格式化标签值，通常用于选项集、布尔值或查找字段的显示名称。`
        /// **适用场景:** `需要获取字段的用户友好显示文本（例如，选项集的标签，而不是其整数值）进行展示时。`
        /// **常见用法:** `var new_approvalstatus_label = Entity.GetFormattedValueOrDefault("new_approvalstatus");`
        /// **性能表现:** `快且稳定，直接从FormattedValues集合中获取。`
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="attr">字段逻辑名</param>
        /// <returns>字段的格式化标签值</returns>
        public static string GetFormattedValueOrDefault(this Entity entity, string attr)
        {
            if (entity.FormattedValues.Contains(attr) && entity[attr] != null)
                return entity.FormattedValues[attr];
            return null;
        }

        /// <summary>
        /// **描述:** `获取实体的指定选项集字段的选中项的描述文本（如果定义了）。`
        /// **适用场景:** `需要显示选项集值的详细描述信息（而非标签）时，例如在提示或详细视图中。`
        /// **常见用法:** `var new_approvalstatus_description = Entity.GetOptionDescriptionOrDefault(OrganizationServiceAdmin, "new_approvalstatus");`
        /// **性能表现:** `涉及元数据查询（GetAttributeMetadata），性能开销较大，应谨慎使用或缓存结果。`
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="OrganizationService">组织服务，通常是OrganizationServiceAdmin</param>
        /// <param name="attr">字段逻辑名</param>
        /// <returns>选项集的描述字符串</returns>
        public static string GetOptionDescriptionOrDefault(this Entity entity, IOrganizationService OrganizationService, string attr)
        {
            var value = entity.GetOptionValueOrDefault(attr);
            if (value.HasValue)
            {
                PicklistAttributeMetadata meta = OrganizationService.GetAttributeMetadata<PicklistAttributeMetadata>(entity.LogicalName, attr);
                OptionMetadata[] optionList = meta.OptionSet.Options.ToArray();
                var Description = optionList == null || !optionList.Any() ? null : optionList.FirstOrDefault(op => op.Value.Equals(value.Value))?.Description;
                return Description != null && Description.UserLocalizedLabel != null ? Description.UserLocalizedLabel.Label : null;
            }
            return null;
        }
        #endregion

        #region Int、Decimal、Double
        /// <summary>
        /// **描述:** `获取实体的指定字段的整数值。`
        /// **适用场景:** `获取整数类型字段的值。`
        /// **常见用法:** `var new_quoteqty = Entity.GetIntOrDefault("new_quoteqty") ?? 0;`
        /// **性能表现:** `快且稳定。`
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="attr">字段逻辑名</param>
        /// <returns>整数</returns>
        public static int? GetIntOrDefault(this Entity entity, string attr)
        {
            if (entity.Contains(attr) && entity[attr] != null)
            {
                var a = entity[attr];
                if (a.GetType() == typeof(AliasedValue))
                {
                    return (int)(((AliasedValue)a).Value);
                }
                return (int)a;
            }
            return null;
        }

        /// <summary>
        /// **描述:** `获取实体的指定字段的十进制数值，并按指定精度四舍五入。`
        /// **适用场景:** `获取十进制类型字段（如 Decimal）的值。`
        /// **常见用法:** `var new_shipmentqty = Entity.GetDecimalOrDefault("new_shipmentqty") ?? 0M;`
        /// **性能表现:** `快且稳定。`
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="attr">字段逻辑名</param>
        /// <param name="decimalPoint">精度，默认为4</param>
        /// <returns>十进制</returns>
        public static decimal? GetDecimalOrDefault(this Entity entity, string attr, int decimalPoint = 4)
        {
            if (entity.Contains(attr) && entity[attr] != null)
            {
                var a = entity[attr];
                if (a.GetType() == typeof(AliasedValue))
                {
                    return Math.Round(((decimal?)((AliasedValue)a).Value).Value * 1.00M, decimalPoint);
                }
                return Math.Round((decimal)a, decimalPoint);
            }
            return null;
        }

        /// <summary>
        /// **描述:** `获取实体或preImage中的指定十进制字段的值，并按指定精度四舍五入。`
        /// **适用场景:** `在Plugin中获取目标实体或其PreImage中的十进制类型字段（如 Decimal）的值，当不确定字段存在于哪个映像或需要从两者中取值时。`
        /// **常见用法:** `decimal? exchangeRate = targetEntity.GetDecimalOrDefault(preImageEntity, "exchangerate", 4);`
        /// **性能表现:** `快且稳定。`
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="preEntity">preImage</param>
        /// <param name="attr">字段逻辑名</param>
        /// <param name="decimalPoint">精度，默认为4</param>
        /// <returns>十进制</returns>
        public static decimal? GetDecimalOrDefault(this Entity entity, Entity preEntity, string attr, int decimalPoint = 4)
        {
            if (entity.Contains(attr))
            {
                return entity.GetDecimalOrDefault(attr, decimalPoint);
            }
            else
            {
                return preEntity.GetDecimalOrDefault(attr, decimalPoint);
            }
        }

        /// <summary>
        /// **描述:** `获取实体的指定字段的十进制数值，并根据提供的精度字典（键为字段逻辑名，值为精度）进行四舍五入。`
        /// **适用场景:** `当不同十进制字段需要基于字段逻辑名配置不同精度控制时。`
        /// **常见用法:** `var fieldPrecisions = new Dictionary<string, int>{ {"price", 2}, {"discountamount", 4} }; decimal? price = Entity.GetDecimalOrDefault("price", fieldPrecisions);`
        /// **性能表现:** `快且稳定。`
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="attr">字段逻辑名</param>
        /// <param name="FieldsAccuracy">字段与精度的字典，当字典不含有该字段时，精度默认为4</param>
        /// <returns>十进制</returns>
        public static decimal? GetDecimalOrDefault(this Entity entity, string attr, Dictionary<string, int> FieldsAccuracy)
        {
            int decimalPoint = 4;
            if (FieldsAccuracy != null && FieldsAccuracy.ContainsKey(attr))
                decimalPoint = FieldsAccuracy[attr];

            if (entity.Contains(attr) && entity[attr] != null)
            {
                var a = entity[attr];
                if (a.GetType() == typeof(AliasedValue))
                {
                    var attribute = (AliasedValue)a;
                    if (FieldsAccuracy != null && FieldsAccuracy.ContainsKey(attribute.AttributeLogicalName))
                        decimalPoint = FieldsAccuracy[attribute.AttributeLogicalName];

                    return Math.Round(((decimal?)attribute.Value).Value * 1.00M, decimalPoint);
                }
                return Math.Round((decimal)a, decimalPoint);
            }
            return null;
        }

        /// <summary>
        /// **描述:** `获取实体的指定字段的浮点数值，并按指定精度四舍五入。`
        /// **适用场景:** `获取浮点数类型字段（如 Float/Double）的值，并控制其小数位数。`
        /// **常见用法:** `var new_orderqty = Entity.GetDoubleOrDefault("new_orderqty") ?? 0D;`
        /// **性能表现:** `快且稳定。`
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="attr">字段逻辑名</param>
        /// <param name="decimalPoint">精度，默认为4</param>
        /// <returns>浮点数</returns>
        public static double? GetDoubleOrDefault(this Entity entity, string attr, int decimalPoint = 4)
        {
            if (entity.Contains(attr) && entity[attr] != null)
            {
                var a = entity[attr];
                if (a.GetType() == typeof(AliasedValue))
                {
                    return Math.Round(((double?)((AliasedValue)a).Value).Value * 1.00D, decimalPoint);
                }
                return Math.Round((double)a, decimalPoint);
            }
            return null;
        }

        /// <summary>
        /// **描述:** `获取实体的指定字段的浮点数值，并根据提供的精度字典（键为字段逻辑名，值为精度）进行四舍五入。`
        /// **适用场景:** `当不同浮点数字段需要基于字段逻辑名配置不同精度控制时。`
        /// **常见用法:** `var fieldPrecisions = new Dictionary<string, int>{ {"latitude", 6}, {"longitude", 6} }; double? latitude = Entity.GetDoubleOrDefault("latitude", fieldPrecisions);`
        /// **性能表现:** `快且稳定。`
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="attr">字段逻辑名</param>
        /// <param name="FieldsAccuracy">字段与精度的字典，当字典不含有该字段时，精度默认为4</param>
        /// <returns>浮点数</returns>
        public static double? GetDoubleOrDefault(this Entity entity, string attr, Dictionary<string, int> FieldsAccuracy)
        {
            int decimalPoint = 4;
            if (FieldsAccuracy != null && FieldsAccuracy.ContainsKey(attr))
                decimalPoint = FieldsAccuracy[attr];

            if (entity.Contains(attr) && entity[attr] != null)
            {
                var a = entity[attr];
                if (a.GetType() == typeof(AliasedValue))
                {
                    var attribute = (AliasedValue)a;
                    if (FieldsAccuracy != null && FieldsAccuracy.ContainsKey(attribute.AttributeLogicalName))
                        decimalPoint = FieldsAccuracy[attribute.AttributeLogicalName];

                    return Math.Round(((double?)attribute.Value).Value * 1.00D, decimalPoint);
                }
                return Math.Round((double)a, decimalPoint);
            }
            return null;
        }
        #endregion

        #region Money
        /// <summary>
        /// **描述:** `获取实体的指定货币字段的十进制值，并按指定精度四舍五入。`
        /// **适用场景:** `获取货币类型字段（如 Money）的数值，当不需要考虑币种特定精度，或币种精度与字段精度一致时。`
        /// **常见用法:** `var new_totalamount = Entity.GetMoneyOrDefault("new_totalamount") ?? 0M;`
        /// **性能表现:** `快且稳定。`
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="attr">字段逻辑名</param>
        /// <param name="decimalPoint">精度，默认为4</param>
        /// <returns>货币的值</returns>
        public static decimal? GetMoneyOrDefault(this Entity entity, string attr, int decimalPoint = 4)
        {
            if (entity.Contains(attr) && entity[attr] != null)
            {
                var a = entity[attr];
                if (a.GetType() == typeof(AliasedValue))
                {
                    return Math.Round(((Money)((AliasedValue)a).Value).Value * 1.00M, decimalPoint);
                }
                return Math.Round(((Money)a).Value * 1.00M, decimalPoint);
            }
            //return null;
            return 0m; //很多地方使用这个值进行运算 导致null+N 值不对的问题
        }

        /// <summary>
        /// **描述:** `获取实体或preImage中的指定货币字段的十进制值，并按指定精度四舍五入。`
        /// **适用场景:** `在Plugin中获取目标实体或其PreImage中的货币字段值，当不需要考虑币种特定精度时。`
        /// **常见用法:** `decimal? actualRevenue = targetEntity.GetMoneyOrDefault("actualrevenue", preImageEntity, 2);`
        /// **性能表现:** `快且稳定。`
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="preEntity">preImage</param>
        /// <param name="attr">字段逻辑名</param>
        /// <param name="decimalPoint">精度，默认为4</param>
        /// <returns>货币的值</returns>
        public static decimal? GetMoneyOrDefault(this Entity entity, string attr, Entity preEntity, int decimalPoint = 4)
        {
            if (entity.Contains(attr))
            {
                return entity.GetMoneyOrDefault(attr, decimalPoint);
            }

            if (preEntity != null)
            {
                return preEntity.GetMoneyOrDefault(attr, decimalPoint);
            }

            //return null;
            return 0m; //很多地方使用这个值进行运算 导致null+N 值不对的问题
        }

        /// <summary>
        /// **描述:** `获取实体的指定货币字段的十进制值，并根据提供的精度字典进行四舍五入。`
        /// **适用场景:** `当不同货币字段需要基于字段逻辑名配置不同精度控制时（不考虑币种自身精度）。`
        /// **常见用法:** `var fieldPrecisions = new Dictionary<string, int>{ {"totalamount", 2}, {"tax", 4} }; decimal? totalAmount = Entity.GetMoneyOrDefault("totalamount", fieldPrecisions);`
        /// **性能表现:** `快且稳定。`
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="attr">字段逻辑名</param>
        /// <param name="FieldsAccuracy">字段与精度的字典，当字典不含有该字段时，精度默认为4</param>
        /// <returns>货币的值</returns>
        public static decimal? GetMoneyOrDefault(this Entity entity, string attr, Dictionary<string, int> FieldsAccuracy)
        {
            int decimalPoint = 4;
            if (FieldsAccuracy != null && FieldsAccuracy.ContainsKey(attr))
                decimalPoint = FieldsAccuracy[attr];

            if (entity.Contains(attr) && entity[attr] != null)
            {
                var a = entity[attr];
                if (a.GetType() == typeof(AliasedValue))
                {
                    var attribute = (AliasedValue)a;
                    if (FieldsAccuracy != null && FieldsAccuracy.ContainsKey(attribute.AttributeLogicalName))
                        decimalPoint = FieldsAccuracy[attribute.AttributeLogicalName];

                    return Math.Round(((Money)attribute.Value).Value * 1.00M, decimalPoint);
                }
                return Math.Round(((Money)a).Value * 1.00M, decimalPoint);
            }
            //return null;
            return 0m; //很多地方使用这个值进行运算 导致null+N 值不对的问题
        }

        /// <summary>
        /// **描述:** `获取指定实体的所有货币、十进制、浮点数类型字段的逻辑名及其各自的CRM定义精度，并缓存结果。`
        /// **适用场景:** `需要批量获取实体数字段精度信息，通常用于初始化或在需要重复获取精度信息的场景中提高性能（通过缓存）。`
        /// **常见用法:** `var account_Precision = OrganizationServiceAdmin.GetNumberPrecision("account");`
        /// **性能表现:** `首次调用（缓存未命中时）涉及元数据查询，性能较低；后续调用（缓存命中时）非常快。`
        /// </summary>
        /// <param name="OrganizationServiceAdmin">组织服务，通常是OrganizationServiceAdmin</param>
        /// <param name="EntityName">实体逻辑名</param>
        /// <returns>该实体的货币、十进制、浮点数字段的字段逻辑名与精度的字典</returns>
        public static Dictionary<string, int> GetNumberPrecision(this IOrganizationService OrganizationServiceAdmin, string EntityName)
        {
            string entityFieldsAccuracy = CacheHelper.CreateInstance(OrganizationServiceAdmin).GetValue(EntityName, "EntityFieldsAccuracy");

            Dictionary<string, int> FieldsAccuracy = new Dictionary<string, int>();

            if (string.IsNullOrWhiteSpace(entityFieldsAccuracy))
            {
                //获取money、decimal精度
                RetrieveEntityRequest retrieveEntityRequest = new RetrieveEntityRequest
                {
                    LogicalName = EntityName,
                    EntityFilters = EntityFilters.Attributes,
                };
                RetrieveEntityResponse retrieveEntityResponse = (RetrieveEntityResponse)OrganizationServiceAdmin.Execute(retrieveEntityRequest);
                EntityMetadata entityMetadata = retrieveEntityResponse.EntityMetadata;
                foreach (AttributeMetadata attributeMetadata in entityMetadata.Attributes)
                {
                    int? precision = null;
                    switch (attributeMetadata.AttributeType)
                    {
                        case AttributeTypeCode.Money:
                            precision = (attributeMetadata as MoneyAttributeMetadata)?.Precision; break;
                        case AttributeTypeCode.Decimal:
                            precision = (attributeMetadata as DecimalAttributeMetadata)?.Precision; break;
                        case AttributeTypeCode.Double:
                            precision = (attributeMetadata as DoubleAttributeMetadata)?.Precision; break;
                        default: break;
                    }
                    if (precision.HasValue)
                        FieldsAccuracy.Add(attributeMetadata.LogicalName, precision.Value);
                }
                CacheHelper.CreateInstance(OrganizationServiceAdmin).SetValue(EntityName, JsonConvert.SerializeObject(FieldsAccuracy), "EntityFieldsAccuracy");//创建实体数字精度缓存
            }
            else
            {
                FieldsAccuracy = JsonConvert.DeserializeObject<Dictionary<string, int>>(entityFieldsAccuracy);
            }
            return FieldsAccuracy;
        }
        #endregion

        #region Bool
        /// <summary>
        /// **描述:** `获取实体的指定布尔字段的值。`
        /// **适用场景:** `获取布尔（是/否，Two Options）类型字段的值。`
        /// **常见用法:** `var new_isdefault = Entity.GetBoolOrDefault("new_isdefault") ?? false;`
        /// **性能表现:** `快且稳定。`
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="attr">字段逻辑名</param>
        /// <returns>布尔值</returns>
        public static bool? GetBoolOrDefault(this Entity entity, string attr)
        {
            if (entity.Contains(attr) && entity[attr] != null)
            {
                var a = entity[attr];
                if (a.GetType() == typeof(AliasedValue))
                {
                    return (bool)(((AliasedValue)a).Value);
                }
                return (bool)a;
            }
            return null;
        }

        /// <summary>
        /// **描述:** `获取实体或preImage中的指定布尔字段的值。`
        /// **适用场景:** `在Plugin中获取目标实体或其PreImage中的布尔字段值，当不确定字段在哪个实体中时使用。`
        /// **常见用法:** `bool? doNotBulkEmail = targetEntity.GetBoolOrDefault(preImageEntity, "donotbulkemail");`
        /// **性能表现:** `快且稳定。`
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="preEntity">preImage</param>
        /// <param name="attr">字段逻辑名</param>
        /// <returns>布尔值</returns>
        public static bool? GetBoolOrDefault(this Entity entity, Entity preEntity, string attr)
        {
            if (entity.Contains(attr))
            {
                return entity.GetBoolOrDefault(attr);
            }
            else
            {
                return preEntity.GetBoolOrDefault(attr);
            }
        }
        #endregion

        #region EntityReference
        /// <summary>
        /// **描述:** `获取实体的指定查找 (EntityReference) 字段的记录ID字符串。`
        /// **适用场景:** `仅需要查找字段引用的记录ID时，例如用于构建查询条件或与其他ID比较。`
        /// **常见用法:** `string accountId = Entity.GetEFIdOrDefault("parentaccountid"); if (!string.IsNullOrEmpty(accountId)) { /* ... */ }`
        /// **性能表现:** `快且稳定。`
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="attr">字段逻辑名</param>
        /// <returns>该EntityReference字段的Id字符串</returns>
        public static string GetEFIdOrDefault(this Entity entity, string attr)
        {
            if (entity.Contains(attr) && entity[attr] != null)
            {
                var a = entity[attr];
                if (a.GetType() == typeof(AliasedValue))
                {
                    return ((EntityReference)((AliasedValue)a).Value).Id.ToString();
                }
                return ((EntityReference)a).Id.ToString();
            }
            return null;
        }

        /// <summary>
        /// **描述:** `获取实体或preImage中的指定查找 (EntityReference) 字段的对象。`
        /// **适用场景:** `在Plugin中获取目标实体或其PreImage中的查找字段对象，当不确定字段在哪个实体中时使用。`
        /// **常见用法:** `EntityReference ownerRef = targetEntity.GetEFOrDefault(preImageEntity, "ownerid");`
        /// **性能表现:** `快且稳定。`
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="preEntity">preImage</param>
        /// <param name="attr">字段逻辑名</param>
        /// <returns>EntityReference值</returns>
        public static EntityReference GetEFOrDefault(this Entity entity, Entity preEntity, string attr)
        {
            if (entity.Contains(attr))
            {
                return entity.GetEFOrDefault(attr);
            }

            if (preEntity != null)
            {
                return preEntity.GetEFOrDefault(attr);
            }

            return null;
        }

        /// <summary>
        /// **描述:** `获取实体的指定查找 (EntityReference) 字段的对象。`
        /// **适用场景:** `需要获取查找字段的完整引用对象（ID, LogicalName, Name），用于更新、创建关联或传递给其他需要EntityReference的方法。`
        /// **常见用法:** `var transactioncurrencyid = Entity.GetEFOrDefault("transactioncurrencyid");`
        /// **性能表现:** `快且稳定。`
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="attr">字段逻辑名</param>
        /// <returns>EntityReference值</returns>
        public static EntityReference GetEFOrDefault(this Entity entity, string attr)
        {
            if (entity.Contains(attr) && entity[attr] != null)
            {
                var a = entity[attr];
                if (a == null) return null;
                if (a.GetType() == typeof(AliasedValue))
                {
                    return (EntityReference)((AliasedValue)a).Value;
                }
                return (EntityReference)a;
            }
            return null;
        }

        /// <summary>
        /// **描述:** `获取实体的指定查找 (EntityReference) 字段的记录显示名称。`
        /// **适用场景:** `仅需要查找字段引用的记录的Name属性时，例如用于显示或日志记录。`
        /// **常见用法:** `string customerName = Entity.GetEFNameOrDefault("customerid"); Console.WriteLine("Customer: " + customerName);`
        /// **性能表现:** `快且稳定，直接访问EntityReference的Name属性。`
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="attr">字段逻辑名</param>
        /// <returns>该EntityReference字段的Name字符串</returns>
        public static string GetEFNameOrDefault(this Entity entity, string attr)
        {
            if (entity.Contains(attr) && entity[attr] != null)
            {
                var a = entity[attr];
                if (a == null) return null;
                if (a.GetType() == typeof(AliasedValue))
                {
                    return (((EntityReference)((AliasedValue)a).Value)?.Name) ?? string.Empty;
                }
                return (((EntityReference)a)?.Name) ?? string.Empty;
            }
            return null;
        }

        /// <summary>
        /// **描述:** `获取实体指定查找字段关联记录的特定代码字段的值。`
        /// **适用场景:** `需要获取查找字段所引用记录的某个业务编码（如客户编码、产品编码等），而不仅仅是ID或Name。`
        /// **常见用法:** `var new_country_code = Entity.GetEFCodeOrDefault(OrganizationServiceAdmin, "new_country_id", "new_code");`
        /// **性能表现:** `依赖一次Retrieve操作来获取关联记录的字段，性能适中。`
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="OrganizationService">组织服务，通常是OrganizationServiceAdmin</param>
        /// <param name="attr">字段逻辑名</param>
        /// <param name="codeField">该实体的code字段逻辑名</param>
        /// <returns>该EntityReference字段的Code字段的值</returns>
        public static string GetEFCodeOrDefault(this Entity entity, IOrganizationService OrganizationService, string attr, string codeField = "new_code")
        {
            codeField = string.IsNullOrWhiteSpace(codeField) ? "new_code" : codeField;

            EntityReference ef = entity.GetEFOrDefault(attr);
            if (ef is null) return null;
            Entity e = ef.ToEntity(OrganizationService, new ColumnSet(codeField));
            return e.GetStringOrDefault(codeField);
        }

        /// <summary>
        /// **描述:** `获取实体或PreImage中指定查找字段关联记录的特定代码字段的值。`
        /// **适用场景:** `Plugin中需要获取查找字段（可能在Target或PreImage中）所引用记录的业务编码，当不确定字段在哪个实体中时使用。`
        /// **常见用法:** `string productNumber = targetEntity.GetEFCodeOrDefault(preImageEntity, service, "new_productid", "productnumber");`
        /// **性能表现:** `依赖一次Retrieve操作来获取关联记录的字段，性能适中。`
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="preEntity">preImage</param>
        /// <param name="OrganizationService">组织服务，通常是OrganizationServiceAdmin</param>
        /// <param name="attr">字段逻辑名</param>
        /// <param name="codeField">该实体的code字段逻辑名</param>
        /// <returns>该EntityReference字段的Code字段的值</returns>
        public static string GetEFCodeOrDefault(this Entity entity, Entity preEntity, IOrganizationService OrganizationService, string attr, string codeField = "new_code")
        {
            codeField = string.IsNullOrWhiteSpace(codeField) ? "new_code" : codeField;

            if (entity.Contains(attr))
            {
                return GetEFCodeOrDefault(entity, OrganizationService, attr, codeField);
            }
            else
            {
                return GetEFCodeOrDefault(preEntity, OrganizationService, attr, codeField);
            }
        }

        /// <summary>
        /// **描述:** `将源实体 (dataEntity) 中指定字段的值复制到目标实体 (entity) 的对应字段。`
        /// **适用场景:** `实体间数据迁移或复制部分属性，如根据模板创建新记录，或在不同类型但有共同字段的实体间转换数据。`
        /// **常见用法:** `newQuoteDetail.CopyData(opportunityProduct, new Dictionary<string, string> { {"productid", "productid"}, {"quantity", "quantity"} });`
        /// **性能表现:** `快且稳定，操作在内存中进行。`
        /// </summary>
        /// <param name="entity">目标实体</param>
        /// <param name="dataEntity">数据来源实体</param>
        /// <param name="fieldsMap">字段与字段的映射关系(key为数据来源实体的字段逻辑名，value为目标实体的字段逻辑名)</param>
        public static void CopyData(this Entity entity, Entity dataEntity, Dictionary<string, string> fieldsMap)
        {
            foreach (var key in fieldsMap.Keys)
            {
                if (dataEntity.Contains(key))
                    entity[fieldsMap[key]] = dataEntity[key];
            }
        }

        /// <summary>
        /// **描述:** `根据唯一名称在指定实体的指定字段中查询单条记录，并可选择性地指定返回列和状态。`
        /// **适用场景:** `通过某个唯一标识（如名称、编码）查找特定记录以获取其引用或其它信息，通常用于数据导入或集成场景中查找关联记录。`
        /// **常见用法:** `var new_country = OrganizationServiceAdmin.GetEntityReference("new_country", "001", "new_code");`
        /// **性能表现:** `依赖一次RetrieveMultiple查询，性能适中。`
        /// </summary>
        /// <param name="OrganizationService">组织服务，通常是OrganizationServiceAdmin</param>
        /// <param name="uniqueName">查询字段的值</param>
        /// <param name="entityName">实体逻辑名</param>
        /// <param name="queryField">查询的字段逻辑名</param>
        /// <param name="columns">要查询的字段</param>
        /// <param name="statecode">实体记录的状态，通常0表示有效，1表示停用</param>
        /// <returns>该字符串对应的实体记录</returns>
        public static Entity GetEntityReference(this IOrganizationService OrganizationService, string entityName, string uniqueName, string queryField = "new_name", string columns = "new_name", int? statecode = null)
        {
            if (string.IsNullOrWhiteSpace(uniqueName)) return null;
            QueryExpression query = new QueryExpression(entityName);
            query.TopCount = 1;
            query.ColumnSet.AddColumn(entityName + "id");
            query.ColumnSet.AddColumns(columns.Split(','));
            query.Criteria.AddCondition(queryField, ConditionOperator.Equal, uniqueName);
            if (statecode != null)
                query.Criteria.AddCondition("statecode", ConditionOperator.Equal, statecode.Value);
            var ec = OrganizationService.RetrieveMultiple(query);
            return ec != null && ec.Entities.Count > 0 ? ec.Entities[0] : null;
        }

        /// <summary>
        /// **描述:** `根据EntityReference对象（包含实体逻辑名和ID）查询并返回完整的实体记录，可指定需要的列。`
        /// **适用场景:** `已有记录的EntityReference，需要获取该记录的更多字段信息时，例如在获取查找字段后需要其关联实体的其他属性。`
        /// **常见用法:** `var new_contact = Entity.GetEFOrDefault("new_contact_id").ToEntity(OrganizationServiceAdmin, new ColumnSet("new_emailaddress"));`
        /// **性能表现:** `依赖一次Retrieve查询，性能适中。`
        /// </summary>
        /// <param name="ef">EntityReference字段</param>
        /// <param name="OrganizationService">组织服务，通常是OrganizationServiceAdmin</param>
        /// <param name="cols">要查询的字段</param>
        /// <returns>该EntityReference字段对应的实体记录</returns>
        public static Entity ToEntity(this EntityReference ef, IOrganizationService OrganizationService, ColumnSet cols)
        {
            if (ef == null) return null;
            else
                return OrganizationService.Retrieve(ef.LogicalName, ef.Id, cols);
        }

        /// <summary>
        /// **描述:** `检查指定ID的实体记录是否存在于CRM中。`
        /// **适用场景:** `在执行更新、删除或创建关联操作前，验证记录的有效性或是否存在，避免操作不存在的记录。`
        /// **常见用法:** `if (service.EntityExists("account", accountId)) { // Proceed with update or other operations }`
        /// **性能表现:** `依赖一次RetrieveMultiple查询（仅查询ID，不请求数据列），性能适中。`
        /// </summary>
        /// <param name="service">组织服务，通常是OrganizationServiceAdmin</param>
        /// <param name="entityName">实体逻辑名</param>
        /// <param name="entityId">实体Id</param>
        /// <returns>布尔值，指示该实体是否存在该Id的实体记录</returns>
        public static bool EntityExists(this IOrganizationService service, string entityName, Guid entityId)
        {
            QueryExpression query = new QueryExpression(entityName);
            query.Criteria.AddCondition(new ConditionExpression(entityName + "id", ConditionOperator.Equal, entityId));

            EntityCollection result = service.RetrieveMultiple(query);

            return result.Entities.Count > 0;
        }

        /// <summary>
        /// **描述:** `根据现有实体对象创建一个新的、只包含逻辑名和ID的实体对象。`
        /// **适用场景:** `需要更新实体记录的特定字段时，创建一个"干净"的实体对象以避免意外更新其他已加载的字段。常用于部分更新操作。`
        /// **常见用法:** `Entity upAddress = e.ToNewEntity();`
        /// **性能表现:** `快且稳定，操作在内存中进行。`
        /// </summary>
        /// <param name="e">源Entity字段</param>
        /// <returns>新的Entity变量</returns>
        public static Entity ToNewEntity(this Entity e)
        {
            Entity upEntity = new Entity(e.LogicalName, e.Id);
            return upEntity;
        }

        /// <summary>
        /// **描述:** `根据EntityReference对象创建一个新的、只包含逻辑名和ID的实体对象。`
        /// **适用场景:** `从一个EntityReference快速创建一个用于更新、删除或作为其他操作参数的"干净"的实体对象。`
        /// **常见用法:** `EntityReference contactRef = new EntityReference("contact", contactId); Entity contactToUpdate = contactRef.ToNewEntity(); contactToUpdate["emailaddress1"] = "new@example.com"; service.Update(contactToUpdate);`
        /// **性能表现:** `快且稳定，操作在内存中进行。`
        /// </summary>
        /// <param name="e">源EntityReference字段</param>
        /// <returns>新的Entity变量</returns>
        public static Entity ToNewEntity(this EntityReference e)
        {
            Entity upEntity = new Entity(e.LogicalName, e.Id);
            return upEntity;
        }
        #endregion

        #region ExecuteMultiple
        /// <summary>
        /// **描述:** `检查ExecuteMultipleResponse中是否存在错误，并提取错误信息列表。`
        /// **适用场景:** `执行批量操作（ExecuteMultipleRequest）后，判断操作是否部分或全部失败，并获取每个失败操作的详细错误信息。`
        /// **常见用法:** `ExecuteMultipleResponse response = (ExecuteMultipleResponse)service.Execute(request); List<string> errors; if (response.ValidationExecution(out errors)) { foreach(var err in errors) { LogError(err); } }`
        /// **性能表现:** `快且稳定，直接操作Response对象。`
        /// </summary>
        /// <param name="multipleResponse">ExecuteMultipleResponse</param>
        /// <param name="execerror">ExecuteMultipleResponse的异常信息</param>
        /// <returns>是否存在异常</returns>
        public static bool ValidationExecution(this OrganizationResponse multipleResponse, out List<string> execerror)
        {
            execerror = new List<string>();
            if (multipleResponse is ExecuteMultipleResponse)
            {
                if (multipleResponse.Results.ContainsKey("IsFaulted") && Convert.ToBoolean(multipleResponse.Results["IsFaulted"]))
                {
                    var errorResult = (ExecuteMultipleResponseItemCollection)multipleResponse.Results["Responses"];
                    if (errorResult != null && errorResult.Count > 0)
                    {
                        var errorList = errorResult.Where(p => p.Fault != null && !string.IsNullOrWhiteSpace(p.Fault.Message)).ToList();
                        foreach (var error in errorList)
                        {
                            execerror.Add(error.Fault.Message);
                        }
                    }

                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// **描述:** `检查ExecuteMultipleResponse中是否存在错误，如果存在则抛出包含所有错误信息的异常。`
        /// **适用场景:** `执行批量操作后，如果期望所有操作都成功，则可用此方法快速失败并统一提示所有错误。`
        /// **常见用法:** `try { batchResponse.ValidationExecution(); // All operations succeeded } catch (Exception ex) { // Handle batch failure, ex.Message contains all errors }`
        /// **性能表现:** `快且稳定，直接操作Response对象。`
        /// </summary>
        /// <param name="multipleResponse">ExecuteMultipleResponse</param>
        /// <exception cref="Exception">包含ExecuteMultipleResponse的异常信息的Exception</exception>
        public static void ValidationExecution(this OrganizationResponse multipleResponse)
        {
            StringBuilder errMsg = new StringBuilder();
            List<string> execerror = new List<string>();
            if (ValidationExecution(multipleResponse, out execerror))
            {
                if (execerror != null && execerror.Any())
                {
                    foreach (var str in execerror)
                    {
                        errMsg.AppendLine(str);
                    }
                }
                throw new Exception(!string.IsNullOrWhiteSpace(errMsg.ToString()) ? errMsg.ToString() : "执行出现未知错误");
            }
        }

        /// <summary>
        /// **描述:** `检查ExecuteMultipleResponse中是否存在错误，并返回一个拼接了所有错误信息的单一字符串。`
        /// **适用场景:** `执行批量操作后，获取一个单一字符串形式的错误汇总，用于日志记录或简单显示。`
        /// **常见用法:** `string errorSummary = batchResponse.ValidationExecutionStr(); if (!string.IsNullOrEmpty(errorSummary)) { LogError(errorSummary); }`
        /// **性能表现:** `快且稳定，直接操作Response对象。`
        /// </summary>
        /// <param name="multipleResponse">ExecuteMultipleResponse</param>
        /// <returns>ExecuteMultipleResponse的异常信息</returns>
        public static string ValidationExecutionStr(this OrganizationResponse multipleResponse)
        {
            StringBuilder errMsg = new StringBuilder();
            List<string> execerror = new List<string>();
            if (ValidationExecution(multipleResponse, out execerror))
            {
                if (execerror != null && execerror.Any())
                {
                    foreach (var str in execerror)
                    {
                        errMsg.AppendLine(str);
                    }
                }
                return !string.IsNullOrWhiteSpace(errMsg.ToString()) ? errMsg.ToString() : "执行出现未知错误";
            }
            return null;
        }
        #endregion

        #region Retrieve
        /// <summary>
        /// **描述:** `通过QueryExpression自动处理分页，查询并返回所有匹配的实体记录。`
        /// **适用场景:** `需要检索可能超过CRM单次查询上限（默认5000条）的大量数据时使用QueryExpression。`
        /// **常见用法:** `QueryExpression contactQuery = new QueryExpression("contact"); contactQuery.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0); EntityCollection allActiveContacts = service.GetAllEntities(contactQuery);`
        /// **性能表现:** `依赖多次RetrieveMultiple查询，数据量大时性能较低。每次分页查询会产生一次SDK调用。`
        /// </summary>
        /// <param name="OrganizationService">组织服务</param>
        /// <param name="qe">QueryExpression</param>
        /// <param name="pageIndex">分页，页码，默认为1</param>
        /// <param name="pageSize">分页，每页大小，默认为5000</param>
        /// <returns>实体数据集</returns>
        public static EntityCollection GetAllEntities(this IOrganizationService OrganizationService, QueryExpression qe, int pageIndex = 1, int pageSize = 5000)
        {
            EntityCollection result = new EntityCollection()
            {
                EntityName = qe.EntityName,
                MoreRecords = false,
                TotalRecordCount = 0,
            };

            qe.PageInfo = new PagingInfo()
            {
                Count = pageSize,
                PageNumber = pageIndex,
                ReturnTotalRecordCount = true,
            };

            while (true)
            {
                EntityCollection ec = OrganizationService.RetrieveMultiple(qe);
                if (ec != null && ec.Entities.Count > 0) result.Entities.AddRange(ec.Entities);

                if (ec.MoreRecords)
                {
                    qe.PageInfo.PageNumber++;
                    qe.PageInfo.PagingCookie = ec.PagingCookie;
                }
                else break;
            }

            return result;
        }

        /// <summary>
        /// **描述:** `通过FetchExpression自动处理分页，查询并返回所有匹配的实体记录。`
        /// **适用场景:** `使用FetchXML查询且需要检索可能超过CRM单次查询上限（默认5000条）的大量数据。`
        /// **常见用法:** `string fetchXml = "<fetch><entity name='account'><attribute name='name'/></entity></fetch>"; FetchExpression orderFetch = new FetchExpression(fetchXml); EntityCollection allOrders = service.GetAllEntities(orderFetch);`
        /// **性能表现:** `依赖多次RetrieveMultiple查询，数据量大时性能较低。每次分页查询会产生一次SDK调用。`
        /// </summary>
        /// <param name="OrganizationService">组织服务</param>
        /// <param name="qe">FetchExpression，当使用fetchXml查询数据时使用</param>
        /// <param name="pageIndex">分页，页码，默认为1</param>
        /// <param name="pageSize">分页，每页大小，默认为5000</param>
        /// <returns>实体数据集</returns>
        public static EntityCollection GetAllEntities(this IOrganizationService OrganizationService, FetchExpression qe, int pageIndex = 1, int pageSize = 5000)
        {
            XDocument xml = XDocument.Parse(qe.Query);
            XElement entityNode = xml.Descendants("entity").FirstOrDefault();
            string entityName = entityNode?.Attribute("name")?.Value;

            EntityCollection result = new EntityCollection()
            {
                EntityName = entityName,
                MoreRecords = false,
                TotalRecordCount = 0,
            };

            //检查是否输入了页码和页大小，并初始化
            XElement fetchNode = xml.Descendants("fetch").FirstOrDefault();
            string page = fetchNode?.Attribute("page")?.Value;
            if (string.IsNullOrWhiteSpace(page))
            {
                page = pageIndex.ToString();
                fetchNode?.SetAttributeValue("page", pageIndex.ToString());
            }

            string count = fetchNode?.Attribute("count")?.Value;
            if (string.IsNullOrWhiteSpace(count))
                fetchNode?.SetAttributeValue("count", pageSize.ToString());

            //更新fetchxml
            qe.Query = PaginationFetchXml(qe.Query, pageIndex, pageSize);

            int PageNumber = int.Parse(page);
            while (true)
            {
                EntityCollection ec = OrganizationService.RetrieveMultiple(qe);
                if (ec != null && ec.Entities.Count > 0) result.Entities.AddRange(ec.Entities);

                if (ec.MoreRecords)
                {
                    PageNumber++;
                    fetchNode?.SetAttributeValue("page", PageNumber.ToString());
                    qe.Query = xml.ToString();//更新fetchxml
                }
                else break;
            }

            return result;
        }

        /// <summary>
        /// 格式化FetchXml，分页查询
        /// </summary>
        /// <param name="fetchXml"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static string PaginationFetchXml(string fetchXml, int pageIndex, int pageSize)
        {
            XDocument xml = XDocument.Parse(fetchXml);
            XElement entityNode = xml.Descendants("entity").FirstOrDefault();
            string entityName = entityNode?.Attribute("name")?.Value;

            //检查是否输入了页码和页大小，并初始化
            XElement fetchNode = xml.Descendants("fetch").FirstOrDefault();
            string page = fetchNode?.Attribute("page")?.Value;
            if (string.IsNullOrWhiteSpace(page))
            {
                page = pageIndex.ToString();
                fetchNode?.SetAttributeValue("page", pageIndex.ToString());
            }

            string count = fetchNode?.Attribute("count")?.Value;
            if (string.IsNullOrWhiteSpace(count))
                fetchNode?.SetAttributeValue("count", pageSize.ToString());

            string returnTotalCount = fetchNode?.Attribute("returntotalrecordcount")?.Value;
            if (string.IsNullOrWhiteSpace(returnTotalCount))
                fetchNode?.SetAttributeValue("returntotalrecordcount", "true");

            return xml.ToString();
        }

        /// <summary>
        /// **描述:** `查询指定实体逻辑名，指定过滤条件，指定查询字段的实体记录，通常用于检查是否存在重复记录，如：查询产品编码为"001"且客户编号为"010"的产品价格明细，如果存在多条则抛出异常`
        /// **适用场景:** `验证数据唯一性，或查找符合特定唯一条件的单个记录。`
        /// **常见用法:** `Entity account = OrganizationServiceAdmin.QueryEntity("account", new Dictionary<string, object>() { { "accountnumber", "001" } }, new string[] { "name" });`
        /// **性能表现:** `依赖一次RetrieveMultiple查询，性能适中。`
        /// </summary>
        /// <param name="organizationService">组织服务</param>
        /// <param name="entityName">实体逻辑名</param>
        /// <param name="conditions">过滤条件</param>
        /// <param name="columnSet">要查询的字段</param>
        /// <param name="multiException">如果查询的数据超过1条，则抛出异常，该字段为异常信息</param>
        /// <returns>实体记录</returns>
        /// <exception cref="Exception">查询结果超过1条记录时抛出异常</exception>
        public static Entity QueryEntity(this IOrganizationService organizationService, string entityName, Dictionary<string, object> conditions = null, string[] columnSet = null, string multiException = null)
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
        /// **描述:** `根据指定条件（字段等于特定值）查询实体记录集合。`
        /// **适用场景:** `需要使用QueryExpression查询一组条件为Equal的多条记录。所有条件以AND连接。`
        /// **常见用法:** `var ec2 = OrganizationServiceAdmin.QueryEntites("new_account_address", new Dictionary<string, object>() { { "new_account_id", PrimaryEntityId },{ "statecode", (int)StateCode.Active } });`
        /// **性能表现:** `依赖一次RetrieveMultiple查询，性能适中。`
        /// </summary>
        /// <param name="organizationService">组织服务</param>
        /// <param name="entityName">实体逻辑名</param>
        /// <param name="conditions">过滤条件</param>
        /// <param name="columnSet">要查询的字段</param>
        /// <returns>实体数据集</returns>
        public static EntityCollection QueryEntites(this IOrganizationService organizationService, string entityName, Dictionary<string, object> conditions, string[] columnSet = null)
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
            return organizationService.RetrieveMultiple(query);
        }
        #endregion

        #region AttributeMetadata
        /// <summary>
        /// **描述:** `获取指定实体指定字段的通用元数据 (AttributeMetadata)。`
        /// **适用场景:** `需要获取字段的基础元数据信息，如类型、显示名称、是否必须、最大长度等，不包含特定类型（如选项集）的详细信息。`
        /// **常见用法:** `AttributeMetadata attrMeta = service.GetAttributeMetadata("account", "industrycode"); var displayName = attrMeta.DisplayName.UserLocalizedLabel.Label;`
        /// **性能表现:** `依赖一次RetrieveEntityRequest，涉及元数据查询，性能开销较大，应谨慎使用或缓存结果。`
        /// </summary>
        /// <param name="OrganizationService">组织服务，通常是OrganizationServiceAdmin</param>
        /// <param name="entityName">实体逻辑名</param>
        /// <param name="attributeName">字段逻辑名</param>
        /// <returns>指定实体指定字段的元数据</returns>
        public static AttributeMetadata GetAttributeMetadata(this IOrganizationService OrganizationService, string entityName, string attributeName)
        {
            RetrieveEntityRequest entityRequest = new RetrieveEntityRequest
            {
                EntityFilters = EntityFilters.Attributes,
                LogicalName = entityName
            };

            RetrieveEntityResponse entityResponse = (RetrieveEntityResponse)OrganizationService.Execute(entityRequest);
            EntityMetadata entityMetadata = entityResponse.EntityMetadata;
            if (entityMetadata == null || entityMetadata.Attributes.Length == 0) return null;
            return entityMetadata.Attributes.FirstOrDefault(attr => attr.LogicalName.Equals(attributeName));
        }

        /// <summary>
        /// **描述:** `获取指定实体指定字段的特定类型元数据 (如PicklistAttributeMetadata, StringAttributeMetadata)。`
        /// **适用场景:** `需要获取字段特定类型的详细元数据，例如获取选项集的所有选项、状态字段的状态码和状态原因的映射等。`
        /// **常见用法:** `PicklistAttributeMetadata optionsetMeta = service.GetAttributeMetadata<PicklistAttributeMetadata>("opportunity", "opportunityratingcode"); var options = optionsetMeta.OptionSet.Options;`
        /// **性能表现:** `依赖一次RetrieveAttributeRequest，涉及元数据查询，性能开销较大，应谨慎使用或缓存结果。`
        /// </summary>
        /// <typeparam name="T">泛型，期望的AttributeMetadata子类型</typeparam>
        /// <param name="OrganizationService">组织服务，通常是OrganizationServiceAdmin</param>
        /// <param name="entityName">实体逻辑名</param>
        /// <param name="attributeName">字段逻辑名</param>
        /// <returns>指定实体指定字段的元数据</returns>
        public static T GetAttributeMetadata<T>(this IOrganizationService OrganizationService, string entityName, string attributeName) where T : AttributeMetadata
        {
            RetrieveAttributeRequest retrieveAttributeRequest = new RetrieveAttributeRequest
            {
                EntityLogicalName = entityName, // 实体名称
                LogicalName = attributeName, // 属性名称
                RetrieveAsIfPublished = true
            };

            RetrieveAttributeResponse retrieveAttributeResponse = (RetrieveAttributeResponse)OrganizationService.Execute(retrieveAttributeRequest);

            // 获取选项集的选项
            var picklistAttributeMetadata = (T)retrieveAttributeResponse.AttributeMetadata;

            return picklistAttributeMetadata;
        }
        #endregion

        #region Other
        /// <summary>
        /// **描述:** `获取当前CRM环境的组织ID。`
        /// **适用场景:** `需要获取当前组织的唯一标识符时，例如用于某些API调用或日志记录。`
        /// **常见用法:** `Guid orgId = service.GetCurrentOrganization(); Log("Processing for organization: " + orgId);`
        /// **性能表现:** `依赖一次RetrieveCurrentOrganizationRequest，性能适中。结果通常不会改变，可以考虑缓存。`
        /// </summary>
        /// <param name="organizationService">组织服务，通常是OrganizationServiceAdmin</param>
        /// <returns>当前系统的组织Id</returns>
        public static Guid GetCurrentOrganization(this IOrganizationService organizationService)
        {
            RetrieveCurrentOrganizationRequest req = new RetrieveCurrentOrganizationRequest();
            RetrieveCurrentOrganizationResponse res = (RetrieveCurrentOrganizationResponse)organizationService.Execute(req);
            OrganizationDetail organizationDetail = res.Detail;
            return organizationDetail.OrganizationId;
        }

        /// <summary>
        /// **描述:** `获取指定实体逻辑名称对应的ObjectTypeCode。`
        /// **适用场景:** `在某些需要实体类型代码的API或操作中使用，例如构建FetchXML中的link-entity的objecttypecode属性，或与某些旧版API交互。`
        /// **常见用法:** `int accountTypeCode = service.GetObjectTypeCode("account"); string fetchXml = $"<fetch><entity name='contact'><link-entity name='account' from='accountid' to='parentcustomerid' alias='acc' link-type='outer'><attribute name='name'/><filter type='and'><condition attribute='objecttypecode' operator='eq' value='{accountTypeCode}'/></filter></link-entity></entity></fetch>";`
        /// **性能表现:** `依赖一次RetrieveEntityRequest，涉及元数据查询，性能开销较大，应谨慎使用或缓存结果。`
        /// </summary>
        /// <param name="service">组织服务，通常是OrganizationServiceAdmin</param>
        /// <param name="entityLogicalName">实体逻辑名</param>
        /// <returns>指定实体的objecttypecode</returns>
        public static int GetObjectTypeCode(this IOrganizationService service, string entityLogicalName)
        {
            RetrieveEntityRequest request = new RetrieveEntityRequest
            {
                LogicalName = entityLogicalName,
                EntityFilters = EntityFilters.Entity
            };

            RetrieveEntityResponse response = (RetrieveEntityResponse)service.Execute(request);
            EntityMetadata entityMetadata = response.EntityMetadata;

            return entityMetadata.ObjectTypeCode.Value;
        }

        /// <summary>
        /// **描述:** `获取当前CRM环境的基础货币 (Base Currency) 的EntityReference。`
        /// **适用场景:** `需要获取系统默认的基础币种信息时，例如用于金额计算的基准或设置默认币种。`
        /// **常见用法:** `EntityReference baseCurrencyRef = service.GetBaseCurrency(); if (recordCurrency.Id != baseCurrencyRef.Id) { /* Convert to base currency */ }`
        /// **性能表现:** `依赖两次查询 (GetCurrentOrganization, Retrieve)，性能适中。基础货币通常不变，可以考虑缓存。`
        /// </summary>
        /// <param name="organizationService">组织服务，通常是OrganizationServiceAdmin</param>
        /// <returns>当前环境的默认币种记录</returns>
        public static EntityReference GetBaseCurrency(this IOrganizationService organizationService)
        {
            Entity organization = organizationService.Retrieve("organization", organizationService.GetCurrentOrganization(), new ColumnSet("basecurrencyid"));
            return organization.GetEFOrDefault("basecurrencyid");
        }
        #endregion

        #region 逻辑判断
        /// <summary>
        /// **描述:** `判断指定字段是否存在于当前实体或其PreImage中，并返回包含该字段的实体（优先返回当前实体）。`
        /// **适用场景:** `在读取字段值之前进行有效性检查，确保字段存在且有值，避免NullReferenceException。`
        /// **常见用法:** `if (record.ContainsAndNotNull("primarycontactid")) { var contactRef = record.GetAttributeValue<EntityReference>("primarycontactid"); // Safe to use contactRef }`
        /// **性能表现:** `快且稳定。`
        /// </summary>
        /// <param name="entity">当前实体</param>
        /// <param name="image">preImage</param>
        /// <param name="attr">字段逻辑名</param>
        /// <returns>返回包含指定字段的当前实体或preImage，优先返回当前实体</returns>
        public static Entity SelectEntityContainField(this Entity entity, Entity image, string attr)
        {
            if (image == null)
            {
                return entity;
            }
            if (entity.Contains(attr))
            {
                return entity;
            }
            else
            {
                return image;
            }
        }

        /// <summary>
        /// **描述:** `判断当前实体是否包含指定的多个字段中的至少一个。`
        /// **适用场景:** `在需要检查实体是否包含多个字段中的至少一个时使用。`
        /// **常见用法:** `if (!Entity.ContainsFieldsOr("new_status", "new_name")) return;`
        /// **性能表现:** `快且稳定。`
        /// </summary>
        /// <param name="entity">当前实体</param>
        /// <param name="fields">字段逻辑名</param>
        /// <returns>是否包含任意一个指定字段</returns>
        public static bool ContainsFieldsOr(this Entity entity, params string[] fields)
        {
            bool isContainsOr = false;

            if (fields.Length > 0)
            {
                foreach (string f in fields)
                {
                    isContainsOr = isContainsOr || entity.Contains(f);
                }
            }
            else
            {
                throw new Exception("Please Check Code!");
            }
            return isContainsOr;
        }

        /// <summary>
        /// **描述:** `判断当前实体是否包含全部指定字段`
        /// **适用场景:** `在需要检查实体是否包含所有指定字段时使用。`
        /// **常见用法:** `if (Entity.ContainsFieldsAnd("new_name", "ownerid"))`
        /// **性能表现:** `快且稳定。`
        /// </summary>
        /// <param name="entity">当前实体</param>
        /// <param name="fields">字段逻辑名</param>
        /// <returns>是否包含全部指定字段</returns>
        public static bool ContainsFieldsAnd(this Entity entity, params string[] fields)
        {
            bool isContainsAnd = true;

            if (fields.Length > 0)
            {
                foreach (string f in fields)
                {
                    isContainsAnd = isContainsAnd && entity.Contains(f);
                }
            }
            else
            {
                throw new Exception("Please Check Code!");
            }
            return isContainsAnd;
        }

        /// <summary>
        /// **描述:** `判断当前实体包含此字段且该字段的值不为空`
        /// **适用场景:** `在读取字段值之前进行有效性检查，确保字段存在且有值，避免NullReferenceException。`
        /// **常见用法:** `if (Entity.ContainsAndNotNull("new_name"))`
        /// **性能表现:** `快且稳定。`
        /// </summary>
        /// <param name="entity">当前实体</param>
        /// <param name="attr">字段逻辑名</param>
        /// <returns>当前实体是否包含此字段且该字段的值不为空</returns>
        public static bool ContainsAndNotNull(this Entity entity, string attr)
        {
            return entity.Contains(attr) && entity[attr] != null;
        }
        #endregion
    }

    #region CRMDescription
    public class CRMDescription : Attribute
    {
        /// <summary>
        /// 字段类型
        /// </summary>
        public AttributeTypeCode attributeType { get; set; }

        /// <summary>
        /// 字段名
        /// </summary>
        public string attributeName { get; set; }

        public CRMDescription(AttributeTypeCode attributeType, string attributeName)
        {
            this.attributeType = attributeType;
            this.attributeName = attributeName;
        }
    }

    public class CRMMoneyDescription : CRMDescription
    {
        public int decimalPoint { get; set; }

        public bool currencyPoint { get; set; }

        public CRMMoneyDescription(string attributeName, int decimalPoint = 4, bool currencyPoint = true) : base(AttributeTypeCode.Money, attributeName)
        {
            this.attributeName = attributeName;
            this.decimalPoint = decimalPoint;
            this.currencyPoint = currencyPoint;
        }
    }

    public class CRMNumberDescription : CRMDescription
    {
        public int decimalPoint { get; set; }

        public bool fieldPoint { get; set; }

        public CRMNumberDescription(AttributeTypeCode attributeType, string attributeName, int decimalPoint = 4, bool fieldPoint = true) : base(attributeType, attributeName)
        {
            this.attributeName = attributeName;
            this.decimalPoint = decimalPoint;
            this.fieldPoint = fieldPoint;
        }
    }

    public class CRMEntityReferenceDescription : CRMDescription
    {
        public string codeField { get; set; }

        public CRMEntityReferenceDescription(string attributeName, string codeField = null) : base(AttributeTypeCode.Money, attributeName)
        {
            this.attributeName = attributeName;
            if (!string.IsNullOrWhiteSpace(codeField)) this.codeField = codeField;
        }
    }

    public class CRMDatetimeDescription : CRMDescription
    {
        public string format { get; set; }

        public CRMDatetimeDescription(string attributeName, string format = "yyyy-MM-dd HH:mm:ss") : base(AttributeTypeCode.Money, attributeName)
        {
            this.attributeName = attributeName;
            this.format = format;
        }
    }
    #endregion
}
