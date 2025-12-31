using APIService.RetrieveEntityMetadata;
using CommonHelper.Model;
using System.Web.Http;

namespace WebApplication.Controllers
{
    /// <summary>
    /// 查询CRM实体元数据
    /// </summary>
    public class RetrieveEntityMetadataController : BaseController
    {
        /// <summary>
        /// 获取所有实体元数据
        /// </summary>
        /// <param name="envir"></param>
        /// <param name="isCustomEntity">是否只获取用户定制的实体</param>
        /// <returns></returns>
        [HttpPost]
        public virtual ResultModel GetAllEntityMetadata(string envir, bool isCustomEntity)
        {
            return Command<RetrieveEntityMetadataCommand>().GetAllEntityMetadata(envir, isCustomEntity);
        }

        /// <summary>
        /// 获取字段类型
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public virtual ResultModel GetAttributeTypeList()
        {
            return Command<RetrieveEntityMetadataCommand>().GetAttributeTypeList();
        }

        /// <summary>
        /// 获取实体字段元数据
        /// </summary>
        /// <param name="envir"></param>
        /// <param name="entityName"></param>
        /// <param name="attributeName"></param>
        /// <param name="attributeType"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual ResultModel GetAllAttributeMetadataFromEntity(string envir, string entityName, int? attributeType = null)
        {
            return Command<RetrieveEntityMetadataCommand>().GetAllAttributeMetadataFromEntity(envir, entityName, attributeType);
        }
    }
}
