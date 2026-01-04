using APIService.Default;
using CommonHelper.Model;
using Newtonsoft.Json;
using System.Web.Http;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    /// <summary>
    /// DefaultController
    /// </summary>
    public class DefaultController : BaseController
    {
        /// <summary>
        /// 测试API是否联通
        /// </summary>
        [HttpGet]
        public virtual ResultModel TestAPIRun()
        {
            ResultModel result = new ResultModel();
            result.Success(message: "API is running.");
            return result;
        }

        /// <summary>
        /// 测试组织服务是否联通
        /// </summary>
        [HttpGet]
        public virtual ResultModel TestCRMService()
        {
            return Command<DefaultCommand>().TestCRMService();
        }

        /// <summary>
        /// 测试日志功能是否正常
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="message">日志消息，默认为"Hello World"</param>
        /// <returns>测试结果</returns>
        [HttpGet]
        public virtual ResultModel TestLogTrace(string level, string message = "Hello World")
        {
            return Command<DefaultCommand>().TestLogTrace(level, message);
        }

        /// <summary>
        /// 测试API Post入参-单个基础属性
        /// </summary>
        /// <param name="input">输入的字符串参数</param>
        /// <returns>包含输入参数的结果模型</returns>
        [HttpPost]
        public virtual ResultModel TestAPIPost(string input)
        {
            ResultModel result = new ResultModel();
            result.Success(message: $"Input is：{input}");
            return result;
        }

        /// <summary>
        /// 测试API Post入参2-多个基础属性
        /// </summary>
        /// <param name="input">第一个输入的字符串参数</param>
        /// <param name="input2">第二个输入的字符串参数</param>
        /// <returns>包含所有输入参数的结果模型</returns>
        [HttpPost]
        public virtual ResultModel TestAPIPost2(string input, string input2)
        {
            ResultModel result = new ResultModel();
            result.Success(message: $"Input is：{input},Input2 is：{input2}");
            return result;
        }

        /// <summary>
        /// 测试API Post入参3-复杂参数
        /// </summary>
        /// <param name="input">复杂对象参数，包含input和list属性</param>
        /// <returns>包含序列化后输入参数的结果模型</returns>
        [HttpPost]
        public virtual ResultModel TestAPIPost3([FromBody] TestAPIPost3Model input)
        {
            ResultModel result = new ResultModel();
            result.Success(message: $"Input is：{JsonConvert.SerializeObject(input)}");
            return result;
        }

        /// <summary>
        /// 获取网站配置的CRM环境信息
        /// </summary>
        /// <returns>包含所有配置的CRM环境信息的结果模型</returns>
        [HttpGet]
        public virtual ResultModel GetCRMEnvironments()
        {
            return Command<DefaultCommand>().GetCRMEnvironments();
        }
    }
}