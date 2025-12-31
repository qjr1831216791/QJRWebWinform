using CommonHelper.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;

namespace APIService.Default
{
    /// <summary>
    /// Default Command
    /// </summary>
    public class DefaultCommand : BaseCommand
    {
        /// <summary>
        /// Test CRM Service
        /// </summary>
        /// <returns></returns>
        public ResultModel TestCRMService()
        {
            ResultModel result = new ResultModel();
            try
            {
                QueryExpression qe = new QueryExpression("account");
                qe.TopCount = 1;
                qe.ColumnSet = new ColumnSet("name");
                EntityCollection ec = OrganizationServiceAdmin.RetrieveMultiple(qe);

                result.Success(message: "Successfully retrieved data from CRM");
            }
            catch (Exception)
            {
                result.Failed("Failed to retrieve data from CRM");
            }
            return result;
        }

        /// <summary>
        /// 获取网站配置的CRM环境信息
        /// </summary>
        /// <returns></returns>
        public ResultModel GetCRMEnvironments()
        {
            ResultModel result = new ResultModel();
            try
            {
                WebConfigModel webConfig = WebConfigModel.Instance;

                var data = webConfig.environment.Select(e => new
                {
                    label = e.name,
                    value = e.key
                }).ToList();

                result.Success(message: "Successfully to load web config", data: data);
            }
            catch (Exception)
            {
                result.Failed("Failed to load web config");
            }
            return result;

        }

        /// <summary>
        /// 测试记录日志
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        public ResultModel TestLogTrace(string level, string message = "Hello World")
        {
            ResultModel result = new ResultModel();
            try
            {
                switch (level.ToLower())
                {
                    case "info":
                        Log.InfoMsg(message);
                        break;
                    case "warning":
                        Log.WarningMsg(message);
                        break;
                    case "debug":
                        Log.DebugMsg(message);
                        break;
                    case "error":
                        Log.ErrorMsg(message);
                        break;
                    case "exception":
                        throw new Exception(message);
                    default:
                        Log.InfoMsg(message);
                        break;
                }
                result.Success(data: Log.writeLogConfig);
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
                throw;
            }
            return result;
        }
    }
}
