using CommonHelper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using ThinkAPIService.API;
using ThinkAPIService.Model;

namespace APIService.Holiday
{
    public class HolidayCommand : BaseCommand
    {
        /// <summary>
        /// 通过ThinkAPI获取节假日信息
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public ResultModel GetHolidayList(string date)
        {
            ResultModel result = new ResultModel();
            try
            {
                DateTime dt = DateTime.Today;
                if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out dt)) { }
                date = dt.ToString("yyyy-MM");

                if (HolidayAndAnniversaryData.holidaysDict.Contains(date))
                {
                    result.Success(data: HolidayAndAnniversaryData.holidays);
                    return result;
                }
                HolidayAndAnniversaryData.holidaysDict.Add(date);

                CalendarMonthCommand cmd = new CalendarMonthCommand();
                result = cmd.Execute(dt);

                if (result.isSuccess && result.data != null && ((HolidayModel)result.data).holiday_array.Any())
                {
                    List<HolidayDateItem> holidays = new List<HolidayDateItem>();
                    foreach (var holiday in ((HolidayModel)result.data).holiday_array)
                    {
                        if (holiday.list == null || !holiday.list.Any()) continue;

                        var dates = holiday.list.Select(e => new HolidayDateItem(e.date)
                        {
                            holiday_CN_Name = holiday.name,
                            status = e.status
                        }).Where(e => !HolidayAndAnniversaryData.holidaysDayDict.Contains(e.date)).ToList();

                        if (dates != null && dates.Any())
                        {
                            foreach (var d in dates)
                            {
                                HolidayAndAnniversaryData.holidaysDayDict.Add(d.date);
                            }

                            holidays.AddRange(dates);
                        }
                    }
                    HolidayAndAnniversaryData.holidays.AddRange(holidays);
                    result.Success(data: HolidayAndAnniversaryData.holidays);
                }
                else if (!result.isSuccess && result.code.Equals(217701))
                {
                    //没有指定日期的假期信息
                    result.Success();
                }
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetHolidayList");
                Log.LogException(ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 获取纪念日信息
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public ResultModel GetAnniversaryList(string date)
        {
            ResultModel result = new ResultModel();
            try
            {
                DateTime dt = DateTime.Today;
                if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out dt)) { }
                date = dt.ToString("yyyy-MM");

                if (HolidayAndAnniversaryData.anniversariesDict.Contains(date))
                {
                    result.Success(data: HolidayAndAnniversaryData.anniversaries);
                    return result;
                }

                HolidayAndAnniversaryData.anniversariesDict.Add(date);

                AnniversariesData.AnniversariesDateInitialization();

                #region 每年
                var everyYear = AnniversariesData.data.Where(e => e.type.Equals((int)AnniversaryType.EveryYear));
                if (everyYear != null && everyYear.Any())
                {
                    foreach (var day in everyYear)
                    {
                        if (!day.month.Equals(dt.Month)) continue;

                        string _date = DateTime.Parse($"{dt.Year}-{day.month}-{day.day}").ToString("yyyy-MM-dd");
                        if (!HolidayAndAnniversaryData.anniversaries.ContainsKey(_date))
                        {
                            HolidayAndAnniversaryData.anniversaries.Add(_date, new List<AnniversaryDataItem>());
                        }

                        var item = new AnniversaryDataItem(_date)
                        {
                            label = day.label,
                            description = day.description,
                            type = day.type,
                            dayrange = day.dayrange,
                        };

                        HolidayAndAnniversaryData.anniversaries[_date].Add(item);
                    }
                }
                #endregion

                #region 每月
                var everyMonth = AnniversariesData.data.Where(e => e.type.Equals((int)AnniversaryType.EveryMonth));
                if (everyMonth != null && everyMonth.Any())
                {
                    foreach (var day in everyMonth)
                    {
                        string _date = DateTime.Parse($"{dt.Year}-{dt.Month}-{day.day}").ToString("yyyy-MM-dd");
                        if (!HolidayAndAnniversaryData.anniversaries.ContainsKey(_date))
                        {
                            HolidayAndAnniversaryData.anniversaries.Add(_date, new List<AnniversaryDataItem>());
                        }

                        var item = new AnniversaryDataItem(_date)
                        {
                            label = day.label,
                            description = day.description,
                            type = day.type,
                            dayrange = day.dayrange,
                        };

                        HolidayAndAnniversaryData.anniversaries[_date].Add(item);
                    }
                }
                #endregion

                #region 指定日期
                var specified = AnniversariesData.data.Where(e => e.type.Equals((int)AnniversaryType.Specified));
                if (specified != null && specified.Any())
                {
                    foreach (var day in specified)
                    {
                        if (!day.year.Equals(dt.Year) || !day.month.Equals(dt.Month)) continue;

                        string _date = DateTime.Parse(day.date).ToString("yyyy-MM-dd");
                        if (!HolidayAndAnniversaryData.anniversaries.ContainsKey(_date))
                        {
                            HolidayAndAnniversaryData.anniversaries.Add(_date, new List<AnniversaryDataItem>());
                        }

                        var item = new AnniversaryDataItem(_date)
                        {
                            label = day.label,
                            description = day.description,
                            type = day.type,
                            dayrange = day.dayrange,
                        };

                        HolidayAndAnniversaryData.anniversaries[_date].Add(item);
                    }
                }
                #endregion

                #region 日期区间
                var range = AnniversariesData.data.Where(e => e.type.Equals((int)AnniversaryType.Range));
                if (range != null && range.Any())
                {
                    foreach (var day in range)
                    {
                        if (day.dayrange <= 0) continue;
                        else if (!day.year.Equals(dt.Year) || !day.month.Equals(dt.Month))
                        {
                            //需要判断指定年月查询时是否在本日期区间的范围内
                            var _dtr = DateTime.Parse(day.date).AddDays(day.dayrange);
                            if (_dtr.Year < dt.Year) continue;
                            else if (_dtr.Year.Equals(dt.Year) && _dtr.Month < dt.Month) continue;
                        }

                        for (int i = 0; i < day.dayrange; i++)
                        {
                            string _date = DateTime.Parse(day.date).AddDays(i).ToString("yyyy-MM-dd");
                            if (!HolidayAndAnniversaryData.anniversaries.ContainsKey(_date))
                            {
                                HolidayAndAnniversaryData.anniversaries.Add(_date, new List<AnniversaryDataItem>());
                            }
                            else
                            {
                                //由于日期区间可能存在跨年或者跨月，需要先判断当前数据是否已经在上个月创建，如果存在则跳过
                                if (HolidayAndAnniversaryData.anniversaries.Any(e => e.Value.Any(p => p.label.Equals(day.label)))) continue;
                            }

                            var item = new AnniversaryDataItem(_date)
                            {
                                label = day.label,
                                description = day.description,
                                type = day.type,
                                dayrange = day.dayrange,
                            };

                            HolidayAndAnniversaryData.anniversaries[_date].Add(item);
                        }
                    }
                }
                #endregion

                result.Success(data: HolidayAndAnniversaryData.anniversaries);
            }
            catch (Exception ex)
            {
                Log.ErrorMsg("GetAnniversaryList");
                Log.LogException(ex);
                throw ex;
            }
            return result;
        }
    }
}
