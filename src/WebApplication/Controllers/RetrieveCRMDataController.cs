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
        /// 查询CRM Data
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual ResultModel RetrieveCRMData([FromBody] GetTreeDataInput input)
        {
            return Command<RetrieveCRMDataCommand>().RetrieveCRMData(input.envir, input.entityName, input.customFields);
        }

        /// <summary>
        /// 查询CRM Data
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual ResultModel RetrieveCRMDataByFetchXml([FromBody] GetCRMDataByFetchXml input)
        {
            return Command<RetrieveCRMDataCommand>().RetrieveCRMDataByFetchXml(input.envir, input.fetchXml, input.pageIndex, input.pageSize);
        }
    }
}
