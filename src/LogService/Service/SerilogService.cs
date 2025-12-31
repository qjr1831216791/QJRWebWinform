using LogService.Model;
using Serilog;
using System;
using System.IO;

namespace LogService.Service
{
    public class SerilogService
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        private SerilogService()
        {
            writeLogConfig = new WriteLogConfig()
            {
                logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logConfig.TracePath),
                rollingInterval = RollingInterval.Day,
                retainedFileCountLimit = logConfig.KeepLogDays,
                outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}{NewLine}" // 可选：自定义模板
            };
            this.Init();
        }

        private static readonly object syncRoot = new object();

        private static SerilogService instance = new SerilogService();

        private ILogger _logger;

        /// <summary>
        /// 日志是否已关闭
        /// </summary>
        private bool _isClosed = false;

        /// <summary>
        /// 日志配置静态类
        /// </summary>
        private LogConfig logConfig { get; set; } = LogConfig.GetInstance();

        /// <summary>
        /// 写入日志配置类
        /// </summary>
        public WriteLogConfig writeLogConfig { get; set; }

        /// <summary>
        /// 获取实例
        /// </summary>
        /// <returns></returns>
        public static SerilogService GetInstance()
        {
            // 双重检查锁定
            if (instance == null)
            {
                lock (syncRoot)
                {
                    if (instance == null)
                    {
                        instance = new SerilogService();
                    }
                }
            }

            return instance;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            _logger = new LoggerConfiguration()
                .MinimumLevel.Debug() // 设置最低日志级别为 Debug
                .WriteTo.File(
                    path: writeLogConfig.logPath, // 日志文件路径
                    rollingInterval: writeLogConfig.rollingInterval, // 按天滚动
                    retainedFileCountLimit: writeLogConfig.retainedFileCountLimit, // 保留最近 X 天的日志
                    outputTemplate: writeLogConfig.outputTemplate
                )
                .CreateLogger();
            _isClosed = false;
        }

        /// <summary>
        /// 记录详细日志
        /// </summary>
        /// <param name="msg"></param>
        public void InfoMsg(string msg)
        {
            if (!_isClosed)
            {
                _logger.Information(msg);
            }
            else
            {
                throw new Exception("日志服务已关闭");
            }
        }

        /// <summary>
        /// 记录Debug日志
        /// </summary>
        /// <param name="msg"></param>
        public void DebugMsg(string msg)
        {
            if (!_isClosed)
            {
                _logger.Debug(msg);
            }
            else
            {
                throw new Exception("日志服务已关闭");
            }
        }

        /// <summary>
        /// 记录Warning日志
        /// </summary>
        /// <param name="msg"></param>
        public void WarningMsg(string msg)
        {
            if (!_isClosed)
            {
                _logger.Warning(msg);
            }
            else
            {
                throw new Exception("日志服务已关闭");
            }
        }

        /// <summary>
        /// 记录Error日志
        /// </summary>
        /// <param name="msg"></param>
        public void ErrorMsg(string msg)
        {
            if (!_isClosed)
            {
                _logger.Error(msg);
            }
            else
            {
                throw new Exception("日志服务已关闭");
            }
        }

        /// <summary>
        /// 记录Exception日志
        /// </summary>
        /// <param name="msg"></param>
        public void LogException(Exception ex)
        {
            if (!_isClosed)
            {
                _logger.Error(ex, "发生异常");
            }
            else
            {
                throw new Exception("日志服务已关闭");
            }
        }

        /// <summary>
        /// 关闭并刷新日志
        /// </summary>
        public void CloseAndFlush()
        {
            _isClosed = true;
            Log.CloseAndFlush(); // 确保所有日志写入
        }
    }

    /// <summary>
    /// 写入日志配置类
    /// </summary>
    public class WriteLogConfig
    {
        /// <summary>
        /// 日志路径
        /// </summary>
        public string logPath { get; set; }

        /// <summary>
        /// 间隔时间
        /// </summary>
        public RollingInterval rollingInterval { get; set; }

        /// <summary>
        /// 保留日志天数
        /// </summary>
        public int retainedFileCountLimit { get; set; }

        /// <summary>
        /// 日志模板
        /// </summary>
        public string outputTemplate { get; set; }
    }
}
