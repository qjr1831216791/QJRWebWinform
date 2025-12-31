using APIService.RetrieveCRMData;
using CommonHelper.Model;
using System.Windows;

namespace QJRWebWinform.WPF.Controllers
{
    public class RetrieveCRMDataController : DynamicControllerBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="mainWindow"></param>
        public RetrieveCRMDataController(Window mainWindow) : base(mainWindow)
        {
        }

        public override string Name => "RetrieveCRMData";

        /// <summary>
        /// 查询CRM Data
        /// </summary>
        /// <param name="parameters">JSON参数：GetTreeDataInput 对象</param>
        /// <returns></returns>
        public virtual object RetrieveCRMData(string parameters)
        {
            var input = DeserializeParameters<GetTreeDataInput>(parameters);
            SetParameters(parameters); // 设置参数以提取 crmEnv
            return Command<RetrieveCRMDataCommand>().RetrieveCRMData(input?.envir, input?.entityName, input?.customFields);
        }

        /// <summary>
        /// 查询CRM Data
        /// </summary>
        /// <param name="parameters">JSON参数：GetCRMDataByFetchXml 对象</param>
        /// <returns></returns>
        public virtual object RetrieveCRMDataByFetchXml(string parameters)
        {
            var input = DeserializeParameters<GetCRMDataByFetchXml>(parameters);
            SetParameters(parameters); // 设置参数以提取 crmEnv
            return Command<RetrieveCRMDataCommand>().RetrieveCRMDataByFetchXml(input?.envir, input?.fetchXml, input?.pageIndex ?? 1, input?.pageSize ?? 5000);
        }
    }
}
