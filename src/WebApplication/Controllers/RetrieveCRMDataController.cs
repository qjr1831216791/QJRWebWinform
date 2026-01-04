using APIService.RetrieveCRMData;
using CommonHelper.Model;
using System.Web.Http;

namespace WebApplication.Controllers
{
    /// <summary>
    /// 查询CRM Data
    /// </summary>
    public class RetrieveCRMDataController : BaseController
    {
        /// <summary>
        /// 查询CRM数据，支持自定义字段查询
        /// </summary>
        /// <param name="input">查询输入模型，包含环境标识、实体名称和自定义字段列表</param>
        /// <returns>包含CRM数据的结果模型</returns>
        [HttpPost]
        public virtual ResultModel RetrieveCRMData([FromBody] GetTreeDataInput input)
        {
            return Command<RetrieveCRMDataCommand>().RetrieveCRMData(input.envir, input.entityName, input.customFields);
        }

        /// <summary>
        /// 使用FetchXml查询CRM数据，支持分页
        /// </summary>
        /// <param name="input">查询输入模型，包含环境标识、FetchXml查询语句和分页参数</param>
        /// <returns>包含CRM数据的结果模型</returns>
        [HttpPost]
        public virtual ResultModel RetrieveCRMDataByFetchXml([FromBody] GetCRMDataByFetchXml input)
        {
            return Command<RetrieveCRMDataCommand>().RetrieveCRMDataByFetchXml(input.envir, input.fetchXml, input.pageIndex, input.pageSize);
        }
    }
}
