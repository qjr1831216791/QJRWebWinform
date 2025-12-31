using System;
using System.Collections.Generic;
using System.Linq;

namespace APIService.SyncConfiguration
{
    public class SyncConfigurationModel
    {
        public object ecFrom { get; set; }
        public object ecTo { get; set; }
    }

    public class SystemParameterModel
    {
        public Guid Id { get; set; }
        public string new_name { get; set; }
        public string new_value { get; set; }
        public string new_desc { get; set; }
    }

    public class AutoNumberModel
    {
        public Guid Id { get; set; }
        public string new_name { get; set; }
        public string new_nofieldname { get; set; }
        public string new_useexpression { get; set; }
        public string new_prefix { get; set; }
        public string new_isyear { get; set; }
        public int? new_length { get; set; }
        public string new_ismonth { get; set; }
        public string new_istwo { get; set; }
        public string new_isday { get; set; }
    }

    public class DuplicateDetectModel
    {
        public Guid Id { get; set; }
        public string new_name { get; set; }
        public string new_message { get; set; }

        public List<DuplicateDetectDetail> detail { get; set; }

        public bool hasDetail { get { return detail != null && detail.Any(); } }
    }

    public class DuplicateDetectDetail
    {
        public Guid Id { get; set; }
        public string new_name { get; set; }
        public string new_null_ne_null { get; set; }
    }

    public class SumRelationshipDetailModel
    {
        public Guid Id { get; set; }
        public string new_name { get; set; }
        public string new_listentity { get; set; }
        public string new_reffield { get; set; }
        public string new_total { get; set; }
        public string new_list { get; set; }
        public string new_type { get; set; }
    }

    public class RibbonRuleModel
    {
        public Guid Id { get; set; }
        public string new_name { get; set; }
        public string new_desc { get; set; }

        public List<RibbonRuleDetailModel> detail { get; set; }

        public bool hasDetail { get { return detail != null && detail.Any(); } }
    }

    public class RibbonRuleDetailModel
    {
        public Guid Id { get; set; }
        public string new_roleid { get; set; }
    }

    public class CommonDeleteCheckPluginStepModel
    {
        public Guid Id { get; set; }

        public string name { get; set; }

        public string configuration { get; set; }

        public string stage { get; set; }

        public string sdkmessageid { get; set; }

        public string primaryobjecttypecode { get; set; }

        public string secureconfig { get; set; }
    }

    public class GetImportConfigModel
    {
        public Guid Id { get; set; }

        public string new_name { get; set; }

        public string new_etn { get; set; }

        public string new_importtype { get; set; }
    }

    public class GetLanguageConfigResult
    {
        public int TotalRecordCount { get; set; }

        public List<GetLanguageConfigModel> data { get; set; } = new List<GetLanguageConfigModel>();
    }

    public class GetLanguageConfigModel
    {
        public Guid Id { get; set; }

        public string new_name { get; set; }

        public string new_language_id { get; set; }

        public string new_content { get; set; }

        public string new_note { get; set; }
    }

    public class GetDocumenttemplatesModel
    {
        public Guid Id { get; set; }

        public string name { get; set; }

        public int? languagecode { get; set; }

        public string languageName { get; set; }

        public string associatedentityLogicalName { get; set; }

        public string associatedentityName { get; set; }

        public string documenttype { get; set; }
    }

    public class SyncSystemConfigsInput
    {
        public string envirFrom { get; set; }
        public string envirTo { get; set; }
        public string entityName { get; set; }
        public List<string> configList { get; set; } = new List<string>();
    }

    public class MultipleLanguageContrastsModel
    {
        public Guid Id { get; set; }

        public string new_entity_name { get; set; }

        public string new_multi_attribute_name { get; set; }

        public string new_code { get; set; }

        public string createdon { get; set; }
    }

    public class DataLanguageConfigsResult
    {
        public int TotalRecordCount { get; set; }

        public List<DataLanguageConfigsModel> data { get; set; } = new List<DataLanguageConfigsModel>();
    }

    public class DataLanguageConfigsModel
    {
        public Guid Id { get; set; }

        public string new_entity_name { get; set; }

        public string new_attribute_name { get; set; }

        public string new_language_id { get; set; }

        public string new_value { get; set; }

        public string new_code { get; set; }

        public string new_data_id { get; set; }

        public string new_language_code { get; set; }

        public string createdon { get; set; }
    }
}
