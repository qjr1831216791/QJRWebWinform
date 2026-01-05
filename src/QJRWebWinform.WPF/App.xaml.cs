using CefSharp;
using CefSharp.Internals;
using CefSharp.Wpf;
using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace QJRWebWinform.WPF
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// 安全地添加 CefSharp 命令行参数（防重复添加）
        /// </summary>
        /// <param name="args">命令行参数字典</param>
        /// <param name="key">参数键</param>
        /// <param name="value">参数值</param>
        /// <param name="overwrite">如果键已存在，是否覆盖（默认：false，跳过）</param>
        private static void SafeAddCommandLineArg(CommandLineArgDictionary args, string key, string value, bool overwrite = false)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("参数键不能为空", nameof(key));

            if (args.ContainsKey(key))
            {
                if (overwrite)
                {
                    // 覆盖已存在的值
                    args[key] = value;
                    System.Diagnostics.Debug.WriteLine($"已覆盖命令行参数: {key} = {value}");
                }
                else
                {
                    // 跳过，不重复添加
                    System.Diagnostics.Debug.WriteLine($"跳过重复的命令行参数: {key} (当前值: {args[key]})");
                }
            }
            else
            {
                // 添加新参数
                args.Add(key, value);
            }
        }

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

            // ============================================
            // 功能优化配置：禁用不需要的功能以减少占用空间
            // ============================================

            // 1. 禁用 PDF 查看器
            // 说明：禁用 Chromium 内置的 PDF 查看器功能
            // 影响：无法在浏览器中直接查看 PDF 文件，PDF 文件将触发下载或使用系统默认程序打开
            // 可减少空间：约 5-10MB（PDF 扩展相关文件）
            SafeAddCommandLineArg(settings.CefCommandLineArgs, "disable-pdf-extension", "1");

            // 2. 限制语言包支持（仅保留中文和英文）
            // 说明：限制 CefSharp 支持的语言列表，只保留中文（简体、繁体）和英文（美式、英式）
            // 影响：其他语言的网页界面可能显示为英文或中文，但不会影响网页内容本身
            // 可减少空间：约 10-20MB（locales 目录中其他语言文件，默认包含 100+ 种语言）
            // 注意：此配置需要在打包脚本中配合，只复制需要的语言文件（zh-CN.pak, zh.pak, en-US.pak, en.pak）
            settings.AcceptLanguageList = "zh-CN,zh,en-US,en";

            // 3. 禁用浏览器插件支持
            // 说明：禁用所有浏览器插件（如已淘汰的 Flash 插件等）
            // 影响：无法使用任何浏览器插件，现代网页基本不依赖插件
            // 可减少空间：约 1-2MB（插件相关代码）
            SafeAddCommandLineArg(settings.CefCommandLineArgs, "disable-plugins", "1");

            // 4. 禁用浏览器扩展支持
            // 说明：禁用浏览器扩展功能
            // 影响：无法安装或使用浏览器扩展，嵌入式浏览器通常不需要扩展功能
            // 可减少空间：约 1-2MB（扩展相关代码）
            SafeAddCommandLineArg(settings.CefCommandLineArgs, "disable-extensions", "1");

            // 5. 禁用音频播放功能
            // 说明：禁用所有音频播放功能，包括 HTML5 音频、Web Audio API 等
            // 影响：网页中的音频内容将无法播放，包括背景音乐、音效、语音等
            // 可减少空间：约 1-3MB（音频编解码器和相关代码）
            // 注意：如果应用需要音频功能，请移除此配置
            SafeAddCommandLineArg(settings.CefCommandLineArgs, "disable-audio", "1");

            // 6. 禁用视频播放功能
            // 说明：禁用所有视频播放功能，包括 HTML5 视频、视频流等
            // 影响：网页中的视频内容将无法播放，包括 HTML5 <video> 标签、视频流等
            // 可减少空间：约 2-5MB（视频编解码器和相关代码）
            // 注意：如果应用需要视频功能，请移除此配置
            SafeAddCommandLineArg(settings.CefCommandLineArgs, "disable-video", "1");

            // 7. 禁用 WebRTC 功能
            // 说明：禁用 Web Real-Time Communication（Web 实时通信）功能
            // 影响：无法使用 WebRTC 进行音视频通话、屏幕共享、P2P 数据传输等功能
            // 可减少空间：约 3-8MB（WebRTC 相关代码和库）
            // 注意：如果应用需要音视频通话功能，请移除此配置
            SafeAddCommandLineArg(settings.CefCommandLineArgs, "disable-webrtc", "1");

            // 8. 禁用翻译功能
            // 说明：禁用 Chromium 内置的网页翻译功能（Google 翻译）
            // 影响：无法使用浏览器右键菜单中的"翻译"功能，网页内容不会自动提示翻译
            // 可减少空间：约 1-2MB（翻译相关代码和资源）
            // 注意：嵌入式桌面应用通常不需要此功能，禁用可减少不必要的网络请求
            SafeAddCommandLineArg(settings.CefCommandLineArgs, "disable-translate", "1");

            // 9. 禁用同步功能
            // 说明：禁用浏览器数据同步功能（书签、历史记录、密码等同步）
            // 影响：无法同步浏览器数据到云端，嵌入式浏览器通常不需要此功能
            // 可减少空间：约 1-2MB（同步相关代码和网络请求处理）
            // 注意：桌面应用通常不需要浏览器同步功能，禁用可减少后台网络活动
            SafeAddCommandLineArg(settings.CefCommandLineArgs, "disable-sync", "1");

            // 10. 禁用通知功能
            // 说明：禁用 Web Notifications API，阻止网页显示系统通知
            // 影响：网页无法通过 Notification API 显示系统级通知弹窗
            // 可减少空间：约 0.5-1MB（通知相关代码）
            // 注意：桌面应用通常不需要网页通知功能，禁用可提升用户体验（避免干扰）
            SafeAddCommandLineArg(settings.CefCommandLineArgs, "disable-notifications", "1");

            // 11. 禁用后台网络请求
            // 说明：禁用后台网络请求，包括更新检查、遥测数据上报、预加载等
            // 影响：无法进行后台更新检查、无法上报使用统计等，但不会影响正常的网页加载
            // 可减少空间：约 1-2MB（后台网络请求相关代码）
            // 注意：禁用后可减少不必要的网络流量和后台活动，提升隐私保护
            SafeAddCommandLineArg(settings.CefCommandLineArgs, "disable-background-networking", "1");

            // 12. 禁用组件更新
            // 说明：禁用 Chromium 组件的自动更新检查功能
            // 影响：无法自动检查和更新浏览器组件（如安全更新等）
            // 可减少空间：约 1-2MB（组件更新相关代码）
            // 注意：嵌入式应用通常通过应用更新来更新浏览器组件，不需要自动更新功能
            SafeAddCommandLineArg(settings.CefCommandLineArgs, "disable-component-update", "1");

            // 13. 禁用语音 API
            // 说明：禁用 Web Speech API（语音识别和语音合成功能）
            // 影响：无法使用语音识别（SpeechRecognition）和语音合成（SpeechSynthesis）功能
            // 可减少空间：约 2-4MB（语音识别和合成引擎相关代码）
            // 注意：如果应用不需要语音输入输出功能，禁用可显著减少占用空间
            SafeAddCommandLineArg(settings.CefCommandLineArgs, "disable-speech-api", "1");

            // 14. 禁用 WebGL 功能
            // 说明：禁用 WebGL（Web Graphics Library），用于在浏览器中渲染 3D 图形
            // 影响：无法运行 WebGL 应用和 3D 图形内容，但普通的 2D Canvas 和 CSS 动画不受影响
            // 可减少空间：约 2-5MB（WebGL 相关代码和库）
            // 注意：如果应用不需要 3D 图形功能，禁用可减少占用空间
            SafeAddCommandLineArg(settings.CefCommandLineArgs, "disable-webgl", "1");

            // 15. 禁用 GPU 硬件加速
            // 说明：禁用 GPU 硬件加速，使用软件渲染
            // 影响：可能降低渲染性能，某些复杂的 CSS 动画和 Canvas 操作可能变慢，但基本网页浏览不受影响
            // 可减少空间：约 15-25MB（可移除 GPU 相关 DLL 文件：libEGL.dll, libGLESv2.dll, vulkan-1.dll, vk_swiftshader.dll, d3dcompiler_47.dll 等）
            // 注意：禁用 GPU 后需要在打包脚本中排除 GPU 相关文件，否则这些文件仍会被包含
            // 警告：禁用 GPU 可能影响某些 Web 内容的渲染性能，建议在禁用前测试应用性能
            SafeAddCommandLineArg(settings.CefCommandLineArgs, "disable-gpu", "1");
            SafeAddCommandLineArg(settings.CefCommandLineArgs, "disable-gpu-compositing", "1");
            SafeAddCommandLineArg(settings.CefCommandLineArgs, "disable-software-rasterizer", "1");

            // ============================================
            // 调试配置（仅在 Debug 模式下启用）
            // ============================================

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

