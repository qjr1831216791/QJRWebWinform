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
        /// 获取环境参数列表
        /// </summary>
        /// <returns>包含所有可用环境信息的结果模型</returns>
        [HttpPost]
        public virtual ResultModel GetEnvironments()
        {
            return Command<SyncConfigurationCommand>().GetEnvironments();
        }

        /// <summary>
        /// 根据日期范围获取系统配置，支持分页查询
        /// </summary>
        /// <param name="envirFrom">源环境标识</param>
        /// <param name="envirTo">目标环境标识</param>
        /// <param name="entityName">实体名称，支持new_languageconfig和new_data_languageconfig</param>
        /// <param name="pageIndexEnvirFrom">源环境分页索引</param>
        /// <param name="pageSizeEnvirFrom">源环境每页大小</param>
        /// <param name="pageIndexEnvirTo">目标环境分页索引</param>
        /// <param name="pageSizeEnvirTo">目标环境每页大小</param>
        /// <param name="createdonRange">创建日期范围，格式：开始日期,结束日期</param>
        /// <returns>包含系统配置列表的结果模型</returns>
        /// <exception cref="Exception">当实体名称无法识别时抛出异常</exception>
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
        /// 获取系统配置列表
        /// </summary>
        /// <param name="envirFrom">源环境标识</param>
        /// <param name="envirTo">目标环境标识</param>
        /// <param name="entityName">实体名称，支持：new_systemparameter、new_autonumber、new_sumrelationshipdetail、new_ribbon、new_duplicatedetect、commondeletecheck、new_import、documenttemplate、new_multiple_language_contrast</param>
        /// <returns>包含系统配置列表的结果模型</returns>
        /// <exception cref="Exception">当实体名称无法识别时抛出异常</exception>
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
        /// 同步系统配置，将源环境的配置同步到目标环境
        /// </summary>
        /// <param name="model">同步配置输入模型，包含环境标识、实体名称和配置列表</param>
        /// <returns>同步操作的结果模型</returns>
        /// <exception cref="Exception">当model为null或实体名称无法识别时抛出异常</exception>
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
        /// 下载系统模板文件
        /// </summary>
        /// <param name="envir">环境标识</param>
        /// <param name="templateId">模板ID</param>
        /// <returns>包含模板文件数据的结果模型</returns>
        [HttpPost]
        public virtual ResultModel DownloadTemplate(string envir, string templateId)
        {
            return Command<SyncConfigurationCommand>().DownloadTemplate(envir, templateId);
        }
    }
}