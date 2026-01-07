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
                retainedFileTimeLimit = TimeSpan.FromDays(logConfig.KeepLogDays),
                fileSizeLimitBytes = 10 * 1024 * 1024, // 10MB
                rollOnFileSizeLimit = true,
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
        /// 日志配置实例，用于获取日志配置信息
        /// </summary>
        private LogConfig logConfig { get; set; } = LogConfig.GetInstance();

        /// <summary>
        /// 写入日志配置类，包含日志路径、滚动间隔、保留天数等配置
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
                    retainedFileTimeLimit: writeLogConfig.retainedFileTimeLimit, // 保留最近 X 天的日志
                    fileSizeLimitBytes: writeLogConfig.fileSizeLimitBytes, // 单个文件最大10MB
                    rollOnFileSizeLimit: writeLogConfig.rollOnFileSizeLimit, // 达到大小限制时创建新文件
                    outputTemplate: writeLogConfig.outputTemplate
                )
                .CreateLogger();
            _isClosed = false;
        }

        /// <summary>
        /// 记录信息级别日志
        /// </summary>
        /// <param name="msg">要记录的日志消息</param>
        /// <exception cref="Exception">当日志服务已关闭时抛出异常</exception>
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
        /// 记录调试级别日志
        /// </summary>
        /// <param name="msg">要记录的日志消息</param>
        /// <exception cref="Exception">当日志服务已关闭时抛出异常</exception>
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
        /// 记录警告级别日志
        /// </summary>
        /// <param name="msg">要记录的日志消息</param>
        /// <exception cref="Exception">当日志服务已关闭时抛出异常</exception>
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
        /// 记录错误级别日志
        /// </summary>
        /// <param name="msg">要记录的日志消息</param>
        /// <exception cref="Exception">当日志服务已关闭时抛出异常</exception>
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
        /// 记录异常日志，包含异常详细信息
        /// </summary>
        /// <param name="ex">要记录的异常对象</param>
        /// <exception cref="Exception">当日志服务已关闭时抛出异常</exception>
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
    /// 写入日志配置类，用于配置日志文件的写入参数
    /// </summary>
    public class WriteLogConfig
    {
        /// <summary>
        /// 日志文件存储路径
        /// </summary>
        public string logPath { get; set; }

        /// <summary>
        /// 日志文件滚动间隔，用于控制日志文件按时间分割的方式
        /// </summary>
        public RollingInterval rollingInterval { get; set; }

        /// <summary>
        /// 保留日志文件的时间限制，超过此时间的日志文件将被自动删除
        /// </summary>
        public TimeSpan? retainedFileTimeLimit { get; set; }

        /// <summary>
        /// 单个日志文件的最大大小限制（字节），超过此大小将创建新文件
        /// </summary>
        public long? fileSizeLimitBytes { get; set; }

        /// <summary>
        /// 当日志文件达到大小限制时是否创建新文件
        /// </summary>
        public bool rollOnFileSizeLimit { get; set; }

        /// <summary>
        /// 日志输出模板，用于格式化日志消息的显示格式
        /// </summary>
        public string outputTemplate { get; set; }
    }
}
