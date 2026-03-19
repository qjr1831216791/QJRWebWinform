using CommonHelper.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace APIService.APITest
{
    /// <summary>
    /// APITestCommand
    /// </summary>
    public class APITestCommand : BaseCommand
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
