using APIService.Default;
using CommonHelper.Model;
using CefSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace QJRWebWinform.WPF.Controllers
{
    public class DefaultController : DynamicControllerBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="mainWindow"></param>
        public DefaultController(Window mainWindow) : base(mainWindow)
        {
        }

        public override string Name => "Default";

        /// <summary>
        /// 测试API是否联通
        /// </summary>
        /// <param name="parameters">JSON参数：null 或 "{}"</param>
        public virtual object TestAPIRun(string parameters)
        {
            SetParameters(parameters);
            ResultModel result = new ResultModel();
            result.Success(message: "API is running.");
            return result;
        }

        /// <summary>
        /// 测试组织服务是否联通（核心业务逻辑）
        /// </summary>
        /// <param name="parameters">JSON参数：null 或 "{}"</param>
        private ResultModel TestCRMServiceCore(string parameters)
        {
            SetParameters(parameters);
            return Command<DefaultCommand>().TestCRMService();
        }

        /// <summary>
        /// 测试组织服务是否联通（同步版本）
        /// </summary>
        /// <param name="parameters">JSON参数：null 或 "{}"</param>
        public virtual object TestCRMService(string parameters)
        {
            return TestCRMServiceCore(parameters);
        }

        /// <summary>
        /// 测试组织服务是否联通（异步版本）
        /// </summary>
        /// <param name="parameters">JSON参数：null 或 "{}"</param>
        /// <param name="callback">JavaScript 回调函数</param>
        public virtual void TestCRMService(string parameters, IJavascriptCallback callback)
        {
            // 使用 Task.Run 在后台线程执行，避免阻塞 UI 线程
            Task.Run(() =>
            {
                try
                {
                    var result = TestCRMServiceCore(parameters);
                    var resultJson = JsonConvert.SerializeObject(result);
                    callback.ExecuteAsync(true, resultJson);
                }
                catch (Exception ex)
                {
                    callback.ExecuteAsync(false, ex.Message);
                }
            });
        }

        /// <summary>
        /// 测试日志功能是否正常
        /// </summary>
        /// <param name="parameters">JSON参数：{"level": "string", "message": "string"}</param>
        public virtual object TestLogTrace(string parameters)
        {
            var input = DeserializeParameters<TestLogTraceInput>(parameters);
            SetParameters(parameters);
            return Command<DefaultCommand>().TestLogTrace(input?.level, input?.message ?? "Hello World");
        }

        /// <summary>
        /// 测试API Post入参-单个基础属性
        /// </summary>
        /// <param name="parameters">JSON参数：{"input": "string"}</param>
        /// <returns></returns>
        public virtual object TestAPIPost(string parameters)
        {
            var input = DeserializeParameters<TestAPIPostInput>(parameters);
            ResultModel result = new ResultModel();
            result.Success(message: $"Input is：{input?.input}");
            return result;
        }

        /// <summary>
        /// 测试API Post入参2-多个基础属性
        /// </summary>
        /// <param name="parameters">JSON参数：{"input": "string", "input2": "string"}</param>
        /// <returns></returns>
        public virtual object TestAPIPost2(string parameters)
        {
            var input = DeserializeParameters<TestAPIPost2Input>(parameters);
            ResultModel result = new ResultModel();
            result.Success(message: $"Input is：{input?.input},Input2 is：{input?.input2}");
            return result;
        }

        /// <summary>
        /// 测试API Post入参3-复杂参数
        /// </summary>
        /// <param name="parameters">JSON参数：TestAPIPost3Model 对象</param>
        /// <returns></returns>
        public virtual object TestAPIPost3(string parameters)
        {
            var input = DeserializeParameters<TestAPIPost3Model>(parameters);
            ResultModel result = new ResultModel();
            result.Success(message: $"Input is：{JsonConvert.SerializeObject(input)}");
            return result;
        }

        /// <summary>
        /// 获取网站配置的CRM环境信息（核心业务逻辑）
        /// </summary>
        /// <param name="parameters">JSON参数：null 或 "{}"</param>
        /// <returns></returns>
        private ResultModel GetCRMEnvironmentsCore(string parameters)
        {
            SetParameters(parameters);
            return Command<DefaultCommand>().GetCRMEnvironments();
        }

        /// <summary>
        /// 获取网站配置的CRM环境信息（同步版本）
        /// </summary>
        /// <param name="parameters">JSON参数：null 或 "{}"</param>
        /// <returns></returns>
        public virtual object GetCRMEnvironments(string parameters)
        {
            return GetCRMEnvironmentsCore(parameters);
        }

        /// <summary>
        /// 获取网站配置的CRM环境信息（异步版本）
        /// </summary>
        /// <param name="parameters">JSON参数：null 或 "{}"</param>
        /// <param name="callback">JavaScript 回调函数</param>
        public virtual void GetCRMEnvironments(string parameters, IJavascriptCallback callback)
        {
            // 使用 Task.Run 在后台线程执行，避免阻塞 UI 线程
            Task.Run(() =>
            {
                try
                {
                    var result = GetCRMEnvironmentsCore(parameters);
                    
                    // 将 ResultModel 序列化为 JSON 字符串
                    // 注意：异步 API 需要返回 JSON 字符串，前端会解析
                    var resultJson = JsonConvert.SerializeObject(result);
                    
                    // 成功时调用回调：callback.ExecuteAsync(true, resultJson)
                    callback.ExecuteAsync(true, resultJson);
                }
                catch (Exception ex)
                {
                    // 失败时调用回调：callback.ExecuteAsync(false, errorMessage)
                    callback.ExecuteAsync(false, ex.Message);
                }
            });
        }

        private class TestLogTraceInput
        {
            public string level { get; set; }
            public string message { get; set; }
        }

        private class TestAPIPostInput
        {
            public string input { get; set; }
        }

        private class TestAPIPost2Input
        {
            public string input { get; set; }
            public string input2 { get; set; }
        }
    }

    public class TestAPIPost3Model
    {
        public string input { get; set; }

        public List<String> list { get; set; }
    }
}
