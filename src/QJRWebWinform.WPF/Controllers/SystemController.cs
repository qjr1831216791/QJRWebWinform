using System;
using System.Windows;

namespace QJRWebWinform.WPF.Controllers
{
    /// <summary>
    /// 系统相关 Controller
    /// </summary>
    public class SystemController : ControllerBase
    {
        public override string Name => "System";

        public SystemController(Window mainWindow) : base(mainWindow)
        {
        }

        /// <summary>
        /// 获取系统信息
        /// </summary>
        public object GetSystemInfo(string parameters)
        {
            return new
            {
                osVersion = Environment.OSVersion.ToString(),
                dotNetVersion = Environment.Version.ToString(),
                machineName = Environment.MachineName,
                userName = Environment.UserName,
                processorCount = Environment.ProcessorCount,
                workingSet = Environment.WorkingSet
            };
        }

        /// <summary>
        /// 显示消息框
        /// </summary>
        public object ShowMessage(string parameters)
        {
            var param = DeserializeParameters<ShowMessageParams>(parameters);
            if (param == null || string.IsNullOrWhiteSpace(param.Message))
            {
                throw new ArgumentException("消息内容不能为空");
            }

            InvokeOnUIThread(() =>
            {
                System.Windows.MessageBox.Show(param.Message, param.Title ?? "来自后端", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            });

            return new { success = true, message = "消息已显示" };
        }

        private class ShowMessageParams
        {
            public string Message { get; set; }
            public string Title { get; set; }
        }
    }
}

