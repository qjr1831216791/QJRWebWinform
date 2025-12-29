using System;
using System.Windows;
using CefSharp;
using CefSharp.Wpf;

namespace QJRWebWinform.WPF
{
    /// <summary>
    /// CefSharp 封装类，负责初始化和通信管理
    /// </summary>
    public class CefSharpHost
    {
        private ChromiumWebBrowser _browser;
        private NativeHost _nativeHost;
        private bool _isRegistered = false;
        private System.Threading.ManualResetEventSlim _registrationEvent = new System.Threading.ManualResetEventSlim(false);

        public CefSharpHost(ChromiumWebBrowser browser, NativeHost nativeHost)
        {
            _browser = browser ?? throw new ArgumentNullException(nameof(browser));
            _nativeHost = nativeHost ?? throw new ArgumentNullException(nameof(nativeHost));

            InitializeBrowser();
        }

        /// <summary>
        /// 等待 NativeHost 注册完成
        /// </summary>
        public bool WaitForRegistration(int timeoutMs = 5000)
        {
            return _registrationEvent.Wait(timeoutMs);
        }

        /// <summary>
        /// 检查 NativeHost 是否已注册
        /// </summary>
        public bool IsRegistered
        {
            get { return _isRegistered && _browser.JavascriptObjectRepository.IsBound("nativeHost"); }
        }

        private void InitializeBrowser()
        {
            // 设置浏览器事件处理器
            SetupEventHandlers();

            // 等待浏览器初始化完成
            if (_browser.IsBrowserInitialized)
            {
                RegisterNativeHost();
            }
            else
            {
                _browser.IsBrowserInitializedChanged += (sender, args) =>
                {
                    if (_browser.IsBrowserInitialized)
                    {
                        RegisterNativeHost();
                    }
                };
            }
        }

        /// <summary>
        /// 注册 C# 对象到 JavaScript，使前端可以调用后端方法
        /// </summary>
        private void RegisterNativeHost()
        {
            try
            {
                // 如果已经注册，先取消注册
                if (_browser.JavascriptObjectRepository.IsBound("nativeHost"))
                {
                    _browser.JavascriptObjectRepository.UnRegister("nativeHost");
                    _isRegistered = false;
                    _registrationEvent.Reset();
                }

                // 尝试同步绑定（如果支持）
                bool useSyncBinding = false;
                try
                {
                    if (CefSharpSettings.WcfEnabled)
                    {
                        _browser.JavascriptObjectRepository.Register("nativeHost", _nativeHost, isAsync: false);
                        useSyncBinding = true;
                    }
                }
                catch (Exception syncEx)
                {
                    // 同步绑定失败，将使用异步绑定
                    System.Diagnostics.Debug.WriteLine($"同步绑定失败，使用异步绑定: {syncEx.Message}");
                }

                // 如果同步绑定失败或未启用，使用异步绑定
                if (!useSyncBinding)
                {
                    _browser.JavascriptObjectRepository.Register("nativeHost", _nativeHost, isAsync: true);
                }

                // 验证注册是否成功
                bool isBound = _browser.JavascriptObjectRepository.IsBound("nativeHost");
                
                if (isBound)
                {
                    _isRegistered = true;
                    _registrationEvent.Set(); // 通知注册完成
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("警告: NativeHost 注册验证失败");
                    _isRegistered = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"注册 NativeHost 失败: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"异常堆栈: {ex.StackTrace}");
                _isRegistered = false;
            }
        }

        /// <summary>
        /// 通过 JavaScript 注入确保 NativeHost 在 window 作用域中可用
        /// 这对于异步绑定特别重要
        /// </summary>
        private void InjectNativeHostToWindow()
        {
            try
            {
                // 对于异步绑定，对象可能不在 window 作用域中
                // 我们需要通过 JavaScript 来确保它可用
                string injectScript = @"
                    (function() {
                        try {
                            // 如果同步绑定成功，对象可能在全局作用域中，需要添加到 window
                            if (typeof nativeHost !== 'undefined' && typeof window.nativeHost === 'undefined') {
                                window.nativeHost = nativeHost;
                            }
                            
                            // 如果使用异步绑定，需要通过 CefSharp.BindObjectAsync
                            if (typeof window.nativeHost === 'undefined' && typeof CefSharp !== 'undefined' && CefSharp.BindObjectAsync) {
                                CefSharp.BindObjectAsync('nativeHost', 'nativeHost').then(function(bound) {
                                    if (typeof nativeHost !== 'undefined') {
                                        window.nativeHost = nativeHost;
                                    }
                                }).catch(function(err) {
                                    console.error('异步绑定失败:', err);
                                });
                            }
                        } catch(e) {
                            console.error('注入 nativeHost 时出错:', e);
                        }
                    })();
                ";
                _browser.ExecuteScriptAsync(injectScript);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"注入 NativeHost 失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试 NativeHost 在 JavaScript 中的可用性（仅在需要时调用）
        /// </summary>
        private void TestNativeHostAvailability()
        {
            // 移除详细诊断脚本，减少控制台噪音
            // 仅在 Debug 模式下进行简单检查
            #if DEBUG
            try
            {
                string testScript = @"
                    (function() {
                        if (typeof window.nativeHost === 'undefined' && typeof nativeHost !== 'undefined') {
                            window.nativeHost = nativeHost;
                        }
                    })();
                ";
                _browser.ExecuteScriptAsync(testScript);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"测试 NativeHost 可用性失败: {ex.Message}");
            }
            #endif
        }

        /// <summary>
        /// 设置浏览器事件处理器
        /// </summary>
        private void SetupEventHandlers()
        {
            // 页面加载开始事件 - 确保在页面加载前注册完成
            _browser.FrameLoadStart += (sender, args) =>
            {
                if (args.Frame.IsMain)
                {
                    // 确保在页面加载前已注册
                    if (!_browser.JavascriptObjectRepository.IsBound("nativeHost"))
                    {
                        RegisterNativeHost();
                    }
                }
            };

            // 页面加载完成事件 - 在这里确保 NativeHost 已注册并通知前端
            _browser.FrameLoadEnd += (sender, args) =>
            {
                if (args.Frame.IsMain)
                {
                    // 立即确保 NativeHost 已注册（不延迟，因为前端可能已经开始检查）
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // 确保 NativeHost 已注册
                        if (!_browser.JavascriptObjectRepository.IsBound("nativeHost"))
                        {
                            RegisterNativeHost();
                        }

                        // 立即注入到 window 作用域（同步绑定可能已经可用，但需要确保在 window 中）
                        InjectNativeHostToWindow();

                        // 立即通知前端（不延迟，让前端尽快知道）
                        NotifyNativeHostReady();
                    });
                }
            };

            // JavaScript 控制台消息（仅在 Debug 模式下输出错误和警告）
            _browser.ConsoleMessage += (sender, args) =>
            {
                #if DEBUG
                // 只输出错误和警告，忽略普通日志
                if (args.Level == LogSeverity.Error || args.Level == LogSeverity.Warning)
                {
                    System.Diagnostics.Debug.WriteLine($"JS {args.Level}: {args.Message} (Line: {args.Line}, Source: {args.Source})");
                }
                #endif
            };
        }

        /// <summary>
        /// 通知前端 NativeHost 已就绪
        /// </summary>
        private void NotifyNativeHostReady()
        {
            try
            {
                string notifyScript = @"
                    (function() {
                        // 调用回调函数（如果存在）
                        if (typeof window.nativeHostReady === 'function') {
                            try {
                                window.nativeHostReady();
                            } catch(e) {
                                console.error('调用 nativeHostReady 时出错:', e);
                            }
                        }
                        
                        // 触发自定义事件
                        try {
                            var event = new Event('nativeHostReady');
                            window.dispatchEvent(event);
                        } catch(e) {
                            console.error('触发事件时出错:', e);
                        }
                        
                        // 设置全局标志
                        window.__nativeHostReady = true;
                    })();
                ";
                _browser.ExecuteScriptAsync(notifyScript);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"通知前端失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 执行 JavaScript 代码
        /// </summary>
        public void ExecuteScript(string script)
        {
            if (_browser.IsBrowserInitialized)
            {
                _browser.ExecuteScriptAsync(script);
            }
        }

        /// <summary>
        /// 调用前端 JavaScript 方法
        /// </summary>
        public void CallFrontendMethod(string methodName, object data = null)
        {
            if (_browser.IsBrowserInitialized)
            {
                string jsonData = data != null ? Newtonsoft.Json.JsonConvert.SerializeObject(data) : "null";
                string script = $"if (typeof window.{methodName} === 'function') {{ window.{methodName}({jsonData}); }}";
                _browser.ExecuteScriptAsync(script);
            }
        }
    }
}

