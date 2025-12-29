using System;
using System.IO;
using System.Reflection;
using System.Windows;
using CefSharp;

namespace QJRWebWinform.WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private CefSharpHost _cefHost;
        private NativeHost _nativeHost;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void WebBrowser_Loaded(object sender, RoutedEventArgs e)
        {
            // 创建 NativeHost 实例
            _nativeHost = new NativeHost(this);

            // 创建 CefSharpHost 并初始化
            _cefHost = new CefSharpHost(WebBrowser, _nativeHost);

            // 等待浏览器初始化完成后再加载页面
            WebBrowser.IsBrowserInitializedChanged += (s, args) =>
            {
                if (WebBrowser.IsBrowserInitialized)
                {
                    // 浏览器初始化后，等待 NativeHost 注册完成，再加载页面
                    // 这样确保同步绑定在页面加载前完成
                    System.Threading.Tasks.Task.Delay(50).ContinueWith(t =>
                    {
                        // 等待注册完成（最多等待 5 秒）
                        bool registered = _cefHost.WaitForRegistration(5000);
                        
                        if (!registered)
                        {
                            System.Diagnostics.Debug.WriteLine("警告: NativeHost 注册超时，但仍尝试加载页面");
                        }
                        
                        // 在 UI 线程上加载页面
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            LoadFrontend();
                            
                            #if DEBUG
                            // Debug 模式下，页面加载后自动打开开发者工具
                            System.Threading.Tasks.Task.Delay(500).ContinueWith(delayTask =>
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    ShowDevTools();
                                });
                            });
                            #endif
                        });
                    });
                }
            };

            // 如果浏览器已经初始化，立即处理
            if (WebBrowser.IsBrowserInitialized)
            {
                // 延迟一小段时间，确保 CefSharpHost 完成初始化
                System.Threading.Tasks.Task.Delay(100).ContinueWith(t =>
                {
                    // 等待注册完成
                    bool registered = _cefHost.WaitForRegistration(5000);
                    
                    if (!registered)
                    {
                        System.Diagnostics.Debug.WriteLine("警告: NativeHost 注册超时，但仍尝试加载页面");
                    }
                    
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LoadFrontend();
                        
                        #if DEBUG
                        // Debug 模式下，页面加载后自动打开开发者工具
                        System.Threading.Tasks.Task.Delay(500).ContinueWith(delayTask =>
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                ShowDevTools();
                            });
                        });
                        #endif
                    });
                });
            }
        }

        private void LoadFrontend()
        {
            // 获取前端文件路径
            string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string htmlPath = Path.Combine(appPath, "wwwroot", "index.html");

            if (File.Exists(htmlPath))
            {
                // 使用 file:// 协议加载本地文件
                // 将路径转换为正确的 file:// URI 格式
                string fileUri = new Uri(htmlPath).AbsoluteUri;
                WebBrowser.Address = fileUri;
            }
            else
            {
                // 如果本地文件不存在，可以加载开发服务器地址
                #if DEBUG
                WebBrowser.Address = "http://localhost:8080";
                #else
                MessageBox.Show($"前端文件未找到: {htmlPath}\n请先构建前端项目。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                #endif
            }
        }

        /// <summary>
        /// 从后端调用前端方法（示例）
        /// </summary>
        public void CallFrontendMethod(string methodName, object data)
        {
            if (WebBrowser.IsBrowserInitialized)
            {
                string jsonData = data != null ? Newtonsoft.Json.JsonConvert.SerializeObject(data) : "null";
                string script = $"if (typeof window.{methodName} === 'function') {{ window.{methodName}({jsonData}); }}";
                WebBrowser.ExecuteScriptAsync(script);
            }
        }

        /// <summary>
        /// 处理窗口按键事件（支持 F12 打开开发者工具）
        /// </summary>
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // F12 键打开/关闭开发者工具
            if (e.Key == System.Windows.Input.Key.F12)
            {
                ShowDevTools();
                e.Handled = true;
            }
        }

        /// <summary>
        /// 显示/隐藏开发者工具
        /// </summary>
        private void ShowDevTools()
        {
            if (WebBrowser.IsBrowserInitialized)
            {
                try
                {
                    WebBrowser.ShowDevTools();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"打开开发者工具失败: {ex.Message}");
                }
            }
        }
    }
}

