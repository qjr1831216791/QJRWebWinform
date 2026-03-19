using System.Windows;

namespace QJRWebWinform.WPF.Controllers
{
    /// <summary>
    /// 兼容控制器（Deprecated Wrapper）。
    /// 说明：
    /// 1. API 已迁移到 APITest/BaseData/CRMLogin 控制器。
    /// 2. 此控制器仅保留旧路由兼容，不再持有业务实现，避免重复逻辑。
    /// 3. 新功能请只在新控制器维护。
    /// </summary>
    public class DefaultController : DynamicControllerBase
    {
        private readonly APITestController _apiTestController;
        private readonly BaseDataController _baseDataController;

        public DefaultController(Window mainWindow) : base(mainWindow)
        {
            _apiTestController = new APITestController(mainWindow);
            _baseDataController = new BaseDataController(mainWindow);
        }

        public override string Name => "Default";

    }
}
