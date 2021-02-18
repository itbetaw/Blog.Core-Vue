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
    public class BlogCacheAOP : CacheAOPBase
    {
        // 通过注入的方式，把缓存操作接口通过构造函数注入
        private ICaching _cache;
        public BlogCacheAOP(ICaching cache)
        {
            _cache = cache;
        }
        //Intercept方法是拦截的关键所在，也是IInterceptor接口中的唯一定义
        public override void Intercept(IInvocation invocation)
        {
            var method = invocation.MethodInvocationTarget ?? invocation.Method;
            var qCachingAttribute = method.GetCustomAttributes(true).FirstOrDefault(x =>
              x.GetType() == typeof(CachingAttribute)) as CachingAttribute;
            if (qCachingAttribute != null)
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
            else
            {
                invocation.Proceed();
            }
        }


    }
}
