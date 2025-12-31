using APIService.RetrieveEntityMetadata;
using CommonHelper.Model;
using CefSharp;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace QJRWebWinform.WPF.Controllers
{
    /// <summary>
    /// 查询CRM实体元数据
    /// </summary>
    public class RetrieveEntityMetadataController : DynamicControllerBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="mainWindow"></param>
        public RetrieveEntityMetadataController(Window mainWindow) : base(mainWindow)
        {
        }

        public override string Name => "RetrieveEntityMetadata";

        /// <summary>
        /// 获取所有实体元数据（核心业务逻辑）
        /// </summary>
        /// <param name="parameters">JSON参数：{"envir": "string", "isCustomEntity": bool}</param>
        /// <returns></returns>
        private ResultModel GetAllEntityMetadataCore(string parameters)
        {
            var input = DeserializeParameters<GetAllEntityMetadataInput>(parameters);
            SetParameters(parameters); // 设置参数以提取 crmEnv
            return Command<RetrieveEntityMetadataCommand>().GetAllEntityMetadata(input?.envir, input?.isCustomEntity ?? false);
        }

        /// <summary>
        /// 获取所有实体元数据（同步版本）
        /// </summary>
        /// <param name="parameters">JSON参数：{"envir": "string", "isCustomEntity": bool}</param>
        /// <returns></returns>
        public virtual object GetAllEntityMetadata(string parameters)
        {
            return GetAllEntityMetadataCore(parameters);
        }

        /// <summary>
        /// 获取所有实体元数据（异步版本）
        /// </summary>
        /// <param name="parameters">JSON参数：{"envir": "string", "isCustomEntity": bool}</param>
        /// <param name="callback">JavaScript 回调函数</param>
        public virtual void GetAllEntityMetadata(string parameters, IJavascriptCallback callback)
        {
            Task.Run(() =>
            {
                try
                {
                    var result = GetAllEntityMetadataCore(parameters);
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
        /// 获取字段类型（核心业务逻辑）
        /// </summary>
        /// <param name="parameters">JSON参数：null 或 "{}"</param>
        /// <returns></returns>
        private ResultModel GetAttributeTypeListCore(string parameters)
        {
            SetParameters(parameters);
            return Command<RetrieveEntityMetadataCommand>().GetAttributeTypeList();
        }

        /// <summary>
        /// 获取字段类型（同步版本）
        /// </summary>
        /// <param name="parameters">JSON参数：null 或 "{}"</param>
        /// <returns></returns>
        public virtual object GetAttributeTypeList(string parameters)
        {
            return GetAttributeTypeListCore(parameters);
        }

        /// <summary>
        /// 获取字段类型（异步版本）
        /// </summary>
        /// <param name="parameters">JSON参数：null 或 "{}"</param>
        /// <param name="callback">JavaScript 回调函数</param>
        public virtual void GetAttributeTypeList(string parameters, IJavascriptCallback callback)
        {
            Task.Run(() =>
            {
                try
                {
                    var result = GetAttributeTypeListCore(parameters);
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
        /// 获取实体字段元数据（核心业务逻辑）
        /// </summary>
        /// <param name="parameters">JSON参数：{"envir": "string", "entityName": "string", "attributeType": int?}</param>
        /// <returns></returns>
        private ResultModel GetAllAttributeMetadataFromEntityCore(string parameters)
        {
            var input = DeserializeParameters<GetAllAttributeMetadataFromEntityInput>(parameters);
            SetParameters(parameters); // 设置参数以提取 crmEnv
            return Command<RetrieveEntityMetadataCommand>().GetAllAttributeMetadataFromEntity(input?.envir, input?.entityName, input?.attributeType);
        }

        /// <summary>
        /// 获取实体字段元数据（同步版本）
        /// </summary>
        /// <param name="parameters">JSON参数：{"envir": "string", "entityName": "string", "attributeType": int?}</param>
        /// <returns></returns>
        public virtual object GetAllAttributeMetadataFromEntity(string parameters)
        {
            return GetAllAttributeMetadataFromEntityCore(parameters);
        }

        /// <summary>
        /// 获取实体字段元数据（异步版本）
        /// </summary>
        /// <param name="parameters">JSON参数：{"envir": "string", "entityName": "string", "attributeType": int?}</param>
        /// <param name="callback">JavaScript 回调函数</param>
        public virtual void GetAllAttributeMetadataFromEntity(string parameters, IJavascriptCallback callback)
        {
            Task.Run(() =>
            {
                try
                {
                    var result = GetAllAttributeMetadataFromEntityCore(parameters);
                    var resultJson = JsonConvert.SerializeObject(result);
                    callback.ExecuteAsync(true, resultJson);
                }
                catch (Exception ex)
                {
                    callback.ExecuteAsync(false, ex.Message);
                }
            });
        }

        private class GetAllEntityMetadataInput
        {
            public string envir { get; set; }
            public bool isCustomEntity { get; set; }
        }

        private class GetAllAttributeMetadataFromEntityInput
        {
            public string envir { get; set; }
            public string entityName { get; set; }
            public int? attributeType { get; set; }
        }
    }
}

