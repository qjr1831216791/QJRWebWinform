using BaiduMapService.Model;
using CommonHelper.Model;
using CommonHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace BaiduMapService.Service
{
    /// <summary>
    /// 百度地图行政区域数据处理
    /// </summary>
    public static class BaiduMapTownShipArea
    {
        /// <summary>
        /// 初始化百度地图行政区域
        /// </summary>
        public static void BaiduMapTownShipInitialization()
        {
            if (BaiduMapTownShip.options != null) return;

            string path = $"{AppDomain.CurrentDomain.BaseDirectory}/BaseData/{ConfigFileNameEnum.BaiduMapTownshipArea}";
            var dt = ExcelHelper.ExcelToDataTable(path, 0, false);

            //先按照省份排序
            var orderByProvince = dt.AsEnumerable().OrderBy(r => r[1]).ToList();

            BaiduMapTownShip.options = new List<BaiduMapTownShipItem>();
            BaiduMapTownShip.dict = new Dictionary<string, string>();
            BaiduMapTownShip.table = orderByProvince.Select(e => new BaiduMapTownShipRow()
            {
                provinceName = e[0].ToString(),
                provinceCode = e[1].ToString(),
                cityName = e[2].ToString(),
                cityCode = e[3].ToString(),
                countyName = e[4].ToString(),
                countyCode = e[5].ToString(),
                streetName = e[6].ToString(),
                streetCode = e[7].ToString()
            }).ToList();

            #region 取出所有的省份和城市
            for (int proIndex = 0; proIndex < orderByProvince.Count; proIndex++)
            {
                var row = orderByProvince[proIndex];
                string provinceName = row[0].ToString();
                string provinceCode = row[1].ToString();
                if (BaiduMapTownShip.dict.ContainsKey(provinceCode)) continue;

                var item = new BaiduMapTownShipItem(provinceCode, provinceName);
                BaiduMapTownShip.options.Add(item);
                BaiduMapTownShip.dict.Add(provinceCode, provinceName);

                //由于先对省份进行了排序，所以城市的循环可以以省份的index开始
                for (int rIndex = proIndex; rIndex < orderByProvince.Count; rIndex++)
                {
                    var row2 = orderByProvince[rIndex];
                    string r_provinceCode = row2[1].ToString();

                    //由于对省份进行了排序，所以当出现不同省份的时候，剩下的行也不会是当前省份的了
                    if (!provinceCode.Equals(r_provinceCode)) break;

                    string cityName = row2[2].ToString();
                    string cityCode = row2[3].ToString();
                    if (item.children == null) item.children = new List<BaiduMapTownShipItem>();
                    if (item.dict == null) item.dict = new Dictionary<string, string>();
                    if (!item.dict.ContainsKey(cityCode))
                    {
                        item.children.Add(new BaiduMapTownShipItem(cityCode, cityName));
                        item.dict.Add(cityCode, cityName);
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// 获取百度城市代码
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static BaiduMapTownShipRow GetCityCode(string code)
        {
            if (string.IsNullOrEmpty(code)) return null;
            else if (BaiduMapTownShip.table == null || !BaiduMapTownShip.table.Any()) return null;

            string prefix = code.Substring(0, 2);//获取前三位代码
            return BaiduMapTownShip.table
                .Where(r => r.provinceCode.StartsWith(prefix))
                .FirstOrDefault(e => e.cityCode.Equals(code) || e.countyCode.Equals(code) || e.streetCode.Equals(code));
        }
    }
}
