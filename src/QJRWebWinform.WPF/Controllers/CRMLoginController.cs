using APIService;
using CefSharp;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace QJRWebWinform.WPF.Controllers
{
    public class CRMLoginController : DynamicControllerBase
    {
        public CRMLoginController(Window mainWindow) : base(mainWindow)
        {
        }

        public override string Name => "CRMLogin";

        private object LoginCore(string parameters)
        {
            // 通过 DynamicControllerBase.SetParameters 从 JSON 参数提取 crmEnv，
            // 与 WebApplication 读取 Header 的行为保持等价语义。
            SetParameters(parameters);
            BaseCommand cmd = new BaseCommand();
            cmd.Initialize(crmEnv);
            return true;
        }

        public virtual object Login(string parameters)
        {
            return LoginCore(parameters);
        }

        public virtual void Login(string parameters, IJavascriptCallback callback)
        {
            Task.Run(() =>
            {
                try
                {
                    var result = LoginCore(parameters);
                    callback.ExecuteAsync(true, JsonConvert.SerializeObject(result));
                }
                catch (Exception ex)
                {
                    callback.ExecuteAsync(false, ex.Message);
                }
            });
        }
    }
}
