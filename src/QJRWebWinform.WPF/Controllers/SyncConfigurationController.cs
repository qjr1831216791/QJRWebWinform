using APIService.SyncConfiguration;
using CommonHelper.Model;
using CefSharp;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace QJRWebWinform.WPF.Controllers
{
    /// <summary>
    /// 系统配置同步API
    /// </summary>
    public class SyncConfigurationController : DynamicControllerBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="mainWindow"></param>
        public SyncConfigurationController(Window mainWindow) : base(mainWindow)
        {
        }

        public override string Name => "SyncConfiguration";

        /// <summary>
        /// 获取环境参数（核心业务逻辑）
        /// </summary>
        /// <param name="parameters">JSON参数：null 或 "{}"</param>
        /// <returns></returns>
        private ResultModel GetEnvironmentsCore(string parameters)
        {
            SetParameters(parameters);
            return Command<SyncConfigurationCommand>().GetEnvironments();
        }

        /// <summary>
        /// 获取环境参数（同步版本）
        /// </summary>
        /// <param name="parameters">JSON参数：null 或 "{}"</param>
        /// <returns></returns>
        public virtual object GetEnvironments(string parameters)
        {
            return GetEnvironmentsCore(parameters);
        }

        /// <summary>
        /// 获取环境参数（异步版本）
        /// </summary>
        /// <param name="parameters">JSON参数：null 或 "{}"</param>
        /// <param name="callback">JavaScript 回调函数</param>
        public virtual void GetEnvironments(string parameters, IJavascriptCallback callback)
        {
            Task.Run(() =>
            {
                try
                {
                    var result = GetEnvironmentsCore(parameters);
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
        /// 获取系统配置（核心业务逻辑）
        /// </summary>
        /// <param name="parameters">JSON参数：{"envirFrom": "string", "envirTo": "string", "entityName": "string", "pageIndexEnvirFrom": int?, "pageSizeEnvirFrom": int?, "pageIndexEnvirTo": int?, "pageSizeEnvirTo": int?, "createdonRange": "string"}</param>
        /// <returns></returns>
        private ResultModel GetSystemConfigsByDateRangeCore(string parameters)
        {
            var input = DeserializeParameters<GetSystemConfigsByDateRangeInput>(parameters);
            SetParameters(parameters);
            
            if (input == null)
            {
                throw new Exception("参数不能为空");
            }

            switch (input.entityName)
            {
                case "new_languageconfig":
                    return Command<SyncConfigurationCommand>().GetLanguageConfigs(input.envirFrom, input.envirTo, input.pageIndexEnvirFrom, input.pageSizeEnvirFrom, input.pageIndexEnvirTo, input.pageSizeEnvirTo, input.createdonRange);
                case "new_data_languageconfig":
                    return Command<SyncConfigurationCommand>().GetDataLanguageConfigs(input.envirFrom, input.envirTo, input.pageIndexEnvirFrom, input.pageSizeEnvirFrom, input.pageIndexEnvirTo, input.pageSizeEnvirTo, input.createdonRange);
                default:
                    throw new Exception("无法识别的系统配置项！");
            }
        }

        /// <summary>
        /// 获取系统配置（同步版本）
        /// </summary>
        /// <param name="parameters">JSON参数：{"envirFrom": "string", "envirTo": "string", "entityName": "string", "pageIndexEnvirFrom": int?, "pageSizeEnvirFrom": int?, "pageIndexEnvirTo": int?, "pageSizeEnvirTo": int?, "createdonRange": "string"}</param>
        /// <returns></returns>
        public virtual object GetSystemConfigsByDateRange(string parameters)
        {
            return GetSystemConfigsByDateRangeCore(parameters);
        }

        /// <summary>
        /// 获取系统配置（异步版本）
        /// </summary>
        /// <param name="parameters">JSON参数：{"envirFrom": "string", "envirTo": "string", "entityName": "string", "pageIndexEnvirFrom": int?, "pageSizeEnvirFrom": int?, "pageIndexEnvirTo": int?, "pageSizeEnvirTo": int?, "createdonRange": "string"}</param>
        /// <param name="callback">JavaScript 回调函数</param>
        public virtual void GetSystemConfigsByDateRange(string parameters, IJavascriptCallback callback)
        {
            Task.Run(() =>
            {
                try
                {
                    var result = GetSystemConfigsByDateRangeCore(parameters);
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
        /// 获取系统配置（核心业务逻辑）
        /// </summary>
        /// <param name="parameters">JSON参数：{"envirFrom": "string", "envirTo": "string", "entityName": "string"}</param>
        /// <returns></returns>
        private ResultModel GetSystemConfigsCore(string parameters)
        {
            var input = DeserializeParameters<GetSystemConfigsInput>(parameters);
            SetParameters(parameters);
            
            if (input == null)
            {
                throw new Exception("参数不能为空");
            }

            switch (input.entityName)
            {
                case "new_systemparameter":
                    return Command<SyncConfigurationCommand>().GetSystemParameters(input.envirFrom, input.envirTo);
                case "new_autonumber":
                    return Command<SyncConfigurationCommand>().GetAutoNumbers(input.envirFrom, input.envirTo);
                case "new_sumrelationshipdetail":
                    return Command<SyncConfigurationCommand>().GetSumRelationshipDetails(input.envirFrom, input.envirTo);
                case "new_ribbon":
                    return Command<SyncConfigurationCommand>().GetRibbonRules(input.envirFrom, input.envirTo);
                case "new_duplicatedetect":
                    return Command<SyncConfigurationCommand>().GetDuplicateDetects(input.envirFrom, input.envirTo);
                case "commondeletecheck":
                    return Command<SyncConfigurationCommand>().GetCommonDeleteCheckPluginSteps(input.envirFrom, input.envirTo);
                case "new_import":
                    return Command<SyncConfigurationCommand>().GetImportConfigs(input.envirFrom, input.envirTo);
                case "documenttemplate":
                    return Command<SyncConfigurationCommand>().GetDocumenttemplates(input.envirFrom, input.envirTo);
                case "new_multiple_language_contrast":
                    return Command<SyncConfigurationCommand>().GetMultipleLanguageContrasts(input.envirFrom, input.envirTo);
                default:
                    throw new Exception("无法识别的系统配置项！");
            }
        }

        /// <summary>
        /// 获取系统配置（同步版本）
        /// </summary>
        /// <param name="parameters">JSON参数：{"envirFrom": "string", "envirTo": "string", "entityName": "string"}</param>
        /// <returns></returns>
        public virtual object GetSystemConfigs(string parameters)
        {
            return GetSystemConfigsCore(parameters);
        }

        /// <summary>
        /// 获取系统配置（异步版本）
        /// </summary>
        /// <param name="parameters">JSON参数：{"envirFrom": "string", "envirTo": "string", "entityName": "string"}</param>
        /// <param name="callback">JavaScript 回调函数</param>
        public virtual void GetSystemConfigs(string parameters, IJavascriptCallback callback)
        {
            Task.Run(() =>
            {
                try
                {
                    var result = GetSystemConfigsCore(parameters);
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
        /// 同步系统配置（核心业务逻辑）
        /// </summary>
        /// <param name="parameters">JSON参数：SyncSystemConfigsInput 对象</param>
        /// <returns></returns>
        private ResultModel SyncSystemConfigsCore(string parameters)
        {
            var model = DeserializeParameters<SyncSystemConfigsInput>(parameters);
            SetParameters(parameters);
            
            if (model is null)
            {
                throw new Exception("参数Model不能为空!");
            }

            switch (model.entityName)
            {
                case "new_systemparameter":
                    return Command<SyncConfigurationCommand>().SyncSystemParameters(model.envirFrom, model.envirTo, model.configList);
                case "new_autonumber":
                    return Command<SyncConfigurationCommand>().SyncAutoNumbers(model.envirFrom, model.envirTo, model.configList);
                case "new_sumrelationshipdetail":
                    return Command<SyncConfigurationCommand>().SyncSumRelationshipDetails(model.envirFrom, model.envirTo, model.configList);
                case "new_ribbon":
                    return Command<SyncConfigurationCommand>().SyncRibbonRules(model.envirFrom, model.envirTo, model.configList);
                case "new_duplicatedetect":
                    return Command<SyncConfigurationCommand>().SyncDuplicateDetects(model.envirFrom, model.envirTo, model.configList);
                case "commondeletecheck":
                    return Command<SyncConfigurationCommand>().SyncCommonDeleteCheckPluginSteps(model.envirFrom, model.envirTo, model.configList);
                case "new_import":
                    return Command<SyncConfigurationCommand>().SyncImportConfigs(model.envirFrom, model.envirTo, model.configList);
                case "new_languageconfig":
                    return Command<SyncConfigurationCommand>().SyncLanguageConfigs(model.envirFrom, model.envirTo, model.configList);
                case "documenttemplate":
                    return Command<SyncConfigurationCommand>().SyncDocumenttemplates(model.envirFrom, model.envirTo, model.configList);
                case "new_multiple_language_contrast":
                    return Command<SyncConfigurationCommand>().SyncMultipleLanguageContrasts(model.envirFrom, model.envirTo, model.configList);
                case "new_data_languageconfig":
                    return Command<SyncConfigurationCommand>().SyncDataLanguageConfigs(model.envirFrom, model.envirTo, model.configList);
                default:
                    throw new Exception("无法识别的系统配置项！");
            }
        }

        /// <summary>
        /// 同步系统配置（同步版本）
        /// </summary>
        /// <param name="parameters">JSON参数：SyncSystemConfigsInput 对象</param>
        /// <returns></returns>
        public virtual object SyncSystemConfigs(string parameters)
        {
            return SyncSystemConfigsCore(parameters);
        }

        /// <summary>
        /// 同步系统配置（异步版本）
        /// </summary>
        /// <param name="parameters">JSON参数：SyncSystemConfigsInput 对象</param>
        /// <param name="callback">JavaScript 回调函数</param>
        public virtual void SyncSystemConfigs(string parameters, IJavascriptCallback callback)
        {
            Task.Run(() =>
            {
                try
                {
                    var result = SyncSystemConfigsCore(parameters);
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
        /// 下载系统模板（核心业务逻辑）
        /// </summary>
        /// <param name="parameters">JSON参数：{"envir": "string", "templateId": "string"}</param>
        /// <returns></returns>
        private ResultModel DownloadTemplateCore(string parameters)
        {
            var input = DeserializeParameters<DownloadTemplateInput>(parameters);
            SetParameters(parameters);
            return Command<SyncConfigurationCommand>().DownloadTemplate(input?.envir, input?.templateId);
        }

        /// <summary>
        /// 下载系统模板（同步版本）
        /// </summary>
        /// <param name="parameters">JSON参数：{"envir": "string", "templateId": "string"}</param>
        /// <returns></returns>
        public virtual object DownloadTemplate(string parameters)
        {
            return DownloadTemplateCore(parameters);
        }

        /// <summary>
        /// 下载系统模板（异步版本）
        /// </summary>
        /// <param name="parameters">JSON参数：{"envir": "string", "templateId": "string"}</param>
        /// <param name="callback">JavaScript 回调函数</param>
        public virtual void DownloadTemplate(string parameters, IJavascriptCallback callback)
        {
            Task.Run(() =>
            {
                try
                {
                    var result = DownloadTemplateCore(parameters);
                    var resultJson = JsonConvert.SerializeObject(result);
                    callback.ExecuteAsync(true, resultJson);
                }
                catch (Exception ex)
                {
                    callback.ExecuteAsync(false, ex.Message);
                }
            });
        }

        private class GetSystemConfigsByDateRangeInput
        {
            public string envirFrom { get; set; }
            public string envirTo { get; set; }
            public string entityName { get; set; }
            public int? pageIndexEnvirFrom { get; set; }
            public int? pageSizeEnvirFrom { get; set; }
            public int? pageIndexEnvirTo { get; set; }
            public int? pageSizeEnvirTo { get; set; }
            public string createdonRange { get; set; }
        }

        private class GetSystemConfigsInput
        {
            public string envirFrom { get; set; }
            public string envirTo { get; set; }
            public string entityName { get; set; }
        }

        private class DownloadTemplateInput
        {
            public string envir { get; set; }
            public string templateId { get; set; }
        }
    }
}

