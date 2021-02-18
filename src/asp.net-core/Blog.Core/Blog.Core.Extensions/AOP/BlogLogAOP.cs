using Blog.Core.Common;
using Castle.DynamicProxy;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Blog.Core.Extensions
{
    public class BlogLogAOP : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            // 事前处理: 在服务方法执行之前,做相应的逻辑处理
            var dataIntercept = "" +
                $"【当前执行方法】：{ invocation.Method.Name} \r\n" +
                $"【携带的参数有】： " +
                $"{string.Join(", ", invocation.Arguments.Select(a => (a ?? "").ToString()).ToArray())} \r\n";


            // 执行当前访问的服务方法,(注意:如果下边还有其他的AOP拦截器的话,会跳转到其他的AOP里)
            invocation.Proceed();

            // 事后处理: 在service被执行了以后,做相应的处理,这里是输出到日志文件
            dataIntercept += ($"【执行完成结果】：{invocation.ReturnValue}");

            // 输出到日志文件
            Parallel.For(0, 1, e =>
            {
                LogLock.OutSql2Log("AOPLog", new string[] { dataIntercept });
            });
        }

        public static bool IsAsyncMethod(MethodInfo method)
        {
            return (
                method.ReturnType == typeof(Task) ||
                (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                );
        }
    }

    internal static class InternalAsyncHelper
    {
        public static async Task AwaitTaskWithPostActionAndFinally(Task actualReturnValue,
            Func<Task> postAction, Action<Exception> finalAction)
        {
            Exception exception = null;
            try
            {
                await actualReturnValue;
                await postAction();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                finalAction(exception);
            }
        }
    }
}
