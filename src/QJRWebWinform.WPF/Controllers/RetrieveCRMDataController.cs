using APIService.RetrieveCRMData;
using CommonHelper.Model;
using CefSharp;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
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
        /// 查询CRM Data（核心业务逻辑）
        /// </summary>
        /// <param name="parameters">JSON参数：GetTreeDataInput 对象</param>
        /// <returns></returns>
        private ResultModel RetrieveCRMDataCore(string parameters)
        {
            var input = DeserializeParameters<GetTreeDataInput>(parameters);
            SetParameters(parameters); // 设置参数以提取 crmEnv
            return Command<RetrieveCRMDataCommand>().RetrieveCRMData(input?.envir, input?.entityName, input?.customFields);
        }

        /// <summary>
        /// 查询CRM Data（同步版本）
        /// </summary>
        /// <param name="parameters">JSON参数：GetTreeDataInput 对象</param>
        /// <returns></returns>
        public virtual object RetrieveCRMData(string parameters)
        {
            return RetrieveCRMDataCore(parameters);
        }

        /// <summary>
        /// 查询CRM Data（异步版本）
        /// </summary>
        /// <param name="parameters">JSON参数：GetTreeDataInput 对象</param>
        /// <param name="callback">JavaScript 回调函数</param>
        public virtual void RetrieveCRMData(string parameters, IJavascriptCallback callback)
        {
            Task.Run(() =>
            {
                try
                {
                    var result = RetrieveCRMDataCore(parameters);
                    var resultJson = JsonConvert.SerializeObject(result);
                    callback.ExecuteAsync(true, resultJson);
                }
                catch (Exception ex)
                {
                    callback.ExecuteAsync(false, ex.Message);
                }
            });
        }

        /// <summary>
        /// 查询CRM Data（核心业务逻辑）
        /// </summary>
        /// <param name="parameters">JSON参数：GetCRMDataByFetchXml 对象</param>
        /// <returns></returns>
        private ResultModel RetrieveCRMDataByFetchXmlCore(string parameters)
        {
            var input = DeserializeParameters<GetCRMDataByFetchXml>(parameters);
            SetParameters(parameters); // 设置参数以提取 crmEnv
            return Command<RetrieveCRMDataCommand>().RetrieveCRMDataByFetchXml(input?.envir, input?.fetchXml, input?.pageIndex ?? 1, input?.pageSize ?? 5000);
        }

        /// <summary>
        /// 查询CRM Data（同步版本）
        /// </summary>
        /// <param name="parameters">JSON参数：GetCRMDataByFetchXml 对象</param>
        /// <returns></returns>
        public virtual object RetrieveCRMDataByFetchXml(string parameters)
        {
            return RetrieveCRMDataByFetchXmlCore(parameters);
        }

        /// <summary>
        /// 查询CRM Data（异步版本）
        /// </summary>
        /// <param name="parameters">JSON参数：GetCRMDataByFetchXml 对象</param>
        /// <param name="callback">JavaScript 回调函数</param>
        public virtual void RetrieveCRMDataByFetchXml(string parameters, IJavascriptCallback callback)
        {
            Task.Run(() =>
            {
                try
                {
                    var result = RetrieveCRMDataByFetchXmlCore(parameters);
                    var resultJson = JsonConvert.SerializeObject(result);
                    callback.ExecuteAsync(true, resultJson);
                }
                catch (Exception ex)
                {
                    callback.ExecuteAsync(false, ex.Message);
                }
            });
        }
    }
}
