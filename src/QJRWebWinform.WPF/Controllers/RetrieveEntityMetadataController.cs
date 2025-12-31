using APIService.RetrieveEntityMetadata;
using CommonHelper.Model;
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
        /// 获取所有实体元数据
        /// </summary>
        /// <param name="parameters">JSON参数：{"envir": "string", "isCustomEntity": bool}</param>
        /// <returns></returns>
        public virtual object GetAllEntityMetadata(string parameters)
        {
            var input = DeserializeParameters<GetAllEntityMetadataInput>(parameters);
            SetParameters(parameters); // 设置参数以提取 crmEnv
            return Command<RetrieveEntityMetadataCommand>().GetAllEntityMetadata(input?.envir, input?.isCustomEntity ?? false);
        }

        /// <summary>
        /// 获取字段类型
        /// </summary>
        /// <param name="parameters">JSON参数：null 或 "{}"</param>
        /// <returns></returns>
        public virtual object GetAttributeTypeList(string parameters)
        {
            SetParameters(parameters);
            return Command<RetrieveEntityMetadataCommand>().GetAttributeTypeList();
        }

        /// <summary>
        /// 获取实体字段元数据
        /// </summary>
        /// <param name="parameters">JSON参数：{"envir": "string", "entityName": "string", "attributeType": int?}</param>
        /// <returns></returns>
        public virtual object GetAllAttributeMetadataFromEntity(string parameters)
        {
            var input = DeserializeParameters<GetAllAttributeMetadataFromEntityInput>(parameters);
            SetParameters(parameters); // 设置参数以提取 crmEnv
            return Command<RetrieveEntityMetadataCommand>().GetAllAttributeMetadataFromEntity(input?.envir, input?.entityName, input?.attributeType);
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

