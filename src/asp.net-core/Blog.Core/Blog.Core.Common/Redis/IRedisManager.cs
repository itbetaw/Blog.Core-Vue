using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Core.Common
{
    public interface IRedisManager
    {
        string GetValue(string key);

        TEntity Get<TEntity>(string key);

        void Set(string key, object value, TimeSpan cacheTime);

        bool Get(string key);

        void Remove(string key);

        void Clear();
    }
}
