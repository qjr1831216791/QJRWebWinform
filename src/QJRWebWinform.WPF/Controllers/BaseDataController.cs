using APIService.BaseData;
using CommonHelper.Model;
using CefSharp;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace QJRWebWinform.WPF.Controllers
{
    public class BaseDataController : DynamicControllerBase
    {
        public BaseDataController(Window mainWindow) : base(mainWindow)
        {
        }

        public override string Name => "BaseData";

        private ResultModel GetCRMEnvironmentsCore(string parameters)
        {
            SetParameters(parameters);
            return new BaseDataCommand().GetCRMEnvironments();
        }

        public virtual object GetCRMEnvironments(string parameters)
        {
            return GetCRMEnvironmentsCore(parameters);
        }

        public virtual void GetCRMEnvironments(string parameters, IJavascriptCallback callback)
        {
            Task.Run(() =>
            {
                try
                {
                    var result = GetCRMEnvironmentsCore(parameters);
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
