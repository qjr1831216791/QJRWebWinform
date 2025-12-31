using System.Collections.Generic;

namespace ThinkAPIService.Model
{
    public class HolidayItem
    {
        public string desc { get; set; }
        public string festival { get; set; }
        public List<HolidayDateStatus> list { get; set; }
        public int list_num { get; set; }
        public string name { get; set; }
        public string rest { get; set; }
    }

    public class HolidayDateStatus
    {
        public string date { get; set; }

        /// <summary>
        /// 1是放假，2是调休上班
        /// </summary>
        public string status { get; set; }
    }

    public class HolidayModel
    {
        public string year { get; set; }
        public string year_month { get; set; }
        public string holiday { get; set; }
        public List<HolidayItem> holiday_array { get; set; }
    }
}
