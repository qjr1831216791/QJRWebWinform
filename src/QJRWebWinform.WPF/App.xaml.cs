using System.IO;
using System.Reflection;
using System.Windows;
using CefSharp;
using CefSharp.Wpf;

namespace QJRWebWinform.WPF
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 启用同步 JavaScript 绑定（必须在创建 ChromiumWebBrowser 之前设置）
            CefSharpSettings.WcfEnabled = true;

            // 初始化 CefSharp
            CefSettings settings = new CefSettings();
            
            // 设置缓存路径（必须设置，避免进程单例行为问题）
            string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string cachePath = Path.Combine(appPath, "CefCache");
            if (!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
            }
            settings.RootCachePath = cachePath;
            
            // 设置用户数据目录（可选）
            // settings.UserDataPath = "CefSharpData";
            
            // 启用日志（调试时有用）
            #if DEBUG
            settings.LogSeverity = LogSeverity.Info;
            // 启用远程调试（可以通过 Chrome DevTools 连接）
            settings.RemoteDebuggingPort = 9222;
            #endif

            // 初始化 CefSharp
            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // 关闭时清理 CefSharp
            Cef.Shutdown();
            base.OnExit(e);
        }
    }
}

