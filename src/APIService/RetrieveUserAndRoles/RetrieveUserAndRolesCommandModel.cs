using System.Collections.Generic;

namespace APIService.RetrieveUserAndRoles
{
    public class GetUserAndRolesInput
    {
        public string envir { get; set; }

        public List<CustomUserField> customFields { get; set; } = new List<CustomUserField>();
    }

    public class CustomUserField
    {
        public string logicalName { get; set; }
    }

    public class GetUserAndRolesResult
    {
        public int TotalRecordCount { get; set; }

        public List<Dictionary<string, object>> data { get; set; } = new List<Dictionary<string, object>>();
    }
}
