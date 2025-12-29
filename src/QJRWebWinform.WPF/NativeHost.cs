using System;
using System.Windows;
using CefSharp;

namespace QJRWebWinform.WPF
{
    /// <summary>
    /// 暴露给 JavaScript 的 C# 对象
    /// 前端可以通过 window.nativeHost 访问这些方法
    /// </summary>
    public class NativeHost
    {
        private Window _mainWindow;

        public NativeHost(Window mainWindow)
        {
            _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
        }

        /// <summary>
        /// 示例方法：显示消息框
        /// </summary>
        public void ShowMessage(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(message, "来自后端", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        /// <summary>
        /// 示例方法：获取系统信息
        /// </summary>
        public string GetSystemInfo()
        {
            return $"操作系统: {Environment.OSVersion}\n.NET 版本: {Environment.Version}";
        }

        /// <summary>
        /// 示例方法：处理数据并返回结果
        /// </summary>
        public string ProcessData(string input)
        {
            // 模拟业务逻辑处理
            return $"处理后的数据: {input.ToUpper()}";
        }

        /// <summary>
        /// 示例方法：保存数据（异步操作示例）
        /// </summary>
        public void SaveData(string data, IJavascriptCallback callback)
        {
            try
            {
                // 模拟保存操作
                System.Threading.Thread.Sleep(500);
                
                // 调用 JavaScript 回调函数
                callback.ExecuteAsync(true, "数据保存成功");
            }
            catch (Exception ex)
            {
                callback.ExecuteAsync(false, $"保存失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 示例方法：关闭窗口
        /// </summary>
        public void CloseWindow()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _mainWindow.Close();
            });
        }

        /// <summary>
        /// 示例方法：设置窗口标题
        /// </summary>
        public void SetWindowTitle(string title)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _mainWindow.Title = title;
            });
        }
    }
}

