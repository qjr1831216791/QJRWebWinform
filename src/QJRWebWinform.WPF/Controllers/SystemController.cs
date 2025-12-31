using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using CefSharp;

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

        /// <summary>
        /// 保存文件（异步版本，用于桌面端文件下载）
        /// </summary>
        /// <param name="parameters">JSON参数：{"base64Data": "string", "fileName": "string"}</param>
        /// <param name="callback">JavaScript 回调函数</param>
        public void SaveFile(string parameters, IJavascriptCallback callback)
        {
            Task.Run(() =>
            {
                try
                {
                    var param = DeserializeParameters<SaveFileParams>(parameters);
                    if (param == null || string.IsNullOrWhiteSpace(param.Base64Data))
                    {
                        var errorResult = Newtonsoft.Json.JsonConvert.SerializeObject(new
                        {
                            success = false,
                            error = "Base64 数据不能为空"
                        });
                        callback.ExecuteAsync(false, errorResult);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(param.FileName))
                    {
                        var errorResult = Newtonsoft.Json.JsonConvert.SerializeObject(new
                        {
                            success = false,
                            error = "文件名不能为空"
                        });
                        callback.ExecuteAsync(false, errorResult);
                        return;
                    }

                    // 在 UI 线程上显示保存文件对话框
                    string filePath = null;
                    InvokeOnUIThread(() =>
                    {
                        var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                        {
                            FileName = param.FileName,
                            Filter = GetFileFilter(param.FileName),
                            DefaultExt = Path.GetExtension(param.FileName)
                        };

                        if (saveFileDialog.ShowDialog() == true)
                        {
                            filePath = saveFileDialog.FileName;
                        }
                    });

                    if (string.IsNullOrWhiteSpace(filePath))
                    {
                        // 用户取消了保存对话框
                        var cancelResult = Newtonsoft.Json.JsonConvert.SerializeObject(new
                        {
                            success = false,
                            error = "用户取消了保存操作"
                        });
                        callback.ExecuteAsync(false, cancelResult);
                        return;
                    }

                    // 将 base64 数据转换为字节数组
                    byte[] fileBytes;
                    try
                    {
                        // 处理可能包含 data URL 前缀的 base64 字符串
                        string base64Data = param.Base64Data;
                        if (base64Data.Contains(","))
                        {
                            base64Data = base64Data.Substring(base64Data.IndexOf(",") + 1);
                        }
                        fileBytes = Convert.FromBase64String(base64Data);
                    }
                    catch (Exception ex)
                    {
                        var errorResult = Newtonsoft.Json.JsonConvert.SerializeObject(new
                        {
                            success = false,
                            error = $"Base64 数据转换失败: {ex.Message}"
                        });
                        callback.ExecuteAsync(false, errorResult);
                        return;
                    }

                    // 保存文件
                    File.WriteAllBytes(filePath, fileBytes);

                    var successResult = Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        success = true,
                        message = $"文件已保存到: {filePath}",
                        filePath = filePath
                    });
                    callback.ExecuteAsync(true, successResult);
                }
                catch (Exception ex)
                {
                    var errorResult = Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = ex.Message
                    });
                    callback.ExecuteAsync(false, errorResult);
                }
            });
        }

        /// <summary>
        /// 根据文件扩展名获取文件过滤器
        /// </summary>
        private string GetFileFilter(string fileName)
        {
            string ext = Path.GetExtension(fileName)?.ToLower();
            switch (ext)
            {
                case ".xlsx":
                    return "Excel 文件 (*.xlsx)|*.xlsx|所有文件 (*.*)|*.*";
                case ".docx":
                    return "Word 文档 (*.docx)|*.docx|所有文件 (*.*)|*.*";
                case ".pdf":
                    return "PDF 文件 (*.pdf)|*.pdf|所有文件 (*.*)|*.*";
                default:
                    return "所有文件 (*.*)|*.*";
            }
        }

        private class ShowMessageParams
        {
            public string Message { get; set; }
            public string Title { get; set; }
        }

        private class SaveFileParams
        {
            public string Base64Data { get; set; }
            public string FileName { get; set; }
        }
    }
}

