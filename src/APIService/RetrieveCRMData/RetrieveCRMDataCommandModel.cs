using System.Collections.Generic;

namespace APIService.RetrieveCRMData
{
    public class GetTreeDataInput
    {
        public string envir { get; set; }

        public string entityName { get; set; }

        public List<CustomField> customFields { get; set; } = new List<CustomField>();
    }

    public class CustomField
    {
        public string logicalName { get; set; }
    }

    public class CRMDataResult
    {
        public int TotalRecordCount { get; set; }

        public List<Dictionary<string, object>> data { get; set; } = new List<Dictionary<string, object>>();
    }

    public class GetCRMDataByFetchXml
    {
        public string envir { get; set; }

        public string fetchXml { get; set; }

        public int pageIndex { get; set; } = 1;

        public int pageSize { get; set; } = 5000;
    }
}
