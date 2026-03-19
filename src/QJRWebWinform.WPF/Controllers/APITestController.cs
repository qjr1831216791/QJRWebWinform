using APIService.APITest;
using CommonHelper.Model;
using CefSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace QJRWebWinform.WPF.Controllers
{
    public class APITestController : DynamicControllerBase
    {
        public APITestController(Window mainWindow) : base(mainWindow)
        {
        }

        public override string Name => "APITest";

        private ResultModel TestAPIRunCore(string parameters)
        {
            SetParameters(parameters);
            ResultModel result = new ResultModel();
            result.Success(message: "API is running.");
            return result;
        }

        public virtual object TestAPIRun(string parameters)
        {
            return TestAPIRunCore(parameters);
        }

        public virtual void TestAPIRun(string parameters, IJavascriptCallback callback)
        {
            Task.Run(() =>
            {
                try
                {
                    var result = TestAPIRunCore(parameters);
                    callback.ExecuteAsync(true, JsonConvert.SerializeObject(result));
                }
                catch (Exception ex)
                {
                    callback.ExecuteAsync(false, ex.Message);
                }
            });
        }

        private ResultModel TestCRMServiceCore(string parameters)
        {
            SetParameters(parameters);
            return Command<APITestCommand>().TestCRMService();
        }

        public virtual object TestCRMService(string parameters)
        {
            return TestCRMServiceCore(parameters);
        }

        public virtual void TestCRMService(string parameters, IJavascriptCallback callback)
        {
            Task.Run(() =>
            {
                try
                {
                    var result = TestCRMServiceCore(parameters);
                    callback.ExecuteAsync(true, JsonConvert.SerializeObject(result));
                }
                catch (Exception ex)
                {
                    callback.ExecuteAsync(false, ex.Message);
                }
            });
        }

        private ResultModel TestLogTraceCore(string parameters)
        {
            var input = DeserializeParameters<TestLogTraceInput>(parameters);
            SetParameters(parameters);
            return Command<APITestCommand>().TestLogTrace(input?.level, input?.message ?? "Hello World");
        }

        public virtual object TestLogTrace(string parameters)
        {
            return TestLogTraceCore(parameters);
        }

        public virtual void TestLogTrace(string parameters, IJavascriptCallback callback)
        {
            Task.Run(() =>
            {
                try
                {
                    var result = TestLogTraceCore(parameters);
                    callback.ExecuteAsync(true, JsonConvert.SerializeObject(result));
                }
                catch (Exception ex)
                {
                    callback.ExecuteAsync(false, ex.Message);
                }
            });
        }

        private ResultModel TestAPIPostCore(string parameters)
        {
            var input = DeserializeParameters<TestAPIPostInput>(parameters);
            ResultModel result = new ResultModel();
            result.Success(message: $"Input is：{input?.input}");
            return result;
        }

        public virtual object TestAPIPost(string parameters)
        {
            return TestAPIPostCore(parameters);
        }

        public virtual void TestAPIPost(string parameters, IJavascriptCallback callback)
        {
            Task.Run(() =>
            {
                try
                {
                    var result = TestAPIPostCore(parameters);
                    callback.ExecuteAsync(true, JsonConvert.SerializeObject(result));
                }
                catch (Exception ex)
                {
                    callback.ExecuteAsync(false, ex.Message);
                }
            });
        }

        private ResultModel TestAPIPost2Core(string parameters)
        {
            var input = DeserializeParameters<TestAPIPost2Input>(parameters);
            ResultModel result = new ResultModel();
            result.Success(message: $"Input is：{input?.input},Input2 is：{input?.input2}");
            return result;
        }

        public virtual object TestAPIPost2(string parameters)
        {
            return TestAPIPost2Core(parameters);
        }

        public virtual void TestAPIPost2(string parameters, IJavascriptCallback callback)
        {
            Task.Run(() =>
            {
                try
                {
                    var result = TestAPIPost2Core(parameters);
                    callback.ExecuteAsync(true, JsonConvert.SerializeObject(result));
                }
                catch (Exception ex)
                {
                    callback.ExecuteAsync(false, ex.Message);
                }
            });
        }

        private ResultModel TestAPIPost3Core(string parameters)
        {
            var input = DeserializeParameters<TestAPIPost3Model>(parameters);
            ResultModel result = new ResultModel();
            result.Success(message: $"Input is：{JsonConvert.SerializeObject(input)}");
            return result;
        }

        public virtual object TestAPIPost3(string parameters)
        {
            return TestAPIPost3Core(parameters);
        }

        public virtual void TestAPIPost3(string parameters, IJavascriptCallback callback)
        {
            Task.Run(() =>
            {
                try
                {
                    var result = TestAPIPost3Core(parameters);
                    callback.ExecuteAsync(true, JsonConvert.SerializeObject(result));
                }
                catch (Exception ex)
                {
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
        public List<string> list { get; set; }
    }
}
