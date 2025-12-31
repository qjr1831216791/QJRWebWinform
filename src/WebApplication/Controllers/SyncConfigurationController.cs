using APIService.SyncConfiguration;
using CommonHelper.Model;
using System;
using System.Web.Http;

namespace WebApplication.Controllers
{
    /// <summary>
    /// 系统配置同步API
    /// </summary>
    public class SyncConfigurationController : BaseController
    {
        /// <summary>
        /// 获取环境参数
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public virtual ResultModel GetEnvironments()
        {
            return Command<SyncConfigurationCommand>().GetEnvironments();
        }

        /// <summary>
        /// 获取系统配置
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <param name="entityName"></param>
        /// <param name="pageIndexEnvirFrom"></param>
        /// <param name="pageSizeEnvirFrom"></param>
        /// <param name="pageIndexEnvirTo"></param>
        /// <param name="pageSizeEnvirTo"></param>
        /// <param name="createdonRange"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual ResultModel GetSystemConfigsByDateRange(string envirFrom, string envirTo, string entityName, int? pageIndexEnvirFrom, int? pageSizeEnvirFrom, int? pageIndexEnvirTo, int? pageSizeEnvirTo, string createdonRange)
        {
            switch (entityName)
            {
                case "new_languageconfig":
                    return Command<SyncConfigurationCommand>().GetLanguageConfigs(envirFrom, envirTo, pageIndexEnvirFrom, pageSizeEnvirFrom, pageIndexEnvirTo, pageSizeEnvirTo, createdonRange);
                case "new_data_languageconfig":
                    return Command<SyncConfigurationCommand>().GetDataLanguageConfigs(envirFrom, envirTo, pageIndexEnvirFrom, pageSizeEnvirFrom, pageIndexEnvirTo, pageSizeEnvirTo, createdonRange);
                default:
                    throw new Exception("无法识别的系统配置项！");
            }
        }

        /// <summary>
        /// 获取系统配置
        /// </summary>
        /// <param name="envirFrom"></param>
        /// <param name="envirTo"></param>
        /// <param name="entityName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost]
        public virtual ResultModel GetSystemConfigs(string envirFrom, string envirTo, string entityName)
        {
            switch (entityName)
            {
                case "new_systemparameter":
                    return Command<SyncConfigurationCommand>().GetSystemParameters(envirFrom, envirTo);
                case "new_autonumber":
                    return Command<SyncConfigurationCommand>().GetAutoNumbers(envirFrom, envirTo);
                case "new_sumrelationshipdetail":
                    return Command<SyncConfigurationCommand>().GetSumRelationshipDetails(envirFrom, envirTo);
                case "new_ribbon":
                    return Command<SyncConfigurationCommand>().GetRibbonRules(envirFrom, envirTo);
                case "new_duplicatedetect":
                    return Command<SyncConfigurationCommand>().GetDuplicateDetects(envirFrom, envirTo);
                case "commondeletecheck":
                    return Command<SyncConfigurationCommand>().GetCommonDeleteCheckPluginSteps(envirFrom, envirTo);
                case "new_import":
                    return Command<SyncConfigurationCommand>().GetImportConfigs(envirFrom, envirTo);
                case "documenttemplate":
                    return Command<SyncConfigurationCommand>().GetDocumenttemplates(envirFrom, envirTo);
                case "new_multiple_language_contrast":
                    return Command<SyncConfigurationCommand>().GetMultipleLanguageContrasts(envirFrom, envirTo);
                default:
                    throw new Exception("无法识别的系统配置项！");
            }
        }

        /// <summary>
        /// 同步系统配置
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost]
        public virtual ResultModel SyncSystemConfigs([FromBody] SyncSystemConfigsInput model)
        {
            if (model is null) throw new Exception("参数Model不能为空!");
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
        /// 下载系统模板
        /// </summary>
        /// <param name="envir"></param>
        /// <param name="templateId"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual ResultModel DownloadTemplate(string envir, string templateId)
        {
            return Command<SyncConfigurationCommand>().DownloadTemplate(envir, templateId);
        }
    }
}