using CommonHelper.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace ThinkAPIService.Model
{
    public static class HolidayAndAnniversaryData
    {
        /// <summary>
        /// 节假日-按照年月分组
        /// </summary>
        public static HashSet<string> holidaysDict = new HashSet<string>();

        /// <summary>
        /// 节假日-按照日期
        /// </summary>
        public static HashSet<string> holidaysDayDict = new HashSet<string>();

        /// <summary>
        /// 节假日
        /// </summary>
        public static List<HolidayDateItem> holidays { get; set; } = new List<HolidayDateItem>();

        /// <summary>
        /// 纪念日-按照年月分组
        /// </summary>
        public static HashSet<string> anniversariesDict = new HashSet<string>();

        /// <summary>
        /// 纪念日-按照日期分组
        /// </summary>
        public static Dictionary<string, List<AnniversaryDataItem>> anniversaries { get; set; } = new Dictionary<string, List<AnniversaryDataItem>>();
    }

    public static class AnniversariesData
    {
        /// <summary>
        /// 纪念日配置数据
        /// </summary>
        public static List<AnniversaryDataItem> data { get; set; }

        public static void AnniversariesDateInitialization()
        {
            if (AnniversariesData.data != null) return;

            string path = $"{AppDomain.CurrentDomain.BaseDirectory}/BaseData/{ConfigFileNameEnum.AnniversaryDate}";

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                AnniversariesData.data = JsonConvert.DeserializeObject<List<AnniversaryDataItem>>(json);
            }
            else
            {
                throw new FileNotFoundException($"纪念日配置数据缺失：{ConfigFileNameEnum.AnniversaryDate}");
            }
        }
    }

    public class ThinkHADateItem
    {
        private string _date;

        public string date
        {
            get
            {
                return this._date;
            }
            set
            {
                DateTime dt = DateTime.Parse(value);
                this._date = value;
                year = dt.Year;
                month = dt.Month;
                day = dt.Day;
            }
        }

        public int year { get; set; }

        public int month { get; set; }

        public int day { get; set; }

        public ThinkHADateItem(DateTime dt)
        {
            date = dt.ToString("yyyy-MM-dd");
            year = dt.Year;
            month = dt.Month;
            day = dt.Day;
        }

        public ThinkHADateItem(string date)
        {
            DateTime dt = DateTime.Parse(date);
            this._date = date;
            year = dt.Year;
            month = dt.Month;
            day = dt.Day;
        }

        public ThinkHADateItem() { }
    }

    /// <summary>
    /// 节假日
    /// </summary>
    public class HolidayDateItem : ThinkHADateItem
    {
        public HolidayDateItem(DateTime dt) : base(dt)
        {
        }

        public HolidayDateItem(string dt) : base(dt)
        {
        }

        public HolidayDateItem() { }

        public string holiday_CN_Name { get; set; }

        /// <summary>
        /// 1: 节假日 2: 调休上班
        /// </summary>
        public string status { get; set; }
    }

    /// <summary>
    /// 纪念日
    /// </summary>
    public class AnniversaryDataItem : ThinkHADateItem
    {
        public AnniversaryDataItem(DateTime dt) : base(dt)
        {
        }

        public AnniversaryDataItem(string dt) : base(dt)
        {
        }

        public AnniversaryDataItem() { }

        /// <summary>
        /// 描述
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        public string label { get; set; }

        /// <summary>
        /// 日期类型
        /// </summary>
        public int type { get; set; }

        /// <summary>
        /// 日期区间的长度
        /// </summary>
        public int dayrange { get; set; }
    }

    public enum AnniversaryType
    {
        /// <summary>
        /// 每年
        /// </summary>
        [Description("EveryYear")]
        EveryYear = 0,

        /// <summary>
        /// 每月
        /// </summary>
        [Description("EveryMonth")]
        EveryMonth = 1,

        /// <summary>
        /// 指定日期
        /// </summary>
        [Description("Specified")]
        Specified = 2,

        /// <summary>
        /// 日期区间
        /// </summary>
        [Description("Range")]
        Range = 3,
    }
}
