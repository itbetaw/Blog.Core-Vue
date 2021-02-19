using System;

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
