using ExcelDataReader;
using System.Data;
using System.IO;

namespace CommonHelper
{
    public static class ExcelHelper
    {
        /// <summary>
        /// 读取Excel文件为DataSet，支持.xls、.xlsx和.csv格式
        /// </summary>
        /// <param name="relativeFilePath">Excel文件的相对路径</param>
        /// <param name="useHeaderRow">是否使用第一行作为列名，默认为true</param>
        /// <returns>包含所有工作表的DataSet对象，如果文件格式不支持或读取失败则返回null</returns>
        /// <exception cref="FileNotFoundException">当指定的文件不存在时抛出</exception>
        public static DataSet ExcelToDataSet(string relativeFilePath, bool useHeaderRow = true)
        {
            var filePath = Path.GetFullPath(relativeFilePath);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("找不到文件！");
            }
            var extension = Path.GetExtension(filePath).ToLower();
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                IExcelDataReader reader = null;
                if (extension == ".xls")
                {
                    reader = ExcelReaderFactory.CreateBinaryReader(stream);
                }
                else if (extension == ".xlsx")
                {
                    reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                }
                else if (extension == ".csv")
                {
                    reader = ExcelReaderFactory.CreateCsvReader(stream);
                }
                if (reader == null) return null;

                DataSet ds;
                using (reader)
                {
                    ds = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        UseColumnDataType = false,
                        ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = useHeaderRow // 第一行包含列名
                        }
                    });
                }
                return ds;
            }
        }

        /// <summary>
        /// 读取Excel指定sheet为DataTable
        /// </summary>
        /// <param name="relativeFilePath"></param>
        /// <param name="sheet"></param>
        /// <param name="useHeaderRow"></param>
        /// <returns></returns>
        public static DataTable ExcelToDataTable(string relativeFilePath, int sheet = 0, bool useHeaderRow = true)
        {
            var ds = ExcelToDataSet(relativeFilePath, useHeaderRow);
            if (ds == null) return null;
            return ds.Tables[sheet];
        }
    }
}
