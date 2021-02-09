using Blog.Core.Common;
using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Core.Extensions
{
    /// <summary>
    ///  面向切面的缓存使用
    /// </summary>
    public class BlogCacheAOP : IInterceptor
    {
        // 通过注入的方式，把缓存操作接口通过构造函数注入
        private ICaching _cache;
        public BlogCacheAOP(ICaching cache)
        {
            _cache = cache;
        }
        //Intercept方法是拦截的关键所在，也是IInterceptor接口中的唯一定义
        public void Intercept(IInvocation invocation)
        {
            // 获取自定义缓存键
            var cacheKey = CustomCacheKey(invocation);
            var cacheValue = _cache.Get(cacheKey);
            if (cacheValue != null)
            {
                // 将当前获取到的缓存值，赋值给当前执行方法
                invocation.ReturnValue = cacheValue;
                return;
            }
            // 执行当前的方法
            invocation.Proceed();

            if (!string.IsNullOrWhiteSpace(cacheKey))
            {
                _cache.Set(cacheKey, invocation.ReturnValue);
            }
        }

        private string CustomCacheKey(IInvocation invocation)
        {
            var typeName = invocation.TargetType.Name;
            var methodName = invocation.Method.Name;
            var methodArguments = invocation.Arguments.Select(GetArgumentValue).Take(3).ToList();
            string key = $"{typeName}:{methodName}:";
            foreach (var param in methodArguments)
            {
                key += $"{param}:";
            }
            return key.TrimEnd(':');
        }
        //object 转 string
        private string GetArgumentValue(object arg)
        {
            // PS：这里仅仅是很简单的数据类型，如果参数是表达式/类等，比较复杂的，请看我的在线代码吧，封装的比较多，当然也可以自己封装。
            if (arg is int || arg is long || arg is string)
                return arg.ToString();

            if (arg is DateTime)
                return ((DateTime)arg).ToString("yyyyMMddHHmmss");

            return "";
        }
    }
}
