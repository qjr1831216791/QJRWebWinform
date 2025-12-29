using System;
using System.Windows;
using CefSharp;

namespace QJRWebWinform.WPF.Controllers
{
    /// <summary>
    /// 窗口相关 Controller
    /// </summary>
    public class WindowController : ControllerBase
    {
        public override string Name => "Window";

        public WindowController(Window mainWindow) : base(mainWindow)
        {
        }

        /// <summary>
        /// 设置窗口标题
        /// </summary>
        public object SetTitle(string parameters)
        {
            var param = DeserializeParameters<SetTitleParams>(parameters);
            if (param == null || string.IsNullOrWhiteSpace(param.Title))
            {
                throw new ArgumentException("标题不能为空");
            }

            InvokeOnUIThread(() =>
            {
                MainWindow.Title = param.Title;
            });

            return new { success = true, title = param.Title };
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        public object Close(string parameters)
        {
            InvokeOnUIThread(() =>
            {
                MainWindow.Close();
            });

            return new { success = true, message = "窗口已关闭" };
        }

        private class SetTitleParams
        {
            public string Title { get; set; }
        }
    }
}

