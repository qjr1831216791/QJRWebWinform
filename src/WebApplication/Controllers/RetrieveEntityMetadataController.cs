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
        /// <param name="envir">环境标识</param>
        /// <param name="isCustomEntity">是否只获取用户定制的实体，true表示只获取自定义实体，false表示获取所有实体</param>
        /// <returns>包含实体元数据列表的结果模型</returns>
        [HttpPost]
        public virtual ResultModel GetAllEntityMetadata(string envir, bool isCustomEntity)
        {
            return Command<RetrieveEntityMetadataCommand>().GetAllEntityMetadata(envir, isCustomEntity);
        }

        /// <summary>
        /// 获取所有字段类型列表
        /// </summary>
        /// <returns>包含字段类型列表的结果模型</returns>
        [HttpPost]
        public virtual ResultModel GetAttributeTypeList()
        {
            return Command<RetrieveEntityMetadataCommand>().GetAttributeTypeList();
        }

        /// <summary>
        /// 获取指定实体的所有字段元数据
        /// </summary>
        /// <param name="envir">环境标识</param>
        /// <param name="entityName">实体逻辑名</param>
        /// <param name="attributeType">字段类型，如果指定则只返回该类型的字段，如果为null则返回所有类型的字段</param>
        /// <returns>包含字段元数据列表的结果模型</returns>
        [HttpPost]
        public virtual ResultModel GetAllAttributeMetadataFromEntity(string envir, string entityName, int? attributeType = null)
        {
            return Command<RetrieveEntityMetadataCommand>().GetAllAttributeMetadataFromEntity(envir, entityName, attributeType);
        }
    }
}
