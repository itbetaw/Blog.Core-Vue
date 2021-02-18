using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Core.Common
{

    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class CachingAttribute : Attribute
    {
        /// <summary>
        /// 缓存绝对过期时间
        /// </summary>
        public int AbsoluteExpiration { get; set; } = 30;
    }
}
