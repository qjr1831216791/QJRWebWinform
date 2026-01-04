using System;
using System.Threading;
using System.Threading.Tasks;

namespace CommonHelper
{
    /// <summary>
    /// 异步工具类，提供同步执行异步方法的功能
    /// </summary>
    public static class AsyncUtil
    {
        /// <summary>
        /// 任务工厂实例，用于创建和执行任务
        /// </summary>
        private static readonly TaskFactory _taskFactory =
                new TaskFactory(CancellationToken.None,
                        TaskCreationOptions.None,
                        TaskContinuationOptions.None,
                        TaskScheduler.Default);

        /// <summary>
        /// 同步执行返回void的异步Task方法
        /// </summary>
        /// <param name="task">要执行的异步任务委托</param>
        /// <remarks>
        /// 使用示例: AsyncUtil.RunSync(() => AsyncMethod());
        /// </remarks>
        public static void RunSync(Func<Task> task)
            => _taskFactory
                .StartNew(task)
                .Unwrap()
                .GetAwaiter()
                .GetResult();

        /// <summary>
        /// 同步执行返回TResult的异步Task方法
        /// </summary>
        /// <typeparam name="TResult">异步方法的返回类型</typeparam>
        /// <param name="task">要执行的异步任务委托</param>
        /// <returns>异步方法的返回值</returns>
        /// <remarks>
        /// 使用示例: TResult result = AsyncUtil.RunSync(() => AsyncMethod&lt;TResult&gt;());
        /// </remarks>
        public static TResult RunSync<TResult>(Func<Task<TResult>> task)
            => _taskFactory
                .StartNew(task)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
    }
}
